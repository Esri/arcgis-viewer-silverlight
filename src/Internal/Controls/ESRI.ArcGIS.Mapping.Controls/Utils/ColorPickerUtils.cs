/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ColorPickerUtils
    {
        internal static Color FromHex(string newColor)
        {
            byte position = 0;
            newColor = newColor.Replace("#", "");
            byte alpha = System.Convert.ToByte("ff", 16);

            if (newColor.Length == 8)
            {
                // get the alpha channel value
                alpha = System.Convert.ToByte(newColor.Substring(position, 2), 16);
                position = 2;
            }

            // get the red value
            byte red = System.Convert.ToByte(newColor.Substring(position, 2), 16);
            position += 2;

            // get the green value
            byte green = System.Convert.ToByte(newColor.Substring(position, 2), 16);
            position += 2;

            // get the blue value
            byte blue = System.Convert.ToByte(newColor.Substring(position, 2), 16);

            // create the SolidColorBrush object
            return Color.FromArgb(alpha, red, green, blue);
        }

    }
}
