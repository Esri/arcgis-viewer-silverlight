/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace SearchTool
{
    // TODO: Refactor after replacing DetailsPopup

    /// <summary>
    /// Shows a popup containing the targeted element
    /// </summary>
    [TypeConstraint(typeof(FrameworkElement))] // Ensure the associated object is a FrameworkElement
    public class ShowPopupAction : TargetedTriggerAction<FrameworkElement>
    {
        private FrameworkElement _associatedElement;
        protected override void Invoke(object parameter)
        {
            if (Target != null)
            {
                if (DetailsPopup.AssociatedUIElement == _associatedElement)
                    return;
                DetailsPopup.AssociatedUIElement = _associatedElement;
                if (PopupDataContext != null)
                    Target.DataContext = PopupDataContext;

                //get the center y coordinate of the list item
                GeneralTransform gt = _associatedElement.TransformToVisual(Application.Current.RootVisual);
                Point topLeft = gt.Transform(new Point(0, 0));
                double centerY = topLeft.Y + _associatedElement.ActualHeight / 2;

                double xOffset = GetXOffset(_currentMousePos);
                DetailsPopup.Show(Target, new Point(_currentMousePos.X, centerY), (int)xOffset,
                    PopupContentContainerStyle, PopupLeaderStyle);
            }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="PopupContentContainerStyle"/> property
        /// </summary>
        public static DependencyProperty PopupContentContainerStyleProperty = DependencyProperty.Register(
            "PopupContentContainerStyle", typeof(Style), typeof(ShowPopupAction), null);

        /// <summary>
        /// Gets or sets the style of the popup container
        /// </summary>
        public Style PopupContentContainerStyle
        {
            get { return GetValue(PopupContentContainerStyleProperty) as Style; }
            set { SetValue(PopupContentContainerStyleProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="PopupLeaderStyle"/> property
        /// </summary>
        public static DependencyProperty PopupLeaderStyleProperty = DependencyProperty.Register(
            "PopupLeaderStyle", typeof(Style), typeof(ShowPopupAction), null);

        /// <summary>
        /// Gets or sets the style of the popup's leader
        /// </summary>
        public Style PopupLeaderStyle
        {
            get { return (Style)GetValue(PopupLeaderStyleProperty); }
            set { SetValue(PopupLeaderStyleProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="PopupDataContext"/> property
        /// </summary>
        public static DependencyProperty PopupDataContextProperty = DependencyProperty.Register(
            "PopupDataContext", typeof(object), typeof(ShowPopupAction), null);

        /// <summary>
        /// Gets or sets the popup's data context
        /// </summary>
        public object PopupDataContext
        {
            get { return (object)GetValue(PopupDataContextProperty); }
            set { SetValue(PopupDataContextProperty, value); }
        }

        #region Action Overrides

        protected override void OnAttached()
        {
            base.OnAttached();
            _associatedElement = (FrameworkElement)AssociatedObject;
            _associatedElement.MouseMove += AssociatedObject_MouseMove;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            _associatedElement.MouseMove -= AssociatedObject_MouseMove;
        }

        #endregion

        #region Event Handlers

        private Point _currentMousePos;
        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _currentMousePos = e.GetPosition(Application.Current.RootVisual);
        }

        #endregion

        /// <summary>
        /// Calculates an x offset relative to the associated object and specified mouse position.
        /// </summary>
        private double GetXOffset(Point mousePos)
        {
            //obtain transform information based off root element
            GeneralTransform gt = null;
            try
            {
                gt = _associatedElement.TransformToVisual(Application.Current.RootVisual);
            }
            catch (ArgumentException) //todo: hack to avoid mysterious argument exception
            {
                return 0.0;
            }

            //find the top left corner of the element
            Point topLeft = gt.Transform(new Point(0, 0));

            return topLeft.X + _associatedElement.RenderSize.Width - mousePos.X;
        }
    }
}
