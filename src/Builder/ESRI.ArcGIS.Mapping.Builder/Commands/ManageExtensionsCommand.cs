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
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class ManageExtensionsCommand : CommandBase
    {
        ExtensionsManager extensionManager;
        public override void Execute(object parameter)
        {
            if (extensionManager == null)
            {
                extensionManager = new ExtensionsManager();
                extensionManager.ExtensionsCatalogChanged += extensionManager_ExtensionsCatalogChanged;
            }
            BuilderApplication.Instance.ShowWindow(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ManageExtensions, extensionManager);
        }

        void extensionManager_ExtensionsCatalogChanged(object sender, EventArgs e)
        {
            OnExtensionsCatalogChanged(e);
        }

        protected virtual void OnExtensionsCatalogChanged(EventArgs args)
        {
            if (ExtensionsCatalogChanged != null)
                ExtensionsCatalogChanged(this, args);
        }

        /// <summary>
        /// Event fired when the set of extensions in the builder repository changes. This includes new extensions added
        /// and extensions deleted
        /// </summary>
        public event EventHandler ExtensionsCatalogChanged;  
    }
}
