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

namespace ESRI.ArcGIS.Mapping.Core
{
    public class ExtensionsDataManager
    {
        public ExtensionsConfigData ExtensionsConfigData { get; set; }

        public string GetExtensionDataForExtension(string extensionId)
        {
            ExtensionData ex = getExtensionDataForExtensions(extensionId);
            if (ex != null)
                return ex.ConfigData;
            return null;
        }

        public void SetExtensionDataForExtension(string extensionId, string data)
        {
            ExtensionData ex = getExtensionDataForExtensions(extensionId);
            if (ex == null)
            {
                ex = new ExtensionData();
                if (ExtensionsConfigData == null)
                    ExtensionsConfigData = new ExtensionsConfigData() { ExtensionsData = new System.Collections.Generic.Dictionary<string, ExtensionData>() };
                ExtensionsConfigData.ExtensionsData.Add(extensionId, ex);
            }
            ex.ConfigData = data;
        }

        public void DeleteExtensionData(string extensionId)
        {
            if (ExtensionsConfigData == null || ExtensionsConfigData.ExtensionsData == null)
                return;

            ExtensionsConfigData.ExtensionsData.Remove(extensionId);
        }

        private ExtensionData getExtensionDataForExtensions(string extensionId)
        {
            if (string.IsNullOrEmpty(extensionId))
                return null;

            if (ExtensionsConfigData == null || ExtensionsConfigData.ExtensionsData == null)
                return null;

            ExtensionData extData;
            if (ExtensionsConfigData.ExtensionsData.TryGetValue(extensionId, out extData))
            {
                return extData;
            }
            return null;
        }
    }
}
