/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = "OpacitySlider", Type = typeof(Slider))]
    [TemplatePart(Name = "ClusterPanel", Type = typeof(ContentControl))]
    [TemplatePart(Name = "RenderAsHeatMapPanel", Type = typeof(ContentControl))]
    [TemplatePart(Name = "MarkerSymbolSelector", Type = typeof(MarkerSymbolSelector))]
    [TemplatePart(Name = "CurrentSymbolImage", Type = typeof(Image))]
    [TemplatePart(Name = "CurrentSymbolImageOverlay", Type = typeof(Rectangle))]
    [TemplatePart(Name = "ClusterFeaturesCheckBox", Type = typeof(CheckBox))]
    [TemplatePart(Name = "AdvancedClusterPropertiesButton", Type = typeof(Button))]
    [TemplatePart(Name = "RenderAsHeatMapCheckBox", Type = typeof(CheckBox))]
    [TemplatePart(Name = "AdvancedHeatMapPropertiesButton", Type = typeof(Button))]
    public class PointSymbolConfigControl : Control
    {
        Slider OpacitySlider = null;
        ContentControl ClusterPanel = null;
        ContentControl RenderAsHeatMapPanel = null;
        MarkerSymbolSelector MarkerSymbolSelector = null;
        Image CurrentSymbolImage = null;
        Rectangle CurrentSymbolImageOverlay = null;
        CheckBox ClusterFeaturesCheckBox = null;
        Button AdvancedClusterPropertiesButton = null;
        CheckBox RenderAsHeatMapCheckBox = null;
        Button AdvancedHeatMapPropertiesButton = null;

        public PointSymbolConfigControl()
        {
            DefaultStyleKey = typeof(PointSymbolConfigControl);
            
            _opacityChangedTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 500) };
            _opacityChangedTimer.Tick += OpacityChangedTimer_Tick;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            OpacitySlider = GetTemplateChild("OpacitySlider") as Slider;
            if (OpacitySlider != null)
                OpacitySlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(OpacitySlider_ValueChanged);

            ClusterPanel = GetTemplateChild("ClusterPanel") as ContentControl;
            RenderAsHeatMapPanel = GetTemplateChild("RenderAsHeatMapPanel") as ContentControl;
            
            MarkerSymbolSelector = GetTemplateChild("MarkerSymbolSelector") as MarkerSymbolSelector;
            if (MarkerSymbolSelector != null)
            {
                MarkerSymbolSelector.MarkerSymbolConfigFileUrl = MarkerSymbolConfigFileUrl;
                MarkerSymbolSelector.MarkerSymbolDirectory = MarkerSymbolDirectory;
                MarkerSymbolSelector.SymbolSelected += new SymbolSelectedEventHandler(MarkerSymbolSelector_SymbolSelected);
                MarkerSymbolSelector.Show();
            }

            CurrentSymbolImage = GetTemplateChild("CurrentSymbolImage") as Image;
            CurrentSymbolImageOverlay = GetTemplateChild("CurrentSymbolImageOverlay") as Rectangle;

            ClusterFeaturesCheckBox = GetTemplateChild("ClusterFeaturesCheckBox") as CheckBox;
            if (ClusterFeaturesCheckBox != null)
            {
                ClusterFeaturesCheckBox.Checked += new RoutedEventHandler(ClusterFeaturesCheckBox_Checked);
                ClusterFeaturesCheckBox.Unchecked += new RoutedEventHandler(ClusterFeaturesCheckBox_Unchecked);
            }

            AdvancedClusterPropertiesButton = GetTemplateChild("AdvancedClusterPropertiesButton") as Button;
            if (AdvancedClusterPropertiesButton != null)
                AdvancedClusterPropertiesButton.Click += new RoutedEventHandler(AdvancedClusterProperties_Click);

            RenderAsHeatMapCheckBox = GetTemplateChild("RenderAsHeatMapCheckBox") as CheckBox;
            if (RenderAsHeatMapCheckBox != null)
            {
                RenderAsHeatMapCheckBox.Checked += new RoutedEventHandler(RenderAsHeatMapCheckBox_Checked);
                RenderAsHeatMapCheckBox.Unchecked += new RoutedEventHandler(RenderAsHeatMapCheckBox_Unchecked);
            }

            AdvancedHeatMapPropertiesButton = GetTemplateChild("AdvancedHeatMapPropertiesButton") as Button;
            if (AdvancedHeatMapPropertiesButton != null)
                AdvancedHeatMapPropertiesButton.Click += new RoutedEventHandler(AdvancedHeatMapProperties_Click);

            Initialize();
            loadPointSymbolConfigControl();
        }

        void RenderAsHeatMapCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // TODO nik (Refactor) 
        }        

        void RenderAsHeatMapCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // TODO nik (Refactor)   
        }

        void ClusterFeaturesCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
         // TODO nik (Refactor)   
        }

        void ClusterFeaturesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // TODO nik (Refactor)   
        }

        internal void Initialize()
        {
            // Only show this option when configuring the default symbol (not valid for class breaks)
            bool isConfiguringLayer = DataContext is Layer;
            if (ClusterPanel != null)
            {
                ClusterPanel.Visibility = isConfiguringLayer ? Visibility.Visible : Visibility.Collapsed;
                ClusterPanel.IsEnabled = DataContext is ConfigurableHeatMapLayer ? false : true;
            }
            if(RenderAsHeatMapPanel != null)
                RenderAsHeatMapPanel.Visibility = isConfiguringLayer ? Visibility.Visible : Visibility.Collapsed;
            if(MarkerSymbolSelector != null)
                MarkerSymbolSelector.IsEnabled = isConfiguringLayer ? true : false;

            PictureMarkerSymbol symbol = DataContext as PictureMarkerSymbol;
            if (symbol != null)
            {
                if(CurrentSymbolImage != null)
                    CurrentSymbolImage.Source = symbol.Source;
                if (OpacitySlider != null)
                {
                    OpacitySlider.SetBinding(Slider.ValueProperty, new Binding()
                    {
                        Path = new PropertyPath("Opacity"),
                        Source = this.DataContext
                    });
                    OpacitySlider.IsEnabled = DataContext is ConfigurableHeatMapLayer ? false : true;
                }
                if (MarkerSymbolSelector != null)
                    MarkerSymbolSelector.IsEnabled = true;
                if(CurrentSymbolImageOverlay !=null)
                    CurrentSymbolImageOverlay.Visibility = Visibility.Collapsed;
            }
            else
            {
                GraphicsLayer layerInfo = DataContext as GraphicsLayer;
                if (layerInfo != null)
                {
                    ClassBreaksRenderer renderer = layerInfo.Renderer as ClassBreaksRenderer;
                    if (renderer == null)
                        return;
                    PictureMarkerSymbol pictureMarkerSymbol = renderer.DefaultSymbol as PictureMarkerSymbol;
                    if (pictureMarkerSymbol != null)
                    {
                        if(CurrentSymbolImage != null)
                            CurrentSymbolImage.Source = pictureMarkerSymbol.Source;
                    }

                    if (MarkerSymbolSelector != null)
                        MarkerSymbolSelector.IsEnabled = DataContext is ConfigurableHeatMapLayer ? false : true;
                    if (CurrentSymbolImageOverlay != null)
                        CurrentSymbolImageOverlay.Visibility = DataContext is ConfigurableHeatMapLayer ? Visibility.Visible : Visibility.Collapsed;
                    if (ClusterFeaturesCheckBox != null)
                        ClusterFeaturesCheckBox.IsChecked = layerInfo.Clusterer != null;
                    if (AdvancedClusterPropertiesButton != null)
                        AdvancedClusterPropertiesButton.IsEnabled = layerInfo.Clusterer != null;
                }
            }
        }        

        public string MarkerSymbolConfigFileUrl { get; set; }

        public string MarkerSymbolDirectory { get; set; }

        void MarkerSymbolSelector_SymbolSelected(object sender, SymbolSelectedEventArgs e)
        {
            OnSymbolSelected(e);
        }

        public event SymbolSelectedEventHandler SymbolSelected;
        private void OnSymbolSelected(SymbolSelectedEventArgs e)
        {
            if (CurrentSymbolImage != null)
            {
                CurrentSymbolImage.Source = new BitmapImage
                {
                    UriSource = new Uri(e.SelectedImage.RelativeUrl, UriKind.Relative)
                };
            }

            PictureMarkerSymbol Symbol = new PictureMarkerSymbol()
            {
                Source = new BitmapImage() { UriSource = new Uri(e.SelectedImage.RelativeUrl, UriKind.Relative) },
                OffsetX = e.SelectedImage.CenterX,
                OffsetY = e.SelectedImage.CenterY,
                Width = e.SelectedImage.Width,
                Height = e.SelectedImage.Height,
                Opacity = OpacitySlider.Value,
                // TODO
                //CursorName = DataContext is TableLayerInfo ? Cursors.Hand.ToString() : Cursors.Arrow.ToString()
            };

            GraphicsLayer layer = DataContext as GraphicsLayer;
            if (layer != null)
            {
                // TODO:- verify if we still need this
                //LayerInfo layerInfo = DataContext as LayerInfo;
                //// refresh the graphics layer
                //GraphicsLayer lyr = ApplicationInstance.Instance.FindLayerForLayerInfo(layerInfo) as GraphicsLayer;
                //if (lyr != null)
                //{
                //    ensureCustomClassBreakRendererIsSet(lyr, layerInfo);
                //    CustomClassBreakRenderer cb = lyr.Renderer as CustomClassBreakRenderer;
                //    if (cb != null)
                //    {
                //        ImageFillSymbol imageFillSymbol = cb.DefaultSymbol as ImageFillSymbol;
                //        if (imageFillSymbol != null)
                //        {
                //            imageFillSymbol.Fill = new ImageBrush
                //            {
                //                ImageSource = new BitmapImage
                //                {
                //                    UriSource = new Uri(e.SelectedImage.RelativeUrl, UriKind.Relative)
                //                }
                //            };
                //            imageFillSymbol.CursorName = DataContext is TableLayerInfo ? Cursors.Hand.ToString() : Cursors.Arrow.ToString(); 

                //        }
                //    }
                //}

                // update the model
                ClassBreaksRenderer renderer = layer.Renderer as ClassBreaksRenderer;
                if (renderer != null)
                {
                    renderer.DefaultSymbol = Symbol;
                }
            }
            else
            {
                if (SymbolSelected != null)
                    SymbolSelected(this, e);
            }
        }

        private DispatcherTimer _opacityChangedTimer;
        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_opacityChangedTimer.IsEnabled)
                _opacityChangedTimer.Stop();

            _opacityChangedTimer.Start();
        }

        private void loadPointSymbolConfigControl()
        {            
            PictureMarkerSymbol symbol = getCurrentSymbol();
            if (symbol == null)
                return;
            if(CurrentSymbolImage != null)
                CurrentSymbolImage.Source = symbol.Source;
            if (OpacitySlider != null)
            {
                OpacitySlider.SetBinding(Slider.ValueProperty, new System.Windows.Data.Binding
                {
                    Path = new PropertyPath("Opacity"),
                    Source = symbol
                });
                OpacitySlider.Value = symbol.Opacity;
            }
        }

        private PictureMarkerSymbol getCurrentSymbol()
        {
            PictureMarkerSymbol symbol = null;
            GraphicsLayer layer = DataContext as GraphicsLayer;
            if (layer != null)
            {
                ClassBreaksRenderer renderer = layer.Renderer as ClassBreaksRenderer;
                if (renderer != null)
                    symbol = renderer.DefaultSymbol as PictureMarkerSymbol;
            }
            else
            {
                symbol = DataContext as PictureMarkerSymbol;
            }
            return symbol;
        }

        bool loadComplete = false;
        void OpacityChangedTimer_Tick(object sender, EventArgs e)
        {
            _opacityChangedTimer.Stop();
            if (!loadComplete)
            {
                loadComplete = true;
                return;
            }
            if (DataContext is LayerInfo)
            {
                GraphicsLayer lyr = DataContext as GraphicsLayer;
                if (lyr != null)
                {
                    // TODO:- Nik (Refactor)
                    //ensureCustomClassBreakRendererIsSet(lyr, layerInfo);
                    //ClassBreaksRenderer cb = lyr.Renderer as ClassBreaksRenderer;
                    //if (cb != null)
                    //{                        
                    //    ImageFillSymbol imageFillSymbol = cb.DefaultSymbol as ImageFillSymbol;
                    //    if (imageFillSymbol != null)
                    //    {
                    //        imageFillSymbol.Opacity = OpacitySlider.Value;
                    //        lyr.Refresh();
                    //    }
                    //}
                }
                else
                {
                    //HeatMapLayer hLayer = layer as HeatMapLayer;
                    //if (hLayer != null)
                    //{
                    //    hLayer.Opacity = OpacitySlider.Value;
                    //}
                }

                // update the model
                ClassBreaksRenderer renderer = lyr.Renderer as ClassBreaksRenderer;
                if (renderer != null)
                {
                    PictureMarkerSymbol symbol = renderer.DefaultSymbol as PictureMarkerSymbol;
                    if (symbol != null)
                        symbol.Opacity = OpacitySlider.Value;
                }
            }
            else
            {
                if (OpacityChanged != null)
                    OpacityChanged(this, new OpacityChangedEventArgs { Opacity = OpacitySlider.Value });
            }
        }

        internal static void ensureCustomClassBreakRendererIsSet(GraphicsLayer lyr, Layer layerInfo)
        {
            ClassBreaksRenderer cb = lyr.Renderer as ClassBreaksRenderer;
            if (cb == null) // ensure that the classbreaks renderer is enabled
            {
                if (lyr.Clusterer == null) // no clustering enabled
                {
                    cb = new ClassBreaksRenderer();
                    cb.DefaultSymbol = new PictureMarkerSymbol()
                    {
                        // TODO:- nik (Refactor)
                        // ThematicMapWrapper.createImageFillSymbol(layerInfo.DefaultSymbol as PictureMarkerSymbol);
                    };
                    // TODO:- nik (Refactor)
                    //if (layerInfo.ClassBreaks != null)
                    //{
                    //    cb.Attribute = layerInfo.ClassBreaks.Attribute;
                    //    if (layerInfo.ClassBreaks.Items != null)
                    //    {
                    //        foreach (ClassBreakInfo cbInfo in layerInfo.ClassBreaks.Items)
                    //        {
                    //            cb.Classes.Add(
                    //                new ESRI.ArcGIS.Client.ClassBreakInfo
                    //                {
                    //                    MaximumValue = cbInfo.IsMax ? cbInfo.MaxValue + 1 : cbInfo.MaxValue,
                    //                    MinimumValue = cbInfo.MinValue,
                    //                    Symbol = ThematicMapWrapper.createImageFillSymbol(cbInfo.Symbol as PictureMarkerSymbol)
                    //                }
                    //                );
                    //        }
                    //    }
                    //}
                }
                else // clustering enabled
                {
                }
            }
            lyr.Renderer = cb;
        }

        public event OpacityChangedEventHandler OpacityChanged;
        public delegate void OpacityChangedEventHandler(object sender, OpacityChangedEventArgs e);

        FlareClusterer currentClusterInfo;
        private void AdvancedClusterProperties_Click(object sender, RoutedEventArgs e)
        {
            GraphicsLayer layerInfo = DataContext as GraphicsLayer;
            if (layerInfo == null)
                return;
            FlareClusterer flareCluster = layerInfo.Clusterer as FlareClusterer;
            if (flareCluster == null)
                return;
            ChildWindow fw = new ChildWindow();
            //fw.ResizeMode = ResizeMode.NoResize;
            TextBlock title = new TextBlock { Foreground = new SolidColorBrush(Colors.White), FontSize = 12, FontWeight = FontWeights.Bold, Text = "Advanced Cluster Properties" };
            fw.Title = title;
            fw.Height = 235;
            fw.Width = 290;
            currentClusterInfo = flareCluster; // TODO nik (refactor) flareCluster. flareClusterer..ClusterInfo != null ? layerInfo.ClusterInfo.Clone() : new ClusterInfo(); 
            ClusterPropertiesConfigWindow configWindow = new ClusterPropertiesConfigWindow();
            configWindow.OkClicked += (o, args) => { fw.DialogResult = true; };
            configWindow.CancelClicked += (o, args) =>
            {
                fw.DialogResult = false; // automatically calls close
            };
            fw.Closed += (o, args) =>
            {
                if (fw.DialogResult != true) // && !fw.IsAppExit)
                    restoreClusterProperties();
            };
            configWindow.DataContext = flareCluster;
            fw.Content = configWindow;
            fw.DialogResult = null;
            //fw.ShowDialog();
            fw.Show();
        }

        void restoreClusterProperties()
        {
            GraphicsLayer layerInfo = this.DataContext as GraphicsLayer;
            if (layerInfo == null)
                return;
            // TODO:- nik (Refactor)
            //if (layerInfo.ClusterInfo != null && currentClusterInfo != null)
            //{
            //    layerInfo.ClusterInfo.ForeColor = currentClusterInfo.ForeColor;
            //    layerInfo.ClusterInfo.ForeHsv = currentClusterInfo.ForeHsv;
            //    layerInfo.ClusterInfo.BackgroundColor = currentClusterInfo.BackgroundColor;
            //    layerInfo.ClusterInfo.BackgroundHsv = currentClusterInfo.BackgroundHsv;
            //    layerInfo.ClusterInfo.MaximumFlareCount = currentClusterInfo.MaximumFlareCount;
            //}
        }

        //HeatMapInfo currentHeatMapInfo;
        ChildWindow heatMapWindow;
        HeatMapPropertiesConfigWindow configWindow;
        private void AdvancedHeatMapProperties_Click(object sender, RoutedEventArgs e)
        {
            ConfigurableHeatMapLayer layerInfo = this.DataContext as ConfigurableHeatMapLayer;
            if (layerInfo == null)
                return;
            (sender as Button).IsEnabled = false;
            if (heatMapWindow == null)
            {
                heatMapWindow = new ChildWindow();
                //heatMapWindow.ResizeMode = ResizeMode.NoResize;
                TextBlock title = new TextBlock { Foreground = new SolidColorBrush(Colors.White), FontSize = 12, FontWeight = FontWeights.Bold, Text = "Advanced Properties" };
                heatMapWindow.Title = title;
                double dialogWidth = 325;
                double dialogHeight = 268;
                heatMapWindow.Height = dialogHeight;
                heatMapWindow.Width = dialogWidth;

                //heatMapWindow.HorizontalOffset = (Application.Current.RootVisual.RenderSize.Width - heatMapWindow.Width) / 2;
                //heatMapWindow.VerticalOffset = (Application.Current.RootVisual.RenderSize.Height - heatMapWindow.Height) / 2;

                configWindow = new HeatMapPropertiesConfigWindow();
                configWindow.OkClicked += (o, args) => { heatMapWindow.DialogResult = true; };
                configWindow.CancelClicked += (o, args) =>
                {
                    heatMapWindow.DialogResult = false; // automatically calls close
                };
                configWindow.GradientStopsChanged += (o, args) =>
                {
                    //layerInfo.HeatMapInfo.Gradients = args.GradientStops;
                };

                heatMapWindow.Closed += (o, args) =>
                {
                    (sender as Button).IsEnabled = true;
                    if (heatMapWindow.DialogResult != true) // && !heatMapWindow.IsAppExit)
                        restoreHeatMapProperties();
                };
            }
            //if (layerInfo.HeatMapInfo == null)
            //    layerInfo.HeatMapInfo = new HeatMapInfo();
            //currentHeatMapInfo = layerInfo.HeatMapInfo != null ? layerInfo.HeatMapInfo.Clone() : new HeatMapInfo();           

            configWindow.DataContext = layerInfo;

            //Grid g = new Grid() { Width = 310, Height = 245-20 };
            //g.Children.Add(configWindow);
            //(Application.Current.RootVisual as Grid).Children.Add(g);

            heatMapWindow.Content = configWindow;
            heatMapWindow.DialogResult = null;
            //heatMapWindow.ShowWindow(false);
            heatMapWindow.Show();
        }

        private void restoreHeatMapProperties()
        {
            LayerInfo layerInfo = this.DataContext as LayerInfo;
            if (layerInfo == null)
                return;
            //if (layerInfo.HeatMapInfo != null && currentHeatMapInfo != null)
            //{
            //    layerInfo.HeatMapInfo.Intensity = currentHeatMapInfo.Intensity;
            //    layerInfo.HeatMapInfo.Resolution = currentHeatMapInfo.Resolution;
            //    layerInfo.HeatMapInfo.Gradients = currentHeatMapInfo.Gradients;
            //}
        }


    }

    public class OpacityChangedEventArgs
    {
        public double Opacity;
    }    
}
