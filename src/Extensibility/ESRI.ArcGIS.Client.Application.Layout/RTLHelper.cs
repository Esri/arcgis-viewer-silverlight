/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows;
using System.Threading;
using System.ComponentModel;

namespace ESRI.ArcGIS.Client.Application.Layout
{
    /// <summary>
    /// Singleton class that helps manage LTR/RTL culture settings
    /// </summary>
    public class RTLHelper : INotifyPropertyChanged
    {
        #region Static singleton instance support

        private static RTLHelper _instance = new RTLHelper();

        private void ensureSingleton()
        {
            if (_instance == null)
            {
                _instance = new RTLHelper();
                fireChangeOnAllProperties();
            }
        }

        private void fireChangeOnAllProperties()
        {
            OnPropertyChanged("FlowDirection");
        }

        #endregion

        #region Property: FlowDirection

        /// <summary>
        /// Sets or Gets the FlowDirection. If this property is not being set
        /// explicitly, call UpdateFlowDirection() to load it from culture settings
        /// before getting it.
        /// </summary>
        public FlowDirection FlowDirection
        {
            get
            {
                ensureSingleton();
                return _instance._flowDirection;
            }
            set
            {
                ensureSingleton();
                _instance._flowDirection = value;
                OnPropertyChanged("FlowDirection");
            }
        }

        private FlowDirection _flowDirection = FlowDirection.LeftToRight;

        #endregion

        /// <summary>
        /// Updates the FlowDirection property from culture settings
        /// </summary>
        public void UpdateFlowDirection()
        {
            ensureSingleton();
            _instance.FlowDirection = GetFlowDirectionFromCulture();
        }

        #region private methods

        /// <summary>
        /// Returns the FlowDirection (LTR/RTL) from the Culture settings
        /// </summary>
        private static FlowDirection GetFlowDirectionFromCulture()
        {
            string name = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToLower();
            return (name == "ar" || name == "he")
                       ? FlowDirection.RightToLeft
                       : FlowDirection.LeftToRight;
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property on the <see cref="RTLHelper"/> changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
