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
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core.DataSources;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class ScaleBarExtensions
    {
        public static readonly DependencyProperty ScaleBarMapUnitProperty =
                DependencyProperty.RegisterAttached("ScaleBarMapUnit", typeof(MapUnit), typeof(Map), null);
        public static void SetScaleBarMapUnit(Map o, MapUnit value)
        {
            o.SetValue(ScaleBarMapUnitProperty, value);
        }

        public static MapUnit GetScaleBarMapUnit(Map o)
        {
            object unitObj = o.GetValue(ScaleBarMapUnitProperty);
            if(unitObj != null && unitObj is MapUnit)
                return (MapUnit)unitObj;

            return MapUnit.Undefined;
        }

        public static MapUnit ConvertFromArcGISMapUnits(string units)
        {
            switch (units)
            {
                case "esriUnknownUnits":
                    return MapUnit.Undefined;
                case "esriDecimalDegrees":
                    return MapUnit.DecimalDegrees;
                case "esriMillimeters":
                    return MapUnit.Millimeters;
                case "esriCentimeters":
                    return MapUnit.Centimeters;
                case "esriInches":
                    return MapUnit.Inches;
                case "esriDecimeters":
                    return MapUnit.Decimeters;
                case "esriFeet":
                    return MapUnit.Feet;
                case "esriYards":
                    return MapUnit.Yards;
                case "esriMeters":
                    return MapUnit.Meters;
                case "esriKilometers":
                    return MapUnit.Kilometers;
                case "esriMiles":
                    return MapUnit.Miles;
                case "esriNauticalMiles":
                    return MapUnit.NauticalMiles;
                default:
                    break;
            }

            return MapUnit.Undefined;
        }
    }
}
