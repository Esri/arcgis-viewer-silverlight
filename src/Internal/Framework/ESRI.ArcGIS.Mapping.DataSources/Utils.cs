/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
using ESRI.ArcGIS.Mapping.Core.DataSources;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.DataSources
{
    public class Utils
    {
        public static string GetQueryParameters(string Url)
        {
            //string lastRetrieveTimeStamp = null;
            //try
            //{
            //    lastRetrieveTimeStamp = UrlTimeStampStore.GetTimeStampForUrl(Url);
            //}
            //catch { }
            //if (string.IsNullOrEmpty(lastRetrieveTimeStamp))
            //{
            //    lastRetrieveTimeStamp = DateTime.Now.Ticks.ToString();
            //    try
            //    {
            //        UrlTimeStampStore.SetTimeStampForUrl(Url, lastRetrieveTimeStamp);
            //    }
            //    catch { }
            //}
            //return string.Format("f=json&_lrt={0}", lastRetrieveTimeStamp);
            return "f=json";
        }

        public static Exception CheckJsonForException(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;
            if (json.StartsWith("{\"error", StringComparison.Ordinal))
            {
                string exceptionMsg = "";
                string msg = "\"message\":\"";
                int msgLen = msg.Length;
                int pos = json.IndexOf(msg, StringComparison.Ordinal);
                if (pos > -1)
                {
                    int pos2 = json.IndexOf("\"", pos + msgLen, StringComparison.Ordinal);
                    if (pos2 > -1)
                    {
                        exceptionMsg = json.Substring(pos + msgLen, pos2 - pos - msgLen);
                    }
                }
                if (!String.IsNullOrEmpty(exceptionMsg))
                    return new Exception(exceptionMsg);

                msg = "\"details\":[\"";
                msgLen = msg.Length;
                pos = json.IndexOf(msg, StringComparison.Ordinal);
                if (pos > -1)
                {
                    int pos2 = json.IndexOf("\"", pos + msgLen, StringComparison.Ordinal);
                    if (pos2 > -1)
                    {
                        exceptionMsg = json.Substring(pos + msgLen, pos2 - pos - msgLen);
                    }
                }
                if (!String.IsNullOrEmpty(exceptionMsg))
                    return new Exception(exceptionMsg);

                return new Exception(json);
            }
            return null;
        }

        public static bool IsSDSCatalogResponse(string json)
        {
            if (string.IsNullOrEmpty(json))
                return false;
            return json.StartsWith("{\"version\":", StringComparison.Ordinal);
        }

        public static bool IsMessageLimitExceededException(Exception ex)
        {
            return Utility.IsMessageLimitExceededException(ex);
        }
    }
}
