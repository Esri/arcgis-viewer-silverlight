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
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class ChangeLogoIconCommand : CommandBase
    {
        FileBrowser browser;
        public override void Execute(object parameter)
        {
            if (browser == null)
            {
                browser = new FileBrowser()
                {
                    FileExtensions = new System.Collections.ObjectModel.ObservableCollection<string>() { 
                     ".png",".jpg", ".jpeg"
                },
                    StartupRelativeUrl = "Images"
                };                
                browser.CancelClicked += new EventHandler(browser_CancelClicked);
                browser.UrlChosen += new EventHandler<FileChosenEventArgs>(browser_UrlChosen);
            }
			BuilderApplication.Instance.ShowWindow(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ChangeLogo, browser);
        }

        void browser_UrlChosen(object sender, FileChosenEventArgs e)
        {
            if(ViewerApplicationControl.Instance != null && ViewerApplicationControl.Instance.ViewerApplication != null)
                ViewerApplicationControl.Instance.ViewerApplication.LogoFilePath = e.RelativePath;
            BuilderApplication.Instance.HideWindow(browser);
        }

        void browser_CancelClicked(object sender, EventArgs e)
        {
            BuilderApplication.Instance.HideWindow(browser);
        }
    }
}
