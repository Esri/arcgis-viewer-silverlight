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
using System.ComponentModel;
using System.Windows.Threading;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = "Hue", Type = typeof(Rectangle))]
    [TemplatePart(Name = "SelectedColor", Type = typeof(Rectangle))]
    [TemplatePart(Name = "HueMonitor", Type = typeof(Rectangle))]
    [TemplatePart(Name = "BlackGradient", Type = typeof(Rectangle))]
    [TemplatePart(Name = "SampleMonitor", Type = typeof(Rectangle))]
    [TemplatePart(Name = "Sample", Type = typeof(Rectangle))]
    [TemplatePart(Name = "SelectedColorCanvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "SampleSelector", Type = typeof(Canvas))]
    [TemplatePart(Name = "HueSelector", Type = typeof(Canvas))]
    public class ColorPicker : Control, INotifyPropertyChanged
    {
        Rectangle Hue = null;
        Rectangle SelectedColor = null;
        Rectangle HueMonitor = null;
        Rectangle BlackGradient = null;
        Rectangle SampleMonitor = null;
        Rectangle Sample = null;
        Rectangle WhiteGradient = null;
        Canvas SelectedColorCanvas = null;
        Canvas SampleSelector = null;
        Canvas HueSelector = null;

        bool m_sliderMouseDown;
        bool m_sampleMouseDown;
        float m_selectedHue;
        int m_sampleX;
        int m_sampleY;

        private Color _selectedColor;
        private DispatcherTimer _colorChangedTimer;

        public event EventHandler ColorChanged;
        protected void OnColorChanged(EventArgs e)
        {
            if (ColorChanged != null)
                ColorChanged(this, e);
        }

        public ColorPicker()
        {
            DefaultStyleKey = typeof(ColorPicker);

            _bindableColor = new BindableColor();

            _colorChangedTimer = new DispatcherTimer();
            _colorChangedTimer.Interval = new TimeSpan(0, 0, 0, 1);
            _colorChangedTimer.Tick += (o, e) =>
            {
                _colorChangedTimer.Stop();
                OnColorChanged(EventArgs.Empty);
            };

            _hsv.Hue = -1;
            _hsv.Saturation = -1;
            _hsv.Value = -1;

            m_selectedHue = 0;            
            m_sampleY = 0;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Hue = GetTemplateChild("Hue") as Rectangle;
            SelectedColor = GetTemplateChild("SelectedColor") as Rectangle;
            HueMonitor = GetTemplateChild("HueMonitor") as Rectangle;
            BlackGradient = GetTemplateChild("BlackGradient") as Rectangle;
            SampleMonitor = GetTemplateChild("SampleMonitor") as Rectangle;
            Sample = GetTemplateChild("Sample") as Rectangle;
            WhiteGradient = GetTemplateChild("WhiteGradient") as Rectangle;
            SelectedColorCanvas = GetTemplateChild("SelectedColorCanvas") as Canvas;
            SampleSelector = GetTemplateChild("SampleSelector") as Canvas;
            HueSelector = GetTemplateChild("HueSelector") as Canvas;

            attachEventHandlers();

            if (SampleMonitor != null)
                m_sampleX = (int)SampleMonitor.Width;
            UpdateSample(m_sampleX, m_sampleY, false);
        }

        public event EventHandler ColorChanging;
        protected void OnColorChanging(EventArgs e)
        {
            if (ColorChanging != null)
                ColorChanging(this, e);

            if (_colorChangedTimer.IsEnabled)
                _colorChangedTimer.Stop();

            _colorChangedTimer.Start();
        }

        public double Size
        {
            get { return SelectedColor.Width; }
            set
            {
                if (value > 20)
                {
                    double size = !double.IsNaN(MaxSize) && MaxSize > 0 ? Math.Min(value, MaxSize) : value;
                    double newSize = size - 20;
                    Hue.Height = HueMonitor.Height = BlackGradient.Height =
                        BlackGradient.Width = SampleMonitor.Height = SampleMonitor.Width =
                        Sample.Width = Sample.Height = WhiteGradient.Width =
                        WhiteGradient.Height = newSize;
                    if(SelectedColor != null)
                        SelectedColor.Width = size;
                    if(SelectedColorCanvas != null)
                        Canvas.SetTop(SelectedColorCanvas, size - 20);
                    UpdatePosition();
                }
            }
        }

        public double MaxSize { get; set; }

        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(double), typeof(ColorPicker),
            new PropertyMetadata((double)400));


        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(ColorPicker), null);       
        

        private void attachEventHandlers()
        {
            if (HueMonitor != null)
            {
                HueMonitor.MouseLeftButtonDown += new MouseButtonEventHandler(rectHueMonitor_MouseLeftButtonDown);
                HueMonitor.MouseLeftButtonUp += new MouseButtonEventHandler(rectHueMonitor_MouseLeftButtonUp);
                HueMonitor.MouseLeave += new MouseEventHandler(rectHueMonitor_MouseLeave);
                HueMonitor.MouseMove += new MouseEventHandler(rectHueMonitor_MouseMove);
            }

            if (SampleMonitor != null)
            {
                SampleMonitor.MouseLeftButtonDown += new MouseButtonEventHandler(rectSampleMonitor_MouseLeftButtonDown);
                SampleMonitor.MouseLeftButtonUp += new MouseButtonEventHandler(rectSampleMonitor_MouseLeftButtonUp);
                SampleMonitor.MouseLeave += new MouseEventHandler(rectSampleMonitor_MouseLeave);
                SampleMonitor.MouseMove += new MouseEventHandler(rectSampleMonitor_MouseMove);
            }
        }

        void rectHueMonitor_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            m_sliderMouseDown = true;
            int yPos = (int)e.GetPosition((UIElement)sender).Y;
            UpdateSelection(yPos, true);
        }

        public Color Color
        {
            get
            {
                return _selectedColor;
            }
            set
            {
                if (_selectedColor != value)
                {
                    _selectedColor = value;
                    if(SelectedColor != null)
                        SelectedColor.Fill = new SolidColorBrush(Color);

                    _bindableColor.Color = value;

                    OnPropertyChanged(new PropertyChangedEventArgs("Color"));
                }
            }
        }

        private Hsv _hsv = new Hsv();
        public Hsv Hsv
        {
            get { return _hsv; }
            set
            {
                Hsv newHsv = (Hsv)value;
                if ((_hsv.Hue >= 0 && _hsv.Saturation >= 0 && _hsv.Value >= 0) &&
                    (_hsv.Hue != newHsv.Hue ||
                    _hsv.Saturation != newHsv.Saturation ||
                    _hsv.Value != newHsv.Value))
                {
                    _hsv = value;
                    UpdatePosition();
                }
            }
        }

        internal void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, e);
        }

        BindableColor _bindableColor;
        public BindableColor BindableColor
        {
            get { return _bindableColor; }
        }

        void rectHueMonitor_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            m_sliderMouseDown = false;
        }

        void rectHueMonitor_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_sliderMouseDown)
            {
                int yPos = (int)e.GetPosition((UIElement)sender).Y;
                UpdateSelection(yPos, true);
            }
        }

        void rectHueMonitor_MouseLeave(object sender, EventArgs e)
        {
            m_sliderMouseDown = false;
        }

        void rectSampleMonitor_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            m_sampleMouseDown = true;
            Point pos = e.GetPosition((UIElement)sender);
            m_sampleX = (int)pos.X;
            m_sampleY = (int)pos.Y;
            UpdateSample(m_sampleX, m_sampleY, true);
        }

        void rectSampleMonitor_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            m_sampleMouseDown = false;
        }

        void rectSampleMonitor_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_sampleMouseDown)
            {
                Point pos = e.GetPosition((UIElement)sender);
                m_sampleX = (int)pos.X;
                m_sampleY = (int)pos.Y;
                UpdateSample(m_sampleX, m_sampleY, true);
            }
        }

        void rectSampleMonitor_MouseLeave(object sender, EventArgs e)
        {
            m_sampleMouseDown = false;
        }

        private void UpdateSample(int xPos, int yPos, bool fireColorChanged)
        {
            if (SampleSelector != null)
            {
                SampleSelector.SetValue(Canvas.LeftProperty, xPos - (SampleSelector.Height / 2));
                SampleSelector.SetValue(Canvas.TopProperty, yPos - (SampleSelector.Height / 2));
            }

            float yComponent = 1 - (float)(yPos / Sample.Height);
            float xComponent = (float)(xPos / Sample.Width);

            byte alpha = (byte)255;

            Color = ColorPickerUtils.ConvertHsvToRgb(alpha, (float)m_selectedHue, xComponent, yComponent);

            _hsv = new Hsv() { Hue = m_selectedHue, Saturation = xComponent, Value = yComponent };

            if (fireColorChanged)
                OnColorChanging(EventArgs.Empty);
        }

        private void UpdatePosition()
        {
            if (Sample != null)
            {
                m_sampleY = (int)(Sample.Height * (1 - _hsv.Value));
                m_sampleX = (int)(Sample.Width * _hsv.Saturation);
            }

            if (HueMonitor != null)
            {
                double huePos = HueMonitor.Height * (_hsv.Hue / 360);
                UpdateSelection((int)huePos, false);
            }
        }

        internal void Refresh()
        {
            ColorExtensions.HsvColor hsvColor = Color.AsHsv();
            _hsv.Hue = hsvColor.H;
            _hsv.Saturation = hsvColor.S;
            _hsv.Value = hsvColor.V;
            UpdatePosition();
        }

        private void UpdateSelection(int yPos, bool fireColorChanged)
        {
            int huePos = (int)(yPos / HueMonitor.Height * 255);
            int gradientStops = 6;
            byte alpha = (byte)255;
            Color c = ColorPickerUtils.GetColorFromPosition(alpha, huePos * gradientStops);
            Sample.Fill = new SolidColorBrush(c);
            HueSelector.SetValue(Canvas.TopProperty, yPos - (HueSelector.Height / 2));
            m_selectedHue = (float)(yPos / HueMonitor.Height) * 360;
            UpdateSample(m_sampleX, m_sampleY, fireColorChanged);
        }

        internal static Color FromHex(string newColor)
        {
            return ColorPickerUtils.FromHex(newColor);
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }

    public class BindableColor : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        private Color _color;
        public Color Color
        {
            get { return _color; }
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Color"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, e);
        }

        #endregion
    }
}
