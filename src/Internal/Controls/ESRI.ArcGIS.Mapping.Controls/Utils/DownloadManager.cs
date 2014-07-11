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
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Reflection;
using System.IO;
using ESRI.ArcGIS.Client;
using System.Windows.Interactivity;
using System.Windows.Markup;
using System.Threading;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public static class DownloadManager
    {
        public class DownloadCompleteEventArgs : EventArgs
        {
            public Dictionary<string, string> FileContents = new Dictionary<string, string>();
            public object UserState { get; set; }
        }

        public static void DownloadUrls(IEnumerable<string> downloadUrls, EventHandler<DownloadCompleteEventArgs> onCompleted, EventHandler<ExceptionEventArgs> onFailed, object userState)
        {
            Dictionary<string, string> downloadedFiles = new Dictionary<string, string>();
            if (downloadUrls != null)
            {
                
                int downloadCount = downloadUrls.Count();
                if (downloadCount == 0)
                {
                    raiseOnCompletedEvent(downloadedFiles, onCompleted, onFailed, userState);
                    return;
                }

                foreach (string uri in downloadUrls)
                {
                    if (downloadedFiles.ContainsKey(uri)) // Check if already downloaded
                    {
                        if (Interlocked.Decrement(ref downloadCount) == 0)
                        {
                            raiseOnCompletedEvent(downloadedFiles, onCompleted, onFailed, userState);
                        }
                        continue;// skip on to the next extension
                    }

                    Uri validUri = null;
                    if (Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out validUri))
                    {
                        WebClient webClient = new WebClient();
                        webClient.DownloadStringCompleted += (s, e) => {
                            if (e.Error != null)
                            {                                
                                if (onFailed != null)
                                    onFailed(null, new ExceptionEventArgs(e.Error, e.UserState));
                                System.Diagnostics.Debug.WriteLine(e.Error.ToString());
                            }

                            object [] state = e.UserState as object[];
                            if (!e.Cancelled)
                            {
                                try
                                {
                                    downloadedFiles.Add(state[0] as string, e.Result);
                                }
                                catch (Exception ex)
                                {
                                    if (onFailed != null)
                                        onFailed(null, new ExceptionEventArgs(ex, state[1]));
                                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                                }
                            }

                            if (Interlocked.Decrement(ref downloadCount) == 0)
                            {
                                // Last extension
                                raiseOnCompletedEvent(downloadedFiles, onCompleted, onFailed, state[1]);
                            }

                        };
                        webClient.DownloadStringAsync(validUri, new object[] { uri, userState });

                    }
                    else if (Interlocked.Decrement(ref downloadCount) == 0)
                    {
                        raiseOnCompletedEvent(downloadedFiles, onCompleted, onFailed, userState);
                    }
                }
            }
            else
            {
                raiseOnCompletedEvent(downloadedFiles, onCompleted, onFailed, userState);
            }
        }

        private static void raiseOnCompletedEvent(Dictionary<string, string> downloadedFiles, EventHandler<DownloadCompleteEventArgs> onCompleted, EventHandler<ExceptionEventArgs> onFailed, object userState)
        {
            try
            {
                if (onCompleted != null)
                    onCompleted(null, new DownloadCompleteEventArgs() { FileContents = downloadedFiles, UserState = userState });
            }
            catch (Exception ex)
            {
                try
                {
                    if (onFailed != null)
                        onFailed(null, new ExceptionEventArgs(ex, userState));
                }
                catch { }

                if (onFailed != null)
                    onFailed(null, new ExceptionEventArgs(ex, userState));
            }
        }

    }
}
