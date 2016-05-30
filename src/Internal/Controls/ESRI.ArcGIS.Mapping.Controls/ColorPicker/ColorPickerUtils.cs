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
    internal class ColorPickerUtils
    {
        private const byte MIN = 0;
        private const byte MAX = 255;

        internal static Color GetColorFromPosition(byte alpha, int position)
        {
            byte mod = (byte)(position % MAX);
            byte diff = (byte)(MAX - mod);

            switch (position / MAX)
            {
                case 0: return Color.FromArgb(alpha, MAX, mod, MIN);
                case 1: return Color.FromArgb(alpha, diff, MAX, MIN);
                case 2: return Color.FromArgb(alpha, MIN, MAX, mod);
                case 3: return Color.FromArgb(alpha, MIN, diff, MAX);
                case 4: return Color.FromArgb(alpha, mod, MIN, MAX);
                case 5: return Color.FromArgb(alpha, MAX, MIN, diff);
                default: return Colors.Black;
            }
        }

        internal static string GetHexCode(Color c)
        {
            return c.ToString();
            //return string.Format("#{0}{1}{2}",
            //    c.R.ToString("X2"),
            //    c.G.ToString("X2"),
            //    c.B.ToString("X2"));
        }

        // Algorithm ported from: http://www.colorjack.com/software/dhtml+color+picker.html
        internal static Color ConvertHsvToRgb(byte alpha, float h, float s, float v)
        {
            h = h / 360;
            if (s > 0)
            {
                if (h >= 1)
                    h = 0;
                h = 6 * h;
                int hueFloor = (int)Math.Floor(h);
                byte a = (byte)Math.Round(alpha * v * (1.0 - s));
                byte b = (byte)Math.Round(alpha * v * (1.0 - (s * (h - hueFloor))));
                byte c = (byte)Math.Round(alpha * v * (1.0 - (s * (1.0 - (h - hueFloor)))));
                byte d = (byte)Math.Round(alpha * v);

                switch (hueFloor)
                {
                    case 0: return Color.FromArgb(alpha, d, c, a);
                    case 1: return Color.FromArgb(alpha, b, d, a);
                    case 2: return Color.FromArgb(alpha, a, d, c);
                    case 3: return Color.FromArgb(alpha, a, b, d);
                    case 4: return Color.FromArgb(alpha, c, a, d);
                    case 5: return Color.FromArgb(alpha, d, a, b);
                    default: return Color.FromArgb(0, 0, 0, 0);
                }
            }
            else
            {
                byte d = (byte)(v * alpha);
                return Color.FromArgb(alpha, d, d, d);
            }
        }

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

    public struct Hsv
    {
        public double Hue;
        public double Saturation;
        public double Value;
    }

    public static class ColorExtensions
    {
        public class HslColor
        {
            public float Hue { get; set; }
            public float Saturation { get; set; }
            public float Lightness { get; set; }
        }
        public class HsvColor
        {
            public float H { get; set; }
            public float S { get; set; }
            public float V { get; set; }
        }

        public static Hsv ToHsv(this Color color)
        {
            HsvColor h = AsHsv(color);
            return new Hsv() { Hue = h.H,  Saturation = h.S, Value = h.V };
        }

        public static HsvColor AsHsv(this Color color)
        {
            float max = (float)Math.Max(Math.Max(color.R, color.G), color.B) / 255f;
            float min = (float)Math.Min(Math.Min(color.R, color.G), color.B) / 255f;

            float hue = color.GetHue();
            float sat = 0;
            if (max != 0)
            {
                sat = 1f - min / max;
            }
            float v = max;
            return new HsvColor() { H = hue, S = sat, V = v };
        }


        public static HslColor AsHsl(this Color color)
        {
            return new HslColor() { Hue = GetHue(color), Lightness = color.GetBrightness(), Saturation = color.GetSaturation() };
        }

        public static float GetBrightness(this Color color)
        {
            float num = ((float)color.R) / 255f;
            float num2 = ((float)color.G) / 255f;
            float num3 = ((float)color.B) / 255f;
            float num4 = num;
            float num5 = num;
            if (num2 > num4)
            {
                num4 = num2;
            }
            if (num3 > num4)
            {
                num4 = num3;
            }
            if (num2 < num5)
            {
                num5 = num2;
            }
            if (num3 < num5)
            {
                num5 = num3;
            }
            return ((num4 + num5) / 2f);
        }

        public static float GetHue(this Color color)
        {
            if ((color.R == color.G) && (color.G == color.B))
            {
                return 0f;
            }
            float num = ((float)color.R) / 255f;
            float num2 = ((float)color.G) / 255f;
            float num3 = ((float)color.B) / 255f;
            float num7 = 0f;
            float num4 = num;
            float num5 = num;
            if (num2 > num4)
            {
                num4 = num2;
            }
            if (num3 > num4)
            {
                num4 = num3;
            }
            if (num2 < num5)
            {
                num5 = num2;
            }
            if (num3 < num5)
            {
                num5 = num3;
            }
            float num6 = num4 - num5;
            if (num == num4)
            {
                num7 = (num2 - num3) / num6;
            }
            else if (num2 == num4)
            {
                num7 = 2f + ((num3 - num) / num6);
            }
            else if (num3 == num4)
            {
                num7 = 4f + ((num - num2) / num6);
            }
            num7 *= 60f;
            if (num7 < 0f)
            {
                num7 += 360f;
            }
            return num7;
        }

        public static float GetSaturation(this Color color)
        {
            float num = ((float)color.R) / 255f;
            float num2 = ((float)color.G) / 255f;
            float num3 = ((float)color.B) / 255f;
            float num7 = 0f;
            float num4 = num;
            float num5 = num;
            if (num2 > num4)
            {
                num4 = num2;
            }
            if (num3 > num4)
            {
                num4 = num3;
            }
            if (num2 < num5)
            {
                num5 = num2;
            }
            if (num3 < num5)
            {
                num5 = num3;
            }
            if (num4 == num5)
            {
                return num7;
            }
            float num6 = (num4 + num5) / 2f;
            if (num6 <= 0.5)
            {
                return ((num4 - num5) / (num4 + num5));
            }
            return ((num4 - num5) / ((2f - num4) - num5));
        }
    }
}
