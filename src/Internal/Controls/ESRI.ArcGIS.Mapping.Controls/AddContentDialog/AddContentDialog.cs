/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Core.DataSources;
using ESRI.ArcGIS.Mapping.DataSources;
using core = ESRI.ArcGIS.Mapping.Core;
using System.Linq;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = "BrowseContentDialog", Type = typeof(BrowseContentDialog))]
    public class AddContentDialog : Control
    {
        internal BrowseContentDialog BrowseContentDialog { get; private set; }

        public AddContentDialog()
        {
            DefaultStyleKey = typeof(AddContentDialog);

            Filter = core.DataSources.Filter.SpatiallyEnabledResources |
                     core.DataSources.Filter.ImageServices |
                     core.DataSources.Filter.FeatureServices;
        }

        #region DataSourceProvider
        /// <summary>
        /// 
        /// </summary>
        public ESRI.ArcGIS.Mapping.DataSources.DataSourceProvider DataSourceProvider
        {
            get { return GetValue(DataSourceProviderProperty) as ESRI.ArcGIS.Mapping.DataSources.DataSourceProvider; }
            set { SetValue(DataSourceProviderProperty, value); }
        }

        /// <summary>
        /// Identifies the DataSourceProvider dependency property.
        /// </summary>
        public static readonly DependencyProperty DataSourceProviderProperty =
            DependencyProperty.Register(
                "DataSourceProvider",
                typeof(ESRI.ArcGIS.Mapping.DataSources.DataSourceProvider),
                typeof(AddContentDialog),
                new PropertyMetadata(new ESRI.ArcGIS.Mapping.DataSources.DataSourceProvider()));
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
                typeof(AddContentDialog),
                new PropertyMetadata(new ConnectionsProvider()));
        #endregion

        #region Map
        /// <summary>
        /// 
        /// </summary>
        public Map Map
        {
            get
            { 
                Map map = GetValue(MapProperty) as Map;
                if (map == null && ESRI.ArcGIS.Client.Extensibility.MapApplication.Current != null)
                    return ESRI.ArcGIS.Client.Extensibility.MapApplication.Current.Map;
                return map;
            }
            set { SetValue(MapProperty, value); }
        }

        /// <summary>
        /// Identifies the Map dependency property.
        /// </summary>
        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register(
                "Map",
                typeof(Map),
                typeof(AddContentDialog),
                new PropertyMetadata(null));
        #endregion

        #region Filter
        public Filter Filter
        {
            get { return (Filter)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Filter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(Filter), typeof(AddContentDialog), null);
        #endregion

        public void Reset()
        {
            if(BrowseContentDialog != null)
                BrowseContentDialog.Reset();
        }

        public override void OnApplyTemplate()
        {
            if (BrowseContentDialog != null)
                BrowseContentDialog.ResourceSelected -= BrowseContentDialog_ResourceSelected;
            base.OnApplyTemplate();

            BrowseContentDialog = GetTemplateChild("BrowseContentDialog") as BrowseContentDialog;
            if (BrowseContentDialog != null)
                BrowseContentDialog.ResourceSelected += BrowseContentDialog_ResourceSelected;
        }

        void BrowseContentDialog_ResourceSelected(object sender, ResourceSelectedEventArgs e)
        {
            if (e == null)
                return;
            IDataSourceWithResources dataSource = DataSourceProvider.CreateNewDataSourceForConnectionType(e.ConnectionType) as IDataSourceWithResources;
            if (dataSource == null)
                return;
            Resource resource = e.Resource;
            if (resource == null)
                return;

            // Notify listeners that a resource has been selected
            OnResourceSelected(e);

            dataSource.CreateLayerCompleted += (o, args) =>
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    OnLayerAdded(new LayerAddedEventArgs() { Layer = args.Layer });
                });
            };
            dataSource.CreateLayerFailed += (o, args) =>
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    Logger.Instance.LogError(args.Exception);
                    OnLayerAddFailed(args);
                    return;
                });
            };
            dataSource.CreateLayerAsync(resource, Map != null ? Map.SpatialReference : BrowseContentDialog.MapSpatialReference, null);
            // NOTE:- layer can only be instantiated on the UI thread because it has a Canvas (UI) element
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

        protected virtual void OnResourceSelected(ResourceSelectedEventArgs args)
        {
            if (ResourceSelected != null)
                ResourceSelected(this, args);
        }

        public event EventHandler<core.ExceptionEventArgs> LayerAddFailed;
        public event EventHandler<LayerAddedEventArgs> LayerAdded;
        public event EventHandler<ResourceSelectedEventArgs> ResourceSelected;
    }
}
