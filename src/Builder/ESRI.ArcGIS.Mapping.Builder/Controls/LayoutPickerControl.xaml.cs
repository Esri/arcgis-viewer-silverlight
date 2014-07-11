/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Mapping.Builder.FileExplorer;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Mapping.Builder.Common;
using ESRI.ArcGIS.Mapping.Controls;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class LayoutPickerControl : UserControl
    {
        string _startLayoutUrl;
        ChangeLayoutCommand changeLayout;

        public LayoutPickerControl()
        {
            InitializeComponent();
            changeLayout = new ChangeLayoutCommand();
        }

        private void LayoutPickerControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (LayoutsListBox.Items.Count > 0)
                LayoutsListBox.SelectedIndex = 0;
            if (Layouts.Items.Count > 0)
                Layouts.SelectedIndex = 0;
            

            CurrentSelectedLayoutInfo = null;
            Layouts.MaxHeight = Application.Current.RootVisual.RenderSize.Height - 60;
            Layouts.MaxWidth = Application.Current.RootVisual.RenderSize.Width - 100;
            string siteId = null;
            bool isTemplate = BuilderApplication.Instance.CurrentSite == null;
            if (isTemplate)
            {
                if (BuilderApplication.Instance.CurrentTemplate != null)
                    siteId = BuilderApplication.Instance.CurrentTemplate.ID;
            }
            else
                siteId = BuilderApplication.Instance.CurrentSite.ID;

            if (string.IsNullOrEmpty(siteId))
                return;

            // Get the layouts config file
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += WebClient_DownloadStringCompleted;
            if (MapApplication.Current != null && MapApplication.Current.Urls != null 
                && !string.IsNullOrEmpty(MapApplication.Current.Urls.BaseUrl))
            {
                // Construct URL to config file
                string layoutConfigUrl = MapApplication.Current.Urls.BaseUrl;
                if (!layoutConfigUrl.EndsWith("/"))
                    layoutConfigUrl += "/";
                layoutConfigUrl += "Config/Layouts/Layouts.xml";

                // Download config
                wc.DownloadStringAsync(new Uri(layoutConfigUrl));
            }

            _startLayoutUrl = ViewerApplicationControl.Instance.ViewerApplication.LayoutFilePath;
        }

        private void WebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null || string.IsNullOrEmpty(e.Result))
                return;

            // Deserialize the list of layouts
            XmlSerializer serializer = new XmlSerializer(typeof(LayoutInfoCollection)); 
            LayoutInfoCollection layoutList = null;
            using (TextReader reader = new StringReader(e.Result))
            {
                layoutList = (LayoutInfoCollection)serializer.Deserialize(reader);
            }

            // Update UI with layouts list
            if (layoutList != null && layoutList.Layouts != null)
            {
                LayoutsListBox.ItemsSource = Layouts.ItemsSource = layoutList.Layouts;
                if (layoutList.Layouts.Count > 0)
                {
                    CurrentSelectedLayoutInfo = layoutList.Layouts.ElementAtOrDefault(0);
                    if (CurrentSelectedLayoutInfo != null)
                        LayoutsListBox.SelectedItem = Layouts.SelectedItem = CurrentSelectedLayoutInfo;
                }            
            }
        }

        private static string makeLayoutNameFriendly(string layoutFileName)
        {
            if (string.IsNullOrWhiteSpace(layoutFileName))
                return layoutFileName;

            string name = layoutFileName.Replace(".xaml", "");
            return name.InsertSpaces();
        }
        
        private LayoutInfo CurrentSelectedLayoutInfo { get; set; }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
                Cancelled(this, EventArgs.Empty);
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            List<LayoutInfo> infos = Layouts.ItemsSource as List<LayoutInfo>;
            if (infos == null)
                return;

            int index = infos.IndexOf(CurrentSelectedLayoutInfo);
            if (index < 1)
                index = infos.Count;
            CurrentSelectedLayoutInfo = infos.ElementAtOrDefault(index-1);
            if (CurrentSelectedLayoutInfo != null)
            {
                LayoutsListBox.SelectedItem = Layouts.SelectedItem = CurrentSelectedLayoutInfo;
                LayoutsListBox.ScrollIntoView(CurrentSelectedLayoutInfo);
                Layouts.ScrollIntoView(CurrentSelectedLayoutInfo);
            }            
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            List<LayoutInfo> infos = Layouts.ItemsSource as List<LayoutInfo>;
            if (infos == null)
                return;

            int index = infos.IndexOf(CurrentSelectedLayoutInfo);
            if (index >= infos.Count - 1)            
                index = -1;            
            CurrentSelectedLayoutInfo = infos.ElementAtOrDefault(index+1);
            if (CurrentSelectedLayoutInfo != null)
            {
                LayoutsListBox.SelectedItem = Layouts.SelectedItem = CurrentSelectedLayoutInfo;
                Layouts.ScrollIntoView(CurrentSelectedLayoutInfo);
                LayoutsListBox.ScrollIntoView(CurrentSelectedLayoutInfo);
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Only update the layout if the layout currently selected in the dialog does not match the
            // layout currently applied to the application
            if (CurrentSelectedLayoutInfo == null || 
            CurrentSelectedLayoutInfo.File == ViewerApplicationControl.Instance.ViewerApplication.LayoutFilePath)
                return;
            changeLayout.Execute(CurrentSelectedLayoutInfo.File);
        }

        private void LayoutsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LayoutInfo newChoice = LayoutsListBox.SelectedItem as LayoutInfo;
            if (newChoice == CurrentSelectedLayoutInfo)
                return;

            CurrentSelectedLayoutInfo = newChoice;
            if (CurrentSelectedLayoutInfo != null)
            {
                LayoutsListBox.SelectedItem = Layouts.SelectedItem = CurrentSelectedLayoutInfo;
                Layouts.ScrollIntoView(CurrentSelectedLayoutInfo);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentSelectedLayoutInfo == null)
                return;

            // Only update the layout if the layout currently selected in the dialog does not match the
            // layout currently applied to the application
            if (CurrentSelectedLayoutInfo.File != ViewerApplicationControl.Instance.ViewerApplication.LayoutFilePath)
                changeLayout.Execute(CurrentSelectedLayoutInfo.File);

            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }

        public void Cancel()
        {
            if (CurrentSelectedLayoutInfo != null
                && ViewerApplicationControl.Instance != null
                && ViewerApplicationControl.Instance.ViewerApplication != null
                && ViewerApplicationControl.Instance.ViewerApplication.LayoutFilePath != _startLayoutUrl)
            {
                changeLayout.Execute(_startLayoutUrl);
            }
        }

        #region Events

        public event EventHandler<EventArgs> Completed;
        public event EventHandler<EventArgs> Cancelled;

        #endregion

    }
}
