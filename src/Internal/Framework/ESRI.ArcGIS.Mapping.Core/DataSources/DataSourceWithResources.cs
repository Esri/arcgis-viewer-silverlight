/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
using System.Windows.Threading;

namespace ESRI.ArcGIS.Mapping.Core.DataSources
{
    public abstract class DataSourceWithResources : DataSource, IDataSourceWithResources
    {
        #region IDataSourceWithResources Members

        public abstract Resource GetResource(string connectionString, string proxyUrl);
        public abstract void GetCatalog(string connectionString, string proxyUrl, Filter filter, object userState);
        public abstract void GetChildResourcesAsync(Resource parentResource, Filter filter, object userState);
        public abstract void CreateLayerAsync(Resource layerResource, SpatialReference mapSpatialReference, object userState);
        public abstract bool IsResourceSelectable(Resource resource, Filter filter);
        public abstract bool SupportsChildResources(Resource resource, Filter filter);
        public virtual void CancelAllCurrentRequests()
        {

        }

        public event EventHandler<GetCatalogCompletedEventArgs> GetCatalogCompleted;

        public event EventHandler<ExceptionEventArgs> GetCatalogFailed;

        public event EventHandler<CreateLayerCompletedEventArgs> CreateLayerCompleted;

        public event EventHandler<ExceptionEventArgs> CreateLayerFailed;

        public event EventHandler<GetChildResourcesCompletedEventArgs> GetChildResourcesCompleted;

        public event EventHandler<ExceptionEventArgs> GetChildResourcesFailed;

        #endregion

        protected virtual void OnGetChildResourcesCompleted(GetChildResourcesCompletedEventArgs args)
        {
            if (GetChildResourcesCompleted != null)
                GetChildResourcesCompleted(this, args);
        }

        protected virtual void OnGetChildResourcesFailed(ExceptionEventArgs args)
        {
            if (GetChildResourcesFailed != null)
                GetChildResourcesFailed(this, args);
        }

        protected virtual void OnCreateLayerCompleted(CreateLayerCompletedEventArgs args)
        {
            if (CreateLayerCompleted != null)
                CreateLayerCompleted(this, args);
        }

        protected virtual void OnCreateLayerFailed(ExceptionEventArgs args)
        {
            if (CreateLayerFailed != null)
                CreateLayerFailed(this, args);
        }

        protected virtual void OnGetCatalogCompleted(GetCatalogCompletedEventArgs args)
        {
            if (GetCatalogCompleted != null)
                GetCatalogCompleted(this, args);
        }

        protected virtual void OnGetCatalogFailed(ExceptionEventArgs args)
        {
            if (GetCatalogFailed != null)
                GetCatalogFailed(this, args);
        }
    }
}
