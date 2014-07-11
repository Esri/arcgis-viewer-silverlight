/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.GP.ParameterSupport
{
    public class MapServiceLayerParameterConfig : ParameterConfig, INotifyPropertyChanged
    {
        private MapTipsConfig mapTipsConfig;
        private double _opacity;

        public string LayerName { get; set; }
        public Collection<LayerInformation> LayerInfos { get; set; }
        public bool SupportsJobResource { get; set; }
        
        public double Opacity
        {
            get { return _opacity; }
            set
            {
                _opacity = value;
                OnPropertyChanged("Opacity");
            }
        }

        #region (De)serialization
        protected override void FromJsonDictionary(IDictionary<string, object> dictionary)
        {
            base.FromJsonDictionary(dictionary);
            if (dictionary.ContainsKey("supportsJobResource"))
                SupportsJobResource = Convert.ToBoolean(dictionary["supportsJobResource"]);
            if (dictionary.ContainsKey("layerName"))
                LayerName = dictionary["layerName"] as string;
            if (dictionary.ContainsKey("opacity"))
                Opacity = Convert.ToDouble(dictionary["opacity"]);
            if (dictionary.ContainsKey("layerInfos"))
                LayerInfos = JsonSerializer.Deserialize<Collection<LayerInformation>>(dictionary["layerInfos"] as string);
        }

        protected override void AddToJsonDictionary(ref Dictionary<string, object> dictionary)
        {
            base.AddToJsonDictionary(ref dictionary);
            dictionary.Add("supportsJobResource", SupportsJobResource);
            dictionary.Add("layerName", LayerName);
            dictionary.Add("opacity", Opacity);
            if (LayerInfos != null)
                dictionary.Add("layerInfos", JsonSerializer.Serialize<Collection<LayerInformation>>(LayerInfos));
        }
        #endregion


        public override void AddConfigUI(System.Windows.Controls.Grid grid)
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
                Text = Name,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new Thickness(2, 10, 2, 2),
                FontWeight = FontWeights.Bold,
                TextTrimming = TextTrimming.WordEllipsis
            };
            g.Children.Add(label);
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

            #region Layer name
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            label = new TextBlock()
            {
                Text = Resources.Strings.LabelLayerName,
                Margin = new Thickness(2),
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            label.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            grid.Children.Add(label);
            TextBox tb = new TextBox()
            {
                Text = LayerName,
                Margin = new Thickness(2),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
            };
            tb.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            tb.SetValue(Grid.ColumnProperty, 1);
            grid.Children.Add(tb);
            tb.TextChanged += (s, e) =>
            {
                LayerName = tb.Text;
            };
            #endregion

            #region Popups config
            if (SupportsJobResource && LayerInfos != null && LayerInfos.Count > 0 && LayerInfos.All(l => l.Fields != null && l.Fields.Count > 0))
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                label = new TextBlock()
                {
                    Text = Resources.Strings.LabelPopUps,
                    Margin = new Thickness(2),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                label.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                grid.Children.Add(label);
                Button popupButton = new Button
                                        {
                                            Content = new Image
                                            {
                                                Source = new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.GP;component/Images/Show_Popup16.png", UriKind.Relative)),
                                                Stretch = Stretch.None,
                                                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                                                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                                            },
                                            Width = 22,
                                            HorizontalAlignment = HorizontalAlignment.Left,
                                            Style = Application.Current.Resources["SimpleButtonStyle"] as Style
                                        };
                ToolTipService.SetToolTip(popupButton, Resources.Strings.ConfigurePopupFieldAliasesAndVisibility);
                popupButton.Click += popupButton_Click;
                popupButton.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                popupButton.SetValue(Grid.ColumnProperty, 1);
                grid.Children.Add(popupButton);
            }
            #endregion

            #region Opacity
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            label = new TextBlock()
            {
                Text = Resources.Strings.LabelTransparency,
                Margin = new Thickness(2),
                VerticalAlignment = System.Windows.VerticalAlignment.Top
            };
            label.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            grid.Children.Add(label);

            ContentControl sliderOpacity = new ContentControl()
            {
                DataContext = this,
                Style = ResourceUtility.LoadEmbeddedStyle("Themes/HorizontalTransparencySlider.xaml", "TransparencySliderStyle")
            };
            sliderOpacity.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            sliderOpacity.SetValue(Grid.ColumnProperty, 1);
            grid.Children.Add(sliderOpacity);
            #endregion
        }

        private void popupButton_Click(object sender, RoutedEventArgs e)
        {
            if (mapTipsConfig == null)
            {
                ArcGISDynamicMapServiceLayer dummyLayer = new ArcGISDynamicMapServiceLayer();
                ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetLayerInfos(dummyLayer, LayerInfos);
                mapTipsConfig = new MapTipsConfig()
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Layer = dummyLayer,
                    Style = ResourceUtility.LoadEmbeddedStyle("Themes/MapTipsConfig.Theme.xaml", "MapTipsConfigStyle"),
                    Margin = new Thickness(10)
                };
            }
            MapApplication.Current.ShowWindow(Resources.Strings.ConfigurePopups, mapTipsConfig, true, null, null, WindowType.DesignTimeFloating);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}
