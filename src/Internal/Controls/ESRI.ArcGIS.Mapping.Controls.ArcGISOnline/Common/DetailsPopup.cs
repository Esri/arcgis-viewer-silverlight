/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client.Application.Controls;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
    /// <summary>
    /// Represents a popup that shows details of a search result. 
    /// Only one details popup can be shown at a time.
    /// </summary>
    public class DetailsPopup
    {
        static System.Windows.Controls.Primitives.Popup _Popup = new System.Windows.Controls.Primitives.Popup();
        static Point _mousePos;
        static int _xOffset;
        static bool _loaded = false;
        static double _previousHeight = 0.0;
        static Canvas _PopupBaseCanvas = new Canvas();
        static ContentControl _PopupContentControl;
        static FrameworkElement _PopupContent;
        static ContentControl _PopupLeader;
        static StackPanel _PopupContentContainerPanel;
        static Brush TransperantBrush = new SolidColorBrush(Colors.Transparent);
        private static bool isOverPopup = false;

        static DetailsPopup()
        {
            _Popup.Loaded += new RoutedEventHandler(_popup_Loaded);
        }

        static void _popup_Loaded(object sender, RoutedEventArgs e)
        {
            _loaded = true;

            _PopupBaseCanvas = new Canvas();
            _PopupContentContainerPanel = new StackPanel();
            _PopupContentControl = new ContentControl();
            _PopupLeader = new ContentControl();

            _PopupBaseCanvas.Background = TransperantBrush;
            _PopupContentContainerPanel.Background = TransperantBrush;

            _PopupContentContainerPanel.Children.Add(_PopupContentControl);
            _PopupBaseCanvas.Children.Add(_PopupContentContainerPanel);
            _PopupBaseCanvas.Children.Add(_PopupLeader);
            _Popup.Child = _PopupBaseCanvas;
            _PopupContentControl.MouseEnter -= _PopupContentControl_MouseEnter;
            _PopupContentControl.MouseEnter += _PopupContentControl_MouseEnter;
            _PopupContentControl.MouseLeave -= _PopupContentControl_MouseLeave;
            _PopupContentControl.MouseLeave += _PopupContentControl_MouseLeave;

        }

        static void _PopupContentControl_MouseLeave(object sender, MouseEventArgs e)
        {
            isOverPopup = false;
        }

        static void _PopupContentControl_MouseEnter(object sender, MouseEventArgs e)
        {
            isOverPopup = true;
        }

        /// <summary>
        /// Shows the specified control in a popup.
        /// </summary>
        public static void Show(FrameworkElement control, Point mousePos, int xOffset, Style containerStyle, Style leaderStyle)
        {
            if (control == null) return;

            _PopupContent = control;

            // if the popup is already open and displaying a control, remember
            // the previous height for use in the move animation, otherwise reset it to 0
            if (_PopupContentControl != null && _Popup.IsOpen)
                _previousHeight = _PopupContentControl.ActualHeight;
            else
                _previousHeight = 0.0;

            StopListeningEvents(_PopupContentControl);
            StartListeningEvents(_PopupContentControl);

            _mousePos = mousePos;
            _xOffset = xOffset;
            if (_PopupContentControl != null)
            {
                if (_PopupContentControl.Content != null)
                    _PopupContentControl.Content = null;
                _PopupContentControl.Style = containerStyle;
            }
            if (_PopupLeader != null)
            {
                _PopupLeader.Style = leaderStyle;
            }
            _PopupContentControl.Content = _PopupContent;
            PositionPopup();

            _Popup.IsOpen = true;
        }

        /// <summary>
        /// Occurs when the fade in/out animation has completed.
        /// </summary>
        static void storyBoard_Completed(object sender, EventArgs e)
        {
            if (_PopupContentControl == null || !_Popup.IsOpen)
                return;

            //if the popup has faded away make sure it is closed
            if (_PopupContentControl.Opacity == 0.0)
                _Popup.IsOpen = false;
        }

        /// <summary>
        /// Returns true if the popup is currently visible.
        /// </summary>
        public static bool IsOpen
        {
            get { return _Popup.IsOpen; }
        }

        /// <summary>
        /// Gets or sets the UIElement the popup is associated with.
        /// </summary>
        public static Control AssociatedUIElement { get; set; }

        /// <summary>
        /// Hides the popup.
        /// </summary>
        public static void Close()
        {
            if (_Popup != null)
                _Popup.IsOpen = false;
            if (_PopupContentControl != null)
                _PopupContentControl.Content = null;
            AssociatedUIElement = null;
            //FadeInOut(false);
            StopListeningEvents(_PopupContentControl);
        }

        public static bool IsMouseOverPopup()
        {
            if (!_Popup.IsOpen || _PopupContent == null || !_loaded)
                return false;

            return isOverPopup;
        }

        /// <summary>
        /// Start listening to events from the user control.
        /// </summary>
        static void StartListeningEvents(FrameworkElement control)
        {
            if (control == null) return;

            control.SizeChanged += new SizeChangedEventHandler(SizeChanged);
        }

        /// <summary>
        /// Stop listening to events from the user control.
        /// </summary>
        static void StopListeningEvents(FrameworkElement control)
        {
            if (control == null) return;

            control.SizeChanged -= SizeChanged;
        }

        /// <summary>
        /// Called when the size of the user control changes.
        /// </summary>
        static void SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //only when the SizeChanged event is fired from the user control we can
            //be sure the width and height properties are populated
            //now position the popup..
            PositionPopup();
        }

        /// <summary>
        /// Tries to position the popup so it is fully visible.
        /// </summary>
        static void PositionPopup()
        {
            PopupManager.Instance.ArrangePopup(_Popup, _PopupBaseCanvas, null, _PopupContentControl, _PopupLeader, _PopupContentContainerPanel, AssociatedUIElement, ESRI.ArcGIS.Client.Application.Controls.ExpandDirection.HorizontalCenter, false, false);
        }

        /// <summary>
        /// Occurs when the moving and sizing animation has completed.
        /// </summary>
        static void _moveSizeStoryBoard_Completed(object sender, EventArgs e)
        {
            // resume listening to events once the resize animation is complete
            StartListeningEvents(_PopupContentControl);
        }
    }
}
