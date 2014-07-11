/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Core.DataSources;
using ESRI.ArcGIS.Mapping.DataSources;
using core = ESRI.ArcGIS.Mapping.Core;
using System.Windows.Data;
using System.Windows.Media.Imaging;


namespace ESRI.ArcGIS.Mapping.Controls
{

    public class ConnectionAddedEventArgs : EventArgs
    {
        public Connection Connection { get; set; }
    }

    public class ConnectionRemovedEventArgs : EventArgs
    {
        public Connection Connection { get; set; }
    }

    //public class ConnectionUpdatedEventArgs : EventArgs
    //{
    //    public Connection OldConnection { get; set; }
    //    public Connection NewConnection { get; set; }
    //}

    public class ResourceSelectedEventArgs : EventArgs
    {
        public Resource Resource { get; set; }
        public ConnectionType ConnectionType { get; set; }
    }

    public class LayerAddedEventArgs : EventArgs
    {
        public Layer Layer { get; set; }
    }

    public class ConnectionDisplayNameConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Connection connection = value as Connection;
            if (connection != null)
            {
                string name = connection.Name;
                if (string.IsNullOrEmpty(name))
                {
                    if (Uri.IsWellFormedUriString(connection.Url, UriKind.Absolute))
                    {
                        Uri u = new Uri(connection.Url, UriKind.Absolute);
                        name = u.Host;
                        if (u.Port > 0 && u.Port != 80)
                            name = string.Format("{0}:{1}", name, u.Port);
                    }
                    else
                    {
                        name = connection.Url;
                    }
                }
                return name;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Not supported");
        }

        #endregion
    }

    public class ConnectionDisplayIconConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Connection connection = value as Connection;
            if (connection != null)
            {
                switch (connection.ConnectionType)
                {
                    case ConnectionType.ArcGISServer:
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/arcgisServer.png", UriKind.Relative));
                    case ConnectionType.SpatialDataService:
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/sdsServer.png", UriKind.Relative));
                    case ConnectionType.SharePoint:
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/sharePointServer.png", UriKind.Relative));
                    case ConnectionType.OpenDataServer:
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/odata.png", UriKind.Relative));
                    case ConnectionType.Unknown:
                        return null;
                }
                return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/server.png", UriKind.Relative));
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Not supported");
        }

        #endregion
    }

    public class ResourceNodeDisplayIconConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Resource resource = value as Resource;
            if (resource != null)
            {
                switch (resource.ResourceType)
                {
                    case ResourceType.Database:
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/database.png", UriKind.Relative));
                    case ResourceType.DatabaseTable:
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/databasetable.png", UriKind.Relative));
                    case ResourceType.Folder:
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/folder.png", UriKind.Relative));
                    case ResourceType.GroupLayer:
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/grouplayer.png", UriKind.Relative));
                    case ResourceType.Layer:
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/layer.png", UriKind.Relative));
                    case ResourceType.EditableLayer:
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/editableLayer.png", UriKind.Relative));
                    case ResourceType.MapServer:
                    case ResourceType.FeatureServer:
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/mapService.png", UriKind.Relative));
                    case ResourceType.ImageServer:
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/imageService.png", UriKind.Relative));
                    case ResourceType.SharePointList:
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/list.png", UriKind.Relative));
                    case ResourceType.SharePointView:
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/view.png", UriKind.Relative));
                    case ResourceType.GPServer:
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/GeoprocessingToolbox16.png", UriKind.Relative));
                    case ResourceType.GPTool:
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/GeoprocessingTool16.png", UriKind.Relative));
                    case ResourceType.ODataServer:
                    case ResourceType.ODataCollection:
                    case ResourceType.ODataFeed:
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/odata.png", UriKind.Relative));
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public static class TreeViewItemExtensions
    {
        public static readonly DependencyProperty ConnectionProperty = DependencyProperty.RegisterAttached("Connection", typeof(Connection), typeof(TreeViewItem), null);
    }
}
