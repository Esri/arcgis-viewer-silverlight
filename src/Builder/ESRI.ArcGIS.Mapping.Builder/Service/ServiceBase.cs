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

namespace ESRI.ArcGIS.Mapping.Builder.Service
{
    public class ServiceBase 
    {
        public ServiceBase(string userId)
        {
            UserId = userId;
        }

        private string UserId { get; set; }

        private string baseUrl;
        private string BaseUrl
        {
            get
            {
                if (string.IsNullOrEmpty(baseUrl))
                {
                    baseUrl = Application.Current.Host.Source.AbsoluteUri;
                    baseUrl = baseUrl.Substring(0, baseUrl.LastIndexOf('/')); // remove the filename.xap part
                    if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
                        baseUrl += '/';
                }
                return baseUrl;
            }
        }

        protected Uri CreateRestRequest(string relativeUrl, string queryString = null)
        {
            string url = string.Format("{0}{1}", BaseUrl, (UserId != null ? (UserId + "/") : ""));
            UriBuilder ub = new UriBuilder(url);         
            string query = "f=json&r=" + Guid.NewGuid().ToString("N");
            if (queryString != null)
                query += "&" + queryString;
            ub.Query = query;
            ub.Path += relativeUrl;
            return ub.Uri;
        }

        private const string ERROR_MSG = "error:";
        protected Exception GetErrorIfAny(string response)
        {
            if (!string.IsNullOrWhiteSpace(response))
            {
                if (response.StartsWith(ERROR_MSG, StringComparison.OrdinalIgnoreCase))
                    return new Exception(response.Substring(ERROR_MSG.Length));
            }
            return null;
        }

        protected AsyncCompletedEventArgs CreateErrorEventArgs(Exception exception)
        {
            return new AsyncCompletedEventArgs() { Error = exception };                
        }
    }
}
