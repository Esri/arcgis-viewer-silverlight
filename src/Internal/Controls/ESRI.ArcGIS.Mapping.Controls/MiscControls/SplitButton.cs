/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ESRI.ArcGIS.Mapping.Controls
{
    /// <summary>
    /// Implements a "split button" for Silverlight and WPF.
    /// </summary>
    [TemplatePart(Name = SplitElementName, Type = typeof(UIElement))]
    public class SplitButton : Button
    {
        /// <summary>
        /// Stores the public name of the split element.
        /// </summary>
        private const string SplitElementName = "SplitElement";

        /// <summary>
        /// Stores a reference to the split element.
        /// </summary>
        private UIElement _splitElement;

        /// <summary>
        /// Stores a reference to the ContextMenu.
        /// </summary>
        private ContextMenu _contextMenu;

        /// <summary>
        /// Stores the initial location of the ContextMenu.
        /// </summary>
        private Point _contextMenuInitialOffset;

        /// <summary>
        /// Stores the backing collection for the ButtonMenuItemsSource property.
        /// </summary>
        private ObservableCollection<object> _buttonMenuItemsSource = new ObservableCollection<object>();

        /// <summary>
        /// Gets the collection of items for the split button's menu.
        /// </summary>
        public Collection<object> ButtonMenuItemsSource { get { return _buttonMenuItemsSource; } }

        /// <summary>
        /// Gets or sets a value indicating whether the mouse is over the split element.
        /// </summary>
        protected bool IsMouseOverSplitElement { get; private set; }

        /// <summary>
        /// Initializes a new instance of the SplitButton class.
        /// </summary>
        public SplitButton()
        {
            DefaultStyleKey = typeof(SplitButton);
        }

        /// <summary>
        /// Called when the template is changed.
        /// </summary>
        public override void OnApplyTemplate()
        {
            // Unhook existing handlers
            if (null != _splitElement)
            {
                _splitElement.MouseEnter -= new MouseEventHandler(SplitElement_MouseEnter);
                _splitElement.MouseLeave -= new MouseEventHandler(SplitElement_MouseLeave);
                _splitElement = null;
            }
            if (null != _contextMenu)
            {
                _contextMenu.Opened -= new RoutedEventHandler(ContextMenu_Opened);
                _contextMenu.Closed -= new RoutedEventHandler(ContextMenu_Closed);
                _contextMenu = null;
            }

            // Apply new template
            base.OnApplyTemplate();

            // Hook new event handlers
            _splitElement = GetTemplateChild(SplitElementName) as UIElement;
            if (null != _splitElement)
            {
                _splitElement.MouseEnter += new MouseEventHandler(SplitElement_MouseEnter);
                _splitElement.MouseLeave += new MouseEventHandler(SplitElement_MouseLeave);

                _contextMenu = ContextMenuService.GetContextMenu(_splitElement);
                if (null != _contextMenu)
                {
                    _contextMenu.Opened += new RoutedEventHandler(ContextMenu_Opened);
                    _contextMenu.Closed += new RoutedEventHandler(ContextMenu_Closed);
                }
            }
        }

        /// <summary>
        /// Called when the Button is clicked.
        /// </summary>
        protected override void OnClick()
        {
            if (IsMouseOverSplitElement)
            {
                OpenButtonMenu();
            }
            else
            {
                base.OnClick();
            }
        }

        /// <summary>
        /// Called when a key is pressed.
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (null == e)
            {
                throw new ArgumentNullException("e");
            }

            if ((Key.Down == e.Key) || (Key.Up == e.Key))
            {
                // WPF requires this to happen via BeginInvoke
                Dispatcher.BeginInvoke((Action)(() => OpenButtonMenu()));
            }
            else
            {
                base.OnKeyDown(e);
            }
        }

        /// <summary>
        /// Opens the button menu.
        /// </summary>
        protected void OpenButtonMenu()
        {
            if ((0 < _buttonMenuItemsSource.Count) && (null != _contextMenu))
            {
                _contextMenu.HorizontalOffset = 0;
                _contextMenu.VerticalOffset = 0;
                _contextMenu.IsOpen = true;
            }
        }

        /// <summary>
        /// Called when the mouse goes over the split element.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void SplitElement_MouseEnter(object sender, MouseEventArgs e)
        {
            IsMouseOverSplitElement = true;
        }

        /// <summary>
        /// Called when the mouse goes off the split element.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void SplitElement_MouseLeave(object sender, MouseEventArgs e)
        {
            IsMouseOverSplitElement = false;
        }

        /// <summary>
        /// Called when the ContextMenu is opened.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            // Offset the ContextMenu correctly
            _contextMenuInitialOffset = _contextMenu.TransformToVisual(null).Transform(new Point());
            UpdateContextMenuOffsets();

            // Hook LayoutUpdated to handle application resize and zoom changes
            LayoutUpdated += new EventHandler(SplitButton_LayoutUpdated);
        }

        /// <summary>
        /// Called when the ContextMenu is closed.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            // No longer need to handle LayoutUpdated
            LayoutUpdated -= new EventHandler(SplitButton_LayoutUpdated);

            // Restore focus to the Button
            Focus();
        }

        /// <summary>
        /// Called when the ContextMenu is open and layout is updated.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void SplitButton_LayoutUpdated(object sender, EventArgs e)
        {
            UpdateContextMenuOffsets();
        }

        /// <summary>
        /// Updates the ContextMenu's Horizontal/VerticalOffset properties to keep it under the SplitButton.
        /// </summary>
        private void UpdateContextMenuOffsets()
        {
            // Calculate desired offset to put the ContextMenu below and left-aligned to the Button
            Point currentOffset = _contextMenuInitialOffset;
            Point desiredOffset = TransformToVisual(Application.Current.RootVisual).Transform(new Point(0, ActualHeight));
            _contextMenu.HorizontalOffset = desiredOffset.X - currentOffset.X;
            _contextMenu.VerticalOffset = desiredOffset.Y - currentOffset.Y;
            // Adjust for RTL
            if (FlowDirection.RightToLeft == FlowDirection)
            {
                _contextMenu.UpdateLayout();
                _contextMenu.HorizontalOffset -= _contextMenu.ActualWidth;
            }
        }
    }
}
