/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Toolkit.Primitives;
using System.Windows.Data;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls.MapContents
{
    public class MapContentsRefreshed : Behavior<Legend>
    {
        #region Behavior Overrides
        protected override void OnAttached()
        {
            base.OnAttached();
            if(AssociatedObject != null)
                AssociatedObject.Refreshed += AssociatedObject_Refreshed;
        }

        void AssociatedObject_Refreshed(object sender, Legend.RefreshedEventArgs e)
        {
            CorrectLegendImages(e.LayerItem);
            CorrectRendererNodes(e.LayerItem);
        }

        private void CorrectRendererNodes(LegendItemViewModel model)
        {
            LayerItemViewModel mod = model as LayerItemViewModel;
            if (mod != null)
            {
                if (MapContentsControlHelper.IsTopMostLayerType(mod.LayerType))
                {
                    GraphicsLayer lay = mod.Layer as GraphicsLayer;
                    if (lay == null)
                        return;

                    ClassBreaksRenderer classBreaksrenderer = lay.Renderer as ClassBreaksRenderer;
                    if (classBreaksrenderer != null)
                    {
                        if (mod.LegendItems != null && mod.LegendItems.Count > 0 && 
                            string.IsNullOrWhiteSpace(mod.LegendItems[0].Label))
                                mod.LegendItems[0].Label = Resources.Strings.DefaultLegentItemViewModelLabel;
                    }
                    else
                    {
                        UniqueValueRenderer uniqueRenderer = lay.Renderer as UniqueValueRenderer;
                        if (uniqueRenderer != null && string.IsNullOrWhiteSpace(uniqueRenderer.DefaultLabel))
                            uniqueRenderer.DefaultLabel = Resources.Strings.DefaultLegentItemViewModelLabel;
                    }

                    //TODO:
                    //TODO: This code adds a new node for the renderer attribute field, but currently there is no
                    //TODO: way for us to set a different template as LayerItemViewModel.Template is read-only.
                    //TODO
                    //string attribute = null;
                    //ClassBreaksRenderer classBreaksrenderer = lay.Renderer as ClassBreaksRenderer;
                    //if (classBreaksrenderer != null)
                    //{
                    //    attribute = classBreaksrenderer.Attribute;
                    //}
                    //else
                    //{
                    //    UniqueValueRenderer uniqueRenderer = lay.Renderer as UniqueValueRenderer;
                    //    if (uniqueRenderer != null)
                    //        attribute = uniqueRenderer.Attribute;
                    //}
                    //if (renModel.LegendItems != null && renModel.LegendItems.Count > 0)
                    //{
                    //    if (string.IsNullOrWhiteSpace(renModel.LegendItems[0].Label))
                    //        renModel.LegendItems[0].Label = Resources.Strings.DefaultLegentItemViewModelLabel;
                    //}
                    //if (string.IsNullOrWhiteSpace(attribute))
                    //    return;
                    //LayerItemViewModel renModel = new LayerItemViewModel(mod.Layer);
                    //renModel.Label = attribute;
                    //renModel.LayerItems = mod.LayerItems;
                    //renModel.LegendItems = mod.LegendItems;
                    //renModel.Tag = AssociatedObject.DataContext;
                    //renModel.LayerType = "Renderer Layer";

                    //mod.LayerItems = null;
                    //mod.LegendItems = null;

                    //mod.LayerItems = new System.Collections.ObjectModel.ObservableCollection<LayerItemViewModel>();
                    //mod.LayerItems.Add(renModel);
                }
            }
        }

        private void CorrectLegendImages(LegendItemViewModel model)
        {
            LayerItemViewModel mod = model as LayerItemViewModel;
            if (mod != null)
            {
                MapContentsConfiguration config = AssociatedObject.DataContext as MapContentsConfiguration;
                if (config.Mode == Mode.LayerList)
                {
                    if (mod.LegendItems != null)
                        mod.LegendItems.Clear();
                }

                bool isBaseMap = false;
                if (mod.Layer != null)
                {
                    isBaseMap = (bool)mod.Layer.GetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty);
                }
                if (isBaseMap || config.Mode == Mode.TopLevelLayersOnly)
                {
                    if (mod.LegendItems != null)
                        mod.LegendItems.Clear();
                    if (mod.LayerItems != null)
                        mod.LayerItems.Clear();

                    //only show map layer level
                    return;
                }

                if (mod.Tag == null)//first time only
                {
                    mod.Tag = config;

                    if (mod.Layer != null)
                        mod.IsExpanded = config.ExpandLayersOnAdd;

                    if (mod.LegendItems != null)
                    {
                        foreach (LegendItemViewModel leg in mod.LegendItems)
                            leg.Tag = config;
                    }

                }
            }

            if (model.LayerItemsSource != null)
            {
                foreach (LegendItemViewModel subModel in model.LayerItemsSource)
                    CorrectLegendImages(subModel);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
                AssociatedObject.Refreshed -= AssociatedObject_Refreshed;
        }
        #endregion
    }

    public class MapContentsSelectLayerItemOnClickBehavior : Behavior<FrameworkElement>
    {
        #region IsEnabled
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register(
                "IsEnabled",
                typeof(bool),
                typeof(MapContentsSelectLayerItemOnClickBehavior),
                new PropertyMetadata(true));

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
            }
        }

        void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled)
                return;

            if (AssociatedObject == null)
                return;

            if (MapApplication.Current == null || MapApplication.Current.Map == null || MapApplication.Current.Map.Layers == null)
                return;

            LayerItemViewModel layerVM = AssociatedObject.DataContext as LayerItemViewModel;
            if (layerVM == null || layerVM.Layer == null)
                return;

            MapApplication.Current.SelectedLayer = layerVM.Layer;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
            }
        }
    }

}
