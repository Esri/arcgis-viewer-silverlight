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
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Core.DataSources;
using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.DataSources.ArcGISServer;

namespace ESRI.ArcGIS.Mapping.DataSources
{
    public class DataSourceProvider : DataSourceProviderBase
    {        
        public DataSourceProvider() : base()
        {
            AddDataSources(new DataSource[] { 
                new DataSources.ArcGISServer.ArcGISServerDataSource(),
                new DataSources.BingMaps.BingMapsDataSource(),
                new DataSources.OpenStreetMap.OpenStreetMapDataSource(),
                new DataSources.SpatialDataService.SpatialDataServiceDataSource()});
        }

        public override DataSource CreateDataSourceForBaseMapType(BaseMapType baseMapType)
        {            
            switch (baseMapType)
            {
                case BaseMapType.ArcGISServer:
                    return new DataSources.ArcGISServer.ArcGISServerDataSource();
                case BaseMapType.BingMaps:
                    return new BingMaps.BingMapsDataSource();
                case BaseMapType.OpenStreetMap:
                    return new OpenStreetMap.OpenStreetMapDataSource();
                default:
                    return null;
            }
        }

        public DataSource CreateDataSourceForBaseMapType(BaseMapInfo baseMapInfo)
        {
            DataSource dataSource = CreateDataSourceForBaseMapType(baseMapInfo.BaseMapType);
            if (baseMapInfo.BaseMapType == BaseMapType.ArcGISServer)
                ((ArcGISServerDataSource)dataSource).ProxyUrl = baseMapInfo.ProxyUrl;

            return dataSource;
        }

        public override DataSource CreateNewInstance(string dataSourceId)
        {
            switch (dataSourceId)
            {
                case Constants.ArcGISServer:
                    return new ArcGISServer.ArcGISServerDataSource();
                case Constants.BingMaps:
                    return new BingMaps.BingMapsDataSource();       
                case Constants.OpenStreetMap:
                    return new OpenStreetMap.OpenStreetMapDataSource();
                case Constants.SpatialDataService:
                    return new SpatialDataService.SpatialDataServiceDataSource();
                default:
                    return null;
            }
        }

        public override DataSource CreateNewDataSourceForConnectionType(ConnectionType connectionType)
        {
            switch (connectionType)
            {
                case ConnectionType.ArcGISServer:
                    return new ArcGISServer.ArcGISServerDataSource();
                case ConnectionType.SpatialDataService:
                    return new SpatialDataService.SpatialDataServiceDataSource();
                default:
                    return null;
            }
        }

        public override IEnumerable<IDataSourceWithResources> GetAllDataSourcesWhichSupportResources()
        {
            List<IDataSourceWithResources> dataSources = new List<IDataSourceWithResources>();
            foreach (IDataSourceWithResources dataSource in base.GetAllDataSourcesWhichSupportResources())
            {
                DataSource ds = dataSource as DataSource;
                if (ds == null)
                    continue;
                IDataSourceWithResources newInstance = CreateNewInstance(ds.ID) as IDataSourceWithResources;
                if (newInstance == null)
                    continue;
                dataSources.Add(newInstance);
            }
            return dataSources;
        }

        public override ConnectionType MapDataSourceToConnectionType(IDataSourceWithResources dataSourceWithResources)
        {
            if (dataSourceWithResources == null)
                return ConnectionType.Unknown;
            DataSource dataSource = dataSourceWithResources as DataSource;
            if(dataSource == null)
                return ConnectionType.Unknown;
            switch (dataSource.ID)
            {
                case Constants.ArcGISServer:
                    return ConnectionType.ArcGISServer;
                case Constants.SpatialDataService:
                    return ConnectionType.SpatialDataService;
                case Constants.SharePoint:
                    return ConnectionType.SharePoint;
            }
            return ConnectionType.Unknown;
        }
    }
}
