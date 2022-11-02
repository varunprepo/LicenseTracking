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
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
            //logo.Source = new BitmapImage(new Uri("~/Image/Asghal New Logo.jpg", UriKind.Relative)); 
            //ImageSource imageSource = new BitmapImage(new Uri("C:\\FileName.gif"));f:\WpfApplication\WpfApplication\WpfApplication\Image\Asghal New Logo.jpg
            //logo.Source = imageSource;
        }
    }
}
