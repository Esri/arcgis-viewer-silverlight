/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Runtime.Serialization;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer
{
    /// <summary>
    /// Adds Configuration capabilities to ArcGISDynamicMapServiceLayer
    /// </summary>
    [DataContract]
    public class ConfigurableArcGISDynamicMapServiceLayer : DynamicMapServiceLayer, IConfigurableLayer
    {
        private string _displayName;
        [DataMember]
        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    OnPropertyChanged("DisplayName");
                }
            }
        }

        private bool _visibleInLayerList;
        public bool VisibleInLayerList
        {
            get { return _visibleInLayerList; }
            set 
            {
                if (_visibleInLayerList != value)
                {
                    _visibleInLayerList = value;
                    OnPropertyChanged("VisibleInLayerList");
                }
            }
        }
        
        public string Url
        {
            get { return ArcGISDynamicMapServiceLayer != null ? ArcGISDynamicMapServiceLayer.Url : null; }            
        }

        public Client.Layer Layer
        {
            get { return ArcGISDynamicMapServiceLayer; }
        }

        public GeometryType GeometryType
        {
            get
            {
                return GeometryType.Unknown;
            }
        }

        public ArcGISDynamicMapServiceLayer ArcGISDynamicMapServiceLayer { get; set; }

        bool _initialized = false;
        public override void GetUrl(Client.Geometry.Envelope extent, int width, int height, DynamicMapServiceLayer.OnUrlComplete onComplete)
        {
            if (ArcGISDynamicMapServiceLayer != null)
            {                
                if (!_initialized)
                {
                    ArcGISDynamicMapServiceLayer.Initialize();
                    _initialized = true;
                }
                ArcGISDynamicMapServiceLayer.GetUrl(extent, width, height, onComplete);
            }
        }
    }
}
