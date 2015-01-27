/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Runtime.Serialization;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Core
{
    [DataContract]
    public class ConfigurationStore : INotifyPropertyChanged
    {
        [DataMember(Name="BaseMaps", Order=0, IsRequired=false)]
        public ObservableCollection<BaseMapInfo> BaseMaps { get; set; }

        [DataMember(Name = "Locators", Order = 1, IsRequired = false)]
        public List<LocatorInfo> Locators { get; set; }

        [DataMember(Name = "GeometryServices", Order = 2, IsRequired = false)]
        public List<GeometryServiceInfo> GeometryServices { get; set; }

        string bingMapsAppId;
        [DataMember(Name = "BingMapsAppId", Order = 3, IsRequired = false)]
        public string BingMapsAppId
        {
            get { return bingMapsAppId; }
            set
            {
                bingMapsAppId = value;
                raisePropertyChange("BingMapsAppId");
            }
        }

        string portalAppId;
        [DataMember(Name = "PortalAppId", Order = 4, IsRequired = false)]
        public string PortalAppId
        {
            get { return portalAppId; }
            set
            {
                portalAppId = value;
                raisePropertyChange("PortalAppId");
            }
        }

        [DataMember(Name = "PlaceNameLookups", Order = 5, IsRequired = false)]
        public List<PlaceNameLookupInfo> PlaceNameLookups { get; set; }

        public static ConfigurationStore Current { get; set; }

        void raisePropertyChange(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
