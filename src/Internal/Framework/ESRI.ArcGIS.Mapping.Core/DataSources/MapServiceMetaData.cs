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
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcGIS.Mapping.Core.DataSources
{
    public enum MapUnit
    {
        // Summary:
        //     Undefined
        Undefined = -1,
        //
        // Summary:
        //     Decimal degrees
        DecimalDegrees = 0,
        //
        // Summary:
        //     Millimeters
        Millimeters = 10,
        //
        // Summary:
        //     Centimeters
        Centimeters = 100,
        //
        // Summary:
        //     Inches
        Inches = 254,
        //
        // Summary:
        //     Decimeters
        Decimeters = 1000,
        //
        // Summary:
        //     Feet
        Feet = 3048,
        //
        // Summary:
        //     Yards
        Yards = 9144,
        //
        // Summary:
        //     Meters
        Meters = 10000,
        //
        // Summary:
        //     Kilometers
        Kilometers = 10000000,
        //
        // Summary:
        //     Miles
        Miles = 16093440,
        //
        // Summary:
        //     Nautical Miles
        NauticalMiles = 18520000,
    }
}
