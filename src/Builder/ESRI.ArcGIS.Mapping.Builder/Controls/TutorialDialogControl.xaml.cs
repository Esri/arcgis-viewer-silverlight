/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Builder.Controls;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class TutorialDialogControl : UserControl
    {
        private const int TAB_COUNT = 3;

        public TabControl sidePanel;
        public Ribbon syncRibbonControl;

        public TutorialDialogControl()
        {
            InitializeComponent();
            IsTutorialDisabled = isTutorialDisabled;
        }

        public static readonly DependencyProperty ViewerApplicationControlProperty = DependencyProperty.Register(
            "ViewerApplicationControl", typeof(ViewerApplicationControl), typeof(TutorialDialogControl), null);

        public ViewerApplicationControl ViewerApplicationControl 
        {
            get { return GetValue(ViewerApplicationControlProperty) as ViewerApplicationControl; }
            set { SetValue(ViewerApplicationControlProperty, value); } 
        }

        private void TutorialDialog_Loaded(object sender, RoutedEventArgs e)
        {
            checkDisable.DataContext = this;
        }

        public void SetTabIndex(int tabIndex)
        {
            if (tabIndex >= 0 && tabIndex < TAB_COUNT)
                TutorialTab.SelectedIndex = (int)tabIndex;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // If the current tab is the last one (or somehow a higher value) then reset to the first one, otherwise
            // simply increment to next tab
            if (TutorialTab.SelectedIndex >= (TAB_COUNT - 1))
                TutorialTab.SelectedIndex = 0;
            else
                TutorialTab.SelectedIndex++;
        }

        private void TutorialTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If the tab control exists then keep this tab the the corresponding builder tab in sync.
            if (TutorialTab != null)
            {
                if (syncRibbonControl != null)
                    syncRibbonControl.SetSelectedTab(TutorialTab.SelectedIndex);
            }
        }

        Button animatingButton;
        Storyboard flashStoryboard = null;

        private void ShowMe_Click(object sender, RoutedEventArgs e)
        {
            // Make sure the ribbon control is available
            if (syncRibbonControl != null)
            {
                // Since the builder tab is always in sync with that of the tutorial dialog, we can get
                // the currently selected item
                TabItem current = syncRibbonControl.TabControl.SelectedItem as TabItem;

                // The content of the currently selected tab item will have a visual tree and we can get
                // this content then find the associated named element to control
                ContentControl cc = current.Content as ContentControl;

                // Use the sending "Show Me" button's name as the value to find in the visual tree to
                // obtain the corresponding control
                Button showMe = sender as Button;
                Button commandControl = cc.FindName(showMe.Name) as Button;
                if (commandControl != null)
                {
                    // Make sure the button is enabled
                    if (commandControl.IsEnabled == true)
                    {
                        // Only allow a single button at a time to be animated
                        if (animatingButton != null)
                            return;
                        animatingButton = commandControl;

                        FrameworkElement templateRoot = System.Windows.Media.VisualTreeHelper.GetChild(animatingButton as DependencyObject, 0) as FrameworkElement;
                        flashStoryboard = templateRoot.Resources["FlashStoryboard"] as Storyboard;
                        if (flashStoryboard == null)
                            throw new Exception(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.FlashAnimationIsNotDefinedOnTargetBuilderToolbarButton);

                        // If the completed event was wired already, remove it and re-establish
                        flashStoryboard.Completed -= flashStoryboard_Completed;
                        flashStoryboard.Completed += flashStoryboard_Completed;

                        // Begin the storyboard animation
                        flashStoryboard.Begin();
                    }
                }
            }
        }

        private void flashStoryboard_Completed(object sender, EventArgs e)
        {
            // Tell the storyboard to officially stop once is has animated itself (otherwise you could
            // suspend, resume, etc. and not start another animation using this same storyboard until
            // truly stopping this one which is what we want to do).
            flashStoryboard.Stop();
            flashStoryboard.Completed -= flashStoryboard_Completed;
            flashStoryboard = null;

            // If the button we are animating is the "Browse" button and the side panel containing the "Browse Control" is created and it
            // is visible and if the current tab selected on this side panel is already the one we want, then exit. Otherwise, invoke
            // the button programmatically as usual.
            if (animatingButton.Name == "BrowseMenuButton" && sidePanel != null && sidePanel.Visibility == System.Windows.Visibility.Visible)
            {
                TabItem ti = (TabItem)sidePanel.SelectedItem;
                if (ti.Name == "BrowseTabItem")
                {
                    animatingButton = null;
                    return;
                }
            }

            // Simulate clicking the target button
            ButtonAutomationPeer peer = new ButtonAutomationPeer((Button)animatingButton);
            IInvokeProvider ip = (IInvokeProvider)peer;
            ip.Invoke();

            // Reset which button is being animated so another may be animated
            animatingButton = null;
        }

        private const string TUTORIAL_KEY = "esri.arcgis.mapping.builder.tutorialdialog.key";
        bool isTutorialDisabled
        {
            get
            {
                // In design inviroments, IsolatedStorageSettings.ApplicationSettings throws an exception
                try
                {
                    bool b;
                    if (IsolatedStorageSettings.ApplicationSettings.TryGetValue(TUTORIAL_KEY, out b))
                        return b;
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            set
            {
                try
                {
                    if (!IsolatedStorageSettings.ApplicationSettings.Contains(TUTORIAL_KEY))
                        IsolatedStorageSettings.ApplicationSettings.Add(TUTORIAL_KEY, value);
                    else
                        IsolatedStorageSettings.ApplicationSettings[TUTORIAL_KEY] = value;
                }
                catch (Exception) { }
            }
        }

        public bool IsTutorialDisabled
        {
            get { return (bool)GetValue(IsTutorialDisabledProperty); }
            set { SetValue(IsTutorialDisabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTutorialDisabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTutorialDisabledProperty =
            DependencyProperty.Register("IsTutorialDisabled", typeof(bool), typeof(TutorialDialogControl), new PropertyMetadata(false, OnDisabledPropertyChange));

        public static void OnDisabledPropertyChange(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            TutorialDialogControl control = o as TutorialDialogControl;
            control.isTutorialDisabled = control.IsTutorialDisabled;
            if (!control.IsTutorialDisabled)
                BuilderApplication.Instance.GettingStartedVisibility = System.Windows.Visibility.Visible;
        }
    }
}
