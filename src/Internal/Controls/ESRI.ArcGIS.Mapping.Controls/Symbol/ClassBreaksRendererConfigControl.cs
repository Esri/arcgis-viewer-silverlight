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
using ESRI.ArcGIS.Client.Symbols;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ClassBreaksRendererConfigControl : Control
    {
        internal ListBox ClassBreakConfigItems;
        internal TextBox MinTextBox;
        internal TextBox MaxTextBox;
        internal Button AddRangeButton;
        internal Button DeleteRangeButton;
        internal Button PreviousRangeButton;
        internal Button NextRangeButton;
        internal SymbolConfigControl SymbolConfigControl;

        private const string PART_CLASSBREAKSCONFIGITEMS = "ClassBreakConfigItems";
        private const string PART_MINTEXTBOX = "MinTextBox";
        private const string PART_MAXTEXTBOX = "MaxTextBox";
        private const string PART_ADDRANGEBUTTON = "AddRangeButton";
        private const string PART_DELETERANGEBUTTON = "DeleteRangeButton";
        private const string PART_PREVIOUSRANGEBUTTON = "PreviousRangeButton";
        private const string PART_NEXTRANGEBUTTON = "NextRangeButton";
        private const string PART_SYMBOLCONFIGCONTROL = "SymbolConfigControl";

        public ClassBreaksRendererConfigControl()
        {
            DefaultStyleKey = typeof(ClassBreaksRendererConfigControl);
        }

        public override void OnApplyTemplate()
        {
            if(ClassBreakConfigItems != null)
                ClassBreakConfigItems.SelectionChanged -= ClassBreakConfigItems_SelectionChanged;

            if(MinTextBox != null)
                MinTextBox.TextChanged -= MinTextBox_TextChanged;

            if(MaxTextBox != null)
                MaxTextBox.TextChanged -= MaxTextBox_TextChanged;

            if(AddRangeButton != null)
                AddRangeButton.Click -= AddRangeButton_Click;

            if(DeleteRangeButton != null)
                DeleteRangeButton.Click -= DeleteRangeButton_Click;

            if(PreviousRangeButton != null)
                PreviousRangeButton.Click -= PreviousRangeButton_Click;

            if(NextRangeButton != null)
                NextRangeButton.Click -= NextRangeButton_Click;

            base.OnApplyTemplate();

            ClassBreakConfigItems = GetTemplateChild(PART_CLASSBREAKSCONFIGITEMS) as ListBox;
            if (ClassBreakConfigItems != null)
                ClassBreakConfigItems.SelectionChanged += ClassBreakConfigItems_SelectionChanged;

            MinTextBox = GetTemplateChild(PART_MINTEXTBOX) as TextBox;
            if (MinTextBox != null)
                MinTextBox.TextChanged += MinTextBox_TextChanged;

            MaxTextBox = GetTemplateChild(PART_MAXTEXTBOX) as TextBox;
            if (MaxTextBox != null)
                MaxTextBox.TextChanged += MaxTextBox_TextChanged;

            AddRangeButton = GetTemplateChild(PART_ADDRANGEBUTTON) as Button;
            if (AddRangeButton != null)
                AddRangeButton.Click += AddRangeButton_Click;

            DeleteRangeButton = GetTemplateChild(PART_DELETERANGEBUTTON) as Button;
            if (DeleteRangeButton != null)
                DeleteRangeButton.Click += DeleteRangeButton_Click;

            PreviousRangeButton = GetTemplateChild(PART_PREVIOUSRANGEBUTTON) as Button;
            if (PreviousRangeButton != null)
                PreviousRangeButton.Click += PreviousRangeButton_Click;

            NextRangeButton = GetTemplateChild(PART_NEXTRANGEBUTTON) as Button;
            if (NextRangeButton != null)
                NextRangeButton.Click += NextRangeButton_Click;

            SymbolConfigControl = GetTemplateChild(PART_SYMBOLCONFIGCONTROL) as SymbolConfigControl;

            bindList();
        }

        void NextRangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClassBreakConfigItems == null || ClassBreaksRenderer == null)
                return;

            if (ClassBreakConfigItems.SelectedIndex < ClassBreaksRenderer.Classes.Count) // Remember .. we have a default symbol item at the top
                ClassBreakConfigItems.SelectedIndex++;
        }

        void PreviousRangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClassBreakConfigItems == null)
                return;

            if(ClassBreakConfigItems.SelectedIndex > 0)
                ClassBreakConfigItems.SelectedIndex--;
        }

        void DeleteRangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClassBreakConfigItems == null)
                return;

            ClassBreakConfigControl classBreakConfigControl = ClassBreakConfigItems.SelectedItem as ClassBreakConfigControl;
            if (classBreakConfigControl == null || classBreakConfigControl.ClassBreak == null)
                return;

            if (ClassBreaksRenderer != null)            
                ClassBreaksRenderer.Classes.Remove(classBreakConfigControl.ClassBreak);

            int prevIndex = ClassBreakConfigItems.SelectedIndex;
            ClassBreakConfigItems.Items.Remove(classBreakConfigControl);

            if (prevIndex > 0 && prevIndex <= ClassBreakConfigItems.Items.Count)  // Remember .. we have a default symbol item at the top
                ClassBreakConfigItems.SelectedIndex = prevIndex; // preserve selected item
        }

        void AddRangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClassBreaksRenderer == null)
                return;

            ClassBreakConfigControl classBreakConfigControl = null;
            if (ClassBreaksRenderer.Classes.Count < 1) // Empty class break set
            {
                // Assign a first class break and symbolize it using the first class break
                ClassBreakInfo newClassBreak = new ClassBreakInfo() { 
                     MinimumValue = 0,
                     MaximumValue = 0,
                     Symbol = ClassBreaksRenderer.DefaultSymbol != null ? ClassBreaksRenderer.DefaultSymbol.CloneSymbol() : SymbolUtils.CreateDefaultSymbol(GeometryType),
                };
                classBreakConfigControl = createNewClassBreakConfigControl(newClassBreak);
                ClassBreaksRenderer.Classes.Add(newClassBreak);
            }
            else 
            {
                ClassBreakInfo lastClassBreak = ClassBreaksRenderer.Classes[ClassBreaksRenderer.Classes.Count - 1];
                double currentMaxVal = lastClassBreak.MaximumValue;
                lastClassBreak.MaximumValue -= Math.Floor((currentMaxVal - lastClassBreak.MinimumValue) / 2); // split class break into two
                ClassBreakInfo newClassBreak = new ClassBreakInfo()
                {
                    MinimumValue = lastClassBreak.MaximumValue + 1,
                    MaximumValue = currentMaxVal,
                    Symbol = lastClassBreak.Symbol != null ? lastClassBreak.Symbol.CloneSymbol() : SymbolUtils.CreateDefaultSymbol(GeometryType),
                };
                classBreakConfigControl = createNewClassBreakConfigControl(newClassBreak);
                ClassBreaksRenderer.Classes.Add(newClassBreak);
            }

            if (ClassBreakConfigItems != null)
            {
                ClassBreakConfigItems.Items.Add(classBreakConfigControl);
                ClassBreakConfigItems.SelectedItem = classBreakConfigControl;
            }            
        }        

        void MaxTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ClassBreakConfigItems == null)
                return;

            ClassBreakConfigControl classBreakConfigControl = ClassBreakConfigItems.SelectedItem as ClassBreakConfigControl;
            if (classBreakConfigControl == null || classBreakConfigControl.ClassBreak == null)
                return;

            double d;
            if (double.TryParse(MaxTextBox.Text, out d))
                classBreakConfigControl.UpdateMaxValue(d);

            OnClassBreakRendererModified(EventArgs.Empty);            
        }

        void MinTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ClassBreakConfigItems == null)
                return;

            ClassBreakConfigControl classBreakConfigControl = ClassBreakConfigItems.SelectedItem as ClassBreakConfigControl;
            if (classBreakConfigControl == null || classBreakConfigControl.ClassBreak == null)
                return;

            double d;
            if (double.TryParse(MinTextBox.Text, out d))
                classBreakConfigControl.UpdateMinValue(d);

            OnClassBreakRendererModified(EventArgs.Empty);            
        }

        void ClassBreakConfigItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            if (ClassBreakConfigItems == null)
                return;
            
            // Enable disable previous/next buttons appropriately
            if (PreviousRangeButton != null)
                PreviousRangeButton.IsEnabled = ClassBreakConfigItems.SelectedIndex > 0;
            if (NextRangeButton != null)
            {
                // remember .. the first item in the list is the default symbol
                if (ClassBreaksRenderer != null)
                    NextRangeButton.IsEnabled = ClassBreakConfigItems.SelectedIndex < ClassBreaksRenderer.Classes.Count;
            }

            ClassBreakConfigControl classBreakConfigControl = ClassBreakConfigItems.SelectedItem as ClassBreakConfigControl;
            if (classBreakConfigControl != null)
            {
                if (DeleteRangeButton != null)
                    DeleteRangeButton.IsEnabled = true;

                if (classBreakConfigControl.ClassBreak == null)
                    return;                

                if (SymbolConfigControl != null && classBreakConfigControl.ClassBreak != null)
                    SymbolConfigControl.Symbol = classBreakConfigControl.ClassBreak.Symbol;

                if (MinTextBox != null)
                {
                    MinTextBox.IsEnabled = true;
                    MinTextBox.TextChanged -= MinTextBox_TextChanged;
                    MinTextBox.Text = classBreakConfigControl.ClassBreak.MinimumValue.ToString();
                    MinTextBox.TextChanged += MinTextBox_TextChanged;
                }

                if (MaxTextBox != null)
                {
                    MaxTextBox.IsEnabled = true;
                    MaxTextBox.TextChanged -= MaxTextBox_TextChanged;
                    MaxTextBox.Text = classBreakConfigControl.ClassBreak.MaximumValue.ToString();
                    MaxTextBox.TextChanged += MaxTextBox_TextChanged;
                }
            }
            else
            {
                DefaultSymbolConfigControl defaultSymbolConfigControl = ClassBreakConfigItems.SelectedItem as DefaultSymbolConfigControl;
                if (defaultSymbolConfigControl != null)
                {
                    if (SymbolConfigControl != null)
                        SymbolConfigControl.Symbol = defaultSymbolConfigControl.Symbol;

                    if (DeleteRangeButton != null)
                        DeleteRangeButton.IsEnabled = false;

                    if (MinTextBox != null)
                    {
                        MinTextBox.IsEnabled = false;
                        MinTextBox.TextChanged -= MinTextBox_TextChanged;
                        MinTextBox.Text = "";
                        MinTextBox.TextChanged += MinTextBox_TextChanged;
                    }

                    if (MaxTextBox != null)
                    {
                        MaxTextBox.IsEnabled = false;
                        MaxTextBox.TextChanged -= MaxTextBox_TextChanged;
                        MaxTextBox.Text = "";
                        MaxTextBox.TextChanged += MaxTextBox_TextChanged;
                    }
                }
            }
        }

        private void bindList()
        {
            if (ClassBreakConfigItems != null)
            {
                if (ClassBreaksRenderer != null)
                {
                    // Add the default symbol config control
                    DefaultSymbolConfigControl defaultSymbolConfigControl = new DefaultSymbolConfigControl() { 
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

            OnClassBreakRendererModified(EventArgs.Empty);
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
            if (e.ClassBreakModificationType == ClassBreakModificationType.SymbolChanged)
            {
                if (ClassBreakConfigItems != null)
                {
                    ClassBreakConfigControl control = sender as ClassBreakConfigControl;
                    if (control != null && ClassBreakConfigItems.SelectedItem == control && control.ClassBreak != null)
                    {
                        if (SymbolConfigControl != null)
                            SymbolConfigControl.Symbol = control.ClassBreak.Symbol;
                    }
                }
            }
            OnClassBreakRendererModified(EventArgs.Empty);
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
                typeof(ClassBreaksRendererConfigControl),
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
                typeof(ClassBreaksRendererConfigControl),
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
                typeof(ClassBreaksRendererConfigControl),
                new PropertyMetadata(null));
        #endregion 

        protected virtual void OnClassBreakRendererModified(EventArgs e)
        {
            if (ClassBreakRendererModified != null)
                ClassBreakRendererModified(this, e);
        }

        public event EventHandler ClassBreakRendererModified;
    }
}
