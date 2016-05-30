/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

// This source is subject to the Microsoft Public License (Ms-PL).
// Please see https://opensource.org/licenses/ms-pl for details.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ESRI.ArcGIS.Client.Application.Layout;
using System.Windows.Markup;
using System.Globalization;

namespace ESRI.ArcGIS.Client.Application.Controls
{
    /// <summary>
    /// Provides a movable window to host content in
    /// </summary>
    [TemplatePart(Name = PART_Chrome, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = PART_CloseButton, Type = typeof(ButtonBase))]
    [TemplatePart(Name = PART_ContentRoot, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = PART_Root, Type = typeof(FrameworkElement))]
    [TemplateVisualState(Name = VSMSTATE_StateClosing, GroupName = VSMGROUP_Window)]
    [TemplateVisualState(Name = VSMSTATE_StateOpen, GroupName = VSMGROUP_Window)]
    [TemplateVisualState(Name = VSMSTATE_StateOpening, GroupName = VSMGROUP_Window)]
    public class FloatingWindow : ContentControl
    {
        #region Static Fields and Constants

        /// <summary>
        /// The name of the Chrome template part.
        /// </summary>
        private const string PART_Chrome = "Chrome";

        /// <summary>
        /// The name of the CloseButton template part.
        /// </summary>
        private const string PART_CloseButton = "CloseButton";

        /// <summary>
        /// The name of the ContentRoot template part.
        /// </summary>
        private const string PART_ContentRoot = "ContentRoot";

        /// <summary>
        /// The name of the Overlay template part.
        /// </summary>
        private const string PART_Overlay = "Overlay";

        /// <summary>
        /// The name of the Root template part.
        /// </summary>
        private const string PART_Root = "Root";

        /// <summary>
        /// The name of the WindowStates VSM group.
        /// </summary>
        private const string VSMGROUP_Window = "WindowStates";

        /// <summary>
        /// The name of the Closing VSM state.
        /// </summary>
        private const string VSMSTATE_StateClosing = "Closing";

        /// <summary>
        /// The name of the Open VSM state.
        /// </summary>
        private const string VSMSTATE_StateOpen = "Open";

        /// <summary>
        /// The name of the Opening VSM state.
        /// </summary>
        private const string VSMSTATE_StateOpening = "Opening";

        /// <summary>
        /// Title of the ChildWindow.
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
            "Title",
            typeof(object),
            typeof(FloatingWindow),
            null);

        /// <summary>
        /// Gets the root visual element.
        /// </summary>
        private static Control RootVisual
        {
            get
            {
                return System.Windows.Application.Current == null ? null : 
                    System.Windows.Application.Current.RootVisual as Control;
            }
        }

        #endregion Static Fields and Constants

        #region Member Fields
        /// <summary>
        /// Private accessor for the Chrome.
        /// </summary>
        private FrameworkElement _chrome;

        /// <summary>
        /// Private accessor for the IsModal
        /// </summary>
        [DefaultValue(false)]
        private bool _modal;

        /// <summary>
        /// Private accessor for the click point on the chrome.
        /// </summary>
        private Point _clickPoint;

        /// <summary>
        /// Private accessor for the close button.
        /// </summary>
        private ButtonBase _closeButton;

        /// <summary>
        /// Private accessor for the Closing storyboard.
        /// </summary>
        private Storyboard _closed;

        /// <summary>
        /// Content area desired width.
        /// </summary>
        private double _desiredContentWidth;

        /// <summary>
        /// Content area desired height.
        /// </summary>
        private double _desiredContentHeight;

        /// <summary>
        /// Private accessor for the Dialog Result property.
        /// </summary>
        private bool? _dialogresult;

        /// <summary>
        /// Boolean value that specifies whether the window is in closing state or not.
        /// </summary>
        private bool _isClosing;

        /// <summary>
        /// Boolean value that specifies whether the application is exit or not.
        /// </summary>
        private bool _isAppExit;

        /// <summary>
        /// Boolean value that specifies whether the window is in opening state or not.
        /// </summary>
        private bool _isOpening;

        /// <summary>
        /// Private accessor for the Opening storyboard.
        /// </summary>
        private Storyboard _opened;

        /// <summary>
        /// Private accessor for the Overlay of the window.
        /// </summary>
        private FrameworkElement _overlay;

        /// <summary>
        /// Boolean value that specifies whether the mouse is captured or not.
        /// </summary>
        private bool _isMouseCaptured;

        /// <summary>
        /// Private accessor for the Root of the window.
        /// </summary>
        private FrameworkElement _root;

        private static int z;

        private Point _windowPosition;
        private TranslateTransform _contentRootTransform;

        #endregion Member Fields

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public FloatingWindow()
        {
            this.DefaultStyleKey = typeof(FloatingWindow);
            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.Name);
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Occurs when the window is closed.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Occurs directly after Close is called, and can be handled to cancel window closure. 
        /// </summary>
        public event EventHandler<CancelEventArgs> Closing;

        /// <summary>
        /// Occurs after the story board animation for closing the window is complete.
        /// </summary>
        private event EventHandler CloseCompleted;

        /// <summary>
        /// Occurs after the story board animation for opening the window is complete.
        /// </summary>
        private event EventHandler OpenCompleted;

        /// <summary>
        /// Occurs when the user is moving the window
        /// </summary>
        private event EventHandler<MouseEventArgs> Moving;
        #endregion Events

        #region Properties

        /// <summary>
        /// Gets the private accessor for the ContentRoot of the window.
        /// </summary>
        private FrameworkElement ContentRoot
        {
            get;
            set;
        }

        private static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register(
            "HorizontalOffset", typeof(double), typeof(FloatingWindow), 
            new PropertyMetadata(double.MinValue, OnHorizontalOffsetChanged));

        /// <summary>
        /// Gets or sets the horizontal position of the window relative to the left edge of the app
        /// </summary>
        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }

        private static void OnHorizontalOffsetChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var window = (FloatingWindow)o;
            if (window._contentRootTransform != null && RootVisual != null)
            {
                window._contentRootTransform.X = window.HorizontalOffset - RootVisual.ActualWidth / 2
                    + window.ContentRoot.ActualWidth / 2;
            }
        }


        private static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register(
            "VerticalOffset", typeof(double), typeof(FloatingWindow),
            new PropertyMetadata(double.MinValue, OnVerticalOffsetChanged));

        /// <summary>
        /// Gets or sets the vertical position of the window relative to the left edge of the app
        /// </summary>
        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        private static void OnVerticalOffsetChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var window = (FloatingWindow)o;
            if (window._contentRootTransform != null && RootVisual != null)
            {
                window._contentRootTransform.Y = window.VerticalOffset - RootVisual.ActualHeight / 2
                    + window.ContentRoot.ActualHeight / 2;
            }
        }

        /// <summary>
        /// Gets the private accessor for the modal of the window.
        /// </summary>
        private bool IsModal
        {
            get
            {
                return _modal;
            }
        }

        /// <summary>
        /// Gets or sets the DialogResult property.
        /// </summary>
        private bool? DialogResult
        {
            get
            {
                return this._dialogresult;
            }

            set
            {
                if (this._dialogresult != value)
                {
                    this._dialogresult = value;
                    this.Hide();
                }
            }
        }

        /// <summary>
        /// Gets the private accessor for the PopUp of the window.
        /// </summary>
        private Popup ChildWindowPopup
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Title property.
        /// </summary>
        public object Title
        {
            get { return (object)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the PopUp is open or not.
        /// </summary>
        private bool IsOpen
        {
            get
            {
                return (this.ChildWindowPopup != null && this.ChildWindowPopup.IsOpen);
            }
        }

        /// <summary>
        /// Backing dependency property for OverlayBrush.  Enables binding on this property.
        /// </summary>
        public static DependencyProperty OverlayBrushProperty = DependencyProperty.Register(
            "OverlayBrush", typeof(Brush), typeof(FloatingWindow), null);

        /// <summary>
        /// Gets or sets the brush used to cover the parent window when the child window is open.
        /// </summary>
        public Brush OverlayBrush
        {
            get { return GetValue(OverlayBrushProperty) as Brush; }
            set { SetValue(OverlayBrushProperty, value); }
        }

        #endregion Properties

        #region Static Methods

        private static double FindPositionX(Point p1, Point p2, double y)
        {
            if ((y != p1.Y) && (p1.X != p2.X))
            {
                return ((((y - p1.Y) * (p1.X - p2.X)) / (p1.Y - p2.Y)) + p1.X);
            }
            return p2.X;
        }

        private static double FindPositionY(Point p1, Point p2, double x)
        {
            if ((p1.Y != p2.Y) && (x != p1.X))
            {
                return ((((p1.Y - p2.Y) * (x - p1.X)) / (p1.X - p2.X)) + p1.Y);
            }
            return p2.Y;
        }


        /// <summary>
        /// Inverts the input matrix.
        /// </summary>
        /// <param name="matrix">The matrix values that is to be inverted.</param>
        /// <returns>Returns a value indicating whether the inversion was successful or not.</returns>
        private static bool InvertMatrix(ref Matrix matrix)
        {
            double determinant = (matrix.M11 * matrix.M22) - (matrix.M12 * matrix.M21);

            if (determinant == 0.0)
            {
                return false;
            }

            Matrix matCopy = matrix;
            matrix.M11 = matCopy.M22 / determinant;
            matrix.M12 = -1 * matCopy.M12 / determinant;
            matrix.M21 = -1 * matCopy.M21 / determinant;
            matrix.M22 = matCopy.M11 / determinant;
            matrix.OffsetX = ((matCopy.OffsetY * matCopy.M21) - (matCopy.OffsetX * matCopy.M22)) / determinant;
            matrix.OffsetY = ((matCopy.OffsetX * matCopy.M12) - (matCopy.OffsetY * matCopy.M11)) / determinant;

            return true;
        }

        #endregion Static Methods

        #region Methods

        /// <summary>
        /// Executed when the application is exited.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Event args.</param>
        private void Application_Exit(object sender, EventArgs e)
        {
            if (this.IsOpen)
            {
                this._isAppExit = true;
                try
                {
                    this.Hide();
                }
                finally
                {
                    this._isAppExit = false;
                }
            }
        }

        /// <summary>
        /// Executed when focus is given to the window via a click.  Attempts to bring current 
        /// window to the front in the event there are more windows.
        /// </summary>
        private void BringToFront()
        {
            z++;
            Canvas.SetZIndex(this, z);
        }

        /// <summary>
        /// Changes the visual state of the ChildWindow.
        /// </summary>
        private void ChangeVisualState()
        {
            if (this._isClosing)
            {
                if (!VisualStateManager.GoToState(this, VSMSTATE_StateClosing, false))
                {
                    this.OnClosed(EventArgs.Empty);
                }
            }
            else
            {
                if (this._isOpening)
                {
                    VisualStateManager.GoToState(this, VSMSTATE_StateOpening, false);
                }
                else
                {
                    VisualStateManager.GoToState(this, VSMSTATE_StateOpen, false);
                    BringToFront();
                }
            }
        }

        /// <summary>
        /// Executed when ChildWindow size is changed.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Size changed event args.</param>
        private void ChildWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_modal)
            {
                if (this._overlay != null)
                {
                    if (e.NewSize.Height != this._overlay.Height)
                    {
                        this._desiredContentHeight = e.NewSize.Height;
                    }

                    if (e.NewSize.Width != this._overlay.Width)
                    {
                        this._desiredContentWidth = e.NewSize.Width;
                    }
                }

                if (this.IsOpen)
                {
                    this.UpdateOverlaySize();
                }
            }
        }

        /// <summary>
        /// Closes a Window
        /// </summary>
        public void Hide(bool animate = true)
        {
            CancelEventArgs e = new CancelEventArgs();

            this.OnClosing(e);

            // On ApplicationExit, close() cannot be cancelled
            if (!e.Cancel || this._isAppExit)
            {
                // Close Popup
                if (this.IsOpen)
                {
                    if (this._closed != null && animate)
                    {
                        // Popup will be closed when the storyboard ends
                        this._isClosing = true;
                        try
                        {
                            this.ChangeVisualState();
                        }
                        catch
                        {
                            this.OnClosed(EventArgs.Empty);
                        }
                        finally
                        {
                            this._isClosing = false;
                        }
                    }
                    else
                    {
                        // If no closing storyboard is defined, close the Popup
                        this.ChildWindowPopup.IsOpen = false;
                    }

                    if (!this._dialogresult.HasValue)
                    {
                        // If close action is not happening because of DialogResult property change action,
                        // Dialogresult is always false:
                        this._dialogresult = false;
                    }

                    this.UnSubscribeFromEvents();
                    this.UnsubscribeFromTemplatePartEvents();

                    //TODO: See if this matters for FloatableWindow
                    if (System.Windows.Application.Current.RootVisual != null)
                    {
                        System.Windows.Application.Current.RootVisual.GotFocus -= this.RootVisual_GotFocus;
                    }

                    if (!animate)
                    {
                        this.OnClosed(EventArgs.Empty);
                        if (CloseCompleted != null)
                            CloseCompleted(this, e);
                    }
                }
            }
            else
            {
                // If the Close is cancelled, DialogResult should always be NULL:
                this._dialogresult = null;
            }
        }

        /// <summary>
        /// Brings the window to the front of others
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentRoot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BringToFront();
        }

        /// <summary>
        /// Executed when the CloseButton is clicked.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Routed event args.</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        /// <summary>
        /// Executed when the Closing storyboard ends.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event args.</param>
        private void Closing_Completed(object sender, EventArgs e)
        {
            if (this.ChildWindowPopup != null)
            {
                this.ChildWindowPopup.IsOpen = false;
            }

            if (this._closed != null)
            {
                this._closed.Completed -= this.Closing_Completed;
            }

            this.OnClosed(EventArgs.Empty);

            if (CloseCompleted != null)
                CloseCompleted(this, e);
        }

        /// <summary>
        /// Executed when the a key is presses when the window is open.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Key event args.</param>
        private void ChildWindow_KeyDown(object sender, KeyEventArgs e)
        {
            FloatingWindow ew = sender as FloatingWindow;
            Debug.Assert(ew != null, "FloatableWindow instance is null.");

            // Ctrl+Shift+F4 closes the FloatableWindow
            if (e != null && !e.Handled && e.Key == Key.F4 &&
                ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) &&
                ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift))
            {
                ew.Hide();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Executed when the window loses focus.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Routed event args.</param>
        private void ChildWindow_LostFocus(object sender, RoutedEventArgs e)
        {
            // If the ChildWindow loses focus but the popup is still open,
            // it means another popup is opened. To get the focus back when the
            // popup is closed, we handle GotFocus on the RootVisual
            // TODO: Something else could get focus and handle the GotFocus event right.  
            // Try listening to routed events that were Handled (new SL 3 feature)
            //TODO: See if this matters for FloatableWindow
            if (this.IsOpen && System.Windows.Application.Current != null && System.Windows.Application.Current.RootVisual != null)
            {
                System.Windows.Application.Current.RootVisual.GotFocus += this.RootVisual_GotFocus;
            }
        }

        /// <summary>
        /// Executed when mouse left button is down on the chrome.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Mouse button event args.</param>
        private void Chrome_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this._chrome != null)
            {
                this._chrome.CaptureMouse();
                this._isMouseCaptured = true;
                this._clickPoint = e.GetPosition(sender as UIElement);
            }
        }

        /// <summary>
        /// Executed when mouse left button is up on the chrome.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Mouse button event args.</param>
        private void Chrome_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this._chrome != null)
            {
                e.Handled = true;
                this._chrome.ReleaseMouseCapture();
                this._isMouseCaptured = false;
            }
        }

        /// <summary>
        /// Executed when mouse moves on the chrome.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Mouse event args.</param>
        private void Chrome_MouseMove(object sender, MouseEventArgs e)
        {
            if (this._isMouseCaptured && this.ContentRoot != null)
            {
                // If the child window is dragged out of the page, return
                if (System.Windows.Application.Current != null && System.Windows.Application.Current.RootVisual != null &&
                    (e.GetPosition(System.Windows.Application.Current.RootVisual).X < 0 || e.GetPosition(System.Windows.Application.Current.RootVisual).Y < 0))
                {
                    return;
                }
                Point position = e.GetPosition(System.Windows.Application.Current.RootVisual);
                GeneralTransform transform = this.ContentRoot.TransformToVisual(System.Windows.Application.Current.RootVisual);
                if (transform != null)
                {
                    Point point2 = transform.Transform(this._clickPoint);
                    this._windowPosition = transform.Transform(new Point(0.0, 0.0));
                    if (position.X < 0.0)
                    {
                        double num = FindPositionY(point2, position, 0.0);
                        position = new Point(0.0, num);
                    }
                    if (position.X > base.Width)
                    {
                        double num2 = FindPositionY(point2, position, base.Width);
                        position = new Point(base.Width, num2);
                    }
                    if (position.Y < 0.0)
                    {
                        double num3 = FindPositionX(point2, position, 0.0);
                        position = new Point(num3, 0.0);
                    }
                    if (position.Y > base.Height)
                    {
                        double num4 = FindPositionX(point2, position, base.Height);
                        position = new Point(num4, base.Height);
                    }
                    double x = position.X - point2.X;
                    double y = position.Y - point2.Y;
                    FrameworkElement rootVisual = System.Windows.Application.Current.RootVisual as FrameworkElement;
                    if ((rootVisual != null) && (rootVisual.FlowDirection == FlowDirection.RightToLeft))
                    {
                        x = -x;
                    }
                    this.UpdateContentRootTransform(x, y);
                }

                if (Moving != null)
                    Moving(this, e);
            }
        }

        private void UpdateContentRootTransform(double X, double Y)
        {
            HorizontalOffset += X;
            VerticalOffset += Y;
        }

        /// <summary>
        /// When the template is applied, this loads all the template parts.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "No need to split the code into two parts.")]
        public override void OnApplyTemplate()
        {
            this.UnsubscribeFromTemplatePartEvents();

            base.OnApplyTemplate();

            this._closeButton = GetTemplateChild(PART_CloseButton) as ButtonBase;

            if (this._closed != null)
            {
                this._closed.Completed -= this.Closing_Completed;
            }

            if (this._opened != null)
            {
                this._opened.Completed -= this.Opening_Completed;
            }

            this._root = GetTemplateChild(PART_Root) as FrameworkElement;

            if (this._root != null)
            {
                Collection<VisualStateGroup> groups = VisualStateManager.GetVisualStateGroups(this._root) as Collection<VisualStateGroup>;

                if (groups != null)
                {
                    System.Collections.IList states = (from stategroup in groups
                                                       where stategroup.Name == FloatingWindow.VSMGROUP_Window
                                                       select stategroup.States).FirstOrDefault();
                    Collection<VisualState> statesCol = states as Collection<VisualState>;

                    if (statesCol != null)
                    {
                        this._closed = (from state in statesCol
                                        where state.Name == FloatingWindow.VSMSTATE_StateClosing
                                        select state.Storyboard).FirstOrDefault();

                        this._opened = (from state in statesCol
                                        where state.Name == FloatingWindow.VSMSTATE_StateOpening
                                        select state.Storyboard).FirstOrDefault();
                    }
                }
            }

            this.ContentRoot = GetTemplateChild(PART_ContentRoot) as FrameworkElement;

            this._chrome = GetTemplateChild(PART_Chrome) as FrameworkElement;

            this._overlay = GetTemplateChild(PART_Overlay) as FrameworkElement;

            this.SubscribeToTemplatePartEvents();
            this.SubscribeToStoryBoardEvents();

            // Update overlay size
            if (this.IsOpen)
            {
                this._desiredContentHeight = this.Height;
                this._desiredContentWidth = this.Width;
                this.UpdateOverlaySize();
                this.UpdateRenderTransform();
                this._isOpening = true;
                try
                {
                    this.ChangeVisualState();
                }
                finally
                {
                    this._isOpening = false;
                }
            }

            // Initialize translate transform of content root to handle window positioning
            if (this._contentRootTransform == null && ContentRoot != null)
            {
                this._contentRootTransform = new TranslateTransform();

                if (RootVisual != null)
                {
                    SizeChangedEventHandler initializeOffsets = null;
                    initializeOffsets = (o, e) =>
                    {
                        // Make sure content of window has rendered before initializing position
                        if (ContentRoot.ActualWidth > 0 && ContentRoot.ActualHeight > 0)
                        {
                            // Unhook so the handler isn't invoke on subsequent resizing
                            ContentRoot.SizeChanged -= initializeOffsets;

                            // Initialize horizontal offset if it's uninitialized, otherwise initialize horizontal
                            // position of the translate transform
                            if (HorizontalOffset == double.MinValue) 
                                HorizontalOffset = RootVisual.ActualWidth / 2 - ContentRoot.ActualWidth / 2;
                            else
                                _contentRootTransform.X = HorizontalOffset - RootVisual.ActualWidth / 2 + ContentRoot.ActualWidth / 2;

                            // Initialize vertical offset if it's uninitialized, otherwise initialize vertical
                            // position of the translate transform
                            if (VerticalOffset == double.MinValue)
                                VerticalOffset = RootVisual.ActualHeight / 2 - ContentRoot.ActualHeight / 2;
                            else
                                _contentRootTransform.Y = VerticalOffset - RootVisual.ActualHeight / 2 + ContentRoot.ActualHeight / 2;
                        }
                    };

                    if (ContentRoot.ActualWidth > 0 && ContentRoot.ActualHeight > 0)
                        initializeOffsets(null, null);
                    else
                        ContentRoot.SizeChanged += initializeOffsets;
                }

                TransformGroup renderTransform = this.ContentRoot.RenderTransform as TransformGroup;
                if (renderTransform == null)
                {
                    renderTransform = new TransformGroup
                    {
                        Children = { this.ContentRoot.RenderTransform }
                    };
                }
                renderTransform.Children.Add(this._contentRootTransform);
                this.ContentRoot.RenderTransform = renderTransform;
            }        
        }

        /// <summary>
        /// Raises the Closed event. 
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnClosed(EventArgs e)
        {
            EventHandler handler = this.Closed;

            if (null != handler)
            {
                handler(this.Content, e);
            }
        }

        /// <summary>
        /// Raises the Closing event.
        /// </summary>
        /// <param name="e">A CancelEventArgs that contains the event data.</param>
        protected virtual void OnClosing(CancelEventArgs e)
        {
            EventHandler<CancelEventArgs> handler = this.Closing;

            if (null != handler)
            {
                handler(this.Content, e);
            }
        }

        /// <summary>
        /// Executed when the opening storyboard finishes.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event args.</param>
        private void Opening_Completed(object sender, EventArgs e)
        {
            this.ChangeVisualState();

            if (this._opened != null)
            {
                this._opened.Completed -= this.Opening_Completed;
            }

            if (OpenCompleted != null)
                OpenCompleted(this, EventArgs.Empty);
        }

        /// <summary>
        /// Executed when the page resizes.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event args.</param>
        private void Page_Resized(object sender, EventArgs e)
        {
            if (this.ChildWindowPopup != null)
            {
                this.UpdateOverlaySize();
            }
        }

        /// <summary>
        /// Executed when the root visual gets focus.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Routed event args.</param>
        private void RootVisual_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Focus();
        }

        /// <summary>
        /// Opens a child window. The interaction with the underlying UI is disabled but it is not a
        /// blocking call.
        /// </summary>
        public void Show(bool isModal = false)
        {
            ShowWindow(isModal);
        }

        private bool IsAppExit
        {
            get { return _isAppExit; }
        }

        private void ShowWindow(bool isModal)
        {
            if (this.IsOpen)
                return;

            _modal = isModal;
            if (this._overlay != null)
                this._overlay.Visibility = isModal ? Visibility.Visible : Visibility.Collapsed;

            this.SubscribeToEvents();
            this.SubscribeToTemplatePartEvents();
            this.SubscribeToStoryBoardEvents();

            // Set the FloatingWindow FlowDirection since it is not inherited
            RTLHelper helper = new RTLHelper();
            this.FlowDirection = helper.FlowDirection;

            if (this.ChildWindowPopup == null)
            {
                this.ChildWindowPopup = new Popup();
                this.ChildWindowPopup.Child = this;
            }

            // Margin, MaxHeight and MinHeight properties should not be overwritten:
            this.Margin = new Thickness(0);
            this.MaxHeight = double.PositiveInfinity;
            this.MaxWidth = double.PositiveInfinity;

            if (this.ChildWindowPopup != null && System.Windows.Application.Current.RootVisual != null)
            {
                this.ChildWindowPopup.IsOpen = true;

                // while the ChildWindow is open, the DialogResult is always NULL:
                this._dialogresult = null;
            }

            // if the template is already loaded, display loading visuals animation
            if (this.ContentRoot != null)
            {
                this._isOpening = true;
                try
                {
                    this.ChangeVisualState();
                }
                finally
                {
                    this._isOpening = false;
                }
            }
        }

        private void ShowDialog(bool isModal)
        {
            ShowWindow(isModal);
        }

        private void ShowDialog()
        {
            ShowDialog(true);
        }

        /// <summary>
        /// Subscribes to events when the ChildWindow is opened.
        /// </summary>
        private void SubscribeToEvents()
        {
            if (System.Windows.Application.Current != null && System.Windows.Application.Current.Host != null && System.Windows.Application.Current.Host.Content != null)
            {
                System.Windows.Application.Current.Exit += this.Application_Exit;
                System.Windows.Application.Current.Host.Content.Resized += this.Page_Resized;
            }

            this.KeyDown += this.ChildWindow_KeyDown;
            if (_modal)
            {
                this.LostFocus += this.ChildWindow_LostFocus;
            }
            this.SizeChanged += this.ChildWindow_SizeChanged;
        }

        /// <summary>
        /// Subscribes to events that are on the storyboards. 
        /// Unsubscribing from these events happen in the event handlers individually.
        /// </summary>
        private void SubscribeToStoryBoardEvents()
        {
            if (this._closed != null)
            {
                this._closed.Completed += this.Closing_Completed;
            }

            if (this._opened != null)
            {
                this._opened.Completed += this.Opening_Completed;
            }
        }

        /// <summary>
        /// Subscribes to events on the template parts.
        /// </summary>
        private void SubscribeToTemplatePartEvents()
        {
            if (this._closeButton != null)
            {
                this._closeButton.Click += this.CloseButton_Click;
            }

            if (this._chrome != null)
            {
                this._chrome.MouseLeftButtonDown += this.Chrome_MouseLeftButtonDown;
                this._chrome.MouseLeftButtonUp += this.Chrome_MouseLeftButtonUp;
                this._chrome.MouseMove += this.Chrome_MouseMove;
            }
            if (this._root != null)
            {
                this._root.MouseLeftButtonDown += this.ContentRoot_MouseLeftButtonDown;
            }

        }

        /// <summary>
        /// Unsubscribe from events when the ChildWindow is closed.
        /// </summary>
        private void UnSubscribeFromEvents()
        {
            if (System.Windows.Application.Current != null && System.Windows.Application.Current.Host != null && System.Windows.Application.Current.Host.Content != null)
            {
                System.Windows.Application.Current.Exit -= this.Application_Exit;
                System.Windows.Application.Current.Host.Content.Resized -= this.Page_Resized;
            }
            this.KeyDown -= this.ChildWindow_KeyDown;
            if (_modal)
            {
                this.LostFocus -= this.ChildWindow_LostFocus;
            }
            this.SizeChanged -= this.ChildWindow_SizeChanged;
        }

        /// <summary>
        /// Unsubscribe from the events that are subscribed on the template part elements.
        /// </summary>
        private void UnsubscribeFromTemplatePartEvents()
        {
            if (this._closeButton != null)
            {
                this._closeButton.Click -= this.CloseButton_Click;
            }

            if (this._chrome != null)
            {
                this._chrome.MouseLeftButtonDown -= this.Chrome_MouseLeftButtonDown;
                this._chrome.MouseLeftButtonUp -= this.Chrome_MouseLeftButtonUp;
                this._chrome.MouseMove -= this.Chrome_MouseMove;
            }

            if (this._root != null)
            {
                this._root.MouseLeftButtonDown -= this.ContentRoot_MouseLeftButtonDown;
            }
        }

        /// <summary>
        /// Updates the size of the overlay of the window.
        /// </summary>
        private void UpdateOverlaySize()
        {
            if (_overlay != null && System.Windows.Application.Current != null && System.Windows.Application.Current.Host != null && System.Windows.Application.Current.Host.Content != null)
            {
                _overlay.Visibility = _modal ? Visibility.Visible : Visibility.Collapsed;
                base.Height = System.Windows.Application.Current.Host.Content.ActualHeight;
                base.Width = System.Windows.Application.Current.Host.Content.ActualWidth;
                _overlay.Height = base.Height;
                _overlay.Width = base.Width;

                if (ContentRoot != null)
                {
                    ContentRoot.Width = _desiredContentWidth;
                    ContentRoot.Height = _desiredContentHeight;
                }

                if (FlowDirection == FlowDirection.RightToLeft)
                    UpdateRTLTransform();
            }
        }

        /// <summary>
        /// Updates the root RenderTransform translation to match the new overlay size (RTL only)
        /// </summary>
        private void UpdateRTLTransform()
        {
            if (FlowDirection != FlowDirection.RightToLeft)
                return;

            if (_root != null && _root.RenderTransform != null)
            {
                TransformGroup renderTransform = _root.RenderTransform as TransformGroup;
                if (renderTransform != null)
                {
                    if (renderTransform.Children != null && renderTransform.Children.Count > 0)
                    {
                        // find the last transform set for RTL
                        for (int idx = renderTransform.Children.Count - 1; idx >= 0; idx--)
                        {
                            var t = renderTransform.Children[idx] as MatrixTransform;
                            if (IsRTLTransformWithNewX(t, -_overlay.Width))
                            {
                                var transform = new MatrixTransform()
                                {
                                    Matrix = new Matrix
                                    {
                                        M11 = t.Matrix.M11,
                                        M12 = t.Matrix.M12,
                                        M21 = t.Matrix.M21,
                                        M22 = t.Matrix.M22,
                                        OffsetX = -_overlay.Width,
                                        OffsetY = t.Matrix.OffsetY,
                                    }
                                };
                                renderTransform.Children.RemoveAt(idx);
                                renderTransform.Children.Insert(idx, transform);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    var transform = _root.RenderTransform as MatrixTransform;
                    if (IsRTLTransformWithNewX(transform, -_overlay.Width))
                    {
                        _root.RenderTransform = new MatrixTransform()
                        {
                            Matrix = new Matrix
                            {
                                M11 = transform.Matrix.M11,
                                M12 = transform.Matrix.M12,
                                M21 = transform.Matrix.M21,
                                M22 = transform.Matrix.M22,
                                OffsetX = -_overlay.Width,
                                OffsetY = transform.Matrix.OffsetY,
                            }
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the transform is an RTL transform, and then if so, if it
        /// has an OffsetX value != 0 and != x
        /// </summary>
        /// <param name="t">Potential RTL transform</param>
        /// <param name="x">OffsetX value to compare</param>
        /// <returns></returns>
        private bool IsRTLTransformWithNewX(MatrixTransform t, double x)
        {
            if (t == null) return false;

            return (t.Matrix.M11.NearlyEquals(-1) && t.Matrix.M12.NearlyEquals(0) && t.Matrix.M21.NearlyEquals(0) &&
                    t.Matrix.M22.NearlyEquals(1) && !t.Matrix.OffsetX.NearlyEquals(0) && !t.Matrix.OffsetX.NearlyEquals(x));
        }

        /// <summary>
        /// Updates the render transform applied on the overlay.
        /// </summary>
        private void UpdateRenderTransform()
        {
            if (this._root != null && this.ContentRoot != null)
            {
                // The _overlay part should not be affected by the render transform applied on the
                // ChildWindow. In order to achieve this, we adjust an identity matrix to represent
                // the _root's transformation, invert it, apply the inverted matrix on the _root, so that 
                // nothing is affected by the rendertransform, and apply the original transform only on the Content
                GeneralTransform gt = this._root.TransformToVisual(null);
                if (gt != null)
                {
                    Point point = new Point(1.0, 0.0);
                    Point point2 = new Point(0.0, 1.0);
                    Point point3 = gt.Transform(point);
                    Point point4 = gt.Transform(point2);
                    Matrix identity = Matrix.Identity;
                    identity.M11 = point3.X;
                    identity.M12 = point3.Y;
                    identity.M21 = point4.X;
                    identity.M22 = point4.Y;
                    MatrixTransform transform2 = new MatrixTransform
                    {
                        Matrix = identity
                    };
                    InvertMatrix(ref identity);
                    MatrixTransform transform3 = new MatrixTransform
                    {
                        Matrix = identity
                    };
                    // Flip the x transform for RTL to keep the root visual within screen
                    if (FlowDirection == FlowDirection.RightToLeft && _overlay != null)
                    {
                        transform3.Matrix = new Matrix
                        {
                            M11 = transform3.Matrix.M11,
                            M12 = transform3.Matrix.M12,
                            M21 = transform3.Matrix.M21,
                            M22 = transform3.Matrix.M22,
                            OffsetX = -_overlay.Width,
                            OffsetY = transform3.Matrix.OffsetY,
                        };
                    }
                    TransformGroup renderTransform = this._root.RenderTransform as TransformGroup;
                    if (renderTransform != null)
                    {
                        renderTransform.Children.Add(transform3);
                    }
                    else
                    {
                        this._root.RenderTransform = transform3;
                    }
                    renderTransform = this.ContentRoot.RenderTransform as TransformGroup;
                    if (renderTransform != null)
                    {
                        renderTransform.Children.Add(transform2);
                    }
                    else
                    {
                        this.ContentRoot.RenderTransform = transform2;
                    }
                }
            }
        }

        #endregion Methods
    }
}
