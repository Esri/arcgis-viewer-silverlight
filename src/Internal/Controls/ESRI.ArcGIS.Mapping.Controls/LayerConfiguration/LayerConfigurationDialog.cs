/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class LayerConfigurationDialog : Control, INotifyPropertyChanged
    {
        public LayerConfigurationDialog()
        {
            this.DefaultStyleKey = typeof(LayerConfigurationDialog);
            DataContext = this;
        }
        private View _view;
        public View View
        {
            get
            {
                return _view;
            }
            set
            {
                _view = value;
                OnPropertyChanged("View");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
