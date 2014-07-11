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
using System.Windows.Threading;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public static class InfoWindowExtensions
    {
        public static void Show(this InfoWindow _infoWindow, MapPoint anchorPoint)
        {
            if (_infoWindow != null)
            {
                _infoWindow.Anchor = anchorPoint;

                if (_infoWindow.Placement == InfoWindow.PlacementMode.Auto)
                {
                    _infoWindow.IsOpen = true;
                    return;
                }

                Map map = _infoWindow.Map;
                _infoWindow.IsOpen = false;
                _infoWindow.UpdateLayout();

                // Give the Layout time to size/arrange
                _infoWindow.Dispatcher.BeginInvoke(() =>
                {
                    //if (_infoWindow.Placement != InfoWindow.PlacementMode.Auto)
                    //{
                        if (double.IsNaN(_infoWindow.ActualHeight) || _infoWindow.ActualHeight == 0.0
                           || double.IsNaN(_infoWindow.ActualWidth) || _infoWindow.ActualWidth == 0.0)
                            return;
                        double windowHeight = _infoWindow.ActualHeight;
                        double windowWidth = _infoWindow.ActualWidth;
                        Point point = map.MapToScreen(_infoWindow.Anchor);
                        MapPoint mapTopLeft = map.ScreenToMap(new Point(point.X - windowWidth / 2.0, point.Y - windowHeight));
                        MapPoint mapBottomRight = map.ScreenToMap(new Point(point.X + windowWidth / 2.0, point.Y));
                        double mapWindowHeight = mapTopLeft.Y - mapBottomRight.Y;
                        double mapWindowWidth = mapBottomRight.X - mapTopLeft.X;

                        MapPoint newCenter = new MapPoint(map.Extent.XMin + ((map.Extent.XMax - map.Extent.XMin) / 2.0),
                            map.Extent.YMin + ((map.Extent.YMax - map.Extent.YMin) / 2.0));
                        bool panRequired = false;
                        double yDiff = (mapTopLeft.Y + mapWindowHeight * 0.3) - map.Extent.YMax;
                        if (yDiff > 0)
                        {
                            newCenter.Y += yDiff;
                            panRequired = true;
                        }

                        double xMinDiff = map.Extent.XMin - (mapTopLeft.X - mapWindowWidth * 0.2);
                        if (xMinDiff > 0)
                        {
                            newCenter.X -= xMinDiff;
                            panRequired = true;
                        }
                        else
                        {

                            double xMaxDiff = (mapBottomRight.X + mapWindowWidth * 0.2) - map.Extent.XMax;
                            if (xMaxDiff > 0)
                            {
                                newCenter.X += xMaxDiff;
                                panRequired = true;
                            }
                        }
                        if (panRequired)
                            map.PanTo(newCenter);
                    //}

                    _infoWindow.IsOpen = true;
                });
            }
        }

    }
}
