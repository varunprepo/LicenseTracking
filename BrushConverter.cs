using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace LicenseTracking.Converters
{
    public class BrushConverter : IValueConverter
    {
        int days = 0;
        public BrushConverter()
        {
            DAL dal = new DAL();   
            days = dal.GetDaysRemainingToExpire();
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush solidClrBrush = null;                            
            string licExpiryDate = value.ToString().Substring(value.ToString().IndexOf("LicenseExpirationDate")).Split('=')[1].Split(',')[0].Trim().Split(' ')[0]; // rowItem.Row.ItemArray[2].ToString();
                      
            //string daysToExpire = value.ToString().Substring(value.ToString().IndexOf("LicenseExpirationDate")).Split('=')[1].Split(',')[0].Trim().Split(' ')[0];
            
            DateTime expiryDate = DateTime.Now;
            string[] dtFormats = { "dd/MMM/yyyy", "yyyy/dd/mm", "mm/dd/yyyy", "mm/dd/yy", "m/d/yyyy" };
            DateTime.TryParseExact(licExpiryDate, dtFormats, CultureInfo.CurrentCulture,
                    DateTimeStyles.None, out expiryDate);                 
            int daysLeft = Math.Abs(DateTime.Now.Subtract(expiryDate).Days);
                
            if (DateTime.Now.CompareTo(expiryDate) == -1)
            {
                if (daysLeft <= days)
                {
                    solidClrBrush = Brushes.LightYellow;
                     
                }
                else
                {
                    solidClrBrush = Brushes.LightGreen;
                }
            }
            else if (DateTime.Now.CompareTo(expiryDate) == 1 || DateTime.Now.CompareTo(expiryDate) == 0)
            {
                //byte R = Convert(Color.Substring(1, 2), 16);
                //byte G = Convert.ToByte(color.Substring(3, 2), 16);
                //byte B = Convert.ToByte(color.Substring(5, 2), 16);
                
                try
                {
                    solidClrBrush = new SolidColorBrush(Color.FromRgb(255, 80, 80));
                    //solidClrBrush = (SolidColorBrush)(new BrushConverter().Convert("#F08080", null, null, CultureInfo.InvariantCulture)); // Brushes.Red;
                }
                catch (Exception ex)
                {
                    string str = ex.Message;                     
                }
                
            }           
            return solidClrBrush.ToString();             
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
