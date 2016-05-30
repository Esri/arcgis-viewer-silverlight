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
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Builder.Resources;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class ApplicationPropertiesDialog : UserControl
    {
        public ApplicationPropertiesDialog()
        {
            InitializeComponent();            
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            BuilderApplication.Instance.HideWindow(this);
        }

        private void AddHelpLink_Click(object sender, RoutedEventArgs e)
        {
            if (ViewerApplicationControl.Instance == null
                || ViewerApplicationControl.Instance.ViewerApplication == null)
                return;

            ViewerApplication va = ViewerApplicationControl.Instance.ViewerApplication;
            va.HelpLinks = va.HelpLinks ?? new System.Collections.ObjectModel.ObservableCollection<HelpLink>();
            va.HelpLinks.Add(new HelpLink() { DisplayText = Strings.NewLink, Url = "http://www.example.com" });
        }

        private void RemoveHelpLink_Click(object sender, RoutedEventArgs e)
        {
            HelpLink helpLink = HelpLinksGrid.SelectedItem as HelpLink;
            if (helpLink == null)
                return;

            if (ViewerApplicationControl.Instance == null
                || ViewerApplicationControl.Instance.ViewerApplication == null
                || ViewerApplicationControl.Instance.ViewerApplication.HelpLinks == null)
                return;

            ViewerApplicationControl.Instance.ViewerApplication.HelpLinks.Remove(helpLink);
        }

        private void ApplicationPropertiesDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewerApplicationControl.Instance != null)
                DataContext = ViewerApplicationControl.Instance.ViewerApplication;
            TitleTextBox.Focus();
        }
    }
}
