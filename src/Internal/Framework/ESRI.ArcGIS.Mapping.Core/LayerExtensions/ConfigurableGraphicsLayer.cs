/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Core
{
    /// <summary>
    /// Adds configuration capabilities to a GraphicsLayer
    /// </summary>
    public class ConfigurableGraphicsLayer : GraphicsLayer, IConfigurableLayer, ISupportsClassification
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

        private GeometryType? geomType;
        public GeometryType GeometryType
        {
            get
            {
                if(geomType == null)
                    return GeometryType.Unknown;
                return (GeometryType)geomType;
            }
            set
            {
                geomType = value;
            }
        }

        public Layer Layer
        {
            get { return this; }
        }

        #region ISupportsClassification
        private List<FieldInfo> _numericFields = new List<FieldInfo>();
        public List<FieldInfo> NumericFields
        {
            get
            {
                return _numericFields;
            }
            set
            {
                if (_numericFields != value)
                {
                    _numericFields = value;
                    OnPropertyChanged("NumericFields");
                }
            }
        }
        #endregion
    }
}
