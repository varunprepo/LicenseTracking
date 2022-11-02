using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; 

namespace LicenseTracking
{
    class Subroutines
    {
        static int pageIndex = 1;         

        private int numberOfRecPerPage=10;

        static List<object> licTrackDataList = null;
        static List<object> assgLicUsersList = null;
        static List<object> usersList = null;
        //To check the paging direction according to use selection.
        private enum PagingMode
        { First = 1, Next = 2, Previous = 3, Last = 4 };

        public void SetWindowPosition(Window window)
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = window.Width;
            double windowHeight = window.Height;
            window.Left = (screenWidth / 2) - (windowWidth / 2);
            window.Top = (screenHeight / 2) - (windowHeight / 2);
        }         

        public List<object> GetProductList(DataTable dt)
        {
            List<object> list = new List<object>();
            DAL dal = new DAL();
            int days = dal.GetDaysRemainingToExpire();

            string[] dtFormats = { "dd/MMM/yyyy", "yyyy/dd/MM", "MM/dd/yyyy", "MM/dd/yy", "M/d/yyyy" };            
            
            dt.Columns.Add("ImgPath");
            dt.AcceptChanges();             

            DateTime expiryDate = Convert.ToDateTime(DateTime.Now.ToString().Split(' ')[0]);
            foreach (DataRow row in dt.Rows)
            {

                DateTime.TryParseExact(row["License Expiration Date"].ToString().Split(' ')[0],
                                        dtFormats, CultureInfo.CurrentCulture,DateTimeStyles.None, out expiryDate);
                int daysLeft = Math.Abs(DateTime.Now.Subtract(expiryDate).Days);

                if (DateTime.Now.CompareTo(expiryDate) == -1)
                {
                    if (daysLeft <= days)
                    {
                        row["ImgPath"] = "Images\\NearToExpire.png";
                        //solidClrBrush = Brushes.LightYellow; C:\Program Files\Ashghal\LicenseTracking\

                    }
                    else
                    {
                        row["ImgPath"] = "Images\\Ok.png";
                        //solidClrBrush = Brushes.LightGreen; C:\Program Files\Ashghal\LicenseTracking\
                    }
                }
                else if (DateTime.Now.CompareTo(expiryDate) == 1 || DateTime.Now.CompareTo(expiryDate) == 0)
                {
                    //byte R = Convert(Color.Substring(1, 2), 16);
                    //byte G = Convert.ToByte(color.Substring(3, 2), 16);
                    //byte B = Convert.ToByte(color.Substring(5, 2), 16);

                    try
                    {
                        row["ImgPath"] = "Images\\Expired.png"; //C:\Program Files\Ashghal\LicenseTracking\                        
                    }
                    catch (Exception ex)
                    {
                        string str = ex.Message;
                    }

                }             
                 
            }
            var result = (from row in dt.AsEnumerable()
                         select new
                         {
                             ImgPath = row["ImgPath"],
                             ProductName = row["Product Name"],
                             LicenseActivationDate = row["License Activation Date"],
                             LicenseExpirationDate = row["License Expiration Date"],
                             LicenseTypeName = (row["License Type Name"]),
                             NoOfLicenses = row["No Of Licenses"],
                             ProviderName = row["Provider Name"],
                             hpLnkToViewUsers = row["hpLnkToViewUsers"],
                             hpLnkToEditLicDetails = row["hpLnkToEditLicDetails"],                              
                             productID = Convert.ToInt32(row["productID"])                              
                         }).ToList();

            licTrackDataList = result.ConvertAll<object>(o => (object)o);
            return licTrackDataList;
        }


        public List<object> GetUsersList(DataTable dt)
        {
            var result = (from row in dt.AsEnumerable()
                          select new
                          {
                              chkBoxIsAct = row["isUserActive"],
                              isAdminActive = row["isAdminActive"],
                              hyperLinkToChangePwd = row["hyperLinkToChangePwd"],
                              userName = row["userName"],
                              createDate = row["createDate"],
                              updateDate = row["updateDate"],
                              userID = row["userID"]
                          }).ToList();

            usersList = result.ConvertAll<object>(o => (object)o);
            return usersList;
        }

        public List<object> GetAsgUsersList(DataTable dt)
        {
            var result = (from row in dt.AsEnumerable()
                          select new
                          {
                              firstName = row["firstName"],
                              lastName = row["lastName"],
                              productName = row["productName"],
                              assignedUserID = row["assignedUserID"]        
                          }).ToList();

            assgLicUsersList = result.ConvertAll<object>(o => (object)o);
            return assgLicUsersList;
        }

        public string Encrypt(string toEncrypt, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);           
             
            //If hashing use get hashcode regards to your key
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(toEncrypt));
                //Always release the resources and flush data
                // of the Cryptographic service provide. Best Practice

                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes.
            //We choose ECB(Electronic code Book)
            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)

            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            //transform the specified region of bytes array to resultArray
            byte[] resultArray =
              cTransform.TransformFinalBlock(toEncryptArray, 0,
              toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor
            tdes.Clear();
            //Return the encrypted data into unreadable string format
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public string Decrypt(string cipherString, bool useHashing)
        {
            byte[] keyArray;
            //get the byte code of the string

            byte[] toEncryptArray = Convert.FromBase64String(cipherString);            

            if (useHashing)
            {
                //if hashing was used get the hash code with regards to your key
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(cipherString));
                //release any resource held by the MD5CryptoServiceProvider

                hashmd5.Clear();
            }
            else
            {
                //if hashing was not implemented get the byte code of the key
                keyArray = UTF8Encoding.UTF8.GetBytes(cipherString);
            }

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes. 
            //We choose ECB(Electronic code Book)

            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(
                                 toEncryptArray, 0, toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor                
            tdes.Clear();
            //return the Clear decrypted TEXT
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        public string ComputeHash(string plainText, byte[] saltBytes)
        {
            // If salt is not specified, generate it.
            if (saltBytes == null)
            {
                // Define min and max salt sizes.
                int minSaltSize = 4;
                int maxSaltSize = 8;

                // Generate a random number for the size of the salt.
                Random random = new Random();
                int saltSize = random.Next(minSaltSize, maxSaltSize);

                // Allocate a byte array, which will hold the salt.
                saltBytes = new byte[saltSize];

                // Initialize a random number generator.
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

                // Fill the salt with cryptographically strong byte values.
                rng.GetNonZeroBytes(saltBytes);
            }

            // Convert plain text into a byte array.
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            // Allocate array, which will hold plain text and salt.
            byte[] plainTextWithSaltBytes = new byte[plainTextBytes.Length + saltBytes.Length];

            // Copy plain text bytes into resulting array.
            for (int i = 0; i < plainTextBytes.Length; i++)
                plainTextWithSaltBytes[i] = plainTextBytes[i];

            // Append salt bytes to the resulting array.
            for (int i = 0; i < saltBytes.Length; i++)
                plainTextWithSaltBytes[plainTextBytes.Length + i] = saltBytes[i];

            HashAlgorithm hash = new MD5CryptoServiceProvider();

            // Compute hash value of our plain text with appended salt.
            byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);

            // Create array which will hold hash and original salt bytes.
            byte[] hashWithSaltBytes = new byte[hashBytes.Length + saltBytes.Length];

            // Copy hash bytes into resulting array.
            for (int i = 0; i < hashBytes.Length; i++)
                hashWithSaltBytes[i] = hashBytes[i];

            // Append salt bytes to the result.
            for (int i = 0; i < saltBytes.Length; i++)
                hashWithSaltBytes[hashBytes.Length + i] = saltBytes[i];

            // Convert result into a base64-encoded string.
            string hashValue = Convert.ToBase64String(hashWithSaltBytes);

            // Return the result
            return hashValue;
        }

        //VerifyingHash method
        public bool VerifyHash(string plainText, string hashValue)
        {
            // Convert base64-encoded hash value into a byte array.
            byte[] hashWithSaltBytes = Convert.FromBase64String(hashValue);

            // We must know size of hash (without salt).
            int hashSizeInBits, hashSizeInBytes;

            // Size of hash is based on the specified algorithm.               
            hashSizeInBits = 128;

            // Convert size of hash from bits to bytes.
            hashSizeInBytes = hashSizeInBits / 8;

            // Make sure that the specified hash value is long enough.
            if (hashWithSaltBytes.Length < hashSizeInBytes)
                return false;

            // Allocate array to hold original salt bytes retrieved from hash.
            byte[] saltBytes = new byte[hashWithSaltBytes.Length - hashSizeInBytes];

            // Copy salt from the end of the hash to the new array.
            for (int i = 0; i < saltBytes.Length; i++)
                saltBytes[i] = hashWithSaltBytes[hashSizeInBytes + i];

            // Compute a new hash string.
            string expectedHashString = ComputeHash(plainText, saltBytes);

            // If the computed hash matches the specified hash,
            // the plain text value must be correct.
            return (hashValue == expectedHashString);
        }

        public bool CheckChkBoxes(DataGrid dataGrid)
        {
            bool isChecked = false;
            List<CheckBox> checkBoxlist = new List<CheckBox>();

            // Find all elements
            Subroutines subRoutines = new Subroutines();
            subRoutines.FindChildGroup<CheckBox>(dataGrid, "chkBox", ref checkBoxlist);
        
            foreach (CheckBox c in checkBoxlist)
            {
                if ((bool)c.IsChecked)
                {
                    isChecked = true;                   
                }                
            }
            return isChecked;
        }
        

        public void HideDataGridCols(DataGrid dgLicenses)
        {
            dgLicenses.Columns[6].Visibility = Visibility.Hidden; //Hidding hyperlinktext column
            dgLicenses.Columns[7].Visibility = Visibility.Hidden; //Hidding productID column
        }

        static List<object> dataList = null;
        public void Navigate(int mode, Button btnPrev, Button btnFirst, Button btnNext, Button btnLast, DataGrid dataGrid, int typeOfDataList, Label pageInfo)
        {            
            int count;
            if (typeOfDataList == 1)
            {
                dataList = licTrackDataList;
            }
            else if (typeOfDataList == 2)
            {
                dataList = assgLicUsersList;
            }
            else if (typeOfDataList == 3)
            {
                dataList = usersList;
            }
           
            switch (mode)
            {
                case (int)PagingMode.Next:
                    btnPrev.IsEnabled = true;
                    btnFirst.IsEnabled = true;
                    if (dataList.Count >= (pageIndex * numberOfRecPerPage))
                    {
                        if (dataList.Skip(pageIndex *
                        numberOfRecPerPage).Take(numberOfRecPerPage).Count() == 0)
                        {
                            dataGrid.ItemsSource = null;
                            dataGrid.ItemsSource = dataList.Skip((pageIndex *
                            numberOfRecPerPage) - numberOfRecPerPage).Take(numberOfRecPerPage);                             
                            count = (pageIndex * numberOfRecPerPage) +
                            (dataList.Skip(pageIndex *
                            numberOfRecPerPage).Take(numberOfRecPerPage)).Count();
                        }
                        else
                        {
                            dataGrid.ItemsSource = null;
                            dataGrid.ItemsSource = dataList.Skip(pageIndex *
                            numberOfRecPerPage).Take(numberOfRecPerPage);                             
                            count = (pageIndex * numberOfRecPerPage) +
                            (dataList.Skip(pageIndex *
                            numberOfRecPerPage).Take(numberOfRecPerPage)).Count();
                            pageIndex++;
                        }                         
                        pageInfo.Content = count + " of " + dataList.Count;
                    }

                    else
                    {
                        btnNext.IsEnabled = false;
                        btnLast.IsEnabled = false;
                    }

                    break;
                case (int)PagingMode.Previous:
                    btnNext.IsEnabled = true;
                    btnLast.IsEnabled = true;
                    if (pageIndex > 1)
                    {
                        pageIndex -= 1;
                        dataGrid.ItemsSource = null;
                        count = Math.Min(pageIndex * numberOfRecPerPage, dataList.Count);
                        pageInfo.Content = count + " of " + dataList.Count;
                        if (pageIndex == 1)
                        {
                            dataGrid.ItemsSource = dataList.Take(numberOfRecPerPage);                             
                        }
                        else
                        {
                            dataGrid.ItemsSource = dataList.Skip(numberOfRecPerPage).Take(numberOfRecPerPage);                       
                        }                                                
                    }
                    else
                    {
                        btnPrev.IsEnabled = false;
                        btnFirst.IsEnabled = false;
                    }
                    break;

                case (int)PagingMode.First:
                    pageIndex = 2;
                    Navigate((int)PagingMode.Previous, btnPrev, btnFirst, btnNext, btnLast, dataGrid, typeOfDataList, pageInfo);
                    break;
                case (int)PagingMode.Last:
                    pageIndex = (dataList.Count / numberOfRecPerPage);
                    Navigate((int)PagingMode.Next, btnPrev, btnFirst, btnNext, btnLast, dataGrid, typeOfDataList, pageInfo);
                    break;                
            }
        }

        public T IsWindowOpen<T>(string name = "") where T : Window
        {
            //return string.IsNullOrEmpty(name)
            //   ? Application.Current.Windows.OfType<T>().Any()
            //   : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));

            var windows = Application.Current.Windows.OfType<T>();
            return string.IsNullOrEmpty(name) ? windows.FirstOrDefault() : windows.FirstOrDefault(w => w.Name.Equals(name));
        }


        public void FindChildGroup<T>(DependencyObject parent, string childName, ref List<T> list) where T : DependencyObject
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childrenCount; i++)
            {
                // Get the child
                var child = VisualTreeHelper.GetChild(parent, i);

                // Compare on conformity the type
                T child_Test = child as T;

                // Not compare - go next
                if (child_Test == null)
                {
                    // Go the deep
                    FindChildGroup<T>(child, childName, ref list);
                }
                else
                {
                    // If match, then check the name of the item
                    FrameworkElement child_Element = child_Test as FrameworkElement;

                    if (child_Element.Name == childName)
                    {
                        // Found
                        list.Add(child_Test);
                    }

                    // We are looking for further, perhaps there are
                    // children with the same name
                    FindChildGroup<T>(child, childName, ref list);
                }
            }

            return;
        }
        //private void Navigate(int mode, Button btnPrev, Button btnFirst, Button btnNext, Button btnLast, DataGrid dataGrid, List<object> dataList, Label pageInfo)
        //{
        //    int count;
        //    switch (mode)
        //    {
        //        case (int)PagingMode.Next:
        //            btnPrev.IsEnabled = true;
        //            btnFirst.IsEnabled = true;
        //            if (dataList.Count >= (pageIndex * numberOfRecPerPage))
        //            {
        //                if (dataList.Skip(pageIndex *
        //                numberOfRecPerPage).Take(numberOfRecPerPage).Count() == 0)
        //                {
        //                    dataGrid.ItemsSource = null;
        //                    dataGrid.ItemsSource = dataList.Skip((pageIndex *
        //                    numberOfRecPerPage) - numberOfRecPerPage).Take(numberOfRecPerPage);
        //                    count = (pageIndex * numberOfRecPerPage) +
        //                    (dataList.Skip(pageIndex *
        //                    numberOfRecPerPage).Take(numberOfRecPerPage)).Count();
        //                }
        //                else
        //                {
        //                    dataGrid.ItemsSource = null;
        //                    dataGrid.ItemsSource = dataList.Skip(pageIndex *
        //                    numberOfRecPerPage).Take(numberOfRecPerPage);
        //                    count = (pageIndex * numberOfRecPerPage) +
        //                    (dataList.Skip(pageIndex *
        //                    numberOfRecPerPage).Take(numberOfRecPerPage)).Count();
        //                    pageIndex++;
        //                }

        //                pageInfo.Content = count + " of " + dataList.Count;
        //            }

        //            else
        //            {
        //                btnNext.IsEnabled = false;
        //                btnLast.IsEnabled = false;
        //            }

        //            break;
        //        case (int)PagingMode.Previous:
        //            btnNext.IsEnabled = true;
        //            btnLast.IsEnabled = true;
        //            if (pageIndex > 1)
        //            {
        //                pageIndex -= 1;
        //                dataGrid.ItemsSource = null;
        //                if (pageIndex == 1)
        //                {
        //                    dataGrid.ItemsSource = dataList.Take(numberOfRecPerPage);
        //                    count = dataList.Take(numberOfRecPerPage).Count();
        //                    pageInfo.Content = count + " of " + dataList.Count;
        //                }
        //                else
        //                {
        //                    dataGrid.ItemsSource = dataList.Skip
        //                    (pageIndex * numberOfRecPerPage).Take(numberOfRecPerPage);
        //                    count = Math.Min(pageIndex * numberOfRecPerPage, dataList.Count);
        //                    pageInfo.Content = count + " of " + dataList.Count;
        //                }
        //            }
        //            else
        //            {
        //                btnPrev.IsEnabled = false;
        //                btnFirst.IsEnabled = false;
        //            }
        //            break;

        //        case (int)PagingMode.First:
        //            pageIndex = 2;
        //            Navigate((int)PagingMode.Previous);
        //            break;
        //        case (int)PagingMode.Last:
        //            pageIndex = (dataList.Count / numberOfRecPerPage);
        //            Navigate((int)PagingMode.Next);
        //            break;

        //        //case (int)PagingMode.PageCountChange:
        //        //    pageIndex = 1;
        //        //    numberOfRecPerPage = Convert.ToInt32(cbNumberOfRecords.SelectedItem);
        //        //    dataGrid.ItemsSource = null;
        //        //    dataGrid.ItemsSource = dataList.Take(numberOfRecPerPage);
        //        //    count = (dataList.Take(numberOfRecPerPage)).Count();
        //        //    pageInfo.Content = count + " of " + dataList.Count;
        //        //    btnNext.IsEnabled = true;
        //        //    btnLast.IsEnabled = true;
        //        //    btnPrev.IsEnabled = true;
        //        //    btnFirst.IsEnabled = true;
        //        //    break;
        //    }
        //}
    }
}
