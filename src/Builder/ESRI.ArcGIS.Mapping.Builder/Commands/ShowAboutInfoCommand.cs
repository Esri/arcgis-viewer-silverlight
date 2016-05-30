/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Mapping.Builder.Resources;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class ShowAboutInfoCommand : CommandBase
    {
        private const string ABOUT_DIALOG_STYLE = "AboutControlStyle";
        private const string DESIGN_HOST_BACKGROUND_TEXT = "DesignHostBackgroundTextBrush";
        private AboutInfoControl _aboutControl;
        #region ICommand Members

        public override void Execute(object parameter)
        {
            if (_aboutControl == null)
                _aboutControl = new AboutInfoControl();

            string builderTitle = BuilderApplication.Instance != null && !string.IsNullOrWhiteSpace(BuilderApplication.Instance.TitleText) ? BuilderApplication.Instance.TitleText : "";

            _aboutControl.DataContext = new AboutInfo()
            {
                Title = builderTitle,
                Version = ApplicationHelper.GetExecutingAssemblyVersion(),
                SilverlightApiVersion = ApplicationHelper.GetSilverlightAPIVersion(),
            };

            if (Application.Current != null &&
                Application.Current.Resources != null)
            {
                if (Application.Current.Resources.Contains(ABOUT_DIALOG_STYLE))
                {
                    Style style = Application.Current.Resources[ABOUT_DIALOG_STYLE] as Style;
                    if (style != null)
                        _aboutControl.Style = style;
                }
                if (Application.Current.Resources.Contains(DESIGN_HOST_BACKGROUND_TEXT))
                {
                    SolidColorBrush foreground = Application.Current.Resources[DESIGN_HOST_BACKGROUND_TEXT] as SolidColorBrush;
                    if (foreground != null)
                        _aboutControl.Foreground = foreground;
                }
            }


            BuilderApplication.Instance.ShowWindow(Strings.AboutDialogTitle + " " + builderTitle, _aboutControl);
        }
        #endregion
    }
}
