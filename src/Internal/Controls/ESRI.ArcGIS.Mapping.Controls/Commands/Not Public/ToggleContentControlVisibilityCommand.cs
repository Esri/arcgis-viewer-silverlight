/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Extensibility;
using System.Windows.Media.Animation;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ToggleContentControlVisibilityCommand : CommandBase
    {
        private int tabIndex = -1;
        private string _contentControlName;
        private string _tabControlName;
        private string _tabName;

        private ContentControl container;
        private Control tabControlAsControl;
        private Storyboard _currentlyExecutingAnimation;

        public ToggleContentControlVisibilityCommand(string contentControlName, string tabControlName, string tabName)
        {
            _contentControlName = contentControlName;
            _tabControlName = tabControlName;
            _tabName = tabName;
        }

        public override void Execute(object parameter)
        {
            if (container == null)
            {
                container = MapApplication.Current.FindObjectInLayout(_contentControlName) as ContentControl;
                if (container == null)
                    return;
            }

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
                tab = tabControl.Items[tabIndex] as TabItem;
                Storyboard storyBoard;
                bool show = false;
                if (tabControl.SelectedIndex != tabIndex || tabControlAsControl.Visibility == Visibility.Collapsed)
                {
                    storyBoard = VisualStateManagerHelper.FindStoryboard(tabControl, null, ControlNames.VISUAL_STATE_SHOW_CONTENTCONTROL, true);
                    show = true;
                }
                else
                    storyBoard = VisualStateManagerHelper.FindStoryboard(tabControl, null, ControlNames.VISUAL_STATE_HIDE_CONTENTCONTROL, true);

                //
                // Hosted in tab: Case one - storyboard for showing/hiding the tab control has been specified
                //
                if (storyBoard != null)
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
                        _currentlyExecutingAnimation = storyBoard;

                        storyBoard.Begin(); //if animation was defined
                }
                //
                // Hosted in tab: Case two - No animation for showing/hiding the tab control was specified, so simply show/hide via visibility
                //
                else
                {
                    if (show)
                    {
                        tabControl.SelectedIndex = tabIndex;
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
                int index = -1;
                Accordion accordion = tabControlAsControl as Accordion;
                for (int i = 0; i < accordion.Items.Count; i++)
                {
                    tab = accordion.Items[i] as AccordionItem;
                    if (tab.Name == _tabName)
                    {
                        index = i;
                        break;
                    }
                }
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
            //
            // Case three - the content control is hosted elsewhere (Custom Layout)
            //
            else
            {

                Storyboard storyBoard;

                if (container.Visibility == Visibility.Visible)
                    storyBoard = VisualStateManagerHelper.FindStoryboard(container, null, ControlNames.VISUAL_STATE_HIDE_CONTENTCONTROL, true);
                else
                    storyBoard = VisualStateManagerHelper.FindStoryboard(container, null, ControlNames.VISUAL_STATE_SHOW_CONTENTCONTROL, true);

                //
                // Hosted elsewhere: Case one - storyboard for showing/hiding the tab control has been specified
                //
                if (storyBoard != null)
                {
                    if (_currentlyExecutingAnimation != null)
                        _currentlyExecutingAnimation.Stop();
                    _currentlyExecutingAnimation = storyBoard;
                    storyBoard.Begin(); //if animation was defined
                }
                //
                // Hosted elsewhere: Case two - No animation for showing/hiding the tab control was specified, so simply show/hide via visibility
                //
                else
                {
                    container.Visibility = (container.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
                }

                if (container.Visibility == Visibility.Visible)
                    SidePanelHelper.EnsureSidePanelVisibility();
            }
        }
    }
}
