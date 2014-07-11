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
using ESRI.ArcGIS.Mapping.Builder.ApplicationBuilder;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Builder.Common;
using System.Linq;
using ESRI.ArcGIS.Client.Application.Controls;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class SaveApplicationCommand : DependencyObject, ICommand
    {

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (ParentDropDownButton != null)
                ParentDropDownButton.IsContentPopupOpen = false;

            if (ViewerApplicationControl.Instance == null || BuilderApplication.Instance == null
                || BuilderApplication.Instance.CurrentSite == null)
                return;

            ViewerApplicationControl va = ViewerApplicationControl.Instance;
            if (va != null)
            {
                IncrementVersionNumber(va.ViewerApplication);
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

                SitePublishInfo info = new SitePublishInfo()
                {
                    ApplicationXml = appXml,
                    BehaviorsXml = behaviorsXml,
                    ColorsXaml = colorsXaml,
                    ControlsXml = controlsXml,
                    ExtensionsXapsInUse = usedXaps.ToArray(),
                    MapXaml = mapXaml,
                    PreviewImageBytes = previewImageBytes,
                    ToolsXml = toolsXml,
                };
                ApplicationBuilderClient client = WCFProxyFactory.CreateApplicationBuilderProxy();
                client.SaveConfigurationForSiteCompleted += new EventHandler<SaveConfigurationForSiteCompletedEventArgs>(client_SaveConfigurationForSiteCompleted);
                client.SaveConfigurationForSiteAsync(BuilderApplication.Instance.CurrentSite.ID, info);
            }
        }

        private void IncrementVersionNumber(ViewerApplication application)
        {
            if (application != null && !string.IsNullOrWhiteSpace(application.Version))
            {
                Version version = null;
                if (Version.TryParse(application.Version, out version))
                {
                    version = new Version(version.Major, version.Minor, version.Build, version.Revision + 1);
                    application.Version = version.ToString();
                }
            }
        }

        void client_SaveConfigurationForSiteCompleted(object sender, SaveConfigurationForSiteCompletedEventArgs e)
        {
            MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ChangesSavedSuccessfully, ESRI.ArcGIS.Mapping.Builder.Resources.Strings.Success, MessageBoxButton.OK); 
        }       

        #region ParentDropDownButton
        /// <summary>
        /// 
        /// </summary>
        public DropDownButton ParentDropDownButton
        {
            get { return GetValue(ParentDropDownButtonProperty) as DropDownButton; }
            set { SetValue(ParentDropDownButtonProperty, value); }
        }

        /// <summary>
        /// Identifies the ParentDropDownButton dependency property.
        /// </summary>
        public static readonly DependencyProperty ParentDropDownButtonProperty =
            DependencyProperty.Register(
                "ParentDropDownButton",
                typeof(DropDownButton),
                typeof(SaveApplicationCommand),
                new PropertyMetadata(null));
        #endregion

        protected void OnCanExecuteChanged(EventArgs e)
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, e);
        }
    }
}
