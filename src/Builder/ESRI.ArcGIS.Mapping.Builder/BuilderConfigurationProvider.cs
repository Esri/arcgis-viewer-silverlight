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
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class BuilderConfigurationProvider : FileConfigurationProvider
    {
        public string MapXaml { get; set; }
        
        public override void GetConfigurationAsync(object userState, EventHandler<GetConfigurationCompletedEventArgs> onCompleted, EventHandler<ExceptionEventArgs> onFailed)
        {
            if (string.IsNullOrEmpty(MapXaml))
            {
                base.GetConfigurationAsync(userState, onCompleted, onFailed);
            }
            else
            {
                base.LoadConfigurationFromXmlString(MapXaml, userState, onCompleted, onFailed);
            }
        }
    }
}
