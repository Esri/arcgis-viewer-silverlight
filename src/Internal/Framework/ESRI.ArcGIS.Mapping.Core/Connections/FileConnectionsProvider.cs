/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class FileConnectionsProvider : ConnectionsProvider
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
                if (FileLoader != null)
                    FileLoader.File = value;
            }
        }

        public FileConnectionsProvider()
            : this(null)
        {
        }

        public override void GetConnectionsAsync(object userState)
        {
            if (ConfigurationFile == null)
                throw new InvalidOperationException(Resources.Strings.ExceptionMustSpecifyConfigurationFile);
            FileLoader.GetFileAsTextAsync(userState);
        }

        
        public FileConnectionsProvider(DataFile configurationFile)
        {
            ConfigurationFile = configurationFile;
            FileLoader = new FileLoader(configurationFile);
            FileLoader.GetFileAsTextCompleted += new EventHandler<GetFileAsTextCompletedEventArgs>(FileLoader_GetFileAsTextCompleted);
            FileLoader.GetFileAsTextFailed += new EventHandler<ExceptionEventArgs>(FileLoader_GetFileAsTextFailed);
        }

        void FileLoader_GetFileAsTextFailed(object sender, ExceptionEventArgs e)
        {
            if (e.Exception != null)
                OnGetConnectionsFailed(new ExceptionEventArgs(e.Exception, e.UserState));
            else
                OnGetConnectionsFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionUnknownError), e.UserState));
        }

        void FileLoader_GetFileAsTextCompleted(object sender, GetFileAsTextCompletedEventArgs e)
        {
            string configFileXml = e.FileContents;
            if (string.IsNullOrEmpty(configFileXml))
            {
                OnGetConnectionsFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionConfigurationXmlIsEmpty), e.UserState));
                return;
            }

            try
            {
                List<Connection> connections = null;
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(configFileXml)))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(List<Connection>));
                    connections = serializer.ReadObject(ms) as List<Connection>;
                }
                AddUserConnectionsToAvailableConnections(connections);
                OnGetConnectionsCompleted(new GetConnectionsCompletedEventArgs() { Connections = connections, UserState = e.UserState });
            }
            catch (Exception ex)
            {
                OnGetConnectionsFailed(new ExceptionEventArgs(ex, e.UserState));
            }
        }
    }
}
