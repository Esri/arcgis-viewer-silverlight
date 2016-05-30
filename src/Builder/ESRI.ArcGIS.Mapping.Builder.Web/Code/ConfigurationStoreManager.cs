/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.IO;

namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    public static class ConfigurationStoreManager
    {
        private const string TEMPLATECONFIGPATH = "~/App_Data/Basemaps.xml";
        static object lockObject = new object();
        private static string TemplateConfigPath
        {
            get
            {
                return ServerUtility.MapPath(TEMPLATECONFIGPATH);
            }
        }

        public static string GetConfigurationStoreXml()
        {
            string configPath = TemplateConfigPath;
            return GetFileContents(configPath);
        }

        public static string GetFileContents(string configPath)
        {
            string configurationStoreXml = null;
            if (File.Exists(configPath))
            {
                lock (lockObject)
                {
                    using (System.IO.StreamReader file = new System.IO.StreamReader(configPath))
                    {
                        configurationStoreXml = file.ReadToEnd();
                    }
                }
            }
            return configurationStoreXml;
        }

        public static bool SaveConfigurationStore(string configurationStoreXml)
        {
            if (configurationStoreXml == null)
                return false;
            bool success = false;
            string configPath = TemplateConfigPath;
            if (File.Exists(configPath))
            {
                lock (lockObject)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(configPath))
                    {
                        file.Write(configurationStoreXml);
                        file.Flush();
                    }
                }
            }
            return success;
        }
    }
}
