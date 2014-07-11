/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
    [DisplayName("MoveLayerUpDisplayName")]
    [Category("CategoryLayer")]
    [Description("MoveLayerUpDescription")]
    public class MoveLayerUpCommand : LayerCommandBase
    {
        #region ICommand Members

        public override bool CanExecute(object parameter)
        {
            return (Layer != null
                && Map != null
                && LayerUtils.HasNonBasemapLayerBeforeAfterIndex(Layer, Map.Layers, true)
                && !(bool)Layer.GetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty)); // (base map = 0) layer cannot already be the first layer
        }

        private Map Map
        {
            get
            {
                return MapApplication.Current.Map;
            }
        }

        public override void Execute(object parameter)
        {
            if (Layer == null || Map == null)
                return;
            int pos = Map.Layers.IndexOf(Layer);
            int newPos = pos - 1;
            //make sure we account for basemap layers
            for (int index = newPos; index > -1; index--)
            {
                bool isBasemapLayerList = (bool)Map.Layers[index].GetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty);
                if (!isBasemapLayerList)
                {
                    newPos = index;
                    break;
                }
            }

            if (newPos < 1) // check for out of bounds [Remember, we ignore the base map layer]
                return;


            Layer layer = Layer;
            Map.Layers.Remove(layer);
            Map.Layers.Insert(newPos, layer);
            if (View.Instance != null)
            {
                GraphicsLayer graphicsLayer = layer as GraphicsLayer;
                if (graphicsLayer != null)
                    View.Instance.AttributeDisplay.GraphicsLayer = graphicsLayer;
            }
            OnCanExecuteChanged(EventArgs.Empty);
        }

        #endregion

    }
}
