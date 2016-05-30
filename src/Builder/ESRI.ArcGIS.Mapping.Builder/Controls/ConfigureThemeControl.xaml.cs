/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class ConfigureThemeControl : UserControl
    {
        Core.ApplicationColorSet _applicationColorSet;

        public ConfigureThemeControl()
        {
            InitializeComponent();
        }

        public void SetThemeColorsToView()
        {
            ColorPalette.ThemeColors = View.Instance.ThemeColors;
            _applicationColorSet = new Core.ApplicationColorSet();
            Core.ApplicationColorSet set = View.Instance.ApplicationColorSet;
            
            _applicationColorSet.AccentColor = Color.FromArgb(set.AccentColor.A,set.AccentColor.R,set.AccentColor.G,set.AccentColor.B);
            _applicationColorSet.AccentTextColor = Color.FromArgb(set.AccentTextColor.A,set.AccentTextColor.R,set.AccentTextColor.G,set.AccentTextColor.B);
            _applicationColorSet.BackgroundEndGradientColor = Color.FromArgb(set.BackgroundEndGradientColor.A, set.BackgroundEndGradientColor.R, set.BackgroundEndGradientColor.G, set.BackgroundEndGradientColor.B);
            _applicationColorSet.BackgroundStartGradientColor = Color.FromArgb(set.BackgroundStartGradientColor.A, set.BackgroundStartGradientColor.R, set.BackgroundStartGradientColor.G, set.BackgroundStartGradientColor.B);
            _applicationColorSet.BackgroundTextColor = Color.FromArgb(set.BackgroundTextColor.A, set.BackgroundTextColor.R, set.BackgroundTextColor.G, set.BackgroundTextColor.B);
            _applicationColorSet.SelectionColor = Color.FromArgb(set.SelectionColor.A, set.SelectionColor.R, set.SelectionColor.G, set.SelectionColor.B);
            _applicationColorSet.SelectionOutlineColor = Color.FromArgb(set.SelectionOutlineColor.A, set.SelectionOutlineColor.R, set.SelectionOutlineColor.G, set.SelectionOutlineColor.B);
            
        }

        public void Cancel()
        {
            View.Instance.ApplicationColorSet.AccentColor = Color.FromArgb(_applicationColorSet.AccentColor.A, _applicationColorSet.AccentColor.R, _applicationColorSet.AccentColor.G, _applicationColorSet.AccentColor.B);
            View.Instance.ApplicationColorSet.AccentTextColor = Color.FromArgb(_applicationColorSet.AccentTextColor.A, _applicationColorSet.AccentTextColor.R, _applicationColorSet.AccentTextColor.G, _applicationColorSet.AccentTextColor.B);
            View.Instance.ApplicationColorSet.BackgroundEndGradientColor = Color.FromArgb(_applicationColorSet.BackgroundEndGradientColor.A, _applicationColorSet.BackgroundEndGradientColor.R, _applicationColorSet.BackgroundEndGradientColor.G, _applicationColorSet.BackgroundEndGradientColor.B);
            View.Instance.ApplicationColorSet.BackgroundStartGradientColor = Color.FromArgb(_applicationColorSet.BackgroundStartGradientColor.A, _applicationColorSet.BackgroundStartGradientColor.R, _applicationColorSet.BackgroundStartGradientColor.G, _applicationColorSet.BackgroundStartGradientColor.B);
            View.Instance.ApplicationColorSet.BackgroundTextColor = Color.FromArgb(_applicationColorSet.BackgroundTextColor.A, _applicationColorSet.BackgroundTextColor.R, _applicationColorSet.BackgroundTextColor.G, _applicationColorSet.BackgroundTextColor.B);
            View.Instance.ApplicationColorSet.SelectionColor = Color.FromArgb(_applicationColorSet.SelectionColor.A, _applicationColorSet.SelectionColor.R, _applicationColorSet.SelectionColor.G, _applicationColorSet.SelectionColor.B);
            View.Instance.ApplicationColorSet.SelectionOutlineColor = Color.FromArgb(_applicationColorSet.SelectionOutlineColor.A, _applicationColorSet.SelectionOutlineColor.R, _applicationColorSet.SelectionOutlineColor.G, _applicationColorSet.SelectionOutlineColor.B);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
                Cancelled(this, EventArgs.Empty);
        }

        private void ConfigureThemeControl_Loaded(object sender, RoutedEventArgs e)
        {   
            if (ColorOptionComboBox.SelectedIndex == -1)
                ColorOptionComboBox.SelectedIndex = 0;
        }

        private void ColorOptionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem selectedItem = ColorOptionComboBox.SelectedItem as ListBoxItem;
            if(selectedItem == null)
                return;
            string colorOption = selectedItem.Tag as string;
            if(string.IsNullOrEmpty(colorOption))
                return;

            if(colorOption == "BackgroundColor")
            {
                ColorPaletteContainer.Visibility = System.Windows.Visibility.Collapsed;
                BackgroundColorPicker.BackgroundStart = new SolidColorBrush(View.Instance.ApplicationColorSet.BackgroundStartGradientColor);
                BackgroundColorPicker.BackgroundEnd = new SolidColorBrush(View.Instance.ApplicationColorSet.BackgroundEndGradientColor);
                BackgroundColorPicker.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {                
                BackgroundColorPicker.Visibility = System.Windows.Visibility.Collapsed;
                    ThemeColorProperty property = (ThemeColorProperty)Enum.Parse(typeof(ThemeColorProperty), colorOption, true);
                    switch (property)
                    {
                        case ThemeColorProperty.AccentColor:
                            ColorPalette.SelectedColorBrush = new SolidColorBrush(View.Instance.ApplicationColorSet.AccentColor);
                            break;
                        case ThemeColorProperty.AccentTextColor:
                            ColorPalette.SelectedColorBrush = new SolidColorBrush(View.Instance.ApplicationColorSet.AccentTextColor);
                            break;
                        case ThemeColorProperty.BackgroundTextColor:
                            ColorPalette.SelectedColorBrush = new SolidColorBrush(View.Instance.ApplicationColorSet.BackgroundTextColor);
                            break;
                        case ThemeColorProperty.SelectionColor:
                            ColorPalette.SelectedColorBrush = new SolidColorBrush(View.Instance.ApplicationColorSet.SelectionColor);
                            break;
                        case ThemeColorProperty.SelectionOutlineColor:
                            ColorPalette.SelectedColorBrush = new SolidColorBrush(View.Instance.ApplicationColorSet.SelectionOutlineColor);
                            break;
                    }
                ColorPaletteContainer.Visibility = System.Windows.Visibility.Visible;
            }
        }

        void ColorPalette_ColorPicked(object sender, ColorChosenEventArgs e)
        {
            ListBoxItem selectedItem = ColorOptionComboBox.SelectedItem as ListBoxItem;
            if(selectedItem == null)
                return;
            string colorOption = selectedItem.Tag as string;
            if(string.IsNullOrEmpty(colorOption))
                return;

            ThemeColorProperty property = (ThemeColorProperty)Enum.Parse(typeof(ThemeColorProperty), colorOption, true);
            ThemeColorHelper.ApplyColorProperty(View.Instance.ApplicationColorSet, e.Color, property);
        }

        private void BackgroundColorPicker_ColorChanged(object sender, EventArgs e)
        {
            if (View.Instance.ApplicationColorSet == null)
                return;

            if(BackgroundColorPicker.BackgroundStart != null)
                ThemeColorHelper.ApplyColorProperty(View.Instance.ApplicationColorSet, BackgroundColorPicker.BackgroundStart.Color, ThemeColorProperty.BackgroundStartGradientColor);

            if(BackgroundColorPicker.BackgroundEnd != null)
                ThemeColorHelper.ApplyColorProperty(View.Instance.ApplicationColorSet, BackgroundColorPicker.BackgroundEnd.Color, ThemeColorProperty.BackgroundEndGradientColor);
        }

        #region Events

        public event EventHandler<EventArgs> Completed;
        public event EventHandler<EventArgs> Cancelled;

        #endregion
    }
}
