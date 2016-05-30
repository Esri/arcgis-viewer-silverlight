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
using ESRI.ArcGIS.Mapping.Core;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;
using System.Windows.Data;
using System.Linq;
using System.Windows.Markup;
using System.Collections.ObjectModel;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = "ToggleAllVisibilityButton", Type = typeof(Button))]
    [TemplatePart(Name = PART_HeaderCombo, Type = typeof(ComboBox))]
    [TemplatePart(Name = "MapTipsDataGrid", Type = typeof(DataGrid))]
    [TemplateVisualState(Name = "ClearAllState", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "SelectAllState", GroupName = "CommonStates")]
    public class MapTipsLayerConfig : LayerConfigControl
    {
        const string PART_HeaderCombo = "HeaderCombo";
        internal Button ToggleAllVisibilityButton;
        internal DataGrid MapTipsDataGrid;
        internal ComboBox HeaderCombo;

        private bool isClearAllState = true;
        public MapTipsLayerConfig()
        {
            this.DefaultStyleKey = typeof(MapTipsLayerConfig);
        }

        public override void OnApplyTemplate()
        {
            if (ToggleAllVisibilityButton != null)
                ToggleAllVisibilityButton.Click -= ToggleAllVisibilityButton_Click;
            if (HeaderCombo != null)
                HeaderCombo.SelectionChanged -= HeaderCombo_SelectionChanged;

            base.OnApplyTemplate();

            ToggleAllVisibilityButton = GetTemplateChild("ToggleAllVisibilityButton") as Button;
            if (ToggleAllVisibilityButton != null)
                ToggleAllVisibilityButton.Click += ToggleAllVisibilityButton_Click;

            MapTipsDataGrid = GetTemplateChild("MapTipsDataGrid") as DataGrid;
            HeaderCombo = GetTemplateChild(PART_HeaderCombo) as ComboBox;
            if (HeaderCombo != null)
                HeaderCombo.SelectionChanged += HeaderCombo_SelectionChanged;

            bindUI();

            OnInitCompleted();
        }

        internal event EventHandler InitCompleted;

        protected virtual void OnInitCompleted()
        {
            if (InitCompleted != null)
                InitCompleted(this, EventArgs.Empty);
        }



        public MapTipsConfigInfo Info
        {
            get { return (MapTipsConfigInfo)GetValue(InfoProperty); }
            set { SetValue(InfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Info.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InfoProperty =
            DependencyProperty.Register("Info", typeof(MapTipsConfigInfo), typeof(MapTipsLayerConfig), new PropertyMetadata(null, OnInfoChanged));

        static void OnInfoChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            MapTipsLayerConfig config = o as MapTipsLayerConfig;
            if (config != null)
            {
                if (config.Info != null)
                {
                    config.Info.PropertyChanged += config.Info_PropertyChanged;
                    config.Layer = config.Info.Layer;
                    config.LayerInfo = config.Info.SelectedItem;
                }
                config.bindUI();
            }
        }

        void Info_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedItem")
            {
                LayerInfo = Info.SelectedItem;
                bindUI();
            }
        }
        
        public LayerInformation LayerInfo
        {
            get { return (LayerInformation)GetValue(LayerInfoProperty); }
            set { SetValue(LayerInfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LayerInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayerInfoProperty =
            DependencyProperty.Register("LayerInfo", typeof(LayerInformation), typeof(MapTipsLayerConfig),  new PropertyMetadata(null));

        
        private void bindUI()
        {
            if (LayerInfo == null)
                return;
            object fields = LayerInfo.Fields;
             if (fields != null)
             {
                 if (MapTipsDataGrid != null)
                 {
                     MapTipsDataGrid.IsEnabled = true;
                     MapTipsDataGrid.ItemsSource = fields as IEnumerable<FieldInfo>;
                 }
                 if (ToggleAllVisibilityButton != null)
                     ToggleAllVisibilityButton.IsEnabled = true;
                 if (fields is Collection<FieldInfo>)
                 {
                     CheckToggleButtonState(fields as IEnumerable<FieldInfo>);
                 }
                 if (HeaderCombo != null && fields is IEnumerable<FieldInfo>)
                 {
                     string displayField = LayerInfo.DisplayField;
                     Collection<FieldInfo> filteredFields = new Collection<FieldInfo>();
                     // if not set for popup on click, add <none>
                     FieldInfo selectedItem = null;
                     if (Info != null && Info.PopUpsOnClick != null && !Info.PopUpsOnClick.Value && !Info.LayerSelectionVisibility)
                     {
                        FieldInfo noneField = new FieldInfo() { Name = ESRI.ArcGIS.Mapping.Core.LocalizableStrings.NoneInAngleBraces };
                        filteredFields.Add(noneField);
                        selectedItem = noneField;

                     }

                     foreach (FieldInfo item in fields as IEnumerable<FieldInfo>)
                     {
                         if (FieldInfo.SupportedInHeader(item.FieldType))
                         {
                             filteredFields.Add(item);

                             if (!string.IsNullOrEmpty(displayField))
                             {
                                 if (item.Name.ToLower().Equals(displayField.ToLower()))
                                     selectedItem = item;
                             }
                         }
                     }
                     HeaderCombo.ItemsSource = filteredFields;
                     HeaderCombo.IsEnabled = true;
                     HeaderCombo.SelectedItem = selectedItem;
                 }
             }
             else
             {
                 if (MapTipsDataGrid != null)
                 {
                     MapTipsDataGrid.IsEnabled = false;
                     MapTipsDataGrid.ItemsSource = null;
                 }
                 if (ToggleAllVisibilityButton != null)
                     ToggleAllVisibilityButton.IsEnabled = false;
                 if (HeaderCombo != null)
                 {
                     HeaderCombo.ItemsSource = null;
                     HeaderCombo.IsEnabled = false;
                 }
             }
        }

        public void BindUI()
        {
            bindUI();
        }

        void ToggleAllVisibilityButton_Click(object sender, RoutedEventArgs e)
        {
            bool visible = !isClearAllState;
            if (LayerInfo == null || LayerInfo.Fields == null)
                return;
            IEnumerable<FieldInfo> fields = LayerInfo.Fields as IEnumerable<FieldInfo>;
            if (fields == null)
                return;
            foreach (FieldInfo field in fields)
            {
                field.VisibleOnMapTip = visible;                
            }

            isClearAllState = !isClearAllState;
            VisualStateManager.GoToState(this, isClearAllState ? "ClearAllState" : "SelectAllState", false);

            // For feature layers - add the name of the field to the OutFields collection
            FeatureLayer featureLayer = Layer as FeatureLayer;
            if (featureLayer == null)
                return;

            reBuildOutFields(featureLayer, fields);
            LayerExtensions.SetIsMapTipDirty(featureLayer, true);
        }

        internal void FieldInfo_MapTipVisiblityChecked(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                return;

            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer != null)
                LayerExtensions.SetIsMapTipDirty(graphicsLayer, true);

           
            bool visible = !isClearAllState;
            IEnumerable<FieldInfo> fields = LayerInfo.Fields;
            if (fields == null)
                return;

            // For feature layers - add the name of the field to the OutFields collection
            FeatureLayer featureLayer = Layer as FeatureLayer;
            if (featureLayer != null)
            {

                if (featureLayer.OutFields.Count == 1 && featureLayer.OutFields[0] == "*")
                {
                    reBuildOutFields(featureLayer, fields);
                }
                else
                {
                    if (!featureLayer.OutFields.Contains(fieldInfo.Name))
                        featureLayer.OutFields.Add(fieldInfo.Name);
                }
            }

            CheckToggleButtonState(fields);
        }               

        internal void FieldInfo_MapTipVisibilityUnChecked(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                return;
            
            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer != null)
                LayerExtensions.SetIsMapTipDirty(graphicsLayer, true);

            bool visible = !isClearAllState;
            IEnumerable<FieldInfo> fields = LayerInfo.Fields;
            if (fields == null)
                return;

            // For feature layers - remove the name of the field to the OutFields collection
            FeatureLayer featureLayer = Layer as FeatureLayer;
            if (featureLayer != null)
            {
                if (featureLayer.OutFields.Count == 1 && featureLayer.OutFields[0] == "*")
                {
                    reBuildOutFields(featureLayer, fields);
                }
                else
                {
                    // in not also visible in attribute display and not the renderer field = remove it
                    if (!fieldInfo.VisibleInAttributeDisplay && !isRendererField(fieldInfo))
                    {
                        featureLayer.OutFields.Remove(fieldInfo.Name);
                    }
                }
            }

            CheckToggleButtonState(fields);
        }

        private bool isRendererField(FieldInfo field)
        {
            if (field == null)
                return false;

            string rendererAttr = getRendererAttributeName();
            if (rendererAttr == field.Name)
                return true;

            return false;
        }

        private string getRendererAttributeName()
        {
            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer != null)
            {
                ClassBreaksRenderer classBreaksRenderer = graphicsLayer.Renderer as ClassBreaksRenderer;
                if (classBreaksRenderer != null)
                {
                    return classBreaksRenderer.Field;
                }
                else
                {
                    UniqueValueRenderer uniqueValueRenderer = graphicsLayer.Renderer as UniqueValueRenderer;
                    if (uniqueValueRenderer != null)
                    {
                        return uniqueValueRenderer.Field;
                    }
                }
            }
            return null;
        }

        private void reBuildOutFields(FeatureLayer featureLayer, IEnumerable<FieldInfo> fields)
        {
            featureLayer.OutFields.Clear();
            string rendererAttr = getRendererAttributeName();
            foreach (FieldInfo field in fields)
            {
                if (field.VisibleInAttributeDisplay || field.VisibleOnMapTip || field.Name == rendererAttr)
                {
                    featureLayer.OutFields.Add(field.Name);
                }
            }
        }

        void CheckToggleButtonState(IEnumerable<FieldInfo> fields)
        {
            if (fields == null)
                return;

            int count = fields.Count();
            bool allChecked = fields.Where<FieldInfo>(f => f.VisibleOnMapTip).Count() == count;
            if (allChecked)
            {                
                VisualStateManager.GoToState(this, "ClearAllState", true);
                isClearAllState = true; 
                return;
            }

            bool allUnChecked = fields.Where<FieldInfo>(f => !f.VisibleOnMapTip).Count() == count;
            if (allUnChecked)
            {
                VisualStateManager.GoToState(this, "SelectAllState", true);
                isClearAllState = false;
                return;
            }
        }

        void HeaderCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LayerInfo != null && HeaderCombo.SelectedItem is FieldInfo)
            {
                LayerInfo.DisplayField = (HeaderCombo.SelectedItem as FieldInfo).Name;

                GraphicsLayer layer = Layer as GraphicsLayer;
                if (layer != null)
                {
                    if (HeaderCombo.SelectedItem is FieldInfo)
                        LayerExtensions.SetDisplayField(layer, LayerInfo.DisplayField);
                }
                if (DisplayFieldChanged != null)
                    DisplayFieldChanged(this, null);
            }
        }

        public event EventHandler DisplayFieldChanged;
    }    

    public class VisibilityBasedOnLayerSupportsMapTipsConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is GraphicsLayer ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

