/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Linq;
using System.Xml.Linq;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class MarkerSymbolSelector : Control
    {
        Rectangle Overlay = null;
        ItemsControl SymbolsList = null;

        private static List<ImageInfo> markerSymbolImageInfos;
        public MarkerSymbolSelector()
        {
            DefaultStyleKey = typeof(MarkerSymbolSelector);            
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Overlay = GetTemplateChild("Overlay") as Rectangle;
            if (Overlay != null)
            {
                // Tie the IsEnabled property to the visibility of the overlay rectangle (to give it a grayed out effect)
                Binding binding = new Binding()
                {
                    Source = this,
                    Path = new PropertyPath("IsEnabled"),
                    Converter = new ReverseVisibilityConverter(),
                };
                Overlay.SetBinding(Rectangle.VisibilityProperty, binding);
            }

            SymbolsList = GetTemplateChild("SymbolsList") as ItemsControl;
            if (SymbolsList != null)
            {
                SymbolsList.MouseLeftButtonUp += new MouseButtonEventHandler(Symbols_MouseLeftButtonUp);                
            }
        }

        void Symbols_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            image_MouseLeftButtonUp(e.OriginalSource, e);
        }

        public event SymbolSelectedEventHandler SymbolSelected;
        private void OnSymbolSelected(SymbolSelectedEventArgs e)
        {
            if (SymbolSelected != null)
                SymbolSelected(this, e);
        }

        /// <summary>
        /// Relative URL (to the xap) for the catalog file which contains the list of marker symbols
        /// Eg:- MarkerSymbols/Symbols.xml
        /// </summary>
        public string MarkerSymbolConfigFileUrl { get; set; }

        private string _markerSymbolDirectory;
        /// <summary>
        /// Base Symbol Directory for the Marker Symbols
        /// </summary>
        public string MarkerSymbolDirectory
        {
            get
            {
                return _markerSymbolDirectory;
            }
            set
            {
                _markerSymbolDirectory = value;
            }
        }

        public void Show()
        {
            if (markerSymbolImageInfos == null || markerSymbolImageInfos.Count == 0)
            {
                if (string.IsNullOrEmpty(MarkerSymbolConfigFileUrl))
                    return;
                //markerSymbolConfigFile += "?r=" + Guid.NewGuid().ToString(); // randomize the URL to force download
                // Fetch the symbols.xml file
                WebClient xmlClient = new WebClient();
                xmlClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
                xmlClient.DownloadStringAsync(new Uri(MarkerSymbolConfigFileUrl, UriKind.RelativeOrAbsolute));
            }
            else if (SymbolsList.ItemsSource == null)
            {
                SymbolsList.ItemsSource = markerSymbolImageInfos;
            }
        }

        private void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                loadMarkerSymbolImageInfos(e, MarkerSymbolDirectory);
                SymbolsList.ItemsSource = markerSymbolImageInfos;
            }
        }

        private static void loadMarkerSymbolImageInfos(DownloadStringCompletedEventArgs e, string markerSymbolDirectory)
        {
            string xmlData = e.Result;
            XElement xmlElement = XElement.Parse(xmlData);
            var symbols = from symbol in xmlElement.Descendants("Symbol")
                          select new
                          {
                              FilePath = symbol.Attribute("filepath").Value,
                              CenterX = symbol.Attribute("centerX").Value,
                              CenterY = symbol.Attribute("centerY").Value,
                              Width = symbol.Attribute("width").Value,
                              Height = symbol.Attribute("height").Value,
                          };

            markerSymbolImageInfos = new List<ImageInfo>();
            foreach (var symbol in symbols)
            {
                ImageInfo imageInfo = new ImageInfo
                {
                    RelativeUrl = markerSymbolDirectory + "/" + symbol.FilePath,
                    CenterX = Convert.ToInt32(symbol.CenterX),
                    CenterY = Convert.ToInt32(symbol.CenterY),
                    Width = Convert.ToInt32(symbol.Width),
                    Height = Convert.ToInt32(symbol.Height)
                };
                markerSymbolImageInfos.Add(imageInfo);
            }
        }

        void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image image = sender as Image;
            if (image == null)
                return;
            ImageInfo symbol = image.DataContext as ImageInfo;
            if (symbol == null)
                return;
            OnSymbolSelected(new SymbolSelectedEventArgs { SelectedImage = symbol });
        }

        static string markerSymbolDirectory;
        static string markerSymbolConfigFileUrl;
        internal static void PreLoadMarkerSymbols(string markerSymbolDir, string markerSymbolConfigFile)
        {
            markerSymbolDirectory = markerSymbolDir;
            markerSymbolConfigFileUrl = markerSymbolConfigFile;

            BackgroundWorker worker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = false,
                WorkerReportsProgress = false,
            };
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerAsync();
        }

        static void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (markerSymbolImageInfos == null || markerSymbolImageInfos.Count == 0)
            {
                if (string.IsNullOrEmpty(markerSymbolConfigFileUrl))
                    return;
                //markerSymbolConfigFile += "?r=" + Guid.NewGuid().ToString(); // randomize the URL to force download

                // Fetch the symbols.xml file
                WebClient xmlClient = new WebClient();
                xmlClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(xmlClient_DownloadStringCompleted);
                xmlClient.DownloadStringAsync(new Uri(markerSymbolConfigFileUrl, UriKind.RelativeOrAbsolute));
            }
        }

        static void xmlClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                loadMarkerSymbolImageInfos(e, markerSymbolDirectory);
                if (markerSymbolImageInfos != null)
                {
                    foreach (ImageInfo imageInfo in markerSymbolImageInfos)
                    {
                        WebClient wc = new WebClient();
                        wc.OpenReadAsync(new Uri(imageInfo.RelativeUrl, UriKind.Relative));
                    }
                }
            }
        }
    }

    public delegate void SymbolSelectedEventHandler(object sender, SymbolSelectedEventArgs e);
    public class SymbolSelectedEventArgs : EventArgs
    {
        public ImageInfo SelectedImage { get; set; }
    }
    public class ImageInfo
    {
        public string RelativeUrl;
        public int CenterX;
        public int CenterY;
        public int Width;
        public int Height;
    }

    public class ImageUrlConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ImageInfo imageInfo = value as ImageInfo;
            if (imageInfo != null)
            {
                return new BitmapImage(new Uri(imageInfo.RelativeUrl, UriKind.Relative));
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    internal class ReverseVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visibility = (bool)value;
            return !visibility ? Visibility.Visible : Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            return (visibility == Visibility.Collapsed);
        }

        #endregion
    }
}
