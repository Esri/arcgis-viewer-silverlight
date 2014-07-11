/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Builder.ApplicationBuilder;
using ESRI.ArcGIS.Mapping.Core;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.ReflectionModel;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Mapping.Builder.Common;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class SiteDetailsDialogControl : UserControl
    {
        public SiteDetailsDialogControl()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (validateInput())
            {
                enableDisableUI(true);
                showHideProgressIndicator(Visibility.Visible);
                ViewerApplicationControl va = ViewerApplicationControl.Instance;
                if (va != null)
                {
                    va.View.SaveExtensionsConfigData();

                    // Get add-in configuration (tools, controls, and behaviors) before map configuration so add-ins have a chance
                    // to perform map-related cleanup (e.g. removing temp layers) before the map is saved
                    string toolsXml = va.ToolPanels != null ? va.ToolPanels.GetToolPanelsXml() : string.Empty;
                    string controlsXml = va.View.GetConfigurationOfControls();

                    // Publish only Xaps in use
                    string behaviorsXml = null;
                    ObservableCollection<string> usedXaps = BuilderApplication.Instance.GetXapsInUse(out behaviorsXml);
                    BuilderApplication.Instance.SyncExtensionsInUse(usedXaps);

                    string mapXaml = va.View.GetMapConfiguration(null);

                    // Now that the extensions list has been updated - serialize the applicationXml
                    string appXml = va.ViewerApplication.ToXml();
                    string colorsXaml = va.GetColorXaml();
                    byte[] previewImageBytes = BuilderApplication.Instance.GetPreviewImage();
                    Template template = BuilderApplication.Instance.Templates.FirstOrDefault<Template>(t => t.IsDefault);
                    string templateId = template != null ? template.ID : "Default";

                    SitePublishInfo info = new SitePublishInfo()
                    {
                        ApplicationXml = appXml,
                        BehaviorsXml = behaviorsXml,
                        ColorsXaml = colorsXaml,
                        ControlsXml = controlsXml,
                        ExtensionsXapsInUse = usedXaps.ToArray(),
                        MapXaml = mapXaml,
                        PreviewImageBytes = previewImageBytes,
                        ToolsXml = toolsXml
                    };

                    var title = "";
                    if (ViewerApplicationControl.Instance != null && ViewerApplicationControl.Instance.ViewerApplication != null)
                        title = ViewerApplicationControl.Instance.ViewerApplication.TitleText;
                    ApplicationBuilderClient client = WCFProxyFactory.CreateApplicationBuilderProxy();
                    client.CreateViewerApplicationFromTemplateCompleted += new EventHandler<CreateViewerApplicationFromTemplateCompletedEventArgs>(client_CreateViewerApplicationFromTemplateCompleted);
                    client.CreateViewerApplicationFromTemplateAsync(NameTextBox.Text.Trim(), title, 
                        DescriptionTextBox.Text.Trim(), templateId, info);
                }
            }
        }

        void client_CreateViewerApplicationFromTemplateCompleted(object sender, CreateViewerApplicationFromTemplateCompletedEventArgs e)
        {
            enableDisableUI(false);
            showHideProgressIndicator(Visibility.Collapsed);
            BuilderApplication.Instance.HideWindow(this);
            if (e.Cancelled)
                return;

            if (e.Error != null)
            {
                handleError(e.Error);
                return;
            }            

            if (e.Site == null)
            {
                handleError(new Exception(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.FailedToCreateAndPublishApplication));
                return;
            }

            BuilderApplication.Instance.CurrentSite = e.Site;
            if (BuilderApplication.Instance.Sites == null)
                BuilderApplication.Instance.Sites = new System.Collections.ObjectModel.ObservableCollection<Site>();
            BuilderApplication.Instance.Sites.Add(e.Site);

            //UriBuilder ub = new UriBuilder();
            //ub.Host = WebServerTextBox.Text.Trim();
            //ub.Port = int.Parse(PortTextBox.Text.Trim());
            //ub.Path = NameTextBox.Text.Trim().TrimEnd('/') + "/";
            System.Windows.Browser.HtmlPage.Window.Navigate(new Uri(e.Site.Url, UriKind.Absolute), "_blank");
        }

        private void handleError(Exception exception)
        {
            string errorMsg = "";
            if (exception != null)
            {
                if (!String.IsNullOrEmpty(exception.Message))
                    errorMsg = exception.Message;
                else
                    errorMsg = exception.ToString();
            }
            MessageBoxDialog.Show(exception != null ? errorMsg : ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ErrorDetailsNotKnown,
                ESRI.ArcGIS.Mapping.Builder.Resources.Strings.SiteDeploymentFailed, MessageBoxButton.OK);
        }

        private void enableDisableUI(bool disable)
        {
            OKButton.IsEnabled = CancelButton.IsEnabled = NameTextBox.IsEnabled = DescriptionTextBox.IsEnabled = !disable;
        }

        private void showHideProgressIndicator(Visibility visibility)
        {
            ProgressIndicator.Visibility = visibility;
        }

        private bool validateInput()
        {
            string applicationName = NameTextBox.Text.Trim();
            if(string.IsNullOrWhiteSpace(applicationName))
            {
                MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.EnterNameForApplication, ESRI.ArcGIS.Mapping.Builder.Resources.Strings.InvalidName, MessageBoxButton.OK);
                return false;
            }

            return true;
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            BuilderApplication.Instance.HideWindow(this);
        }

        private void SiteDetailsDialogControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize site name textbox
            if (BuilderApplication.Instance.CurrentSite != null
            && !string.IsNullOrEmpty(BuilderApplication.Instance.CurrentSite.Name))
                NameTextBox.Text = BuilderApplication.Instance.CurrentSite.Name;
            else if (ViewerApplicationControl.Instance != null
            && ViewerApplicationControl.Instance.ViewerApplication != null
            && !string.IsNullOrEmpty(ViewerApplicationControl.Instance.ViewerApplication.TitleText))
                NameTextBox.Text = ViewerApplicationControl.Instance.ViewerApplication.TitleText;

            // Initialize description textbox
            if (BuilderApplication.Instance.CurrentSite != null
            && !string.IsNullOrEmpty(BuilderApplication.Instance.CurrentSite.Description))
                DescriptionTextBox.Text = BuilderApplication.Instance.CurrentSite.Description;
            else
                DescriptionTextBox.Text = "";

            NameTextBox.Focus();
            NameTextBox.SelectAll();
        }

        private void CheckForEnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                OKButton_Click(this, null);
        }
    }
}
