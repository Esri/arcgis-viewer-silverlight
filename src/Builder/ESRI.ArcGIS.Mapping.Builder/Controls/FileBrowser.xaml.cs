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
using ESRI.ArcGIS.Mapping.Builder.FileExplorer;
using System.Collections.ObjectModel;
using System.IO;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Builder.Common;
using ESRI.ArcGIS.Mapping.Builder.Resources;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class FileBrowser : UserControl
    {
        private string Url { get; set; }        
        private string RelativeUrl { get; set; }
        private int CurrentFolderDepth { get; set; }

        public ObservableCollection<string> FileExtensions { get; set; }
        public string StartupRelativeUrl { get; set; }

        public FileBrowser()
        {
            InitializeComponent();
        }

        private void FileBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentFolderDepth = 0;
            RelativeUrl = StartupRelativeUrl;
            if (RelativeUrl != null)
                CurrentFolderDepth = RelativeUrl.Count<char>(c => c == '/') + 1;
            getFiles(RelativeUrl);
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog() {
                 Multiselect = false,
                 Filter = "Image Files|*.png;*.jpg;*.jpeg",
            };

            if (openDialog.ShowDialog() == true)
            {
                FileInfo selectedFile = openDialog.File;
                if (selectedFile == null)
                    return;
                
                byte[] fileBuffer = null;            
                try
                {
                    using (FileStream strm = openDialog.File.OpenRead())
                    {
                        selectedFile = openDialog.File;
                        using (BinaryReader rdr = new BinaryReader(strm))
                        {
                            fileBuffer = rdr.ReadBytes((int)strm.Length);
                        }
                    }

                    if (fileBuffer != null)
                    {
                        string siteId = null;
                        bool isTemplate = BuilderApplication.Instance.CurrentSite == null;
                        if (isTemplate)
                        {
                            if (BuilderApplication.Instance.CurrentTemplate != null)
                                siteId = BuilderApplication.Instance.CurrentTemplate.ID;
                            else if (BuilderApplication.Instance.Templates.Count > 0)
                                siteId = BuilderApplication.Instance.Templates[0].ID;
                        }
                        else
                            siteId = BuilderApplication.Instance.CurrentSite.ID;

                        if (string.IsNullOrEmpty(siteId))
                            return;

                        showHideProgressBar(false);                        
                        FileExplorerClient client = WCFProxyFactory.CreateFileExplorerProxy();
                        client.UploadFileToSiteCompleted += new EventHandler<UploadFileToSiteCompletedEventArgs>(client_UploadFileToSiteCompleted);
                        client.UploadFileToSiteAsync(siteId, isTemplate, RelativeUrl, selectedFile.Name, fileBuffer.ToArray(), "image/png");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogError(ex);
                }
            }
        }

        void client_UploadFileToSiteCompleted(object sender, UploadFileToSiteCompletedEventArgs e)
        {
            showHideProgressBar(true);
            if (e.Cancelled)
                return;

            if (e.Error != null || e.File == null)
                return;

            if (Files.Items != null)
            {
                e.File.DisplayName = makeFileNameUserFriendly(e.File.FileName);
                Files.Items.Add(e.File);
            }
        }

        private void Files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FileDescriptor selectedItem = Files.SelectedItem as FileDescriptor;
            if (selectedItem == null)
                return;
            if (selectedItem.IsFolder)
            {
                btnOk.IsEnabled = false;
                CurrentFolderDepth++;
                Dispatcher.BeginInvoke(() =>
                {
                    getFiles(selectedItem.RelativePath);
                });
            }
            else
            {
                UpNavigationItem upNavigationItem = selectedItem as UpNavigationItem;
                if (upNavigationItem != null)
                {
                    CurrentFolderDepth--;
                    string path = RelativeUrl;
                    if (path != null)
                    {
                        int pos = path.LastIndexOf('/');
                        if (pos > -1)
                            path = path.Substring(0, pos);
                        else
                            path = null;                        
                    }
                    Dispatcher.BeginInvoke(() =>
                    {                        
                        getFiles(path);
                    });
                }
                else
                {
                    btnOk.IsEnabled = true;
                }
            }
            Url = selectedItem.RelativePath;
        }

        private void getFiles(string relativePath)
        {
            string siteId = null;
            bool isTemplate = BuilderApplication.Instance.CurrentSite == null;
            if (isTemplate)
            {
                if (BuilderApplication.Instance.CurrentTemplate != null)
                    siteId = BuilderApplication.Instance.CurrentTemplate.ID;
                else if(BuilderApplication.Instance.Templates.Count > 0)
                    siteId = BuilderApplication.Instance.Templates[0].ID;
            }
            else
                siteId = BuilderApplication.Instance.CurrentSite.ID;

            if (string.IsNullOrEmpty(siteId))
                return;

            RelativeUrl = relativePath;
            if (Files.Items != null)
                Files.Items.Clear();
            btnOk.IsEnabled = false;            

            FileExplorerClient client = WCFProxyFactory.CreateFileExplorerProxy();
            client.GetFilesCompleted += new EventHandler<GetFilesCompletedEventArgs>(client_GetFilesCompleted);
            showHideProgressBar(true);
            Dispatcher.BeginInvoke(() =>
                {
                    client.GetFilesAsync(siteId, isTemplate, relativePath, FileExtensions);
                });
        }

        void client_GetFilesCompleted(object sender, GetFilesCompletedEventArgs e)
        {
            showHideProgressBar(true);

            if (e.Cancelled)
            {
                showHideProgressBar(true);
                return;
            }

            if (e.Error != null || e.Files == null)
            {
                showHideProgressBar(true);
                return;
            }

            ObservableCollection<FileDescriptor> items = e.Files;
            foreach (FileDescriptor item in items)
                item.DisplayName = makeFileNameUserFriendly(item.FileName);

            if (CurrentFolderDepth > 0)
            {
                UpNavigationItem upItem = new UpNavigationItem()
                    {
                        FileName = Strings.Up,
                        DisplayName = Strings.Up,
                        RelativePath = "..",
                    };
                if (items.Count > 0)
                    items.Insert(0, upItem);
                else
                    items.Add(upItem);                
            }
            else
            {
                RelativeUrl = null;
            }
            
            showHideProgressBar(true);
            if (items.Count > 0)
            {
                foreach (var item in items)
                    Files.Items.Add(item);
                NoFilesLabel.Visibility = Visibility.Collapsed;
                Files.Visibility = System.Windows.Visibility.Visible;
                Dispatcher.BeginInvoke(() =>
                {
                    FilesScrollViewer.ScrollToTop();
                });
            }
            else
            {
                NoFilesLabel.Visibility = Visibility.Visible;
                Files.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        void showHideProgressBar(bool hide)
        {
            if (hide)
            {
                ActivityIndicator.StopProgressAnimation();
                ActivityIndicator.Visibility = Visibility.Collapsed;
            }
            else
            {
                ActivityIndicator.Visibility = Visibility.Visible;
                ActivityIndicator.StartProgressAnimation();
            }
        }

        private string makeFileNameUserFriendly(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;
            int posOfDot = url.LastIndexOf(".");
            if (posOfDot != -1)
                return url.Substring(0, posOfDot);
            return url;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (UrlChosen != null)
                UrlChosen(this, new FileChosenEventArgs() { RelativePath = Url });
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (CancelClicked != null)
                CancelClicked(this, EventArgs.Empty);
        }

        public event EventHandler<FileChosenEventArgs> UrlChosen;
        public event EventHandler CancelClicked;

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            bool doubleClick = MouseButtonHelper.IsDoubleClick(sender, e);
            if (doubleClick)
                Ok_Click(sender, null);
        }


    }

    public class FileChosenEventArgs : EventArgs
    {
        public string RelativePath { get; set; }
    }

    public class UpNavigationItem : FileDescriptor
    {

    }

}
