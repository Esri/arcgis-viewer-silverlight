/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Linq;
using System.Windows;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Builder.ApplicationBuilder;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Builder.Controls;
using ESRI.ArcGIS.Mapping.Builder.Common;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class CreateNewApplicationCommand : CommandBase
    {
        NewApplicationDialog newApplicationDialog = null;
        public override void Execute(object parameter)
        {
            // Get global instance of the getting started control and display it if getting started mode is not disabled
            TutorialDialogControl tdc = BuilderApplication.Instance.TutorialDialogControl;
            BuilderApplication.Instance.GettingStartedVisibility = tdc.IsTutorialDisabled == false ? Visibility.Visible : Visibility.Collapsed;

            CreateNewApplication(null);
        }

        void newApplicationDialog_OkClicked(object sender, InitialMapDocumentEventArgs e)
        {
            if(newApplicationDialog != null)
                BuilderApplication.Instance.HideWindow(newApplicationDialog);

            CreateNewApplication(e.MapXaml);
        }

        public static void CreateNewApplication(string MapXaml)
        {
            // Reset web map properties
            if (string.IsNullOrEmpty(MapXaml))
            {
                ViewerApplication.WebMapSettings.Linked = null;
                ViewerApplication.WebMapSettings.Document = null;
                ViewerApplication.WebMapSettings.ID = null;
                ViewerApplication.WebMapSettings.ItemInfo = null;
            }

            BuilderConfigurationProvider configProvider = ViewerApplicationControl.Instance.ConfigurationProvider as BuilderConfigurationProvider;
            if (configProvider != null)
                configProvider.MapXaml = MapXaml;

            if (BuilderApplication.Instance.Templates == null)
                return;

            Template defaultTemplate = BuilderApplication.Instance.Templates.FirstOrDefault<Template>(t => t.IsDefault);
            if (defaultTemplate == null && BuilderApplication.Instance.Templates.Count > 0)
                defaultTemplate = BuilderApplication.Instance.Templates[0];

            if (defaultTemplate != null)
            {
                BuilderApplication.Instance.CurrentTemplate = defaultTemplate;
                BuilderApplication.Instance.BaseUrl = Application.Current.Host.Source.AbsoluteUri.Replace("ESRI.ArcGIS.Mapping.Builder.xap", "").TrimEnd('/');
                ViewerApplicationControl.Instance.DefaultApplicationSettings = BuilderApplication.Instance;
                ViewerApplicationControl.Instance.BaseUri = new Uri(string.Format("{0}/", defaultTemplate.BaseUrl), UriKind.Absolute);                
            }

            BuilderApplication.Instance.CurrentSite = null; // Since this is a new site - it hasn't been published yet
            BuilderApplication.Instance.CatalogScreenVisibility = Visibility.Collapsed;
            BuilderApplication.Instance.NewappScreenVisibility = Visibility.Collapsed;
            BuilderApplication.Instance.BuilderScreenVisibility = Visibility.Visible;
        }
    }
}
