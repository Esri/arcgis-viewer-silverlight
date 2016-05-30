/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Mapping.Controls;
using System;
using System.Net;
using ESRI.ArcGIS.Mapping.Builder.Common;
using System.Linq;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.DataSources;
using ESRI.ArcGIS.Mapping.Core.DataSources;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class NewApplicationControl : UserControl
    {
        public NewApplicationControl()
        {   
            InitializeComponent();
        }

        void NewApplicationControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Templates are not loaded in design environments
            if (BuilderApplication.Instance.Templates != null)
            {
                Template template = BuilderApplication.Instance.Templates.FirstOrDefault<Template>(t => t.IsDefault);
                if (template != null)
                {
                    PreviewImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(template.BaseUrl + "/Config/MapPreview.png"));
                    WebClient webClient = new WebClient();
                    webClient.DownloadStringCompleted += (obj, args) =>
                                                             {
                                                                 if (args.Error == null)
                                                                 {
                                                                     DescriptionTextControl.Text = args.Result ?? string.Empty;
                                                                 }
                                                             };
                    webClient.DownloadStringAsync(new Uri(String.Format("{0}/Config/MapDescription.txt", template.BaseUrl, template.ID), UriKind.Absolute));
                }
                if (BuilderApplication.Instance.MapCenter == null)
                    BuilderApplication.Instance.MapCenter = MapCenter;

            }
        }

        private void MapCenter_MapSelectedForOpening(object sender, Mapping.Controls.MapCenter.MapEventArgs e)
        {
            if (e != null && e.Map != null) 
                CreateNewApplicationCommand.CreateNewApplication(MapCenter.GetMapXaml(e));
        }

        public bool MapCenterRequiresRefresh
        {
            get { return (bool)GetValue(MapCenterRequiresRefreshProperty); }
            set { SetValue(MapCenterRequiresRefreshProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MapCenterRequiresRefresh.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MapCenterRequiresRefreshProperty =
            DependencyProperty.Register("MapCenterRequiresRefresh", typeof(bool), typeof(NewApplicationControl), new PropertyMetadata(false, OnMapCenterRefresh));

        static void OnMapCenterRefresh(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            NewApplicationControl control = o as NewApplicationControl;
            if (control.MapCenterRequiresRefresh)
            {
                control.MapCenter.Activate(true);
                BuilderApplication.Instance.MapCenterRequiresRefresh = false;
            }
        }

        
    }
}
