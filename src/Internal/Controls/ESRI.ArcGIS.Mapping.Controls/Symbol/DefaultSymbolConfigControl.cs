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
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Mapping.Core;
using System.Windows.Controls.Primitives;
using ESRI.ArcGIS.Client.Toolkit.Primitives;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class DefaultSymbolConfigControl : Control
    {
        internal SymbolDisplay SymbolDisplay;
        internal SymbolSelector SymbolSelector;
        internal Rectangle SymbolOverlay;
        internal ToggleButton ToggleButton;

        public DefaultSymbolConfigControl()
        {
            DefaultStyleKey = typeof(DefaultSymbolConfigControl);
        }

        public override void OnApplyTemplate()
        {
            if (SymbolOverlay != null)
                SymbolOverlay.MouseLeftButtonUp -= SymbolOverlay_MouseLeftButtonUp;

            if (SymbolSelector != null)
                SymbolSelector.SymbolSelected -= SymbolSelector_SymbolSelected;

            base.OnApplyTemplate();

            SymbolSelector = GetTemplateChild("SymbolSelector") as SymbolSelector;
            if(SymbolSelector != null)
                SymbolSelector.SymbolSelected += SymbolSelector_SymbolSelected;

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
            Symbol = e.Symbol;
            OnDefaultSymbolModified(e);                        
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
                typeof(DefaultSymbolConfigControl),
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
                typeof(DefaultSymbolConfigControl),
                new PropertyMetadata(GeometryType.Unknown));
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
                typeof(DefaultSymbolConfigControl),
                new PropertyMetadata(null));
        #endregion
        
        protected virtual void OnDefaultSymbolModified(SymbolSelectedEventArgs e)
        {
            if (DefaultSymbolModified != null)
                DefaultSymbolModified(this, e);
        }

        public event EventHandler<SymbolSelectedEventArgs> DefaultSymbolModified;
    }
}
