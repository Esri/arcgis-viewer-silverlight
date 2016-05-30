/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Windows.Media;
using System.Xml;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Toolkit.DataSources;
using System.Globalization;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class HeatMapLayerXamlWriter : LayerXamlWriter
    {
        public HeatMapLayerXamlWriter(XmlWriter writer, Dictionary<string, string> namespaces) :
            base(writer, namespaces)
        {

        }

        protected override void WriteStartElement(string layerPrefix, string layerNamespace)
        {
            writer.WriteStartElement(Constants.esriPrefix, "HeatMapLayer", Constants.esriNamespace);
        }

        protected override void WriteAttributes(Client.Layer layer)
        {
            base.WriteAttributes(layer);

            HeatMapLayer heatMapLayer = layer as HeatMapLayer;
            if (heatMapLayer != null)
            {
                if (heatMapLayer.Intensity != 10.0)
                    writer.WriteAttributeString("Intensity", heatMapLayer.Intensity.ToString(CultureInfo.InvariantCulture));
            }
        }

        protected override void WriteElementContents(Client.Layer layer)
        {
            base.WriteElementContents(layer);

            HeatMapLayer heatMapLayer = layer as HeatMapLayer;
            if (heatMapLayer != null)
            {
                #region Gradient
                if (heatMapLayer.Gradient != null)
                {
                    writer.WriteStartElement(Constants.esriPrefix, "HeatMapLayer.Gradient", Constants.esriNamespace);
                    writer.WriteStartElement("GradientStopCollection");
                    foreach (GradientStop gradient in heatMapLayer.Gradient)
                    {
                        writer.WriteStartElement("GradientStop");
                        if (gradient.Color != null)
                            writer.WriteAttributeString("Color", gradient.Color.ToString(CultureInfo.InvariantCulture));
                        if(gradient.Offset != 0)
                            writer.WriteAttributeString("Offset", gradient.Offset.ToString(CultureInfo.InvariantCulture));
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
                #endregion

                #region Heat Map Points
                if (heatMapLayer.HeatMapPoints != null)
                {
                    writer.WriteStartElement(Constants.esriPrefix, "HeatMapLayer.HeatMapPoints", Constants.esriNamespace);
                    writer.WriteStartElement(Constants.esriPrefix, "PointCollection", Constants.esriNamespace);

                    foreach (MapPoint heatMapPoint in heatMapLayer.HeatMapPoints)
                    {
                        writer.WriteStartElement(Constants.esriPrefix, "MapPoint", Constants.esriNamespace);
                        writer.WriteAttributeString("X", heatMapPoint.X.ToString(CultureInfo.InvariantCulture));
                        writer.WriteAttributeString("Y", heatMapPoint.Y.ToString(CultureInfo.InvariantCulture));
                        if (heatMapPoint.SpatialReference != null)
                        {
                            writer.WriteStartElement(Constants.esriPrefix, "MapPoint.SpatialReference", Constants.esriNamespace);
                            writeSpatialReference(writer, heatMapPoint.SpatialReference);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }

                #endregion
            }
        }

        private void writeSpatialReference(System.Xml.XmlWriter writer, SpatialReference sRef)
        {
            if (sRef == null)
                return;

            writer.WriteStartElement("SpatialReference", Constants.esriNamespace);
            if (sRef.WKID != default(int))
                writer.WriteAttributeString("WKID", sRef.WKID.ToString(CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(sRef.WKT))
                writer.WriteAttributeString("WKT", sRef.WKT);
            writer.WriteEndElement();
        }
    }
}
