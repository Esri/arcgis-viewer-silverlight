/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows.Media;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls
{
    internal abstract class SymbologyUtils
    {
        public static Symbol GetDefaultSymbolForLayer(Layer layer)
        {
            GraphicsLayer graphicsLayer = layer as GraphicsLayer;
            if (graphicsLayer != null)
            {
                ClassBreaksRenderer renderer = graphicsLayer.Renderer as ClassBreaksRenderer;
                if (renderer != null)
                    return renderer.DefaultSymbol;
                else
                {
                    ConfigurableFeatureLayer featureLayer = layer as ConfigurableFeatureLayer;
                    if (featureLayer != null)
                    {
                        if (featureLayer.FeatureSymbol != null)
                            return featureLayer.FeatureSymbol;
                        else
                        {
                            // All Graphics share the same symbol. Return a reference to the first one
                            if (featureLayer.Graphics.Count > 0)
                                return featureLayer.Graphics[0].Symbol;
                        }
                    }
                    else
                    {
                        // All Graphics share the same symbol. Return a reference to the first one
                        if (graphicsLayer.Graphics.Count > 0)
                            return graphicsLayer.Graphics[0].Symbol;
                    }
                }
            }
            return null;
        }
    }
}
