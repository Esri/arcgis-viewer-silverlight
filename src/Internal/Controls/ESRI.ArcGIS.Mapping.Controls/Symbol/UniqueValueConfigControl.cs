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
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Toolkit.Primitives;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class UniqueValueConfigControl : Control
    {
        internal SymbolSelector SymbolSelector;
        internal TextBlock UniqueValueTextBlock;
        internal SymbolDisplay SymbolDisplay;
        internal Rectangle SymbolOverlay;
        internal ToggleButton ToggleButton;

        public UniqueValueConfigControl()
        {
            DefaultStyleKey = typeof(UniqueValueConfigControl);
        }

        public override void OnApplyTemplate()
        {
            if (SymbolOverlay != null)
                SymbolOverlay.MouseLeftButtonUp -= SymbolOverlay_MouseLeftButtonUp;

            if (SymbolSelector != null)
                SymbolSelector.SymbolSelected -= SymbolSelector_SymbolSelected;

            base.OnApplyTemplate();

            SymbolSelector = GetTemplateChild("SymbolSelector") as SymbolSelector;
            if (SymbolSelector != null)
                SymbolSelector.SymbolSelected += SymbolSelector_SymbolSelected;

            UniqueValueTextBlock = GetTemplateChild("UniqueValueTextBlock") as TextBlock;

            ToggleButton = GetTemplateChild("ToggleButton") as ToggleButton;

            SymbolDisplay = GetTemplateChild("SymbolDisplay") as SymbolDisplay;

            SymbolOverlay = GetTemplateChild("SymbolOverlay") as Rectangle;
            if (SymbolOverlay != null)
                SymbolOverlay.MouseLeftButtonUp += SymbolOverlay_MouseLeftButtonUp;

            if (InitCompleted != null)
                InitCompleted(this, EventArgs.Empty);
        }

        internal event EventHandler InitCompleted;

        void SymbolOverlay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ToggleButton != null)
                ToggleButton.IsChecked = !ToggleButton.IsChecked;
        }    

        void SymbolSelector_SymbolSelected(object sender, SymbolSelectedEventArgs e)
        {
            if (UniqueValue != null)
            {                
                UniqueValue.Symbol = e.Symbol;
            }

            if (SymbolDisplay != null)
                SymbolDisplay.Symbol = e.Symbol;

            OnUniqueValueModified(new UniqueValueModifiedEventArgs() { UniqueValue = UniqueValue, UniqueValueModificationType = UniqueValueModificationType.SymbolChanged });
        }

        #region UniqueValue
        /// <summary>
        /// 
        /// </summary>
        public UniqueValueInfo UniqueValue
        {
            get { return GetValue(UniqueValueProperty) as UniqueValueInfo; }
            set { SetValue(UniqueValueProperty, value); }
        }

        /// <summary>
        /// Identifies the UniqueValue dependency property.
        /// </summary>
        public static readonly DependencyProperty UniqueValueProperty =
            DependencyProperty.Register(
                "UniqueValue",
                typeof(UniqueValueInfo),
                typeof(UniqueValueConfigControl),
                new PropertyMetadata(null));
        #endregion public UniqueValueInfo UniqueValue

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
                typeof(UniqueValueConfigControl),
                new PropertyMetadata(GeometryType.Point));
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
                typeof(UniqueValueConfigControl),
                new PropertyMetadata(null));
        #endregion 

        public void UpdateUniqueValue(string newValue)
        {
            if (UniqueValueTextBlock != null && newValue != null)
                UniqueValueTextBlock.Text = newValue;
        }

        protected virtual void OnUniqueValueModified(UniqueValueModifiedEventArgs args)
        {
            if (UniqueValueModified != null)
                UniqueValueModified(this, args);
        }

        public event EventHandler<UniqueValueModifiedEventArgs> UniqueValueModified;
    }

    public class UniqueValueModifiedEventArgs : EventArgs
    {
        public UniqueValueModificationType UniqueValueModificationType { get; set; }
        public UniqueValueInfo UniqueValue { get; set; }
    }

    public enum UniqueValueModificationType
    {
        ValueChanged,
        SymbolChanged,
    }  
}
