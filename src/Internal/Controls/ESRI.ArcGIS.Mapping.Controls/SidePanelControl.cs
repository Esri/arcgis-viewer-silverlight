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
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.DataSources;
using core = ESRI.ArcGIS.Mapping.Core;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Markup;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{    
    public class SidePanelControl : Control
    {
        private const string PART_MAPCONTENTSCONTROL = "MapContentLayerConfigurationHost";
        private const string PART_SIDEPANELTABCONTROL = "SidePanelTabControl";
        private const string PART_ADDCONTENTCONTROL = "AddContentDialog";

        MapContentLayerConfigurationHost MapContentLayerConfigurationHost;
        TabControl SidePanelTabControl;
        AddContentDialog AddContentDialog;

#if SILVERLIGHT
        ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.SearchControl SearchControl;
#endif

        public SidePanelControl()
        {
            DefaultStyleKey = typeof(SidePanelControl);
        }

        public override void OnApplyTemplate()
        {
            if (MapContentLayerConfigurationHost != null)
            {
                MapContentLayerConfigurationHost.LayerSelectionChanged -= MapContentsControl_LayerSelectionChanged;
            }

            if (SidePanelTabControl != null)
                SidePanelTabControl.SelectionChanged -= SidePanelTabControl_SelectionChanged;

            if (AddContentDialog != null)
            {               
                AddContentDialog.LayerAdded -= AddContentDialog_LayerAdded;
                AddContentDialog.LayerAddFailed -= AddContentDialog_LayerAddFailed;
            }

#if SILVERLIGHT
            if (SearchControl != null)
            {
                SearchControl.LayerSelectedForAdd -= SearchControl_LayerSelectedForAdd;
                SearchControl.NoteSelectedForAdd -= SearchControl_NoteSelectedForAdd;
            }
#endif

            base.OnApplyTemplate();

            MapContentLayerConfigurationHost = GetTemplateChild(PART_MAPCONTENTSCONTROL) as MapContentLayerConfigurationHost;
            if (MapContentLayerConfigurationHost != null)
            {
                MapContentLayerConfigurationHost.LayerSelectionChanged += MapContentsControl_LayerSelectionChanged;
            }

            SidePanelTabControl = GetTemplateChild(PART_SIDEPANELTABCONTROL) as TabControl;
            if (SidePanelTabControl != null)
                SidePanelTabControl.SelectionChanged += SidePanelTabControl_SelectionChanged;

            AddContentDialog = GetTemplateChild(PART_ADDCONTENTCONTROL) as AddContentDialog;
            if (AddContentDialog != null)
            {
                AddContentDialog.DataSourceProvider = DataSourceProvider;
                AddContentDialog.ConnectionsProvider = ConnectionsProvider;
                AddContentDialog.Map = Map;
                AddContentDialog.LayerAdded += AddContentDialog_LayerAdded;
                AddContentDialog.LayerAddFailed += AddContentDialog_LayerAddFailed;
            }

            #if SILVERLIGHT
            SearchControl = GetTemplateChild("SearchControl") as ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.SearchControl;
			if (SearchControl != null)
			{				
				SearchControl.LayerSelectedForAdd += SearchControl_LayerSelectedForAdd;
				SearchControl.NoteSelectedForAdd += SearchControl_NoteSelectedForAdd;
			}
            #endif
        }        

        void SidePanelTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SidePanelTabControl.SelectedIndex != 1)
            {
                if (AddContentDialog != null && AddContentDialog.BrowseContentDialog != null)
                    AddContentDialog.BrowseContentDialog.CloseChildPopups();
            }
        }

#if SILVERLIGHT
        void SearchControl_NoteSelectedForAdd(object sender, ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.NoteEventArgs e)
		{
            GraphicsLayer noteLayer = MapCenter.AddToMap(e.Name, e.Graphic, Map);
            if (MapContentLayerConfigurationHost != null)
            {
                ShowMapContents();
                
                // unselect all other layers
                foreach (Layer layer in Map.Layers)
                    LayerExtensions.SetIsSelected(layer, false);

                // select the note layer
                if (noteLayer != null)
                    LayerExtensions.SetIsSelected(noteLayer, true);

                MapContentLayerConfigurationHost.RefreshMapBindings();                                
            }
		}

		void SearchControl_LayerSelectedForAdd(object sender, ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.LayerEventArgs e)
		{
			Layer layer = e.Layer;
			if (layer != null)
            {
                FeatureLayer featureLayer = layer as FeatureLayer;
                if (featureLayer != null)
                {
                    ESRI.ArcGIS.Client.FeatureService.FeatureLayerInfo featureLayerInfo = featureLayer.LayerInfo;
                    if (featureLayerInfo != null)
                    {
                        featureLayer.SetValue(LayerExtensions.DisplayUrlProperty, featureLayer.Url);

                        if (featureLayerInfo.GeometryType == Client.Tasks.GeometryType.Point)
                            LayerExtensions.SetGeometryType(featureLayer, GeometryType.Point);
                        else if (featureLayerInfo.GeometryType == Client.Tasks.GeometryType.Polyline)
                            LayerExtensions.SetGeometryType(featureLayer, GeometryType.Polyline);
                        else if (featureLayerInfo.GeometryType == Client.Tasks.GeometryType.Polygon)
                            LayerExtensions.SetGeometryType(featureLayer, GeometryType.Polygon);

                        Collection<FieldInfo> fields = LayerExtensions.GetFields(featureLayer);
                        if (fields == null)
                            fields = new Collection<FieldInfo>();
                        if (fields.Count < 1)
                        {
                            if (featureLayerInfo.Fields != null)
                            {
                                foreach (ESRI.ArcGIS.Client.Field field in featureLayerInfo.Fields)
                                {
                                    if (field.Type == Client.Field.FieldType.Geometry)
                                        continue;

                                    fields.Add(new FieldInfo()
                                    {
                                        DisplayName = field.Alias,
                                        FieldType = mapFieldType(field.Type),
                                        Name = field.Name,
                                        VisibleInAttributeDisplay = true,
                                        VisibleOnMapTip = true,
                                    });
                                }
                            }
                            LayerExtensions.SetFields(featureLayer, fields);
                            LayerExtensions.SetDisplayField(featureLayer, featureLayerInfo.DisplayField);
                        }
                    }
                }
				OnLayerAdded(new LayerAddedEventArgs() { Layer = layer });
                ShowMapContents();
		    }            
		}

        public FieldType mapFieldType(ESRI.ArcGIS.Client.Field.FieldType fieldType)
        {
            if (fieldType == ESRI.ArcGIS.Client.Field.FieldType.Double
                || fieldType == Client.Field.FieldType.Single)
            {
                return FieldType.DecimalNumber;
            }
            else if (fieldType == Client.Field.FieldType.OID
                || fieldType == ESRI.ArcGIS.Client.Field.FieldType.Integer
                || fieldType == ESRI.ArcGIS.Client.Field.FieldType.SmallInteger)
            {
                return FieldType.Integer;
            }
            else if (fieldType == Client.Field.FieldType.Date)
            {
                return FieldType.DateTime;
            }
            return FieldType.Text; // For now all other fields are treated as strings
        }
#endif

        void AddContentDialog_LayerAddFailed(object sender, ESRI.ArcGIS.Mapping.Core.ExceptionEventArgs e)
        {
            OnLayerAddFailed(e);
        }

        void AddContentDialog_LayerAdded(object sender, LayerAddedEventArgs e)
        {
            if (e.Layer == null)
                return;			
            if (Map != null)
            {
			    OnLayerAdded(e);
                ShowMapContents();
            }
        }

        #region Map
        /// <summary>
        /// 
        /// </summary>
        public Map Map
        {
            get { return GetValue(MapProperty) as Map; }
            set { SetValue(MapProperty, value); }
        }

        /// <summary>
        /// Identifies the Map dependency property.
        /// </summary>
        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register(
                "Map",
                typeof(Map),
                typeof(SidePanelControl),
                new PropertyMetadata(null, OnMapPropertyChanged));

        /// <summary>
        /// MapProperty property changed handler.
        /// </summary>
        /// <param name="d">SidePanelControl that changed its Map.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SidePanelControl source = d as SidePanelControl;
            source.onMapChanged();
        }

        private void onMapChanged()
        {
            if (AddContentDialog != null)
                AddContentDialog.Map = Map;
        }
        #endregion 
        
        #region ConnectionsProvider
        /// <summary>
        /// 
        /// </summary>
        public ConnectionsProvider ConnectionsProvider
        {
            get { return GetValue(ConnectionsProviderProperty) as ConnectionsProvider; }
            set { SetValue(ConnectionsProviderProperty, value); }
        }

        /// <summary>
        /// Identifies the ConnectionsProvider dependency property.
        /// </summary>
        public static readonly DependencyProperty ConnectionsProviderProperty =
            DependencyProperty.Register(        
                "ConnectionsProvider",
                typeof(ConnectionsProvider),
                typeof(SidePanelControl),
                new PropertyMetadata(null, OnConnectionsProviderPropertyChanged));

        /// <summary>
        /// ConnectionsProviderProperty property changed handler.
        /// </summary>
        /// <param name="d">SidePanelControl that changed its ConnectionsProvider.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnConnectionsProviderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SidePanelControl source = d as SidePanelControl;
            source.onConnectionsProviderChanged(); 
        }

        private void onConnectionsProviderChanged()
        {
            if (AddContentDialog != null)
                AddContentDialog.ConnectionsProvider = ConnectionsProvider;
        }
        #endregion 

        #region DataSourceProvider
        /// <summary>
        /// 
        /// </summary>
        public DataSourceProvider DataSourceProvider
        {
            get { return GetValue(DataSourceProviderProperty) as DataSourceProvider; }
            set { SetValue(DataSourceProviderProperty, value); }
        }

        /// <summary>
        /// Identifies the DataSourceProvider dependency property.
        /// </summary>
        public static readonly DependencyProperty DataSourceProviderProperty =
            DependencyProperty.Register(
                "DataSourceProvider",
                typeof(DataSourceProvider),
                typeof(SidePanelControl),
                new PropertyMetadata(null, OnDataSourceProviderPropertyChanged));

        /// <summary>
        /// DataSourceProviderProperty property changed handler.
        /// </summary>
        /// <param name="d">SidePanelControl that changed its DataSourceProvider.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnDataSourceProviderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SidePanelControl source = d as SidePanelControl;
            source.onDataSourceProviderChanged();
        }

        private void onDataSourceProviderChanged()
        {
            if (AddContentDialog != null)
                AddContentDialog.DataSourceProvider = DataSourceProvider;
        }
        #endregion 

		#region ShowContents,Browse,Search
		/// <summary>
		/// 
		/// </summary>
		public bool ShowContentsTab
		{
			get { return (bool)GetValue(ShowContentsTabProperty); }
			set { SetValue(ShowContentsTabProperty, value); }
		}

		/// <summary>
		/// Identifies the ShowContents dependency property.
		/// </summary>
		public static readonly DependencyProperty ShowContentsTabProperty =
						DependencyProperty.Register(
										"ShowContentsTab",
										typeof(bool),
										typeof(SidePanelControl),
										new PropertyMetadata(true, OnTabItemVisibilityChanged));

		/// <summary>
		/// 
		/// </summary>
		public bool ShowBrowseTab
		{
			get { return (bool)GetValue(ShowBrowseTabProperty); }
			set { SetValue(ShowBrowseTabProperty, value); }
		}

		/// <summary>
		/// Identifies the ShowContents dependency property.
		/// </summary>
		public static readonly DependencyProperty ShowBrowseTabProperty =
						DependencyProperty.Register(
										"ShowBrowseTab",
										typeof(bool),
										typeof(SidePanelControl),
										new PropertyMetadata(true, OnTabItemVisibilityChanged));
		/// <summary>
		/// 
		/// </summary>
		public bool ShowSearchTab
		{
			get { return (bool)GetValue(ShowSearchTabProperty); }
			set { SetValue(ShowSearchTabProperty, value); }
		}

		/// <summary>
		/// Identifies the ShowContents dependency property.
		/// </summary>
		public static readonly DependencyProperty ShowSearchTabProperty =
						DependencyProperty.Register(
										"ShowSearchTab",
										typeof(bool),
										typeof(SidePanelControl),
										new PropertyMetadata(true, OnTabItemVisibilityChanged));

		private static void OnTabItemVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SidePanelControl source = d as SidePanelControl;
			int selectedIndex = source.SelectedTabIndex;
			if (!((source.SelectedTabIndex == BrowseTabIndex && source.ShowBrowseTab) ||
				(source.SelectedTabIndex == SearchTabIndex && source.ShowSearchTab) ||
				(source.SelectedTabIndex == ContentsTabIndex && source.ShowContentsTab)))
			{
				if (source.ShowBrowseTab)
					source.SelectedTabIndex = BrowseTabIndex;
				else if (source.ShowSearchTab)
					source.SelectedTabIndex = SearchTabIndex;
				else if (source.ShowContentsTab)
					source.SelectedTabIndex = ContentsTabIndex;
			}
			int i = 0;
			if (source.ShowBrowseTab) i++;
			if (source.ShowSearchTab) i++;
			if (source.ShowContentsTab) i++;
            if (i == 1 || !source.ShowTabHeaders)
				source.ShowTabs = false;
			else
				source.ShowTabs = true;
		}
		#endregion        

		public bool ShowTabs
		{
			get { return (bool)GetValue(ShowTabsProperty); }
			set { SetValue(ShowTabsProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ShowTabs.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ShowTabsProperty =
				DependencyProperty.Register("ShowTabs", typeof(bool), typeof(SidePanelControl), new PropertyMetadata(true));

        public bool ShowTabHeaders
        {
            get { return (bool)GetValue(ShowTabHeadersProperty); }
            set { SetValue(ShowTabHeadersProperty, value); }
        }

        public static readonly DependencyProperty ShowTabHeadersProperty =
                DependencyProperty.Register("ShowTabHeaders", typeof(bool), typeof(SidePanelControl), new PropertyMetadata(true, OnTabItemVisibilityChanged));

        #region SelectedTabIndex
        /// <summary>
        /// 
        /// </summary>
        public int SelectedTabIndex
        {
            get { return (int)GetValue(SelectedTabIndexProperty); }
            set { SetValue(SelectedTabIndexProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectedTabIndex dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedTabIndexProperty =
            DependencyProperty.Register(
                "SelectedTabIndex",
                typeof(int),
                typeof(SidePanelControl),
                new PropertyMetadata(ContentsTabIndex));
        #endregion public int SelectedTabIndex
        
        void MapContentsControl_LayerSelectionChanged(object sender, LayerEventArgs e)
        {
            OnLayerSelectionChanged(e);
        }

        private void GoToMapContents()
        {
            if(MapContentLayerConfigurationHost != null)
                MapContentLayerConfigurationHost.GoToMapContent(false);
        }

        public void GoToLayerConfiguration()
        {
            bool differentTab = SelectedTabIndex != 2;
            SelectedTabIndex = 2;
            if (SidePanelTabControl != null)
                SidePanelTabControl.SelectedIndex = SelectedTabIndex;

            if (MapContentLayerConfigurationHost != null)
            {
                if (differentTab)
                {
                    MapContentLayerConfigurationHost.GoToLayerConfiguration(false);
                }
                else
                {
                    MapContentLayerConfigurationHost.GoToLayerConfiguration(true);
                }
            }
        }

        protected virtual void OnLayerSelectionChanged(LayerEventArgs args)
        {
            if (LayerSelectionChanged != null)
                LayerSelectionChanged(this, args);
        }

        public event EventHandler<LayerEventArgs> LayerSelectionChanged;

        public void ShowMapContents()
        {
            SelectedTabIndex = 2;
            GoToMapContents();
            if (SidePanelTabControl != null)
                SidePanelTabControl.SelectedIndex = SelectedTabIndex;
            
        }

        public void RefreshMapContents()
        {
            if (MapContentLayerConfigurationHost != null)
            {
                MapContentLayerConfigurationHost.RefreshMapBindings();
            }
        }

		const int SearchTabIndex = 0;
		const int BrowseTabIndex = 1;
		const int ContentsTabIndex = 2;
        public void ShowSearch()
        {
			SelectedTabIndex = SearchTabIndex;
            if (SidePanelTabControl != null)
                SidePanelTabControl.SelectedIndex = SelectedTabIndex;
        }

        public void ShowAddContentFromServer()
        {
            SelectedTabIndex = 1;
            if (SidePanelTabControl != null)
                SidePanelTabControl.SelectedIndex = SelectedTabIndex;
        }

        public bool IsMouseOverPopup(Point mousePoint)
        {
#if SILVERLIGHT
			if (SearchControl == null || SidePanelTabControl.SelectedIndex != SearchTabIndex)
                return false;

            if (SearchControl.Visibility == System.Windows.Visibility.Visible)
            {
				try
				{
                if (SearchControl.IsMouseOver(mousePoint) || ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.DetailsPopup.IsMouseOver(mousePoint))
                    return true;
            }
				catch
				{
					return false;
				}
			}

#endif
            return false;

        }

        public void HideSearchUI()
        {
#if SILVERLIGHT
            if (SearchControl != null)
                SearchControl.HideUIElements();
#endif
        }

        protected virtual void OnLayerAdded(LayerAddedEventArgs args)
        {
            if (LayerAdded != null)
                LayerAdded(this, args);
        }

        protected virtual void OnLayerAddFailed(core.ExceptionEventArgs args)
        {
            if (LayerAddFailed != null)
                LayerAddFailed(this, args);
        }

        public event EventHandler<core.ExceptionEventArgs> LayerAddFailed;
        public event EventHandler<LayerAddedEventArgs> LayerAdded;
    }
}
