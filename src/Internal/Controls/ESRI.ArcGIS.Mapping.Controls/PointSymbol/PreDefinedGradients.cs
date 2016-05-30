/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Windows.Media;

namespace ESRI.ArcGIS.Mapping.Controls
{
    internal static class PreDefinedGradients
    {
        private static object lockObject = new object();
        private static List<GradientStopCollection> stops;
        private static List<GradientStopCollection> GetPreDefinedGradients()
        {
            if (stops == null)
            {
                lock (lockObject)
                {
                    if (stops == null)
                    {
                        stops = new List<GradientStopCollection>();
                        stops.Add(createGradientStopCollection(new Color[] { Colors.Transparent, Colors.Blue, Colors.Red, Colors.Yellow, Colors.White }));
                        stops.Add(createGradientStopCollection(new Color[] { Colors.White, Colors.Yellow, Colors.Red, Colors.Blue, Colors.Transparent }));
                        stops.Add(createGradientStopCollection(new Color[] { 
                            ColorPickerUtils.FromHex("#FFFFC800") ,
                            ColorPickerUtils.FromHex("#FF0000FF"), 
                            ColorPickerUtils.FromHex("#FFB700C4"), 
                            ColorPickerUtils.FromHex("#FFFF0088"), 
                            ColorPickerUtils.FromHex("#FFFF0051"),
                            ColorPickerUtils.FromHex("#FFFF4229"), 
                            ColorPickerUtils.FromHex("#FFFF8C00")
                        }));
                        stops.Add(createGradientStopCollection(new Color[] { 
                            ColorPickerUtils.FromHex("#FFFF8C00"),
                            ColorPickerUtils.FromHex("#FFFF4229"), 
                            ColorPickerUtils.FromHex("#FFFF0051"),                                       
                            ColorPickerUtils.FromHex("#FFFF0088"), 
                            ColorPickerUtils.FromHex("#FFB700C4"),  
                            ColorPickerUtils.FromHex("#FF0000FF"), 
                            ColorPickerUtils.FromHex("#FFFFC800") 
                        }));
                        stops.Add(createGradientStopCollection(new Color[] { Colors.Transparent, Colors.Black, Colors.Yellow, Colors.Red, Colors.White }));
                        stops.Add(createGradientStopCollection(new Color[] { Colors.White, Colors.Red, Colors.Yellow, Colors.Black, Colors.Transparent }));
                    }
                }
            }
            return stops;
        }

        internal static List<GradientStopCollection> GradientStops
        {
            get { return GetPreDefinedGradients(); }
        }

        internal static void OverrideWithCustomGradients(List<GradientStopCollection> gradientStops)
        {
            if (gradientStops == null || gradientStops.Count < 1)
                return;
            stops = gradientStops;
        }

        private static GradientStopCollection createGradientStopCollection(Color[] colors)
        {
            GradientStopCollection stops = new GradientStopCollection();
            if (colors != null)
            {
                double[] offsets = null;
                if (colors.Length == 5)
                    offsets = new double[] { 0, 0.5, 0.75, 0.8, 1 };
                else if (colors.Length == 7)
                    offsets = new double[] { 0, 1, 0.864, 0.708, 0.53, 0.36, 0.18 };

                int i = 0;
                foreach (Color clr in colors)
                {
                    stops.Add(new GradientStop() { Color = clr, Offset = offsets[i] });
                    i++;
                }
            }
            return stops;
        }
    }
}
