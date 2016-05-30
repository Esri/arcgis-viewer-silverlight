/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Core
{
    public interface IConfigurableLayer
    {
        string DisplayName { get; set; }
        bool VisibleInLayerList { get; set; }
        Layer Layer { get; }
        GeometryType GeometryType { get; }
    }

    public enum GeometryType { Unknown, Point, PolyLine, Polygon }
}
