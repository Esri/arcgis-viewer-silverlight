/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
    [DataContract]
    public class PortalUser
    {
        [DataMember(Name = "username", IsRequired = false)]
        public string UserName { get; set; }

        [DataMember(Name = "fullname", IsRequired = false)]
        public string FullName { get; set; }

        [DataMember(Name = "role", IsRequired = false)]
        public string Role { get; set; }

        [DataMember(Name = "storageUsage", IsRequired = false)]
        public long StorageUsage { get; set; }

        [DataMember(Name = "storageQuota", IsRequired = false)]
        public long StorageQuota { get; set; }

        [DataMember(Name = "created", IsRequired = false)]
        public long Created { get; set; }

        [DataMember(Name = "modified", IsRequired = false)]
        public long Modified { get; set; }
    }
}
