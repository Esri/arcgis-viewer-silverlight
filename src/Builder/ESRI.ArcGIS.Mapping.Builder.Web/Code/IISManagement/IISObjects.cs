/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Web;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    /// <summary>
    /// Contains information about a virtual directory in IIS
    /// </summary>
    [Serializable]    
    public class IIsVirtualDirectoryInfo
    {
        /// <summary>
        /// Virtual path of the virtual directory.
        /// </summary>
        [XmlElement]
        public string VirtualPath { get; set; }

        /// <summary>
        /// Name of the Virtual Directory (alias)
        /// </summary>
        [XmlElement]
        public string Name { get; set; }

        /// <summary>
        /// The Active Directory path to the virtual directory.
        /// </summary>
        [XmlElement]
        public string ADSIPath { get; set; }

        /// <summary>
        /// The UNC (or local) path to the virtual directory.
        /// </summary>
        [XmlElement]
        public string PhysicalPath { get; set; }
    }

    /// <summary>
    /// Holds information about websites
    /// </summary>
    [Serializable]    
    public class IIsWebsiteInfo
    {
        [XmlElement]
        public string WebsiteID { get; set; }

        private string _friendlyName = "Default Web Site";
        /// <summary>
        /// The friendly name for the website.
        /// </summary>
        [XmlElement]
        public string FriendlyName
        {
            get { return _friendlyName; }
            set { _friendlyName = value; }
        }

        /// <summary>
        /// List of port numbers to which the website is bound to. Each port value is of the form :portNum: (eg:- :80: :121:).
        /// </summary>
        [XmlElement]
        public List<string> Ports { get; set; }

        [XmlElement]
        public bool UsingSSL { get; set; }
    }
}



