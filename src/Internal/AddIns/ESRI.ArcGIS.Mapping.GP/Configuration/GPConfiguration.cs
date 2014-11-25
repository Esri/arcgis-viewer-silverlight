/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Utils;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.GP.ParameterSupport;

namespace ESRI.ArcGIS.Mapping.GP
{
    public class GPConfiguration : INotifyPropertyChanged
    {
        public List<ParameterSupport.ParameterConfig> InputParameters { get; set; }
        public List<ParameterSupport.ParameterConfig> OutputParameters { get; set; }
        public GPConfiguration() { }

        private ObservableCollection<string> layerOrder;
        /// <summary>
        /// Gets or sets the display order of the GP task's layers
        /// </summary>
        public ObservableCollection<string> LayerOrder 
        {
            get { return layerOrder; } 
            set
            {
                if (layerOrder!= value)
                {
                    layerOrder = value;
                    OnPropertyChanged("LayerOrder");
                }
            } 
        }
        private string title;
        /// <summary>
        /// Gets or sets the configured title of the current GP task
        /// </summary>
        public string Title 
        {
            get { return title; }
            set
            {
                if (title != value)
                {
                    title = value;
                    OnPropertyChanged("Title");
                }
            }
        }
        private Uri taskEndPoint;
        /// <summary>
        /// Gets the current GP task's URL
        /// </summary>
        public Uri TaskEndPoint 
        {
            get { return taskEndPoint; }
            private set
            {
                if (taskEndPoint != value)
                {
                    taskEndPoint = value;
                    OnPropertyChanged("TaskEndPoint");
                }
            }
        }
        private Uri helpUrl;
        /// <summary>
        /// Gets the current GP task's help URL
        /// </summary>
        public Uri HelpUrl 
        {
            get { return helpUrl; }
            private set 
            {
                if (helpUrl != value)
                {
                    helpUrl = value;
                    OnPropertyChanged("HelpUrl");
                }
            } 
        }

        private string taskName;
        /// <summary>
        /// Gets the name of the current GP Task
        /// </summary>
        public string TaskName
        {
            get { return taskName; }
            private set
            {
                if (taskName != value)
                {
                    taskName = value;
                    OnPropertyChanged("TaskName");
                }
            }
        }

        private bool useProxy;
        /// <summary>
        /// Gets or sets whether to use a proxy for accessing the GP service
        /// </summary>
        public bool UseProxy
        {
            get { return useProxy; }
            set
            {
                if (useProxy != value)
                {
                    useProxy = value;
                    OnPropertyChanged("UseProxy");
                }
            }
        }

        private string proxyUrl;
        /// <summary>
        /// Gets or sets the URL of the proxy to use when accessing the GP service
        /// </summary>
        public string ProxyUrl
        {
            get { return proxyUrl; }
            set
            {
                if (proxyUrl != value)
                {
                    proxyUrl = value;
                    OnPropertyChanged("ProxyUrl");
                }
            }
        }

        public GPConfiguration(string serialized)
        {
            if (!string.IsNullOrWhiteSpace(serialized))
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();
                IDictionary<string, object> dictionary = jss.DeserializeObject(serialized) as IDictionary<string, object>;
                InputParameters = new List<ParameterSupport.ParameterConfig>();
                OutputParameters = new List<ParameterSupport.ParameterConfig>();
                if (dictionary.ContainsKey("inputParameters"))
                {
                    IEnumerable parameters = dictionary["inputParameters"] as IEnumerable;
                    if (parameters != null)
                    {
                        foreach (Dictionary<string, object> item in parameters)
                        {
                            if (item != null)
                                InputParameters.Add(ParameterSupport.ParameterConfig.Create(item));
                        }
                    }
                }
                if (dictionary.ContainsKey("outputParameters"))
                {
                    IEnumerable parameters = dictionary["outputParameters"] as IEnumerable;
                    if (parameters != null)
                    {
                        foreach (Dictionary<string, object> item in parameters)
                        {
                            if (item != null)
                                OutputParameters.Add(ParameterSupport.ParameterConfig.Create(item));
                        }
                    }
                }

                if (dictionary.ContainsKey("layerOrder"))
                {
                    IEnumerable layers = dictionary["layerOrder"] as IEnumerable;
                    LayerOrder = new ObservableCollection<string>();
                    if (layers != null)
                    {
                        foreach (object item in layers)
                        {
                            if (item is string)
                                LayerOrder.Add(item as string);
                        }
                    }
                }

                if (dictionary.ContainsKey("taskEndPoint"))
                {
                    string url = dictionary["taskEndPoint"] as string;
                    if (!string.IsNullOrEmpty(url))
                        TaskEndPoint = new Uri(url);
                }
                if (dictionary.ContainsKey("title"))
                    Title = dictionary["title"] as string;
                if (dictionary.ContainsKey("taskName"))
                    TaskName = dictionary["taskName"] as string;
                if (dictionary.ContainsKey("helpUrl"))
                {
                    string url = dictionary["helpUrl"] as string;
                    if (!string.IsNullOrEmpty(url))
                        HelpUrl = new Uri(url);
                }

                if (dictionary.ContainsKey("useProxy"))
                {
                    string useProxyString = dictionary["useProxy"] as string;
                    if (!string.IsNullOrEmpty(useProxyString))
                    {
                        bool useProxy;
                        bool.TryParse(useProxyString, out useProxy);
                        UseProxy = useProxy;
                    }
                }
            }
        }

        public string ToJson()
        {
            StringBuilder sb = new StringBuilder();
            JsonWriter jw = new JsonWriter(new StringWriter(sb));
            jw.StartObject();
            jw.WriteProperty("taskEndPoint", TaskEndPoint.ToString());
            jw.Writer.Write(",");
            if (!string.IsNullOrEmpty(Title))
            {
                jw.WriteProperty("title", Title.ToString());
                jw.Writer.Write(",");
            }
            if (!string.IsNullOrEmpty(TaskName))
            {
                jw.WriteProperty("taskName", TaskName.ToString());
                jw.Writer.Write(",");
            }
            if (HelpUrl != null)
            {
                jw.WriteProperty("helpUrl", HelpUrl.ToString());
                jw.Writer.Write(",");
            }
            
            jw.WriteProperty("useProxy", UseProxy.ToString());
            jw.Writer.Write(",");

            jw.StartProperty("inputParameters");
            jw.StartArray();
            bool wroteFirst = false;
            foreach (ParameterSupport.ParameterConfig config in InputParameters)
            {
                if (config == null)
                    continue;
                if (wroteFirst)
                    jw.Writer.Write(",");
                else
                    wroteFirst = true;
                config.ToJson(jw);
            }
            jw.EndArray();
            jw.Writer.Write(",");
            jw.StartProperty("outputParameters");
            jw.StartArray();
            wroteFirst = false;
            foreach (ParameterSupport.ParameterConfig config in OutputParameters)
            {
                if (config == null)
                    continue;
                if (wroteFirst)
                    jw.Writer.Write(",");
                else
                    wroteFirst = true;
                config.ToJson(jw);
            }
            jw.EndArray();

            if (LayerOrder != null)
            {
                jw.Writer.Write(",");
                jw.StartProperty("layerOrder");
                jw.StartArray();
                wroteFirst = false;
                foreach (string layer in LayerOrder)
                {
                    if (string.IsNullOrEmpty(layer))
                        continue;
                    if (wroteFirst)
                        jw.Writer.Write(",");
                    else
                        wroteFirst = true;
                    jw.Writer.Write(string.Format("\"{0}\"", layer));
                }
                jw.EndArray();
            }
            jw.EndObject();
            return sb.ToString();
        }

        public GPConfiguration Clone()
        {
            return new GPConfiguration(this.ToJson());
        }
        internal static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public void LoadConfiguration(GP.MetaData.GPMetaData serviceInfo, Uri taskEndPoint)
        {
            if (serviceInfo == null)
                return;

            TaskEndPoint = taskEndPoint;
            TaskName = Title = serviceInfo.DisplayName;
            HelpUrl = serviceInfo.HelpUrl;
            if (InputParameters == null)
                InputParameters = new List<ParameterSupport.ParameterConfig>();
            else
                InputParameters.Clear();

            if (OutputParameters == null)
                OutputParameters = new List<ParameterSupport.ParameterConfig>();
            else
                OutputParameters.Clear();

            if (LayerOrder == null)
                LayerOrder = new ObservableCollection<string>();
            else
                LayerOrder.Clear();
            
            Collection<LayerInformation> resultMapserviceLayerInfos = string.IsNullOrEmpty(serviceInfo.ResultMapServerName) || string.IsNullOrEmpty(serviceInfo.CurrentVersion) ? null : new Collection<LayerInformation>();

            #region Get parameter configs
            if (serviceInfo.Parameters != null)
            {
                int layerId = 0;
                foreach (ESRI.ArcGIS.Mapping.GP.MetaData.GPParameter param in serviceInfo.Parameters)
                {
                    ParameterConfig config = null;
                    if (param.DataType == "GPFeatureRecordSetLayer")
                    {
                        if (!string.IsNullOrEmpty(serviceInfo.ResultMapServerName) && param.Direction != "esriGPParameterDirectionInput")
                        {
                            if (!string.IsNullOrEmpty(serviceInfo.CurrentVersion)) // A resultmapservice can only be accessed at http://.../<resultMapservice>/MapServer/jobs/<jobId> when server version can be determined.
                                resultMapserviceLayerInfos.Add(ToLayerInfo(param, layerId++));
                            else
                            {
                                MapServiceLayerParameterConfig layerConfig = new MapServiceLayerParameterConfig
                                                                             {
                                                                                 Name = param.Name,
                                                                                 LayerName = param.DisplayName,
                                                                                 Type = GPParameterType.MapServiceLayer,
                                                                                 SupportsJobResource = false,
                                                                                 Opacity = 1,
                                                                             };
                                OutputParameters.Add(layerConfig);
                                LayerOrder.Add(layerConfig.Name);
                            }
                        }
                        else
                        {
                            #region No result GP mapserver
                            GP.ParameterSupport.FeatureLayerParameterConfig layerConfig = new ParameterSupport.FeatureLayerParameterConfig() { ShownAtRunTime = true };
                            layerConfig.Name = param.Name;
                            layerConfig.Label = layerConfig.DisplayName = string.IsNullOrEmpty(param.DisplayName) ? param.Name : param.DisplayName;
                            layerConfig.Mode = ParameterSupport.FeatureLayerParameterConfig.InputMode.SketchLayer;
                            layerConfig.Type = GPParameterType.FeatureLayer;
                            layerConfig.Required = param.ParameterType == "esriGPParameterTypeRequired";
                            ESRI.ArcGIS.Mapping.GP.MetaData.GPFeatureRecordSetLayer frs = param.DefaultValue as ESRI.ArcGIS.Mapping.GP.MetaData.GPFeatureRecordSetLayer;
                            if (frs != null)
                            {
                                if (frs.GeometryType == "esriGeometryPolyline")
                                {
                                    layerConfig.GeometryType = ESRI.ArcGIS.Mapping.Core.GeometryType.Polyline;
                                    layerConfig.HelpText = Resources.Strings.DrawLine;
                                }
                                else if (frs.GeometryType == "esriGeometryPolygon")
                                {
                                    layerConfig.GeometryType = ESRI.ArcGIS.Mapping.Core.GeometryType.Polygon;
                                    layerConfig.HelpText = Resources.Strings.DrawPolygon;
                                }
                                else if (frs.GeometryType == "esriGeometryPoint")
                                {
                                    layerConfig.GeometryType = ESRI.ArcGIS.Mapping.Core.GeometryType.Point;
                                    layerConfig.HelpText = Resources.Strings.DrawPoint;
                                }
                                else if (frs.GeometryType == "esriGeometryMultipoint")
                                {
                                    layerConfig.GeometryType = ESRI.ArcGIS.Mapping.Core.GeometryType.MultiPoint;
                                    layerConfig.HelpText = Resources.Strings.DrawPoint;
                                }
                                #region Layer with field info, geometry type and renderer
                                GraphicsLayer layer = new GraphicsLayer();
                                if (frs.Fields != null && frs.Fields.Length > 0)
                                {
                                    Collection<ESRI.ArcGIS.Mapping.Core.FieldInfo> fields = new Collection<ESRI.ArcGIS.Mapping.Core.FieldInfo>();

                                    List<string> doubleFields = new List<string>();
                                    List<string> singleFields = new List<string>();
                                    foreach (MetaData.Field field in frs.Fields)
                                    {
                                        #region Get Single and Double Fields
                                        string type = field.Type;
                                        if (type.StartsWith(GPConfiguration.esriFieldType, StringComparison.Ordinal))
                                        {
                                            type = type.Substring(GPConfiguration.esriFieldType.Length);
                                            ESRI.ArcGIS.Client.Field.FieldType fieldType =
                                                (ESRI.ArcGIS.Client.Field.FieldType)Enum.Parse(typeof(ESRI.ArcGIS.Client.Field.FieldType), type, true);

                                            if (fieldType == ESRI.ArcGIS.Client.Field.FieldType.Double)
                                                doubleFields.Add(field.Name);
                                            else if (fieldType == Client.Field.FieldType.Single)
                                                singleFields.Add(field.Name);
                                        }
                                        #endregion

                                        #region Get FieldInfos
                                        if (field.Type == "esriGeometry")
                                            continue;

                                        fields.Add(new ESRI.ArcGIS.Mapping.Core.FieldInfo()
                                        {
                                            DisplayName = field.Alias,
                                            FieldType = mapFieldType(field.Type),
                                            Name = field.Name,
                                            VisibleInAttributeDisplay = true,
                                            VisibleOnMapTip = true,
                                        });
                                        #endregion
                                    }

                                    ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetFields(layer, fields);
                                    layerConfig.SingleFields = singleFields.ToArray();
                                    layerConfig.DoubleFields = doubleFields.ToArray();
                                }
                                ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetGeometryType(layer, layerConfig.GeometryType);
                                layer.Renderer = FeatureLayerParameterConfig.GetSimpleRenderer(layerConfig.GeometryType);

                                // Disable pop-ups by default for input layers
                                if (param.Direction == "esriGPParameterDirectionInput")
                                    LayerProperties.SetIsPopupEnabled(layer, false);
                                layerConfig.Layer = layer;
                                #endregion
                            }
                            else
                            {
                                layerConfig.GeometryType = ESRI.ArcGIS.Mapping.Core.GeometryType.Unknown;
                                layerConfig.HelpText = Resources.Strings.UnknownGeometryType;
                            }
                            layerConfig.LayerName = layerConfig.Label;
                            layerConfig.ToolTip = layerConfig.HelpText;
                            layerConfig.Opacity = 1;

                            config = layerConfig;
                            LayerOrder.Add(layerConfig.Name);
                            #endregion
                        }
                    }
                    else if (param.DataType == "GPRasterDataLayer" || param.DataType == "GPRasterData")
                    {
                        if (string.IsNullOrEmpty(serviceInfo.ResultMapServerName) || param.Direction == "esriGPParameterDirectionInput")
                        {
                            config = new RasterDataParameterConfig()
                            {
                                Name = param.Name,
                                ShownAtRunTime = true,
                                FormatToolTip = "e.g. tif, jpg",
                                Type = param.DataType == "GPRasterDataLayer" ? GPParameterType.RasterDataLayer : GPParameterType.RasterData,
                                ToolTip = param.DataType == "GPRasterDataLayer" ? Resources.Strings.EnterUrlForRasterDataLayer : Resources.Strings.EnterUrlForRasterData,
                                HelpText = param.DataType == "GPRasterDataLayer" ? Resources.Strings.EnterUrlForRasterDataLayer : Resources.Strings.EnterUrlForRasterData,
                                Label = param.DisplayName,
                                DisplayName = param.DisplayName,
                                Required = param.ParameterType == "esriGPParameterTypeRequired",
                                Input = param.Direction == "esriGPParameterDirectionInput"
                            };
                        }
                        else if (string.IsNullOrEmpty(serviceInfo.CurrentVersion)) 
                        {
                            MapServiceLayerParameterConfig layerConfig = new MapServiceLayerParameterConfig
                            {
                                Name = param.Name,
                                LayerName = param.DisplayName,
                                Type = GPParameterType.MapServiceLayer,
                                SupportsJobResource = false,
                                Opacity = 1,
                            };
                            OutputParameters.Add(layerConfig);
                            LayerOrder.Add(layerConfig.Name);
                        }
                        else
                            resultMapserviceLayerInfos.Add(ToLayerInfo(param, layerId++));
                    }
                    else
                    {
                        #region other param types
                        if (param.DataType == "GPMultiValue:GPString")
                            config = new MultiValueStringConfig() { ShownAtRunTime = true };
                        else
                            config = new ParameterConfig() { ShownAtRunTime = true };

                        config.Name = param.Name;
                        config.Label = config.DisplayName = param.DisplayName;
                        config.Required = param.ParameterType == "esriGPParameterTypeRequired";

                        string defaultString = param.DefaultValue == null ? null : param.DefaultValue.ToString();

                        switch (param.DataType)
                        {
                            #region
                            case "GPBoolean":
                                config.Type = GPParameterType.Boolean;
                                if (param.DefaultValue != null)
                                    config.DefaultValue = new Client.Tasks.GPBoolean(param.Name, (bool)param.DefaultValue);
                                break;
                            case "GPDouble":
                                config.Type = GPParameterType.Double;
                                if (!string.IsNullOrEmpty(defaultString))
                                {
                                    double val = 0;
                                    if (double.TryParse(defaultString, System.Globalization.NumberStyles.Any, CultureHelper.GetCurrentCulture(), out val))
                                        config.DefaultValue = new Client.Tasks.GPDouble(param.Name, val);
                                }
                                break;
                            case "GPLong":
                                config.Type = GPParameterType.Long;
                                if (!string.IsNullOrEmpty(defaultString))
                                {
                                    int val = 0;
                                    if (int.TryParse(defaultString, out val))
                                        config.DefaultValue = new ESRI.ArcGIS.Client.Tasks.GPLong(param.Name, val);
                                }
                                break;
                            case "GPDate":
                                config.Type = GPParameterType.Date;
                                if (!string.IsNullOrEmpty(defaultString))
                                {
                                    long ticks = 0;
                                    if (long.TryParse(defaultString, out ticks))
                                        config.DefaultValue = new ESRI.ArcGIS.Client.Tasks.GPDate(param.Name, Epoch.AddMilliseconds(ticks));
                                }
                                break;
                            case "GPLinearUnit":
                                config.Type = GPParameterType.LinearUnit;
                                if (param.DefaultValue is ESRI.ArcGIS.Mapping.GP.MetaData.GPLinearUnit)
                                {
                                    ESRI.ArcGIS.Mapping.GP.MetaData.GPLinearUnit value = (param.DefaultValue as ESRI.ArcGIS.Mapping.GP.MetaData.GPLinearUnit);
                                    config.DefaultValue = new ESRI.ArcGIS.Client.Tasks.GPLinearUnit(param.Name,
                                        (ESRI.ArcGIS.Client.Tasks.esriUnits)(Enum.Parse(typeof(ESRI.ArcGIS.Client.Tasks.esriUnits), value.Units, true)),
                                        value.Distance);
                                }
                                break;
                            case "GPString":
                                config.Type = GPParameterType.String;
                                config.DefaultValue = new ESRI.ArcGIS.Client.Tasks.GPString(param.Name, defaultString);
                                if (param.ChoiceList != null && param.ChoiceList.Length > 0)
                                {
                                    config.ChoiceList = new List<Choice>(param.ChoiceList.Length);
                                    for (int i = 0; i < param.ChoiceList.Length; i++)
                                    {
                                        config.ChoiceList.Add(new Choice()
                                        {
                                            DisplayText = param.ChoiceList[i],
                                            Value =
                                                new ESRI.ArcGIS.Client.Tasks.GPString(param.Name, param.ChoiceList[i])
                                        });
                                    }
                                }
                                break;
                            case "GPMultiValue:GPString":
                                config.Type = GPParameterType.MultiValueString;

                                object[] defaultStrings = param.DefaultValue as object[];
                                // the default value could be an array of strings
                                if (defaultStrings != null && defaultStrings.Length > 0)
                                {
                                    List<GPString> list =
                                        (from string s in defaultStrings select new GPString(param.Name, s)).ToList();

                                    config.DefaultValue = new GPMultiValue<GPString>(param.Name, list);
                                }

                                if (param.ChoiceList != null && param.ChoiceList.Length > 0)
                                {
                                    config.ChoiceList = new List<Choice>(param.ChoiceList.Length);
                                    foreach (string t in param.ChoiceList)
                                    {
                                        config.ChoiceList.Add(new Choice
                                                                  {
                                                                      DisplayText = t,
                                                                      Value = new GPString(param.Name, t)
                                                                  });
                                    }
                                }
                                break;
                            case "GPRecordSet":
                                config.Type = GPParameterType.RecordSet;
                                config.ToolTip = config.HelpText = Resources.Strings.EnterUrlForRecordset;
                                break;
                            case "GPDataFile":
                                config.Type = GPParameterType.DataFile;
                                config.ToolTip = config.HelpText = Resources.Strings.EnterUrlForFile;
                                break;
                            default:
                                break;
                            #endregion
                        }
                        #endregion
                    }
                    if (config != null)
                    {
                        if (param.Direction == "esriGPParameterDirectionInput")
                        {
                            config.Input = true;
                            InputParameters.Add(config);
                        }
                        else
                        {
                            config.Input = false;
                            OutputParameters.Add(config);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(serviceInfo.ResultMapServerName) && !string.IsNullOrEmpty(serviceInfo.CurrentVersion))
            {
                MapServiceLayerParameterConfig layerConfig = new MapServiceLayerParameterConfig
                {
                    Name = serviceInfo.ResultMapServerName,
                    LayerName = serviceInfo.ResultMapServerName,
                    Type = GPParameterType.MapServiceLayer,
                    LayerInfos = resultMapserviceLayerInfos,
                    SupportsJobResource = true,
                    Opacity = 1,
                };
                OutputParameters.Add(layerConfig);
                LayerOrder.Add(layerConfig.Name);
            }

            #endregion
        }

        private LayerInformation ToLayerInfo(MetaData.GPParameter param, int id)
        {
            LayerInformation layerInfo = new LayerInformation()
            {
                Name = param.Name,
                ID = id
            };

            MetaData.GPFeatureRecordSetLayer frs = param.DefaultValue as MetaData.GPFeatureRecordSetLayer;

            if (frs != null && frs.Fields != null && frs.Fields.Length > 0)
            {
                foreach (MetaData.Field field in frs.Fields)
                {
                    if (FieldHelper.IsFieldFilteredOut(field.Type))
                        continue;

                    layerInfo.Fields.Add(new ESRI.ArcGIS.Mapping.Core.FieldInfo()
                    {
                        DisplayName = field.Alias,
                        FieldType = mapFieldType(field.Type),
                        Name = field.Name,
                        VisibleInAttributeDisplay = true,
                        VisibleOnMapTip = true,
                    });
                    if (string.IsNullOrEmpty(layerInfo.DisplayField)) layerInfo.DisplayField = field.Name;
                }
            }
            return layerInfo;
        }

        internal const string esriFieldType = "esriFieldType";
        private static FieldType mapFieldType(string type)
        {
            if (type.StartsWith(esriFieldType, StringComparison.Ordinal))
            {
                type = type.Substring(esriFieldType.Length);
                ESRI.ArcGIS.Client.Field.FieldType fieldType =
                    (ESRI.ArcGIS.Client.Field.FieldType)Enum.Parse(typeof(ESRI.ArcGIS.Client.Field.FieldType), type, true);

                if (fieldType == ESRI.ArcGIS.Client.Field.FieldType.Double
                    || fieldType == Client.Field.FieldType.Single)
                {
                    return FieldType.DecimalNumber;
                }
                else if (fieldType == Client.Field.FieldType.OID
                    || fieldType == ESRI.ArcGIS.Client.Field.FieldType.Integer
                    || fieldType == ESRI.ArcGIS.Client.Field.FieldType.SmallInteger)
                {
                    return FieldType.Integer;
                }
                else if (fieldType == Client.Field.FieldType.Date)
                {
                    return FieldType.DateTime;
                }
            }
            return FieldType.Text;// For now all other fields are treated as strings
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
