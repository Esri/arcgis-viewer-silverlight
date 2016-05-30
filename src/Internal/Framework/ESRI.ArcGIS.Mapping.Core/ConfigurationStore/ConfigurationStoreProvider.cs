/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Runtime.Serialization;
using ESRI.ArcGIS.Mapping.Core.Resources;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class ConfigurationStoreProvider 
    {
        public virtual void GetConfigurationStoreAsync(object userState)
        {
            ConfigurationStore store = ReadStoreFromEmbeddedFile();
            if (store != null)
                OnGetConfigurationStoreCompleted(new GetConfigurationStoreCompletedEventArgs() { ConfigurationStore = store, UserState = userState });
            else
            {
                OnGetConfigurationStoreFailed(new ExceptionEventArgs(new Exception(Strings.ExceptionUnableToLoadconfigurationStoreFromEmbeddedResource), userState));
            }
        }        

        protected virtual void OnGetConfigurationStoreFailed(ExceptionEventArgs args)
        {
            if (GetConfigurationStoreFailed != null)
                GetConfigurationStoreFailed(this, args);
        }

        public static ConfigurationStore DefaultConfigurationStore;
        protected virtual void OnGetConfigurationStoreCompleted(GetConfigurationStoreCompletedEventArgs args)
        {
            DefaultConfigurationStore = args.ConfigurationStore;
            if(GetConfigurationStoreCompleted != null)
                GetConfigurationStoreCompleted(this, args);
        }

        public event EventHandler<GetConfigurationStoreCompletedEventArgs> GetConfigurationStoreCompleted;

        public event EventHandler<Core.ExceptionEventArgs> GetConfigurationStoreFailed;        

        #region Helper Functions
        public static ConfigurationStore ReadStoreFromEmbeddedFile()
        {
            string configFileXml = string.Empty;
            System.Reflection.Assembly a = typeof(ConfigurationStore).Assembly;
            using (System.IO.Stream str = a.GetManifestResourceStream("ESRI.ArcGIS.Mapping.Core.Embedded.DefaultConfigurationStore.xml"))
            {
                using (System.IO.StreamReader rdr = new System.IO.StreamReader(str))
                {
                    configFileXml = rdr.ReadToEnd();
                }
            }
            return ParseConfigurationStoreXml(configFileXml);
        }

        public static ConfigurationStore ParseConfigurationStoreXml(string configFileXml)
        {
            if (string.IsNullOrEmpty(configFileXml))
                return null;

            string xmlPrefix = @"<?xml version=""1.0"" encoding=""utf-8""?>";
            if (configFileXml.StartsWith(xmlPrefix, StringComparison.Ordinal))
                configFileXml = configFileXml.Substring(xmlPrefix.Length);

            ConfigurationStore store = null;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(configFileXml)))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(ConfigurationStore));
                store = serializer.ReadObject(ms) as ConfigurationStore;
            }
            return store;
        }
        #endregion
    }
}
