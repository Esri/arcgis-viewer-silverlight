/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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
using ESRI.ArcGIS.Mapping.Builder.ApplicationBuilder;
using System.ServiceModel;
using ESRI.ArcGIS.Mapping.Builder.FileExplorer;

namespace ESRI.ArcGIS.Mapping.Builder
{
    internal static class WCFProxyFactory
    {
        public static string UserId { get; set; }
        private static string GetServiceEndpoint(string servicePath)
        {
            Uri uri = App.Current.Host.Source;            
            
            // TODO:- This makes an assumption that the xap is at the root of the folder. See if we can 
            // improve this
            string baseUri = uri.AbsoluteUri; 
            int pos = baseUri.LastIndexOf(".xap", StringComparison.InvariantCultureIgnoreCase);
            if(pos > -1)
                baseUri = baseUri.Substring(0, pos);
            pos = baseUri.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase);
            if (pos > -1)
                baseUri = baseUri.Substring(0, pos);
            string serviceUri = baseUri.TrimEnd('/');
            if (!string.IsNullOrEmpty(servicePath))
                serviceUri += '/' + servicePath.TrimStart('/');
            return serviceUri;
        }

        public static FileExplorerClient CreateFileExplorerProxy()
        {
            TimeSpan timeout = TimeSpan.FromMinutes(3);
            FileExplorerClient client = new FileExplorerClient(UserId);
            return client;
        }

        public static ApplicationBuilderClient CreateApplicationBuilderProxy()
        {
            TimeSpan timeout = TimeSpan.FromMinutes(3);
            ApplicationBuilderClient client = new ApplicationBuilderClient(UserId);
            return client;
        }
    }
}
