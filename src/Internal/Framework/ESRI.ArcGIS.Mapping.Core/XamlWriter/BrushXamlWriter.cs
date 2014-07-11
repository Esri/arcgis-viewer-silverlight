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
using System.Collections.Generic;
using System.Xml;
using System.Windows.Media.Imaging;
using System.Globalization;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class BrushXamlWriter: XamlWriterBase
    {
        public BrushXamlWriter(XmlWriter writer, Dictionary<string, string> namespaces)
            : base(writer, namespaces)
        {
        }

        public void WriteBrush(Brush brush)
        {
            StartType(brush, null);
            if (brush.Opacity < 1)
                WriteAttribute("Opacity", brush.Opacity.ToString(CultureInfo.InvariantCulture));
            if (brush is SolidColorBrush)
            {
                WriteAttribute("Color", (brush as SolidColorBrush).Color);
            }
            else if (brush is GradientBrush)
            {
                if (brush is LinearGradientBrush)
                {
                    LinearGradientBrush g = brush as LinearGradientBrush;
                    if (g.MappingMode != BrushMappingMode.RelativeToBoundingBox)
                        WriteAttribute("MappingMode", g.MappingMode.ToString());
                    WriteGradientStops(g.GradientStops);
                }
                else if (brush is RadialGradientBrush)
                {
                    RadialGradientBrush r = brush as RadialGradientBrush;
                    if (r.ColorInterpolationMode != ColorInterpolationMode.SRgbLinearInterpolation)
                        WriteAttribute("ColorInterpolationMode", r.ColorInterpolationMode.ToString());
                    if (r.MappingMode != BrushMappingMode.RelativeToBoundingBox)
                        WriteAttribute("MappingMode", r.MappingMode.ToString());
                    if (r.Center != null)
                    {
                        //TODO
                    }
                    if (r.GradientOrigin != null)
                    {
                        // TODO
                    }
                    foreach (GradientStop stop in r.GradientStops)
                    {
                        writer.WriteStartElement("GradientStop");
                        WriteAttribute("Color", stop.Color);
                        WriteAttribute("Offset", stop.Offset);
                        writer.WriteEndElement();
                    }
                    //TODO
                }
            }
            else if (brush is TileBrush)
            {
                TileBrush tb = brush as TileBrush;
                if (tb.AlignmentX != AlignmentX.Center)
                    WriteAttribute("AlignmentX", tb.AlignmentX.ToString());
                if (tb.AlignmentY != AlignmentY.Center)
                    WriteAttribute("AlignmentY", tb.AlignmentY.ToString());
                if (tb.Stretch != Stretch.Fill)
                    WriteAttribute("Stretch", tb.Stretch.ToString());
                if (brush is ImageBrush)
                {
                    ImageBrush b = brush as ImageBrush;
                    if (b.ImageSource is BitmapImage)
                    {
                        Uri uri = (b.ImageSource as BitmapImage).UriSource;
                        WriteAttribute("ImageSource", uri.OriginalString);
                    }
                }
                else if (brush is VideoBrush)
                {
                    VideoBrush b = brush as VideoBrush;
                    WriteAttribute("SourceName", b.SourceName);
                }
            }
            writer.WriteEndElement();
        }

        public void WriteGradientStops(GradientStopCollection gradientStops)
        {
            foreach (GradientStop stop in gradientStops)
            {
                writer.WriteStartElement("GradientStop");
                WriteAttribute("Color", stop.Color);
                WriteAttribute("Offset", stop.Offset);
                writer.WriteEndElement();
            }
        }
    }
}
