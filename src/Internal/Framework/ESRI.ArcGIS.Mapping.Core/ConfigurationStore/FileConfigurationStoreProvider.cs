/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class FileConfigurationStoreProvider : ConfigurationStoreProvider
    {
        private FileLoader FileLoader { get; set; }
        
        private DataFile _dataFile;
        public DataFile ConfigurationFile
        {
            get
            {
                return _dataFile;
            }
            set
            {
                _dataFile = value;
                if(FileLoader != null)
                    FileLoader.File = value;
            }
        }

        public FileConfigurationStoreProvider() : this(null)
        {            
        }

        public FileConfigurationStoreProvider(DataFile configurationFile)
        {
            ConfigurationFile = configurationFile;
            FileLoader = new FileLoader(configurationFile);
            FileLoader.GetFileAsTextCompleted += new EventHandler<GetFileAsTextCompletedEventArgs>(FileLoader_FileLoaded);
            FileLoader.GetFileAsTextFailed += new EventHandler<ExceptionEventArgs>(FileLoader_FileLoadFailed);
        }

        void FileLoader_FileLoadFailed(object sender, ExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                OnGetConfigurationStoreFailed(new ExceptionEventArgs(e.Exception, e.UserState));
            }
            else
            {
                OnGetConfigurationStoreFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionUnknownError), e.UserState));
            }
        }

        void FileLoader_FileLoaded(object sender, GetFileAsTextCompletedEventArgs e)
        {
            string configFileXml = e.FileContents;            
            if (string.IsNullOrEmpty(configFileXml))
            {
                OnGetConfigurationStoreFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionConfigurationXmlIsEmpty), e.UserState));
                return;
            }
            
            try
            {
                ConfigurationStore store = null;
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(configFileXml)))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(ConfigurationStore));
                    store = serializer.ReadObject(ms) as ConfigurationStore;                    
                }
                OnGetConfigurationStoreCompleted(new GetConfigurationStoreCompletedEventArgs() { ConfigurationStore = store, UserState = e.UserState });
            }
            catch (Exception ex)
            {
                OnGetConfigurationStoreFailed(new ExceptionEventArgs(ex, e.UserState));
            }
        }        

        public override void GetConfigurationStoreAsync(object userState)
        {
            if (ConfigurationFile == null)
                throw new InvalidOperationException(Resources.Strings.ExceptionMustSpecifyConfigurationFile);
            FileLoader.GetFileAsTextAsync(userState);
        }        
    }
}
