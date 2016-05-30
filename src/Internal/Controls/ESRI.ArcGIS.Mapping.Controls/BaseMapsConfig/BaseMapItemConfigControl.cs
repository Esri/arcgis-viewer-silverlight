/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
using ESRI.ArcGIS.Mapping.Core;
using System.Windows.Data;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls.Resources;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class BaseMapItemConfigControl : Control
    {
        public BaseMapItemConfigControl()
        {
            DefaultStyleKey = typeof(BaseMapItemConfigControl);
            BrowseForThumbnailImageCommand = new DelegateCommand(onBrowseForThumbnailImage);
            BrowseForUrlCommand = new DelegateCommand(onBrowseForUrl);
        }

        #region BaseMapInfo
        /// <summary>
        /// 
        /// </summary>
        public BaseMapInfo BaseMapInfo
        {
            get { return GetValue(BaseMapInfoProperty) as BaseMapInfo; }
            set { SetValue(BaseMapInfoProperty, value); }
        }

        /// <summary>
        /// Identifies the BaseMapInfo dependency property.
        /// </summary>
        public static readonly DependencyProperty BaseMapInfoProperty =
            DependencyProperty.Register(
                "BaseMapInfo",
                typeof(BaseMapInfo),
                typeof(BaseMapItemConfigControl),
                new PropertyMetadata(null, OnBaseMapInfoPropertyChanged));

        /// <summary>
        /// BaseMapInfoProperty property changed handler.
        /// </summary>
        /// <param name="d">BaseMapItemConfigControl that changed its BaseMapInfo.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnBaseMapInfoPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BaseMapItemConfigControl source = d as BaseMapItemConfigControl;
            source.DataContext = e.NewValue;
        }
        #endregion 

        string[] basemapTypes = new string[] { Strings.ArcGISServer, Strings.BingMaps, Strings.OpenStreetMap };
        public string[] BaseMapTypes { get { return basemapTypes; } }

        public ICommand BrowseForThumbnailImageCommand { get; private set; }
        public ICommand BrowseForUrlCommand { get; private set; }

        private void onBrowseForThumbnailImage(object parameter)
        {
            if (AppCoreHelper.Current != null)
                AppCoreHelper.Current.BrowseForFile(onBrowseComplete, new string[] { ".png" }, "Images");
        }

        private void onBrowseComplete(object sender, BrowseCompleteEventArgs args)
        {
            if (BaseMapInfo == null)
                return;
            BaseMapInfo.ThumbnailImage = args.RelativeUri;
        }

        private void onBrowseForUrl(object parameter)
        {
            
        }
    }

    public class BaseMapTypeConfigurableToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BaseMapType baseMapType = (BaseMapType)value;
            BaseMapType compareType = (BaseMapType)Enum.Parse(typeof(BaseMapType), parameter as string, false); return baseMapType == compareType ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BaseMapTypeEnumToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)((BaseMapType)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int enumVal = (int)value;            
            switch (enumVal)
            {
                case 1:
                    return BaseMapType.BingMaps;
                case 2:
                    return BaseMapType.OpenStreetMap;
                default:
                    return BaseMapType.ArcGISServer;
            }
        }
    }

    public class BingMapsTypeToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string name = value as string;
            switch (name)
            {                
                case "Aerial":
                    return 1;
                case "Hybrid":
                    return 2;
                default:
                    return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int enumVal = (int)value;
            switch (enumVal)
            {
                case 1:
                    return "Aerial";
                case 2:
                    return "Hybrid";
                default:
                    return "Roads";
            }
        }
    }

    public class OSMTypeToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string name = value as string;
            switch (name)
            {                
                case "CycleMap":
                    return 1;
                case "NoName":
                    return 2;
                default:
                    return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int enumVal = (int)value;
            switch (enumVal)
            {               
                case 1:
                    return "CycleMap";
                case 2:
                    return "NoName";
                default:
                    return "Mapnik";
            }            
        }
    }
}
