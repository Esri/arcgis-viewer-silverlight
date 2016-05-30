/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class UniqueValueRendererSymbolsConfigControl : Control
    {
        internal ListBox UniqueValueConfigItems;
        private const string PART_UNIQUEVALUECONFIGITEMS = "UniqueValueConfigItems";

        public UniqueValueRendererSymbolsConfigControl()
        {
            DefaultStyleKey = typeof(UniqueValueRendererSymbolsConfigControl);
        }

        public override void OnApplyTemplate()
        {
            if (UniqueValueConfigItems != null)
                UniqueValueConfigItems.SelectionChanged -= UniqueValueConfigItems_SelectionChanged;

            base.OnApplyTemplate();

            UniqueValueConfigItems = GetTemplateChild(PART_UNIQUEVALUECONFIGITEMS) as ListBox;
            if (UniqueValueConfigItems != null)
                UniqueValueConfigItems.SelectionChanged += UniqueValueConfigItems_SelectionChanged;

            bindUIToRenderer();

            if (InitCompleted != null)
                InitCompleted(this, EventArgs.Empty);
        }

        internal event EventHandler InitCompleted;

        public void AddNewUniqueValue(object newValue, FieldType fieldType)
        {
            if (UniqueValueRenderer == null)
                return;

            UniqueValueInfo newUniqueValue = new UniqueValueInfoObj()
            {
                SerializedValue = newValue,
                Symbol = UniqueValueRenderer.DefaultSymbol != null ? UniqueValueRenderer.DefaultSymbol.CloneSymbol() : UniqueValueRenderer.GetDefaultSymbolClone(GeometryType),
                FieldType = fieldType,
            };

            UniqueValueConfigControl uniqueValueConfigControl = createNewUniqueValueConfigControl(newUniqueValue);
            UniqueValueRenderer.Infos.Add(newUniqueValue);

            if (UniqueValueConfigItems != null)
            {
                UniqueValueConfigItems.Items.Add(uniqueValueConfigControl);
                UniqueValueConfigItems.SelectedItem = uniqueValueConfigControl;
            }
        }

        private void bindUIToRenderer()
        {
            bindList();
        }

        void UniqueValueConfigItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UniqueValueConfigItems == null)
                return;

            UniqueValueConfigControl configControl = UniqueValueConfigItems.SelectedItem as UniqueValueConfigControl;
            if (configControl != null)
            {
                OnCurrentUniqueValueChanged(new CurrentUniqueValueChangedEventArgs()
                {
                    UniqueValue = configControl.UniqueValue,
                });
                return;
            }
            else
            {
                DefaultSymbolConfigControl defaultConfigControl = UniqueValueConfigItems.SelectedItem as DefaultSymbolConfigControl;
                if (defaultConfigControl != null)
                {
                    OnDefaultClassBreakBeingConfigured(new DefaultClassBreakBeingConfiguredEventArgs()
                    {
                        DefaultSymbol = defaultConfigControl.Symbol,
                    });
                    return;
                }
            }
        }

        #region UniqueValueRenderer
        /// <summary>
        /// 
        /// </summary>
        public UniqueValueRenderer UniqueValueRenderer
        {
            get { return GetValue(UniqueValueRendererProperty) as UniqueValueRenderer; }
            set { SetValue(UniqueValueRendererProperty, value); }
        }

        /// <summary>
        /// Identifies the UniqueValueRenderer dependency property.
        /// </summary>
        public static readonly DependencyProperty UniqueValueRendererProperty =
            DependencyProperty.Register(
                "UniqueValueRenderer",
                typeof(UniqueValueRenderer),
                typeof(UniqueValueRendererSymbolsConfigControl),
                new PropertyMetadata(null));
        #endregion

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
                typeof(UniqueValueRendererSymbolsConfigControl),
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
                typeof(UniqueValueRendererSymbolsConfigControl),
                new PropertyMetadata(GeometryType.Unknown));
        #endregion

        private void bindList()
        {
            if (UniqueValueConfigItems != null)
            {
                UniqueValueConfigItems.Items.Clear();

                if (UniqueValueRenderer != null)
                {                    
                    // Add the default symbol config control
                    DefaultSymbolConfigControl defaultSymbolConfigControl = new DefaultSymbolConfigControl()
                    {
                        Symbol = UniqueValueRenderer.DefaultSymbol,
                        SymbolConfigProvider = SymbolConfigProvider,
                        GeometryType = GeometryType,
                    };
                    defaultSymbolConfigControl.DefaultSymbolModified += new EventHandler<SymbolSelectedEventArgs>(defaultSymbolConfigControl_DefaultSymbolModified);
                    UniqueValueConfigItems.Items.Add(defaultSymbolConfigControl);
                    UniqueValueConfigItems.SelectedItem = defaultSymbolConfigControl;

                    foreach (UniqueValueInfo uniqueValueInfo in UniqueValueRenderer.Infos)
                    {
                        UniqueValueConfigControl uniqueValueConfigControl = createNewUniqueValueConfigControl(uniqueValueInfo);
                        UniqueValueConfigItems.Items.Add(uniqueValueConfigControl);
                    }
                }
            }
        }

        void defaultSymbolConfigControl_DefaultSymbolModified(object sender, SymbolSelectedEventArgs e)
        {
            if (UniqueValueRenderer != null)
                UniqueValueRenderer.DefaultSymbol = e.Symbol;

            bool isSelectedItem = UniqueValueConfigItems.SelectedItem == sender;

            OnUniqueValueRendererModified(new SelectedUniqueValueModificationEventArgs() { 
                UniqueValueModificationType = UniqueValueModificationType.SymbolChanged, 
                UniqueValue = null, 
                IsSelectedItem = isSelectedItem,
            });
        }

        private UniqueValueConfigControl createNewUniqueValueConfigControl(UniqueValueInfo uniqueValueInfo)
        {
            UniqueValueConfigControl uniqueValueConfigControl = new UniqueValueConfigControl()
            {
                UniqueValue = uniqueValueInfo,
                SymbolConfigProvider = SymbolConfigProvider,
                GeometryType = GeometryType,
            };
            uniqueValueConfigControl.UniqueValueModified += uniqueValueConfigControl_UniqueValueModified;
            return uniqueValueConfigControl;
        }

        void uniqueValueConfigControl_UniqueValueModified(object sender, UniqueValueModifiedEventArgs e)
        {
            bool isSelectedItem = UniqueValueConfigItems.SelectedItem == sender;

            OnUniqueValueRendererModified(new SelectedUniqueValueModificationEventArgs() { 
                                                    UniqueValue = e.UniqueValue, 
                                                    UniqueValueModificationType = e.UniqueValueModificationType,
                                                    IsSelectedItem = isSelectedItem
            });
        }

        public void DeleteCurrentSelectedUniqueValue()
        {
            if (UniqueValueConfigItems == null)
                return;

            UniqueValueConfigControl uniqueValueConfigControl = UniqueValueConfigItems.SelectedItem as UniqueValueConfigControl;
            if (uniqueValueConfigControl == null || uniqueValueConfigControl.UniqueValue == null)
                return;

            if (UniqueValueRenderer != null)
                UniqueValueRenderer.Infos.Remove(uniqueValueConfigControl.UniqueValue);

            int prevIndex = UniqueValueConfigItems.SelectedIndex;
            UniqueValueConfigItems.Items.Remove(uniqueValueConfigControl);

            if (prevIndex > 0 && prevIndex < UniqueValueConfigItems.Items.Count) // Remember .. we have a default symbol item at the top
                UniqueValueConfigItems.SelectedIndex = prevIndex; // preserve selected item            
        }

        internal Control GetCurrentSelectedConfigControl()
        {
            if (UniqueValueConfigItems == null)
                return null;
            return UniqueValueConfigItems.SelectedItem as Control;
        }

        internal void RefreshSymbols()
        {
            bindUIToRenderer();
        }

        protected virtual void OnUniqueValueRendererModified(SelectedUniqueValueModificationEventArgs e)
        {
            if (UniqueValueRendererModified != null)
                UniqueValueRendererModified(this, e);
        }

        protected virtual void OnCurrentUniqueValueChanged(CurrentUniqueValueChangedEventArgs e)
        {
            if (CurrentUniqueValueChanged != null)
                CurrentUniqueValueChanged(this, e);
        }        

        protected virtual void OnDefaultClassBreakBeingConfigured(DefaultClassBreakBeingConfiguredEventArgs e)
        {
            if (DefaultClassBreakBeingConfigured != null)
                DefaultClassBreakBeingConfigured(this, e);
        }

        public event EventHandler<SelectedUniqueValueModificationEventArgs> UniqueValueRendererModified;
        public event EventHandler<CurrentUniqueValueChangedEventArgs> CurrentUniqueValueChanged;
        public event EventHandler<DefaultClassBreakBeingConfiguredEventArgs> DefaultClassBreakBeingConfigured;                
        
    }

    public class SelectedUniqueValueModificationEventArgs : UniqueValueModifiedEventArgs
    {
        public bool IsSelectedItem { get; set; }
    }

    public class CurrentUniqueValueChangedEventArgs : EventArgs
    {
        public UniqueValueInfo UniqueValue { get; set; }
    }    
}
