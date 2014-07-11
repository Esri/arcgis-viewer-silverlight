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
using ESRI.ArcGIS.Mapping.Controls;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class ToggleSettingsPageVisibilityCommand : CommandBase
    {
        // this member needs to be static because we want all the command instances to share the state of the application
        static bool isPrevPageTheBuilderScreen;

        public override void Execute(object parameter)
        {
            BuilderApplication ba = BuilderApplication.Instance;
            if (ba != null)
                BuilderApplication.Instance.WindowManager.HideAllWindows();

            if (View.Instance != null)
                View.Instance.WindowManager.HideAllWindows();

            if (ba.SettingsPageVisibility == Visibility.Collapsed)
            {
                // Save whether the user can from builder screen or catalog screen for later
                isPrevPageTheBuilderScreen = BuilderApplication.Instance.BuilderScreenVisibility == Visibility.Visible;

                ba.BuilderScreenVisibility = Visibility.Collapsed;
                ba.CatalogScreenVisibility = Visibility.Collapsed;
                ba.SettingsPageVisibility = Visibility.Visible;
            }
            else
            {
                ba.SettingsPageVisibility = Visibility.Collapsed;
                if (isPrevPageTheBuilderScreen)
                {
                    ba.CatalogScreenVisibility = Visibility.Collapsed;
                    ba.BuilderScreenVisibility = Visibility.Visible;                    
                }
                else
                {
                    if (ViewerApplicationControl.Instance != null)
                        ViewerApplicationControl.Instance.ViewerApplication = null;
                    ba.CatalogScreenVisibility = Visibility.Visible;
                    ba.BuilderScreenVisibility = Visibility.Collapsed;                    
                }
            }
        }
    }
}
