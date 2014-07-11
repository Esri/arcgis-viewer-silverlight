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
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class UniqueValueRendererOptionsConfigControl : Control
    {
        internal ColorRampControl ColorRampControl;        
        internal Button AddUniqueValueButton;
        internal Button DeleteUniqueValueButton;
        internal TextBox UniqueValueTextBox;

        private const string PART_COLORRAMPCONTROL = "ColorRampControl";        
        private const string PART_ADDUNIQUEVALUEBUTTON = "AddUniqueValueButton";
        private const string PART_DELETEUNIQUEVALUEBUTTON = "DeleteUniqueValueButton";
        private const string PART_UNIQUEVALUETEXTBOX = "UniqueValueTextBox";

        public UniqueValueRendererOptionsConfigControl()
        {
            DefaultStyleKey = typeof(UniqueValueRendererOptionsConfigControl);
        }

        public override void OnApplyTemplate()
        {
            if (ColorRampControl != null)
                ColorRampControl.ColorGradientChosen -= ColorRampControl_GradientBrushChanged;

            if (AddUniqueValueButton != null)
                AddUniqueValueButton.Click -= AddUniqueValueButton_Click;

            if (DeleteUniqueValueButton != null)
                DeleteUniqueValueButton.Click -= DeleteUniqueValueButton_Click;

            base.OnApplyTemplate();

            AddUniqueValueButton = GetTemplateChild(PART_ADDUNIQUEVALUEBUTTON) as Button;
            if (AddUniqueValueButton != null)
                AddUniqueValueButton.Click += AddUniqueValueButton_Click;

            DeleteUniqueValueButton = GetTemplateChild(PART_DELETEUNIQUEVALUEBUTTON) as Button;
            if (DeleteUniqueValueButton != null)
                DeleteUniqueValueButton.Click += DeleteUniqueValueButton_Click;

            ColorRampControl = GetTemplateChild(PART_COLORRAMPCONTROL) as ColorRampControl;
            if (ColorRampControl != null)
                ColorRampControl.ColorGradientChosen += ColorRampControl_GradientBrushChanged;

            UniqueValueTextBox = GetTemplateChild(PART_UNIQUEVALUETEXTBOX) as TextBox;

            if (InitCompleted != null)
                InitCompleted(this, EventArgs.Empty);
        }

        internal event EventHandler InitCompleted;

        void AddUniqueValueButton_Click(object sender, RoutedEventArgs e)
        {
            OnNewUniqueValueClicked(new NewUniqueValueInfoEventArgs() {
                 UniqueValue =  UniqueValueTextBox == null ? string.Empty : UniqueValueTextBox.Text,
            });
            UniqueValueTextBox.Text = string.Empty;
        }

        void DeleteUniqueValueButton_Click(object sender, RoutedEventArgs e)
        {
            OnDeleteUniqueValueClicked(EventArgs.Empty);
        }

        void ColorRampControl_GradientBrushChanged(object sender, GradientBrushChangedEventArgs e)
        {
            OnRendererColorSchemeChanged(e);
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
                typeof(UniqueValueRendererOptionsConfigControl),
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
                typeof(UniqueValueRendererOptionsConfigControl),
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
                typeof(UniqueValueRendererOptionsConfigControl),
                new PropertyMetadata(GeometryType.Unknown));
        #endregion        

        protected void OnRendererColorSchemeChanged(GradientBrushChangedEventArgs args)
        {
            if (RendererColorSchemeChanged != null)
                RendererColorSchemeChanged(this, args);
        }

        protected void OnNewUniqueValueClicked(NewUniqueValueInfoEventArgs args)
        {
            if (NewUniqueValueAdded != null)
                NewUniqueValueAdded(this, args);
        }

        protected void OnDeleteUniqueValueClicked(EventArgs args)
        {
            if (DeleteUniqueValueClicked != null)
                DeleteUniqueValueClicked(this, args);
        }

        public event EventHandler<GradientBrushChangedEventArgs> RendererColorSchemeChanged;
        public event EventHandler<NewUniqueValueInfoEventArgs> NewUniqueValueAdded;
        public event EventHandler DeleteUniqueValueClicked;        
    }

    public class NewUniqueValueInfoEventArgs : EventArgs
    {
        public string UniqueValue { get; set; }
    }
}
