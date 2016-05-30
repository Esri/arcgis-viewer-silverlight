/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class Ribbon : Control
    {
        Button BackStageButton;

        #region RibbonTab
        /// <summary>
        /// 
        /// </summary>
        public TabControl TabControl
        {
            get { return GetValue(RibbonTabProperty) as TabControl; }
            set { SetValue(RibbonTabProperty, value); }
        }

        /// <summary>
        /// Identifies the RibbonTab dependency property.
        /// </summary>
        public static readonly DependencyProperty RibbonTabProperty =
            DependencyProperty.Register(
                "TabControl",
                typeof(TabControl),
                typeof(Ribbon),
                new PropertyMetadata(null));
        #endregion

        #region BackStageButtonToolTip
        /// <summary>
        /// 
        /// </summary>
        public string BackStageButtonToolTip
        {
            get { return GetValue(BackstageButtonToolTipProperty) as string; }
            set { SetValue(BackstageButtonToolTipProperty, value); }
        }

        /// <summary>
        /// Identifies the BackstageButtonToolTip dependency property.
        /// </summary>
        public static readonly DependencyProperty BackstageButtonToolTipProperty =
            DependencyProperty.Register(
                "BackStageButtonToolTip",
                typeof(string),
                typeof(Ribbon),
                new PropertyMetadata(null));
        #endregion

        #region BackStageButtonCommand
        /// <summary>
        /// 
        /// </summary>
        public ICommand BackStageButtonCommand
        {
            get { return GetValue(BackStageButtonCommandProperty) as ICommand; }
            set { SetValue(BackStageButtonCommandProperty, value); }
        }

        /// <summary>
        /// Identifies the BackStageButtonCommand dependency property.
        /// </summary>
        public static readonly DependencyProperty BackStageButtonCommandProperty =
            DependencyProperty.Register(
                "BackStageButtonCommand",
                typeof(ICommand),
                typeof(Ribbon),
                new PropertyMetadata(null));
        #endregion

        #region BackStageButtonVisibility
        /// <summary>
        /// 
        /// </summary>
        public Visibility BackStageButtonVisibility
        {
            get { return (Visibility)GetValue(BackStageCommandVisibilityProperty); }
            set { SetValue(BackStageCommandVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the BackStageCommandVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty BackStageCommandVisibilityProperty =
            DependencyProperty.Register(
                "BackStageButtonVisibility",
                typeof(Visibility),
                typeof(Ribbon),
                new PropertyMetadata(Visibility.Visible));
        #endregion

        #region BackStageButtonContents
        /// <summary>
        /// 
        /// </summary>
        public FrameworkElement BackStageButtonContents
        {
            get { return GetValue(BackStageButtonContentsProperty) as FrameworkElement; }
            set { SetValue(BackStageButtonContentsProperty, value); }
        }

        /// <summary>
        /// Identifies the BackStageButtonContents dependency property.
        /// </summary>
        public static readonly DependencyProperty BackStageButtonContentsProperty =
            DependencyProperty.Register(
                "BackStageButtonContents",
                typeof(FrameworkElement),
                typeof(Ribbon),
                new PropertyMetadata(null));
        #endregion

        #region RibbonVisibility
        /// <summary>
        /// Specifies whether the ribbon is expanded or collapsed
        /// </summary>
        public Visibility RibbonVisibility
        {
            get { return (Visibility)GetValue(RibbonVisibilityProperty); }
            set { SetValue(RibbonVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the RibbonVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty RibbonVisibilityProperty =
            DependencyProperty.Register(
                "RibbonVisibility",
                typeof(Visibility),
                typeof(Ribbon),
                new PropertyMetadata(Visibility.Visible));

        #endregion

        public Ribbon()
        {
            DefaultStyleKey = typeof(Ribbon);
            RibbonTabs = new ObservableCollection<RibbonTabControl>();
        }

        public override void OnApplyTemplate()
        {
            if(TabControl != null)
                TabControl.SelectionChanged -= TabControl_SelectionChanged;

            base.OnApplyTemplate();

            TabControl = GetTemplateChild("TabControl") as TabControl;
            // Initialize tab control visibility
            if (TabControl != null)
            {
                TabControl.SelectionChanged += TabControl_SelectionChanged;
            }

            BackStageButton = GetTemplateChild("BackStageButton") as Button;           
        }

        void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        public void SetSelectedTab(int index)
        {
            if (TabControl != null)
                TabControl.SelectedIndex = index;
        }

        #region View
        /// <summary>
        /// 
        /// </summary>
        public View View
        {
            get { return GetValue(ViewProperty) as View; }
            set { SetValue(ViewProperty, value); }
        }

        /// <summary>
        /// Identifies the View dependency property.
        /// </summary>
        public static readonly DependencyProperty ViewProperty =
            DependencyProperty.Register(
                "View",
                typeof(View),
                typeof(Ribbon),
                new PropertyMetadata(null, OnViewPropertyChanged));

        /// <summary>
        /// ViewProperty property changed handler.
        /// </summary>
        /// <param name="d">Ribbon that changed its View.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnViewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Ribbon source = d as Ribbon;
            source.OnViewChanged();
        }
        #endregion

        private void OnViewChanged()
        {
            if (View == null)
                return;

            if (View.ConfigurationStore != null)
                ConfigurationStore = View.ConfigurationStore;

            View.SelectedLayerChanged -= View_LayerSelectionChanged;
            View.SelectedLayerChanged += View_LayerSelectionChanged;

            View.SelectionChanged -= View_SelectionChanged;
            View.SelectionChanged += View_SelectionChanged;
        }

        void View_SelectionChanged(object sender, EventArgs e)
        {
            refreshButtonCommands();
            //if (!View.HasActiveSelection)
            //{
            //    RibbonTab.SelectedIndex = 1; // reset to home tab
            //}
        }

        void View_LayerSelectionChanged(object sender, EventArgs e)
        {
            refreshButtonCommands();
        }

        private void OnLayerChanged()
        {
            refreshButtonCommands();
        }

        private void refreshButtonCommands()
        {
            if (TabControl == null)
                return;
            List<ButtonBase> customButtons = ControlTreeHelper.FindChildrenOfType<ButtonBase>(TabControl as DependencyObject, 12);
            if (customButtons != null)
            {
                foreach (ButtonBase btn in customButtons)
                {
                    ICommand cmd = btn.Command;
                    if (cmd != null)
                    {
                        cmd.CanExecuteChanged -= cmd_CanExecuteChanged;
                        cmd.CanExecuteChanged += cmd_CanExecuteChanged;                        
                        btn.Command = null;
                        btn.Command = cmd;
                        ToggleButton toggleButton = btn as ToggleButton;
                        if (toggleButton != null)
                        {
                            IToggleCommand toggleButtonCmd = cmd as IToggleCommand;
                            if (toggleButtonCmd != null)
                                toggleButton.IsChecked = toggleButtonCmd.IsChecked();
                        }
                    }
                }
            }
        }

        void cmd_CanExecuteChanged(object sender, EventArgs e)
        {
            refreshButtonCommands();
        }

        public void RefreshRibbon()
        {
            refreshButtonCommands();
        }

        #region TopLeftElement
        /// <summary>
        /// 
        /// </summary>
        public FrameworkElement TopLeftElement
        {
            get { return GetValue(TopLeftElementProperty) as FrameworkElement; }
            set { SetValue(TopLeftElementProperty, value); }
        }

        /// <summary>
        /// Identifies the TopLeftElement dependency property.
        /// </summary>
        public static readonly DependencyProperty TopLeftElementProperty =
            DependencyProperty.Register(
                "TopLeftElement",
                typeof(FrameworkElement),
                typeof(Ribbon),
                new PropertyMetadata(null));
        #endregion

        #region TopRightElement
        /// <summary>
        /// 
        /// </summary>
        public FrameworkElement TopRightElement
        {
            get { return GetValue(TopRightElementProperty) as FrameworkElement; }
            set { SetValue(TopRightElementProperty, value); }
        }

        /// <summary>
        /// Identifies the HelpSection dependency property.
        /// </summary>
        public static readonly DependencyProperty TopRightElementProperty =
            DependencyProperty.Register(
                "TopRightElement",
                typeof(FrameworkElement),
                typeof(Ribbon),
                new PropertyMetadata(null));
        #endregion

        #region InitialSelectedIndex
        /// <summary>
        /// 
        /// </summary>
        public int InitialSelectedIndex
        {
            get { return (int)GetValue(InitialSelectedIndexProperty); }
            set { SetValue(InitialSelectedIndexProperty, value); }
        }

        /// <summary>
        /// Identifies the InitialSelectedIndex dependency property.
        /// </summary>
        public static readonly DependencyProperty InitialSelectedIndexProperty =
            DependencyProperty.Register(
                "InitialSelectedIndex",
                typeof(int),
                typeof(Ribbon),
                new PropertyMetadata(0));
        #endregion

        #region RibbonTabs
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<RibbonTabControl> RibbonTabs
        {
            get { return GetValue(RibbonTabsProperty) as ObservableCollection<RibbonTabControl>; }
            set { SetValue(RibbonTabsProperty, value); }
        }

        /// <summary>
        /// Identifies the RibbonTabs dependency property.
        /// </summary>
        public static readonly DependencyProperty RibbonTabsProperty =
            DependencyProperty.Register(
                "RibbonTabs",
                typeof(ObservableCollection<RibbonTabControl>),
                typeof(Ribbon),
                new PropertyMetadata(null));
        #endregion

        #region TitleText
        /// <summary>
        /// 
        /// </summary>
        public string TitleText
        {
            get { return GetValue(TitleTextProperty) as string; }
            set { SetValue(TitleTextProperty, value); }
        }

        /// <summary>
        /// Identifies the TitleText dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleTextProperty =
            DependencyProperty.Register(
                "TitleText",
                typeof(string),
                typeof(Ribbon),
                new PropertyMetadata(null));
        #endregion

        #region ConfigurationStore
        /// <summary>
        /// 
        /// </summary>
        public ConfigurationStore ConfigurationStore
        {
            get { return GetValue(ConfigurationStoreProperty) as ConfigurationStore; }
            set { SetValue(ConfigurationStoreProperty, value); }
        }

        /// <summary>
        /// Identifies the ConfigurationStore dependency property.
        /// </summary>
        public static readonly DependencyProperty ConfigurationStoreProperty =
            DependencyProperty.Register(
                "ConfigurationStore",
                typeof(ConfigurationStore),
                typeof(Ribbon),
                new PropertyMetadata(null));
        #endregion
    }
}
