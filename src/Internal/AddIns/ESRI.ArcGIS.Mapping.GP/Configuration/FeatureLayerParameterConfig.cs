/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using System.Text;
using System.Xml;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.GP.Resources;

namespace ESRI.ArcGIS.Mapping.GP.ParameterSupport
{
    public class FeatureLayerParameterConfig : ParameterConfig
    {
        public enum InputMode
        {
            SketchLayer,
            SelectExistingLayer,
            CurrentExtent,
            Url
        }
        public string LayerName { get; set; }
        public ESRI.ArcGIS.Mapping.Core.GeometryType GeometryType { get; set; }


        public string[] SingleFields { get; internal set; }
        public string[] DoubleFields { get; internal set; }

        public InputMode Mode { get; set; }

        public ESRI.ArcGIS.Client.GraphicsLayer Layer { get; set; }

        public double Opacity { get; set; }

        protected override void AddToJsonDictionary(ref Dictionary<string, object> dictionary)
        {
            base.AddToJsonDictionary(ref dictionary);
            dictionary.Add("mode", Mode);
            dictionary.Add("geometryType", GeometryType);
            if (Layer != null)
                dictionary.Add("layer", serializeLayer(Layer));
            dictionary.Add("layerName", LayerName);
            dictionary.Add("opacity", Opacity);

            if (SingleFields != null && SingleFields.Length > 0)
                dictionary.Add("singleFields", serializeFields(SingleFields));
            if (DoubleFields != null && DoubleFields.Length > 0)
                dictionary.Add("doubleFields", serializeFields(DoubleFields));
        }

        string serializeFields(string[] fields)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (string item in fields)
            {
                if (!first)
                    sb.Append(",");
                sb.Append(item);
                first = false;
            }
            return sb.ToString();
        }

        string serializeLayer(GraphicsLayer layer)
        {

            Dictionary<string, string> Namespaces = new Dictionary<string, string>();
            Namespaces.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            Namespaces.Add(ESRI.ArcGIS.Mapping.Core.Constants.esriPrefix, ESRI.ArcGIS.Mapping.Core.Constants.esriNamespace);
            Namespaces.Add("esriBing", "clr-namespace:ESRI.ArcGIS.Client.Bing;assembly=ESRI.ArcGIS.Client.Bing");
            Namespaces.Add(ESRI.ArcGIS.Mapping.Core.Constants.esriMappingPrefix, ESRI.ArcGIS.Mapping.Core.Constants.esriMappingNamespace);
            Namespaces.Add(ESRI.ArcGIS.Mapping.Core.Constants.esriFSSymbolsPrefix, ESRI.ArcGIS.Mapping.Core.Constants.esriFSSymbolsNamespace);
			Namespaces.Add(ESRI.ArcGIS.Mapping.Core.Constants.esriExtensibilityPrefix, ESRI.ArcGIS.Mapping.Core.Constants.esriExtensibilityNamespace);

            StringBuilder xaml = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(xaml, new XmlWriterSettings() { OmitXmlDeclaration = true });
            writer.WriteStartElement("ContentControl");

            // write namespaces
            foreach (string key in Namespaces.Keys)
            {
                string _namespace = "http://schemas.microsoft.com/winfx/2006/xaml"; // default
                if (Namespaces.ContainsKey(key))
                    _namespace = Namespaces[key];
                writer.WriteAttributeString("xmlns", key, null, _namespace);
            }
            ESRI.ArcGIS.Mapping.Core.GraphicsLayerXamlWriter layerWriter = new Core.GraphicsLayerXamlWriter(writer, Namespaces);
            layerWriter.WriteLayer(layer, layer.GetType().Name, ESRI.ArcGIS.Mapping.Core.Constants.esriNamespace);

            writer.WriteEndElement();

            writer.Flush();
            writer = null;
            string config = xaml.ToString();
            // Inject default namespace
            config = config.Insert(16, "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" ");
            return config;
        }

        protected override void FromJsonDictionary(IDictionary<string, object> dictionary)
        {
            base.FromJsonDictionary(dictionary);
            if (dictionary.ContainsKey("mode"))
                Mode = (InputMode)(Enum.Parse(typeof(InputMode), dictionary["mode"] as string, true));
            if (dictionary.ContainsKey("geometryType"))
                GeometryType = (ESRI.ArcGIS.Mapping.Core.GeometryType)
                    (Enum.Parse(typeof(ESRI.ArcGIS.Mapping.Core.GeometryType), dictionary["geometryType"] as string, true));
            if (dictionary.ContainsKey("layer"))
                Layer = deserializeLayer(dictionary["layer"] as string);
            if (dictionary.ContainsKey("layerName"))
                LayerName = dictionary["layerName"] as string;
            if (dictionary.ContainsKey("opacity"))
                Opacity = Convert.ToDouble(dictionary["opacity"]);
            if (dictionary.ContainsKey("singleFields") && !string.IsNullOrEmpty(dictionary["singleFields"] as string))
                SingleFields = (dictionary["singleFields"] as string).Split(new char[] { ',' });
            if (dictionary.ContainsKey("doubleFields") && !string.IsNullOrEmpty(dictionary["doubleFields"] as string))
                DoubleFields = (dictionary["doubleFields"] as string).Split(new char[] { ',' });
        }

        private GraphicsLayer deserializeLayer(string xaml)
        {
            ContentControl control = System.Windows.Markup.XamlReader.Load(xaml) as ContentControl;
            return control.Content as GraphicsLayer;
        }

        public override void AddConfigUI(System.Windows.Controls.Grid grid)
        {
            base.AddConfigUI(grid);
            #region Layer name
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            TextBlock layerName = new TextBlock()
            {
                Text = Resources.Strings.LabelLayerName,
                Margin = new Thickness(2),
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            layerName.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            grid.Children.Add(layerName);
            TextBox labelTextBox = new TextBox()
            {
                Text = LayerName == null ? string.Empty : LayerName,
                Margin = new Thickness(2),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
            };
            labelTextBox.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            labelTextBox.SetValue(Grid.ColumnProperty, 1);
            grid.Children.Add(labelTextBox);
            labelTextBox.TextChanged += (s, e) =>
            {
                LayerName = labelTextBox.Text;
            };
            #endregion
            #region Renderer
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            TextBlock label2 = new TextBlock()
            {
                Text = Resources.Strings.LabelRenderer,
                Margin = new Thickness(2),
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            label2.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            grid.Children.Add(label2);
            Button rendererButton = null;
            if (GeometryType == Core.GeometryType.Unknown)
            {
                TextBlock tb = new TextBlock() { Text = Resources.Strings.NotAvailable, VerticalAlignment = VerticalAlignment.Center };
                ToolTipService.SetToolTip(tb, Resources.Strings.GeometryTypeIsNotKnown);
                tb.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                tb.SetValue(Grid.ColumnProperty, 1);
                grid.Children.Add(tb);
            }
            else
            {
                rendererButton = new Button()
                {
                    Content = new Image()
                               {
                                   Source = new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.GP;component/Images/ColorScheme16.png", UriKind.Relative)),
                                   Stretch = Stretch.None,
                                   VerticalAlignment = System.Windows.VerticalAlignment.Center,
                                   HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                               },
                    Width = 22,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Style = Application.Current.Resources["SimpleButtonStyle"] as Style,
                    IsEnabled = (Mode == InputMode.SketchLayer),
                };
                ToolTipService.SetToolTip(rendererButton, Resources.Strings.ConfigureRenderer);
                rendererButton.Click += rendererButton_Click;
                rendererButton.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                rendererButton.SetValue(Grid.ColumnProperty, 1);
                grid.Children.Add(rendererButton);
            }
            #endregion
            #region Popup Config
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            label2 = new TextBlock()
            {
                Text = Resources.Strings.LabelPopUps,
                Margin = new Thickness(2),
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            label2.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            grid.Children.Add(label2);
            Button popupButton = null;
            if (Layer == null)
            {
                TextBlock tb = new TextBlock() { Text = Resources.Strings.NotAvailable, VerticalAlignment = VerticalAlignment.Center };
                ToolTipService.SetToolTip(tb, Resources.Strings.FieldInformationIsNotKnown);
                tb.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                tb.SetValue(Grid.ColumnProperty, 1);
                grid.Children.Add(tb);
            }
            else
            {
                popupButton = new Button()
                {
                    Content = new Image()
                    {
                        Source = new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.GP;component/Images/Show_Popup16.png", UriKind.Relative)),
                        Stretch = Stretch.None,
                        VerticalAlignment = System.Windows.VerticalAlignment.Center,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                    },
                    Width = 22,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Style = Application.Current.Resources["SimpleButtonStyle"] as Style,
                    IsEnabled = (Mode == InputMode.SketchLayer),
                };
                ToolTipService.SetToolTip(popupButton, Resources.Strings.ConfigurePopupFieldAliasesAndVisibility);
                popupButton.Click += popupButton_Click;
                popupButton.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                popupButton.SetValue(Grid.ColumnProperty, 1);
                grid.Children.Add(popupButton);
            }
            #endregion

            #region Transparency
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            label2 = new TextBlock()
            {
                Text = Resources.Strings.LabelTransparency,
                Margin = new Thickness(2),
                VerticalAlignment = System.Windows.VerticalAlignment.Top
            };
            label2.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            grid.Children.Add(label2);

            ContentControl slider = new ContentControl()
            {
                DataContext = this,
                IsEnabled = (Mode == InputMode.SketchLayer),
                Style = ResourceUtility.LoadEmbeddedStyle("Themes/HorizontalTransparencySlider.xaml", "TransparencySliderStyle")
            };
            //Slider slider = new Slider()
            //{
            //    DataContext = this,
            //    IsEnabled = (Mode == InputMode.SketchLayer),
            //    Orientation = Orientation.Horizontal,
            //    Width = 145,
            //    Minimum = 0,
            //    Maximum = 1
            //};
            //slider.SetBinding(Slider.ValueProperty,
            //    new System.Windows.Data.Binding("Opacity") { Mode = System.Windows.Data.BindingMode.TwoWay });
            slider.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            slider.SetValue(Grid.ColumnProperty, 1);
            grid.Children.Add(slider);
            #endregion

            if (Input)
            {
                #region Input Mode
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                TextBlock label = new TextBlock()
                {
                    Text = Resources.Strings.LabelInputFeatures,
                    Margin = new Thickness(2),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                label.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                grid.Children.Add(label);

                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                StackPanel panel = new StackPanel()
                {
                    Orientation = System.Windows.Controls.Orientation.Vertical,
                    Margin = new Thickness(15, 0, 0, 0),
                };
                panel.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                panel.SetValue(Grid.ColumnSpanProperty, 2);
                RadioButton interactive = new RadioButton()
                {
                    Content = Resources.Strings.Interactively,
                    IsChecked = (Mode == InputMode.SketchLayer),
                    Margin = new Thickness(2),
                    Foreground = Application.Current.Resources["DesignHostBackgroundTextBrush"] as Brush
                };
                panel.Children.Add(interactive);
                RadioButton selection = new RadioButton()
                {
                    Content = Resources.Strings.BySelectingLayerFromMap,
                    IsChecked = (Mode == InputMode.SelectExistingLayer),
                    Margin = new Thickness(2),
                    Foreground = Application.Current.Resources["DesignHostBackgroundTextBrush"] as Brush
                };
                panel.Children.Add(selection);
                RadioButton fromExtent = null;
                if (GeometryType == Core.GeometryType.Polygon)
                {
                    fromExtent = new RadioButton()
                    {
                        Content = Resources.Strings.FromMapExtent,
                        IsChecked = (Mode == InputMode.CurrentExtent),
                        Margin = new Thickness(2),
                        Foreground = Application.Current.Resources["DesignHostBackgroundTextBrush"] as Brush
                    };
                    panel.Children.Add(fromExtent);

                }
                interactive.Checked += (a, b) =>
                {
                    Mode = InputMode.SketchLayer;
                    selection.IsChecked = false;
                    if (fromExtent != null)
                        fromExtent.IsChecked = false;
                    if (popupButton != null)
                        popupButton.IsEnabled = true;
                    if (rendererButton != null)
                        rendererButton.IsEnabled = true;
                    //if (slider != null)
                    //    slider.IsEnabled = true;
                };
                selection.Checked += (a, b) =>
                 {
                     Mode = InputMode.SelectExistingLayer;
                     interactive.IsChecked = false;
                     if (fromExtent != null)
                         fromExtent.IsChecked = false;
                     if (popupButton != null)
                         popupButton.IsEnabled = false;
                     if (rendererButton != null)
                         rendererButton.IsEnabled = false;
                     //if (slider != null)
                     //    slider.IsEnabled = false;
                 };
                if (fromExtent != null)
                {
                    fromExtent.Checked += (a, b) =>
                     {
                         Mode = InputMode.CurrentExtent;
                         interactive.IsChecked = false;
                         selection.IsChecked = false;
                         if (popupButton != null)
                             popupButton.IsEnabled = false;
                         if (rendererButton != null)
                             rendererButton.IsEnabled = false;
                         //if (slider != null)
                         //    slider.IsEnabled = false;
                     };
                }
                grid.Children.Add(panel);
                #endregion
            }
        }

        MapTipsConfig mapTipsConfig;
        void popupButton_Click(object sender, RoutedEventArgs e)
        {
            if (mapTipsConfig == null)
            {
                mapTipsConfig = new MapTipsConfig()
                {
                    Width = 350,
                    MaxHeight = 400,
                    Layer = Layer,
                    Style = ResourceUtility.LoadEmbeddedStyle("Themes/MapTipsConfig.Theme.xaml", "MapTipsConfigStyle"),
                    Margin = new Thickness(10)
                };
            }
            MapApplication.Current.ShowWindow(Resources.Strings.ConfigurePopups, mapTipsConfig, true, null, null, 
                WindowType.DesignTimeFloating);
        }

        internal static GeometryType ToSlapiGeomType(ESRI.ArcGIS.Mapping.Core.GeometryType geomType)
        {
            switch (geomType)
            {
                case ESRI.ArcGIS.Mapping.Core.GeometryType.Point:
                    return ESRI.ArcGIS.Client.Tasks.GeometryType.Point;
                case ESRI.ArcGIS.Mapping.Core.GeometryType.Polygon:
                    return ESRI.ArcGIS.Client.Tasks.GeometryType.Polygon;
                case ESRI.ArcGIS.Mapping.Core.GeometryType.Polyline:
                    return ESRI.ArcGIS.Client.Tasks.GeometryType.Polyline;
            }
            return ESRI.ArcGIS.Client.Tasks.GeometryType.Envelope;
        }


        internal static ESRI.ArcGIS.Mapping.Core.GeometryType ToGeomType(GeometryType geomType)
        {
            switch (geomType)
            {
                case ESRI.ArcGIS.Client.Tasks.GeometryType.Point:
                    return Core.GeometryType.Point;
                case ESRI.ArcGIS.Client.Tasks.GeometryType.Polygon:
                    return Core.GeometryType.Polygon;
                case ESRI.ArcGIS.Client.Tasks.GeometryType.Polyline:
                    return Core.GeometryType.Polyline;
                case ESRI.ArcGIS.Client.Tasks.GeometryType.Envelope:
                case ESRI.ArcGIS.Client.Tasks.GeometryType.MultiPoint:
                default:
                    return Core.GeometryType.Unknown;
            }
        }

        internal Symbol GetDrawSymbol()
        {
            if (Layer != null && Layer.Renderer != null)
            {
                if (Layer.Renderer is SimpleRenderer)
                    return (Layer.Renderer as SimpleRenderer).Symbol;
                if (Layer.Renderer is ClassBreaksRenderer)
                    return (Layer.Renderer as ClassBreaksRenderer).DefaultSymbol;
                if (Layer.Renderer is UniqueValueRenderer)
                    return (Layer.Renderer as UniqueValueRenderer).DefaultSymbol;
            }
            return GetSymbol(GeometryType);
        }

        //internal ConfigureLayerRendererCommand configureLayerRendererCommand;
        private LayerSymbologyConfigControl symbologyConfigDialog;
        void rendererButton_Click(object sender, RoutedEventArgs e)
        {
            if (symbologyConfigDialog == null)
            {
                symbologyConfigDialog = new LayerSymbologyConfigControl() { 
                    Margin = new Thickness(10),
                    SymbolConfigProvider = View.Instance.SymbolConfigProvider,
                    ThemeColors = View.Instance.ThemeColors
                };
            }
            ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetGeometryType(Layer, GeometryType);
            symbologyConfigDialog.Layer = Layer;
            MapApplication.Current.ShowWindow(Strings.ConfigureRenderer, symbologyConfigDialog, true);
        }

        public static SimpleRenderer GetSimpleRenderer(ESRI.ArcGIS.Mapping.Core.GeometryType geometryType)
        {
            return new SimpleRenderer() { Symbol = GetSymbol(geometryType) };
        }

        static Symbol GetSymbol(ESRI.ArcGIS.Mapping.Core.GeometryType geometryType)
        {
            Core.SymbolConfigProvider provider = ViewUtility.GetSymbolConfigProvider();
            switch (geometryType)
            {
                case ESRI.ArcGIS.Mapping.Core.GeometryType.Point:
                    return new ESRI.ArcGIS.Mapping.Core.Symbols.ImageFillSymbol()
                    {
                        DisplayName = Resources.Strings.OrangeStickpin,
                        Size = 20d,
                        Source = "/ESRI.ArcGIS.Mapping.GP;component/Images/OrangeStickpin.png",
                        OriginX = 0.5,
                        OriginY = 1
                    };
                case ESRI.ArcGIS.Mapping.Core.GeometryType.Polygon:
                    return new ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleFillSymbol()
                        {
                            Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 255, 255, 255)),
                            SelectionColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Cyan)
                        };
                case ESRI.ArcGIS.Mapping.Core.GeometryType.Polyline:
                    return new ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleLineSymbol()
                    {
                        Color = new SolidColorBrush(Colors.Red),
                        SelectionColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Cyan)
                    };
            }
            return null;
        }
    }



}
