/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Core.DataSources
{
    public abstract class DataSourceProviderBase
    {
        public abstract DataSource CreateNewInstance(string dataSourceId);
        public abstract DataSource CreateDataSourceForBaseMapType(BaseMapType baseMapType);
        public abstract DataSource CreateNewDataSourceForConnectionType(ConnectionType connectionType);
        public abstract ConnectionType MapDataSourceToConnectionType(IDataSourceWithResources dataSource);

        protected void AddDataSources(IEnumerable<DataSource> dataSources)
        {
            if (dataSources != null)
            {
                foreach (DataSource dataSource in dataSources)
                    DataSourceManager.RegisterDataSource(dataSource);
            }
        }

        protected void AddDataSource(DataSource dataSource)
        {
            if (dataSource != null)
                DataSourceManager.RegisterDataSource(dataSource);
        }

        protected void RemoveDataSource(DataSource dataSource)
        {
            if (dataSource != null)
                DataSourceManager.RegisterDataSource(dataSource);
        }

        public virtual DataSource GetDataSource(string dataSourceId)
        {
            return DataSourceManager.GetDataSource(dataSourceId);
        }

        public virtual IEnumerable<IDataSourceWithResources> GetAllDataSourcesWhichSupportResources()
        {
            return DataSourceManager.GetAllDataSourcesWhichSupportResources();
        }

        public virtual bool ConnectionTypeSupportsFilter(ConnectionType type, Filter filter)
        {
            switch (type)
            {
                case ConnectionType.Unknown:
                case ConnectionType.ArcGISServer:
                    return true;
                case ConnectionType.SharePoint:
                case ConnectionType.SpatialDataService:
                    if ((filter & Filter.None) == Filter.None ||
                        (filter & Filter.SpatiallyEnabledResources) == Filter.SpatiallyEnabledResources)
                        return true;
                    return false;
            }
            return false;
        }
    }
}
