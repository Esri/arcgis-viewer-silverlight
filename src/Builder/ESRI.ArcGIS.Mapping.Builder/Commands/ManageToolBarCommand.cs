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
using ESRI.ArcGIS.Mapping.Builder.Controls;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class ManageToolbarCommand : CommandBase
    {
        public ManageToolbarCommand() {
            Instance = this;
        }

        internal static ManageToolbarCommand Instance { get; private set; }

        private ManageToolbarControl manageToolbarControl;
        public override void Execute(object parameter)
        {
            manageToolbarControl = manageToolbarControl ?? new ManageToolbarControl();

            // Reset the "loaded" value to the tree is reconstructed each time the dialog is displayed to ensure
            // it remains in sync with any changes made to the toolbar.
            manageToolbarControl.IsLoaded = false;

            BuilderApplication.Instance.ShowWindow(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ManageToolbars, manageToolbarControl);
        }

        internal void OnExtensionsCatalogChanged()
        {
            if (manageToolbarControl != null)
                manageToolbarControl.Refresh();
        }
    }
}
