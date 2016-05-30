/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    //[Export(typeof(ICommand))]
	//[DisplayName("ToggleFullScreenDisplayName")]
	//[Category("CategoryLayer")]
	//[Description("ToggleFullScreenDescription")]
    public class ToggleFullScreenCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            Application.Current.Host.Content.IsFullScreen = !Application.Current.Host.Content.IsFullScreen;
        }              
    }
}
