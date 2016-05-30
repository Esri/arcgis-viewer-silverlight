/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
    public class ClassBreaksRendererOptionsConfigControl : Control
    {
        internal ColorRampControl ColorRampControl;        
        internal NumericUpDown ClassBreaksNumericUpDown;

        private const string PART_COLORRAMPCONTROL = "ColorRampControl";        
        private const string PART_CLASSBREAKSNUMERICUPDOWN = "ClassBreaksNumericUpDown";

        public ClassBreaksRendererOptionsConfigControl()
        {
            DefaultStyleKey = typeof(ClassBreaksRendererOptionsConfigControl);
        }

        public override void OnApplyTemplate()
        {
            if (ColorRampControl != null)
                ColorRampControl.ColorGradientChosen -= ColorRampControl_GradientBrushChanged;

            if (ClassBreaksNumericUpDown != null)
            {                
                ClassBreaksNumericUpDown.ValueChanged -= ClassBreaksNumericUpDown_ValueChanged;
                ClassBreaksNumericUpDown.ValueChanging -= ClassBreaksNumericUpDown_ValueChanging;
            }

            base.OnApplyTemplate();

            ColorRampControl = GetTemplateChild(PART_COLORRAMPCONTROL) as ColorRampControl;                        

            ClassBreaksNumericUpDown = GetTemplateChild(PART_CLASSBREAKSNUMERICUPDOWN) as NumericUpDown;
            
            bindUIToRenderer(); // bind before attaching to event handlers

            if (ColorRampControl != null)
                ColorRampControl.ColorGradientChosen += ColorRampControl_GradientBrushChanged;

            if (ClassBreaksNumericUpDown != null)
            {
                ClassBreaksNumericUpDown.ValueChanged += ClassBreaksNumericUpDown_ValueChanged;
                ClassBreaksNumericUpDown.ValueChanging += ClassBreaksNumericUpDown_ValueChanging;
            }

            if (InitCompleted != null)
                InitCompleted(this, EventArgs.Empty);
        }

        internal event EventHandler InitCompleted;

        void ClassBreaksNumericUpDown_ValueChanging(object sender, RoutedPropertyChangingEventArgs<double> e)
        {
            if (e.NewValue < 2) // don't let users have less than 2 class breaks
            {
                e.Cancel = true;
                return;
            }            
        }

        void ClassBreaksNumericUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ClassBreaksRenderer == null)
                return;

            if (e.NewValue < 2 || e.NewValue == e.OldValue || ClassBreaksRenderer.Classes.Count == e.NewValue)
                return;            

            OnRendererClassBreaksChanged(new RendererClassBreaksCountChangedEventArgs() { Value = e.NewValue });
        }

        void bindUIToRenderer()
        {
            if (ClassBreaksRenderer == null)
                return;

            if (ClassBreaksNumericUpDown != null)
                ClassBreaksNumericUpDown.Value = ClassBreaksRenderer.Classes.Count;
        }

        void ColorRampControl_GradientBrushChanged(object sender, GradientBrushChangedEventArgs e)
        {   
            OnRendererColorSchemeChanged(e);
        }

        #region ClassBreaksRenderer
        /// <summary>
        /// 
        /// </summary>
        public ClassBreaksRenderer ClassBreaksRenderer
        {
            get { return GetValue(ClassBreaksRendererProperty) as ClassBreaksRenderer; }
            set { SetValue(ClassBreaksRendererProperty, value); }
        }

        /// <summary>
        /// Identifies the ClassBreaksRenderer dependency property.
        /// </summary>
        public static readonly DependencyProperty ClassBreaksRendererProperty =
            DependencyProperty.Register(
                "ClassBreaksRenderer",
                typeof(ClassBreaksRenderer),
                typeof(ClassBreaksRendererOptionsConfigControl),
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
                typeof(ClassBreaksRendererOptionsConfigControl),
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
                typeof(ClassBreaksRendererOptionsConfigControl),
                new PropertyMetadata(GeometryType.Unknown));
        #endregion        

        protected void OnRendererColorSchemeChanged(GradientBrushChangedEventArgs args)
        {
            if (RendererColorSchemeChanged != null)
                RendererColorSchemeChanged(this, args);
        }

        protected void OnRendererClassBreaksChanged(RendererClassBreaksCountChangedEventArgs args)
        {
            if (RendererClassBreaksChanged != null)
                RendererClassBreaksChanged(this, args);
        }

        public event EventHandler<GradientBrushChangedEventArgs> RendererColorSchemeChanged;
        public event EventHandler<RendererClassBreaksCountChangedEventArgs> RendererClassBreaksChanged;

        
    }

    public class RendererClassBreaksCountChangedEventArgs : EventArgs
    {
        public double Value { get; set; }
    }
}
