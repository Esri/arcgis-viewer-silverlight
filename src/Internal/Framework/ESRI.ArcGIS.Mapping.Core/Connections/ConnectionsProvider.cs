/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class ConnectionsProvider
    {
        const string USER_CONNECTIONS = "esri.arcgis.mapping.viewer.user.connections.key";
        public virtual void GetConnectionsAsync(object userState)
        {
            List<Connection> connections = getDefaultConnectionsFromEmbeddedResourceFile();
            if (connections == null)
            {
                OnGetConnectionsFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionUnableToRetrieveConnectionsFromEmbeddedResource), userState));
            }
            else
                OnGetConnectionsCompleted(new GetConnectionsCompletedEventArgs() { Connections = connections, UserState = userState });
        }

        public virtual void AddConnection(Connection newConnection)
        {
            // Deriving classes are required to update their backing store)

            //Add to isolated storage
            AddNewUserConnection(newConnection);
        }

        #region User connections support
        public void AddUserConnectionsToAvailableConnections(List<Connection> availableConnections)
        {
            List<Connection> userConnections = GetUserConnections();
            foreach (Connection item in userConnections)
            {
                if (availableConnections.FirstOrDefault<Connection>(con => con.Url == item.Url) == null)
                {
                    availableConnections.Add(item);
                }
            }
        }

        public void AddNewUserConnection(Connection newConnection)
        {
            List<Connection> userConnections = GetUserConnections();
            bool found = false;
            foreach (Connection item in userConnections)
            {
                if (item.Url == newConnection.Url)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                userConnections.Add(newConnection);
                SaveUserConnections(userConnections);
            }
        }

        public void DeleteUserConnection(Connection connection)
        {
            List<Connection> userConnections = GetUserConnections();
            foreach (Connection item in userConnections)
            {
                if (item.Url == connection.Url)
                {
                    userConnections.Remove(item);
                    SaveUserConnections(userConnections);
                    break;
                }
            }
        }

        public List<Connection> GetUserConnections()
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(USER_CONNECTIONS))
                IsolatedStorageSettings.ApplicationSettings[USER_CONNECTIONS] = new List<Connection>();
            return IsolatedStorageSettings.ApplicationSettings[USER_CONNECTIONS] as List<Connection>;
        }

        public void SaveUserConnections(List<Connection> connections)
        {
            IsolatedStorageSettings.ApplicationSettings[USER_CONNECTIONS] = connections;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }
        #endregion
        public virtual void DeleteConnection(Connection oldConnection)
        {
            // NO-OP (Derieving classes are required to update their backing store)

            //Delete from isolated storage
            DeleteUserConnection(oldConnection);
        }

        protected virtual void OnGetConnectionsCompleted(GetConnectionsCompletedEventArgs args)
        {
            if (GetConnectionsCompleted != null)
                GetConnectionsCompleted(this, args);
        }

        protected virtual void OnGetConnectionsFailed(ExceptionEventArgs args)
        {
            if (GetConnectionsFailed != null)
                GetConnectionsFailed(this, args);
        }

        public event EventHandler<GetConnectionsCompletedEventArgs> GetConnectionsCompleted;

        public event EventHandler<ExceptionEventArgs> GetConnectionsFailed;

        #region Helper Functions
        private List<Connection> getDefaultConnectionsFromEmbeddedResourceFile()
        {
            string connectionFileXml = string.Empty;
            Assembly a = typeof(ConnectionsProvider).Assembly;

            using (Stream str = a.GetManifestResourceStream("ESRI.ArcGIS.Mapping.Core.Embedded.DefaultConnections.xml"))
            {
                using (StreamReader rdr = new StreamReader(str))
                {
                    connectionFileXml = rdr.ReadToEnd();
                }
            }

            return ParseConnectionsXml(connectionFileXml);
        }

        public List<Connection> ParseConnectionsXml(string connectionFileXml)
        {
            if (string.IsNullOrEmpty(connectionFileXml))
                return null;

            string xmlPrefix = @"<?xml version=""1.0"" encoding=""utf-8""?>";
            if (connectionFileXml.StartsWith(xmlPrefix, StringComparison.Ordinal))
                connectionFileXml = connectionFileXml.Substring(xmlPrefix.Length);

            List<Connection> connections = null;
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(connectionFileXml)))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(List<Connection>));
                connections = serializer.ReadObject(ms) as List<Connection>;
            }
            return connections;
        }
        #endregion
    }
}
