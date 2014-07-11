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
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace ESRI.ArcGIS.Client.Application.Controls
{
    /// <summary>
    /// DropDownButton similar to the ComboBox control, but with fully customizable
    /// content.
    /// </summary>
    public class DropDownButton : Button
    {
        private Canvas ElementOutsidePopup;
        private Canvas ElementPopupChildCanvas;
        private Popup ElementPopup;
        private FrameworkElement ElementPopupChild;
        private ContentControl LeaderPopupContent;
        private StackPanel ElementPopupChildMaxRangeStackPanel;
        /// <summary>
        /// Initializes a new instance of the <see cref="DropDownButton"/> class.
        /// </summary>
        public DropDownButton()
        {
            DefaultStyleKey = typeof(DropDownButton);
            this.SizeChanged += DropDownButton_SizeChanged;
        }

        #region PopupContent
        /// <summary>
        /// Sets the content in the dropdown
        /// </summary>
        public FrameworkElement PopupContent
        {
            get { return GetValue(PopupContentProperty) as FrameworkElement; }
            set { SetValue(PopupContentProperty, value); }
        }

        /// <summary>
        /// Identifies the Content dependency property.
        /// </summary>
        public static readonly DependencyProperty PopupContentProperty =
            DependencyProperty.Register(
                "PopupContent",
                typeof(UIElement),
                typeof(DropDownButton),
                new PropertyMetadata(OnPopupContentPropertyChanged));

        private static void OnPopupContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DropDownButton ddb = d as DropDownButton;
            var canvas = ddb.ElementPopupChildCanvas;
            if (e.OldValue is UIElement)
            {
                (e.OldValue as FrameworkElement).SizeChanged -= ddb.DropDownButton_SizeChanged;
                if (canvas != null && (canvas.Children.Contains(e.OldValue as UIElement)))
                    canvas.Children.Remove(e.OldValue as UIElement);
            }
            if (ddb != null && ddb.Command != null && !ddb.Command.CanExecute(ddb.CommandParameter))
            {
                //do not add UI unless can be executed
                return;
            }

            if (e.NewValue is UIElement)
            {
                (e.NewValue as FrameworkElement).SizeChanged += ddb.DropDownButton_SizeChanged;
                if (canvas != null)
                    canvas.Children.Add(e.NewValue as UIElement);
            }
        }
        #endregion public UIElement Content

        #region IsContentPopupOpen

        /// <summary>
        /// Sets the open state of the dropdown
        /// </summary>
        public bool IsContentPopupOpen
        {
            get { return (bool)GetValue(IsContentPopupOpenProperty); }
            set { SetValue(IsContentPopupOpenProperty, value); }
        }

        /// <summary>
        /// Identifies the IsContentPopupOpen dependency property.
        /// </summary>
        public static readonly DependencyProperty IsContentPopupOpenProperty =
            DependencyProperty.Register(
                "IsContentPopupOpen",
                typeof(bool),
                typeof(DropDownButton),
                new PropertyMetadata(false, OnIsContentPopupOpenPropertyChanged));

        /// <summary>
        /// IsContentPopupOpenProperty property changed handler.
        /// </summary>
        /// <param name="d">DropDownButton that changed its IsContentPopupOpen.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnIsContentPopupOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DropDownButton source = d as DropDownButton;
            source.OnIsContentPopupOpenPropertyChanged();
        }

        internal void OnIsContentPopupOpenPropertyChanged()
        {
            if (ElementPopup != null && PopupContent != null)
            {
                Panel panel = PopupContent as Panel;
                // if the popup content is a panel, it needs to have at least one child in order to be shown.
                // This prevents showing empty pop-ups - at least in the case where the pop-up content is a panel.
                if (panel == null || (panel != null && panel.Children.Count > 0))
                    ElementPopup.IsOpen = IsContentPopupOpen;
            }

            if (IsContentPopupOpen)
                this.OnOpening(EventArgs.Empty);

            ChangeVisualState(true);
        }

        #endregion IsContentPopupOpen

        #region ShowLeader
        /// <summary>
        /// 
        /// </summary>
        public bool ShowLeader
        {
            get { return (bool)GetValue(ShowLeaderProperty); }
            set { SetValue(ShowLeaderProperty, value); }
        }

        /// <summary>
        /// Identifies the PopupContentContainerStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowLeaderProperty =
            DependencyProperty.Register(
                "ShowLeader",
                typeof(bool),
                typeof(DropDownButton),
                new PropertyMetadata(false));
        #endregion

        #region PopupContentContainerStyle
        /// <summary>
        /// 
        /// </summary>
        public Style PopupContentContainerStyle
        {
            get { return GetValue(PopupContentContainerStyleProperty) as Style; }
            set { SetValue(PopupContentContainerStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the PopupContentContainerStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty PopupContentContainerStyleProperty =
            DependencyProperty.Register(
                "PopupContentContainerStyle",
                typeof(Style),
                typeof(DropDownButton),
                new PropertyMetadata(null));
        #endregion

        #region PopupLeaderStyle
        /// <summary>
        /// 
        /// </summary>
        public Style PopupLeaderStyle
        {
            get { return GetValue(PopupLeaderStyleProperty) as Style; }
            set { SetValue(PopupLeaderStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the PopupContentContainerStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty PopupLeaderStyleProperty =
            DependencyProperty.Register(
                "PopupLeaderStyle",
                typeof(Style),
                typeof(DropDownButton),
                new PropertyMetadata(null));
        #endregion

        #region public ExpandDirection ExpandDirection

        /// <summary>
        /// Sets the open state of the dropdown
        /// </summary>
        public ExpandDirection ExpandDirection
        {
            get { return (ExpandDirection)GetValue(ExpandDirectionProperty); }
            set { SetValue(ExpandDirectionProperty, value); }
        }

        /// <summary>
        /// Identifies the IsContentPopupOpen dependency property.
        /// </summary>
        public static readonly DependencyProperty ExpandDirectionProperty =
            DependencyProperty.Register(
                "ExpandDirection",
                typeof(ExpandDirection),
                typeof(DropDownButton),
                new PropertyMetadata(ExpandDirection.BottomLeft));

        #endregion public ExpandDirection ExpandDirection

        #region public bool EnforceMinWidth
        public bool EnforceMinWidth
        {
            get { return (bool)GetValue(EnforceMinWidthProperty); }
            set { SetValue(EnforceMinWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the EnforeMinWidth dependency property.
        /// </summary>
        public static readonly DependencyProperty EnforceMinWidthProperty =
            DependencyProperty.Register(
                "EnforceMinWidth",
                typeof(bool),
                typeof(DropDownButton),
                new PropertyMetadata(true));
        #endregion public bool EnforceMinWidth

        /// <summary>
        /// Builds the visual tree for the <see cref="T:System.Windows.Controls.Button"/>
        /// when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.ElementPopup = base.GetTemplateChild("Popup") as Popup;
            if (this.ElementPopup != null)
            {
                ElementPopupChild = this.ElementPopup.Child as FrameworkElement;
                ElementOutsidePopup = new Canvas();
                if (ExpandDirection == ExpandDirection.HorizontalCenter)
                    ElementPopupChildMaxRangeStackPanel = new StackPanel();
            }
            else
            {
                ElementOutsidePopup = null;
                ElementPopupChildMaxRangeStackPanel = null;
            }
            if (ElementOutsidePopup != null)
            {
                ElementOutsidePopup.Background = new SolidColorBrush(Colors.Transparent);
                ElementOutsidePopup.MouseLeftButtonDown -= ElementOutsidePopup_MouseLeftButtonDown;
                ElementOutsidePopup.MouseLeftButtonDown += ElementOutsidePopup_MouseLeftButtonDown;
            }
            if (ElementPopupChildMaxRangeStackPanel != null)
            {
                ElementPopupChildMaxRangeStackPanel.Background = new SolidColorBrush(Colors.Transparent);
                ElementPopupChildMaxRangeStackPanel.MouseLeftButtonDown -= ElementOutsidePopup_MouseLeftButtonDown;
                ElementPopupChildMaxRangeStackPanel.MouseLeftButtonDown += ElementOutsidePopup_MouseLeftButtonDown;
            }
            if (ElementPopupChild != null)
            {
                ElementPopupChildCanvas = new Canvas() { Background = new SolidColorBrush(Colors.Transparent) };
                ElementPopupChild.MouseLeftButtonDown -= ElementChild_MouseLeftButtonDown;
                ElementPopupChild.MouseLeftButtonDown += ElementChild_MouseLeftButtonDown;
                ElementPopupChild.SizeChanged -= ElementPopupChild_SizeChanged;
                ElementPopupChild.SizeChanged += ElementPopupChild_SizeChanged;
            }
            else
                ElementPopupChildCanvas = null;

            if (ShowLeader)
            {
                LeaderPopupContent = new ContentControl();
                LeaderPopupContent.Style = PopupLeaderStyle;
            }

            if (ElementPopupChildCanvas != null && ElementOutsidePopup != null)
            {
                ElementPopup.Child = ElementPopupChildCanvas;
                if (ElementPopupChildMaxRangeStackPanel != null)
                {
                    ElementPopupChildCanvas.Children.Add(ElementOutsidePopup);
                    ElementPopupChildMaxRangeStackPanel.Children.Add(ElementPopupChild);
                    ElementPopupChildCanvas.Children.Add(ElementPopupChildMaxRangeStackPanel);
                }
                else
                {
                    ElementPopupChildCanvas.Children.Add(ElementOutsidePopup);
                    ElementPopupChildCanvas.Children.Add(ElementPopupChild);
                }
                if (LeaderPopupContent != null)
                {
                    ElementPopupChildCanvas.Children.Add(LeaderPopupContent);
                }
            }
            ChangeVisualState(true);
        }

        void ElementPopupChild_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsContentPopupOpen)
                this.ArrangePopup();
        }

        private void ElementOutsidePopup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.IsContentPopupOpen = false;
            e.Handled = false;
        }
        private void ElementChild_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Arranges the override.
        /// </summary>
        /// <param name="arrangeBounds">The arrange bounds.</param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            Size size = base.ArrangeOverride(arrangeBounds);
            if(IsContentPopupOpen)
                this.ArrangePopup();
            return size;
        }

        /// <summary>
        /// Handles the SizeChanged event of the DropDownButton control and
        /// repositions the popup.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
        private void DropDownButton_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(IsContentPopupOpen)
                this.ArrangePopup();
        }

        private void ChangeVisualState(bool useTransitions)
        {
            if (IsContentPopupOpen)
            {
                GoToState(useTransitions, "PopupOpen");
            }
            else
            {
                GoToState(useTransitions, "PopupClosed");
            }
        }

        private bool GoToState(bool useTransitions, string stateName)
        {
            return VisualStateManager.GoToState(this, stateName, useTransitions);
        }

        private void ArrangePopup()
        {
            if (PopupContent != null)
            {
                try
                {
                    PopupManager.Instance.ArrangePopup(ElementPopup, 
                        ElementPopupChildCanvas, 
                        ElementOutsidePopup, 
                        ElementPopupChild, 
                        LeaderPopupContent, 
                        ElementPopupChildMaxRangeStackPanel, 
                        this, 
                        ExpandDirection, 
                        EnforceMinWidth);
                }
                catch
                {
                    IsContentPopupOpen = false;
                }
            }
        }

        /// <summary>
        /// Toggles the open state and raises the 
        /// <see cref="E:System.Windows.Controls.Primitives.ButtonBase.Click"/> event.
        /// </summary>
        protected override void OnClick()
        {
            IsContentPopupOpen = !IsContentPopupOpen;
            base.OnClick();
        }

        /// <summary>
        /// Provides class handling for the <see cref="E:System.Windows.UIElement.KeyDown"/> 
        /// event that occurs when the user presses a key while this control has focus.
        /// </summary>
        /// <param name="e">The event data.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="e"/> is null.</exception>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if ((Key.Down == e.Key) || (Key.Up == e.Key))
            {
                IsContentPopupOpen = !IsContentPopupOpen;
            }
            else
            {
                base.OnKeyDown(e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:Opening"/> event.
        /// </summary>
        /// <param name="args">The <see cref="System.EventArgs"/> instance containing
        /// the event data.</param>
        protected virtual void OnOpening(EventArgs args)
        {
            if (Opening != null)
                Opening(this, args);
        }

        /// <summary>
        /// Occurs when the dropdown opens.
        /// </summary>
        public event EventHandler Opening;
    }
}
