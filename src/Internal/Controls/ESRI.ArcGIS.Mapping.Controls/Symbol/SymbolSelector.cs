/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Core.Symbols;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Client.Toolkit.Primitives;
using System.Windows.Media;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name="SymbolCategories", Type=typeof(ComboBox))]
    [TemplatePart(Name="Symbols", Type=typeof(ListBox))]
    [TemplatePart(Name="SymbolsScrollViewer", Type=typeof(ScrollViewer))]
    [TemplatePart(Name="LayoutRoot", Type=typeof(FrameworkElement))]
    public class SymbolSelector : Control
    {
        private const string PART_SYMBOLCATEGORIES = "SymbolCategories";
        private const string PART_SYMBOLS = "Symbols";
        private const string PART_SYMBOLSSCROLLVIEWER = "SymbolsScrollViewer";
        private const string PART_LAYOUTROOT = "LayoutRoot";

        internal ComboBox SymbolCategories;
        internal ListBox Symbols;
		internal ScrollViewer SymbolsScrollViewer;
		internal FrameworkElement LayoutRoot;
		internal WrapPanel SymbolsWrapPanel;
		internal bool isInitialized = false;

        public SymbolSelector()
        { 
            DefaultStyleKey = typeof(SymbolSelector);
        }

        public override void OnApplyTemplate()
        {
            if (SymbolCategories != null)
                SymbolCategories.SelectionChanged -= SymbolCategories_SelectionChanged;

            if (Symbols != null)
                Symbols.SelectionChanged -= Symbols_SelectionChanged;

            base.OnApplyTemplate();
            
            SymbolCategories = GetTemplateChild(PART_SYMBOLCATEGORIES) as ComboBox;
            
            if(SymbolCategories != null)
                SymbolCategories.SelectionChanged += SymbolCategories_SelectionChanged;

            Symbols = GetTemplateChild(PART_SYMBOLS) as ListBox;

            if (Symbols != null)
                Symbols.SelectionChanged += Symbols_SelectionChanged;                

            SymbolsScrollViewer = GetTemplateChild(PART_SYMBOLSSCROLLVIEWER) as ScrollViewer;

            LayoutRoot = GetTemplateChild(PART_LAYOUTROOT) as FrameworkElement;

            getSymbolCategories();

            if (Symbols != null)
            {
                // Size width of symbols wrap panel to fit within parent ListBox
                Dispatcher.BeginInvoke((Action)delegate()
                {
                    SymbolsWrapPanel = ControlTreeHelper.FindChildOfType<WrapPanel>(Symbols, 5);
                    if (SymbolsWrapPanel != null && double.IsNaN(SymbolsWrapPanel.Width) && !double.IsNaN(Symbols.ActualWidth) && Symbols.ActualWidth > 0)
                    {
                        SymbolsWrapPanel.Width = SymbolsScrollViewer.ViewportWidth - 2;
                        Symbols.SizeChanged -= Symbols_SizeChanged;
                        Symbols.SizeChanged += Symbols_SizeChanged;
                    }
                });
            }
			isInitialized = true;

            if (InitCompleted != null)
                InitCompleted(this, EventArgs.Empty);
        }

        internal event EventHandler InitCompleted;

        void Symbols_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Symbols.SizeChanged -= Symbols_SizeChanged;
            SymbolsWrapPanel.Width = SymbolsScrollViewer.ViewportWidth - 2;
        }

        void Symbols_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Symbols == null)
                return;

            Grid symbolGrid = Symbols.SelectedItem as Grid;
            if (symbolGrid == null || symbolGrid.Children.Count == 0)
                return;

            SymbolDisplay symbolDisplay = symbolGrid.Children[0] as SymbolDisplay;
            if (symbolDisplay == null)
                return;

            OnSymbolSelected(new SymbolSelectedEventArgs() { Symbol = (symbolDisplay.Symbol == null) ? null : symbolDisplay.Symbol.CloneSymbol() });
        }

        void getSymbolCategories()
        {
            if (SymbolConfigProvider == null)
                return;

            SymbolConfigProvider.GetSymbolResourceDictionaryEntriesFailed -= SymbolConfigProvider_GetSymbolCategoriesFailed;
            SymbolConfigProvider.GetSymbolResourceDictionaryEntriesFailed += SymbolConfigProvider_GetSymbolCategoriesFailed;

            SymbolConfigProvider.GetSymbolCategoriesCompleted -= SymbolConfigProvider_GetSymbolCategoriesCompleted;
            SymbolConfigProvider.GetSymbolCategoriesCompleted += SymbolConfigProvider_GetSymbolCategoriesCompleted;            

            SymbolConfigProvider.GetSymbolResourceDictionaryEntriesAsync(GeometryType, null);
        }

        void SymbolConfigProvider_GetSymbolCategoriesFailed(object sender, ESRI.ArcGIS.Mapping.Core.ExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                MessageBoxDialog.Show(e.Exception.Message);
                Logger.Instance.LogError(e.Exception);
            }
        }

        void SymbolConfigProvider_GetSymbolCategoriesCompleted(object sender, GetSymbolResourceDictionaryEntriesCompletedEventArgs e)
        {
            if (e.SymbolResourceDictionaries == null)
                return;

            if (SymbolCategories != null)
            {
                SymbolCategories.ItemsSource = e.SymbolResourceDictionaries;
                if (e.SymbolResourceDictionaries.Count() > 0)
                {
                    SymbolCategories.SelectedIndex = 0;
                }
            }            
        }
        
        void SymbolCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SymbolResourceDictionaryEntry resourceDictionary = SymbolCategories.SelectedItem as SymbolResourceDictionaryEntry;
            if (resourceDictionary == null)
                return;

            if (SymbolConfigProvider == null)
                return;

            SymbolConfigProvider.GetSymbolsForResourceDictionaryFailed -= SymbolConfigProvider_GetSymbolsForCategoryFailed;
            SymbolConfigProvider.GetSymbolsForResourceDictionaryFailed += SymbolConfigProvider_GetSymbolsForCategoryFailed;

            SymbolConfigProvider.GetSymbolsForResourceDictionaryCompleted -= SymbolConfigProvider_GetSymbolForResourceDictionary;
            SymbolConfigProvider.GetSymbolsForResourceDictionaryCompleted +=SymbolConfigProvider_GetSymbolForResourceDictionary;

            SymbolConfigProvider.GetSymbolsForResourceDictionaryAsync(resourceDictionary, null);
        }

        void SymbolConfigProvider_GetSymbolForResourceDictionary(object sender, GetSymbolsForResourceDictionaryCompletedEventArgs e)
        {
            if (e.Symbols == null)
                return;            

            if (Symbols != null)
            {
                try
                {
                    Symbols.Items.Clear();
                    foreach (SymbolDescription symbolDescription in e.Symbols)
                    {
                        if (symbolDescription == null || symbolDescription.Symbol == null)
                            continue;
                        Symbol symbol = symbolDescription.Symbol;
                        double size = 50;
                        ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol markerSymbol = symbol as ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol;
                        if (markerSymbol != null)
                        {
                            if (!double.IsNaN(markerSymbol.Size))
                                size = markerSymbol.Size;
                        }
                        else if (symbol is FillSymbol || symbol is LineSymbol)
                            size = 25;
                        else if (symbol is ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol )
                        {
                            ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol simpleMarkerSymbol =
                                symbol as ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol;
                            if (simpleMarkerSymbol != null && !double.IsNaN(simpleMarkerSymbol.Size))
                                size = simpleMarkerSymbol.Size;
                        }
                        else if (symbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleMarkerSymbol)
                        {
                            ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleMarkerSymbol sms =
                                symbol as ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleMarkerSymbol;
                            if (sms != null && !double.IsNaN(sms.Size))
                                size = sms.Size;
                        }

                        SymbolDisplay disp = new SymbolDisplay()
                        {
                            Symbol = symbolDescription.Symbol,                            
                            Width = size,
                            Height = size,
                            IsHitTestVisible = false // Set to false to prevent mouseover and selection effects
                        };

                        // Wrap symbol display in a grid to allow cursor and tooltip
                        Grid symbolGrid = new Grid() 
                            { 
                                Cursor = Cursors.Hand,
                                // Apply nearly transparent background so grid is hit-test visible
                                Background = new SolidColorBrush(Color.FromArgb(1, 255, 255, 255))
                            };
                        symbolGrid.Children.Add(disp);
                       
                        Symbols.Items.Add(symbolGrid);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogError(ex);
					MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ErrorRetrievingSymbols + Environment.NewLine + ex.Message);
                }
            }
        }


        protected override Size ArrangeOverride(Size finalSize)
        {
            if (LayoutRoot is Panel)
            {
            // Set height of SymbolsScrollViewer.  Assumes that all elements aside from the ScrollViewer have an explicitly declared height.
                if (SymbolsScrollViewer != null && double.IsNaN(SymbolsScrollViewer.Height) && LayoutRoot != null && !double.IsNaN(Height))
                    SymbolsScrollViewer.Height = Height - LayoutRoot.DesiredSize.Height;
            }
            else if (LayoutRoot is Border)
            {
                Border border = (Border)LayoutRoot;
                UIElement child = border.Child;
                if (SymbolsScrollViewer != null && double.IsNaN(SymbolsScrollViewer.Height) && child != null && !double.IsNaN(Height))
                    SymbolsScrollViewer.Height = Height - child.DesiredSize.Height - border.Padding.Top - border.Padding.Bottom;
            }
            return base.ArrangeOverride(finalSize);
        }

        void SymbolConfigProvider_GetSymbolsForCategoryFailed(object sender, ESRI.ArcGIS.Mapping.Core.ExceptionEventArgs e)
        {
            Logger.Instance.LogError(e.Exception);
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
                typeof(SymbolSelector),
                new PropertyMetadata(null));
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
                typeof(SymbolSelector),
                new PropertyMetadata(GeometryType.Point, OnGeometryTypePropertyChanged));

        /// <summary>
        /// GeometryTypeProperty property changed handler.
        /// </summary>
        /// <param name="d">SymbolSelector that changed its GeometryType.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnGeometryTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SymbolSelector source = d as SymbolSelector;
            source.OnGeometryTypeChanged();
        }
        #endregion

        #region Symbol
        /// <summary>
        /// 
        /// </summary>
        public Symbol Symbol
        {
            get { return GetValue(SymbolProperty) as Symbol; }
            set { SetValue(SymbolProperty, value); }
        }

        /// <summary>
        /// Identifies the Symbol dependency property.
        /// </summary>
        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register(
                "Symbol",
                typeof(Symbol),
                typeof(SymbolSelector),
                new PropertyMetadata(null, OnSymbolPropertyChanged));

        /// <summary>
        /// SymbolProperty property changed handler.
        /// </summary>
        /// <param name="d">SymbolSelector that changed its Symbol.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnSymbolPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SymbolSelector source = d as SymbolSelector;
            source.OnSymbolChanged();
        }
        #endregion        


        private void OnSymbolChanged()
        {

        }

        private void OnGeometryTypeChanged()
        {
            // Re-get the symbol categories
            getSymbolCategories();
        }

        protected void OnSymbolSelected(SymbolSelectedEventArgs args)
        {
            if (SymbolSelected != null)
                SymbolSelected(this, args);
        }

        public event EventHandler<SymbolSelectedEventArgs> SymbolSelected;
    }

    public class SymbolSelectedEventArgs : EventArgs
    {
        public Symbol Symbol { get; set; }
        public bool LayerSymbolUpdated { get; set; }
    }
}
