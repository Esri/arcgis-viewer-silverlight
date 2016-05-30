/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ESRI.ArcGIS.Mapping.Builder.Common
{
    [Serializable]
    public class FileDescriptor
    {
        [XmlElement(IsNullable = true)]
        public string FileName { get; set; }

        [XmlElement(IsNullable = true)]
        public string DisplayName { get; set; }

        [XmlElement(IsNullable = true)]
        public string RelativePath { get; set; }

        [XmlElement]
        public bool IsFolder { get; set; }

        //Adding this property to set the Automation property for Coded UI Tests
        public override string ToString()
        {
            return DisplayName; 
        }
    }
}
