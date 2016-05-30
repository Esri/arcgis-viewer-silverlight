/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Client.Toolkit.Primitives;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class SymbolPicker : Control
    {
        internal SymbolSelector SymbolSelector;
        internal SymbolDisplay SymbolDisplay;
        internal DropDownButton SymbolDropDownButton;

        public SymbolPicker()
        {
            DefaultStyleKey = typeof(SymbolPicker);
        }

        public override void OnApplyTemplate()
        {            
            if (SymbolSelector != null)
                SymbolSelector.SymbolSelected -= SymbolSelector_SymbolSelected;

            if (SymbolDropDownButton != null)
                SymbolDropDownButton.IsContentPopupOpen = false;

            base.OnApplyTemplate();

            SymbolSelector = GetTemplateChild("SymbolSelector") as SymbolSelector;
            if (SymbolSelector != null)
                SymbolSelector.SymbolSelected += SymbolSelector_SymbolSelected;

            SymbolDisplay = GetTemplateChild("SymbolDisplay") as SymbolDisplay;

            SymbolDropDownButton = GetTemplateChild("SymbolDropDownButton") as DropDownButton;

            if (InitCompleted != null)
                InitCompleted(this, EventArgs.Empty);
        }

        internal event EventHandler InitCompleted;

        void SymbolSelector_SymbolSelected(object sender, SymbolSelectedEventArgs e)
        {
            Symbol = e.Symbol;
            if (SymbolDisplay != null)
                SymbolDisplay.Symbol = Symbol;
            OnSymbolSelected(e);
        }

        public void CollapseDropDown()
        {
            if (SymbolDropDownButton != null)
                SymbolDropDownButton.IsContentPopupOpen = false;
        }
        
        public void RepositionPopup()
        {
            
        }

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
                typeof(SymbolPicker),
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
                typeof(SymbolPicker),
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
                typeof(SymbolPicker),
                new PropertyMetadata(GeometryType.Point));
        #endregion 

        protected virtual void OnSymbolSelected(SymbolSelectedEventArgs args)
        {
            if (SymbolSelected != null)
                SymbolSelected(this, args);
        }

        public event EventHandler<SymbolSelectedEventArgs> SymbolSelected;
    }
}
