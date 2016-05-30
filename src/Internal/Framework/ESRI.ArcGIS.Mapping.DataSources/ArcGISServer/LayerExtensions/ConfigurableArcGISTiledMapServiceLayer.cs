/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Runtime.Serialization;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Core
{
    /// <summary>
    /// Adds Configuration capabilities to ArcGISTiledMapServiceLayer
    /// </summary>
    public class ConfigurableArcGISTiledMapServiceLayer : ArcGISTiledMapServiceLayer
        //, IConfigurableLayer
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

        //public GeometryType GeometryType
        //{
        //    get
        //    {
        //        return GeometryType.Unknown;
        //    }
        //}

        public Layer Layer
        {
            get { return this; }
        }
    }
}
