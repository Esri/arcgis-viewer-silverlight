/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;

namespace SearchTool
{
    /// <summary>
    /// Provides static utility methods for interacting with the web.
    /// </summary>
    public class WebUtil
    {
        /// <summary>
        /// Helper method to read from a web service asynchronously.
        /// </summary>
        public static void OpenReadAsync(string url, object userState, EventHandler<OpenReadEventArgs> callback)
        {
            try
            {
                OpenReadAsync(new Uri(url), userState, callback);
            }
            catch (Exception ex)
            {
                callback(null, new OpenReadEventArgs(null) { Error = ex });
            }
        }

        /// <summary>
        /// Helper method to read from a web service asynchronously.
        /// </summary>
        public static void OpenReadAsync(Uri uri, object userState, EventHandler<OpenReadEventArgs> callback, string proxyUrl = null)
        {
            ArcGISWebClient wc = new ArcGISWebClient() { ProxyUrl = proxyUrl };

            wc.OpenReadCompleted += (sender, e) =>
            {
                // TODO: Revisit handling of request failure due to missing clientaccesspolicy
                
                // if the request failed because of a security exception - missing clientaccesspolicy file -
                // then try to go thru the proxy server
                //
                //if (e.Error is System.Security.SecurityException)
                //{
                //    string proxyUrl = MapApplication.Current.Urls.GetProxyUrl();
                //    if (string.IsNullOrEmpty(proxyUrl))
                //    {
                //        callback(sender, new OpenReadEventArgs(e));
                //        return;
                //    }

                //    wc = new WebClient();
                //    wc.OpenReadCompleted += (sender2, e2) =>
                //    {
                //        callback(sender, new OpenReadEventArgs(e2) { UsedProxy = true });
                //    };

                //    uri = new Uri(proxyUrl + "?" + uri.ToString());
                //    wc.OpenReadAsync(uri, userState);
                //}
                //else

                callback(sender, new OpenReadEventArgs(e) { UsedProxy = !string.IsNullOrEmpty(proxyUrl) });
            };

            wc.OpenReadAsync(uri, null, ArcGISWebClient.HttpMethods.Auto, userState);
        }

        /// <summary>
        /// Reads an object from the specified stream. Returns null if the object could
        /// not be read. The format of the object is json.
        /// </summary>
        public static T ReadObject<T>(System.IO.Stream stream) where T : class
        {
            try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                return serializer.ReadObject(stream) as T;
            }
            catch
            {
                return null;
            }
        }
    }

    public class OpenReadEventArgs : EventArgs
    {
        public bool Cancelled { get; set; }
        public Exception Error { get; set; }
        public object UserState { get; set; }
        public Stream Result { get; set; }
        public bool UsedProxy { get; set; }

        public OpenReadEventArgs(ArcGISWebClient.OpenReadCompletedEventArgs e)
        {
            if (e != null)
            {
                Cancelled = e.Cancelled;
                Error = e.Error;
                UserState = e.UserState;
                if (e.Error == null)
                    Result = e.Result;
            }
        }
    }
}
