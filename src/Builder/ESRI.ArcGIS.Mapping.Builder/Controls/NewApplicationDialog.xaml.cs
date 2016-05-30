/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class NewApplicationDialog : UserControl
    {
        private string mapXaml;

        public NewApplicationDialog()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            BuilderApplication.Instance.HideWindow(this);
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            OnOkClicked(new InitialMapDocumentEventArgs() { MapXaml = mapXaml });
        }

        protected virtual void OnOkClicked(InitialMapDocumentEventArgs args)
        {
            if (OkClicked != null)
                OkClicked(this, args);
        }

        public event EventHandler<InitialMapDocumentEventArgs> OkClicked;

        private void NewMap_Checked(object sender, RoutedEventArgs e)
        {
            if(btnOk != null)
                btnOk.IsEnabled = true;
        }

        private void ExistingMap_Checked(object sender, RoutedEventArgs e)
        {
            if (btnOk != null)
                btnOk.IsEnabled = !string.IsNullOrWhiteSpace(mapXaml);
        }


        MapCenter mapCenter;
        private void BrowseWebMap_Click(object sender, RoutedEventArgs e)
        {
            if (mapCenter == null)
            {
                View view = View.Instance;
                mapCenter = new MapCenter();
                mapCenter.Height = Application.Current.RootVisual.RenderSize.Height - 100;
                mapCenter.Width = Application.Current.RootVisual.RenderSize.Width - 100;
                mapCenter.InitialVisibility = System.Windows.Visibility.Visible;
                mapCenter.MapSelectedForOpening += new EventHandler<MapCenter.MapEventArgs>(mapCenter_MapSelectedForOpening);
            }

            BuilderApplication.Instance.ShowWindow(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.OpenWebMapFromArcGISCom,
                mapCenter);            
        }

        void mapCenter_MapSelectedForOpening(object sender, MapCenter.MapEventArgs e)
        {
            if (e != null && e.Map != null)
            {
                mapXaml = MapCenter.GetMapXaml(e);
                btnOk.IsEnabled = true;
            }
            BuilderApplication.Instance.HideWindow(mapCenter);
        }

        ChooseSiteControl chooseSiteControl;
        private void ChooseExistingProject_Click(object sender, RoutedEventArgs e)
        {
            if (chooseSiteControl == null)
            {
                chooseSiteControl = new ChooseSiteControl();
                chooseSiteControl.SiteChosen += new EventHandler<SiteChosenEventArgs>(chooseSiteControl_SiteChosen);
            }
            BuilderApplication.Instance.ShowWindow(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ChooseExistingProject, chooseSiteControl);
        }

        void chooseSiteControl_SiteChosen(object sender, SiteChosenEventArgs e)
        {
            if (e == null || e.Site == null
                || string.IsNullOrWhiteSpace(e.Site.Url))
                return;

            ConfigurationProvider configProvider = new ConfigurationProvider();
            configProvider.GetConfigurationAsync(null, onGetConfigurationCompleted, onGetConfigurationFailed);
        }

        void onGetConfigurationCompleted(object sender, GetConfigurationCompletedEventArgs e)
        {
            if (e.Map == null)
            {
                MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ErrorRetrievingMapDocumentForSite);
                return;
            }

            mapXaml = new MapXamlWriter(true).MapToXaml(e.Map);
            btnOk.IsEnabled = true;
            BuilderApplication.Instance.HideWindow(chooseSiteControl);
        }

        void onGetConfigurationFailed(object sender, ExceptionEventArgs e)
        {
			MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ErrorRetrievingMapDocumentForSite);
        }
    }

    public class InitialMapDocumentEventArgs : EventArgs
    {
        public string MapXaml { get; set; }
    }
}
