/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/


using System;
using ESRI.ArcGIS.Client.Geometry;
using System.Windows.Threading;
using System.Windows.Controls;
namespace ESRI.ArcGIS.Mapping.Core.DataSources
{
    public interface IDataSourceWithResources
    {
        Resource GetResource(string connectionString, string proxyUrl);
        void GetCatalog(string connectionString, string proxyUrl, Filter filter, object userState);
        void GetChildResourcesAsync(Resource parentResource, Filter filter, object userState);
        void CreateLayerAsync(Resource layerResource, SpatialReference mapSpatialReference, object userState);
        bool IsResourceSelectable(Resource resource, Filter filter);
        bool SupportsChildResources(Resource resource, Filter filter);
        void CancelAllCurrentRequests();

        event EventHandler<GetCatalogCompletedEventArgs> GetCatalogCompleted;
        event EventHandler<ExceptionEventArgs> GetCatalogFailed;
        event EventHandler<CreateLayerCompletedEventArgs> CreateLayerCompleted;
        event EventHandler<ExceptionEventArgs> CreateLayerFailed;
        event EventHandler<GetChildResourcesCompletedEventArgs> GetChildResourcesCompleted;
        event EventHandler<ExceptionEventArgs> GetChildResourcesFailed;        
    }
}
