/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;
using System.Windows.Controls;
using System.Reflection;
using System.Windows;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
    [DisplayName("AboutDisplayName")]
	[Category("CategoryApplication")]
    [Description("AboutDescription")]
    public class ShowAboutInfoCommand : CommandBase
    {
        private const string ABOUT_DIALOG_STYLE = "AboutControlStyle";
        private AboutInfoControl _aboutControl;

        #region ICommand Members

        public override void Execute(object parameter)
        {
            if (_aboutControl == null)
                _aboutControl = new AboutInfoControl();

            string builderTitle = ApplicationHelper.GetViewerApplicationTitle();
            if (string.IsNullOrWhiteSpace(builderTitle))
                builderTitle = string.Empty;
            AboutInfo info = new AboutInfo()
            {
                Title = builderTitle,
            };
            if (ViewerApplicationControl.Instance != null)
                info.Version = ApplicationHelper.GetProductVersion();
            else
                info.Version = ApplicationHelper.GetExecutingAssemblyVersion();

            _aboutControl.DataContext = info;

            Style style = LayoutStyleHelper.Instance.GetStyle(ABOUT_DIALOG_STYLE);
            if (style != null)
                _aboutControl.Style = style;

            MapApplication.Current.ShowWindow(LocalizableStrings.GetString("AboutDialogTitle") + " " + builderTitle, _aboutControl);
        }
        #endregion
    }
}
