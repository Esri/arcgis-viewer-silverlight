/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class ApplicationServices : IApplicationServices
    {        
        public void BrowseForFile(EventHandler<BrowseCompleteEventArgs> onComplete, string[] fileExts = null, string startupFolderRelativePath = null, object userState = null)
        {
            FileBrowser browser = new FileBrowser()
                {
                    FileExtensions = new System.Collections.ObjectModel.ObservableCollection<string>(),
                    StartupRelativeUrl = startupFolderRelativePath,
                    Tag = new object[]{ onComplete, userState},
                };
            if (fileExts != null)
            {
                foreach (string fileExt in fileExts)
                    browser.FileExtensions.Add(fileExt);
            }
            browser.UrlChosen += onUrlChosen;
            browser.CancelClicked += browser_CancelClicked;
            MapApplication.Current.ShowWindow(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.BrowseForFile, 
                browser, true, null, null, WindowType.DesignTimeFloating);
        }

        void browser_CancelClicked(object sender, EventArgs e)
        {
            FrameworkElement elem = sender as FrameworkElement;
            if (elem != null)
                MapApplication.Current.HideWindow(elem);
        }

        void onUrlChosen(object sender, FileChosenEventArgs args)
        {
            FrameworkElement elem = sender as FrameworkElement;
            MapApplication.Current.HideWindow(elem);
            object [] tag = elem.Tag as object[];
            EventHandler<BrowseCompleteEventArgs> onComplete = tag[0] as EventHandler<BrowseCompleteEventArgs>;
            if (onComplete == null)
                return;

            Uri baseUri = new Uri(MapApplication.Current.Urls.BaseUrl);
            onComplete(this, new BrowseCompleteEventArgs() { Uri = new Uri(baseUri, args.RelativePath), RelativeUri = args.RelativePath, UserState = tag[1] });
        }

        public void BrowseForLayer(EventHandler<BrowseForLayerCompletedEventArgs> onComplete, object userState = null)
        {
            AddContentDialog browseDialog = new AddContentDialog()
            { 
                 MaxHeight = Application.Current.RootVisual.RenderSize.Height - 200,
                 Height = 600,
                 Width = 500,
            };
            browseDialog.ConnectionsProvider = View.Instance.ConnectionsProvider;
            browseDialog.LayerAdded += new EventHandler<LayerAddedEventArgs>(browseDialog_LayerAdded); 
            browseDialog.Tag = new object[] { onComplete, userState };
			MapApplication.Current.ShowWindow(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.BrowseForResource, browseDialog, true);
        }

        void browseDialog_LayerAdded(object sender, LayerAddedEventArgs e)
        {
            FrameworkElement elem = sender as FrameworkElement;
            MapApplication.Current.HideWindow(elem);
            object[] tag = elem.Tag as object[];
            EventHandler<BrowseForLayerCompletedEventArgs> onComplete = tag[0] as EventHandler<BrowseForLayerCompletedEventArgs>;
            if (onComplete == null)
                return;
            onComplete(this, new BrowseForLayerCompletedEventArgs() { Layer = e.Layer, UserState = tag[1] });            
        }
    }
}
