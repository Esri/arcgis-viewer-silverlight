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
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class SchemeBasedConfigurationStoreProvider : FileConfigurationStoreProvider
    {
        protected override void OnGetConfigurationStoreCompleted(GetConfigurationStoreCompletedEventArgs args)
        {
            ConfigurationStore store = args.ConfigurationStore;
            if (store != null)
            {
                bool isHttps = "https".Equals(Application.Current.Host.Source.Scheme);
                if (store.BaseMaps != null)
                {
                    foreach (BaseMapInfo baseMapInfo in store.BaseMaps)
                    {
                        if (isHttps && baseMapInfo.BaseMapType == BaseMapType.OpenStreetMap)
                        {
                            // Open street map doesn't have a secure end point, so don't show it in https mode
                            store.BaseMaps.Remove(baseMapInfo);
                            break;
                        }

                        if (string.IsNullOrWhiteSpace(baseMapInfo.Url))
                            continue;

                        if(isHttps)
                            baseMapInfo.Url = baseMapInfo.Url.Replace("http://", "https://");
                        else
                            baseMapInfo.Url = baseMapInfo.Url.Replace("https://", "http://");
                    }
                }

                if (store.GeometryServices != null)
                {
                    foreach (GeometryServiceInfo serviceInfo in store.GeometryServices)
                    {
                        if (isHttps)
                            serviceInfo.Url = serviceInfo.Url.Replace("http://", "https://");
                        else
                            serviceInfo.Url = serviceInfo.Url.Replace("https://", "http://");
                    }
                }
            }

            base.OnGetConfigurationStoreCompleted(args);
        }
    }
}
