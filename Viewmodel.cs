using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication.Converters
{
    public class Viewmodel : INotifyPropertyChanged
    {

        private ObservableCollection<BackgroundColors> collection;
        public ObservableCollection<BackgroundColors> Collection
        {
            get { return collection; }
            set { collection = value; OnPropertyChanged("Collection"); }
        }


        public Viewmodel()
        {
            Collection = new ObservableCollection<BackgroundColors>();
            BackgroundColors color1 = new BackgroundColors { HexColor = "#FFFFFF00" };
            BackgroundColors color2 = new BackgroundColors { HexColor = "#FF90EE90" };
            BackgroundColors color3 = new BackgroundColors { HexColor = "#FFFF0000" };
            DispatchService.Invoke(() =>
            {
                Collection.Add(color1);
                Collection.Add(color2);
                Collection.Add(color3);
            });
        }


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
