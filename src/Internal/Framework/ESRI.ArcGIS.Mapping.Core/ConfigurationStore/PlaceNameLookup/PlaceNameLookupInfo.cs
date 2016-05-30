/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

namespace ESRI.ArcGIS.Mapping.Core
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Linq;
    
    [DataContract]
    public class PlaceNameLookupInfo : LocationProviderInfo
    {
        [DataMember(Name = "DisplayName", Order = 0, IsRequired = false)]
        public string DisplayName { get; set; }

        [DataMember(Name = "DataFile", Order = 1, IsRequired = false)]
        public string DataFile { get; set; }

        [DataMember(Name = "Source", Order = 2, IsRequired = false)]
        public string Source { get; set; }

        [DataMember(Name = "GeometryType", Order = 3, IsRequired = false)]
        public string GeometryType { get; set; }

        [DataMember(Name = "Description", Order = 4, IsRequired = false)]
        public string Description { get; set; }

        [DataMember(Name = "PlaceNameLookupFields", Order = 6, IsRequired = false)]
        public List<PlaceNameLookupField> PlaceNameLookupFields { get; set; }

        public override IEnumerable<LocationProviderField> LocationFields
        {
            get
            {
                return this.PlaceNameLookupFields != null ? this.PlaceNameLookupFields.Select(p => (LocationProviderField)p) : null;
            }
        }
    }
}
