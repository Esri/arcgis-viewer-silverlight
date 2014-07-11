/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.ComponentModel.Composition;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ConfigureProxyCommand : CommandBase
    {
        SecureServicesConfig control;
        public override void Execute(object parameter)
        {
            if (control == null)
            {
                control = new SecureServicesConfig();
                MapApplication.Current.ShowWindow(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ConfigureSecuredServices, control,
                    false, null, (o, e) => { control = null; }, WindowType.DesignTimeFloating);
            }
        }
    }
}
