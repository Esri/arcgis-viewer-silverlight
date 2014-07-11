/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.ComponentModel;
using System.Windows;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;

namespace SearchTool
{
    /// <summary>
    /// Represents a result from an ArcGIS Locator service
    /// </summary>
    public class LocatorResultViewModel : DependencyObject, INotifyPropertyChanged
    {
        public LocatorResultViewModel(AddressCandidate candidate)
        {
            Candidate = candidate;
        }

        /// <summary>
        /// The candidate returned by the locator search
        /// </summary>
        public AddressCandidate Candidate { get; internal set; }

        private Envelope _extent;
        /// <summary>
        /// The extent for viewing the result
        /// </summary>
        public Envelope Extent 
        {
            get { return _extent; }
            set
            {
                if (_extent != value)
                {
                    _extent = value;
                    OnPropertyChanged("Extent");
                }
            }
        }

        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised when a property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
