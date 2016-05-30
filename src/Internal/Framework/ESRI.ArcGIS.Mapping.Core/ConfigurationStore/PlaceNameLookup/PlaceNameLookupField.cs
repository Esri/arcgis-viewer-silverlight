/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

namespace ESRI.ArcGIS.Mapping.Core
{
    using System.Runtime.Serialization;

    [DataContract]
    public class PlaceNameLookupField : LocationProviderField
    {
        [DataMember(Name = "Column", IsRequired = false, Order = 0)]
        public string Column { get; set; }

        [DataMember(Name = "DisplayName", IsRequired = false, Order = 1)]
        public string DisplayName { get; set; }

        public override string GetName()
        {
            return this.Column;
        }

        public override string GetDisplayName()
        {
            return this.DisplayName;
        }

        public override void SetDisplayName(string displayName)
        {
            this.DisplayName = displayName;
        }
    }
}
