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
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client.Application.Controls;
using EsriMapContents = ESRI.ArcGIS.Mapping.Controls.MapContents;

namespace ESRI.ArcGIS.Mapping.Controls
{
    internal static class SidePanelHelper
    {
        static EsriMapContents.MapContents mapContentsControl;

        private static int mapContentsTabIndex = -1;

        public static void Reset()
        {
            mapContentsControl = null;
            mapContentsTabIndex = -1;
        }

        public static EsriMapContents.MapContents GetMapContentsControl()
        {
            if (mapContentsControl == null)
                mapContentsControl = MapApplication.Current.FindObjectInLayout(ControlNames.MAPCONTENTS_CONTROL_NAME) as EsriMapContents.MapContents;
            
            return mapContentsControl;
        }

        public static void ShowMapContents()
        {
            mapContentsControl = mapContentsControl ?? GetMapContentsControl();
            if (mapContentsControl != null)
                mapContentsControl.Visibility = Visibility.Visible;
            Control sidePanelContainer = MapApplication.Current.FindObjectInLayout(ControlNames.SIDEPANELCONTAINER) as Control;

            if (sidePanelContainer is TabControl)
            {
                TabControl sidePanel = sidePanelContainer as TabControl;
                if (mapContentsTabIndex == -1)
                {
                    for (int i = 0; i < sidePanel.Items.Count; i++)
                    {
                        TabItem tab = sidePanel.Items[i] as TabItem;
                        if (tab.Name == ControlNames.MAPCONTENTSTABITEM)
                        {
                            mapContentsTabIndex = i;
                            break;
                        }
                    }
                }

                sidePanel.SelectedIndex = mapContentsTabIndex;
            }
            else if (sidePanelContainer is Accordion)
            {
                Accordion accordion = sidePanelContainer as Accordion;
                if (mapContentsTabIndex == -1)
                {
                    for (int i = 0; i < accordion.Items.Count; i++)
                    {
                        AccordionItem accItem = accordion.Items[i] as AccordionItem;
                        if (accItem.Name == ControlNames.MAPCONTENTSTABITEM)
                        {
                            mapContentsTabIndex = i;
                            break;
                        }
                    }
                }

                accordion.SelectedIndex = mapContentsTabIndex;

            }
        }

        public static void EnsureSidePanelVisibility()
        {
            ContentControl sidePanelContainer = MapApplication.Current.FindObjectInLayout(ControlNames.SIDEPANELCONTAINER) as ContentControl;
            if (sidePanelContainer != null)
                sidePanelContainer.Visibility = Visibility.Visible;
        }
    }
}
