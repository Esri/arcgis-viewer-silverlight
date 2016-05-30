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
using System.Windows.Controls.Primitives;
using ESRI.ArcGIS.Client.Toolkit.Primitives;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ClassBreakConfigControl : Control
    {
        internal SymbolSelector SymbolSelector;
        internal TextBlock MinValueTextBlock;
        internal TextBlock MaxValueTextBlock;
        internal TextBox MinValueTextBox;
        internal TextBox MaxValueTextBox;
        internal ToggleButton ToggleButton;
        internal SymbolDisplay SymbolDisplay;
        internal Rectangle SymbolOverlay;

        public ClassBreakConfigControl()
        {
            DefaultStyleKey = typeof(ClassBreakConfigControl);
        }

        public override void OnApplyTemplate()
        {
            if (SymbolSelector != null)
                SymbolSelector.SymbolSelected -= SymbolSelector_SymbolChanged;

            if (MinValueTextBlock != null)
                MinValueTextBlock.MouseLeftButtonUp -= MinValueTextBlock_MouseLeftButtonUp;

            if (MaxValueTextBlock != null)
                MaxValueTextBlock.MouseLeftButtonUp -= MaxValueTextBlock_MouseLeftButtonUp;

            if (MinValueTextBox != null)
            {
                MinValueTextBox.KeyDown -= MinValueTextBox_KeyDown;
                MinValueTextBox.LostFocus -= MinValueTextBox_LostFocus;
            }

            if (MaxValueTextBox != null)
            {
                MaxValueTextBox.KeyDown += MaxValueTextBox_KeyDown;
                MaxValueTextBox.LostFocus -= MaxValueTextBox_LostFocus;
            }

            if (SymbolOverlay != null)
                SymbolOverlay.MouseLeftButtonUp -= SymbolOverlay_MouseLeftButtonUp;

            base.OnApplyTemplate();

            SymbolSelector = GetTemplateChild("SymbolSelector") as SymbolSelector;
            if(SymbolSelector != null)
                SymbolSelector.SymbolSelected += SymbolSelector_SymbolChanged;

            MinValueTextBlock = GetTemplateChild("MinValueTextBlock") as TextBlock;
            if (MinValueTextBlock != null)
                MinValueTextBlock.MouseLeftButtonUp += MinValueTextBlock_MouseLeftButtonUp;

            MaxValueTextBlock = GetTemplateChild("MaxValueTextBlock") as TextBlock;
            if (MaxValueTextBlock != null)
                MaxValueTextBlock.MouseLeftButtonUp += MaxValueTextBlock_MouseLeftButtonUp;

            MinValueTextBox = GetTemplateChild("MinValueTextBox") as TextBox;
            if (MinValueTextBox != null)
            {
                MinValueTextBox.KeyDown += MinValueTextBox_KeyDown;
                MinValueTextBox.LostFocus += MinValueTextBox_LostFocus;
            }

            MaxValueTextBox = GetTemplateChild("MaxValueTextBox") as TextBox;
            if (MaxValueTextBox != null)
            {
                MaxValueTextBox.KeyDown += MaxValueTextBox_KeyDown;
                MaxValueTextBox.LostFocus += MaxValueTextBox_LostFocus;
            }

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

        void MinValueTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                onMinValueEntered();
        }

        void MaxValueTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                onMaxValueEntered();
        }

        internal void MaxValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            onMaxValueEntered();
			base.OnLostFocus(e);
        }

        private void onMinValueEntered()
        {
            double newValue = 0;
            if (MinValueTextBox != null)
            {
                if (!double.TryParse(MinValueTextBox.Text, System.Globalization.NumberStyles.Any, CultureHelper.GetCurrentCulture(), out newValue))
                {
                    MinValueTextBox.SetValue(ToolTipService.ToolTipProperty, LocalizableStrings.InvalidClassBreakValue);
                    VisualStateManager.GoToState(MinValueTextBox, "InvalidUnfocused", true);
                    MinValueTextBox.Focus();
                    return;
                }

                double min, max;
                if (!isMinLessThanMax(out min, out max))
                {
                    double decr = 1.0;
                    if (Math.Round(max) != Math.Round(max,4))
                    {
                        decr = 0.01;
                    }
                    newValue = max - decr;                    
                }

                MinValueTextBox.Visibility = System.Windows.Visibility.Collapsed;                
            }

            VisualStateManager.GoToState(MinValueTextBox, "Valid", true);
            MinValueTextBox.SetValue(ToolTipService.ToolTipProperty, null);

            if (MinValueTextBlock != null)
                MinValueTextBlock.Visibility = System.Windows.Visibility.Visible;

            if (newValue == ClassBreak.MaximumValue) // no changes detected
                return;

            UpdateMinValue(newValue);            

            OnClassBreakModified(new ClassBreakModificationEventArgs() { ClassBreakModificationType = ClassBreakModificationType.MinValueChanged });
        }  

        private void onMaxValueEntered()
        {
            double newValue = 0;            
            if (MaxValueTextBox != null)
            {
                if (!double.TryParse(MaxValueTextBox.Text, System.Globalization.NumberStyles.Any, CultureHelper.GetCurrentCulture(), out newValue))
                {
                    MaxValueTextBox.SetValue(ToolTipService.ToolTipProperty, LocalizableStrings.InvalidClassBreakValue);
                    VisualStateManager.GoToState(MaxValueTextBox, "InvalidUnfocused", true);
                    MaxValueTextBox.Focus();                    
                    return;
                }

                double min, max;
                if (!isMinLessThanMax(out min, out max))
                {
                    double incr = 1.0;
                    if (Math.Round(min) != Math.Round(min,4))
                    {
                        incr = 0.01;
                    }
                    newValue = min + incr;                    
                }

                MaxValueTextBox.Visibility = System.Windows.Visibility.Collapsed;
            }            

            VisualStateManager.GoToState(MinValueTextBox, "Valid", true);
            MaxValueTextBox.SetValue(ToolTipService.ToolTipProperty, null);            

            if (MaxValueTextBlock != null)
                MaxValueTextBlock.Visibility = System.Windows.Visibility.Visible;

            if (newValue == ClassBreak.MaximumValue) // no changes detected
                return;

            UpdateMaxValue(newValue);

            OnClassBreakModified(new ClassBreakModificationEventArgs() { ClassBreakModificationType = ClassBreakModificationType.MaxValueChanged });
        }

        void MaxValueTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
			MaxValueTextBlock_Click();
			base.OnMouseLeftButtonUp(e);
        }

		internal void MaxValueTextBlock_Click()
		{
			// Show the max value textbox for editing
			if (MaxValueTextBox != null)
			{
				MaxValueTextBox.Visibility = System.Windows.Visibility.Visible;
				MaxValueTextBox.SelectAll();
				MaxValueTextBox.Focus();
			}

			// Hide the max value label
			if (MaxValueTextBlock != null)
				MaxValueTextBlock.Visibility = System.Windows.Visibility.Collapsed;

			// ensure min value is not in edit mode
			if (MinValueTextBlock != null)
				MinValueTextBlock.Visibility = System.Windows.Visibility.Visible;

			// ensure min value is not in edit mode
			if (MinValueTextBox != null)
				MinValueTextBox.Visibility = System.Windows.Visibility.Collapsed;
		}        

        internal void MinValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            onMinValueEntered();
			base.OnLostFocus(e);
        }            

        void MinValueTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
			MinValueTextBlock_Click();
			base.OnMouseLeftButtonUp(e);
        }

		internal void MinValueTextBlock_Click()
		{
			//Show the min value textbox for editing
			if (MinValueTextBox != null)
			{
				MinValueTextBox.Visibility = System.Windows.Visibility.Visible;
				MinValueTextBox.SelectAll();
				MinValueTextBox.Focus();
			}

			// Hide the min value label
			if (MinValueTextBlock != null)
				MinValueTextBlock.Visibility = System.Windows.Visibility.Collapsed;

			// Ensure max value is not in edit mode
			if (MaxValueTextBlock != null)
				MaxValueTextBlock.Visibility = System.Windows.Visibility.Visible;

			// Ensure max value is not in edit mode
			if (MaxValueTextBox != null)
				MaxValueTextBox.Visibility = System.Windows.Visibility.Collapsed;
		}

        void SymbolSelector_SymbolChanged(object sender, SymbolSelectedEventArgs e)
        {
            if (ClassBreak != null)
            {                
                ClassBreak.Symbol = e.Symbol;
            }

            if (SymbolDisplay != null)
                SymbolDisplay.Symbol = e.Symbol;            

            OnClassBreakModified(new ClassBreakModificationEventArgs() { ClassBreakModificationType = ClassBreakModificationType.SymbolChanged });
        }

        private bool isMinLessThanMax(out double minValue, out double maxValue)
        {
            double d;
            minValue = double.NaN;
            if (MinValueTextBox != null)
            {
                if (double.TryParse(MinValueTextBox.Text, System.Globalization.NumberStyles.Any, CultureHelper.GetCurrentCulture(), out d))
                    minValue = d;
            }
            maxValue = double.NaN;
            if (MaxValueTextBox != null)
            {
                if (double.TryParse(MaxValueTextBox.Text, System.Globalization.NumberStyles.Any, CultureHelper.GetCurrentCulture(), out d))
                    maxValue = d;
            }
            if (!double.IsNaN(minValue) && !double.IsNaN(maxValue))
            {
                return minValue < maxValue;
            }
            return false;
        }

        #region ClassBreak
        /// <summary>
        /// 
        /// </summary>
        public ClassBreakInfo ClassBreak
        {
            get { return GetValue(ClassBreakProperty) as ClassBreakInfo; }
            set { SetValue(ClassBreakProperty, value); }
        }

        /// <summary>
        /// Identifies the ClassBreak dependency property.
        /// </summary>
        public static readonly DependencyProperty ClassBreakProperty =
            DependencyProperty.Register(
                "ClassBreak",
                typeof(ClassBreakInfo),
                typeof(ClassBreakConfigControl),
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
                typeof(ClassBreakConfigControl),
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
                typeof(ClassBreakConfigControl),
                new PropertyMetadata(null));
        #endregion                       

        public void UpdateMaxValue(double maxValue)
        {
            if (ClassBreak != null)
                ClassBreak.MaximumValue = maxValue;

            if (MaxValueTextBlock != null)
                MaxValueTextBlock.Text = maxValue.ToString();

            if (MaxValueTextBox != null)
                MaxValueTextBox.Text = maxValue.ToString();
        }

        public void UpdateMinValue(double minValue)
        {
            if (ClassBreak != null)
                ClassBreak.MinimumValue = minValue;

            if (MinValueTextBlock != null)
                MinValueTextBlock.Text = minValue.ToString();

            if (MinValueTextBox != null)
                MinValueTextBox.Text = minValue.ToString();
        }

        public void StopEditMode()
        {
            if (MinValueTextBox != null)
                MinValueTextBox.Visibility = System.Windows.Visibility.Collapsed;

            if (MinValueTextBlock != null)
                MinValueTextBlock.Visibility = System.Windows.Visibility.Visible;

            if (MaxValueTextBox != null)
                MaxValueTextBox.Visibility = System.Windows.Visibility.Collapsed;

            if (MaxValueTextBlock != null)
                MaxValueTextBlock.Visibility = System.Windows.Visibility.Visible;
        }

        protected virtual void OnClassBreakModified(ClassBreakModificationEventArgs args)
        {
            if (ClassBreakModified != null)
                ClassBreakModified(this, args);
        }

        public event EventHandler<ClassBreakModificationEventArgs> ClassBreakModified;
    }

    public class ClassBreakModificationEventArgs : EventArgs
    {
        public ClassBreakModificationType ClassBreakModificationType { get; set; }
    }    

    public enum ClassBreakModificationType
    {
        MinValueChanged,
        MaxValueChanged,
        SymbolChanged,
    }   
}
