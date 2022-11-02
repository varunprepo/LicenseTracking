using System;
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
    /// Interaction logic for AddLicenseInfo.xaml
    /// </summary>
    public partial class AddOrEditLicenseInfo : Window
    {
        private DataGrid _dgLicenses = null;
        private Label _lblPageInfo = null;
        private string _prodID = null;
        public AddOrEditLicenseInfo(DataGrid dgLicenses, Label lblPageInfo, string prodID)
        {
            InitializeComponent();
            _prodID = prodID;
            _dgLicenses = dgLicenses;
            DAL dal = new DAL();                   
            dal.FillTypes(cmbTypeOfLicense, "SELECT [licenseTypeID],[licenseTypeName] FROM [LicenseTypes]",false);
            if (prodID.Equals("0"))
            {
                lblHeader.Content = "Add Product License Info";                  
            }
            else
            {
                lblHeader.Content = "Edit Product License Info";
                txtProviderName.IsEnabled = false;
                txtOfficeAddress.IsEnabled = false;
                txtProductName.IsEnabled = false;
                this.Title = "Edit License Info";                 
                                  
                DataTable dt = dal.GetDataTable("select prov.providerName,prov.providerAddress,prod.productName,prod.licenseActivationDate,prod.licenseExpirationDate,prod.licenseTypeID," +
                "prod.noOfLicenses from Products prod inner join Providers prov on prod.providerID=prov.providerID where productID=" + prodID);
                txtProviderName.Text = dt.Rows[0]["providerName"].ToString();
                txtOfficeAddress.Text = dt.Rows[0]["providerAddress"].ToString();
                txtProductName.Text = dt.Rows[0]["productName"].ToString();
                dtPickLicActDate.Text = dt.Rows[0]["licenseActivationDate"].ToString();
                dtPickLicExpDate.Text = dt.Rows[0]["licenseExpirationDate"].ToString();
                cmbTypeOfLicense.SelectedValue = dt.Rows[0]["licenseTypeID"].ToString();
                txtNoLicenses.Text = dt.Rows[0]["noOfLicenses"].ToString();                     
                
            }
            _lblPageInfo = lblPageInfo;
            Subroutines subRoutines = new Subroutines();
            subRoutines.SetWindowPosition(this);
            
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (txtProductName.Text =="")
            {
                MessageBox.Show("Product Name cannot be left blank", "Validation Info", MessageBoxButton.OK, MessageBoxImage.Information);
                txtProductName.Focus();
                return;
            }
            else if (lblProdNameValidIndicator.IsVisible==true)
            {
                MessageBox.Show("Entered product name is already exists in the database", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                txtProductName.Focus();
                return;
            }
            else if (txtOfficeAddress.Text == "")
            {
                MessageBox.Show("Office Address cannot be left blank", "Validation Info", MessageBoxButton.OK, MessageBoxImage.Information);
                txtOfficeAddress.Focus();
                return;
            }
            else if (dtPickLicActDate.Text == "")
            {
                MessageBox.Show("Select License Activation date", "Validation Info", MessageBoxButton.OK, MessageBoxImage.Information);
                dtPickLicActDate.Focus();
                return;
            }
            else if (dtPickLicExpDate.Text == "")
            {
                MessageBox.Show("Select License Expiration date", "Validation Info", MessageBoxButton.OK, MessageBoxImage.Information);
                dtPickLicExpDate.Focus();
                return;
            }
            else if (cmbTypeOfLicense.SelectedIndex == -1)
            {
                MessageBox.Show("Select an item from the Type Of License", "Validation Info", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbTypeOfLicense.Focus();
                return;
            }
            else if (txtNoLicenses.Text == "")
            {
                MessageBox.Show("No. of Licenses cannot be left blank", "Validation Info", MessageBoxButton.OK, MessageBoxImage.Information);
                txtNoLicenses.Focus();
                return;
            }

            DAL dal = new DAL();
            string[] licenseInfo = null;
            if (_prodID.Equals("0"))
            {
                licenseInfo = new string[7];
                licenseInfo[0] = txtProviderName.Text;
                licenseInfo[1] = txtOfficeAddress.Text;
                licenseInfo[2] = txtProductName.Text;                
                licenseInfo[3] = dtPickLicActDate.Text;
                licenseInfo[4] = dtPickLicExpDate.Text;
                licenseInfo[5] = cmbTypeOfLicense.SelectedValue.ToString();
                licenseInfo[6] = txtNoLicenses.Text;
            }
            else
            {
                licenseInfo = new string[5];               
                licenseInfo[0] = _prodID;
                licenseInfo[1] = dtPickLicActDate.Text;
                licenseInfo[2] = dtPickLicExpDate.Text;
                licenseInfo[3] = cmbTypeOfLicense.SelectedValue.ToString();
                licenseInfo[4] = txtNoLicenses.Text;
            }            

            dal.AddOrEditProdLicenseInfo(licenseInfo);
            this.Close();

            dal.FillDataGrid(_dgLicenses, "SELECT Products.productName as [Product Name], Products.licenseActivationDate as [License Activation Date], Products.licenseExpirationDate as [License Expiration Date], " +
            "LicenseTypes.licenseTypeName as [License Type Name], Products.noOfLicenses as [No Of Licenses], Providers.providerName as [Provider Name],Products.hpLnkToViewUsers,Products.hpLnkToEditLicDetails,Products.productID "+
            "FROM Products INNER JOIN Providers ON Products.providerID = Providers.providerID INNER JOIN LicenseTypes ON Products.licenseTypeID = LicenseTypes.licenseTypeID", false, false, _lblPageInfo);
        }

         

        private void txtProductName_LostFocus(object sender, RoutedEventArgs e)
        {
            SqlConnection sqlConn = null;
            try
            {
                lblProdNameValidIndicator.Visibility = Visibility.Hidden;
                DAL dal = new DAL();
                sqlConn = dal.ConnectToDB();
                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd.Connection = sqlConn;
                sqlCmd.CommandText = "select productID from Products where LOWER(productName)='" + txtProductName.Text.ToLower() + "'";
                SqlDataReader sqlDtReader = sqlCmd.ExecuteReader();
                if (sqlDtReader.HasRows)
                {
                    MessageBox.Show("Entered product name is already exists in the database", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    lblProdNameValidIndicator.Visibility = Visibility.Visible;
                    //txtProductName.Text = "";
                }
                sqlDtReader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception occurred while performing operation on database", "Exception Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConn.Close();
            }
        }

        
    }
}
