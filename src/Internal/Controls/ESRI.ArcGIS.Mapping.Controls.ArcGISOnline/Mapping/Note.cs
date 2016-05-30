/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
    public class Note
    {
        public static GraphicsLayer AddToMap(string name, Graphic graphic, Map map)
        {
            GraphicsLayer graphicsLayer = CreateGraphicsLayer(name, graphic, map);
            map.Layers.Add(graphicsLayer);
            return graphicsLayer;
        }

        /// <summary>
        /// Converts a single graphic "note" element into a graphics layer.
        /// </summary>
        /// <param name="name">The name to assign to the graphics layer.</param>
        /// <param name="graphic">The graphic element to add to the graphics layer.</param>
        /// <param name="map">The map to which the layer will eventually be added, to prevent name conflicts with existing layers.</param>
        /// <returns>A graphics layer that contains the graphic element.</returns>
        public static GraphicsLayer CreateGraphicsLayer(string name, Graphic graphic, Map map)
        {
            graphic.Attributes["Name"] = name;
            GraphicsLayer graphicsLayer = new GraphicsLayer();
            graphicsLayer.ID = name;
            if (string.IsNullOrEmpty(graphicsLayer.ID) || (!string.IsNullOrEmpty(graphicsLayer.ID) && map.Layers[graphicsLayer.ID] != null))
                graphicsLayer.ID = Guid.NewGuid().ToString("N");
            graphicsLayer.Graphics.Add(graphic);
            graphicsLayer.Renderer = new ESRI.ArcGIS.Mapping.Core.Symbols.HiddenRenderer();
            graphicsLayer.SetValue(ESRI.ArcGIS.Client.Extensibility.MapApplication.LayerNameProperty, name);

            Collection<FieldInfo> fields = new Collection<FieldInfo>();
            fields.Add(new FieldInfo()
            {
                Name = "Name",
                VisibleInAttributeDisplay = true,
                DisplayName = "Name",
                FieldType = FieldType.Text,
                VisibleOnMapTip = false
            });
            ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetFields(graphicsLayer, fields);
            return graphicsLayer;
        }
    }
}
