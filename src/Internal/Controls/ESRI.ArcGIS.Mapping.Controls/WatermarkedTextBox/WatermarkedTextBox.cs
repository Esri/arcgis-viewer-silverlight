/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ESRI.ArcGIS.Mapping.Controls
{
    /// <summary>
    /// WatermarkedTextBox is a specialized form of TextBox which displays custom visuals when its contents are empty
    /// </summary>
    [TemplatePart(Name = WatermarkedTextBox.ElementContentName, Type = typeof(ContentControl))]
    [TemplateVisualState(Name = VisualStateHelper.StateNormal, GroupName = VisualStateHelper.GroupCommon)]
    [TemplateVisualState(Name = VisualStateHelper.StateMouseOver, GroupName = VisualStateHelper.GroupCommon)]
    [TemplateVisualState(Name = VisualStateHelper.StateDisabled, GroupName = VisualStateHelper.GroupCommon)]
    [TemplateVisualState(Name = VisualStateHelper.StateUnfocused, GroupName = VisualStateHelper.GroupFocus)]
    [TemplateVisualState(Name = VisualStateHelper.StateFocused, GroupName = VisualStateHelper.GroupFocus)]
    [TemplateVisualState(Name = VisualStateHelper.StateUnwatermarked, GroupName = VisualStateHelper.GroupWatermark)]
    [TemplateVisualState(Name = VisualStateHelper.StateWatermarked, GroupName = VisualStateHelper.GroupWatermark)]
    public partial class WatermarkedTextBox : TextBox
    {
        #region Constants
        private const string ElementContentName = "Watermark";
        private const string TemplateXamlPath = "ESRI.ArcGIS.Mapping.Controls.Themes.WatermarkedTextBox.Theme.xaml";
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="WatermarkedTextBox"/> class.
        /// </summary>
        public WatermarkedTextBox()
        {
            this.DefaultStyleKey = typeof(WatermarkedTextBox);
            SetDefaults();

            this.MouseEnter += OnMouseEnter;
            this.MouseLeave += OnMouseLeave;
            this.Loaded += OnLoaded;
            this.LostFocus += OnLostFocus;
            this.GotFocus += OnGotFocus;
            this.TextChanged += OnTextChanged;
			this.IsEnabledChanged += OnIsEnabledPropertyChanged;
        }
        #endregion

        #region Internal

        internal ContentControl elementContent;
        internal bool isHovered;
        internal bool hasFocus;

        //this method is made 'internal virtual' so the a TestWatermarkedTextBox with custom verification code
        //that executes in OnLoaded could be created
        internal virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
                ApplyTemplate();
                ChangeVisualState(false);
        }

        /// <summary>
        /// Change to the correct visual state for the textbox.
        /// </summary>
        internal void ChangeVisualState()
        {
            ChangeVisualState(true);
        }

        /// <summary>
        /// Change to the correct visual state for the textbox.
        /// </summary>
        /// <param name="useTransitions">
        /// true to use transitions when updating the visual state, false to
        /// snap directly to the new visual state.
        /// </param>
        internal void ChangeVisualState(bool useTransitions)
        {
            // Update the CommonStates group
            if (!IsEnabled)
            {
                VisualStateHelper.GoToState(this, useTransitions, VisualStateHelper.StateDisabled, VisualStateHelper.StateNormal);
            }
            else if (isHovered)
            {
                VisualStateHelper.GoToState(this, useTransitions, VisualStateHelper.StateMouseOver, VisualStateHelper.StateNormal);
            }
            else
            {
                VisualStateHelper.GoToState(this, useTransitions, VisualStateHelper.StateNormal);
            }

            // Update the FocusStates group
            if (hasFocus && IsEnabled)
            {
                VisualStateHelper.GoToState(this, useTransitions, VisualStateHelper.StateFocused, VisualStateHelper.StateUnfocused);
            }
            else
            {
                VisualStateHelper.GoToState(this, useTransitions, VisualStateHelper.StateUnfocused);
            }

            // Update the WatermarkStates group
            if (!hasFocus && this.WatermarkText != null && string.IsNullOrEmpty(this.Text))
            {
                VisualStateHelper.GoToState(this, useTransitions, VisualStateHelper.StateWatermarked, VisualStateHelper.StateUnwatermarked);
            }
            else
            {
                VisualStateHelper.GoToState(this, useTransitions, VisualStateHelper.StateUnwatermarked);
            }
        }
        #endregion

        #region Public

        /// <summary>
        /// Called when template is applied to the control.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            elementContent = ExtractTemplatePart<ContentControl>(ElementContentName);

            ChangeVisualState(false);
        }

		#region WatermarkText
		/// <summary>
        /// Watermark dependency property
        /// </summary>
		public static readonly DependencyProperty WatermarkTextProperty = DependencyProperty.Register(
			"WatermarkText", typeof(string), typeof(WatermarkedTextBox), new PropertyMetadata(OnWatermarkPropertyChanged));

        /// <summary>
        /// Watermark content
        /// </summary>
        /// <value>The watermark.</value>
        public string WatermarkText
        {
			get { return (string)GetValue(WatermarkTextProperty); }
            set { SetValue(WatermarkTextProperty, value); }
        }

        #endregion

        #endregion

        #region Private

        private T ExtractTemplatePart<T>(string partName) where T : DependencyObject
        {
            DependencyObject obj = GetTemplateChild(partName);
            return ExtractTemplatePart<T>(partName, obj);
        }

        private static T ExtractTemplatePart<T>(string partName, DependencyObject obj) where T : DependencyObject
        {
            Debug.Assert(obj == null || typeof(T).IsInstanceOfType(obj), 
            string.Format(CultureInfo.InvariantCulture, "Template part is of incorrect type.", partName, typeof(T).Name));
            return obj as T;
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (IsEnabled)
            {
                hasFocus = true;

                if (!string.IsNullOrEmpty(this.Text))
                {
                    Select(0, this.Text.Length);
                }

                ChangeVisualState();
            }
        }

        /// <summary>
        /// Called when is enabled property is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnIsEnabledPropertyChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            WatermarkedTextBox watermarkedTextBox = this;
            Debug.Assert(watermarkedTextBox != null, "The source is not an instance of a WatermarkedTextBox!");
            bool newValue = (bool)args.NewValue;

            watermarkedTextBox.IsHitTestVisible = newValue;
            watermarkedTextBox.IsTabStop = newValue;
            watermarkedTextBox.IsReadOnly = !newValue;

            watermarkedTextBox.ChangeVisualState();
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            hasFocus = false;
            ChangeVisualState();
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            isHovered = true;

            if (!hasFocus)
            {
                ChangeVisualState();
            }
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            isHovered = false;

            ChangeVisualState();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeVisualState();
        }

        /// <summary>
        /// Called when watermark property is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnWatermarkPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            WatermarkedTextBox watermarkTextBox = sender as WatermarkedTextBox;
            Debug.Assert(watermarkTextBox != null, "The source is not an instance of a WatermarkedTextBox!");
            watermarkTextBox.ChangeVisualState();
        }

        private void SetDefaults()
        {
            IsEnabled = true;
			this.WatermarkText = "";
        }

        #endregion
    }
}
