/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace ESRI.ArcGIS.Mapping.Controls
{
    /// <summary>
    /// Closes the associated <see cref="Popup"/> when any point outside of it but within the application surface 
    /// is clicked
    /// </summary>
    /// <remarks>
    /// The functionality relies on inserting an overlay for listening to clicks.  The overlay is inserted into the
    /// Grid that is closest to the root of the application's visual tree.  If no Grid is present, or this Grid does
    /// not fully cover the application's surface, then the behavior will not function properly.
    /// </remarks>
    public class ClosePopupOnClickBehavior : Behavior<Popup>
    {
        private Grid overlay; // Overlay element for intercepting click events on the application surface
        private Grid rootGrid; // Grid that is closest to the root of the application's visual tree

        #region Behavior Overrides 

        protected override void OnAttached()
        {
            base.OnAttached();

            // Listen for when the popup is opened and closed
            if (AssociatedObject != null)
            {
                AssociatedObject.Opened += Popup_Opened;
                AssociatedObject.Closed += Popup_Closed;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            // Unhook from popup events
            if (AssociatedObject != null)
            {
                AssociatedObject.Opened -= Popup_Opened;
                AssociatedObject.Closed -= Popup_Closed;
            }

            // Remove the overlay from the visual tree
            if (rootGrid != null && overlay != null && rootGrid.Children.Contains(overlay))
                rootGrid.Children.Remove(overlay);

            // Unhook from overlay events
            if (overlay != null)
            {
                overlay.MouseLeftButtonDown -= Overlay_Click;
                overlay.MouseRightButtonDown -= Overlay_Click;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Fires when the associated Popup is opened
        /// </summary>
        private void Popup_Opened(object sender, EventArgs e)
        {
            if (Application.Current != null && Application.Current.RootVisual != null)
            {
                // Find the Grid that is closest to the root of the application's visual tree
                if (rootGrid == null)
                    rootGrid = ControlTreeHelper.FindChildOfType<Grid>(Application.Current.RootVisual);

                if (rootGrid != null)
                {
                    // Create the overlay grid
                    if (overlay == null)
                    {
                        // Make the overlay grid almost transparent, but not completely so.  Elements that are
                        // completely transparent cannot intercept clicks.
                        overlay = new Grid()
                        {
                            Background = new SolidColorBrush(Color.FromArgb(1, 255, 255, 255)),
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch
                        };

                        // Set the column and row span of the overlay so that it covers the entire grid

                        if (rootGrid.ColumnDefinitions.Count > 0)
                            Grid.SetColumnSpan(overlay, rootGrid.ColumnDefinitions.Count);

                        if (rootGrid.RowDefinitions.Count > 0)
                            Grid.SetRowSpan(overlay, rootGrid.RowDefinitions.Count);

                        // Listen to click events on the overlay
                        overlay.MouseLeftButtonDown += Overlay_Click;
                        overlay.MouseRightButtonDown += Overlay_Click;
                    }

                    // Insert the overlay into the grid
                    rootGrid.Children.Add(overlay);
                }
            }
        }

        /// <summary>
        /// Fires when a mouse-down event occurs on the overlay
        /// </summary>
        private void Overlay_Click(object sender, MouseButtonEventArgs e)
        {
            // Close the popup
            if (AssociatedObject != null && AssociatedObject.IsOpen)
                AssociatedObject.IsOpen = false;
        }

        /// <summary>
        /// Fires when the associated <see cref="Popup"/> is closed
        /// </summary>
        private void Popup_Closed(object sender, EventArgs e)
        {
            // Remove the overlay from the visual tree
            if (rootGrid != null && overlay != null && rootGrid.Children.Contains(overlay))
                rootGrid.Children.Remove(overlay);
        }

        #endregion
    }
}
