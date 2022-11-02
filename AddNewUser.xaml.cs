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
    /// Interaction logic for AddNewUser.xaml
    /// </summary>
    public partial class AddNewUser : Window
    {
        private bool _isEdit = false;
        private bool _isAdmin = false;
        private Int16 _userID = 0;
        public AddNewUser(bool isEdit, Int16 userID, string userName, bool isUserActive, bool isAdminActive, bool isAdmin)
        {
            InitializeComponent();
            Subroutines subRoutines = new Subroutines();
            subRoutines.SetWindowPosition(this);
            _isAdmin = isAdmin;
            if(isEdit)
            {
                lblHeader.Content = "Change Password";
                txtUserName.Text = userName;
                txtUserName.IsEnabled = false;
                chkIsAdminActive.IsChecked = isAdminActive;                     
                chkIsUserActive.IsChecked = isUserActive;               
            }
            _isEdit = isEdit;
            _userID = userID;
        }

        //private void chkIsAdminActive_Checked(object sender, RoutedEventArgs e)
        //{
        //    EditOrUpdateUserInfo();
        //}

        //private void chkIsUserActive_Checked(object sender, RoutedEventArgs e)
        //{
        //    EditOrUpdateUserInfo();
        //}

        private void btnAddNewUser_Click(object sender, RoutedEventArgs e)
        {
            if (txtUserName.Text.Trim().Equals("") && (!_isEdit))
            {
                MessageBox.Show("User Name cannot be left blank", "Validation Message", MessageBoxButton.OK, MessageBoxImage.Information);
                txtUserName.Focus();
                return;
            }
            else if (txtPassword.Password.Trim() == "")
            {
                MessageBox.Show("Password cannot be left blank", "Validation Message", MessageBoxButton.OK, MessageBoxImage.Information);
                txtPassword.Focus();
                return;
            }
            else if (txtReEnterPassword.Password.Trim().Equals(""))
            {
                MessageBox.Show("Re-Enter Password cannot be left blank", "Validation Message", MessageBoxButton.OK, MessageBoxImage.Information);
                txtReEnterPassword.Focus();
                return;
            }
            else if (!txtPassword.Password.Trim().Equals(txtReEnterPassword.Password.Trim()))
            {
                MessageBox.Show("Password and Re-Enter Password must match", "Validation Message", MessageBoxButton.OK, MessageBoxImage.Information);
                txtReEnterPassword.Focus();
                return;
            }

            EditOrUpdateUserInfo();
           
        }

        private void EditOrUpdateUserInfo()
        {
            DAL dal = new DAL();
            string[] userInfo = new string[5];
            userInfo[0] = txtUserName.Text.Trim();
            userInfo[1] = txtPassword.Password.Trim();
            userInfo[2] = _userID.ToString();
            if ((bool)chkIsUserActive.IsChecked)
            {
                userInfo[3] = "1";
            }
            else
            {
                userInfo[3] = "0";
            }

            if ((bool)chkIsAdminActive.IsChecked)
            {
                userInfo[4] = "1";
            }
            else
            {
                userInfo[4] = "0";
            }
            dal.AddOrUpdateUser(userInfo, _isEdit);
            this.Close();
            ViewUsers vwUsr = new ViewUsers(_isAdmin);
            vwUsr.Show();
        }

         
    }
}
