/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ESRI.ArcGIS.Mapping.Builder.Common
{
    [XmlRoot("Extensions")]
    [Serializable]
    public class Extensions : List<Extension>
    {
        [XmlAttribute]
        public string BaseUrl { get; set; }
    }
    
    [Serializable]
    public class Extension
    {
        public Extension() {
            Assemblies = new List<Assembly>();
        }
        public Extension(IEnumerable<Assembly> assemblies) {
            Assemblies = new List<Assembly>(assemblies);
        }
        public Extension(IEnumerable<string> assemblyNames) {
            Assemblies = new List<Assembly>();
            if (assemblyNames != null)
            {
                foreach (string assemblyname in assemblyNames)
                    Assemblies.Add(new Assembly(assemblyname));
            }
        }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Url { get; set; }

        [XmlAttribute]
        public bool Required { get; set; }

        [XmlArrayItem(ElementName = "Assembly", Type = typeof(Assembly))]
        public List<Assembly> Assemblies { get; set; }
    }
    
    [Serializable]
    public class Assembly
    {
        public Assembly() { }
        public Assembly(string name) {
            Name = name;
        }

        [XmlAttribute]
        public string Name { get; set; }
    }
}
