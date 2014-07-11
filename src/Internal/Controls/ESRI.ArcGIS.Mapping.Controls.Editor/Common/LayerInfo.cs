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
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls.Editor
{
    public class LayerInfo : INotifyPropertyChanged
    {
        #region Layer
        private FeatureLayer _layer;
        public FeatureLayer Layer
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

        #region IsChecked
        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    if (value && Layer.Clusterer != null)
                    {
                        MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Controls.Editor.Resources.Strings.DisableClusteringPrompt,
                            ESRI.ArcGIS.Mapping.Controls.Editor.Resources.Strings.DisableClustering, MessageBoxButton.OKCancel,
                            (obj, args1) =>
                            {
                                if (args1.Result == MessageBoxResult.OK)
                                {
                                    Layer.Clusterer = null;
                                    _isChecked = value;
                                }
                                OnPropertyChanged("IsChecked");
                            });
                    }
                    else
                    {
                        _isChecked = value;
                        OnPropertyChanged("IsChecked");
                    }
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
