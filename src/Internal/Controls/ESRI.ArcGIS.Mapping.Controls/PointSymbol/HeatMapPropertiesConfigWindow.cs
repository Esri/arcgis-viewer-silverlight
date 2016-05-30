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
using System.Collections.Generic;
using System.Windows.Data;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name="ColorGradientsListBox", Type=typeof(ListBox))]
    [TemplatePart(Name="OKButton", Type=typeof(Button))]
    [TemplatePart(Name="CancelButton", Type=typeof(Button))]
    public class HeatMapPropertiesConfigWindow : Control
    {
        ListBox ColorGradientsListBox = null;
        Button CancelButton = null;
        Button OKButton = null;
        Slider IntensitySlider = null;
        Slider ResolutionSlider = null;

        public HeatMapPropertiesConfigWindow()
        {
            DefaultStyleKey = typeof(HeatMapPropertiesConfigWindow);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ColorGradientsListBox = GetTemplateChild("ColorGradientsListBox") as ListBox;
            if (ColorGradientsListBox != null)
            {
                loadColorGradientsListBox();
                ColorGradientsListBox.SelectionChanged += new SelectionChangedEventHandler(onListBoxSelectionChanged);
            }

            CancelButton = GetTemplateChild("CancelButton") as Button;
            if(CancelButton != null)
                CancelButton.Click += new RoutedEventHandler(CancelButton_Click);

            OKButton = GetTemplateChild("OKButton") as Button;
            if(OKButton != null)
                OKButton.Click += new RoutedEventHandler(OkButton_Click);

            IntensitySlider = GetTemplateChild("IntensitySlider") as Slider;
            if (IntensitySlider != null)
            {
                IntensitySlider.SetBinding(Slider.ValueProperty, new Binding()
                {
                    Mode = BindingMode.TwoWay,
                    Source = DataContext,
                    Path = new PropertyPath("Intensity")
                });
            }

            ResolutionSlider = GetTemplateChild("ResolutionSlider") as Slider;
            if (ResolutionSlider != null)
            {
                ResolutionSlider.SetBinding(Slider.ValueProperty, new Binding()
                {
                    Mode = BindingMode.TwoWay,
                    Source = DataContext,
                    Path = new PropertyPath("Resolution")
                });
            }
        }        

        private bool _fireSelectionChangedEvent = true;
        private void onListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_fireSelectionChangedEvent)
                return;
            ListBoxItem item = ColorGradientsListBox.SelectedItem as ListBoxItem;
            if (item == null)
                return;
            Rectangle rectangle = item.Content as Rectangle;
            if (rectangle == null)
                return;
            LinearGradientBrush brush = rectangle.Fill as LinearGradientBrush;
            if (brush == null)
                return;
            if (GradientStopsChanged != null)
                GradientStopsChanged(this, new GradientStopsChangedEventArgs() { GradientStops = ToCollection(brush.GradientStops) });
        }        

        public event EventHandler OkClicked;
        public event EventHandler CancelClicked;
        public event EventHandler<GradientStopsChangedEventArgs> GradientStopsChanged;

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (OkClicked != null)
                OkClicked(this, EventArgs.Empty);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (CancelClicked != null)
                CancelClicked(this, EventArgs.Empty);
        }

        private void loadColorGradientsListBox()
        {
            if (ColorGradientsListBox == null)
                return;
            if (ColorGradientsListBox.Items.Count == 0)
            {
                GradientStopCollection stopInfos = null;
                ConfigurableHeatMapLayer layerInfo = DataContext as ConfigurableHeatMapLayer;
                if (layerInfo != null)
                    stopInfos = layerInfo.Gradient;
                bool foundSelectedItem = false;
                foreach (GradientStopCollection stops in PreDefinedGradients.GradientStops)
                {
                    bool isSelectedItem = false;
                    if (!foundSelectedItem)
                        isSelectedItem = isMatchingGradientStopCollection(stopInfos, stops);
                    ListBoxItem item = new ListBoxItem()
                    {
                        Content = createColorGradientRectangle(stops),
                        IsSelected = isSelectedItem
                    };
                    if (isSelectedItem)
                    {
                        ColorGradientsListBox.SelectedItem = item;
                        foundSelectedItem = true;
                    }
                    ColorGradientsListBox.Items.Add(item);
                }
            }
            else
            {
                setSelectedGradient();
            }
        }

        private bool isMatchingGradientStopCollection(GradientStopCollection stopInfos, GradientStopCollection stops)
        {
            if (stopInfos == null || stops == null || stopInfos.Count != stops.Count)
                return false;
            int i = 0;
            foreach (GradientStop stopInfo in stopInfos)
            {
                GradientStop stop = stops[i];
                if (Math.Round(stop.Offset, 2) != Math.Round(stopInfo.Offset, 2))
                    return false;
                if (stop.Color != stopInfo.Color)
                    return false;
                i++;
            }
            return true;
        }

        private Rectangle createColorGradientRectangle(GradientStopCollection coll)
        {
            Rectangle rect = new Rectangle()
            {
                Width = 165,
                Height = 20,
                VerticalAlignment = VerticalAlignment.Stretch,
                Stroke = new SolidColorBrush(Colors.Black),
                Margin = new Thickness(3, 1, 3, 1),
                StrokeThickness = 1,
                Fill = new LinearGradientBrush() { GradientStops = coll },
            };
            return rect;
        }

        private void setSelectedGradient()
        {
            if (ColorGradientsListBox != null)
            {
                GradientStopCollection stopInfos = null;
                ConfigurableHeatMapLayer layerInfo = DataContext as ConfigurableHeatMapLayer;
                if (layerInfo != null)
                    stopInfos = layerInfo.Gradient;
                foreach (ListBoxItem item in ColorGradientsListBox.Items)
                {
                    Rectangle rectangle = item.Content as Rectangle;
                    if (rectangle == null)
                        continue;
                    LinearGradientBrush brush = rectangle.Fill as LinearGradientBrush;
                    if (brush == null)
                        continue;
                    if (isMatchingGradientStopCollection(stopInfos, brush.GradientStops))
                    {
                        _fireSelectionChangedEvent = false;
                        ColorGradientsListBox.SelectedItem = item;
                        _fireSelectionChangedEvent = true;
                        break;
                    }
                }
            }
        }
        
        private List<GradientStopInfo> ToCollection(GradientStopCollection stops)
        {
            if (stops == null)
                return null;
            List<GradientStopInfo> coll = new List<GradientStopInfo>();
            foreach (GradientStop stop in stops)
            {
                coll.Add(new GradientStopInfo() { Color = ColorPickerUtils.GetHexCode(stop.Color), Offset = stop.Offset });
            }
            return coll;
        }    
    }

    public class GradientStopsChangedEventArgs : EventArgs
    {
        public List<GradientStopInfo> GradientStops { get; set; }
    }

    public class GradientStopInfo
    {
        public string Color { get; set; }
        public double Offset { get; set; }
    }

    public class GradientStopsBrushConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            GradientStopCollection coll = value as GradientStopCollection;
            if (coll != null)
                return new LinearGradientBrush() { GradientStops = coll };
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Not supported");
        }

        #endregion
    }
}
