using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
    /// Interaction logic for LicenseTracking.xaml
    /// </summary>
    public partial class LicenseTracking : Window
    {
        
        //To check the paging direction according to use selection.
        private enum PagingMode
        { First = 1, Next = 2, Previous = 3, Last = 4 };

        private List<object> dataList = null;

        Subroutines subRoutines = null;
        private string _userName = null;
        bool _isAdmin = false;
        public LicenseTracking(string userName,bool isAdmin)
        {
            InitializeComponent();
            lblWelcome.Content = "Welcome " + userName;
            subRoutines = new Subroutines();
            subRoutines.SetWindowPosition(this);
            _userName = userName;
            if (!isAdmin)
            {
                CreateNewUsers.Visibility = Visibility.Hidden;
                ViewExistingUsers.Visibility = Visibility.Hidden;
                btnDeleteLicenses.Visibility = Visibility.Hidden;
                ViewLicRpt.Visibility = Visibility.Hidden;
                addNewLicMenuItem.Visibility = Visibility.Hidden;
                usersMenu.Visibility = Visibility.Hidden;
                reportMenu.Visibility = Visibility.Hidden;
                AddNewLicInfo.Visibility = Visibility.Hidden;
                AssignedLicToUser.Visibility = Visibility.Hidden;
            }
            else
            {
                _isAdmin = true;
            }
            FillLicensesInfo();
        }        

        private void ShowLicensesInfo_Click(object sender, RoutedEventArgs e)
        {
            SetVisibilityOfControls(Visibility.Hidden);
            FillLicensesInfo();
        }

        private void HpLnkToViewUsers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(_isAdmin)
                {
                    ViewAssignedUsers vwAsgUrs = new ViewAssignedUsers(dgLicenses, lblPageInfo, Convert.ToInt16(dgLicenses.SelectedItem.ToString().Substring(dgLicenses.SelectedItem.ToString().IndexOf("productID")).Split('=')[1].Replace("}", "").Trim())
                    , _userName, _isAdmin); //.Split(',')[0]
                    vwAsgUrs.ShowDialog();             
                }
                else
                {
                    MessageBox.Show("Sorry, You are not Admin user. Cannot view assigned users.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Entered product name is already exists in the database", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }            
        }

        private void HpLnkToEditLicenseInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isAdmin)
                {
                    var rowData = ((Hyperlink)e.OriginalSource).DataContext;
                    AddOrEditLicenseInfo addLicInfo = new AddOrEditLicenseInfo(dgLicenses, lblPageInfo, rowData.ToString().Substring(rowData.ToString().IndexOf("productID")).Split('=')[1].Replace("}", "").Trim());
                    addLicInfo.ShowDialog(); //Split(',')[0].
                }
                else
                {
                    MessageBox.Show("Sorry, You are not Admin user. Cannot edit license information.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Entered product name is already exists in the database", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        } 

        private void ShowSearch_Click(object sender, RoutedEventArgs e)
        {
           DAL dal = new DAL();         
           dgLicenses.ItemsSource = null;
           dal.FillTypes(cbLicenseType, "SELECT [licenseTypeID],[licenseTypeName] FROM [LicenseTypes]",true);
           SetVisibilityOfControls(Visibility.Visible);
        }

        private void SetVisibilityOfControls(Visibility isVisible)
        {
            lblLicType.Visibility = isVisible;
            lblSearch.Visibility = isVisible;
            cbLicenseType.Visibility = isVisible;
            lblLicActivationDate.Visibility = isVisible;
            dpLicActivationDate.Visibility = isVisible;
            lblLicExpirationDate.Visibility = isVisible;
            dpLicExpiryDate.Visibility = isVisible;
            lblProviderName.Visibility = isVisible;
            txtProviderName.Visibility = isVisible;
            btnProdSearch.Visibility = isVisible;  
        }

        //private string ProdNameSearchQuery()
        //{
        //    string searchQuery = "SELECT Products.productName as [Product Name], Products.licenseActivationDate as [License Activation Date], Products.licenseExpirationDate as [License Expiration Date], " +
        //    "LicenseTypes.licenseTypeName as [License Type Name], Products.noOfLicenses as [No Of Licenses], Providers.providerName as [Provider Name] FROM Products INNER JOIN Providers ON Products.providerID = Providers.providerID " +
        //    "INNER JOIN LicenseTypes ON Products.licenseTypeID = LicenseTypes.licenseTypeID where Products.productName like '%" + txtSearchProdName.Text + "%'";
        //    return searchQuery;
        //}

        //private string ProviderNameSearchQuery()
        //{
        //    string searchQuery = "SELECT Products.productName as [Product Name], Products.licenseActivationDate as [License Activation Date], Products.licenseExpirationDate as [License Expiration Date], " +
        //    "LicenseTypes.licenseTypeName as [License Type Name], Products.noOfLicenses as [No Of Licenses], Providers.providerName as [Provider Name] FROM Products INNER JOIN Providers ON Products.providerID = Providers.providerID " +
        //    "INNER JOIN LicenseTypes ON Products.licenseTypeID = LicenseTypes.licenseTypeID where Providers.providerName like '%" + txtSearchProvName.Text + "%'";
        //    return searchQuery;
        //}
        private string LicActivationDateSearchQuery()
        {
            string searchQuery ="SELECT Products.productName as [Product Name], Products.licenseActivationDate as [License Activation Date], Products.licenseExpirationDate as [License Expiration Date], " +
            "LicenseTypes.licenseTypeName as [License Type Name], Products.noOfLicenses as [No Of Licenses], Providers.providerName as [Provider Name],Products.hpLnkToViewUsers,Products.hpLnkToEditLicDetails,Products.productID FROM Products " +
            "INNER JOIN Providers ON Products.providerID = Providers.providerID INNER JOIN LicenseTypes ON Products.licenseTypeID = LicenseTypes.licenseTypeID where ";

            string queryCondition = null;
            if (dpLicActivationDate.Text != "")
            {
                queryCondition = "Products.licenseActivationDate = '" + dpLicActivationDate.Text + "'";                 
            }

            if (dpLicExpiryDate.Text != "")
            {
                if (queryCondition != null)
                {
                    queryCondition = queryCondition + " and Products.licenseExpirationDate = '" + dpLicExpiryDate.Text + "'";
                }
                else
                {
                    queryCondition = "Products.licenseExpirationDate = '" + dpLicExpiryDate.Text + "'";
                }                
            }

            if (cbLicenseType.SelectedIndex != -1 && cbLicenseType.SelectedIndex != 3)
            {
                if (queryCondition != null)
                {
                    queryCondition = queryCondition + " and LicenseTypes.licenseTypeName = '" + cbLicenseType.Text + "'";
                }
                else
                {
                    queryCondition = "LicenseTypes.licenseTypeID = '" + cbLicenseType.SelectedValue + "'";
                }                
            }

            string providerName = txtProviderName.Text.Trim();
            if (providerName != "")
            {
                if (queryCondition != null)
                {
                    queryCondition = queryCondition + " and Providers.providerName like '%" + providerName + "%'";
                }
                else
                {
                    queryCondition = "Providers.providerName like '%" + providerName + "%'";
                }
            }

            if (queryCondition != null)
            {
                searchQuery = searchQuery + queryCondition;
            }  
            else
            {
                searchQuery = searchQuery.Replace("where", "");
            }
            return searchQuery;
        }     

        private void btnLicSearch_Click(object sender, RoutedEventArgs e)
        {
            DAL dal = new DAL();
            dataList = dal.FillDataGrid(dgLicenses, LicActivationDateSearchQuery(), true, false, lblPageInfo);
        }

        //private void btnProvSearch_Click(object sender, RoutedEventArgs e)
        //{
        //    DAL dal = new DAL();
        //    dal.FillDataGrid(dgLicenses, LicActivationDateSearchQuery(), true);
            
        //}

        //private void txtSearchProdName_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        DAL dal = new DAL();
        //        dal.FillDataGrid(dgLicenses, ProdNameSearchQuery(), true);
        //    }
        //} 

        //private void txtSearchProdName_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    txtSearchProdName.Text = "";
        //}

        //private void txtSearchProvName_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        DAL dal = new DAL();
        //        dal.FillDataGrid(dgLicenses, LicActivationDateSearchQuery(), true);
        //    }
        //}

        //private void txtSearchProvName_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    txtSearchProvName.Text = "";
        //}

        private void AddLicenseInfo_Click(object sender, RoutedEventArgs e)
        {
            //var window = subRoutines.IsWindowOpen<Window>("Add License");
            if (_isAdmin)
            {
                AddOrEditLicenseInfo addLicenseInfo = new AddOrEditLicenseInfo(dgLicenses, lblPageInfo, "0");
                addLicenseInfo.ShowDialog();
            }            
            else
            {
                MessageBox.Show("Sorry, You are not Admin user. Cannot Add License Information.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            
        }

        private void AddUserLicenseInfo_Click(object sender, RoutedEventArgs e)
        {
            if (_isAdmin)
            {
                AssignedLicensedUser addLicensedUser = new AssignedLicensedUser();
                addLicensedUser.ShowDialog();
            }
            else
            {
                MessageBox.Show("Sorry, You are not Admin user. Cannot Assigned Licensed to User.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ReFillDataGrid()
        {
            DAL dal = new DAL();
            dataList = dal.FillDataGrid(dgLicenses, "SELECT Products.productName as [Product Name], Products.licenseActivationDate as [License Activation Date], Products.licenseExpirationDate as [License Expiration Date], " +
            "LicenseTypes.licenseTypeName as [License Type Name], Products.noOfLicenses as [No Of Licenses], Providers.providerName as [Provider Name],Products.hpLnkToViewUsers,Products.hpLnkToEditLicDetails,Products.productID FROM Products INNER JOIN " +
            "Providers ON Products.providerID = Providers.providerID INNER JOIN LicenseTypes ON Products.licenseTypeID = LicenseTypes.licenseTypeID", false, false, lblPageInfo);            
        }

        private void btnFirst_Click(object sender, System.EventArgs e)
        {
            Subroutines sub = new Subroutines();                         
            sub.Navigate((int)PagingMode.First, btnPrev, btnFirst, btnNext, btnLast, dgLicenses, 1, lblPageInfo);
        }

        private void btnNext_Click(object sender, System.EventArgs e)
        {
            Subroutines sub = new Subroutines();                          
            sub.Navigate((int)PagingMode.Next, btnPrev, btnFirst, btnNext, btnLast, dgLicenses, 1, lblPageInfo);
        }

        private void btnPrev_Click(object sender, System.EventArgs e)
        {
            Subroutines sub = new Subroutines();                          
            sub.Navigate((int)PagingMode.Previous, btnPrev, btnFirst, btnNext, btnLast, dgLicenses, 1, lblPageInfo);
        }

        private void btnLast_Click(object sender, System.EventArgs e)
        {
            Subroutines sub = new Subroutines();             
            sub.Navigate((int)PagingMode.Last, btnPrev, btnFirst, btnNext, btnLast, dgLicenses, 1, lblPageInfo);
        }

        private void ViewUsers_Click(object sender, RoutedEventArgs e)
        {
            ViewUsers vwUsr = new ViewUsers(_isAdmin);
            vwUsr.ShowDialog();
        }

        public DataTable DataViewAsDataTable(DataView dv)
        {
            DataTable dt = dv.Table.Clone();
            foreach (DataRowView drv in dv)
                dt.ImportRow(drv.Row);
            return dt;
        }

        private void ViewReport_Click(object sender, RoutedEventArgs e)
        {
            DAL dal = new DAL();
            DataTable dtLicenses = dal.GetDataTable("SELECT Products.productName as [Product Name], Products.licenseActivationDate as [License Activation Date], Products.licenseExpirationDate as [License Expiration Date], " +
            "LicenseTypes.licenseTypeName as [License Type Name], Products.noOfLicenses as [No Of Licenses], Providers.providerName as [Provider Name] FROM Products INNER JOIN " +
            "Providers ON Products.providerID = Providers.providerID INNER JOIN LicenseTypes ON Products.licenseTypeID = LicenseTypes.licenseTypeID");

            if (dtLicenses.Rows.Count != 0)
            {             

                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                Microsoft.Office.Interop.Excel._Application app = null;
                Microsoft.Office.Interop.Excel._Workbook workbook = null;
                //if(!mIsWorkOrder)
                //{
                // commented by Varun on 08/Jun/15 saveFileDialog1.Filter = "Excel Path|*.xls|Excel Format|*.xlsx";

                saveFileDialog1.Filter = "Excel Path|*.xls|Excel Path|*.xlsx";
                saveFileDialog1.Title = "Save an Excel File";
                saveFileDialog1.ShowDialog();

                if (saveFileDialog1.FileName != "")
                {
                    // creating Excel Application
                    app = new Microsoft.Office.Interop.Excel.Application();

                    // creating new WorkBook within Excel application
                    workbook = app.Workbooks.Add(Type.Missing);

                    // creating new Excelsheet in workbook
                    Microsoft.Office.Interop.Excel._Worksheet worksheet = null;

                    // see the excel sheet behind the program
                    app.Visible = true;

                    // get the reference of first sheet. By default its name is Sheet1.
                    // store its reference to worksheet
                    worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets["Sheet1"];
                    worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.ActiveSheet;

                    Microsoft.Office.Interop.Excel.Range formatRange = worksheet.UsedRange;
                    Microsoft.Office.Interop.Excel.Range formatA1CellRange = null;
                    Microsoft.Office.Interop.Excel.Range formatA1Cell = null;
                    //Microsoft.Office.Interop.Excel.Range formatOtherCell = null;

                    formatA1CellRange = worksheet.get_Range("A1", "G1");
                    formatA1Cell = (Microsoft.Office.Interop.Excel.Range)formatRange.Cells[1, 7];
                    formatA1Cell.Font.Size = 30;
                    formatA1CellRange.ColumnWidth = 150;
                    formatA1CellRange.Merge(true);                   
                    formatA1CellRange.FormulaR1C1 = "LICENSES REPORT";
                    formatA1CellRange.Font.Bold = true;
                    formatA1CellRange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                                        
                    int days = dal.GetDaysRemainingToExpire();

                    //for (int colIdx = 1; colIdx <= 7; colIdx++)
                    //{
                    int colIdx = 1;
                    SetThicknessOfExcelCell(1, worksheet); //Heading of the Report
                    SetThicknessOfExcelCell(4, worksheet); //Column Headers of the Report
                    //}

                    for (int asciiCode = 65; asciiCode <= 71; asciiCode++)
                    {
                        char alphaBet = (char)asciiCode;
                        worksheet.get_Range(alphaBet + "4", alphaBet + "4").Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft].Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
                        if (asciiCode == 65)
                        {
                            worksheet.get_Range(alphaBet + "4", alphaBet + "4").ColumnWidth = 6;
                        }
                        else if (asciiCode == 66)
                        {
                            worksheet.get_Range(alphaBet + "4", alphaBet + "4").ColumnWidth = 16;
                        }
                        else if (asciiCode == 67 || asciiCode == 68)
                        {
                            worksheet.get_Range(alphaBet + "4", alphaBet + "4").ColumnWidth = 21;
                        }
                        else if (asciiCode == 69)
                        {
                            worksheet.get_Range(alphaBet + "4", alphaBet + "4").ColumnWidth = 18;
                        }
                        else if (asciiCode == 70)
                        {
                            worksheet.get_Range(alphaBet + "4", alphaBet + "4").ColumnWidth = 17;
                        }
                        else if (asciiCode == 71)
                        {
                            worksheet.get_Range(alphaBet + "4", alphaBet + "4").ColumnWidth = 23;
                        }
                                 
                    }

                    //for (int i = 0; i < 1; i++)
                    //{
                        //worksheet.get_Range("C" + (i + 5).ToString(), "C" + (i + 5).ToString()).Font
                        
                        worksheet.Cells[4, 1].Font.Bold = true;
                        worksheet.Cells[4, 1].
                        worksheet.Cells[4, 1] = "SNo.";
                        for (colIdx = 0; colIdx < 6; colIdx++) // Need to print only six columns
                        {
                            if (colIdx == 0)
                            {
                                worksheet.Cells[4, 2].Font.Bold = true;
                                worksheet.Cells[4, 2] = dtLicenses.Columns[colIdx].ColumnName;
                            }
                            else if (colIdx == 1)
                            {
                                worksheet.Cells[4, 3].Font.Bold = true;
                                worksheet.get_Range("C4").NumberFormat = "dd/mmm/yy hh:m:ss";
                                worksheet.Cells[4, 3] = dtLicenses.Columns[colIdx].ColumnName;
                            }
                            else if (colIdx == 2)
                            {
                                worksheet.Cells[4, 4].Font.Bold = true;
                                worksheet.get_Range("D4").NumberFormat = "dd/mmm/yy hh:m:ss";
                                //worksheet.Cells[i + 5, j + 4].Style.Format = "dd/MMM/yy hh:m:ss tt";                                 
                                worksheet.Cells[4, 4] = dtLicenses.Columns[colIdx].ColumnName;
                            }
                            else if (colIdx == 3)
                            {
                                worksheet.Cells[4, 5].Font.Bold = true;
                                worksheet.Cells[4, 5] = dtLicenses.Columns[colIdx].ColumnName;
                            }
                            else if (colIdx == 4)
                            {
                                worksheet.Cells[4, 6].Font.Bold = true;
                                worksheet.Cells[4, 6] = dtLicenses.Columns[colIdx].ColumnName;
                            }
                            else if (colIdx == 5)
                            {
                                worksheet.Cells[4, 7].Font.Bold = true;
                                worksheet.Cells[4, 7] = dtLicenses.Columns[colIdx].ColumnName;
                            }
                            //if (dtLicenses.Rows[i][j].ToString() != "" && j != 5 && j != 4)
                            //    worksheet.Cells[i + 5, j + 2] = dgvTendererInfo.Rows[i].Cells[j].Value.ToString();
                            //else if ((dtLicenses.Rows[i][4].ToString() == "" ||
                            //    dgvTendererInfo.Rows[i].Cells[4].Value.ToString().Equals("Regret and Not Submitted")) && j == 4)
                            //    worksheet.Cells[i + 5, j + 2] = "Not Submitted";
                            //else if (dgvTendererInfo.Rows[i].Cells[5].Value.ToString() != "")
                            //{
                            //    worksheet.Cells[i + 5, j + 2] = dgvTendererInfo.Rows[i].Cells[j].Style.Format = "dd/MMM/yy hh:m:ss tt";
                            //    worksheet.Cells[i + 5, j + 2] = dgvTendererInfo.Rows[i].Cells[j].Value.ToString();
                            //}
                        }
                    //}
                    // storing Each row and column value to excel sheet
                    for (int rowIdx = 0; rowIdx < dtLicenses.Rows.Count; rowIdx++)
                    {

                        SetThicknessOfExcelCell(rowIdx + 5, worksheet);
                        worksheet.Cells[rowIdx + 5, 1] = rowIdx + 1;
                        for (colIdx = 0; colIdx < 6; colIdx++) // Need to print only six columns
                        {
                            if (colIdx == 0)
                            {
                                worksheet.Cells[rowIdx + 5, 2] = dtLicenses.Rows[rowIdx][colIdx].ToString();
                            }
                            else if (colIdx == 1)
                            {
                                worksheet.get_Range("C" + (rowIdx + 5).ToString(), "C" + (rowIdx + 5).ToString()).NumberFormat = "dd/mmm/yy hh:m:ss tt";                                                          
                                worksheet.Cells[rowIdx + 5, 3] = dtLicenses.Rows[rowIdx][colIdx].ToString();
                            }
                            else if (colIdx == 2)
                            {
                                worksheet.get_Range("D" + (rowIdx + 5).ToString(), "D" + (rowIdx + 5).ToString()).NumberFormat = "dd/mmm/yy hh:m:ss tt"; 
                                //worksheet.Cells[i + 5, j + 4].Style.Format = "dd/MMM/yy hh:m:ss tt";   
                                string licExpiryDate = dtLicenses.Rows[rowIdx][colIdx].ToString();
                                worksheet.Cells[rowIdx + 5, 4] = licExpiryDate;

                                DateTime expiryDate = DateTime.Now;
                                string[] dtFormats = { "dd/MMM/yyyy HH:mm:ss", "yyyy/dd/mm HH:mm:ss", "mm/dd/yyyy HH:mm:ss", "mm/dd/yy HH:mm:ss", "m/d/yyyy HH:mm:ss" }; 
                                DateTime.TryParseExact(licExpiryDate, dtFormats, CultureInfo.CurrentCulture,
                                       DateTimeStyles.None, out expiryDate);
                                //string colorName = null;
                                int daysLeft = Math.Abs(DateTime.Now.Subtract(expiryDate).Days);

                                Microsoft.Office.Interop.Excel.Range rng = worksheet.get_Range("A" + (rowIdx + 5).ToString(), "G" + (rowIdx + 5).ToString());
                                if (DateTime.Now.CompareTo(expiryDate) == -1)
                                {
                                    if (daysLeft <= days)
                                    {
                                        rng.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DarkOrange);
                                    }
                                    else
                                    {
                                        rng.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGreen);
                                    }
                                }
                                else if (DateTime.Now.CompareTo(expiryDate) == 1 || DateTime.Now.CompareTo(expiryDate) == 0)
                                {
                                    rng.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);  
                                }
                            }
                            else if (colIdx == 3)
                            {                                 
                                worksheet.Cells[rowIdx + 5,  5] = dtLicenses.Rows[rowIdx][colIdx].ToString();
                            }
                            else if (colIdx == 4)
                            {                              
                                worksheet.Cells[rowIdx + 5,  6] = dtLicenses.Rows[rowIdx][colIdx].ToString();
                            }
                            else if (colIdx == 5)
                            {
                                worksheet.Cells[rowIdx + 5,  7] = dtLicenses.Rows[rowIdx][colIdx].ToString();
                            }
                            //if (dtLicenses.Rows[i][j].ToString() != "" && j != 5 && j != 4)
                            //    worksheet.Cells[i + 5, j + 2] = dgvTendererInfo.Rows[i].Cells[j].Value.ToString();
                            //else if ((dtLicenses.Rows[i][4].ToString() == "" ||
                            //    dgvTendererInfo.Rows[i].Cells[4].Value.ToString().Equals("Regret and Not Submitted")) && j == 4)
                            //    worksheet.Cells[i + 5, j + 2] = "Not Submitted";
                            //else if (dgvTendererInfo.Rows[i].Cells[5].Value.ToString() != "")
                            //{
                            //    worksheet.Cells[i + 5, j + 2] = dgvTendererInfo.Rows[i].Cells[j].Style.Format = "dd/MMM/yy hh:m:ss tt";
                            //    worksheet.Cells[i + 5, j + 2] = dgvTendererInfo.Rows[i].Cells[j].Value.ToString();
                            //}
                        }
                    }

                }
            }
            else
            {
                MessageBox.Show("No records found, Cannot generate report", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
                
            //ViewUsers vwUsr = new ViewUsers(_userName);
            //vwUsr.Show();
        }

        private void SetThicknessOfExcelCell(int rowIdx, Microsoft.Office.Interop.Excel._Worksheet worksheet)
        {

            for (int asciiCode = 65; asciiCode <= 71; asciiCode++)
            {
                char alphaBet = (char)asciiCode;
                worksheet.get_Range(alphaBet + rowIdx.ToString(), alphaBet + rowIdx.ToString()).Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft].Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
                worksheet.get_Range(alphaBet + rowIdx.ToString(), alphaBet + rowIdx.ToString()).Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop].Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
                worksheet.get_Range(alphaBet + rowIdx.ToString(), alphaBet + rowIdx.ToString()).Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeRight].Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
                worksheet.get_Range(alphaBet + rowIdx.ToString(), alphaBet + rowIdx.ToString()).Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
                //worksheet.get_Range(alphaBet + (i + 5).ToString(), alphaBet + (i + 5).ToString()).RowHeight = 25;
                //if (asciiCode == 71)
                //worksheet.get_Range(alphaBet + (rowIdx + 5).ToString(), alphaBet + (rowIdx + 5).ToString()).WrapText = true;
            }
        }
        private void CreateNewUsers_Click(object sender, RoutedEventArgs e)
        {
            AddNewUser addUsr = null;
            if (!_isAdmin)
            {
                MessageBox.Show("Cannot create new user. You are not an Admin", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                addUsr = new AddNewUser(false, 0, "", false,false,true);
            }
            addUsr.ShowDialog();
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

            List<CheckBox> checkBoxlist = new List<CheckBox>();
            // Find all elements            
            Subroutines subRoutines = new Subroutines();
            subRoutines.FindChildGroup<CheckBox>(dgLicenses, "chkBox", ref checkBoxlist);

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
            subRoutines.FindChildGroup<CheckBox>(dgLicenses, "chkBox", ref checkBoxlist);

            foreach (CheckBox c in checkBoxlist)
            {
                if ((bool)c.IsChecked)
                {
                    c.IsChecked = false;
                    //do whatever you want
                }
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

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();             
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            DaysRemainingToExpire daysToExp = null;
            if (!_isAdmin)
            {
                MessageBox.Show("Cannot create new user. You are not an Admin", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                daysToExp = new DaysRemainingToExpire();
            }
            daysToExp.ShowDialog();
        }

        private void btnDeleteLicenses_Click(object sender, RoutedEventArgs e)
        {
            Subroutines subRoutines = new Subroutines();
            if (!subRoutines.CheckChkBoxes(dgLicenses))
            {
                MessageBox.Show("For deleting a record, please check at least one checkbox", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            MessageBoxResult result = MessageBox.Show("Are you sure to delete the selected licenses?", "Delete Licenses", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                List<CheckBox> checkBoxlist = new List<CheckBox>();

                // Find all elements                 
                subRoutines.FindChildGroup<CheckBox>(dgLicenses, "chkBox", ref checkBoxlist);
                int rowIndex = 0;
                StringBuilder strBuild = new StringBuilder();
                foreach (CheckBox c in checkBoxlist)
                {
                    if ((bool)c.IsChecked)
                    {

                        strBuild.Append(dgLicenses.Items[rowIndex].ToString().Substring(dgLicenses.Items[rowIndex].ToString().IndexOf("productID")).Split('=')[1].Replace("}", "").Trim());
                        strBuild.Append(",");
                    }

                    rowIndex++;
                }
                if (strBuild.Length > 0)
                {
                    DAL dal = new DAL();
                    dal.ConnectToDB();
                    if (dal.ExecuteNonQuery("delete from Products where productID in (" + strBuild.ToString().Substring(0, strBuild.ToString().LastIndexOf(",")) + ")") != 0)
                    {
                        MessageBox.Show("License has been successfully deleted", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                }
                FillLicensesInfo();
            }  
        }

        private void FillLicensesInfo()
        {
            DAL dal = new DAL();
            dataList = dal.FillDataGrid(dgLicenses, "SELECT Products.productName as [Product Name], Products.licenseActivationDate as [License Activation Date], Products.licenseExpirationDate as [License Expiration Date], " +
            "LicenseTypes.licenseTypeName as [License Type Name], Products.noOfLicenses as [No Of Licenses], Providers.providerName as [Provider Name],Products.hpLnkToViewUsers,Products.hpLnkToEditLicDetails,Products.productID FROM Products INNER JOIN " +
            "Providers ON Products.providerID = Providers.providerID INNER JOIN LicenseTypes ON Products.licenseTypeID = LicenseTypes.licenseTypeID", false, false, lblPageInfo);                       

        }
      
    }

     

    public class ExpiryDateChecker : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //DateTime.TryParse(value);
            System.Data.DataRowView rowItem = (System.Data.DataRowView)value;
            string licExpiryDate = rowItem.Row.ItemArray[2].ToString();
            DateTime expiryDate = DateTime.Now;
            string[] dtFormats = { "dd/MMM/yyyy HH:mm:ss", "yyyy/dd/mm HH:mm:ss", "mm/dd/yyyy HH:mm:ss", "mm/dd/yy HH:mm:ss", "m/d/yyyy HH:mm:ss" };
            DateTime.TryParseExact(licExpiryDate, dtFormats, CultureInfo.CurrentCulture,
                   DateTimeStyles.None, out expiryDate);
            //string colorName = null;
            int daysLeft = Math.Abs(DateTime.Now.Subtract(expiryDate).Days);
            //SolidColorBrush solidClrBrush = null;
            string hexValue = null;
            if (DateTime.Now.CompareTo(expiryDate) == -1)
            {
                if (daysLeft <= 90)
                {
                    hexValue = Brushes.LightYellow.ToString();
                }
                else
                {
                    hexValue = Brushes.LightGreen.ToString();
                }
            }
            else if (DateTime.Now.CompareTo(expiryDate) == 1 || DateTime.Now.CompareTo(expiryDate) == 0)
            {
                hexValue = Brushes.Red.ToString();
            }
            return hexValue;  
            //return Convert. ((int)value) > ExpiryDate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        //public string ColorName { get; set; }
    }         

      
}
