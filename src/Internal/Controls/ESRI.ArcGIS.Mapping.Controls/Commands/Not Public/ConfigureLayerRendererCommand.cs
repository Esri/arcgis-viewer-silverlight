/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows.Input;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    //[Export(typeof(ICommand))]
	//[DisplayName("ConfigureLayerRendererDisplayName")]
	//[Category("CategorySymbology")]
	//[Description("ConfigureLayerRendererDescription")]
    public class ConfigureLayerRendererCommand : LayerCommandBase
    {

        private SymbolConfigProvider SymbolConfigProvider
        {
            get
            {
                return View.Instance.SymbolConfigProvider;
            }
        }

        private GeometryType GeometryType
        {
            get
            {
                GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
                if (graphicsLayer != null)
                {
                    return Core.LayerExtensions.GetGeometryType(graphicsLayer);
                }
                return GeometryType.Unknown;
            }
        }

        private Collection<Color> ThemeColors
        {
            get
            {
                return View.Instance.ThemeColors;
            }
        }
        
        protected override void OnLayerChanged(object sender, EventArgs e)
        {
            base.OnLayerChanged(sender, e);

            if (layerSymbologyConfigControl == null)
                return; 
            
            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
            {
                layerSymbologyConfigControl.Layer = Layer; // update the layer
            }
            else // not a graphics layer .. close the window
            {
                MapApplication.Current.HideWindow(layerSymbologyConfigControl);
                layerSymbologyConfigControl = null;
            }
        }

        #region ICommand Members

        public override bool CanExecute(object parameter)
        {
            if (Layer == null)
                return false;
            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
                return false;
            return graphicsLayer.Renderer != null;
        }

        internal LayerSymbologyConfigControl layerSymbologyConfigControl;
        public override void Execute(object parameter)
        {
            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
                return;

            if (layerSymbologyConfigControl != null)
                MapApplication.Current.HideWindow(layerSymbologyConfigControl);

            layerSymbologyConfigControl = new LayerSymbologyConfigControl()
            { 
                 GeometryType = GeometryType,
                 SymbolConfigProvider = SymbolConfigProvider,
                 Layer = Layer,
                 ThemeColors = ThemeColors,
            };
            layerSymbologyConfigControl.LayerRendererChanged += new EventHandler<LayerRendererChangedEventArgs>(layerSymbologyConfigControl_LayerRendererChanged);
            layerSymbologyConfigControl.LayerRendererAttributeChanged += new EventHandler<LayerRendererAttributeChangedEventArgs>(layerSymbologyConfigControl_LayerRendererAttributeChanged);

            MapApplication.Current.ShowWindow(LocalizableStrings.SymbolOptions, layerSymbologyConfigControl, false, null, (o, e) => { if (layerSymbologyConfigControl != null) layerSymbologyConfigControl.CloseAllPopups(); layerSymbologyConfigControl = null; });            
        }

        void layerSymbologyConfigControl_LayerRendererAttributeChanged(object sender, LayerRendererAttributeChangedEventArgs e)
        {
            OnLayerRendererAttributeChanged(e);
        }

        void layerSymbologyConfigControl_LayerRendererChanged(object sender, LayerRendererChangedEventArgs e)
        {
            OnLayerRendererChanged(e);
        }
        #endregion
        
        public void NotifyOfRendererModified()
        {
            if (layerSymbologyConfigControl == null)
                return;

            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
                return;

            layerSymbologyConfigControl.GeometryType = GeometryType;
            layerSymbologyConfigControl.SymbolConfigProvider = SymbolConfigProvider;
            layerSymbologyConfigControl.Layer = null;
            layerSymbologyConfigControl.Layer = Layer;
        }        

        protected virtual void OnLayerRendererChanged(LayerRendererChangedEventArgs e)
        {
            if (LayerRendererChanged != null)
                LayerRendererChanged(this, e);
        }

        protected virtual void OnLayerRendererAttributeChanged(LayerRendererAttributeChangedEventArgs e)
        {
            if (LayerRendererAttributeChanged != null)
                LayerRendererAttributeChanged(this, e);
        }

        public event EventHandler<LayerRendererChangedEventArgs> LayerRendererChanged;
        public event EventHandler<LayerRendererAttributeChangedEventArgs> LayerRendererAttributeChanged;
    }
}
