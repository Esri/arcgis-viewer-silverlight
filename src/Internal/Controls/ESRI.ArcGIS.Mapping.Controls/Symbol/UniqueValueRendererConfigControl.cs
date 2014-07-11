/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;
using System;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class UniqueValueRendererConfigControl : Control
    {
        internal ListBox UniqueValueConfigItems;
        internal TextBox ValueTextBox;
        internal Button AddUniqueValueButton;
        internal Button DeleteUniqueValueButton;
        internal Button PreviousUniqueValueButton;
        internal Button NextUniqueValueButton;
        internal SymbolConfigControl SymbolConfigControl;

        private const string PART_UNIQUEVALUECONFIGITEMS = "UniqueValueConfigItems";
        private const string PART_VALUETEXTBOX = "ValueTextBox";
        private const string PART_ADDUNIQUEVALUEBUTTON = "AddUniqueValueButton";
        private const string PART_DELETEUNIQUEVALUEBUTTON = "DeleteUniqueValueButton";
        private const string PART_PREVIOUSUNIQUEVALUEBUTTON = "PreviousUniqueValueButton";
        private const string PART_NEXTUNIQUEVALUEBUTTON = "NextUniqueValueButton";
        private const string PART_SYMBOLCONFIGCONTROL = "SymbolConfigControl";

        public UniqueValueRendererConfigControl()
        {
            DefaultStyleKey = typeof(UniqueValueRendererConfigControl);
        }

        public override void OnApplyTemplate()
        {
            if (UniqueValueConfigItems != null)
                UniqueValueConfigItems.SelectionChanged -= UniqueValueConfigItems_SelectionChanged;

            if (ValueTextBox != null)
                ValueTextBox.TextChanged -= ValueTextBox_TextChanged;

            if (AddUniqueValueButton != null)
                AddUniqueValueButton.Click -= AddUniqueValueButton_Click;

            if (DeleteUniqueValueButton != null)
                DeleteUniqueValueButton.Click -= DeleteUniqueValueButton_Click;

            if (PreviousUniqueValueButton != null)
                PreviousUniqueValueButton.Click -= PreviousUniqueValueButton_Click;

            if (NextUniqueValueButton != null)
                NextUniqueValueButton.Click -= NextUniqueValueButton_Click;

            base.OnApplyTemplate();

            UniqueValueConfigItems = GetTemplateChild(PART_UNIQUEVALUECONFIGITEMS) as ListBox;
            if(UniqueValueConfigItems != null)
                UniqueValueConfigItems.SelectionChanged += UniqueValueConfigItems_SelectionChanged;

            AddUniqueValueButton = GetTemplateChild(PART_ADDUNIQUEVALUEBUTTON) as Button;
            if (AddUniqueValueButton != null)
                AddUniqueValueButton.Click += AddUniqueValueButton_Click;

            DeleteUniqueValueButton = GetTemplateChild(PART_DELETEUNIQUEVALUEBUTTON) as Button;
            if (DeleteUniqueValueButton != null)
                DeleteUniqueValueButton.Click += DeleteUniqueValueButton_Click;

            PreviousUniqueValueButton = GetTemplateChild(PART_PREVIOUSUNIQUEVALUEBUTTON) as Button;
            if (PreviousUniqueValueButton != null)
                PreviousUniqueValueButton.Click += PreviousUniqueValueButton_Click;

            NextUniqueValueButton = GetTemplateChild(PART_NEXTUNIQUEVALUEBUTTON) as Button;
            if (NextUniqueValueButton != null)
                NextUniqueValueButton.Click += NextUniqueValueButton_Click;

            SymbolConfigControl = GetTemplateChild(PART_SYMBOLCONFIGCONTROL) as SymbolConfigControl;

            ValueTextBox = GetTemplateChild(PART_VALUETEXTBOX) as TextBox;

            bindList();
        }

        void UniqueValueConfigItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UniqueValueConfigItems == null)
                return;

            if (PreviousUniqueValueButton != null)
                PreviousUniqueValueButton.IsEnabled = UniqueValueConfigItems.SelectedIndex > 0;

            if (NextUniqueValueButton != null)
            {
                // remember .. the first item in the list is the default symbol
                if (UniqueValueRenderer != null)
                    NextUniqueValueButton.IsEnabled = UniqueValueConfigItems.SelectedIndex < UniqueValueRenderer.Infos.Count;
            }

            UniqueValueConfigControl uniqueValueConfigControl = UniqueValueConfigItems.SelectedItem as UniqueValueConfigControl;
            if (uniqueValueConfigControl != null)
            {
                if (uniqueValueConfigControl.UniqueValue == null)
                    return;

                if (DeleteUniqueValueButton != null)
                    DeleteUniqueValueButton.IsEnabled = true;                

                if (SymbolConfigControl != null && uniqueValueConfigControl.UniqueValue != null)
                    SymbolConfigControl.Symbol = uniqueValueConfigControl.UniqueValue.Symbol;

                if (ValueTextBox != null)
                {
                    ValueTextBox.IsEnabled = true;
                    if (uniqueValueConfigControl.UniqueValue.Value != null)
                    {                        
                        ValueTextBox.TextChanged -= ValueTextBox_TextChanged;
                        ValueTextBox.Text = uniqueValueConfigControl.UniqueValue.Value.ToString();
                        ValueTextBox.TextChanged += ValueTextBox_TextChanged;
                    }
                }
            }
            else
            {
                DefaultSymbolConfigControl defaultSymbolConfigControl = UniqueValueConfigItems.SelectedItem as DefaultSymbolConfigControl;
                if (defaultSymbolConfigControl != null)
                {
                    if (SymbolConfigControl != null)
                        SymbolConfigControl.Symbol = defaultSymbolConfigControl.Symbol;

                    if (DeleteUniqueValueButton != null)
                        DeleteUniqueValueButton.IsEnabled = false;

                    if (ValueTextBox != null)
                    {
                        ValueTextBox.IsEnabled = false;
                        ValueTextBox.TextChanged -= ValueTextBox_TextChanged;
                        ValueTextBox.Text = "";
                        ValueTextBox.TextChanged += ValueTextBox_TextChanged;
                    }
                }
            }
        }

        void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UniqueValueConfigItems == null)
                return;

            UniqueValueConfigControl uniqueValueConfigControl = UniqueValueConfigItems.SelectedItem as UniqueValueConfigControl;
            if (uniqueValueConfigControl == null || uniqueValueConfigControl.UniqueValue == null)
                return;

            uniqueValueConfigControl.UpdateUniqueValue(ValueTextBox.Text);

            OnUniqueValueRendererModified(EventArgs.Empty);
        }

        void NextUniqueValueButton_Click(object sender, RoutedEventArgs e)
        {
            if (UniqueValueConfigItems == null || UniqueValueRenderer == null)
                return;

            if (UniqueValueConfigItems.SelectedIndex < UniqueValueRenderer.Infos.Count)  // Remember .. we have a default symbol item at the top
                UniqueValueConfigItems.SelectedIndex++;
        }

        void PreviousUniqueValueButton_Click(object sender, RoutedEventArgs e)
        {
            if (UniqueValueConfigItems == null)
                return;

            if(UniqueValueConfigItems.SelectedIndex > 0)
                UniqueValueConfigItems.SelectedIndex--;
        }

        void DeleteUniqueValueButton_Click(object sender, RoutedEventArgs e)
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

            if (prevIndex > 0 && prevIndex <= UniqueValueConfigItems.Items.Count) // Remember .. we have a default symbol item at the top
                UniqueValueConfigItems.SelectedIndex = prevIndex; // preserve selected item
        }

        void AddUniqueValueButton_Click(object sender, RoutedEventArgs e)
        {
            if (UniqueValueRenderer == null)
                return;

            UniqueValueConfigControl uniqueValueConfigControl;
            UniqueValueInfo newUniqueValue = new UniqueValueInfo()
                {
                    Value = "",     // TODO:- set the default value based on selected attribute
                    Symbol = UniqueValueRenderer.DefaultSymbol != null ? UniqueValueRenderer.DefaultSymbol.CloneSymbol() : SymbolUtils.CreateDefaultSymbol(GeometryType),
                };
            uniqueValueConfigControl = createNewUniqueValueConfigControl(newUniqueValue);
            UniqueValueRenderer.Infos.Add(newUniqueValue);            

            if (UniqueValueConfigItems != null)
            {
                UniqueValueConfigItems.Items.Add(uniqueValueConfigControl);
                UniqueValueConfigItems.SelectedItem = uniqueValueConfigControl;
            }       
        }

        private void bindList()
        {
            if (UniqueValueConfigItems != null)
            {
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

            OnUniqueValueRendererModified(EventArgs.Empty);
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

        void uniqueValueConfigControl_UniqueValueModified(object sender, EventArgs e)
        {
            if (UniqueValueConfigItems != null)
            {
                UniqueValueConfigControl control = sender as UniqueValueConfigControl;
                if (control != null && UniqueValueConfigItems.SelectedItem == control && control.UniqueValue != null)
                {
                    if (SymbolConfigControl != null)
                        SymbolConfigControl.Symbol = control.UniqueValue.Symbol;
                }
            }

            OnUniqueValueRendererModified(EventArgs.Empty);
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
                typeof(UniqueValueRendererConfigControl),
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
                typeof(UniqueValueRendererConfigControl),
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
                typeof(UniqueValueRendererConfigControl),
                new PropertyMetadata(GeometryType.Point));
        #endregion 

        protected virtual void OnUniqueValueRendererModified(EventArgs e)
        {
            if (UniqueValueRendererModified != null)
                UniqueValueRendererModified(this, e);
        }

        public event EventHandler UniqueValueRendererModified;
    }
}
