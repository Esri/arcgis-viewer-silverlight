/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
	[DisplayName("ZoomToSelectionDisplayName")]
	[Category("CategorySelection")]
	[Description("ZoomToSelectionDescription")]
    public class ZoomToSelectionCommand : LayerCommandBase
    {
        // The Minimum Extent Ratio for the Map:
        private const double EXPAND_EXTENT_RATIO = .05;        

        private Map Map
        {
            get
            {
                return MapApplication.Current.Map;
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

            return graphicsLayer.SelectedGraphics.Count() > 0;
        }

        public override void Execute(object parameter)
        {
            if (Layer == null)
                return;

            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
                return;

            IEnumerable<Graphic> SelectedGraphics = graphicsLayer.SelectedGraphics;
            if (Map != null && SelectedGraphics != null)
            {
                Envelope newMapExtent = null;
                foreach (Graphic graphic in SelectedGraphics)
                {
                    if (graphic.Geometry != null && graphic.Geometry.Extent != null)
                        newMapExtent = graphic.Geometry.Extent.Union(newMapExtent);
                }
                if (newMapExtent != null)
                {
                    if (newMapExtent.Width > 0 || newMapExtent.Height > 0)
                    {
                        newMapExtent = new Envelope(newMapExtent.XMin - newMapExtent.Width * EXPAND_EXTENT_RATIO, newMapExtent.YMin - newMapExtent.Height * EXPAND_EXTENT_RATIO,
                            newMapExtent.XMax + newMapExtent.Width * EXPAND_EXTENT_RATIO, newMapExtent.YMax + newMapExtent.Height * EXPAND_EXTENT_RATIO);
                        Map.ZoomTo(newMapExtent);
                    }
                    else
                        Map.PanTo(newMapExtent);
                }
            }
        }

        #endregion
    }
}
