/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Core.DataSources
{
    internal static class DataSourceManager
    {        
        public static object lockObject = new object();

        private static Dictionary<string, DataSource> _pluginDataSources;
        private static Dictionary<string, DataSource> PluginDataSources
        {
            get
            {
                if (_pluginDataSources == null)
                {
                    lock (lockObject)
                    {
                        if (_pluginDataSources == null)
                        {
                            _pluginDataSources = new Dictionary<string, DataSource>();
                        }
                    }
                }
                return _pluginDataSources;
            }
        }

        public static void RegisterDataSource(DataSource dataSource)
        {
            if (dataSource == null)
                throw new ArgumentNullException("dataSource");
            if (string.IsNullOrEmpty(dataSource.ID))
				throw new ArgumentException(Resources.Strings.ExceptionIdforDataSourceMustNotBeEmpty, "dataSource");
            PluginDataSources[dataSource.ID] = dataSource;
        }

        public static void UnRegisterDataSource(DataSource dataSource)
        {
            PluginDataSources.Remove(dataSource.ID);
        }

        public static void UnRegisterDataSource(string dataSourceId)
        {
            if (PluginDataSources.ContainsKey(dataSourceId))
                PluginDataSources.Remove(dataSourceId);
        }

        public static DataSource GetDataSource(string dataSourceId)
        {
            DataSource dataSource = null;
            PluginDataSources.TryGetValue(dataSourceId, out dataSource);
            return dataSource;
        }

        public static IEnumerable<IDataSourceWithResources> GetAllDataSourcesWhichSupportResources()
        {
            List<IDataSourceWithResources> dataSources = new List<IDataSourceWithResources>();
            foreach (DataSource dataSource in PluginDataSources.Values)
            {
                IDataSourceWithResources dataSourceWithResource = dataSource as IDataSourceWithResources;
                if (dataSourceWithResource == null)
                    continue;
                dataSources.Add(dataSourceWithResource);
            }
            return dataSources;
        }
    }
}
