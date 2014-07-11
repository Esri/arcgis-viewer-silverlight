/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace ESRI.ArcGIS.Mapping.Builder.Common
{
    [Serializable]
    public class Site
    {
        public Site()
        {
            id = Guid.NewGuid().ToString();
        }

        string id = null;
        [XmlElement(IsNullable = true)]
        public string ID
        {
            get
            {
                if (string.IsNullOrEmpty(id))
                    id = Guid.NewGuid().ToString();
                return id;
            }
            set
            {
                id = value;
            }
        }

        [XmlElement]
        public string Name { get; set; }

        [XmlElement(IsNullable = true)]
        public string Url { get; set; }

        [XmlElement(IsNullable = true)]
        public string PhysicalPath { get; set; }

        [XmlElement]
        public bool IsHostedOnIIS { get; set; }

        [XmlElement(IsNullable = true)]
        public string IISHost { get; set; }

        int iisport = 80;
        [XmlElement]
        public int IISPort
        {
            get { return iisport; }
            set { iisport = value; }
        }

        [XmlElement(IsNullable = true)]
        public string IISPath { get; set; }

        [XmlElement(IsNullable = true)]
        public string Description { get; set; }

        [XmlElement(IsNullable = true)]
        public string Title { get; set; }

        /// <summary>
        /// The version of the product with which the site is compatible
        /// </summary>
        [XmlElement(IsNullable = true)]        
        public string ProductVersion { get; set; }
    }

    [XmlRoot("Sites")]
    [Serializable]
    public class Sites : List<Site>
    {
    }
}
