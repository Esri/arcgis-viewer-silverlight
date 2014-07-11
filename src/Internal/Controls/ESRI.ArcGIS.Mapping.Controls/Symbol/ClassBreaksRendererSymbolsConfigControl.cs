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
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Mapping.Core.Symbols;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ClassBreaksRendererSymbolsConfigControl : Control
    {
        internal ListBox ClassBreakConfigItems;
        private const string PART_CLASSBREAKSCONFIGITEMS = "ClassBreakConfigItems";

        public ClassBreaksRendererSymbolsConfigControl()
        {
            DefaultStyleKey = typeof(ClassBreaksRendererSymbolsConfigControl);
        }

        public override void OnApplyTemplate()
        {
            if (ClassBreakConfigItems != null)
                ClassBreakConfigItems.SelectionChanged -= ClassBreakConfigItems_SelectionChanged;

            base.OnApplyTemplate();

            ClassBreakConfigItems = GetTemplateChild(PART_CLASSBREAKSCONFIGITEMS) as ListBox;
            if (ClassBreakConfigItems != null)
                ClassBreakConfigItems.SelectionChanged += ClassBreakConfigItems_SelectionChanged;

            bindUIToRenderer();

            if (InitCompleted != null)
                InitCompleted(this, EventArgs.Empty);
        }

        internal event EventHandler InitCompleted;

        void ClassBreakConfigItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClassBreakConfigItems == null)
                return;

            ClassBreakConfigControl configControl = ClassBreakConfigItems.SelectedItem as ClassBreakConfigControl;
            if (configControl != null)
            {
                OnCurrentClassBreakChanged(new CurrentClassBreakChangedEventArgs()
                {
                    ClassBreak = configControl.ClassBreak,
                });
                return;
            }
            else
            {
                DefaultSymbolConfigControl defaultConfigControl = ClassBreakConfigItems.SelectedItem as DefaultSymbolConfigControl;
                if (defaultConfigControl != null)
                {
                    OnDefaultClassBreakBeingConfigured(new DefaultClassBreakBeingConfiguredEventArgs() { 
                         DefaultSymbol = defaultConfigControl.Symbol,
                    });
                    return;
                }
            }

            if(ClassBreakConfigItems.Items != null)
            {
                foreach (Control control in ClassBreakConfigItems.Items)
                {
                    ClassBreakConfigControl classBreakConfigItemControl = control as ClassBreakConfigControl;
                    if (classBreakConfigItemControl != null)
                        classBreakConfigItemControl.StopEditMode();
                }
            }
        }

        void bindUIToRenderer()
        {
            if (ClassBreaksRenderer == null)
                return;

            bindList();
        }

        private void bindList()
        {
            if (ClassBreakConfigItems != null)
            {
                ClassBreakConfigItems.Items.Clear();

                if (ClassBreaksRenderer != null)
                {                    
                    // Add the default symbol config control
                    DefaultSymbolConfigControl defaultSymbolConfigControl = new DefaultSymbolConfigControl()
                    {
                        Symbol = ClassBreaksRenderer.DefaultSymbol,
                        SymbolConfigProvider = SymbolConfigProvider,
                        GeometryType = GeometryType,
                    };
                    defaultSymbolConfigControl.DefaultSymbolModified += new EventHandler<SymbolSelectedEventArgs>(defaultSymbolConfigControl_DefaultSymbolModified);
                    ClassBreakConfigItems.Items.Add(defaultSymbolConfigControl);
                    ClassBreakConfigItems.SelectedItem = defaultSymbolConfigControl;

                    foreach (ClassBreakInfo classBreak in ClassBreaksRenderer.Classes)
                    {
                        ClassBreakConfigControl classBreakConfigControl = createNewClassBreakConfigControl(classBreak);
                        ClassBreakConfigItems.Items.Add(classBreakConfigControl);
                    }
                }
            }
        }

        void defaultSymbolConfigControl_DefaultSymbolModified(object sender, SymbolSelectedEventArgs e)
        {
            if (ClassBreaksRenderer != null)
                ClassBreaksRenderer.DefaultSymbol = e.Symbol;

            bool isSelectedItem = ClassBreakConfigItems.SelectedItem == sender;

            OnClassBreakRendererModified(new SelectedClassBreakModificationEventArgs() { ClassBreakModificationType = ClassBreakModificationType.SymbolChanged, IsSelectedItem = isSelectedItem });
        }

        private ClassBreakConfigControl createNewClassBreakConfigControl(ClassBreakInfo classBreak)
        {
            ClassBreakConfigControl classBreakConfigControl = new ClassBreakConfigControl()
            {
                ClassBreak = classBreak,
                SymbolConfigProvider = SymbolConfigProvider,
                GeometryType = GeometryType,
            };
            classBreakConfigControl.ClassBreakModified += new EventHandler<ClassBreakModificationEventArgs>(classBreakConfigControl_ClassBreakChanged);
            return classBreakConfigControl;
        }

        void classBreakConfigControl_ClassBreakChanged(object sender, ClassBreakModificationEventArgs e)
        {
            // Auto adjust adjacent class breaks
            if (e.ClassBreakModificationType == ClassBreakModificationType.MinValueChanged)
            {                
                ClassBreakConfigControl control = sender as ClassBreakConfigControl;
                if (control != null)
                {
                    int indexOfClassBreak = -1;
                    ClassBreakInfo classBreak = control.ClassBreak;
                    indexOfClassBreak = ClassBreaksRenderer.Classes.IndexOf(classBreak);
                    if (indexOfClassBreak > 0)
                    {                        
                        // Update the previous class break in the collection                        
                        if (ClassBreakConfigItems != null && ClassBreakConfigItems.Items != null)
                        {
                            int indexOfControl = ClassBreakConfigItems.Items.IndexOf(control);
                            if (indexOfControl > 0)// array bounds check
                            {
                                ClassBreakConfigControl prevControl = ClassBreakConfigItems.Items[indexOfControl - 1] as ClassBreakConfigControl;
                                if (prevControl != null)
                                {
                                    ClassBreakInfo prevClassBreak = prevControl.ClassBreak;
                                    double incr = 1.0;
                                    double delta = 2 * incr;
                                    double diff = prevClassBreak.MaximumValue - prevClassBreak.MinimumValue;
                                    if (Math.Round(diff, 0) != Math.Round(diff,4)) // dealing with precision of less than 1 integer
                                    {
                                        incr = 0.01;
                                        delta = 2 * incr;
                                    }

                                    double newValue = classBreak.MinimumValue - incr;
                                    if (classBreak.MinimumValue <= prevClassBreak.MinimumValue + delta)
                                    {
                                        // don't allow min value to drop below previous class break value                                        
                                        // Auto audjust the class breaks
                                        newValue = prevClassBreak.MinimumValue + delta;
                                        control.UpdateMinValue(newValue); 

                                        prevControl.UpdateMaxValue(newValue - incr);
                                    }
                                    else
                                    {
                                        prevControl.UpdateMaxValue(newValue);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (e.ClassBreakModificationType == ClassBreakModificationType.MaxValueChanged)
            {
                ClassBreakConfigControl control = sender as ClassBreakConfigControl;
                if (control != null)
                {
                    int indexOfClassBreak = -1;
                    ClassBreakInfo classBreak = control.ClassBreak;
                    indexOfClassBreak = ClassBreaksRenderer.Classes.IndexOf(classBreak);
                    if (indexOfClassBreak > -1 && indexOfClassBreak < ClassBreaksRenderer.Classes.Count - 1)
                    {                        
                        // Update following class break                        
                        if (ClassBreakConfigItems != null && ClassBreakConfigItems.Items != null)
                        {
                            int indexOfControl = ClassBreakConfigItems.Items.IndexOf(control);
                            if (indexOfControl > -1 && indexOfControl < ClassBreakConfigItems.Items.Count - 1)// array bounds check
                            {
                                ClassBreakConfigControl nextControl = ClassBreakConfigItems.Items[indexOfControl + 1] as ClassBreakConfigControl;
                                if (nextControl != null)
                                {
                                    ClassBreakInfo nextClassBreak = nextControl.ClassBreak;
                                    double incr = 1.0;                                    
                                    double delta = 2 * incr;
                                    double diff = nextClassBreak.MaximumValue - nextClassBreak.MinimumValue;
                                    if (Math.Round(diff, 0) != Math.Round(diff,4)) // dealing with precision of less than 1 integer
                                    {
                                        incr = 0.01;
                                        delta = 2 * incr;
                                    }

                                    double newValue = classBreak.MaximumValue + incr;
                                    // check if the max value is greater than max of next classbreak (minus the minimum spacing)
                                    if (classBreak.MaximumValue >= (nextClassBreak.MaximumValue - delta))
                                    {
                                        // don't allow max value to go above next class break value                                        
                                        // Auto audjust the class breaks
                                        newValue = nextClassBreak.MaximumValue - delta;
                                        control.UpdateMaxValue(newValue); 

                                        nextControl.UpdateMinValue(newValue + incr);
                                    }
                                    else
                                    {
                                        nextControl.UpdateMinValue(newValue);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            SelectedClassBreakModificationEventArgs args = new SelectedClassBreakModificationEventArgs()
            {
                ClassBreakModificationType = e.ClassBreakModificationType,
                IsSelectedItem = ClassBreakConfigItems.SelectedItem == sender,
            };
            OnClassBreakRendererModified(args);
        }

        internal Control GetCurrentSelectedConfigControl()
        {
            if (ClassBreakConfigItems == null)
                return null;
            return ClassBreakConfigItems.SelectedItem as Control;
        }

        internal void RefreshSymbols()
        {
            bindUIToRenderer();            
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
                typeof(ClassBreaksRendererSymbolsConfigControl),
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
                typeof(ClassBreaksRendererSymbolsConfigControl),
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
                typeof(ClassBreaksRendererSymbolsConfigControl),
                new PropertyMetadata(null));
        #endregion

        protected virtual void OnClassBreakRendererModified(SelectedClassBreakModificationEventArgs e)
        {
            if (ClassBreakRendererModified != null)
                ClassBreakRendererModified(this, e);
        }

        protected virtual void OnCurrentClassBreakChanged(CurrentClassBreakChangedEventArgs e)
        {
            if (CurrentClassBreakChanged != null)
                CurrentClassBreakChanged(this, e);
        }

        protected virtual void OnDefaultClassBreakBeingConfigured(DefaultClassBreakBeingConfiguredEventArgs e)
        {
            if (DefaultClassBreakBeingConfigured != null)
                DefaultClassBreakBeingConfigured(this, e);
        }

        public event EventHandler<SelectedClassBreakModificationEventArgs> ClassBreakRendererModified;
        public event EventHandler<CurrentClassBreakChangedEventArgs> CurrentClassBreakChanged;
        public event EventHandler<DefaultClassBreakBeingConfiguredEventArgs> DefaultClassBreakBeingConfigured;
    }

    public class SelectedClassBreakModificationEventArgs : ClassBreakModificationEventArgs
    {
        public bool IsSelectedItem { get; set; }
    }   

    public class CurrentClassBreakChangedEventArgs : EventArgs
    {
        public ClassBreakInfo ClassBreak { get; set; }
    }

    public class DefaultClassBreakBeingConfiguredEventArgs : EventArgs
    {
        public Symbol DefaultSymbol { get; set; }
    }
}
