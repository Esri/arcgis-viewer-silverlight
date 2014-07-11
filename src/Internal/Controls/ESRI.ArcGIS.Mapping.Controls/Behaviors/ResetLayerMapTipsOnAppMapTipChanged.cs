/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ResetLayerMapTipsOnAppMapTipChanged : Behavior<SolidColorBrushSelector>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject == null)
                return;

            AssociatedObject.ColorPicked -= AssociatedObject_ColorPicked;
            AssociatedObject.ColorPicked += AssociatedObject_ColorPicked;
        }

        void AssociatedObject_ColorPicked(object sender, ColorChosenEventArgs e)
        {
            ResetMapTipOnGraphicsLayers();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.ColorPicked -= AssociatedObject_ColorPicked;
            base.OnDetaching();
        }

        #region public Map Map
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
                typeof(ResetLayerMapTipsOnAppMapTipChanged),
                new PropertyMetadata(null));
        #endregion public Map Map

        public void ResetMapTipOnGraphicsLayers()
        {
            if (Map == null)
                return;

            foreach (Layer layer in Map.Layers)
            {
                GraphicsLayer graphicsLayer = layer as GraphicsLayer;
                if (graphicsLayer == null || graphicsLayer.MapTip == null)
                    continue;

                graphicsLayer.MapTip = null; // the PositionMapTip behavior will re-buid it
            }

            // Also save out the Xaml for the ApplicationMapTip
            ApplicationMapTip appMapTip = ApplicationMapTipExtensions.GetApplicationMapTip(Map) as ApplicationMapTip;
            if(appMapTip != null)
                ApplicationMapTipExtensions.SetApplicationMapTipContainerXaml(Map, appMapTip.ToXaml());
        }
    }
}
