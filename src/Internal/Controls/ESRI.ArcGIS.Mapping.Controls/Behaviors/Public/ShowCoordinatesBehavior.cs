/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel.Composition;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Mapping.Core;
using System.Windows;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(Behavior<Map>))]
	[DisplayName("ShowMapCoordinatesDisplayName")]
	[Category("CategoryMouseEvents")]
	[Description("ShowMapCoordinatesDescription")]
    public class ShowCoordinatesBehavior : ESRI.ArcGIS.Client.Behaviors.ShowCoordinatesBehavior
    {
    }
}
