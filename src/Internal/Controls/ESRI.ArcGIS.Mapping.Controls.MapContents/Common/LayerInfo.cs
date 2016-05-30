/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Controls.MapContents
{
    public class LayerInfo : INotifyPropertyChanged
    {
        #region Layer
        private Layer _layer;
        public Layer Layer
        {
            get { return _layer; }
            set
            {
                if (_layer != value)
                {
                    _layer = value;
                    OnPropertyChanged("Layer");
                }
            }
        }
        #endregion

        #region IsExcluded
        private bool _isExcluded;
        public bool IsExcluded
        {
            get { return _isExcluded; }
            set
            {
                if (_isExcluded != value)
                {
                    _isExcluded = value;
                    OnPropertyChanged("IsExcluded");
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
