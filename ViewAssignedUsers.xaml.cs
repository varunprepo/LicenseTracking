using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LicenseTracking
{
    /// <summary>
    /// Interaction logic for ViewAssignedUsers.xaml
    /// </summary>
    public partial class ViewAssignedUsers : Window
    {
        
        //To check the paging direction according to use selection.
        private enum PagingMode
        { First = 1, Next = 2, Previous = 3, Last = 4 };

        private Int16 _prodID = 0;
        private string _userName = null;
        DataGrid _dgLicenses = null;
        Label _lblPageInfo = null;
        public ViewAssignedUsers(DataGrid dgLicenses, Label lblPageInfo, Int16 prodID,string userName,bool isAdmin)
        {
            InitializeComponent();
            //this.Closed += new EventHandler(Window_Closed);
            _dgLicenses = dgLicenses;
            _lblPageInfo = lblPageInfo;
            Subroutines subRoutines = new Subroutines();
            subRoutines.SetWindowPosition(this);
            _prodID = prodID;
            _userName = userName;
            if (!isAdmin)
            {
                btnDeleteUser.Visibility = Visibility.Hidden;                 
            }
            FillAssignedUsersInfo(prodID);
        }

        private List<object> dataList = null;
        private void FillAssignedUsersInfo(Int16 prodID)
        {

            DAL dal = new DAL();
            dataList = dal.FillDataGrid(dgViewAsgUsers, "SELECT alu.firstName,alu.lastName,prods.productName,alu.assignedUserID from AssignedLicensedUsers alu inner join Products prods on " +
            "alu.productID = prods.productID where prods.productID = " + prodID, false, true, lblPageInfo);
            if (dgViewAsgUsers.Items.Count == 0)
            {
                btnFirst.IsEnabled = false;
                btnNext.IsEnabled = false;
                btnPrev.IsEnabled = false;
                btnLast.IsEnabled = false;
            }
        }

        //public static T FindChild<T>(DependencyObject parent, string childName)
        //   where T : DependencyObject
        //{
        //    // Confirm parent is valid.  
        //    if (parent == null) return null;

        //    T foundChild = null;

        //    int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        //    for (int i = 0; i < childrenCount; i++)
        //    {
        //        var child = VisualTreeHelper.GetChild(parent, i);
        //        // If the child is not of the request child type child 
        //        T childType = child as T;
        //        if (childType == null)
        //        {
        //            // recursively drill down the tree 
        //            foundChild = FindChild<T>(child, childName);

        //            // If the child is found, break so we do not overwrite the found child.  
        //            if (foundChild != null) break;
        //        }
        //        else if (!string.IsNullOrEmpty(childName))
        //        {
        //            var frameworkElement = child as FrameworkElement;
        //            // If the child's name is set for search 
        //            if (frameworkElement != null && frameworkElement.Name == childName)
        //            {
        //                // if the child's name is of the request name 
        //                foundChild = (T)child;
        //                break;
        //            }
        //        }
        //        else
        //        {
        //            // child element found. 
        //            foundChild = (T)child;
        //            break;
        //        }
        //    }
        //    return foundChild;
        //}

        

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {   

            List<CheckBox> checkBoxlist = new List<CheckBox>();
            // Find all elements            
            Subroutines subRoutines = new Subroutines();
            subRoutines.FindChildGroup<CheckBox>(dgViewAsgUsers, "chkBox", ref checkBoxlist);            
            
            foreach (CheckBox c in checkBoxlist)
            {
                if (!(bool)c.IsChecked)
                {
                    c.IsChecked = true;                     
                }
            }            
        }
        private void UnheckBox_Checked(object sender, RoutedEventArgs e)
        {
            List<CheckBox> checkBoxlist = new List<CheckBox>();

            // Find all elements
            Subroutines subRoutines = new Subroutines();
            subRoutines.FindChildGroup<CheckBox>(dgViewAsgUsers, "chkBox", ref checkBoxlist);

            foreach (CheckBox c in checkBoxlist)
            {
                if ((bool)c.IsChecked)
                {
                    c.IsChecked = false;
                    //do whatever you want
                }
            }

            //IEnumerable list = dgViewAsgUsers.ItemsSource as IEnumerable;

            //foreach (var row in list)
            //{
            //    CheckBox chkBox = ((CheckBox)dgViewAsgUsers.Columns[0].GetCellContent(row));
            //    chkBox.IsChecked = false;

            //    //if (IsChecked)
            //    //{
            //    //    string id = ((TextBlock)grdLossCodelist.Columns[2].GetCellContent(row)).Text;
            //    //    lstFile.Add(id);

            //    //}
            //}
        }

        private void btnFirst_Click(object sender, System.EventArgs e)
        {
            Subroutines sub = new Subroutines();
            sub.Navigate((int)PagingMode.First, btnPrev, btnFirst, btnNext, btnLast, dgViewAsgUsers, 2, lblPageInfo);
        }

        private void btnNext_Click(object sender, System.EventArgs e)
        {
            Subroutines sub = new Subroutines();
            sub.Navigate((int)PagingMode.Next, btnPrev, btnFirst, btnNext, btnLast, dgViewAsgUsers, 2, lblPageInfo);
        }

        private void btnPrev_Click(object sender, System.EventArgs e)
        {
            Subroutines sub = new Subroutines();
            sub.Navigate((int)PagingMode.Previous, btnPrev, btnFirst, btnNext, btnLast, dgViewAsgUsers, 2, lblPageInfo);
        }

        private void btnLast_Click(object sender, System.EventArgs e)
        {
            Subroutines sub = new Subroutines();
            sub.Navigate((int)PagingMode.Last, btnPrev, btnFirst, btnNext, btnLast, dgViewAsgUsers, 2, lblPageInfo);
        }

        private void btnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            Subroutines subRoutines = new Subroutines();
            if(!subRoutines.CheckChkBoxes(dgViewAsgUsers))
            {
                MessageBox.Show("For deleting a record, please check at least one checkbox", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Are you sure to delete the selected users?", "Delete Assigned Users", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                List<CheckBox> checkBoxlist = new List<CheckBox>();

                // Find all elements
               
                subRoutines.FindChildGroup<CheckBox>(dgViewAsgUsers, "chkBox", ref checkBoxlist);
                int rowIndex = 0;
                StringBuilder strBuild = new StringBuilder();
                foreach (CheckBox c in checkBoxlist)
                {
                    if ((bool)c.IsChecked)
                    {

                        strBuild.Append(dgViewAsgUsers.Items[rowIndex].ToString().Substring(dgViewAsgUsers.Items[rowIndex].ToString().IndexOf("assignedUserID")).Split('=')[1].Replace("}", "").Trim());
                        strBuild.Append(",");                         
                    }

                    rowIndex++;
                }
                if(strBuild.Length>0)
                {
                    DAL dal = new DAL();
                    dal.ConnectToDB();
                    if(dal.ExecuteNonQuery("delete from AssignedLicensedUsers where assignedUserID in ("+strBuild.ToString().Substring(0,strBuild.ToString().LastIndexOf(","))+")")!=0)
                    {
                        MessageBox.Show("User has been successfully deleted from the product license list", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                                        
                }
                FillAssignedUsersInfo(_prodID);
            }           
                
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //int pgIdx = Convert.ToInt32(_lblPageInfo.ToString().Split(':')[1].Split(' ')[1]);
            //if ((pgIdx % 10) == 0)
            //{                 
            //    pgIdx = (pgIdx / 10);                 
            //}
            //else
            //{
            //    pgIdx = ((pgIdx % 10)+1);
            //}
            //DAL dal = new DAL();
            //dataList = dal.FillDataGrid(_dgLicenses, "SELECT Products.productName as [Product Name], Products.licenseActivationDate as [License Activation Date], Products.licenseExpirationDate as [License Expiration Date], " +
            //"LicenseTypes.licenseTypeName as [License Type Name], Products.noOfLicenses as [No Of Licenses], Providers.providerName as [Provider Name],Products.hpLnkToViewUsers,Products.hpLnkToEditLicDetails,Products.productID FROM Products INNER JOIN " +
            //"Providers ON Products.providerID = Providers.providerID INNER JOIN LicenseTypes ON Products.licenseTypeID = LicenseTypes.licenseTypeID", false, false, _lblPageInfo);
            //Subroutines sub = new Subroutines();
                      
            //if (pgIdx == 1)
            //{
            //    sub.PageIndex = 1;
            //    sub.Navigate(0, btnPrev, btnFirst, btnNext, btnLast, _dgLicenses, dataList, _lblPageInfo);
            //}
            //else
            //{
            //    sub.PageIndex = 1;
            //    sub.Navigate(pgIdx, btnPrev, btnFirst, btnNext, btnLast, _dgLicenses, dataList, _lblPageInfo);
            //}
        }
 
    }
}
