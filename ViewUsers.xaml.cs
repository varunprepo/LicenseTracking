using System;
using System.Collections.Generic;
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
    /// Interaction logic for ViewUsers.xaml
    /// </summary>
    public partial class ViewUsers : Window
    {
        //To check the paging direction according to use selection.
        private enum PagingMode
        { First = 1, Next = 2, Previous = 3, Last = 4 };

        bool isFirstCall = false;
        bool _isAdmin = false;
        public ViewUsers(bool isAdmin)
        {
            InitializeComponent();
            Subroutines subRoutines = new Subroutines();
            subRoutines.SetWindowPosition(this);
            if (!isAdmin)
            {
                btnDeleteUser.Visibility = Visibility.Hidden;
            }             
            _isAdmin = isAdmin;            
            FillUsersInfo();
            //isFirstCall = true;
        }

        private void FillUsersInfo()
        {

            DAL dal = new DAL();
            dataList = dal.FillDataGrid(dgViewUsers, "SELECT isUserActive,isAdminActive,hyperLinkToChangePwd,userName,replace(convert(nvarchar,createDate,106),' ','/') as createDate, " +
            "replace(convert(nvarchar,updateDate,106),' ','/') as updateDate,userID from Users", false, false, lblPageInfo);
            if (dgViewUsers.Items.Count == 0)
            {
                btnFirst.IsEnabled = false;
                btnNext.IsEnabled = false;
                btnPrev.IsEnabled = false;
                btnLast.IsEnabled = false;
            }
        }

        private void HpLnkChangePwd_Click(object sender, RoutedEventArgs e)
        {
            
             
            //DataGrid dgUsers = (DataGrid)sender;
             //var item = dgViewUsers.Items[i];
            //    DataGridRow row = (DataGridRow)dgViewUsers.ItemContainerGenerator.ContainerFromIndex(i);
            //    var mycheckbox = dgViewUsers.Columns[0].GetCellContent(row.Item) as CheckBox;
            //dgUsers
            //dgViewAsgUsers.Items[rowIndex].ToString().Substring(dgViewAsgUsers.Items[rowIndex].ToString().IndexOf("assignedUserID")).Split('=')[1].Replace("}", "").Trim()
            if (_isAdmin)
            {
                var rowData = ((Hyperlink)e.OriginalSource).DataContext;
                string userActive = rowData.ToString().Substring(rowData.ToString().IndexOf("chkBoxIsAct")).Split(',')[0].Split('=')[1].Trim();
                string adminActive = rowData.ToString().Substring(rowData.ToString().IndexOf("isAdminActive")).Split(',')[0].Split('=')[1].Trim();
                bool isUserActive = false;
                bool isAdminActive = false;

                if(userActive!="")
                {
                    isUserActive = Convert.ToBoolean(userActive);
                }

                if(adminActive!="")
                {
                    isAdminActive = Convert.ToBoolean(adminActive);
                }
                AddNewUser addNewUsr = new AddNewUser(true, Convert.ToInt16(rowData.ToString().Substring(rowData.ToString().IndexOf("userID")).Split('=')[1].Replace("}", "").Trim())
                    , rowData.ToString().Substring(rowData.ToString().IndexOf("userName")).Split(',')[0].Split('=')[1].Trim(),isUserActive,isAdminActive,_isAdmin);
                this.Close();
                addNewUsr.Title = "Change Password";
                addNewUsr.Show();
            }
            else
            {
                MessageBox.Show("Sorry, You are not Admin user. Cannot change password.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }

        private void btnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            Subroutines subRoutines = new Subroutines();
            if (!subRoutines.CheckChkBoxes(dgViewUsers))
            {
                MessageBox.Show("For deleting a record, please check at least one checkbox", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            MessageBoxResult result = MessageBox.Show("Are you sure to delete the selected users?", "Delete Users", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                string userIds = CheckedRecords(subRoutines, "chkBox");
                if (userIds != "")
                {
                    DAL dal = new DAL();                     
                    if (dal.ExecuteNonQuery("delete from Users where userID in (" + userIds.Substring(0, userIds.LastIndexOf(",")) + ")") != 0)
                    {
                        MessageBox.Show("User has been successfully deleted", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                FillUsersInfo();
                //isFirstCall = true;
            }           
        }     

        private string CheckedRecords(Subroutines subRoutines,string chkName)
        {
            List<CheckBox> checkBoxlist = new List<CheckBox>();

            // Find all elements                
            subRoutines.FindChildGroup<CheckBox>(dgViewUsers, chkName, ref checkBoxlist);
            int rowIndex = 0;
            StringBuilder strBuild = new StringBuilder();
            foreach (CheckBox c in checkBoxlist)
            {
                if ((bool)c.IsChecked)
                {
                    strBuild.Append(dgViewUsers.Items[rowIndex].ToString().Substring(dgViewUsers.Items[rowIndex].ToString().IndexOf("userID")).Split('=')[1].Replace("}", "").Trim());
                    strBuild.Append(",");
                }

                rowIndex++;
            }
            return strBuild.ToString();
        }

        
        private void ChkBoxDeleteHeader_Checked(object sender, RoutedEventArgs e)
        {
            if (_isAdmin)
            {
                if (!isFirstCall)
                {
                    List<CheckBox> checkBoxlist = new List<CheckBox>();
                    // Find all elements            
                    Subroutines subRoutines = new Subroutines();
                    subRoutines.FindChildGroup<CheckBox>(dgViewUsers, "chkBox", ref checkBoxlist);
                    //StringBuilder strBuild = new StringBuilder();

                    foreach (CheckBox c in checkBoxlist)
                    {
                        if (!(bool)c.IsChecked)
                        {
                            c.IsChecked = true;
                            //strBuild.Append(dgViewUsers.Items[rowIndex].ToString().Substring(dgViewUsers.Items[rowIndex].ToString().IndexOf("userID")).Split('=')[1].Replace("}", "").Trim());
                            //strBuild.Append(","); 
                        }
                    }
                    //isTriggeredFromChkBoxHeader = false;
                    //if (strBuild.Length > 0)
                    //{
                    //    DAL dal = new DAL();
                    //    dal.ExecuteNonQuery("update Users set isActive=1,updateDate=getDate() where userID in(" + strBuild.ToString().Substring(0, strBuild.ToString().LastIndexOf(",")) + ")");                                 
                    //} 
                }
            }
            else
            {
                MessageBox.Show("Sorry, You are not Admin user. Cannot check or uncheck checkboxes.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }

        bool isTriggeredFromUnChkIsActHeaderBox = false;
        private void UnchkBoxIsActHeader_Checked(object sender, RoutedEventArgs e)
        {
            if (_isAdmin)
            {
                List<CheckBox> checkBoxlist = new List<CheckBox>();

                // Find all elements
                Subroutines subRoutines = new Subroutines();
                subRoutines.FindChildGroup<CheckBox>(dgViewUsers, "chkBoxIsAct", ref checkBoxlist);
                StringBuilder strBuild = new StringBuilder();
                int rowIndex = 0;
                isTriggeredFromUnChkIsActHeaderBox = true;
                foreach (CheckBox c in checkBoxlist)
                {
                    if ((bool)c.IsChecked)
                    {
                        c.IsChecked = false;
                        strBuild.Append(dgViewUsers.Items[rowIndex].ToString().Substring(dgViewUsers.Items[rowIndex].ToString().IndexOf("userID")).Split('=')[1].Replace("}", "").Trim());
                        strBuild.Append(",");
                    }
                    rowIndex++;
                }
                isTriggeredFromUnChkIsActHeaderBox = false;
                if (strBuild.Length > 0)
                {
                    DAL dal = new DAL();
                    dal.ExecuteNonQuery("update Users set isUserActive=0,updateDate=getDate() where userID in(" + strBuild.ToString().Substring(0, strBuild.ToString().LastIndexOf(",")) + ")");
                }
            }
            else
            {
                MessageBox.Show("Sorry, You are not Admin user. Cannot check or uncheck checkboxes.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        bool isTriggeredFromChkIsActHeaderBox = false;
        private void ChkBoxIsActHeader_Checked(object sender, RoutedEventArgs e)
        {
            if (_isAdmin)
            {
                List<CheckBox> checkBoxlist = new List<CheckBox>();
                // Find all elements            
                Subroutines subRoutines = new Subroutines();
                subRoutines.FindChildGroup<CheckBox>(dgViewUsers, "chkBoxIsAct", ref checkBoxlist);
                StringBuilder strBuild = new StringBuilder();
                int rowIndex = 0;
                isTriggeredFromChkIsActHeaderBox = true;
                foreach (CheckBox c in checkBoxlist)
                {
                    if (!(bool)c.IsChecked)
                    {
                        c.IsChecked = true;
                        strBuild.Append(dgViewUsers.Items[rowIndex].ToString().Substring(dgViewUsers.Items[rowIndex].ToString().IndexOf("userID")).Split('=')[1].Replace("}", "").Trim());
                        strBuild.Append(",");
                    }
                    rowIndex++;
                }
                isTriggeredFromChkIsActHeaderBox = false;
                if(strBuild.Length >0)
                {
                    DAL dal = new DAL();
                    dal.ExecuteNonQuery("update Users set isUserActive=1,updateDate=getDate() where userID in(" + strBuild.ToString().Substring(0, strBuild.ToString().LastIndexOf(",")) + ")");
                    //if (dal.ExecuteNonQuery("update Users set isActive=1 where userID in("+ strBuild.ToString().Substring(0, strBuild.ToString().LastIndexOf(",")) + ")") != 0)
                    //{
                    //    MessageBox.Show("User has been successfully deleted", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    //}               
                }
            }
            else
            {
                MessageBox.Show("Sorry, You are not Admin user. Cannot check or uncheck checkboxes.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            //FillUsersInfo();
            //isFirstCall = true;
        }

        //bool isTriggeredFromUnChkIsActHeaderBox = false;
        private void UnchkBoxDeleteHeader_Checked(object sender, RoutedEventArgs e)
        {
            if (_isAdmin)
            {
                List<CheckBox> checkBoxlist = new List<CheckBox>();
                //isTriggeredFromUnChkHeaderBox = true;
                // Find all elements
                Subroutines subRoutines = new Subroutines();
                subRoutines.FindChildGroup<CheckBox>(dgViewUsers, "chkBox", ref checkBoxlist);
                //StringBuilder strBuild = new StringBuilder();
                //int rowIndex = 0;
                foreach (CheckBox c in checkBoxlist)
                {
                    if ((bool)c.IsChecked)
                    {
                        c.IsChecked = false;
                        //strBuild.Append(dgViewUsers.Items[rowIndex].ToString().Substring(dgViewUsers.Items[rowIndex].ToString().IndexOf("userID")).Split('=')[1].Replace("}", "").Trim());
                        //strBuild.Append(",");                   
                    }                 
                }
            }
            else
            {
                MessageBox.Show("Sorry, You are not Admin user. Cannot check or uncheck checkboxes.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            //isFirstCall = false;             
        }

        private void ChkBoxIsAct_Checked(object sender, RoutedEventArgs e)
        {
            if (_isAdmin)
            {
                if (!isFirstCall)
                {
                    if (!isTriggeredFromChkIsActHeaderBox)
                    {
                        List<CheckBox> checkBoxlist = new List<CheckBox>();

                        // Find all elements
                        Subroutines subRoutines = new Subroutines();
                        subRoutines.FindChildGroup<CheckBox>(dgViewUsers, "chkBoxIsAct", ref checkBoxlist);
                        StringBuilder strBuild = new StringBuilder();
                        int rowIndex = 0;
                        isTriggeredFromChkIsActHeaderBox = true;
                        foreach (CheckBox c in checkBoxlist)
                        {
                            if ((bool)c.IsChecked)
                            {
                                c.IsChecked = true;
                                strBuild.Append(dgViewUsers.Items[rowIndex].ToString().Substring(dgViewUsers.Items[rowIndex].ToString().IndexOf("userID")).Split('=')[1].Replace("}", "").Trim());
                                strBuild.Append(",");
                            }
                            rowIndex++;
                        }
                        isTriggeredFromChkIsActHeaderBox = false;
                        isTriggeredFromUnChkIsActHeaderBox = false;
                        if (strBuild.Length > 0)
                        {
                            DAL dal = new DAL();
                            dal.ExecuteNonQuery("update Users set isUserActive=1,updateDate=getDate() where userID in(" + strBuild.ToString().Substring(0, strBuild.ToString().LastIndexOf(",")) + ")");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Sorry, You are not Admin user. Cannot check or uncheck checkboxes.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }            
            
                //IEnumerable list = dgViewUsers.ItemsSource as IEnumerable;

                //foreach (var row in list)
                //{
                //    CheckBox chkBox = ((CheckBox)dgViewUsers.Columns[0].GetCellContent(row));
                //    chkBox.IsChecked = false;

                //    //if (IsChecked)
                //    //{
                //    //    string id = ((TextBlock)grdLossCodelist.Columns[2].GetCellContent(row)).Text;
                //    //    lstFile.Add(id);

                //    //}
                //}
            
        }

        private void UnChkBoxIsAct_Checked(object sender, RoutedEventArgs e)
        {
            if (_isAdmin)
            {
                isFirstCall = false;
                if (!isTriggeredFromUnChkIsActHeaderBox)
                {
                    List<CheckBox> checkBoxlist = new List<CheckBox>();

                    // Find all elements
                    Subroutines subRoutines = new Subroutines();
                    subRoutines.FindChildGroup<CheckBox>(dgViewUsers, "chkBoxIsAct", ref checkBoxlist);
                    StringBuilder strBuild = new StringBuilder();
                    int rowIndex = 0;
                    isTriggeredFromUnChkIsActHeaderBox = true;
                    foreach (CheckBox c in checkBoxlist)
                    {
                        if (!(bool)c.IsChecked)
                        {
                            //c.IsChecked = false;
                            strBuild.Append(dgViewUsers.Items[rowIndex].ToString().Substring(dgViewUsers.Items[rowIndex].ToString().IndexOf("userID")).Split('=')[1].Replace("}", "").Trim());
                            strBuild.Append(",");
                        }
                        rowIndex++;
                    }
                    isTriggeredFromUnChkIsActHeaderBox = false;
                    if (strBuild.Length > 0)
                    {
                        DAL dal = new DAL();
                        dal.ExecuteNonQuery("update Users set isUserActive=0 where userID in(" + strBuild.ToString().Substring(0, strBuild.ToString().LastIndexOf(",")) + ")");
                    }
                }
            }
            else
            {
                MessageBox.Show("Sorry, You are not Admin user. Cannot check or uncheck checkboxes.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
             
            //isFirstCall = true;
            //IEnumerable list = dgViewUsers.ItemsSource as IEnumerable;

            //foreach (var row in list)
            //{
            //    CheckBox chkBox = ((CheckBox)dgViewUsers.Columns[0].GetCellContent(row));
            //    chkBox.IsChecked = false;

            //    //if (IsChecked)
            //    //{
            //    //    string id = ((TextBlock)grdLossCodelist.Columns[2].GetCellContent(row)).Text;
            //    //    lstFile.Add(id);

            //    //}
            //}
        }

        private List<object> dataList = null;
        private void btnFirst_Click(object sender, System.EventArgs e)
        {
            isFirstCall = true;
            Subroutines sub = new Subroutines();
            sub.Navigate((int)PagingMode.First, btnPrev, btnFirst, btnNext, btnLast, dgViewUsers, 3, lblPageInfo);
        }

        private void btnNext_Click(object sender, System.EventArgs e)
        {
            isFirstCall = true;
            Subroutines sub = new Subroutines();
            sub.Navigate((int)PagingMode.Next, btnPrev, btnFirst, btnNext, btnLast, dgViewUsers, 3, lblPageInfo);
        }

        private void btnPrev_Click(object sender, System.EventArgs e)
        {
            isFirstCall = true;
            Subroutines sub = new Subroutines();
            sub.Navigate((int)PagingMode.Previous, btnPrev, btnFirst, btnNext, btnLast, dgViewUsers, 3, lblPageInfo);
        }

        private void btnLast_Click(object sender, System.EventArgs e)
        {
            isFirstCall = true;
            Subroutines sub = new Subroutines();
            sub.Navigate((int)PagingMode.Last, btnPrev, btnFirst, btnNext, btnLast, dgViewUsers, 3, lblPageInfo);
        }

       
    }
}
