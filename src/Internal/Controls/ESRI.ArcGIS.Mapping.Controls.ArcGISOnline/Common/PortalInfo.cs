/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
    /// <summary>
    /// Contains information about the ArcGIS Portal instance
    /// </summary>
    [DataContract]
    public class PortalInfo : INotifyPropertyChanged
    {
        private string _name = null;
        [DataMember(Name = "name")]
        public string Name 
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        [DataMember(Name = "description", IsRequired = false)]
        public string Description { get; set; }

        private string _portalName = null;
        [DataMember(Name = "portalName", IsRequired = false)]
        public string PortalName 
        {
            get { return _portalName; }
            set
            {
                if (_portalName != value)
                {
                    _portalName = value;
                    OnPropertyChanged("PortalName");
                }
            }
        }

        [DataMember(Name = "thumbnail", IsRequired = false)]
        public string Thumbnail { get; set; }

        [DataMember(Name = "portalThumbnail", IsRequired = false)]
        public string PortalThumbnail { get; set; }

        [DataMember(Name = "featuredItemsGroupQuery", IsRequired = false)]
        public string FeaturedItemsGroupQuery { get; set; }

        [DataMember(Name = "homePageFeaturedContentCount", IsRequired = false)]
        public int HomePageFeaturedContentCount { get; set; }

        [DataMember(Name = "homePageFeaturedContent", IsRequired = false)]
        public string HomePageFeaturedContent { get; set; }

        [DataMember(Name = "basemapGalleryGroupQuery", IsRequired = false)]
        public string BasemapGalleryGroupQuery { get; set; }

        [DataMember(Name = "templatesGroupQuery", IsRequired = false)]
        public string TemplatesGroupQuery { get; set; }

        [DataMember(Name = "layerTemplatesGroupQuery", IsRequired = false)]
        public string LayerTemplatesGroupQuery { get; set; }

        [DataMember(Name = "symbolSetsGroupQuery", IsRequired = false)]
        public string SymbolSetsGroupQuery { get; set; }

        [DataMember(Name = "colorSetsGroupQuery", IsRequired = false)]
        public string ColorSetsGroupQuery { get; set; }

        [DataMember(Name = "featuredGroups", IsRequired = false)]
        public List<Group> FeaturedGroups { get; set; }

        [DataMember(Name = "defaultBasemap", IsRequired = false)]
        public BasemapInfo DefaultBasemap { get; set; }

        [DataMember(Name = "rotatorPanels", IsRequired = false)]
        public List<string> RotatorPanels { get; set; }

        [DataMember(Name = "portalMode", IsRequired = false)]
        public string PortalMode { get; set; }

        [DataMember(Name = "created", IsRequired = false)]
        public double Created { get; set; }

        [DataMember(Name = "modified", IsRequired = false)]
        public double Modified { get; set; }

        [DataMember(Name = "canSearchPublic", IsRequired = false)]
        public bool CanSearchPublic { get; set; }

        [DataMember(Name = "CanSharePublic", IsRequired = false)]
        public bool CanSharePublic { get; set; }

        [DataMember(Name = "databaseUsage", IsRequired = false)]
        public double DatabaseUsage { get; set; }

        [DataMember(Name = "databaseQuota", IsRequired = false)]
        public double DatabaseQuota { get; set; }

        [DataMember(Name = "featureGroupsId", IsRequired = false)]
        public string FeatureGroupsId { get; set; }

        [DataMember(Name = "id", IsRequired = false)]
        public string ID { get; set; }

        [DataMember(Name = "showHomePageDescription", IsRequired = false)]
        public bool ShowHomePageDescription { get; set; }

        [DataMember(Name = "storageQuota", IsRequired = false)]
        public double StorageQuota { get; set; }

        [DataMember(Name = "storageUsage", IsRequired = false)]
        public double StorageUsage { get; set; }

        [DataMember(Name = "urlHostname", IsRequired = false)]
        public string UrlHostname { get; set; }

        [DataMember(Name = "urlKey", IsRequired = false)]
        public string UrlKey { get; set; }

        [DataMember(Name = "user", IsRequired = false)]
        public PortalUser User { get; set; }

        [DataMember(Name = "supportsOAuth", IsRequired = false)]
        public bool SupportsOAuth { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
