/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Extensibility;
using System.Windows.Media.Animation;
using System;
using System.Windows.Media;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ToggleControlVisibilityCommand : CommandBase
    {
        private int tabIndex = -1;
        private string _tabControlName;
        private string _tabName;

        private Control tabControlAsControl;

        public ToggleControlVisibilityCommand(string tabControlName, string tabName)
        {
            _tabControlName = tabControlName;
            _tabName = tabName;
        }

        public override void Execute(object parameter)
        {
            if (tabControlAsControl == null)
                tabControlAsControl = MapApplication.Current.FindObjectInLayout(_tabControlName) as Control;

            //
            // Case one - the content control is hosted within a Tab Control (OOTB layouts)
            //
            if (tabControlAsControl is TabControl)
            {
                TabItem tab = null;
                TabControl tabControl = tabControlAsControl as TabControl;
                if (tabIndex == -1)
                {
                    for (int i = 0; i < tabControl.Items.Count; i++)
                    {
                        tab = tabControl.Items[i] as TabItem;
                        if (tab.Name == _tabName)
                        {
                            tabIndex = i;
                            break;
                        }
                    }
                }

                if (tabIndex == -1)
                    throw new Exception(string.Format(
                        ESRI.ArcGIS.Mapping.Controls.Resources.Strings.NotFoundInLayout, _tabName));
                tab = tabControl.Items[tabIndex] as TabItem;
                Storyboard visualStateStoryboard = null;
                Storyboard resourceStoryboard = null;

                bool show = false;
                bool useGoToState = false;
                if (tabControl.SelectedIndex != tabIndex || tabControlAsControl.Visibility == Visibility.Collapsed)
                {
                    if (VisualStateManagerHelper.StateWellDefined(tabControl, ControlNames.VISUAL_STATE_SHOW_CONTENTCONTROL, true))
                        useGoToState = true;
                    else
                        visualStateStoryboard = VisualStateManagerHelper.FindStoryboard(tabControl, null, ControlNames.VISUAL_STATE_SHOW_CONTENTCONTROL, true);
                    resourceStoryboard = findStoryboardIncludedInResources(ControlNames.VISUAL_STATE_SHOW_CONTENTCONTROL);
                    show = true;
                }
                else
                {
                    if (VisualStateManagerHelper.StateWellDefined(tabControl, ControlNames.VISUAL_STATE_HIDE_CONTENTCONTROL, true))
                        useGoToState = true;
                    else
                        visualStateStoryboard = VisualStateManagerHelper.FindStoryboard(tabControl, null, ControlNames.VISUAL_STATE_HIDE_CONTENTCONTROL, true);
                    resourceStoryboard = findStoryboardIncludedInResources(ControlNames.VISUAL_STATE_HIDE_CONTENTCONTROL);
                }

                //
                // Hosted in tab: Case one - storyboard for showing/hiding the tab control has been specified
                //
                if (visualStateStoryboard != null || resourceStoryboard != null || useGoToState)
                {
                    if (show)
                    {
                        tabControl.SelectedIndex = tabIndex;
                        tab.Visibility = Visibility.Visible;
                    }

                        //for (int i = storyBoard.Children.Count - 1; i >= 0; i--)//set the target properties if missing
                        //{
                        //    var timeLine = storyBoard.Children[i];
                        //    var timeLineTargetXName = Storyboard.GetTargetName(timeLine);
                        //    if (string.IsNullOrEmpty(timeLineTargetXName))
                        //    {
                        //        Storyboard.SetTarget(timeLine, tabControl);
                        //    }
                        //}
                        //if (_currentlyExecutingAnimation != null)
                        //    _currentlyExecutingAnimation.Stop();
                        //_currentlyExecutingAnimation = visualStateStoryboard;

                    if (useGoToState)
                    {
                        string showHideString = show ? ControlNames.VISUAL_STATE_SHOW_CONTENTCONTROL : 
                            ControlNames.VISUAL_STATE_HIDE_CONTENTCONTROL;
                        string state = string.Format("{0}_{1}", tabControl.Name, showHideString);
                        VisualStateHelper.GoToState(tabControl, true, state);
                    }

                    if (visualStateStoryboard != null)
                        visualStateStoryboard.Begin(); //if animation was defined

                    if (resourceStoryboard != null)
                        resourceStoryboard.Begin();
                }
                //
                // Hosted in tab: Case two - No animation for showing/hiding the tab control was specified, so simply show/hide via visibility
                //
                else
                {
                    if (show)
                    {
                        tabControl.SelectedIndex = tabIndex;
                        tab.Visibility = Visibility.Visible;
                        tabControlAsControl.Visibility = Visibility.Visible;
                    }
                    else
                        tabControlAsControl.Visibility = Visibility.Collapsed;
                }
            }
            //
            // Case two - the content control is hosted an accordion
            //
            else if (tabControlAsControl is Accordion)
            {
                AccordionItem tab = null;
                Accordion accordion = tabControlAsControl as Accordion;
                int index = -1;
                for (int i = 0; i < accordion.Items.Count; i++)
                {
                    tab = accordion.Items[i] as AccordionItem;
                    if (tab.Name == _tabName)
                    {
                        index = i;
                        break;
                    }
                }
                if (index == -1)
                    throw new Exception(string.Format(
                        ESRI.ArcGIS.Mapping.Controls.Resources.Strings.NotFoundInLayout, _tabName));
                bool show = (accordion.SelectedIndex != index || accordion.Visibility == Visibility.Collapsed);
                accordion.SelectedIndex = index;
                if (show)
                {
                    AccordionItem accItem = accordion.Items[index] as AccordionItem;
                    accItem.Visibility = Visibility.Visible;
                    tabControlAsControl.Visibility = Visibility.Visible;
                }
                else
                    tabControlAsControl.Visibility = Visibility.Collapsed;
                

            }

        }

        private Storyboard findStoryboardIncludedInResources(string showHideString)
        {
            Storyboard resourceStoryboard = null;
            string resourceStoryboardKey = string.Format("{0}{1}Storyboard", showHideString, _tabControlName);

            if (Application.Current.Resources.Contains(resourceStoryboardKey))
                resourceStoryboard = Application.Current.Resources[resourceStoryboardKey] as Storyboard;
            else // try to find storyboard from application root
                resourceStoryboard = findResource(Application.Current.RootVisual as FrameworkElement, resourceStoryboardKey, 10) as Storyboard;

            return resourceStoryboard;
        }

        private object findResource(FrameworkElement element, string resourceKey, int recursionLevels = 0)
        {
            if (element == null)
                return null;

            object resource = null;
            if (element.Resources.Contains(resourceKey))
            {
                return element.Resources[resourceKey];
            }
            else if (recursionLevels > 0)
            {
                recursionLevels--;
                int childCount = VisualTreeHelper.GetChildrenCount(element);
                for (int i = 0; i < childCount; i++)
                {
                    resource = findResource(VisualTreeHelper.GetChild(element, i) as FrameworkElement, 
                        resourceKey, recursionLevels);
                    if (resource != null)
                        return resource;
                }
            }
            return resource;
        }
    }
}
