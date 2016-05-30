/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();

            Loaded += (s, e) =>
                          {
                              // Tooltips in RTL require the main page to be LTR, and LayoutRoot to be RTL
                              FlowDirection = FlowDirection.LeftToRight;

                          };
        }

        private void ExtensionsCatalog_Changed(object sender, EventArgs e)
        {
            if (ExtensionsCatalogChanged != null)
                ExtensionsCatalogChanged(this, e);
        }

        /// <summary>
        /// Event fired when the set of extensions in the builder repository changes. This includes new extensions added
        /// and extensions deleted
        /// </summary>
        public event EventHandler ExtensionsCatalogChanged;
    }
}
