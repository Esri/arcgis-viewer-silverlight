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
using System.Windows.Data;
using ESRI.ArcGIS.Client;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Toolkit.DataSources;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class LayerDisplayIconConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FeatureLayer featureLayer = value as FeatureLayer;
            if (featureLayer != null)
            {
                if (featureLayer.IsReadOnly)
                {
                    if (featureLayer.Graphics.Count == 0)
                    {
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/layer.png", UriKind.Relative));
                    }
                    else
                    {
                        ESRI.ArcGIS.Client.Geometry.Geometry g = featureLayer.Graphics[0].Geometry;
                        if (g == null)
                            return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/layer.png", UriKind.Relative));

                        if (g is MapPoint)
                            return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/point.png", UriKind.Relative));
                        else if (g is ESRI.ArcGIS.Client.Geometry.Polyline)
                            return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/line.png", UriKind.Relative));
                        else if (g is ESRI.ArcGIS.Client.Geometry.Polygon)
                            return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/poly.png", UriKind.Relative));
                        else
                            return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/layer.png", UriKind.Relative));
                    }
                }
                else
                    return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/editableLayer.png", UriKind.Relative));
            }

            if (value is GeoRssLayer || value is CustomGeoRssLayer)
                return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/geoRssFeed.png", UriKind.Relative));

            if (value is HeatMapLayer)
                return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/heatMap.png", UriKind.Relative));

            if (value is CustomGraphicsLayer) // This would be the case for SharePoint layer
                return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/view.png", UriKind.Relative));

            if (value is WmsLayer)
                return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/wms.png", UriKind.Relative));

            if (value is ArcGISDynamicMapServiceLayer || value is ArcGISTiledMapServiceLayer)
                return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/mapService.png", UriKind.Relative));

            if (value is TileLayer)
                return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/bing16.png", UriKind.Relative));            

            if (value is OpenStreetMapLayer)
                return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/OpenStreetMap16.png", UriKind.Relative));            

            if (value is ArcGISImageServiceLayer)
                return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/imageService.png", UriKind.Relative));
            
            GraphicsLayer graphicsLayer = value as GraphicsLayer;
            if (graphicsLayer != null) // Generic graphics layer
            {
                if (graphicsLayer.Graphics.Count == 0)
                {
                    return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/layer.png", UriKind.Relative));
                }
                else
                {
                    ESRI.ArcGIS.Client.Geometry.Geometry g = graphicsLayer.Graphics[0].Geometry;
                    if (g == null)
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/layer.png", UriKind.Relative));

                    if (g is MapPoint)
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/point.png", UriKind.Relative));
                    else if (g is ESRI.ArcGIS.Client.Geometry.Polyline)
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/line.png", UriKind.Relative));
                    else if (g is ESRI.ArcGIS.Client.Geometry.Polygon)
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/poly.png", UriKind.Relative));
                    else
                        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/layer.png", UriKind.Relative));
                }
            }
            return value;
        }
        #endregion

        #region IValueConverter Members


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ErrorDisplayIconConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/caution16.png", UriKind.Relative));
        }
        #endregion

        #region IValueConverter Members


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
