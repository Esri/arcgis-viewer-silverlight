/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using System.Windows.Media;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    //[Export(typeof(ICommand))]
	//[DisplayName("ChangeThemeBackgroundDisplayName")]
	//[Category("CategoryTheme")]
	//[Description("ChangeThemeBackgroundDescription")]
    public class ChangeThemeBackgroundCommand : CommandBase
    {
        BackgroundColorPicker backgroundColorPicker;
        public override void Execute(object parameter)
        {
            if (View.Instance == null)
                return;

            if (backgroundColorPicker != null)
                backgroundColorPicker.ColorChanged -= backgroundColorPicker_ColorChanged;
            //creating control again because this mimics the sharepoint experience
            backgroundColorPicker = new BackgroundColorPicker();
            backgroundColorPicker.ThemeColors = View.Instance.ThemeColors;
            backgroundColorPicker.BackgroundStart =
                new SolidColorBrush(View.Instance.ApplicationColorSet.BackgroundStartGradientColor);
            backgroundColorPicker.BackgroundEnd =
                new SolidColorBrush(View.Instance.ApplicationColorSet.BackgroundEndGradientColor);

            backgroundColorPicker.ColorChanged += new EventHandler(backgroundColorPicker_ColorChanged);

            MapApplication.Current.ShowWindow(Resources.Strings.ChangeBackground, backgroundColorPicker);
        }

        void backgroundColorPicker_ColorChanged(object sender, EventArgs e)
        {
            if (View.Instance == null)
                return;

            View.Instance.ApplicationColorSet.BackgroundStartGradientColor = backgroundColorPicker.BackgroundStart.Color;
            View.Instance.ApplicationColorSet.BackgroundEndGradientColor = backgroundColorPicker.BackgroundEnd.Color;
        }
    }
}
