/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;

namespace ESRI.ArcGIS.Mapping.Core
{
    /// <summary>
    /// Adds configuration capabilities to a FeatureLayer
    /// </summary>
    [DataContract]
    public class ConfigurableFeatureLayer : FeatureLayer
            //, IConfigurableLayer, 
            //ISupportsFilter, 
            //ISupportsMapTipConfiguration, ISupportsClassification
    {
        public ConfigurableFeatureLayer(): base()
        {
            base.Graphics.CollectionChanged += new NotifyCollectionChangedEventHandler(Graphics_CollectionChanged);
        }        

        public Layer Layer
        {
            get { return this; }
        }        

        private GeometryType _geometryType;
        public GeometryType GeometryType
        {
            get
            {
                return _geometryType; 
            }
            set
            {
                if (_geometryType != value)
                {
                    _geometryType = value;
                    OnPropertyChanged("GeometryType");
                }
            }
        }

        public bool RetrieveMetaDataOnStartup { get; set; }

        public override void Initialize()
        {            
            // First - retrieve metadata for the service (if geometry is not known)
            if (RetrieveMetaDataOnStartup && Uri.IsWellFormedUriString(Url, System.UriKind.Absolute))
            {
                string metaDataUrl = Url;
                if (!metaDataUrl.Contains("f=json"))
                {
                    if (!metaDataUrl.Contains("?"))
                        metaDataUrl += "?f=json";
                    else
                        metaDataUrl += "&f=json";
                }

                WebClient wc = new WebClient();
                wc.DownloadStringCompleted += (o, e) =>
                {
                    if (e.Error != null || string.IsNullOrEmpty(e.Result))
                    {
                        System.Diagnostics.Debug.WriteLine(e.Error.Message);
                        System.Diagnostics.Debug.WriteLine(e.Error.StackTrace);
                        base.Initialize();
                        return;
                    }
                    parseLayerDetailsFromResponse(e.Result);
                };
                wc.DownloadStringAsync(new Uri(metaDataUrl, UriKind.Absolute));
            }
            else
            {
                base.Initialize();           
            }
        }

        void Graphics_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Graphic g in e.NewItems)
                {
                    addFieldDisplayInfoToGraphic(g);
                }
            }
        }

        private void addFieldDisplayInfoToGraphic(Graphic graphic)
        {
            // Add FieldInfo to the graphic's attributes
            foreach (FieldInfo field in Fields)
            {
                if (graphic.Attributes.ContainsKey(field.Name))
                    graphic.Attributes[field.Name] = new object[] { field, graphic.Attributes[field.Name] };
            }
        }

        private void parseLayerDetailsFromResponse(string jsonResponse)
        {
            using (MemoryStream memoryStream = new MemoryStream(Encoding.Unicode.GetBytes(jsonResponse)))
            {
                DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(LayerDetails));
                LayerDetails layerDetails = dataContractJsonSerializer.ReadObject(memoryStream) as LayerDetails;
                if (layerDetails != null)
                {
                    foreach (Field field in layerDetails.Fields)
                    {
                        FieldInfo fieldInfo = new FieldInfo()
                       {
                           DisplayName = field.Alias,
                           Name = field.Name,
                           FieldType = field.Type,
                           VisibleOnMapTip = true
                       };
                        _fields.Add(fieldInfo);
                        if (field.Type == "esriFieldTypeInteger"
                            || field.Type == "esriFieldTypeSmallInteger"
                            || field.Type == "esriFieldTypeDouble")
                        {
                            NumericFields.Add(fieldInfo);
                        }
                    }
                    switch (layerDetails.GeometryType)
                    {
                        case "esriGeometryPoint":
                            GeometryType = GeometryType.Point;
                            break;
                        case "esriGeometryPolyline":
                            GeometryType = GeometryType.Polyline;
                            break;
                        case "esriGeometryPolygon":
                            GeometryType = GeometryType.Polygon;
                            break;
                    }
                }
            }

            // Now that all Metadata has been retrieved .. call base.initialize
            base.Initialize();           
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

        #region ISupportsMapTipConfiguration Members
        private List<FieldInfo> _fields = new List<FieldInfo>();
        public List<FieldInfo> Fields
        {
            get
            {
                return _fields;
            }
            set
            {
                if (_fields != value)
                {
                    _fields = value;
                    OnPropertyChanged("Fields");
                }
            }
        }

        #endregion
    }
}
