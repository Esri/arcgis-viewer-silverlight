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

namespace ESRI.ArcGIS.Mapping.GP
{
    public enum GPParameterType
    {
        Boolean,
        Double,
        Long,
        String,
        Date,
        LinearUnit,
        FeatureLayer,
        RecordSet,
        DataFile,
        RasterData,
        RasterDataLayer,
        MultiValueString,
        MapServiceLayer
    }
}
