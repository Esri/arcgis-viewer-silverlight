/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
	[DisplayName("ToggleTableDisplayName")]
	[Category("CategoryLayer")]
	[Description("ToggleTableDescription")]
    public class ToggleTableCommand : CommandBase
    {        
        public override void Execute(object parameter)
        {
            System.Windows.Controls.ContentControl container = MapApplication.Current.FindObjectInLayout(ControlNames.FEATUREDATAGRIDTABLECONTAINER) as ContentControl;
            if (container == null)
                return;
             
            Storyboard showStoryBoard;
            Storyboard hideStoryBoard;

            hideStoryBoard = VisualStateManagerHelper.FindStoryboard(container, null, ControlNames.VISUAL_STATE_HIDE_CONTENTCONTROL, true);
            showStoryBoard = VisualStateManagerHelper.FindStoryboard(container, null, ControlNames.VISUAL_STATE_SHOW_CONTENTCONTROL, true);

            if (showStoryBoard != null && hideStoryBoard != null) //if animation is defined
            {
                if (container.Visibility == Visibility.Collapsed && 
                hideStoryBoard.GetCurrentState() != ClockState.Active)
                {
                    showStoryBoard.Begin(); 
                }
                else if (hideStoryBoard.GetCurrentState() == ClockState.Active)
                {
                    // Account for the possibility that the hiding storyboard could be running
                    hideStoryBoard.Stop();
                    showStoryBoard.Begin();
                }
                else if (container.Visibility == Visibility.Visible &&
                showStoryBoard.GetCurrentState() != ClockState.Active)
                {
                    hideStoryBoard.Begin(); 
                }
                else if (showStoryBoard.GetCurrentState() == ClockState.Active)
                {
                    // Account for the possibility that the showing storyboard could be running
                    showStoryBoard.Stop();
                    hideStoryBoard.Begin();
                }
            }
            else
            {
                //otherwise simply show/hide using visibility
                container.Visibility = (container.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            }
        }              

        /// <summary>
        /// Shows/Hides the Attribute Table
        /// </summary>
        /// <param name="visibility">Visibility to be set on the Attribute Table</param>
        public static void SetTableVisibility(Visibility visibility)
        {
            ContentControl container = MapApplication.Current.FindObjectInLayout(ControlNames.FEATUREDATAGRIDTABLECONTAINER) as ContentControl;
            if (container == null)
                return;

            if (container.Visibility != visibility)
            {
                Storyboard storyBoard = VisualStateManagerHelper.FindStoryboard(container, null, (visibility == Visibility.Visible)
                    ? ControlNames.VISUAL_STATE_SHOW_CONTENTCONTROL
                    : ControlNames.VISUAL_STATE_HIDE_CONTENTCONTROL, true);

                if (storyBoard != null)
                {
                    //if animation was defined
                    storyBoard.Begin();
                }
                else
                {
                    //otherwise simply show/hide using visibility
                    container.Visibility = (container.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
                }
            }
        }
    }
}
