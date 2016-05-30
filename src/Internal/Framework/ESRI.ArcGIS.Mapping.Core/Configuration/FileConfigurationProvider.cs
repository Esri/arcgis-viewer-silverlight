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
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core.Resources;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class FileConfigurationProvider : ConfigurationProvider
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

        public FileConfigurationProvider()
            : this(null)
        {
        }

        public FileConfigurationProvider(DataFile configurationFile)
        {
            ConfigurationFile = configurationFile;
            FileLoader = new FileLoader(configurationFile);
            FileLoader.GetFileAsTextCompleted += new EventHandler<GetFileAsTextCompletedEventArgs>(FileLoader_FileLoaded);
            FileLoader.GetFileAsTextFailed += new EventHandler<ExceptionEventArgs>(FileLoader_FileLoadFailed);
        }

        void FileLoader_FileLoadFailed(object sender, ExceptionEventArgs e)
        {
            object[] userState = (e.UserState as object[]);
            if (userState == null || userState.Length < 2)
                return;
            EventHandler<ExceptionEventArgs> onFailed = userState[2] as EventHandler<ExceptionEventArgs>;
            if (onFailed == null)
                return;
            if (e.Exception != null)
            {
                onFailed(this, new ExceptionEventArgs(e.Exception, userState[0]));
            }
            else
            {
                onFailed(this, new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionUnknownError), e.UserState));
            }
        }

        void FileLoader_FileLoaded(object sender, GetFileAsTextCompletedEventArgs e)
        {
            object[] userState = (e.UserState as object[]);
            if (userState == null || userState.Length < 2)
                return;
            
            EventHandler<ExceptionEventArgs> onFailed = userState[2] as EventHandler<ExceptionEventArgs>;                
            string configFileXml = e.FileContents;
            if (string.IsNullOrEmpty(configFileXml))
            {
                if (onFailed != null)
                    onFailed(this, new ExceptionEventArgs(new Exception(Strings.ExceptionConfigurationXmlIsEmpty), e.UserState));
                return;
            }
            EventHandler<GetConfigurationCompletedEventArgs> onCompleted = userState[1] as EventHandler<GetConfigurationCompletedEventArgs>;
            LoadConfigurationFromXmlString(configFileXml, userState[0], onCompleted, onFailed);
        }

        public static Map ParseConfigurationXml(string configFileXml)
        {
            string xmlPrefix = @"<?xml version=""1.0"" encoding=""utf-8""?>";
            if (configFileXml.StartsWith(xmlPrefix, StringComparison.Ordinal))
                configFileXml = configFileXml.Substring(xmlPrefix.Length);
            Map map = null;

            map = System.Windows.Markup.XamlReader.Load(configFileXml) as Map;
            return map;
        }

        protected void LoadConfigurationFromXmlString(string configFileXml, object userState, 
            EventHandler<GetConfigurationCompletedEventArgs> onCompleted,
            EventHandler<ExceptionEventArgs> onFailed)
        {
            Map map = null;
            try
            {
                map = ParseConfigurationXml(configFileXml);
            }
            catch(Exception ex)
            {
                if (onFailed != null)
                    onFailed(this, new ExceptionEventArgs(ex, userState));
                return;
            }

            if(onCompleted != null)
                onCompleted(this, new GetConfigurationCompletedEventArgs() { Map = map, UserState = userState });
        }

        public override void GetConfigurationAsync(object userState, EventHandler<GetConfigurationCompletedEventArgs> onCompleted, EventHandler<ExceptionEventArgs> onFailed)
        {
            if (ConfigurationFile == null)
                throw new InvalidOperationException(Resources.Strings.ExceptionMustSpecifyConfigurationFile);

            FileLoader.GetFileAsTextAsync(new object[] { userState, onCompleted, onFailed });
        }
    }
}
