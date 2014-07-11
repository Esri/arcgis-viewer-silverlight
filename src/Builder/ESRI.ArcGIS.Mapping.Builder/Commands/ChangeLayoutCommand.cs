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
using ESRI.ArcGIS.Mapping.Controls;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class ChangeLayoutCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            string layoutFilePath = parameter as string;
            if (string.IsNullOrEmpty(layoutFilePath))
                return;

            // Applying the layout will, among other things, retrieve the configuration of tools.  Important to do this prior to getting map 
            // configuration so that tools have a chance to do any cleanup that affects the map before the map is saved
            ViewerApplicationControl.Instance.ApplyNewLayout(layoutFilePath);
            BuilderConfigurationProvider configProvider = ViewerApplicationControl.Instance.ConfigurationProvider as BuilderConfigurationProvider;
            if (configProvider != null)
                configProvider.MapXaml = ViewerApplicationControl.Instance.View.GetMapConfiguration(null);
        }
    }
}
