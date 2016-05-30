/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Utils;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.GP.ParameterSupport
{

    public class ParameterConfig
    {
        public string Name { get; set; }

        /// <summary>
        /// The display name of the parameter, as specified by the GP service.
        /// Not intended to be configurable.
        /// </summary>
        public string DisplayName { get; set; }
        public string Label { get; set; }
        public string HelpText { get; set; }
        public string ToolTip { get; set; }
        public GPParameterType Type { get; set; }
        public bool ShownAtRunTime { get; set; }
        public bool Required { get; set; }
        public GPParameter DefaultValue { get; set; }
        public bool Input { get; set; }
        public List<Choice> ChoiceList { get; set; }

        internal virtual void ToJson(JsonWriter jw)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            AddToJsonDictionary(ref dictionary);
            jw.StartObject();
            bool wroteFirst = false;
            #region serialize dictionary
            foreach (KeyValuePair<string, object> item in dictionary)
            {
                if (wroteFirst)
                    jw.Writer.Write(",");
                else
                    wroteFirst = true;
                if (item.Key == "defaultValue")
                {
                    if (item.Value is ESRI.ArcGIS.Client.Tasks.GPLinearUnit)
                    {
                        ESRI.ArcGIS.Client.Tasks.GPLinearUnit linearUnit = item.Value as ESRI.ArcGIS.Client.Tasks.GPLinearUnit;
                        jw.StartProperty("defaultValue");
                        jw.StartObject();
                        jw.WriteProperty("distance", linearUnit.Distance);
                        jw.Writer.Write(",");
                        jw.WriteProperty("units", linearUnit.Unit);
                        jw.EndObject();
                    }
                    else if (item.Value is ESRI.ArcGIS.Client.Tasks.GPRasterData)
                    {
                        ESRI.ArcGIS.Client.Tasks.GPRasterData raster = item.Value as ESRI.ArcGIS.Client.Tasks.GPRasterData;
                        jw.StartProperty("defaultValue");
                        jw.StartObject();
                        jw.WriteProperty("url", raster.Url);
                        jw.Writer.Write(",");
                        jw.WriteProperty("format", raster.Format);
                        jw.EndObject();
                    }
                    else
                        jw.WriteProperty(item.Key, item.Value, false);
                }
                else
                    jw.WriteProperty(item.Key, item.Value, false);
            }
            #endregion

            #region Choice List
            if (ChoiceList != null && ChoiceList.Count > 0)
            {
                jw.Writer.Write(",");
                jw.StartProperty("choiceList");
                jw.StartArray();
                wroteFirst = false;
                foreach (Choice item in ChoiceList)
                {
                    if (item != null)
                    {
                        if (!wroteFirst)
                            wroteFirst = true;
                        else
                            jw.Writer.Write(",");
                        item.ToJson(jw);
                    }
                }
                jw.EndArray();
            }
            #endregion
            jw.EndObject();
        }

        protected virtual void AddToJsonDictionary(ref Dictionary<string, object> dictionary)
        {
            addString(ref dictionary, Name, "name");
            addString(ref dictionary, Label, "label");
            addString(ref dictionary, DisplayName, "displayName");
            addString(ref dictionary, HelpText, "helpText");
            addString(ref dictionary, ToolTip, "toolTip");
            dictionary.Add("type", Type);
            dictionary.Add("shownAtRunTime", ShownAtRunTime);
            dictionary.Add("required", Required);
            dictionary.Add("input", Input);
            string defaultValue = null;
            switch (Type)
            {
                case GPParameterType.LinearUnit: //handled in ToJson
                case GPParameterType.RasterData:
                case GPParameterType.RasterDataLayer:
                    dictionary.Add("defaultValue", DefaultValue);
                    break;
                case GPParameterType.Boolean:
                case GPParameterType.Date:
                case GPParameterType.Double:
                case GPParameterType.Long:
                case GPParameterType.String:
                case GPParameterType.FeatureLayer:
                case GPParameterType.RecordSet:
                case GPParameterType.DataFile:
                    defaultValue = ParameterBase.ParameterToString(Type, DefaultValue, CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(defaultValue))
                        dictionary.Add("defaultValue", defaultValue);
                    break;
            }
        }

        private void addString(ref Dictionary<string, object> dictionary, string value, string attributeName)
        {
            if (!string.IsNullOrEmpty(value))
                dictionary.Add(attributeName, value);
        }

        public virtual void FromJson(string json)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            IDictionary<string, object> dictionary = jss.DeserializeObject(json) as IDictionary<string, object>;
            FromJsonDictionary(dictionary);
        }

        protected virtual void FromJsonDictionary(IDictionary<string, object> dictionary)
        {
            if (dictionary.ContainsKey("name"))
                Name = dictionary["name"] as string;
            if (dictionary.ContainsKey("label"))
                Label = dictionary["label"] as string;
            if (dictionary.ContainsKey("displayName"))
                DisplayName = dictionary["displayName"] as string;
            if (dictionary.ContainsKey("helpText"))
                HelpText = dictionary["helpText"] as string;
            if (dictionary.ContainsKey("toolTip"))
                ToolTip = dictionary["toolTip"] as string;
            if (dictionary.ContainsKey("type"))
                Type = (GPParameterType)(Enum.Parse(typeof(GPParameterType), dictionary["type"] as string, true));
            if (dictionary.ContainsKey("shownAtRunTime"))
                ShownAtRunTime = Convert.ToBoolean(dictionary["shownAtRunTime"]);
            if (dictionary.ContainsKey("required"))
                Required = Convert.ToBoolean(dictionary["required"]);
            if (dictionary.ContainsKey("input"))
                Input = Convert.ToBoolean(dictionary["input"]);
            #region Default value
            if (dictionary.ContainsKey("defaultValue"))
            {
                object o = dictionary["defaultValue"];
                if (o != null)
                {
                    switch (Type)
                    {
                        case GPParameterType.Boolean:
                        case GPParameterType.Date:
                        case GPParameterType.Double:
                        case GPParameterType.Long:
                        case GPParameterType.String:
                        case GPParameterType.FeatureLayer:
                        case GPParameterType.RecordSet:
                        case GPParameterType.DataFile:
                            DefaultValue = ParameterBase.StringToParameter(Name, Type, o as string, CultureInfo.InvariantCulture);
                            break;
                        case GPParameterType.RasterData:
                        case GPParameterType.RasterDataLayer:
                            IDictionary<string, object> rDic = o as IDictionary<string, object>;
                            if (rDic != null)
                            {
                                string url = null, format = null;
                                if (rDic.ContainsKey("url"))
                                    url = rDic["url"] as string;
                                if (rDic.ContainsKey("format"))
                                    format = rDic["format"] as string;
                                if (url != null || format != null)
                                    DefaultValue = new Client.Tasks.GPRasterData(Name, url, format) { Format = format };//Workaround for slapi bug
                            }
                            break;
                        case GPParameterType.LinearUnit:
                            IDictionary<string, object> lDic = o as IDictionary<string, object>;
                            if (lDic != null)
                            {
                                double distance = double.NaN;
                                ESRI.ArcGIS.Client.Tasks.esriUnits units = Client.Tasks.esriUnits.esriMiles;
                                if (lDic.ContainsKey("distance"))
                                    distance = Convert.ToDouble(lDic["distance"]);
                                if (lDic.ContainsKey("units"))
                                    units = (ESRI.ArcGIS.Client.Tasks.esriUnits)(Enum.Parse(typeof(ESRI.ArcGIS.Client.Tasks.esriUnits),
                                        lDic["units"] as string, true));
                                DefaultValue = new Client.Tasks.GPLinearUnit(Name, units, distance);
                            }
                            break;
                    }
                }
            }
            #endregion

            #region ChoiceList

            if (dictionary.ContainsKey("choiceList"))
            {
                ChoiceList = new List<ParameterSupport.Choice>();
            
                IEnumerable choices = dictionary["choiceList"] as IEnumerable;
                if (choices != null)
                {
                    foreach (Dictionary<string, object> item in choices)
                    {
                        if (item != null)
                        {
                            Choice choice = new Choice();
                            choice.FromJsonDictionary(item, Name);
                            ChoiceList.Add(choice);
                        }
                    }
                }
            }
            #endregion
        }

        public static ParameterConfig Create(IDictionary<string, object> dictionary)
        {
            GPParameterType type = GPParameterType.String;
            if (dictionary.ContainsKey("type"))
                type = (GPParameterType)(Enum.Parse(typeof(GPParameterType), dictionary["type"] as string, true));
            ParameterConfig config = null;
            switch (type)
            {
                case GPParameterType.Boolean:
                case GPParameterType.Double:
                case GPParameterType.Long:
                case GPParameterType.String:
                case GPParameterType.Date:
                case GPParameterType.LinearUnit:
                case GPParameterType.RecordSet:
                case GPParameterType.DataFile:
                    config = new ParameterConfig();
                    break;
                case GPParameterType.RasterData:
                case GPParameterType.RasterDataLayer:
                    config = new RasterDataParameterConfig();
                    break;
                case GPParameterType.MultiValueString:
                    config = new MultiValueStringConfig();
                    break;
                case GPParameterType.FeatureLayer:
                    config = new FeatureLayerParameterConfig();
                    break;
                case GPParameterType.MapServiceLayer:
                    config = new MapServiceLayerParameterConfig();
                    break;
            }
            if (config != null)
                config.FromJsonDictionary(dictionary);
            return config;
        }

        public virtual void AddConfigUI(Grid grid)
        {
            TextBlock label;
            #region Header
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.RowDefinitions.Add(new RowDefinition());
            label = new TextBlock()
            {
                Text = DisplayName ?? Name,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new Thickness(2,10,2,2),
                FontWeight = FontWeights.Bold, 
                TextTrimming = TextTrimming.WordEllipsis
            };
            g.Children.Add(label);

            if (Required)
            {
                label = new TextBlock()
                    {
                        Text = Resources.Strings.Required,
                        VerticalAlignment = System.Windows.VerticalAlignment.Center,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                        Margin = new Thickness(2),
                        FontWeight = FontWeights.Light,
                        FontStyle = FontStyles.Italic
                    };
                label.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(label);
            }
            g.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            g.SetValue(Grid.ColumnSpanProperty, 2);
            grid.Children.Add(g);
            #endregion

            #region Type
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            label = new TextBlock()
            {
				Text = Resources.Strings.LabelType,
                Margin = new Thickness(2),
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            label.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            grid.Children.Add(label);
            label = new TextBlock()
            {
                Text = Type.ToString(),
                Margin = new Thickness(2),
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            label.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            label.SetValue(Grid.ColumnProperty, 1);
            grid.Children.Add(label);
            #endregion

            #region Label
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            label = new TextBlock()
            {
				Text = Resources.Strings.LabelLabel,
                Margin = new Thickness(2),
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            label.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            grid.Children.Add(label);
            TextBox labelTextBox = new TextBox()
            {
                Text = Label == null ? string.Empty : Label,
                Margin = new Thickness(2),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
            };
            labelTextBox.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            labelTextBox.SetValue(Grid.ColumnProperty, 1);
            grid.Children.Add(labelTextBox);
            labelTextBox.TextChanged += (s, e) =>
            {
                Label = labelTextBox.Text;
            };
            #endregion

            if (Input)
            {
                #region Tooltip
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                label = new TextBlock()
                {
					Text = Resources.Strings.LabelTooltip,
                    Margin = new Thickness(2),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                label.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                grid.Children.Add(label);
                TextBox tbToolTip = new TextBox()
                {
                    Margin = new Thickness(2),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                };
                if (!string.IsNullOrEmpty(ToolTip))
                    tbToolTip.Text = ToolTip;
                tbToolTip.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                tbToolTip.SetValue(Grid.ColumnProperty, 1);
                grid.Children.Add(tbToolTip);
                tbToolTip.TextChanged += (s, e) =>
                {
                    ToolTip = tbToolTip.Text;
                };
                #endregion

                #region ShownAtRuntime
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                label = new TextBlock()
                {
					Text = Resources.Strings.LabelShowParameter,
                    Margin = new Thickness(2),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                label.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                grid.Children.Add(label);
                CheckBox chb = new CheckBox() { IsChecked = ShownAtRunTime, Margin = new Thickness(2) };
                chb.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                chb.SetValue(Grid.ColumnProperty, 1);
                grid.Children.Add(chb);
                chb.Checked += (s, e) => { ShownAtRunTime = true; };
                chb.Unchecked += (s, e) => { ShownAtRunTime = false; };
                #endregion

                #region Default Value
                if (Type != GPParameterType.FeatureLayer && Type != GPParameterType.MultiValueString)
                {
                    grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                    label = new TextBlock()
                    {
						Text = Resources.Strings.LabelDefaultValue,
                        Margin = new Thickness(2),
                        VerticalAlignment = System.Windows.VerticalAlignment.Center
                    };
                    label.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                    grid.Children.Add(label);
                    ParameterBase paramUI = ParameterFactory.Create(this, null);
                    paramUI.AddUI(grid);
                    paramUI.CanExecuteChanged += (a, b) =>
                    {
                        this.DefaultValue = paramUI.Value;
                    };
                }
                #endregion
            }
        }
    }
}
