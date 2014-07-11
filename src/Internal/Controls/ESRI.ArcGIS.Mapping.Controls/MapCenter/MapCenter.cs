/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client.WebMap;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = "MapCenter", Type = typeof(MapCenter))]
    [TemplatePart(Name = PART_MapCenterContainer, Type = typeof(Panel))]
    [TemplatePart(Name = PART_Container, Type = typeof(Panel))]
    [TemplatePart(Name = "LoadingMapIndicator", Type = typeof(BusyIndicator))]
    public class MapCenter : Control
    {
        private const string PART_MapCenterContainer = "MapCenterContainer";
        private const string PART_Container = "Container";

        public MapCenter()
        {
            DefaultStyleKey = typeof(MapCenter);
            Close = new DelegateCommand(close);
        }

        public override void OnApplyTemplate()
        {
            MapCenterContainer = GetTemplateChild(PART_MapCenterContainer) as Panel;
            Container = GetTemplateChild(PART_Container) as Panel;
            LoadingMapIndicator = GetTemplateChild("LoadingMapIndicator") as BusyIndicator;
            LoadingMapIndicator.Visibility = System.Windows.Visibility.Collapsed;
            if (InitialVisibility == System.Windows.Visibility.Visible)
            {
                ShowBackStage(true);
            }
        }

        public Visibility InitialVisibility
        {
            get { return (Visibility)GetValue(InitialVisibilityProperty); }
            set { SetValue(InitialVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InitialVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InitialVisibilityProperty =
            DependencyProperty.Register("InitialVisibility", typeof(Visibility), typeof(MapCenter), new PropertyMetadata(Visibility.Collapsed));

        private static void OnInitialVisibilityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapCenter mapCenter = d as MapCenter;
            Visibility visibility = (Visibility)e.NewValue;
            if (visibility == Visibility.Visible)
            {
                mapCenter.ShowBackStage(true);
            }
        }

        public Visibility CloseButtonVisibility
        {
            get { return (Visibility)GetValue(CloseButtonVisibilityProperty); }
            set { SetValue(CloseButtonVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CloseButtonVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseButtonVisibilityProperty =
            DependencyProperty.Register("CloseButtonVisibility", typeof(Visibility), typeof(MapCenter), new PropertyMetadata(Visibility.Collapsed));
        
        public void Activate(bool force = false)
        {
            if (backStageControl != null)
                backStageControl.Activate(force);
        }

        private Panel MapCenterContainer = null;
        private Panel Container = null;
        public BusyIndicator LoadingMapIndicator;
        ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.BackStageControl backStageControl;
        private ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.BackStageControl BackStageControl
        {
            get
            {
                if (backStageControl == null)
                {
                    if (MapCenterContainer != null)
                    {
                        foreach (UIElement ctrl in MapCenterContainer.Children)
                        {
                            if (ctrl is ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.BackStageControl)
                            {
                                backStageControl = ctrl as ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.BackStageControl;
                                break;
                            }
                        }
                        if (backStageControl == null)
                        {
                            FrameworkElement rootVisual = Application.Current.RootVisual as FrameworkElement;
                            if (rootVisual != null)
                                rootVisual.Cursor = System.Windows.Input.Cursors.Wait;
                            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString());
                            backStageControl = new ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.BackStageControl();
                            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString());
                            backStageControl.Margin = new Thickness(0);
                            backStageControl.Visibility = System.Windows.Visibility.Collapsed;
                            MapCenterContainer.Children.Clear();
                            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString());
                            MapCenterContainer.Children.Insert(0, backStageControl);
                            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString());
                            if (rootVisual != null)
                                rootVisual.Cursor = System.Windows.Input.Cursors.Arrow;
                        }
                    }
                }
                return backStageControl;
            }
        }

        #region View
        public View View
        {
            get { return View.Instance; }
        }
        #endregion

        public bool IsVisible
        {
            get { return Container != null ? Container.Visibility == System.Windows.Visibility.Visible : false; }
        }

        #region Cancel Command
        private void close(object commandParameter)
        {
            ToggleVisibility(false);
            Visibility = System.Windows.Visibility.Collapsed;
        }

        public ICommand Close
        {
            get { return (ICommand)GetValue(CloseProperty); }
            private set { SetValue(CloseProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Cancel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CloseProperty =
            DependencyProperty.Register("Close", typeof(ICommand), typeof(MapCenter), null);
        #endregion

        #region Toggle visibility
        public TabControl TabControl { get; set; }
        List<int> previouslyVisibleTabIndices = new List<int>();
        public void ToggleVisibility(bool visible)
        {
            Visibility visibility = visible ? Visibility.Visible : System.Windows.Visibility.Collapsed;

            if (Container == null)
            {
                InitialVisibility = visibility;
                return;
            }

            if (visibility == Container.Visibility)
                return;

            #region showHideRibbonTabs
            if (TabControl != null)
            {
                if (visible)
                {
                    previouslyVisibleTabIndices.Clear();
                    TabControl.SelectedIndex = 0;
                }
                else if (TabControl.SelectedIndex == 0)
                    TabControl.SelectedIndex = 1;

                Grid mainGrid = ControlTreeHelper.FindChildOfType<Grid>(TabControl);
                if (mainGrid != null && mainGrid.Children.Count > 0)
                {
                    Grid templateTop = mainGrid.Children[0] as Grid;
                    if (templateTop != null && templateTop.Children.Count > 1)
                    {
                        Border border = templateTop.Children[1] as Border;
                        if (border != null)
                            border.Visibility = visible ? Visibility.Collapsed : Visibility.Visible;
                    }
                }
            }
            #endregion

            ShowBackStage(visible);

            //TODO:- EVAL
            //if (visible)
            //    View.HideSearchUI();
        }

        internal void ShowBackStage(bool visible)
        {
            if (Container == null)
                return;

            Visibility visibility = visible ? Visibility.Visible : System.Windows.Visibility.Collapsed;

            Container.Visibility = visibility;
            if (!visible && backStageControl != null)
                backStageControl.Visibility = visibility;
            #region Show BackStageControl if necessary
            if (visible)
            {
                if (backStageControl == null)
                {
                    if (Application.Current.Resources["MapCenterBackgroundGradientBrush"] == null)
                    {
                        ResourceDictionary rd = new ResourceDictionary();
                        rd.Source = new Uri("/ESRI.ArcGIS.Mapping.Controls.ArcGISOnline;component/Backstage/MapCenterColors.xaml", UriKind.RelativeOrAbsolute);
                        Application.Current.Resources.MergedDictionaries.Add(rd);
                    }

                    Dispatcher.BeginInvoke((Action)delegate() // Let UI thread switch to map center before loading backstage
                    {
                        loadingContents = null;
                        loadingContents_Loaded(null, null);
                    });
                }
                else
                {
                    loadingContents = null;
                    loadingContents_Loaded(null, null);
                }
            }
            #endregion
        }

        TextBlock loadingContents;
        void loadingContents_Loaded(object sender, RoutedEventArgs e)
        {
            SetupBackStage();
        }

        private void SetupBackStage()
        {
            if (BackStageControl != null)
            {
                #region Set backstage properties on first use
                AttachEvents();

                #region Get basemaps and activate backstage control
                BackStageControl.Activate();
                BackStageControl.Visibility = System.Windows.Visibility.Visible;
                if (loadingContents != null)
                    ESRI.ArcGIS.Client.Extensibility.MapApplication.Current.HideWindow(loadingContents);
                #endregion
                #endregion
            }
        }
        #endregion

        #region Event handlers and events
        public void AttachEvents()
        {
            BackStageControl.MapSelectedForOpening -= BackStageControl_MapSelectedForOpening;
            BackStageControl.MapSelectedForOpening += BackStageControl_MapSelectedForOpening;
        }

        void BackStageControl_MapSelectedForOpening(object sender, ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.MapDocumentEventArgs e)
        {
            LoadingMapIndicator.Visibility = Visibility.Visible;
            e.Document.GetMapCompleted += Document_GetMapCompleted;
            e.Document.GetMapAsync(e.DocumentID);
        }

        void Document_GetMapCompleted(object sender, Client.WebMap.GetMapCompletedEventArgs e)
        {
            LoadingMapIndicator.Visibility = Visibility.Collapsed;
            if (e.Map == null)
                return;
            BackStageControl.IsEnabled = false;
            Cursor = System.Windows.Input.Cursors.Wait;
            Map map = new Map();
            map.InitializeFromWebMap(e);

            Document doc = sender as Document;
            ViewerApplication.WebMapSettings.Linked = null;
            ViewerApplication.WebMapSettings.Document = doc;
            ViewerApplication.WebMapSettings.ItemInfo = null;
            ViewerApplication.WebMapSettings.ID = e.ItemInfo.ID;

            doc.GetMapCompleted -= Document_GetMapCompleted;

            if (MapSelectedForOpening != null)
                MapSelectedForOpening(this, new MapEventArgs() { Map = map });
            BackStageControl.IsEnabled = true;
            Cursor = System.Windows.Input.Cursors.Arrow;
        }

        public class MapEventArgs : EventArgs
        {
            public Map Map { get; set; }
        }

        public event EventHandler<MapEventArgs> MapSelectedForOpening;
        #endregion

        public static void OpenMap(MapEventArgs e)
        {
            if (MapApplication.Current != null)
                MapApplication.Current.LoadMap(e.Map);
        }

        public static string GetMapXaml(MapEventArgs e)
        {
            MapXamlWriter writer = new MapXamlWriter(true);
            return writer.MapToXaml(e.Map);
        }
    }
}
