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
    /// Interaction logic for AssignedLicensedUser.xaml
    /// </summary>
    public partial class AssignedLicensedUser : Window
    {
        public AssignedLicensedUser()
        {
            InitializeComponent();
            Subroutines subRoutines = new Subroutines();
            subRoutines.SetWindowPosition(this);
            DAL dal = new DAL();
            dal.FillTypes(cbProductName, "select productID,productName from Products", false);
        }

        private void btnAsgnLicToUser_Click(object sender, RoutedEventArgs e)
        {          

            if (txtFirstName.Text =="")
            {
                MessageBox.Show("First Name cannot be left blank", "Validation Message", MessageBoxButton.OK, MessageBoxImage.Information);
                txtFirstName.Focus();
                return;
            }
            else if (txtLastName.Text =="")
            {
                MessageBox.Show("Last Name cannot be left blank", "Validation Message", MessageBoxButton.OK, MessageBoxImage.Information);
                txtLastName.Focus();
                return;
            }
            else if (cbProductName.SelectedIndex == -1)
            {
                MessageBox.Show("Select Product Name", "Validation Message", MessageBoxButton.OK, MessageBoxImage.Information);
                cbProductName.Focus();
                return;
            }
            else if (dpDateAssigned.Text == "")
            {
                MessageBox.Show("Select Date Assigned", "Validation Message", MessageBoxButton.OK, MessageBoxImage.Information);
                dpDateAssigned.Focus();
                return;
            }             

            DAL dal = new DAL();     
            string [] userLicenseInfo = new string[4];
            userLicenseInfo[0] = txtFirstName.Text;
            userLicenseInfo[1] = txtLastName.Text;
            userLicenseInfo[2] = cbProductName.SelectedValue.ToString();
            userLicenseInfo[3] = dpDateAssigned.Text;

            dal.AddAssignedLicenseToUser(userLicenseInfo);
            this.Close();

        }
    }
}
