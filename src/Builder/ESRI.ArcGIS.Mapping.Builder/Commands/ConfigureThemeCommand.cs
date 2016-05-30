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
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class ConfigureThemeCommand : CommandBase
    {
        private ConfigureThemeControl configureThemeControl;
        private bool _completed = false;

        public override void Execute(object parameter)
        {
            if (configureThemeControl == null)
            {
                configureThemeControl = new ConfigureThemeControl();
                configureThemeControl.Completed += ConfigureThemeControl_Completed;
                configureThemeControl.Cancelled += ConfigureThemeControl_Cancelled;
            }

            configureThemeControl.SetThemeColorsToView();
            BuilderApplication.Instance.ShowWindow(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.Theme, configureThemeControl, false, null, ConfigureTheme_WindowClosed);
        }

        private void ConfigureThemeControl_Cancelled(object sender, EventArgs e)
        {
            BuilderApplication.Instance.HideWindow(configureThemeControl);
        }

        private void ConfigureThemeControl_Completed(object sender, EventArgs e)
        {
            _completed = true;
            BuilderApplication.Instance.HideWindow(configureThemeControl);
        }

        private void ConfigureTheme_WindowClosed(object sender, EventArgs e)
        {
            if (!_completed)
                configureThemeControl.Cancel();
            else
                _completed = false;
        }
    }
}
