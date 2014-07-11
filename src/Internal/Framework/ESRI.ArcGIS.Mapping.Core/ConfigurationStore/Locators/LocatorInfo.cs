/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcGIS.Mapping.Core
{
    [DataContract]
    public class LocatorInfo : LocationProviderInfo
    {
        public LocatorInfo() { }
        public LocatorInfo(LocatorInfo locatorInfo)
        {
            FromLocatorInfo(locatorInfo);
        }

        [DataMember(Name = "DisplayName", Order = 0, IsRequired = false)]
        public string DisplayName { get; set; }

        [DataMember(Name = "Name", Order = 1, IsRequired = false)]
        public string Name { get; set; }

        [DataMember(Name = "LocatorType", Order = 2, IsRequired = false)]
        public LocatorType LocatorType { get; set; }

        [DataMember(Name="Url", Order=3, IsRequired=false)]
        public string Url { get; set; }

        [DataMember(Name = "Description", Order = 4, IsRequired = false)]
        public string Description { get; set; }

        [DataMember(Name = "IsDefault", Order = 5, IsRequired = false)]
        public bool IsDefault { get; set; }

        [DataMember(Name = "SpatialReference", Order = 6, IsRequired = false)]
        public SpatialReference SpatialReference { get; set; }
        private List<LocatorAddressField> _locatorAddressFields;
        [DataMember(Name = "LocatorAddressFields", Order = 5, IsRequired = false)]
        public List<LocatorAddressField> LocatorAddressFields
        {
            get
            {
                return _locatorAddressFields;
            }
            set
            {
                if (_locatorAddressFields != value)
                {
                    _locatorAddressFields = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("LocatorAddressFields"));
                }
            }
        }

        public string GeometryType { get { return "Point"; } }

        public override IEnumerable<LocationProviderField> LocationFields
        {
            get
            {
                return this.LocatorAddressFields != null ? this.LocatorAddressFields.Select(p => (LocationProviderField)p) : null;
            }
        }

        internal LocatorInfo Clone()
        {
            LocatorInfo l = new LocatorInfo
            {
                Name = this.Name,
                Url = this.Url,
                DisplayName = this.DisplayName,
                IsDefault = this.IsDefault,
                LocatorType = this.LocatorType,
                Description = this.Description,
                SpatialReference = this.SpatialReference,
            };
            if (this.LocatorAddressFields != null)
            {
                l.LocatorAddressFields = new List<LocatorAddressField>(this.LocatorAddressFields.Count);
                foreach (LocatorAddressField addressField in this.LocatorAddressFields)
                {
                    l.LocatorAddressFields.Add(addressField.Clone());
                }
            }
            return l;
        }

        internal void FromLocatorInfo(LocatorInfo l)
        {
            if (l == null)
                return;
            this.Name = l.Name;
            this.DisplayName = l.DisplayName;
            this.IsDefault = l.IsDefault;
            this.Url = l.Url;
            this.LocatorType = l.LocatorType;
            this.Description = l.Description;
            this.SpatialReference = l.SpatialReference;
            if (l.LocatorAddressFields != null)
            {
                this.LocatorAddressFields = new List<LocatorAddressField>(l.LocatorAddressFields.Count);
                foreach (LocatorAddressField addressField in l.LocatorAddressFields)
                {
                    this.LocatorAddressFields.Add(new LocatorAddressField(addressField));
                }
            }
            else
            {
                this.LocatorAddressFields = null;
            }
        }
    }

    [DataContract]
    public enum LocatorType
    {
        [EnumMember]
        ArcGISServer,
        //[EnumMember]
        //BingMaps
    }
}
