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
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Core
{
    //WORKAROUND
    //http://support.microsoft.com/kb/982482
    //http://msdn.microsoft.com/en-us/library/dd920295(VS.95).aspx (another alternative to be explored)
    public static class WebClientFactory
    {
        private static bool isIE = false;

        public static void Initialize()
        {
            isIE = (System.Windows.Browser.HtmlPage.BrowserInformation.Name == "Microsoft Internet Explorer");
        }

        public static WebClient CreateWebClient()
        {
            WebClient webClient = new WebClient();
            if (isIE)
                webClient.AllowReadStreamBuffering = false;
            return webClient;
        }

        public static void RedownloadStringAsync(WebClient webClient, Uri uri, object userToken)
        {
            if (RedownloadAttempted.Contains(webClient))
                return;
            RedownloadAttempted.Add(webClient);
            webClient.DownloadStringAsync(uri, userToken);
        }

        public static List<WebClient> RedownloadAttempted = new List<WebClient>();
        
    }
}
