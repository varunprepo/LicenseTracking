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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LicenseTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            Subroutines subRoutines = new Subroutines();
            subRoutines.SetWindowPosition(this);
            txtUserName.Focus();
        }

        private void LogIn()
        {
            if (txtUserName.Text.Trim() == "")
            {
                MessageBox.Show("User Name cannot be left blank", "Validation Info", MessageBoxButton.OK, MessageBoxImage.Information);
                txtUserName.Focus();
                return;
            }
            else if (txtPwd.Password.Trim() == "")
            {
                MessageBox.Show("Password cannot be left blank", "Validation Info", MessageBoxButton.OK, MessageBoxImage.Information);
                txtPwd.Focus();
                return;
            }
            DAL dal = new DAL();
            string[] userInfo = new string[2];
            userInfo[0] = txtUserName.Text.Trim();
            userInfo[1] = txtPwd.Password.Trim();
            if (dal.ValidateUser(userInfo))
            {
                this.Close();
            }
        }
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            LogIn();
        }

        private void btnLogin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                LogIn();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                LogIn();
            }
        } 
    }
}
