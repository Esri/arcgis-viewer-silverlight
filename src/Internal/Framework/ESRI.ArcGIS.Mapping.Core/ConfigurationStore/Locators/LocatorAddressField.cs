/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Mapping.Core
{
    [DataContract]
    public class LocatorAddressField : LocationProviderField
    {
        public LocatorAddressField() { }
        public LocatorAddressField(LocatorAddressField addressField)
        {
            FromLocatorAddressField(addressField);
        }

        [DataMember(Name = "name", IsRequired = false, Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "alias", IsRequired = false, Order = 1)]
        public string Alias { get; set; }

        [DataMember(Name = "type", IsRequired = false, Order = 2)]
        public string Type { get; set; }

        [DataMember(Name = "required", IsRequired = false, Order = 3)]
        public bool Required { get; set; }

        public override string GetName()
        {
            return this.Name;
        }

        public override string GetDisplayName()
        {
            return this.Alias;
        }

        public override void SetDisplayName(string displayName)
        {
            this.Alias = displayName;
        }

        internal LocatorAddressField Clone()
        {
            return new LocatorAddressField
            {
                Alias = this.Alias,
                Name = this.Name,
                Required = this.Required,
                Type = this.Type
            };
        }

        internal void FromLocatorAddressField(LocatorAddressField addressField)
        {
            if (addressField == null)
                return;
            this.Alias = addressField.Alias;
            this.Name = addressField.Name;
            this.Required = addressField.Required;
            this.Type = addressField.Type;
        }
    }
}
