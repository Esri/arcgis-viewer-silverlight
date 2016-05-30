/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcGIS.Mapping.Core.DataSources
{
    public class GetMapServiceMetaDataCompletedEventArgs : EventArgs
    {
        public SpatialReference SpatialReference { get; set; }
        public MapUnit MapUnit { get; set; }
        public Envelope FullExtent { get; set; }
        public Envelope InitialExtent { get; set; }
        public bool IsCached { get; set; }
        public object UserState { get; set; }
    }    
    
    public class CreateLayerCompletedEventArgs : EventArgs
    {
        public Layer Layer { get; set; }
        public GeometryType GeometryType { get; set; }
        public object UserState { get; set; }
    }

    public class GetChildResourcesCompletedEventArgs : EventArgs
    {
        public IEnumerable<Resource> ChildResources { get; set; }
        public object UserState { get; set; }
    }

    public class GetCatalogCompletedEventArgs : EventArgs
    {
        public IEnumerable<Resource> ChildResources { get; set; }
        public object UserState { get; set; }
        public string Error { get; set; }
    }

    public class GetCatalogFailedEventArgs : ExceptionEventArgs
    {
        public GetCatalogFailedEventArgs(string ex, object userState) : base(ex, userState) { }
        public GetCatalogFailedEventArgs(Exception ex,object userState) : base(ex, userState) { }

        public bool DisplayErrorMessage { get; set; }
    }
}
