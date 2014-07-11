/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

namespace ESRI.ArcGIS.Mapping.Core
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    using ESRI.ArcGIS.Client.Geometry;

    [DataContract]
    public abstract class LocationProviderInfo : INotifyPropertyChanged
    {

        public abstract IEnumerable<LocationProviderField> LocationFields { get; }

        #region INotifyPropertyChanged Members

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
