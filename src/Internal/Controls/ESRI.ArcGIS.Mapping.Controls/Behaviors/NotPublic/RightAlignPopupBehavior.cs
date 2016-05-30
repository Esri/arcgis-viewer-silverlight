/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Mapping.Core;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace ESRI.ArcGIS.Mapping.Controls
{
    /// <summary>
    /// Automatically manipulates the associated popup's <see cref="HorizontalOffset"/> to
    /// align the right side of it with a target element
    /// </summary>
    public class RightAlignPopupBehavior : Behavior<Popup>
    {
        private double popupOpacity = 1; // Stores the popup's original opacity while opacity is temporarily manipulated
        private bool positioned; // Flag indicating whether the popup's position has been calculated

        #region Behavior Overrides

        protected override void OnAttached()
        {
            base.OnAttached();

            // Listen for when the popup is opened
            if (AssociatedObject != null)
                AssociatedObject.Opened += Popup_Opened;

            // Listen for changes in the size of the element that the popup is supposed to be aligned with
            if (AlignmentTarget != null)
                AlignmentTarget.SizeChanged += SizeChanged;

            // Listen for changes in the size of the application, as this will affect what the offset should be
            if (Application.Current != null && Application.Current.RootVisual is FrameworkElement)
                ((FrameworkElement)Application.Current.RootVisual).SizeChanged += SizeChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            // Reset the flag indicating whether the popup's position has been calculated
            positioned = false;

            // Unhook from all event handlers

            if (AssociatedObject != null)
                AssociatedObject.Opened -= Popup_Opened;

            if (AlignmentTarget != null)
                AlignmentTarget.SizeChanged -= SizeChanged;

            if (Application.Current != null && Application.Current.RootVisual is FrameworkElement)
                ((FrameworkElement)Application.Current.RootVisual).SizeChanged -= SizeChanged;
        }

        #endregion

        #region AlignmentTarget Property

        /// <summary>
        /// Identifies the <see cref="AlignmentTarget"/> DependencyProperty
        /// </summary>
        public static DependencyProperty AlignmentTargetProperty = DependencyProperty.Register(
            "AlignmentTarget", typeof(FrameworkElement), typeof(RightAlignPopupBehavior), 
            new PropertyMetadata(OnAlignmentTargetChanged));

        /// <summary>
        /// Gets or sets the element to align the popup to.  The popup will be positioned so that its
        /// right side aligns to the right side of the specified element.
        /// </summary>
        public FrameworkElement AlignmentTarget
        {
            get { return GetValue(AlignmentTargetProperty) as FrameworkElement; }
            set { SetValue(AlignmentTargetProperty, value); }
        }

        /// <summary>
        /// Fires when the <see cref="AlignmentTarget"/> property changes
        /// </summary>
        private async static void OnAlignmentTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Get the current behavior instance
            RightAlignPopupBehavior b = (RightAlignPopupBehavior)d;

            // Stop listening to size changes on the previous alignment target
            if (e.OldValue != null)
                ((FrameworkElement)e.OldValue).SizeChanged -= b.SizeChanged;

            if (e.NewValue != null)
            {
                // Listen for size changes on the new alignment target
                ((FrameworkElement)e.NewValue).SizeChanged += b.SizeChanged;

                // Reset the flag indicating whether the popup's position has been calculated
                b.positioned = false;

                // Calculate the position of the popup
                await b.positionPopup();

                // Revert the popup's opacity to its original value
                b.resetOpacity();
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Fires when the popup associated with the behavior is opened
        /// </summary>
        private async void Popup_Opened(object sender, EventArgs e)
        {
            // Get the popup's opacity and temporarily make the popup transparent
            if (AssociatedObject.Child != null)
            {
                popupOpacity = AssociatedObject.Child.Opacity;
                AssociatedObject.Child.Opacity = .01;
            }

            // Calculate the popup's position
            await positionPopup();

            // Reset the popup's opacity to its original value
            resetOpacity();
        }

        /// <summary>
        /// Fires when the size of the popup or the application changes
        /// </summary>
        private void SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Reset the flag indicating whether the popup's position has been calculated.  This will force a 
            // recalculation of the position the next time the popup is opened.
            positioned = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Reverts the popup's opacity to its original value
        /// </summary>
        private void resetOpacity()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.Dispatcher.BeginInvoke(() =>
                    {
                        if (AssociatedObject.Child != null)
                            AssociatedObject.Child.Opacity = popupOpacity;
                    });
            }
        }

        /// <summary>
        /// Re-positions the popup so that its right side aligns with the right edge of the <see cref="AlignmentTarget"/>
        /// </summary>
        private async Task positionPopup()
        {
            // Check whether position has already been calculated
            if (positioned)
                return;

            if (AlignmentTarget == null || AssociatedObject == null 
            || !(AssociatedObject.Child is FrameworkElement))
                return;

            // Get the position of the target element
            Point? targetPosition = await AlignmentTarget.GetPosition();
            if (targetPosition == null)
                return;

            // Get the width and right edge position of the target element
            double targetWidth = await AlignmentTarget.GetActualWidth();
            double targetRightSide = ((Point)targetPosition).X + targetWidth;

            // Get the position of the popup
            Point? popupPosition = await AssociatedObject.Child.GetPosition();
            if (popupPosition == null)
                return;

            // Get the width and right edge position of the popup
            double popupWidth = await ((FrameworkElement)AssociatedObject.Child).GetActualWidth();
            double popupRightSide = ((Point)popupPosition).X + popupWidth;

            // Calculate the popup's offset given the right edge of the target element and popup
            AssociatedObject.HorizontalOffset = targetRightSide - popupRightSide;

            // Update the flag indicating that the position of the popup has been calculated
            if (AssociatedObject.HorizontalOffset > 0)
                positioned = true;
        }

        #endregion
    }
}
