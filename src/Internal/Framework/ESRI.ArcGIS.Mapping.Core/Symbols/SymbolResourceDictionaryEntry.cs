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
    public class SymbolResourceDictionaryEntry
    {
        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public GeometryType GeometryType { get; set; }

        [DataMember]
        public string DisplayName { get; set; }


        public override bool Equals(object obj)
        {
            SymbolResourceDictionaryEntry o1 = obj as SymbolResourceDictionaryEntry;
            if (o1 != null)
            {
                return o1.ID == ID &&
                        o1.GeometryType == GeometryType &&
                        o1.Path == Path &&
                        o1.DisplayName == DisplayName;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            int result = ID.GetHashCode() ^ GeometryType.GetHashCode();
            if (Path != null)
                result = result ^ Path.GetHashCode();
            if(DisplayName != null)
                result = result ^ DisplayName.GetHashCode();
            return result;
        }
    }
}
