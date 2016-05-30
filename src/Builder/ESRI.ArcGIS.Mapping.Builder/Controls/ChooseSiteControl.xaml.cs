/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Mapping.Builder.ApplicationBuilder;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Builder.Common;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class ChooseSiteControl : UserControl
    {
        public ChooseSiteControl()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (SiteChosen != null)
                SiteChosen(this, new SiteChosenEventArgs() {
                     Site = SitesList.SelectedItem as Site
                });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            BuilderApplication.Instance.HideWindow(this);
        }

        private void ChooseSiteControl_Loaded(object sender, RoutedEventArgs e)
        {
            SitesList.ItemsSource = BuilderApplication.Instance.Sites;
        }

        public event EventHandler<SiteChosenEventArgs> SiteChosen;
    }

    public class SiteChosenEventArgs : EventArgs
    {
        public Site Site { get; set; }
    }
}
