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
using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client;
using System.Collections.ObjectModel;
using System.Linq;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class AttributeDisplayConfig : LayerConfigControl
    {
        private Button ToggleAllVisibilityButton;
        private DataGrid MapTipsDataGrid;        

        private bool isClearAllState = true;
        public AttributeDisplayConfig()
        {
            this.DefaultStyleKey = typeof(AttributeDisplayConfig);
        }

        public override void OnApplyTemplate()
        {
            if (ToggleAllVisibilityButton != null)
                ToggleAllVisibilityButton.Click -= ToggleAllVisibilityButton_Click;

            base.OnApplyTemplate();

            ToggleAllVisibilityButton = GetTemplateChild("ToggleAllVisibilityButton") as Button;
            if (ToggleAllVisibilityButton != null)
                ToggleAllVisibilityButton.Click += ToggleAllVisibilityButton_Click;

            MapTipsDataGrid = GetTemplateChild("MapTipsDataGrid") as DataGrid;
            
            bindUIToLayer();
        }

        private void bindUIToLayer()
        {
            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer != null)
            {
                if (MapTipsDataGrid != null)
                {
                    MapTipsDataGrid.IsEnabled = true;
                    MapTipsDataGrid.ItemsSource = graphicsLayer.GetValue(LayerExtensions.FieldsProperty) as IEnumerable<FieldInfo>;
                }
                if (ToggleAllVisibilityButton != null)
                    ToggleAllVisibilityButton.IsEnabled = true;
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
            }

            if (graphicsLayer != null)
            {
                Collection<FieldInfo> fields = graphicsLayer.GetValue(LayerExtensions.FieldsProperty) as Collection<FieldInfo>;
                if (fields != null)
                {
                    CheckToggleButtonState(fields);
                }
            }
        }

        protected override void OnLayerChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {            
            bindUIToLayer();        
        }

        void ToggleAllVisibilityButton_Click(object sender, RoutedEventArgs e)
        {
            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
             if (graphicsLayer == null)
                 return;
            
            bool visible = !isClearAllState;
            Collection<FieldInfo> fields = graphicsLayer.GetValue(LayerExtensions.FieldsProperty) as Collection<FieldInfo>;
            if (fields == null)
                return;
            for (int i=0; i < fields.Count; i++)
            {
                FieldInfo field = fields[i];
                field.VisibleInAttributeDisplay = visible;                
            }

            isClearAllState = !isClearAllState;
            VisualStateManager.GoToState(this, isClearAllState ? "ClearAllState" : "SelectAllState", false);

            // For feature layers - add the name of the field to the OutFields collection
            FeatureLayer featureLayer = Layer as FeatureLayer;
            if (featureLayer == null)
                return;

            reBuildOutFields(featureLayer, fields);
        }              

        internal void FieldInfo_AttributeDisplayChecked(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                return;

            // For feature layers - add the name of the field to the OutFields collection
            FeatureLayer featureLayer = Layer as FeatureLayer;
            if (featureLayer == null)
                return;

            bool visible = !isClearAllState;
            IEnumerable<FieldInfo> fields = featureLayer.GetValue(LayerExtensions.FieldsProperty) as IEnumerable<FieldInfo>;
            if (fields == null)
                return;
            
            // check if wildcard
            if (featureLayer.OutFields.Count == 1 && featureLayer.OutFields[0] == "*")
            {
                reBuildOutFields(featureLayer, fields);                
            }
            else
            {
                // Add if not already there
                if (!featureLayer.OutFields.Contains(fieldInfo.Name))
                {
                    featureLayer.OutFields.Add(fieldInfo.Name);
                }
            }

            CheckToggleButtonState(fields);
        }        

        internal void FieldInfo_AttributeDisplayUnChecked(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                return;

            // For feature layers - remove the name of the field to the OutFields collection
            FeatureLayer featureLayer = Layer as FeatureLayer;
            if (featureLayer == null)
                return;

            bool visible = !isClearAllState;
            IEnumerable<FieldInfo> fields = featureLayer.GetValue(LayerExtensions.FieldsProperty) as IEnumerable<FieldInfo>;
            if (fields == null)
                return;
            
            if (featureLayer.OutFields.Count == 1 && featureLayer.OutFields[0] == "*")
            {
                reBuildOutFields(featureLayer, fields);                
            }
            else
            {
                // in not also visible in map tip display and not used as  = remove it
                if (!fieldInfo.VisibleOnMapTip && !isRendererField(fieldInfo)) 
                {
                    featureLayer.OutFields.Remove(fieldInfo.Name);
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
            bool allChecked = fields.Where<FieldInfo>(f => f.VisibleInAttributeDisplay).Count() == count;
            if (allChecked)
            {                
                VisualStateManager.GoToState(this, "ClearAllState", true);
                isClearAllState = true;
                return;
            }

            bool allUnChecked = fields.Where<FieldInfo>(f => !f.VisibleInAttributeDisplay).Count() == count;
            if (allUnChecked)
            {
                VisualStateManager.GoToState(this, "SelectAllState", true);
                isClearAllState = false;
                return;
            }
        }
    }
}
