/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client.Application.Controls;

namespace ESRI.ArcGIS.Mapping.Controls
{
    //[Export(typeof(ICommand))]
	//[DisplayName("NavigateToHelpLinkDisplayName")]
	//[Category("CategoryMap")]
	//[Description("NavigateToHelpLinkDescription")]
    public class NavigateToHelpLinkCommand : CommandBase
    {
        public HelpLink HelpLink { get; set; }

        public DropDownButton ParentDropDownButton { get; set; }
        
        public override bool CanExecute(object parameter)
        {
            return HelpLink != null && !string.IsNullOrWhiteSpace(HelpLink.Url);
        }

        public override void Execute(object parameter)
        {
            Uri uri = null;
            if(Uri.TryCreate(HelpLink.Url, UriKind.RelativeOrAbsolute, out uri))
            {
                System.Windows.Browser.HtmlPage.Window.Navigate(uri, "_blank");
            }

            if (ParentDropDownButton != null)
                ParentDropDownButton.IsContentPopupOpen = false;
        }
    }
}
