/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
using System.Windows.Data;

namespace ESRI.ArcGIS.Mapping.Core
{
    public sealed class ViewModel: INotifyPropertyChanged
    {
        private bool _showSidePanel = true;
        public bool ShowSidePanel
        {
            get
            {
                return _showSidePanel;
            }
            set
            {
                if (_showSidePanel != value)
                {
                    _showSidePanel = value;
                    OnPropertyChanged("ShowSidePanel");
                }
            }
        }

        private bool _showBottomPanel = true;
        public bool ShowBottomPanel
        {
            get
            {
                return _showBottomPanel;
            }
            set
            {
                if (_showBottomPanel != value)
                {
                    _showBottomPanel = value;
                    OnPropertyChanged("ShowBottomPanel");
                }
            }
        }

        //private bool _supportsFullScreenModeToggle = false;
        //public bool SupportsFullScreenModeToggle
        //{
        //    get { return _supportsFullScreenModeToggle; }
        //    set
        //    {
        //        if (_supportsFullScreenModeToggle != value)
        //        {
        //            _supportsFullScreenModeToggle = value;
        //            OnPropertyChanged("SupportsFullScreenModeToggle");
        //        }
        //    }
        //}        

        //private bool _isEditMode;
        //public bool IsEditMode
        //{
        //    get { return _isEditMode; }
        //    set
        //    {
        //        if (_isEditMode != value)
        //        {
        //            _isEditMode = value;
        //            OnPropertyChanged("IsEditMode");
        //        }
        //    }
        //}

        //private bool _allowUserToChangeBaseMapInRunMode = true;
        //public bool AllowUserToChangeBaseMapInRunMode
        //{
        //    get
        //    {
        //        return _allowUserToChangeBaseMapInRunMode;
        //    }
        //    set
        //    {
        //        _allowUserToChangeBaseMapInRunMode = value;
        //    }
        //}

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }    
}
