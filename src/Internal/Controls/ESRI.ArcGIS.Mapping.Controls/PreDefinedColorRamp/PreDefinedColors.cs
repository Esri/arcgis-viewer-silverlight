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

namespace ESRI.ArcGIS.Mapping.Controls
{
    internal static class PreDefinedColors
    {
        private static object lockObject = new object();
        private static List<LinearGradientBrush> brushes;
        private static List<LinearGradientBrush> GetPreDefinedColorGradients()
        {
            if (brushes == null)
            {
                lock (lockObject)
                {
                    if (brushes == null)
                    {
                        brushes = new List<LinearGradientBrush>();
                        brushes.Add(createBrush("#FFFF0000", Constants.DefaultLightRedHexColorValue));
                        brushes.Add(createBrush(Constants.DefaultLightRedHexColorValue, "#FFFF0000"));

                        brushes.Add(createBrush(Colors.Black, Colors.White));
                        brushes.Add(createBrush(Colors.White, Colors.Black));

                        brushes.Add(createBrush("#FFCCCCFF", "#FF0101E0"));
                        brushes.Add(createBrush("#FF0101E0", "#FFCCCCFF"));

                        brushes.Add(createBrush("#FFD3E5E8", "#FF2E648C"));
                        brushes.Add(createBrush("#FF2E648C", "#FFD3E5E8"));


                        brushes.Add(createBrush("#FFCBF5EA", "#FF30CF92"));
                        brushes.Add(createBrush("#FF30CF92", "#FFCBF5EA"));

                        brushes.Add(createBrush("#FFF0ECAA", "#FF664830"));
                        brushes.Add(createBrush("#FF664830", "#FFF0ECAA"));


                        brushes.Add(createBrush("#FF4575B5", "#FFD62F27"));
                        brushes.Add(createBrush("#FFD62F27", "#FF4575B5"));


                        brushes.Add(createBrush("#FF00F5F5", "#FFF500F5"));
                        brushes.Add(createBrush("#FFF500F5", "#FF00F5F5"));

                        brushes.Add(createBrush("#FFECFCCC", "#FF9DCC10"));
                        brushes.Add(createBrush("#FF9DCC10", "#FFECFCCC"));

                        brushes.Add(createBrush("#FFB6EDF0", "#FF090991"));
                        brushes.Add(createBrush("#FF090991", "#FFB6EDF0"));
                    }
                }
            }
            return brushes;
        }

        internal static List<LinearGradientBrush> Brushes
        {
            get { return GetPreDefinedColorGradients(); }
        }

        private static LinearGradientBrush createBrush(string startHex, string endHex)
        {
            return (createBrush(ColorPickerUtils.FromHex(startHex), ColorPickerUtils.FromHex(endHex)));
        }

        private static LinearGradientBrush createBrush(Color start, Color end)
        {
            LinearGradientBrush brush = new LinearGradientBrush { StartPoint = new Point(0, 1), EndPoint = new Point(1, 1) ,
                 
            };
            brush.GradientStops.Add(new GradientStop { Color = start, Offset = 0.1 });
            brush.GradientStops.Add(new GradientStop { Color = end, Offset = 1.1 });
            return brush;
        }

        internal static LinearGradientBrush FindMatchingGradientBrush(Color start, Color end)
        {
            foreach (LinearGradientBrush brush in Brushes)
            {
                if (brush.GradientStops[0].Color.Equals(start) &&
                    brush.GradientStops[1].Color.Equals(end))
                {
                    return brush;
                }
            }
            return null; 
        }

        internal static void OverrideWithCustomColors(List<ColorInfo> colors)
        {
            if (colors == null || colors.Count < 1)
                return;
            brushes = new List<LinearGradientBrush>();
            foreach (ColorInfo cInfo in colors)
            {
                brushes.Add(createBrush(cInfo.Start, cInfo.End));
            }
        }
    }
}
