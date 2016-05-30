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
using ESRI.ArcGIS.Client;
using System.Windows.Media.Imaging;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class AttributionDisplayControl : ContentControl
    {
        #region Map
        /// <summary>
        /// 
        /// </summary>
        public Map Map
        {
            get { return GetValue(MapProperty) as Map; }
            set { SetValue(MapProperty, value); }
        }

        /// <summary>
        /// Identifies the Map dependency property.
        /// </summary>
        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register(
                "Map",
                typeof(Map),
                typeof(AttributionDisplayControl),
                new PropertyMetadata(null, OnMapPropertyChanged));

        /// <summary>
        /// MapProperty property changed handler.
        /// </summary>
        /// <param name="d">AttributionDisplayControl that changed its Map.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AttributionDisplayControl source = d as AttributionDisplayControl;
            source.onMapPropertyChanged();
        }
        #endregion

        private void onMapPropertyChanged()
        {
            if (Map != null)
            {
                Map.Layers.CollectionChanged -= Layers_CollectionChanged;
                Map.Layers.CollectionChanged += Layers_CollectionChanged;

                if (Map.Layers.Count > 0)
                    setAttributionForBaseMapLayer(Map.Layers[0]);
            }
        }

        void Layers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                Content = null;
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add
                && e.NewStartingIndex == 0)
            {
                if (Map.Layers.Count > 0)
                    setAttributionForBaseMapLayer(Map.Layers[0]);
            }
        }

        private void setAttributionForBaseMapLayer(Layer layer)
        {
            if (layer == null)
                return;
            else if (layer is ESRI.ArcGIS.Client.Toolkit.DataSources.OpenStreetMapLayer)
            {
                Content = new TextBlock()
                { 
                    FontWeight = FontWeights.Bold,
                    Text = "© OpenStreetMap (and) contributors, CC-BY-SA",                    
                };
            }
            else if (layer is ESRI.ArcGIS.Client.Bing.TileLayer)
            {
                Content = new Image()
                {
                    Stretch = System.Windows.Media.Stretch.None,
                    Source = new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/BingLogo.png", UriKind.Relative)),
                };
            }
            else if (layer is ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer
                || layer is ESRI.ArcGIS.Client.ArcGISImageServiceLayer
                || layer is ESRI.ArcGIS.Client.ArcGISTiledMapServiceLayer)
            {                
                Content = new Image()
                {
                    Stretch = System.Windows.Media.Stretch.None,
                    Source = new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/esri_64x32.png", UriKind.Relative)),
                };
            }
            else
            {
                Content = null;
            }
        }
    }
}
