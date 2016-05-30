/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;
using System.Windows.Data;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class LayerSymbologyConfigControl : LayerConfigControl
    {
        internal SwitchRendererControl SwitchRendererControl;
        internal Border RendererOptionsContainerBorder;
        internal ContentControl RendererOptionsContainerControl;
        internal ContentControl RendererSymbolSetContainerControl;
        internal SymbolConfigControl SymbolConfigControl;
        internal TextBlock CurrentSymbolLabel;
        internal CheckBox GraphicSymbolsTakePrecedence;

        private const string PART_SWITCHRENDERERCONTROL = "SwitchRendererControl";
        private const string PART_RENDEREROPTIONSCONTAINERBORDER = "RendererOptionsContainerBorder";
        private const string PART_RENDEREROPTIONSCONTAINERCONTROL = "RendererOptionsContainerControl";
        private const string PART_RENDERERSYMBOLSETCONTAINERCONTROL = "RendererSymbolSetContainerControl";
        private const string PART_SYMBOLCONFIGCONTROL = "SymbolConfigControl";
        private const string PART_CURRENTSYMBOLLABEL = "CurrentSymbolLabel";
        private const string PART_GraphicSymbolsTakePrecedence = "GraphicSymbolsTakePrecedence";

        public LayerSymbologyConfigControl()
        {
            DefaultStyleKey = typeof(LayerSymbologyConfigControl);
        }

        public override void OnApplyTemplate()
        {            
            if (SwitchRendererControl != null)
            {
                SwitchRendererControl.LayerRendererChanged -= SwitchRendererControl_LayerRendererChanged;
                SwitchRendererControl.LayerRendererAttributeChanged -= SwitchRendererControl_LayerRendererAttributeChanged;
            }

            if (SymbolConfigControl != null)
                SymbolConfigControl.SymbolModified -= SymbolConfigControl_SymbolModified;
            if (GraphicSymbolsTakePrecedence != null)
            {
                GraphicSymbolsTakePrecedence.Checked -= GraphicSymbolsTakePrecedence_Checked;
                GraphicSymbolsTakePrecedence.Unchecked -= GraphicSymbolsTakePrecedence_Checked;
            }

            base.OnApplyTemplate();

            SwitchRendererControl = GetTemplateChild(PART_SWITCHRENDERERCONTROL) as SwitchRendererControl;
            if (SwitchRendererControl != null)
            {
                SwitchRendererControl.LayerRendererChanged += SwitchRendererControl_LayerRendererChanged;
                SwitchRendererControl.LayerRendererAttributeChanged += SwitchRendererControl_LayerRendererAttributeChanged;
            }

            RendererOptionsContainerBorder = GetTemplateChild(PART_RENDEREROPTIONSCONTAINERBORDER) as Border;

            RendererOptionsContainerControl = GetTemplateChild(PART_RENDEREROPTIONSCONTAINERCONTROL) as ContentControl;

            RendererSymbolSetContainerControl = GetTemplateChild(PART_RENDERERSYMBOLSETCONTAINERCONTROL) as ContentControl;

            SymbolConfigControl = GetTemplateChild(PART_SYMBOLCONFIGCONTROL) as SymbolConfigControl;
            if(SymbolConfigControl != null)
                SymbolConfigControl.SymbolModified += SymbolConfigControl_SymbolModified;

            CurrentSymbolLabel = GetTemplateChild(PART_CURRENTSYMBOLLABEL) as TextBlock;

            GraphicSymbolsTakePrecedence = GetTemplateChild(PART_GraphicSymbolsTakePrecedence) as CheckBox;
            if (GraphicSymbolsTakePrecedence != null)
            {
                GraphicSymbolsTakePrecedence.Checked += GraphicSymbolsTakePrecedence_Checked;
                GraphicSymbolsTakePrecedence.Unchecked += GraphicSymbolsTakePrecedence_Checked;
            }

            bindUIToLayer();

            if (InitCompleted != null)
                InitCompleted(this, EventArgs.Empty);
        }

        bool bindingToLayer = false;
        void GraphicSymbolsTakePrecedence_Checked(object sender, RoutedEventArgs e)
        {
            if (bindingToLayer)
                return;
            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer != null && GraphicSymbolsTakePrecedence != null && GraphicSymbolsTakePrecedence.IsChecked.HasValue)
            {
                graphicsLayer.RendererTakesPrecedence = !(GraphicSymbolsTakePrecedence.IsChecked.Value);
                graphicsLayer.Refresh();
            }
        }

        internal event EventHandler InitCompleted;

        protected override void OnLayerChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            bindUIToLayer();
        }

        private void bindUIToLayer()
        {
            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
                return;

            #region Renderer Takes Precedence
            bindingToLayer = true;
            if (GraphicSymbolsTakePrecedence != null)
                GraphicSymbolsTakePrecedence.IsChecked = !graphicsLayer.RendererTakesPrecedence;
            bindingToLayer = false;
            #endregion

            GeometryType = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetGeometryType(graphicsLayer);

            ClassBreaksRenderer classBreaksRenderer = graphicsLayer.Renderer as ClassBreaksRenderer;
            if (classBreaksRenderer != null)
            {
                if (RendererOptionsContainerControl != null)
                {
                    ClassBreaksRendererOptionsConfigControl optionsConfigControl = new ClassBreaksRendererOptionsConfigControl()
                    {
                        ClassBreaksRenderer = classBreaksRenderer,
                        SymbolConfigProvider = SymbolConfigProvider,
                        GeometryType = GeometryType,
                    };

                    Binding b = new Binding("Foreground") { Source = this };
                    optionsConfigControl.SetBinding(ClassBreaksRendererOptionsConfigControl.ForegroundProperty, b);

                    optionsConfigControl.RendererColorSchemeChanged += new EventHandler<GradientBrushChangedEventArgs>(optionsConfigControl_RendererColorSchemeChanged);
                    optionsConfigControl.RendererClassBreaksChanged += new EventHandler<RendererClassBreaksCountChangedEventArgs>(optionsConfigControl_RendererClassBreaksChanged);
                    RendererOptionsContainerControl.Content = optionsConfigControl;
                }

                if (RendererOptionsContainerBorder != null)
                    RendererOptionsContainerBorder.Visibility = Visibility.Visible;

                if (CurrentSymbolLabel != null)
                    CurrentSymbolLabel.Visibility = Visibility.Visible;

                createClassBreaksRendererSymbolsConfigControl(classBreaksRenderer);
            }
            else
            {
                UniqueValueRenderer uniqueValueRenderer = graphicsLayer.Renderer as UniqueValueRenderer;
                if (uniqueValueRenderer != null)
                {
                    if (RendererOptionsContainerControl != null)
                    {
                        UniqueValueRendererOptionsConfigControl optionsConfigControl = new UniqueValueRendererOptionsConfigControl()
                        {
                            UniqueValueRenderer = uniqueValueRenderer,
                            SymbolConfigProvider = SymbolConfigProvider,
                            GeometryType = GeometryType,
                        };

                        Binding b = new Binding("Foreground") { Source = this };
                        optionsConfigControl.SetBinding(UniqueValueRendererOptionsConfigControl.ForegroundProperty, b);

                        optionsConfigControl.RendererColorSchemeChanged += new EventHandler<GradientBrushChangedEventArgs>(optionsConfigControl_RendererColorSchemeChanged);
                        optionsConfigControl.NewUniqueValueAdded += new EventHandler<NewUniqueValueInfoEventArgs>(optionsConfigControl_NewUniqueValueCreated);
                        optionsConfigControl.DeleteUniqueValueClicked += new EventHandler(optionsConfigControl_DeleteUniqueValueClicked);
                        RendererOptionsContainerControl.Content = optionsConfigControl;
                    }

                    if (RendererSymbolSetContainerControl != null)
                    {
                        UniqueValueRendererSymbolsConfigControl symbolsConfigControl = new UniqueValueRendererSymbolsConfigControl()
                        {
                            UniqueValueRenderer = uniqueValueRenderer,
                            SymbolConfigProvider = SymbolConfigProvider,
                            GeometryType = GeometryType,
                        };

                        Binding b = new Binding("Foreground") { Source = this };
                        symbolsConfigControl.SetBinding(UniqueValueRendererSymbolsConfigControl.ForegroundProperty, b);

                        symbolsConfigControl.UniqueValueRendererModified += new EventHandler<SelectedUniqueValueModificationEventArgs>(symbolsConfigControl_UniqueValueRendererModified);
                        symbolsConfigControl.CurrentUniqueValueChanged += new EventHandler<CurrentUniqueValueChangedEventArgs>(symbolsConfigControl_CurrentUniqueValueChanged);
                        symbolsConfigControl.DefaultClassBreakBeingConfigured += new EventHandler<DefaultClassBreakBeingConfiguredEventArgs>(symbolsConfigControl_DefaultClassBreakBeingConfigured);
                        RendererSymbolSetContainerControl.Content = symbolsConfigControl;
                    }

                    if (RendererOptionsContainerBorder != null)
                        RendererOptionsContainerBorder.Visibility = Visibility.Visible;

                    if (CurrentSymbolLabel != null)
                        CurrentSymbolLabel.Visibility = Visibility.Visible;
                }
                else
                {
                    // No renderer / simple renderer ... clear out the control
                    if (RendererOptionsContainerControl != null)
                        RendererOptionsContainerControl.Content = null;

                    if (RendererSymbolSetContainerControl != null)
                    {
                        ESRI.ArcGIS.Client.Symbols.Symbol defaultSymbol = graphicsLayer.GetDefaultSymbol();
                        DefaultSymbolConfigControl defaultSymbolConfig = new DefaultSymbolConfigControl() { 
                             Symbol = defaultSymbol,
                             SymbolConfigProvider = SymbolConfigProvider,
                             GeometryType = GeometryType,
                        };

                        Binding b = new Binding("Foreground") { Source = this };
                        defaultSymbolConfig.SetBinding(DefaultSymbolConfigControl.ForegroundProperty, b);

                        defaultSymbolConfig.DefaultSymbolModified += new EventHandler<SymbolSelectedEventArgs>(defaultSymbolConfig_DefaultSymbolModified);
                        RendererSymbolSetContainerControl.Content = defaultSymbolConfig;
                        if (SymbolConfigControl != null)
                            SymbolConfigControl.Symbol = defaultSymbol;
                    }

                    if (CurrentSymbolLabel != null)
                        CurrentSymbolLabel.Visibility = Visibility.Collapsed;

                    if (RendererOptionsContainerBorder != null)
                        RendererOptionsContainerBorder.Visibility = Visibility.Collapsed;
                }
            }
        }

        void optionsConfigControl_DeleteUniqueValueClicked(object sender, EventArgs e)
        {
            if (RendererSymbolSetContainerControl != null)
            {
                UniqueValueRendererSymbolsConfigControl symbolsConfigControl = RendererSymbolSetContainerControl.Content as UniqueValueRendererSymbolsConfigControl;
                if (symbolsConfigControl != null)
                {
                    symbolsConfigControl.DeleteCurrentSelectedUniqueValue();
                }
            }
        }

        void SymbolConfigControl_SymbolModified(object sender, SymbolSelectedEventArgs e)
        {
            // Some changes to ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleSymbols do not trigger the legendchanged event. Fix this by triggering the event by cloning the symbol.
            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer != null && graphicsLayer.Renderer is ILegendSupport)
            {
                SimpleRenderer simpleRenderer = graphicsLayer.Renderer as SimpleRenderer;
                if (simpleRenderer != null && (e.Symbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleMarkerSymbol || e.Symbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleLineSymbol || e.Symbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleFillSymbol))
                    simpleRenderer.Symbol = e.Symbol.CloneSymbol();
                else
                {
                    ClassBreaksRenderer classBreaksRenderer = graphicsLayer.Renderer as ClassBreaksRenderer;
                    if (classBreaksRenderer != null && (e.Symbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleMarkerSymbol || e.Symbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleLineSymbol || e.Symbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleFillSymbol))
                    {
                        if (_classBreakInfo != null)
                            _classBreakInfo.Symbol = e.Symbol.CloneSymbol();
                        else
                            classBreaksRenderer.DefaultSymbol = e.Symbol.CloneSymbol();
                    }
                    else
                    {
                        UniqueValueRenderer uniqueValueRenderer = graphicsLayer.Renderer as UniqueValueRenderer;
                        if (uniqueValueRenderer != null && (e.Symbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleMarkerSymbol || e.Symbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleLineSymbol || e.Symbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleFillSymbol))
                            if (_uniqueValueInfo != null)
                                _uniqueValueInfo.Symbol = e.Symbol.CloneSymbol();
                            else
                                uniqueValueRenderer.DefaultSymbol = e.Symbol.CloneSymbol();
                    }
                }
            }
        }

        UniqueValueInfo _uniqueValueInfo;
        void symbolsConfigControl_CurrentUniqueValueChanged(object sender, CurrentUniqueValueChangedEventArgs e)
        {
            _uniqueValueInfo = e.UniqueValue;

            if (e.UniqueValue == null)
                return;            

            if (SymbolConfigControl != null)
                SymbolConfigControl.Symbol = e.UniqueValue.Symbol;
        }

        void optionsConfigControl_NewUniqueValueCreated(object sender, NewUniqueValueInfoEventArgs e)
        {
            if (RendererSymbolSetContainerControl != null)
            {
                UniqueValueRendererSymbolsConfigControl symbolsConfigControl = RendererSymbolSetContainerControl.Content as UniqueValueRendererSymbolsConfigControl;
                if (symbolsConfigControl != null)
                {
                    FieldType fieldType = FieldType.Text;
                    GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
                    if (graphicsLayer == null)
                        return;

                    UniqueValueRenderer uniqueValueRenderer = graphicsLayer.Renderer as UniqueValueRenderer;
                    if(uniqueValueRenderer == null)
                        return;

                    Collection<FieldInfo> fields = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetFields(graphicsLayer);
                    if (fields != null)
                    {
                        FieldInfo selectedField = fields.FirstOrDefault<FieldInfo>(f => f.Name == uniqueValueRenderer.Field);
                        if (selectedField != null)
                        {
                            fieldType = selectedField.FieldType;
                            symbolsConfigControl.AddNewUniqueValue(createNewUniqueValue(e.UniqueValue, fieldType), fieldType);
                            graphicsLayer.Refresh();
                        }
                    }                    
                }
            }
        }

        object createNewUniqueValue(string stringValue, FieldType fieldType)
        {
            switch (fieldType)
            {
                case FieldType.Text:
                    return stringValue;
                case FieldType.Attachment:
                    return new AttachmentFieldValue() { DisplayText = stringValue, LinkUrl = stringValue };
                case FieldType.Boolean:
                    bool b;
                    if (bool.TryParse(stringValue, out b))
                        return b;
                    break;
                case FieldType.Currency:
                    double d;
                    if (double.TryParse(stringValue, System.Globalization.NumberStyles.Currency, CultureHelper.GetCurrentCulture(), out d))
                        return new CurrencyFieldValue() { Value = d } ;
                    break;
                case FieldType.DateTime:
                    DateTime dt;
                    if (DateTime.TryParse(stringValue, CultureHelper.GetCurrentCulture(), System.Globalization.DateTimeStyles.None, out dt))
                        return new DateTimeFieldValue() { Value = dt };
                    break;
                case FieldType.DecimalNumber:
                    double dn;
                    if (double.TryParse(stringValue, System.Globalization.NumberStyles.Any, CultureHelper.GetCurrentCulture(), out dn))
                        return dn;
                    break;
                case FieldType.Entity:
                    return new EntityFieldValue() { 
                         LinkUrl = stringValue, DisplayText = stringValue,
                    };
                case FieldType.Hyperlink:
                    return new HyperlinkFieldValue()
                    {
                        LinkUrl = stringValue,
                        DisplayText = stringValue,
                    };
                case FieldType.Image:
                    return new HyperlinkImageFieldValue()
                    {
                        ImageTooltip = stringValue,
                        ImageUrl = stringValue,
                    };                    
                case FieldType.Integer:
                    int i;
                    if(int.TryParse(stringValue, out i))
                        return i;
                    break;    
                case FieldType.Lookup:
                    return new LookupFieldValue() { 
                         DisplayText = stringValue,
                         LinkUrl = stringValue,
                    };                    
            }
            return stringValue;
        }

        private void createClassBreaksRendererSymbolsConfigControl(ClassBreaksRenderer classBreaksRenderer)
        {
            if (RendererSymbolSetContainerControl != null)
            {
                ClassBreaksRendererSymbolsConfigControl symbolsConfigControl = new ClassBreaksRendererSymbolsConfigControl()
                {
                    ClassBreaksRenderer = classBreaksRenderer,
                    SymbolConfigProvider = SymbolConfigProvider,
                    GeometryType = GeometryType,
                };

                Binding b = new Binding("Foreground") { Source = this };
                symbolsConfigControl.SetBinding(ClassBreaksRendererSymbolsConfigControl.ForegroundProperty, b);

                symbolsConfigControl.ClassBreakRendererModified += new EventHandler<SelectedClassBreakModificationEventArgs>(symbolsConfigControl_ClassBreakRendererModified);
                symbolsConfigControl.CurrentClassBreakChanged += new EventHandler<CurrentClassBreakChangedEventArgs>(symbolsConfigControl_CurrentClassBreakChanged);
                symbolsConfigControl.DefaultClassBreakBeingConfigured += new EventHandler<DefaultClassBreakBeingConfiguredEventArgs>(symbolsConfigControl_DefaultClassBreakBeingConfigured);
                RendererSymbolSetContainerControl.Content = symbolsConfigControl;
            }
        }

        void symbolsConfigControl_DefaultClassBreakBeingConfigured(object sender, DefaultClassBreakBeingConfiguredEventArgs e)
        {
            _classBreakInfo = null;

            if (e.DefaultSymbol == null)
                return;

            if (SymbolConfigControl != null)
            {                
                SymbolConfigControl.Symbol = e.DefaultSymbol;
            }
        }

        ClassBreakInfo _classBreakInfo = null;
        void symbolsConfigControl_CurrentClassBreakChanged(object sender, CurrentClassBreakChangedEventArgs e)
        {
            _classBreakInfo = e.ClassBreak;

            if (e.ClassBreak == null)
                return;

            if (SymbolConfigControl != null)
                SymbolConfigControl.Symbol = e.ClassBreak.Symbol;
        }

        void optionsConfigControl_RendererClassBreaksChanged(object sender, RendererClassBreaksCountChangedEventArgs e)
        {
            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
                return;

            graphicsLayer.RegenerateClassBreaksOnCountChanged((int)e.Value);
            refreshLayer();

            ClassBreaksRendererOptionsConfigControl rendererOptions = sender as ClassBreaksRendererOptionsConfigControl;
            if (rendererOptions != null) // refresh the renderer
                rendererOptions.ClassBreaksRenderer = graphicsLayer.Renderer as ClassBreaksRenderer;

            createClassBreaksRendererSymbolsConfigControl(graphicsLayer.Renderer as ClassBreaksRenderer);
        }

        void defaultSymbolConfig_DefaultSymbolModified(object sender, SymbolSelectedEventArgs e)
        {
            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
                return;

            graphicsLayer.ChangeRenderer(e.Symbol);
            refreshLayer();
            if (SymbolConfigControl != null)
                SymbolConfigControl.Symbol = e.Symbol;
        }

        void symbolsConfigControl_UniqueValueRendererModified(object sender, SelectedUniqueValueModificationEventArgs e)
        {
            if (e.UniqueValueModificationType == UniqueValueModificationType.SymbolChanged)
            {
                if (e.IsSelectedItem)
                {
                    // we need to update the symbol config control, if this is the current selected control
                    updateSymbolConfigControlIfCurrentSelectedSymbol();
                }
            }
            refreshLayer();
        }

        public void RepositionPopups()
        {            
            if (SymbolConfigControl != null)
                SymbolConfigControl.RepositionPopups();
        }

        public void CloseAllPopups()
        {
            if (SymbolConfigControl != null)
                SymbolConfigControl.CloseAllPopups();
        }

        void symbolsConfigControl_ClassBreakRendererModified(object sender, SelectedClassBreakModificationEventArgs e)
        {
            if (e.ClassBreakModificationType == ClassBreakModificationType.SymbolChanged)
            {
                if (e.IsSelectedItem)
                {
                    // we need to update the symbol config control, if this is the current selected control
                    updateSymbolConfigControlIfCurrentSelectedSymbol();
                }                
            }
            refreshLayer();
        }

        private void updateSymbolConfigControlIfCurrentSelectedSymbol()
        {
            if (SymbolConfigControl == null || RendererSymbolSetContainerControl == null)
                return;

            ClassBreaksRendererSymbolsConfigControl symbolsConfigControl = RendererSymbolSetContainerControl.Content as ClassBreaksRendererSymbolsConfigControl;
            if (symbolsConfigControl != null)
            {
                Control selectedControl = symbolsConfigControl.GetCurrentSelectedConfigControl();
                ClassBreakConfigControl classBreaksConfigControl = selectedControl as ClassBreakConfigControl;
                if (classBreaksConfigControl != null)
                {
                    if (classBreaksConfigControl.ClassBreak != null)
                        SymbolConfigControl.Symbol = classBreaksConfigControl.ClassBreak.Symbol;
                }
                else
                {
                    DefaultSymbolConfigControl defaultConfigControl = selectedControl as DefaultSymbolConfigControl;
                    if (defaultConfigControl != null)
                        SymbolConfigControl.Symbol = defaultConfigControl.Symbol;
                }
            }
            else
            {
                UniqueValueRendererSymbolsConfigControl uniqueValueRendererConfigControl = RendererSymbolSetContainerControl.Content as UniqueValueRendererSymbolsConfigControl;
                if (uniqueValueRendererConfigControl != null)
                {
                    Control selectedControl = uniqueValueRendererConfigControl.GetCurrentSelectedConfigControl();
                    UniqueValueConfigControl uniqueValueConfigControl = selectedControl as UniqueValueConfigControl;
                    if (uniqueValueConfigControl != null)
                    {
                        if (uniqueValueConfigControl.UniqueValue != null)
                            SymbolConfigControl.Symbol = uniqueValueConfigControl.UniqueValue.Symbol;
                    }
                    else
                    {
                        DefaultSymbolConfigControl defaultConfigControl = selectedControl as DefaultSymbolConfigControl;
                        if (defaultConfigControl != null)
                            SymbolConfigControl.Symbol = defaultConfigControl.Symbol;
                    }
                }
            }
        }

        void refreshLayer()
        {
            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer != null)
                graphicsLayer.Refresh();
        }

        void optionsConfigControl_RendererColorSchemeChanged(object sender, GradientBrushChangedEventArgs e)
        {
            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
                return;

            graphicsLayer.ApplyColorRampGradientBrushToRenderer(e.GradientBrush);

            ClassBreaksRendererOptionsConfigControl rendererOptions = sender as ClassBreaksRendererOptionsConfigControl;
            if (rendererOptions != null) // refresh the renderer on the config control
                rendererOptions.ClassBreaksRenderer = graphicsLayer.Renderer as ClassBreaksRenderer;
            else
            {
                UniqueValueRendererOptionsConfigControl uniqueValueRendererOptions = sender as UniqueValueRendererOptionsConfigControl;
                if (uniqueValueRendererOptions != null)
                    uniqueValueRendererOptions.UniqueValueRenderer = graphicsLayer.Renderer as UniqueValueRenderer;
            }

            if (RendererSymbolSetContainerControl != null)
            {
                ClassBreaksRendererSymbolsConfigControl rendererSymbols = RendererSymbolSetContainerControl.Content as ClassBreaksRendererSymbolsConfigControl;
                if (rendererSymbols != null)
                {                    
                    rendererSymbols.RefreshSymbols();                 
                }
                else
                {
                    UniqueValueRendererSymbolsConfigControl uniqueSymbols = RendererSymbolSetContainerControl.Content as UniqueValueRendererSymbolsConfigControl;
                    if (uniqueSymbols != null)
                        uniqueSymbols.RefreshSymbols();
                }
            }
        }

        void SwitchRendererControl_LayerRendererAttributeChanged(object sender, LayerRendererAttributeChangedEventArgs e)
        {
            bindUIToLayer();

            OnLayerRendererAttributeChanged(e);
        }

        void SwitchRendererControl_LayerRendererChanged(object sender, LayerRendererChangedEventArgs e)
        {
            bindUIToLayer();

            OnLayerRendererChanged(e);
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
                typeof(LayerSymbologyConfigControl),
                new PropertyMetadata(OnSymbolConfigProviderChange));

        public static void OnSymbolConfigProviderChange(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            LayerSymbologyConfigControl control = o as LayerSymbologyConfigControl;
            if (control != null)
                control.bindUIToLayer();
        }
        #endregion

        #region GeometryType
        /// <summary>
        /// 
        /// </summary>
        public GeometryType GeometryType
        {
            get { return (GeometryType)GetValue(GeometryTypeProperty); }
            set { SetValue(GeometryTypeProperty, value); }
        }

        /// <summary>
        /// Identifies the GeometryType dependency property.
        /// </summary>
        public static readonly DependencyProperty GeometryTypeProperty =
            DependencyProperty.Register(
                "GeometryType",
                typeof(GeometryType),
                typeof(LayerSymbologyConfigControl),
                new PropertyMetadata(GeometryType.Unknown));
        #endregion

        #region ThemeColors
        /// <summary>
        /// 
        /// </summary>
        public Collection<Color> ThemeColors
        {
            get { return GetValue(ThemeColorsProperty) as Collection<Color>; }
            set { SetValue(ThemeColorsProperty, value); }
        }

        /// <summary>
        /// Identifies the ThemeColors dependency property.
        /// </summary>
        public static readonly DependencyProperty ThemeColorsProperty =
            DependencyProperty.Register(
                "ThemeColors",
                typeof(Collection<Color>),
                typeof(LayerSymbologyConfigControl),
                new PropertyMetadata(null));
        #endregion


        protected virtual void OnSymbologyModified(EventArgs e)
        {
            if (SymbologyModified != null)
                SymbologyModified(this, e);
        }

        protected virtual void OnLayerRendererChanged(LayerRendererChangedEventArgs e)
        {
            if (LayerRendererChanged != null)
                LayerRendererChanged(this, e);
        }

        protected virtual void OnLayerRendererAttributeChanged(LayerRendererAttributeChangedEventArgs e)
        {
            if (LayerRendererAttributeChanged != null)
                LayerRendererAttributeChanged(this, e);
        }

        public event EventHandler SymbologyModified;
        public event EventHandler<LayerRendererChangedEventArgs> LayerRendererChanged;
        public event EventHandler<LayerRendererAttributeChangedEventArgs> LayerRendererAttributeChanged;
    }
}
