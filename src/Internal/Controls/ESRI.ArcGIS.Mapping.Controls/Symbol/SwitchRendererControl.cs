/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class SwitchRendererControl : LayerConfigControl
    {
        private const string PART_RENDERERTYPE = "RendererType";
        private const string PART_ATTRIBUTENAME = "AttributeName";

        internal ComboBox RendererType;
        internal ComboBox AttributeName;        
        
        public SwitchRendererControl()
        {
            DefaultStyleKey = typeof(SwitchRendererControl);
        }

        public override void OnApplyTemplate()
        {
            if (RendererType != null)
                RendererType.SelectionChanged -= RendererType_SelectionChanged;

            if (AttributeName != null)
                AttributeName.SelectionChanged -= AttributeName_SelectionChanged;

            base.OnApplyTemplate();

            RendererType = GetTemplateChild(PART_RENDERERTYPE) as ComboBox;                                  

            AttributeName = GetTemplateChild(PART_ATTRIBUTENAME) as ComboBox;

            buildDropDowns();

            if (RendererType != null)
                RendererType.SelectionChanged += RendererType_SelectionChanged;

            if (AttributeName != null)
                AttributeName.SelectionChanged += AttributeName_SelectionChanged;

            if (InitCompleted != null)
                InitCompleted(this, EventArgs.Empty);
        }

        internal event EventHandler InitCompleted;

        protected override void OnLayerChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (RendererType != null)
                RendererType.SelectionChanged -= RendererType_SelectionChanged;

            if (AttributeName != null)
                AttributeName.SelectionChanged -= AttributeName_SelectionChanged;

            buildDropDowns();

            if (RendererType != null)
                RendererType.SelectionChanged += RendererType_SelectionChanged;

            if (AttributeName != null)
                AttributeName.SelectionChanged += AttributeName_SelectionChanged;
        }

        private void buildDropDowns()
        {
            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
                return;

            bool hasNumericField = false;
            bool hasAUniqueValueField = false;
            IEnumerable<FieldInfo> fields = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetFields(graphicsLayer);
            if (fields != null)
            {
                foreach (FieldInfo field in fields)
                {
                    if (field.FieldType == FieldType.Integer || field.FieldType == FieldType.DecimalNumber || field.FieldType == FieldType.Currency)
                    {
                        hasNumericField = true;
                        hasAUniqueValueField = true;
                        break;
                    }
                    else if (field.FieldType != FieldType.Attachment)
                    {
                        hasAUniqueValueField = true;
                    }

                    if (hasNumericField && hasAUniqueValueField)
                        break;
                }
            }

            string rendererType = null;            
            if (RendererType != null)
            {                
                RendererType.Items.Clear();
                RendererType.Items.Add(LocalizableStrings.SingleSymbol);
                if(hasNumericField)
                    RendererType.Items.Add(LocalizableStrings.ClassBreaks);
                if(hasAUniqueValueField)
                    RendererType.Items.Add(LocalizableStrings.UniqueValues);
                RendererType.SelectedIndex = 0;

                if (hasNumericField && graphicsLayer.Renderer is ClassBreaksRenderer)
                {
                    RendererType.SelectedIndex = 1;
                    rendererType = Constants.ClassBreaksRenderer;
                    if (AttributeName != null)
                        AttributeName.Visibility = Visibility.Visible;
                }
                else if (hasAUniqueValueField && graphicsLayer.Renderer is UniqueValueRenderer)
                {                    
                    RendererType.SelectedIndex = hasNumericField ? 2 : 1;
                    rendererType = Constants.UniqueValueRenderer;
                    if (AttributeName != null)
                        AttributeName.Visibility = Visibility.Visible;
                }
                else
                {
                    RendererType.SelectedIndex = 0;
                    if (AttributeName != null)
                        AttributeName.Visibility = Visibility.Collapsed;
                }
            }

            if (AttributeName != null)
                buildAttributeNamesList(graphicsLayer, rendererType);
        }

        void buildAttributeNamesList(GraphicsLayer graphicsLayer, string rendererType)
        {
            if (graphicsLayer == null)
                return;

            Collection<FieldInfo> fields = graphicsLayer.GetValue(ESRI.ArcGIS.Mapping.Core.LayerExtensions.FieldsProperty) as Collection<FieldInfo>;
            if (fields != null)
            {
                if (rendererType == Constants.ClassBreaksRenderer)
                {
                    IEnumerable<FieldInfo> numericFields = fields.Where<FieldInfo>(f => f.FieldType == FieldType.Integer || f.FieldType == FieldType.DecimalNumber || f.FieldType == FieldType.Currency);
                    FieldInfo selectedField = null;
                    ClassBreaksRenderer classBreaksRenderer = graphicsLayer.Renderer as ClassBreaksRenderer;
                    if (classBreaksRenderer != null)
                    {
                        foreach (FieldInfo fieldInfo in numericFields)
                        {
                            if (string.Compare(fieldInfo.Name, classBreaksRenderer.Field) == 0)
                            {
                                selectedField = fieldInfo;
                                break;
                            }
                        }
                    }
                    AttributeName.ItemsSource = numericFields;
                    if (null != selectedField)
                    {
                        _ignoreAttributeChangedEvent = true;
                        AttributeName.SelectedItem = selectedField;
                        _ignoreAttributeChangedEvent = false;
                    }
                }
                else
                {
                    if (rendererType == Constants.UniqueValueRenderer)
                    {
                        FieldInfo selectedField = null;
                        IEnumerable<FieldInfo> allowedFields = fields.Where<FieldInfo>(f => f.FieldType != FieldType.Attachment);
                        UniqueValueRenderer uniqueValueRenderer = graphicsLayer.Renderer as UniqueValueRenderer;
                        if (uniqueValueRenderer != null)
                        {
                            foreach (FieldInfo fieldInfo in allowedFields)
                            {
                                if (string.Compare(fieldInfo.Name, uniqueValueRenderer.Field) == 0)
                                {
                                    selectedField = fieldInfo;
                                    break;
                                }
                            }
                        }
                        AttributeName.ItemsSource = allowedFields;
                        if (null != selectedField)
                        {
                            _ignoreAttributeChangedEvent = true;
                            AttributeName.SelectedItem = selectedField;
                            _ignoreAttributeChangedEvent = false;
                        }
                    }
                    else
                    {
                        AttributeName.ItemsSource = null;
                    }
                }
            }
        }

        bool _ignoreAttributeChangedEvent = false;
        void AttributeName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_ignoreAttributeChangedEvent)
                return;
            if (AttributeName.SelectedItem != null)
            {
                GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
                if (graphicsLayer == null)
                    return;

                FieldInfo newAttributeField = AttributeName.SelectedItem as FieldInfo;
                graphicsLayer.ChangeAttributeForRenderer(newAttributeField);

                OnLayerRendererAttributeChanged(new LayerRendererAttributeChangedEventArgs() { 
                     RendererAttributeField = newAttributeField,
                });
            }
        }

        bool _ignoreRendererTypeChangedEvent = false;
        void RendererType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_ignoreRendererTypeChangedEvent)
                return;

            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
                return;

            string rendererType = RendererType.SelectedItem.ToString();
            if (rendererType == LocalizableStrings.SingleSymbol)
                rendererType = Constants.SimpleRenderer;
            else if (rendererType == LocalizableStrings.ClassBreaks)
                rendererType = Constants.ClassBreaksRenderer;
            else if (rendererType == LocalizableStrings.UniqueValues)
                rendererType = Constants.UniqueValueRenderer;

            if (AttributeName != null)
            {
                AttributeName.Visibility = Constants.ClassBreaksRenderer.Equals(rendererType) || Constants.UniqueValueRenderer.Equals(rendererType)
                    ? Visibility.Visible : Visibility.Collapsed;
                buildAttributeNamesList(graphicsLayer, rendererType);
            }

            // 1. Find the attribute field to use as renderer            
            FieldInfo attributeField = null;

            // 2. Check if the dropdown had a selected one
            if (AttributeName.SelectedItem != null)
                attributeField = AttributeName.SelectedItem as FieldInfo;

            // 3. If none selected, Use first one in list
            if (attributeField == null || string.IsNullOrEmpty(attributeField.Name))
            {
                // No attribute selected
                if (AttributeName.Items.Count > 0)
                {
                    _ignoreAttributeChangedEvent = true;
                    AttributeName.SelectedIndex = 0; // Select the first one
                    _ignoreAttributeChangedEvent = false;
                    attributeField = AttributeName.Items[0] as FieldInfo;
                }
            }

            FieldInfo newAttributeField = graphicsLayer.ChangeRenderer(rendererType, attributeField);

            OnLayerRendererChanged(new LayerRendererChangedEventArgs() { 
                 RendererAttributeField = newAttributeField,
                 RendererType = rendererType,
            });
        }

        #region SymbolConfigProvider
        /// <summary>
        /// 
        /// </summary>
        public SymbolConfigProvider SymbolConfigProvider
        {
            get { return GetValue(SymbolConfigProviderProperty) as SymbolConfigProvider; }
            set { SetValue(SymbolConfigProviderProperty, value); }
        }

        /// <summary>
        /// Identifies the SymbolConfigProvider dependency property.
        /// </summary>
        public static readonly DependencyProperty SymbolConfigProviderProperty =
            DependencyProperty.Register(
                "SymbolConfigProvider",
                typeof(SymbolConfigProvider),
                typeof(SwitchRendererControl),
                new PropertyMetadata(null));
        #endregion 

        internal void UpdateLayerRenderer(string rendererType)
        {
            if (RendererType == null)
                return;

            _ignoreRendererTypeChangedEvent = true;
            buildDropDowns();                
            _ignoreRendererTypeChangedEvent = false;
        }

        internal void UpdateLayerRendererAttribute(FieldInfo attributeField)
        {
            if (AttributeName == null)
                return;

            _ignoreRendererTypeChangedEvent = true;
            _ignoreAttributeChangedEvent = true;
            buildDropDowns();
            _ignoreAttributeChangedEvent = false;
            _ignoreRendererTypeChangedEvent = false;
           
            //FieldInfo selectedField = null;            
            //foreach (FieldInfo fieldInfo in AttributeName.Items)
            //{
            //    if (string.Compare(fieldInfo.Name, attributeName) == 0)
            //    {
            //        selectedField = fieldInfo;
            //        break;
            //    }
            //}
            //if (selectedField != null)
            //{
            //    _ignoreAttributeChangedEvent = true;
            //    AttributeName.SelectedItem = selectedField;
            //    _ignoreAttributeChangedEvent = false;
            //}
        }

        protected virtual void OnLayerRendererChanged(LayerRendererChangedEventArgs args)
        {
            if(LayerRendererChanged != null)
                LayerRendererChanged(this, args);
        }

        protected virtual void OnLayerRendererAttributeChanged(LayerRendererAttributeChangedEventArgs args)
        {
            if (LayerRendererAttributeChanged != null)
                LayerRendererAttributeChanged(this, args);
        }

        public event EventHandler<LayerRendererChangedEventArgs> LayerRendererChanged;
        public event EventHandler<LayerRendererAttributeChangedEventArgs> LayerRendererAttributeChanged;
    }

    public class LayerRendererChangedEventArgs : EventArgs
    {        
        public string RendererType { get; set; }
        public FieldInfo RendererAttributeField { get; set; }
    }

    public class LayerRendererAttributeChangedEventArgs : EventArgs
    {
        public FieldInfo RendererAttributeField { get; set; }
    }
}
