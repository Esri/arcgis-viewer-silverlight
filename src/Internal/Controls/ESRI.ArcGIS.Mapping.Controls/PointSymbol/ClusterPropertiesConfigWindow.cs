/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = "BackgroundColorPicker", Type = typeof(ColorPicker))]
    [TemplatePart(Name = "ForegroundColorPicker", Type = typeof(ColorPicker))]
    [TemplatePart(Name = "BackgroundRadioButton", Type = typeof(RadioButton))]
    [TemplatePart(Name = "ForegroundRadioButton", Type = typeof(RadioButton))]
    [TemplatePart(Name = "ToleranceTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "OkButton", Type = typeof(Button))]
    [TemplatePart(Name = "CancelButton", Type = typeof(Button))]
    public class ClusterPropertiesConfigWindow : Control
    {
        ColorPicker BackgroundColorPicker = null;
        ColorPicker ForegroundColorPicker = null;
        RadioButton BackgroundRadioButton = null;
        RadioButton ForegroundRadioButton = null;
        TextBox ToleranceTextBox = null;
        Button OkButton = null;
        Button CancelButton = null;

        public ClusterPropertiesConfigWindow()
        {
            DefaultStyleKey = typeof(ClusterPropertiesConfigWindow);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            BackgroundColorPicker = GetTemplateChild("BackgroundColorPicker") as ColorPicker;
            if (BackgroundColorPicker != null)
            {
                loadBackgroundColorPicker();
                BackgroundColorPicker.ColorChanged += new EventHandler(BackgroundColorPicker_ColorChanged);
            }

            ForegroundColorPicker = GetTemplateChild("ForegroundColorPicker") as ColorPicker;
            if (ForegroundColorPicker != null)
            {
                loadForegroundColorPicker();
                ForegroundColorPicker.ColorChanged += new EventHandler(ForegroundColorPicker_ColorChanged);
            }

            BackgroundRadioButton = GetTemplateChild("BackgroundRadioButton") as RadioButton;
            if (BackgroundRadioButton != null)
            {
                BackgroundRadioButton.Checked += new RoutedEventHandler(RadioButton_Checked);
                BackgroundRadioButton.Unchecked += new RoutedEventHandler(RadioButton_UnChecked);
            }

            ForegroundRadioButton = GetTemplateChild("ForegroundRadioButton") as RadioButton;
            if (ForegroundRadioButton != null)
            {
                ForegroundRadioButton.Checked += new RoutedEventHandler(RadioButton_Checked);
                ForegroundRadioButton.Unchecked += new RoutedEventHandler(RadioButton_UnChecked);
            }

            ToleranceTextBox = GetTemplateChild("ToleranceTextBox") as TextBox;
            if (ToleranceTextBox != null)
            {
                ToleranceTextBox.SetBinding(TextBox.TextProperty, new Binding() { 
                     Mode = System.Windows.Data.BindingMode.TwoWay,
                     Path = new PropertyPath("MaximumFlareCount"),
                     Source = DataContext
                });
            }

            OkButton = GetTemplateChild("OkButton") as Button;
            if (OkButton != null)
                OkButton.Click += new RoutedEventHandler(OkButton_Click);

            CancelButton = GetTemplateChild("CancelButton") as Button;
            if(CancelButton != null)
                CancelButton.Click += new RoutedEventHandler(CancelButton_Click);
        }

        private void loadBackgroundColorPicker()
        {
            FlareClusterer clusterer = DataContext as FlareClusterer;
            if (clusterer == null)
                return;
            if (BackgroundColorPicker != null)
            {
                BackgroundColorPicker.Color = (clusterer.FlareBackground as SolidColorBrush).Color;
                BackgroundColorPicker.Hsv = BackgroundColorPicker.Color.ToHsv();
            }
        }

        private void loadForegroundColorPicker()
        {
            FlareClusterer clusterer = DataContext as FlareClusterer;
            if (clusterer == null)
                return;
            if (ForegroundColorPicker != null)
            {
                ForegroundColorPicker.Color = (clusterer.FlareForeground as SolidColorBrush).Color;
                ForegroundColorPicker.Hsv = ForegroundColorPicker.Color.ToHsv();
            }
        }

        private void BackgroundColorPicker_ColorChanged(object sender, EventArgs e)
        {
            FlareClusterer clusterer = DataContext as FlareClusterer;
            if (clusterer == null)
                return;
            ColorPicker picker = (ColorPicker)sender;
            (clusterer.FlareBackground as SolidColorBrush).Color = picker.Color;
        }       

        private void ForegroundColorPicker_ColorChanged(object sender, EventArgs e)
        {
           FlareClusterer clusterer = DataContext as FlareClusterer;
           if (clusterer == null)
               return;
           ColorPicker picker = (ColorPicker)sender;
           (clusterer.FlareForeground as SolidColorBrush).Color = picker.Color;           
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == BackgroundRadioButton)
                ForegroundRadioButton.IsChecked = false;
            else if (sender == ForegroundRadioButton)
                BackgroundRadioButton.IsChecked = false;

            if (sender == BackgroundRadioButton)
            {
                BackgroundColorPicker.Visibility = Visibility.Visible;
                ForegroundColorPicker.Visibility = Visibility.Collapsed;
            }
            else if (sender == ForegroundRadioButton)
            {
                BackgroundColorPicker.Visibility = Visibility.Collapsed;
                ForegroundColorPicker.Visibility = Visibility.Visible;
            }
        }

        private void RadioButton_UnChecked(object sender, RoutedEventArgs e)
        {
            if (sender == BackgroundRadioButton)
                ForegroundRadioButton.IsChecked = true;
            else if (sender == ForegroundRadioButton)
                BackgroundRadioButton.IsChecked = true;
        }        

        public event EventHandler OkClicked;
        public event EventHandler CancelClicked;

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
    }
}
