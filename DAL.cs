using System; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Data.SqlClient; 
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Threading;
using System.Globalization;

namespace LicenseTracking
{
    public class DataViewModel : INotifyPropertyChanged
    {
        #region Member Variables
        DataTable mData;
        #endregion

        #region Constructors

        /*
		 * The default constructor
 		 */
        public DataViewModel(DataTable inData)
        {
            mData = inData;
        }

        #endregion

        #region Properties
        public System.Data.DataView View
        {
            get { return (DataView)mData.DefaultView; }
        }

        public DataTable Table
        {
            get { return mData; }
        }
        #endregion

        #region Functions


        #endregion

        #region Enums
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    class DAL
    {
        string connect = ConfigurationManager.AppSettings["LicenseTrackingConn"].ToString();

        public SqlConnection ConnectToDB()
        {
            SqlConnection sqlConn = new SqlConnection(connect);
            sqlConn.Open();
            return sqlConn;             
        }

        public bool ValidateUser(string[] userInfo)
        {
            SqlConnection sqlConn = null;
            Subroutines subRout = null;
            try
            {
                sqlConn = ConnectToDB();
                subRout = new Subroutines();
                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd.Connection = sqlConn;
                //sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.CommandText = "select userPassword,isAdminActive from Users where userName=@userName";
                //sqlCmd.Parameters.Add("@pwd", SqlDbType.NVarChar).Size = 100;                 
                sqlCmd.Parameters.AddWithValue("@userName",userInfo[0].ToString());
                SqlDataReader sqlDtReader = sqlCmd.ExecuteReader();
                if (sqlDtReader.HasRows)
                {
                    sqlDtReader.Read();
                    string pwd = sqlDtReader[0].ToString();
                    bool isAdminActive = false;
                    if(sqlDtReader[1].ToString()!="")
                    {
                        isAdminActive = (bool)sqlDtReader[1];
                    }
                    if (subRout.Encrypt(userInfo[1].ToString(), true).Equals(pwd)) // userInfo[1] is pwd
                    {
                        sqlDtReader.Close();
                        sqlCmd.Dispose();
                        sqlCmd = new SqlCommand();
                        sqlCmd.Connection = sqlConn;
                        sqlCmd.CommandText = "select userPassword from Users where userName=@userName and isUserActive=1";
                        sqlCmd.Parameters.AddWithValue("@userName", userInfo[0].ToString());
                        sqlDtReader = sqlCmd.ExecuteReader();
                        if (!sqlDtReader.HasRows)
                        {
                            MessageBox.Show("Cannot login, you are not a active user", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            return false;
                        }
                        else
                        {
                            LicenseTracking licTrack = new LicenseTracking(userInfo[0].ToString(), isAdminActive);
                            licTrack.Show();
                            return true;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid credentials, not authenticate to login", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show("You are not a registered user", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }                 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception Occurred while performing operation with database", "Exception Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                sqlConn.Close();
            }     
        }

        public void FillTypes(ComboBox cmbBox, string sqlQuery,bool isAddBlankItem)
        {
            SqlConnection sqlConn = null;
            DataTable dtLicenseTypes = null;
            try
            {
                sqlConn = ConnectToDB();
                SqlCommand sqlCmd = new SqlCommand(sqlQuery, sqlConn);
                SqlDataAdapter sqlDtAdpter = new SqlDataAdapter(sqlCmd);
                dtLicenseTypes = new DataTable();
                sqlDtAdpter.Fill(dtLicenseTypes);                  
                sqlDtAdpter.Dispose();
                sqlCmd.Dispose();

                if (isAddBlankItem)
                {                 
                    DataRow dr = dtLicenseTypes.NewRow();
                    dr[0] = 0;
                    dr[1] = "";
                    dtLicenseTypes.Rows.Add(dr);
                }               

                cmbBox.ItemsSource = dtLicenseTypes.DefaultView; // contacts;
                if (!cmbBox.Name.Equals("cbProductName"))
                {
                    cmbBox.DisplayMemberPath = "licenseTypeName";
                    cmbBox.SelectedValuePath = "licenseTypeID";
                }
                else
                {
                    cmbBox.DisplayMemberPath = "productName";
                    cmbBox.SelectedValuePath = "productID";
                }
                
            }
            catch(Exception ex)
            {
                MessageBox.Show("Exception Occurred while performing operation with database", "Exception Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConn.Close();
            }            
        }

        public DataTable GetDataTable(string sqlQuery)
        {
            DataTable dt = null;
            DataSet ds = new DataSet();
            try
            {
                SqlConnection sqlConn = ConnectToDB();
                SqlDataAdapter da = new SqlDataAdapter(sqlQuery, sqlConn);
                da.Fill(ds);
                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Occurred while performing operation with database", "Error Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return dt;
        }

        private int numberOfRecPerPage=10;

        public List<object> FillDataGrid(DataGrid dgLicenses, string sqlQuery, bool isSearch, bool isUser, Label lblPageInfo)
        {
            SqlConnection sqlConn = null;
            DataTable dtGridData = null;
            List<object> dataList = null;
            try
            {
                sqlConn = ConnectToDB();
                SqlCommand sqlCmd = new SqlCommand(sqlQuery, sqlConn);
                SqlDataAdapter sqlDtAdpter = new SqlDataAdapter(sqlCmd);
                dtGridData = new DataTable();                
                sqlDtAdpter.Fill(dtGridData);
                sqlDtAdpter.Dispose();
                sqlCmd.Dispose();
                 
                if (dtGridData.Rows.Count != 0)
                {
                    dgLicenses.Visibility = Visibility.Visible;
                    Subroutines sub = new Subroutines();                    

                    if (!isUser)
                    {
                        if (dgLicenses.Name.Equals("dgLicenses"))
                        {
                            dataList = sub.GetProductList(dtGridData);
                        }
                        else
                        {
                            dataList = sub.GetUsersList(dtGridData);
                        }
                    }
                    else
                    {
                        dataList = sub.GetAsgUsersList(dtGridData);
                    }

                    //DataViewModel model = new DataViewModel(new Data());
                    //dgLicenses.DataContext = dataList
                    dgLicenses.ItemsSource =dataList.Take(numberOfRecPerPage);
                    //DataGridTemplateColumn actionColumn = new DataGridTemplateColumn { CanUserReorder = false, Width = 85, CanUserSort = true };
                    //actionColumn.Header = "Action";
                    //actionColumn.CellTemplateSelector = new ActionDataTemplateSelector();
                    //dgLicenses.Columns.Add(actionColumn);
                    int count = dataList.Take(numberOfRecPerPage).Count();
                    lblPageInfo.Content = count + " of " + dataList.Count;                    
                }

                if (isUser)
                {                 
                    if (dtGridData.Rows.Count == 0)
                    {
                        dgLicenses.ItemsSource = dtGridData.DefaultView;
                        MessageBox.Show("No users has been assigned license", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                if (isSearch)
                {
                    if(dtGridData.Rows.Count==0)
                    {
                        dgLicenses.ItemsSource = dtGridData.DefaultView;
                        MessageBox.Show("For the search text, No records found", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
               
                dgLicenses.IsReadOnly = true;
                dgLicenses.CanUserAddRows = false;                                

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Occurred while performing operation with database", "Error Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConn.Close();
            }
            return dataList;
        }
       

        public void AddOrEditProdLicenseInfo(string[] licenseData)
        {
            SqlConnection sqlConn = null;            
            try
            {
                sqlConn = ConnectToDB();
                SqlCommand sqlCmd = new SqlCommand(); 
                sqlCmd.Connection = sqlConn;
                if (licenseData.Length == 7)
                {
                    sqlCmd.CommandText = "select productID from Products where LOWER(productName)='" + licenseData[2].ToLower() + "'";
                    SqlDataReader sqlDtReader = sqlCmd.ExecuteReader();
                    if (sqlDtReader.HasRows)
                    {
                        MessageBox.Show("Entered product name is already exists in the database", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        sqlDtReader.Close();
                    }
                    else
                    {
                        sqlDtReader.Close();
                        sqlCmd = new SqlCommand();
                        sqlCmd.Connection = sqlConn;
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.CommandText = "AddOrUpdateProductLicenseData";
                        sqlCmd.Parameters.AddWithValue("@productID", "0");
                        sqlCmd.Parameters.AddWithValue("@providerName", licenseData[0].ToString());
                        sqlCmd.Parameters.AddWithValue("@providerAddress", licenseData[1].ToString());
                        sqlCmd.Parameters.AddWithValue("@productName", licenseData[2].ToString());
                        sqlCmd.Parameters.AddWithValue("@licenseActivationDate", licenseData[3].ToString());
                        sqlCmd.Parameters.AddWithValue("@licenseExpirationDate", licenseData[4].ToString());
                        sqlCmd.Parameters.AddWithValue("@licenseTypeID", licenseData[5].ToString());
                        sqlCmd.Parameters.AddWithValue("@noOfLicenses", licenseData[6].ToString());
                        sqlCmd.ExecuteNonQuery();
                        sqlCmd.Dispose();
                        MessageBox.Show("Product's Data inserted successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {                     
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.CommandText = "AddOrUpdateProductLicenseData";
                    sqlCmd.Parameters.AddWithValue("@providerName", "");
                    sqlCmd.Parameters.AddWithValue("@providerAddress", "");
                    sqlCmd.Parameters.AddWithValue("@productName", "");
                    sqlCmd.Parameters.AddWithValue("@productID", licenseData[0].ToString());
                    sqlCmd.Parameters.AddWithValue("@licenseActivationDate", licenseData[1].ToString());
                    sqlCmd.Parameters.AddWithValue("@licenseExpirationDate", licenseData[2].ToString());
                    sqlCmd.Parameters.AddWithValue("@licenseTypeID", licenseData[3].ToString());
                    sqlCmd.Parameters.AddWithValue("@noOfLicenses", licenseData[4].ToString());
                    sqlCmd.ExecuteNonQuery();
                    sqlCmd.Dispose();
                    MessageBox.Show("Product's Data updated successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception Occurred while performing operation with database", "Exception Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConn.Close();
            }  
        }

        public void EditProdLicenseInfo(string[] licenseData)
        {
            SqlConnection sqlConn = null;
            try
            {
                sqlConn = ConnectToDB();
                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd.Connection = sqlConn;               
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.CommandText = "EditProductLicenseData";
                sqlCmd.Parameters.AddWithValue("@providerName", licenseData[0].ToString());
                sqlCmd.Parameters.AddWithValue("@providerAddress", licenseData[1].ToString());
                sqlCmd.Parameters.AddWithValue("@productName", licenseData[2].ToString());
                sqlCmd.Parameters.AddWithValue("@licenseActivationDate", licenseData[3].ToString());
                sqlCmd.Parameters.AddWithValue("@licenseExpirationDate", licenseData[4].ToString());
                sqlCmd.Parameters.AddWithValue("@licenseTypeID", licenseData[5].ToString());
                sqlCmd.Parameters.AddWithValue("@noOfLicenses", licenseData[6].ToString());
                sqlCmd.ExecuteNonQuery();
                sqlCmd.Dispose();
                MessageBox.Show("Product's Data updated successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception Occurred while performing operation with database", "Exception Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConn.Close();
            }
        }

        public int GetDaysRemainingToExpire()
        {
            int days = 0;
            SqlConnection sqlConn = null;
            try
            {

                sqlConn = ConnectToDB();
                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd.Connection = sqlConn;
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.CommandText = "GetDaysRemainingToExpire";
                SqlParameter output = new SqlParameter("@daysRemainingToExpire", SqlDbType.Int);
                output.Direction = ParameterDirection.Output;
                sqlCmd.Parameters.Add(output);
                sqlCmd.ExecuteNonQuery();
                //sqlDtReader.Read();
                days = Convert.ToInt32(output.Value);                
                sqlCmd.Dispose();
                //MessageBox.Show("Days Remaining to expire has been set successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                days = 0;
                MessageBox.Show("Exception Occurred while performing operation with database", "Exception Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConn.Close();
            }
            return days;
        }

        public void UpdateDaysRemainingToExpire(string days, TextBox txtBox)
        {
            SqlConnection sqlConn = null;
            try
            {
                sqlConn = ConnectToDB();
                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd.Connection = sqlConn;
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.CommandText = "UpdateDaysRemainingToExpire";
                sqlCmd.Parameters.AddWithValue("@days", days);                                 
                sqlCmd.ExecuteNonQuery();
                sqlCmd.Dispose();
                txtBox.Text = days;
                MessageBox.Show("Days Remaining to expire has been set successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception Occurred while performing operation with database", "Exception Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConn.Close();
            }
        }

        public void AddOrUpdateUser(string[] userInfo, bool isEdit)
        {
            SqlConnection sqlConn = null;
            Subroutines subRout = new Subroutines();
            try
            {
                sqlConn = ConnectToDB();
                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd.Connection = sqlConn;
                if (!isEdit)
                {                   
                    sqlCmd.CommandText = "select userID from Users where Lower(userName)=@userName"; //'" + userInfo[0].ToString().Trim() + "'";
                    sqlCmd.Parameters.AddWithValue("@userName", userInfo[0].ToString().Trim().ToLower());
                    SqlDataReader sqlDtReader = sqlCmd.ExecuteReader();
                    if (sqlDtReader.HasRows)
                    {
                        MessageBox.Show("User already created for the application", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        sqlDtReader.Close();
                    }
                    else
                    {
                        sqlDtReader.Close();
                        sqlCmd.Dispose();                         
                        sqlCmd = new SqlCommand();
                        sqlCmd.Connection = sqlConn;                        
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.CommandText = "AddOrUpdateUserInfo";
                        sqlCmd.Parameters.AddWithValue("@userName", userInfo[0].ToString());
                        sqlCmd.Parameters.AddWithValue("@userID", "");
                        sqlCmd.Parameters.AddWithValue("@userPassword", subRout.Encrypt(userInfo[1].ToString(), true));
                        sqlCmd.Parameters.AddWithValue("@isUserActive", userInfo[3]);
                        sqlCmd.Parameters.AddWithValue("@isAdminActive", userInfo[4]);
                        sqlCmd.ExecuteNonQuery();
                        sqlCmd.Dispose();
                        MessageBox.Show("User has been successfully created", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.CommandText = "AddOrUpdateUserInfo";
                    sqlCmd.Parameters.AddWithValue("@userName", "");                    
                    sqlCmd.Parameters.AddWithValue("@userID", userInfo[2].ToString());
                    sqlCmd.Parameters.AddWithValue("@userPassword", subRout.Encrypt(userInfo[1].ToString(), true));
                    sqlCmd.Parameters.AddWithValue("@isUserActive", userInfo[3]);
                    sqlCmd.Parameters.AddWithValue("@isAdminActive", userInfo[4]);
                    sqlCmd.ExecuteNonQuery();
                    sqlCmd.Dispose();
                    MessageBox.Show("User Password has been successfully updated", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception Occurred while performing operation with database", "Exception Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConn.Close();
            }
        }

        public void AddAssignedLicenseToUser(string[] userLicenseInfo)
        {
            SqlConnection sqlConn = null;
            try
            {
                sqlConn = ConnectToDB();
                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd.Connection = sqlConn;
                sqlCmd.CommandText = "select assignedUserID from AssignedLicensedUsers where Lower(firstName)='" + userLicenseInfo[0].ToString().Trim() + "' and Lower(lastName)='" + userLicenseInfo[1].ToString().Trim() +
                "' and productID="+userLicenseInfo[2].ToString();
                SqlDataReader sqlDtReader = sqlCmd.ExecuteReader();
                if (sqlDtReader.HasRows)
                {
                    MessageBox.Show("For the selected product, User has already assigned a license", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    sqlDtReader.Close();
                }
                else 
                {
                    sqlDtReader.Close();
                    sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.CommandText = "AddAssignedLicensedUser";
                    sqlCmd.Parameters.AddWithValue("@firstName", userLicenseInfo[0].ToString());
                    sqlCmd.Parameters.AddWithValue("@lastName", userLicenseInfo[1].ToString());
                    sqlCmd.Parameters.AddWithValue("@productID", userLicenseInfo[2].ToString());
                    sqlCmd.Parameters.AddWithValue("@dateAssigned", userLicenseInfo[3].ToString());
                    sqlCmd.ExecuteNonQuery();                     
                    sqlCmd.Dispose();
                    MessageBox.Show("User has been successfully assigned the product license", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception Occurred while performing operation with database", "Exception Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConn.Close();
            }
        }

        public int ExecuteNonQuery(string sqlQuery)
        {
            int result = 0;
            SqlConnection sqlConn = null;
            try
            {
                sqlConn = ConnectToDB();
                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd.Connection = sqlConn;
                sqlCmd.CommandText = sqlQuery;
                result = sqlCmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                MessageBox.Show("Exception Occurred while performing operation with database", "Exception Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConn.Close();
            }
            return result;
        }

    }

   
}
