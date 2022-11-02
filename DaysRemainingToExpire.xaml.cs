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
    /// Interaction logic for DaysRemainingToExpire.xaml
    /// </summary>
    public partial class DaysRemainingToExpire : Window
    {
        public DaysRemainingToExpire()
        {
            InitializeComponent();
            Subroutines subRoutines = new Subroutines();
            subRoutines.SetWindowPosition(this);
            DAL dal = new DAL();
            txtDaysRemaining.Text = dal.GetDaysRemainingToExpire().ToString();
            
        }

        private bool nonNumberEntered = false;
        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            if (txtDaysRemaining.Text.Trim() == "")
            {
                MessageBox.Show("Days Remaining cannot be left blank", "Validation Message", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if(Convert.ToInt32(txtDaysRemaining.Text.Trim())>365)
            {
                MessageBox.Show("Days Remaining cannot be more than 365 days", "Validation Message", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            DAL dal = new DAL();
            dal.UpdateDaysRemainingToExpire(txtDaysRemaining.Text.Trim(), txtDaysRemaining);
          
        }

        private void txtDaysRemaining_KeyDown(object sender, KeyEventArgs e)
        {
            ValidateNumericAndDecimalInput(sender, e);
        }


        private void ValidateNumericAndDecimalInput(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                nonNumberEntered = true;
            }

            //numbers from keypad
            if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                nonNumberEntered = false;
            }
            if (e.Key <= Key.D0 && e.Key >= Key.D9) // it`s number
            {
                nonNumberEntered = true;
            }
        //else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) // it`s number
        //    else if (e.Key == Key.Escape || e.Key == Key.Tab || e.Key == Key.CapsLock || e.Key == Key.LeftShift || e.Key == Key.LeftCtrl ||
        //        e.Key == Key.LWin || e.Key == Key.LeftAlt || e.Key == Key.RightAlt || e.Key == Key.RightCtrl || e.Key == Key.RightShift ||
        //        e.Key == Key.Left || e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Right || e.Key == Key.Return || e.Key == Key.Delete ||
        //        e.Key == Key.System)
        //    {

        //    }// it`s a system key (add other key here if you want to allow)
        //    else
            //    e.Handled = true;

            //if (!char.IsDigit((char)e.Key) && !e.Key.Equals(".") && !e.Key.Equals("8") && !e.Key.Equals("13")) //&& e.KeyChar.ToString().Contains('.').ToString().Length>=2
            //{
            //    nonNumberEntered = true;
            //}
            //if (e.Key.Equals(".") && (sender as TextBox).Text.IndexOf('.') > -1)
            //{
            //    nonNumberEntered = true;
            //    e.Handled = true;
            //}

            if (nonNumberEntered == true)
            {
                MessageBox.Show("Please enter number only....", "Validation Message", MessageBoxButton.OK, MessageBoxImage.Information);
                e.Handled = true;
            }

            nonNumberEntered = false;
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            DAL dal = new DAL();
            dal.UpdateDaysRemainingToExpire("90", txtDaysRemaining);            
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
