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
using System.Windows.Media.Imaging;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ColorSelector : Control
    {
        float m_selectedHue;
        double m_sampleX;
        double m_sampleY;
        private Color m_selectedColor = Colors.White;
        private UIElement _mouseCapture = null;
        private TextBox HexValue = null;
        private Rectangle rectHueMonitor = null;
        //private Image imgCheckerBoard = null;
        private Rectangle rectSampleMonitor = null;
        private Rectangle rectMonitor = null;
        private GradientStop displayColor = null;
        private Canvas rectMarker = null;
        private Rectangle rectSample = null;
        private Canvas HueSelector = null;
        private Canvas SampleSelector = null;
        private Rectangle SelectedColor = null;

        public Color Color
        {
            get { return m_selectedColor; }
            set
            {
                m_selectedColor = value;
                if (value != null)
                {
                    UpdateOnColorChanged(m_selectedColor.A, m_selectedColor.R, m_selectedColor.G, m_selectedColor.B, false);
                    if (HexValue != null)
                        HexValue.Text = ColorUtils.GetHexCode(value);
                }
            }
        }

        public Color InitialColor { get; set; }

        public Color DisplayColor
        {
            get { return displayColor.Color; }
            set
            {
                Color color = value;
                color.A = 255;
                if(displayColor != null)
                    displayColor.Color = color;
                UpdateSelectionForAlpha(value.A);
            }
        }

        //static WriteableBitmap _checkerboard = WriteableBitmapHelper.GetCheckerboard(20, 20);

        public ColorSelector()
        {
            DefaultStyleKey = typeof(ColorSelector);           

            m_selectedHue = 0;
            m_sampleX = 0;
            m_sampleY = 0;
            //imgCheckerBoard.Source = _checkerboard;
            //this.LayoutUpdated += new EventHandler(ColorPicker_LayoutUpdated);
        }

        public override void OnApplyTemplate()
        {
            if (HexValue != null)
                HexValue.TextChanged -= HexValue_TextChanged;

            if (rectHueMonitor != null)
            {
                rectHueMonitor.MouseLeftButtonDown -= rectHueMonitor_MouseLeftButtonDown;
                rectHueMonitor.MouseLeftButtonUp -= rectHueMonitor_MouseLeftButtonUp;
                rectHueMonitor.LostMouseCapture -= rectHueMonitor_LostMouseCapture;
                rectHueMonitor.MouseMove -= rectHueMonitor_MouseMove;
            }

            if (rectSampleMonitor != null)
            {
                rectSampleMonitor.MouseLeftButtonDown -= rectSampleMonitor_MouseLeftButtonDown;
                rectSampleMonitor.MouseLeftButtonUp -= rectSampleMonitor_MouseLeftButtonUp;
                rectSampleMonitor.LostMouseCapture -= rectSampleMonitor_LostMouseCapture;
                rectSampleMonitor.MouseMove -= rectSampleMonitor_MouseMove;
            }
            
            if (rectMonitor != null)
            {
                rectMonitor.MouseLeftButtonDown -= rectMonitor_MouseLeftButtonDown;
                rectMonitor.MouseLeftButtonUp -= rectMonitor_MouseLeftButtonUp;
                rectMonitor.MouseMove -= rectMonitor_MouseMove;
                rectMonitor.LostMouseCapture -= rectMonitor_LostMouseCapture;
            }

            base.OnApplyTemplate();

            HexValue = GetTemplateChild("HexValue") as TextBox;
            if (HexValue != null)
            {
                HexValue.TextChanged += HexValue_TextChanged;
                HexValue.Text = ColorUtils.GetHexCode(m_selectedColor);
            }

            rectHueMonitor = GetTemplateChild("rectHueMonitor") as Rectangle;
            if (rectHueMonitor != null)
            {
                rectHueMonitor.MouseLeftButtonDown += rectHueMonitor_MouseLeftButtonDown;
                rectHueMonitor.MouseLeftButtonUp += rectHueMonitor_MouseLeftButtonUp;
                rectHueMonitor.LostMouseCapture += rectHueMonitor_LostMouseCapture;
                rectHueMonitor.MouseMove += rectHueMonitor_MouseMove;
            }

            rectSampleMonitor = GetTemplateChild("rectSampleMonitor") as Rectangle;
            if (rectSampleMonitor != null)
            {
                rectSampleMonitor.MouseLeftButtonDown += rectSampleMonitor_MouseLeftButtonDown;
                rectSampleMonitor.MouseLeftButtonUp += rectSampleMonitor_MouseLeftButtonUp;
                rectSampleMonitor.LostMouseCapture += rectSampleMonitor_LostMouseCapture;
                rectSampleMonitor.MouseMove += rectSampleMonitor_MouseMove;
            }

            rectMonitor = GetTemplateChild("rectMonitor") as Rectangle;
            if (rectMonitor != null)
            {
                rectMonitor.MouseLeftButtonDown += rectMonitor_MouseLeftButtonDown;
                rectMonitor.MouseLeftButtonUp += rectMonitor_MouseLeftButtonUp;
                rectMonitor.MouseMove += rectMonitor_MouseMove;
                rectMonitor.LostMouseCapture += rectMonitor_LostMouseCapture;
            }

            displayColor = GetTemplateChild("displayColor") as GradientStop;
            rectMarker = GetTemplateChild("rectMarker") as Canvas;
            rectSample = GetTemplateChild("rectSample") as Rectangle;
            HueSelector = GetTemplateChild("HueSelector") as Canvas;
            SampleSelector = GetTemplateChild("SampleSelector") as Canvas;
            SelectedColor = GetTemplateChild("SelectedColor") as Rectangle;

            if (InitialColor != null)
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    Color = InitialColor;
                });
            }
            UpdateOnColorChanged(m_selectedColor.A, m_selectedColor.R, m_selectedColor.G, m_selectedColor.B, false);
        }        

        #region RectMonitor (Alpha)
        private void rectMonitor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            rectMonitor.CaptureMouse();
            _mouseCapture = rectMonitor;
            UpdateSelection(e.GetPosition((UIElement)sender).Y);
        }       

        private void rectMonitor_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseCapture != rectMonitor) return;
            double yPos = e.GetPosition((UIElement)sender).Y;
            if (yPos < 0) yPos = 0;
            if (yPos > rectMonitor.ActualHeight) yPos = rectMonitor.ActualHeight;
            UpdateSelection(yPos);

            OnColorSelected(m_selectedColor);
        }

        private void rectMonitor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            rectMonitor.ReleaseMouseCapture();
        }

        private void rectMonitor_LostMouseCapture(object sender, MouseEventArgs e)
        {
            _mouseCapture = null;
        }
        #endregion

        #region RectHueMonitor
        void rectHueMonitor_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            rectHueMonitor.CaptureMouse();
            _mouseCapture = rectHueMonitor;
            int yPos = (int)e.GetPosition((UIElement)sender).Y;
            UpdateSelection(yPos);

            OnColorSelected(m_selectedColor);
        }

        void rectHueMonitor_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseCapture != rectHueMonitor) return;
            int yPos = (int)e.GetPosition((UIElement)sender).Y;
            if (yPos < 0) yPos = 0;
            if (yPos >= rectHueMonitor.ActualHeight) yPos = (int)rectHueMonitor.ActualHeight - 1;            
            UpdateSelection(yPos);
            
            OnColorSelected(m_selectedColor);
        }

        void rectHueMonitor_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            rectHueMonitor.ReleaseMouseCapture();
        }
        
        void rectHueMonitor_LostMouseCapture(object sender, MouseEventArgs e)
        {
            _mouseCapture = null;
        }
        #endregion

        #region RectSampleMonitor 
        void rectSampleMonitor_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            rectSampleMonitor.CaptureMouse();
            _mouseCapture = rectSampleMonitor;
            Point pos = e.GetPosition((UIElement)sender);
            m_sampleX = (int)pos.X;
            m_sampleY = (int)pos.Y;
            UpdateSample(m_sampleX, m_sampleY);

            OnColorSelected(m_selectedColor);
        }

        void rectSampleMonitor_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseCapture != rectSampleMonitor) return;
            Point pos = e.GetPosition((UIElement)sender);
            m_sampleX = (int)pos.X;
            m_sampleY = (int)pos.Y;
            if (m_sampleY < 0) m_sampleY = 0;
            if (m_sampleY > rectSampleMonitor.ActualHeight) m_sampleY = (int)rectSampleMonitor.ActualHeight;
            if (m_sampleX < 0) m_sampleX = 0;
            if (m_sampleX > rectSampleMonitor.ActualWidth) m_sampleX = (int)rectSampleMonitor.ActualWidth;
            UpdateSample(m_sampleX, m_sampleY);

            OnColorSelected(m_selectedColor);
        }

        void rectSampleMonitor_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            rectSampleMonitor.ReleaseMouseCapture();
        }

        void rectSampleMonitor_LostMouseCapture(object sender, MouseEventArgs e)
        {
            _mouseCapture = null;
        }
        #endregion

        #region Helper Functions
        private void UpdateSelectionForAlpha(int alpha)
        {
            if (rectMarker == null || rectMonitor == null)
                return;

            Canvas.SetTop(rectMarker, ((255 - alpha) * rectMonitor.ActualHeight) / 255.0);
        }

        private void UpdateSelection(double yPos)
        {
            if (rectMarker == null || rectMonitor == null)
                return;


            byte alpha = (byte)(255 - (yPos * 255 / rectMonitor.ActualHeight));
            Canvas.SetTop(rectMarker, yPos);

            ctlAlphaSelect_AlphaChanged(alpha);
        }

        bool _firstTime = true;
        void ColorPicker_LayoutUpdated(object sender, EventArgs e)
        {
            if (_firstTime)
            {
                _firstTime = false;
                UpdateOnColorChanged(m_selectedColor.A, m_selectedColor.R, m_selectedColor.G, m_selectedColor.B);
            }
        }

        private void UpdateSample(double xPos, double yPos)
        {
            SampleSelector.Margin = new Thickness(xPos - (SampleSelector.Height / 2), yPos - (SampleSelector.Height / 2), 0, 0);

            float yComponent = 1 - (float)(yPos / rectSample.ActualHeight);
            float xComponent = (float)(xPos / rectSample.ActualWidth);

            byte a = m_selectedColor.A;
            m_selectedColor = ColorUtils.ConvertHsvToRgb((float)m_selectedHue, xComponent, yComponent);
            m_selectedColor.A = a;
            ((SolidColorBrush)SelectedColor.Fill).Color = m_selectedColor;
            HexValue.Text = ColorUtils.GetHexCode(m_selectedColor);

            DisplayColor = m_selectedColor;
        }

        private void UpdateSelection(int yPos)
        {
            int huePos = (int)(yPos / rectHueMonitor.ActualHeight * 255);
            int gradientStops = 6;
            Color c = ColorUtils.GetColorFromPosition(huePos * gradientStops);
            rectSample.Fill = new SolidColorBrush(c);
            HueSelector.Margin = new Thickness(0, yPos - (HueSelector.ActualHeight / 2), 0, 0);
            m_selectedHue = (float)(yPos / rectHueMonitor.ActualHeight) * 360;
            UpdateSample(m_sampleX, m_sampleY);

            OnColorSelected(m_selectedColor);
        }

        private void ctlAlphaSelect_AlphaChanged(byte newAlpha)
        {
            m_selectedColor.A = newAlpha;
            HexValue.Text = ColorUtils.GetHexCode(m_selectedColor);
            ((SolidColorBrush)SelectedColor.Fill).Color = m_selectedColor;

            OnColorSelected(m_selectedColor);
        }

        private void HexValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = HexValue.Text;
            if (text == ColorUtils.GetHexCode(m_selectedColor)) return;
            byte a, r, g, b;
            if (!GetArgb(text, out a, out r, out g, out b)) return; // invalid color

            UpdateOnColorChanged(a, r, g, b);
        }

        private void UpdateOnColorChanged(byte a, byte r, byte g, byte b, bool fireColorChangedEvent = true)
        {
            m_selectedColor.A = a;
            m_selectedColor.R = r;
            m_selectedColor.G = g;
            m_selectedColor.B = b;

            double h, s, v;
            ColorUtils.ConvertRgbToHsv(r / 255.0, g / 255.0, b / 255.0, out h, out s, out v);

            // update selected color
            if (SelectedColor != null)
            {
                SolidColorBrush colorBrush = SelectedColor.Fill as SolidColorBrush;
                if(colorBrush != null)
                    colorBrush.Color = m_selectedColor;
            }
            

            // update Saturation and Value locator
            if (rectSample != null)
            {
                double xPos = s * rectSample.ActualWidth;
                double yPos = (1 - v) * rectSample.ActualHeight;
                m_sampleX = xPos;
                m_sampleY = yPos;
                if (SampleSelector != null)
                    SampleSelector.Margin = new Thickness(xPos - (SampleSelector.Height / 2), yPos - (SampleSelector.Height / 2), 0, 0);
            }

            m_selectedHue = (float)h;
            h /= 360;
            const int gradientStops = 6;
            if (rectSample != null)
            {
                 SolidColorBrush colorBrush = rectSample.Fill as SolidColorBrush;
                if(colorBrush != null)
                    colorBrush.Color= ColorUtils.GetColorFromPosition(((int)(h * 255)) * gradientStops);
            } 

            // Update Hue locator
            if(HueSelector != null)
                HueSelector.Margin = new Thickness(0, (h * rectHueMonitor.ActualHeight) - (HueSelector.ActualHeight / 2), 0, 0);

            // update alpha selector
            DisplayColor = m_selectedColor;

            if(fireColorChangedEvent)
                OnColorSelected(m_selectedColor);
        }

        private bool GetArgb(string hexColor, out byte a, out byte r, out byte g, out byte b)
        {
            a = r = b = g = 0;
            if (hexColor.Length != 9) return false;
            string strA = hexColor.Substring(1, 2);
            string strR = hexColor.Substring(3, 2);
            string strG = hexColor.Substring(5, 2);
            string strB = hexColor.Substring(7, 2);

            if (!byte.TryParse(strA, System.Globalization.NumberStyles.HexNumber, null, out a)) return false;
            if (!byte.TryParse(strR, System.Globalization.NumberStyles.HexNumber, null, out r)) return false;
            if (!byte.TryParse(strG, System.Globalization.NumberStyles.HexNumber, null, out g)) return false;
            if (!byte.TryParse(strB, System.Globalization.NumberStyles.HexNumber, null, out b)) return false;
            return true;
        }
        #endregion

        #region Events
        protected virtual void OnColorSelected(Color newColor)
        {
            if (ColorSelected != null)
                ColorSelected(newColor);
        }

        public delegate void ColorSelectedHandler(Color c);
        public event ColorSelectedHandler ColorSelected;

        #endregion
    }

    internal class ColorUtils
    {
        private const byte MIN = 0;
        private const byte MAX = 255;

        public static Color GetColorFromPosition(int position)
        {
            byte mod = (byte)(position % MAX);
            byte diff = (byte)(MAX - mod);
            byte alpha = 255;

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

        public static string GetHexCode(Color c)
        {
            return string.Format("#{0}{1}{2}{3}",
                c.A.ToString("X2"),
                c.R.ToString("X2"),
                c.G.ToString("X2"),
                c.B.ToString("X2"));
        }

        public static void ConvertRgbToHsv(double r, double g, double b, out double h, out double s, out double v)
        {
            double colorMax = Math.Max(Math.Max(r, g), b);

            v = colorMax;
            if (v == 0)
            {
                h = 0;
                s = 0;
                return;
            }

            // normalize
            r /= v;
            g /= v;
            b /= v;

            double colorMin = Math.Min(Math.Min(r, g), b);
            colorMax = Math.Max(Math.Max(r, g), b);

            s = colorMax - colorMin;
            if (s == 0)
            {
                h = 0;
                return;
            }

            // normalize saturation
            r = (r - colorMin) / s;
            g = (g - colorMin) / s;
            b = (b - colorMin) / s;
            colorMax = Math.Max(Math.Max(r, g), b);

            // calculate hue
            if (colorMax == r)
            {
                h = 0.0 + 60.0 * (g - b);
                if (h < 0.0)
                {
                    h += 360.0;
                }
            }
            else if (colorMax == g)
            {
                h = 120.0 + 60.0 * (b - r);
            }
            else // colorMax == b
            {
                h = 240.0 + 60.0 * (r - g);
            }

        }

        // Algorithm ported from: http://www.colorjack.com/software/dhtml+color+picker.html
        public static Color ConvertHsvToRgb(float h, float s, float v)
        {
            h = h / 360;
            if (s > 0)
            {
                if (h >= 1)
                    h = 0;
                h = 6 * h;
                int hueFloor = (int)Math.Floor(h);
                byte a = (byte)Math.Round(MAX * v * (1.0 - s));
                byte b = (byte)Math.Round(MAX * v * (1.0 - (s * (h - hueFloor))));
                byte c = (byte)Math.Round(MAX * v * (1.0 - (s * (1.0 - (h - hueFloor)))));
                byte d = (byte)Math.Round(MAX * v);

                switch (hueFloor)
                {
                    case 0: return Color.FromArgb(MAX, d, c, a);
                    case 1: return Color.FromArgb(MAX, b, d, a);
                    case 2: return Color.FromArgb(MAX, a, d, c);
                    case 3: return Color.FromArgb(MAX, a, b, d);
                    case 4: return Color.FromArgb(MAX, c, a, d);
                    case 5: return Color.FromArgb(MAX, d, a, b);
                    default: return Color.FromArgb(0, 0, 0, 0);
                }
            }
            else
            {
                byte d = (byte)(v * MAX);
                return Color.FromArgb(255, d, d, d);
            }
        }
    }

    internal static class WriteableBitmapHelper
    {
        public static WriteableBitmap GetCheckerboard(int width, int height)
        {
            WriteableBitmap bitmap = new WriteableBitmap(width, height);
            int[] pixels = bitmap.Pixels;

            int COLOR_1 = (255 << 24) + (255 << 16) + (255 << 8) + (255);
            int COLOR_2 = (255 << 24) + (190 << 16) + (190 << 8) + (190);

            // fill the first line
            for (int x = 0; x < width; x++)
            {
                if ((x & 8) == 0) pixels[x] = COLOR_1;
                else pixels[x] = COLOR_2;
            }

            // fill the first line of the pattern, if needed
            int offset2 = 0;
            if (height >= 8)
            {
                int offset = width << 3;
                offset2 = offset;
                for (int x = 0; x < width; x++)
                {
                    if ((x & 8) == 0) pixels[offset] = COLOR_2;
                    else pixels[offset] = COLOR_1;
                    offset++;
                }
            }

            int yOffset = 0;
            for (int y = 0; y < height; y++)
            {
                if ((y & 8) == 0) Array.Copy(pixels, 0, pixels, yOffset, width);
                else Array.Copy(pixels, offset2, pixels, yOffset, width);
                yOffset += width;
            }

            return bitmap;
        } 
    }
}
