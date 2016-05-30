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
using ESRI.ArcGIS.Mapping.Builder.Controls;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class PersonalSettings : UserControl
    {
        public PersonalSettings()
        {
            InitializeComponent();
        }

        private void PersonalSettings_Loaded(object sender, RoutedEventArgs e)
        {
            TutorialDialogControl tdc = BuilderApplication.Instance.TutorialDialogControl;
            if (tdc == null)
                return;
            TutorialMode.DataContext = tdc;
            TutorialMode.Content = ESRI.ArcGIS.Mapping.Builder.Resources.Strings.TutorialModeDisable;
        }


    }
}
