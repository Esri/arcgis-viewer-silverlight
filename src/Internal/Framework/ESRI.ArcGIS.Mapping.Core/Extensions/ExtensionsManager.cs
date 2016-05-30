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
using ESRI.ArcGIS.Client.Application.Controls;

namespace ESRI.ArcGIS.Mapping.Core
{
    public static class ExtensionsManager
    {
        private static List<string> m_extensionUrls = new List<string>();
        public static void LoadAllExtensions(IEnumerable<string> extensionUrls, EventHandler onCompleted, EventHandler<ExceptionEventArgs> onFailed, object userState = null)
        {
            if (extensionUrls != null)
            {
                int packageDownloadCount = extensionUrls.Count();
                if (packageDownloadCount == 0)
                {
                    raiseOnCompletedEvent(onCompleted, onFailed);
                    return;
                }

                foreach (string uri in extensionUrls)
                {
                    if (m_extensionUrls.Contains(uri)) // Check if already downloaded
                    {
                        if (Interlocked.Decrement(ref packageDownloadCount) == 0)
                        {
                            raiseOnCompletedEvent(onCompleted, onFailed);
                        }
                        continue;// skip on to the next extension
                    }

                    m_extensionUrls.Add(uri);
                    Uri validUri = null;
                    if (Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out validUri))
                    {
                        WebClient webClient = new WebClient();
                        webClient.OpenReadCompleted += (s, e) => {
                            if (e.Error != null)
                            {                                
                                if (onFailed != null)
                                    onFailed(null, new ExceptionEventArgs(e.Error, e.UserState));
                                System.Diagnostics.Debug.WriteLine(e.Error.ToString());
                            }

                            if (!e.Cancelled)
                            {
                                try
                                {
                                    IEnumerable<Assembly> assemblies = getAssemblies(e.Result);
                                    if (assemblies != null)
                                    {
                                        foreach (Assembly assembly in assemblies)
                                            AssemblyManager.AddAssembly(assembly);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (onFailed != null)
                                        onFailed(null, new ExceptionEventArgs(ex, e.UserState));
                                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                                }
                            }

                            if (Interlocked.Decrement(ref packageDownloadCount) == 0)
                            {
                                // Last extension
                                raiseOnCompletedEvent(onCompleted, onFailed);
                            }

                        };
                        webClient.OpenReadAsync(validUri, validUri.IsAbsoluteUri ? validUri.AbsoluteUri : uri);

                    }
                    else if (Interlocked.Decrement(ref packageDownloadCount) == 0)
                    {
                        raiseOnCompletedEvent(onCompleted, onFailed);
                    }
                }
            }
            else
            {
                raiseOnCompletedEvent(onCompleted, onFailed);
            }
        }


        private static IEnumerable<Assembly> getAssemblies(Stream stream)
        {
            if (stream == null)
                return null;

            System.Windows.Resources.StreamResourceInfo xapStreamInfo = new System.Windows.Resources.StreamResourceInfo(stream, null);
            Uri uri = new Uri("AppManifest.xaml", UriKind.Relative);
            System.Windows.Resources.StreamResourceInfo resourceStream = Application.GetResourceStream(xapStreamInfo, uri);            
            if (resourceStream == null)
                return null;

            List<Assembly> list = null;
            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(resourceStream.Stream))
            {
                if (!reader.ReadToFollowing("AssemblyPart"))
                {
                    return list;
                }
                list = new List<Assembly>();
                do
                {
                    string attribute = reader.GetAttribute("Source");
                    if (attribute != null)
                    {
                        AssemblyPart item = new AssemblyPart();
                        item.Source = attribute;

                        System.Windows.Resources.StreamResourceInfo assemblyResourceStream = Application.GetResourceStream(xapStreamInfo, new Uri(item.Source, UriKind.Relative));
                        if (assemblyResourceStream == null || assemblyResourceStream.Stream == null)
                            continue;

                        Assembly assembly = item.Load(assemblyResourceStream.Stream);
                        if (assembly != null)
                            list.Add(assembly);
                    }
                }
                while (reader.ReadToNextSibling("AssemblyPart"));
            }
            return list;
        }

        private static void raiseOnCompletedEvent(EventHandler onCompleted, EventHandler<ExceptionEventArgs> onFailed)
        {
            try
            {
                if (onCompleted != null)
                    onCompleted(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                try
                {
                    if (onFailed != null)
                        onFailed(null, new ExceptionEventArgs(ex, null));
                }
                catch { }

                if (onFailed != null)
                    onFailed(null, new ExceptionEventArgs(ex, null));
            }
        }

        public static void LoadAdditionalExtension(string extensionUrl, EventHandler onCompleted, EventHandler<ExceptionEventArgs> onFailed, object userState = null)
        {
            if (string.IsNullOrEmpty(extensionUrl))
                throw new ArgumentNullException("extensionUrl");

            if (m_extensionUrls.Contains(extensionUrl)) // Check if already downloaded
            {
                if (onCompleted != null)
                    onCompleted(null, EventArgs.Empty);
                return;
            }

            Uri validUri;
            if (Uri.TryCreate(extensionUrl, UriKind.RelativeOrAbsolute, out validUri))
            {
                WebClient webClient = new WebClient();
                webClient.OpenReadCompleted +=(s, e) =>
                {
                    if (e.Error != null)
                    {
                        if (onFailed != null)
                            onFailed(null, new ExceptionEventArgs(e.Error, userState));
                        return;
                    }
                    else if (e.Cancelled)
                    {
                        return;
                    }

                    IEnumerable<Assembly> assemblies = getAssemblies(e.Result);
                    if (assemblies != null)
                    {
                        foreach (Assembly assembly in assemblies)
                            AssemblyManager.AddAssembly(assembly);
                    }
                    
                    if (onCompleted != null)
                        onCompleted(null, EventArgs.Empty);
                };
                webClient.OpenReadAsync(validUri, validUri.IsAbsoluteUri ? validUri.AbsoluteUri : extensionUrl);
            }
        }
    }
}
