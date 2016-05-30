/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.ComponentModel.Composition;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    //[Export(typeof(ICommand))]
    //[DisplayName("ShowPreviewControlDisplayName")]
    //[Category("CategoryTheme")]
    //[Description("ShowPreviewControlDescription")]
    public class ShowPreviewControlCommand : CommandBase
    {
        ThemePreviewControl previewControl;
        public override void Execute(object parameter)
        {
            if (previewControl == null)
            {
                previewControl = new ThemePreviewControl();
                previewControl.Closed += (o, e) => {
                    MapApplication.Current.HideWindow(previewControl);
                };
            }
            MapApplication.Current.ShowWindow(Resources.Strings.PreviewTheme, previewControl);
        }
    }
}
