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
using ESRI.ArcGIS.Client.Application.Controls;

namespace SearchTool
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
        public static FrameworkElement AssociatedUIElement { get; set; }

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

        /// <summary>
        /// Hides the popup
        /// </summary>
        public void ClosePopup()
        {
            DetailsPopup.Close();
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
            try // Transforming the popup content to visual occasionally throws a "Value does not fall within expected range" error.
                // Placing the arrangment in BeginInvoke to allow the UI thread a rendering pass did not fix the issue.  The exception 
                // is simply swallowed to avoid this.  
            {
                // TODO: handle pop-up positioning (logic is currently in Windowing assembly)
                ArrangePopup(_Popup, _PopupBaseCanvas, null, _PopupContentControl, _PopupLeader, _PopupContentContainerPanel, AssociatedUIElement, false, false);
            }
            catch { }
        }

        /// <summary>
        /// Occurs when the moving and sizing animation has completed.
        /// </summary>
        static void _moveSizeStoryBoard_Completed(object sender, EventArgs e)
        {
            // resume listening to events once the resize animation is complete
            StartListeningEvents(_PopupContentControl);
        }

        private static double RIGHT_CENTER_TOP_BOTTOM_MARGIN = 20;
        static void ArrangePopup(Popup ElementPopup,
                                Canvas ElementPopupChildCanvas,
                                Canvas ElementOutsidePopup,
                                FrameworkElement ElementPopupChild,
                                ContentControl LeaderPopupContent,
                                StackPanel ElementPopupChildMaxRangeStackPanel,
                                FrameworkElement AssociatedElement,
                                bool EnforceMinWidth = true,
                                bool isNestedPopup = true
                            )
        {
            if (ElementPopup != null && AssociatedElement != null)
            {
                bool isRTL = (AssociatedElement.FlowDirection == FlowDirection.RightToLeft);
                System.Windows.Interop.Content content = System.Windows.Application.Current.Host.Content;
                double applicationWidth = content.ActualWidth;
                double applicationHeight = content.ActualHeight;
                double popupWidth = ElementPopupChild.ActualWidth;
                double popupHeight = ElementPopupChild.ActualHeight;
                if ((applicationHeight != 0.0) && (applicationWidth != 0.0))
                {
                    GeneralTransform transform = AssociatedElement.TransformToVisual(null);
                    if (isRTL && transform is MatrixTransform)
                    {
                        var mt = (MatrixTransform)transform;
                        transform = new MatrixTransform
                        {
                            Matrix = new Matrix
                            {
                                M11 = mt.Matrix.M11,
                                M12 = mt.Matrix.M12,
                                M21 = mt.Matrix.M21,
                                M22 = mt.Matrix.M22,
                                OffsetX = mt.Matrix.OffsetX - AssociatedElement.ActualWidth,
                                OffsetY = mt.Matrix.OffsetY,
                            }
                        };
                    }
                    if (transform != null)
                    {
                        Point topLeftTransPoint = new Point(0.0, 0.0);
                        Point topRightTransPoint = new Point(1.0, 0.0);
                        Point bottomLeftTransPoint = new Point(0.0, 1.0);
                        Point topLeftPosition = transform.Transform(topLeftTransPoint);
                        Point topRightPosition = transform.Transform(topRightTransPoint);
                        Point bottomLeftPosition = transform.Transform(bottomLeftTransPoint);
                        double xDropDown = topLeftPosition.X;
                        double yDropDown = topLeftPosition.Y;
                        double widthRatio = Math.Abs((double)(topRightPosition.X - topLeftPosition.X));
                        double heightRatio = Math.Abs((double)(bottomLeftPosition.Y - topLeftPosition.Y));
                        double heightDropDown = AssociatedElement.ActualHeight * heightRatio;
                        double widthDropDown = AssociatedElement.ActualWidth * widthRatio;
                        double yBottomDropDown = yDropDown + heightDropDown;
                        //if ((heightDropDown != 0.0) && (widthDropDown != 0.0))
                        //{
                            popupWidth *= widthRatio;
                            popupHeight *= heightRatio;
                            double maxDropDownHeight = double.PositiveInfinity;
                            //if (ExpandDirection == ExpandDirection.BottomLeft)
                            //{
                            //    if (double.IsInfinity(maxDropDownHeight) || double.IsNaN(maxDropDownHeight))
                            //        maxDropDownHeight = ((applicationHeight - heightDropDown) * 3.0) / 5.0;
                            //    bool flag = true;
                            //    if (applicationHeight < (yBottomDropDown + popupHeight))
                            //    {
                            //        flag = false;
                            //        yBottomDropDown = yDropDown - popupHeight;
                            //        if (yBottomDropDown < 0.0)
                            //        {
                            //            if (yDropDown < ((applicationHeight - heightDropDown) / 2.0))
                            //            {
                            //                flag = true;
                            //                yBottomDropDown = yDropDown + heightDropDown;
                            //            }
                            //            else
                            //            {
                            //                flag = false;
                            //                yBottomDropDown = yDropDown - popupHeight;
                            //            }
                            //        }
                            //    }
                            //    if (popupHeight != 0.0)
                            //    {
                            //        if (flag)
                            //            maxDropDownHeight = Math.Min(applicationHeight - yBottomDropDown, maxDropDownHeight);
                            //        else
                            //            maxDropDownHeight = Math.Min(yDropDown, maxDropDownHeight);
                            //    }
                            //}
                            //else
                            //{
                                if (double.IsInfinity(maxDropDownHeight) || double.IsNaN(maxDropDownHeight))
                                    maxDropDownHeight = applicationHeight - 2 * RIGHT_CENTER_TOP_BOTTOM_MARGIN;
                            //}
                            popupWidth = Math.Min(popupWidth, applicationWidth);
                            popupHeight = Math.Min(popupHeight, maxDropDownHeight);
                            popupWidth = Math.Max(widthDropDown, popupWidth);
                            double applicationRemainWidth = 0.0;
                            double leaderWidth = GetFrameworkElementWidth(LeaderPopupContent);
                            if (double.IsNaN(leaderWidth) || double.IsInfinity(leaderWidth))
                                leaderWidth = 0;

                            if (AssociatedElement.FlowDirection == FlowDirection.LeftToRight)
                            {
                                if (applicationWidth < (xDropDown + popupWidth))
                                    applicationRemainWidth = applicationWidth - (popupWidth + xDropDown);
                            }
                            else if (0.0 > (xDropDown - popupWidth))
                                applicationRemainWidth = xDropDown - popupWidth;

                            ElementPopup.HorizontalOffset = 0.0;
                            ElementPopup.VerticalOffset = 0.0;

                            Matrix identity = Matrix.Identity;
                            identity.OffsetX -= topLeftPosition.X / widthRatio;
                            identity.OffsetY -= topLeftPosition.Y / heightRatio;
                            MatrixTransform transform2 = new MatrixTransform();
                            transform2.Matrix = identity;

                            if (ElementOutsidePopup != null)
                            {
                                ElementOutsidePopup.Width = applicationWidth / widthRatio;
                                ElementOutsidePopup.Height = applicationHeight / heightRatio;
                                ElementOutsidePopup.RenderTransform = transform2;
                            }

                            double minWidthDropDown = 0.0;
                            if (EnforceMinWidth)
                            {
                                minWidthDropDown = widthDropDown / widthRatio;
                                ElementPopupChild.MinWidth = minWidthDropDown;
                            }

                            double maxPopupWidth = double.PositiveInfinity;
                            bool reversePopupExpandDirection = false;
                            //if (ExpandDirection == ExpandDirection.BottomLeft)
                            //    maxPopupWidth = applicationWidth / widthRatio;
                            //else
                            //{
                                maxPopupWidth = applicationWidth - (xDropDown + widthDropDown) - leaderWidth;
                                maxPopupWidth = maxPopupWidth / widthRatio;

                                if (maxPopupWidth < popupWidth)
                                {
                                    double tempMaxPopupWidth;
                                    //try show on the other side
                                    tempMaxPopupWidth = xDropDown - leaderWidth;
                                    if (tempMaxPopupWidth >= 0 && tempMaxPopupWidth > maxPopupWidth)
                                    {
                                        maxPopupWidth = tempMaxPopupWidth;
                                        reversePopupExpandDirection = true;
                                    }
                                }
                            //}

                            ElementPopupChild.MaxWidth = Math.Max(minWidthDropDown, maxPopupWidth);
                            ElementPopupChild.MinHeight = heightDropDown / heightRatio;
                            ElementPopupChild.MaxHeight = Math.Max((double)0.0, (double)(maxDropDownHeight / heightRatio));
                            ElementPopupChild.HorizontalAlignment = HorizontalAlignment.Left;
                            ElementPopupChild.VerticalAlignment = VerticalAlignment.Top;
                            ElementPopupChild.FlowDirection = AssociatedElement.FlowDirection;

                            if (ElementPopupChildMaxRangeStackPanel != null) //if horizontal central alignment, then simply reuse the maxrange canvas and align within
                            {
                                double top = isNestedPopup ? (heightDropDown - popupHeight) / 2 : Math.Max(0.0, yDropDown + (heightDropDown - popupHeight) / 2);
                                double overflow = Math.Abs(Math.Min(applicationHeight - (yDropDown + (yBottomDropDown - yDropDown) / 2 + popupHeight / 2 + RIGHT_CENTER_TOP_BOTTOM_MARGIN), 0.0));
                                if (overflow > 0)
                                {
                                    top -= overflow; //move up if overflowing underneath
                                }
                                top = Math.Max(top, -yDropDown + RIGHT_CENTER_TOP_BOTTOM_MARGIN); //did our best to calculate top, so set to top or application level x=0

                                Canvas.SetTop(ElementPopupChildMaxRangeStackPanel, top);
                                ElementPopupChildMaxRangeStackPanel.Width = maxPopupWidth;
                                ElementPopupChildMaxRangeStackPanel.Height = popupHeight;

                                if (isRTL && isNestedPopup)
                                    reversePopupExpandDirection = !reversePopupExpandDirection;

                                if (reversePopupExpandDirection)
                                    Canvas.SetLeft(ElementPopupChildMaxRangeStackPanel, isNestedPopup ? -xDropDown : 0);
                                else
                                {
                                    Canvas.SetLeft(ElementPopupChildMaxRangeStackPanel, (isNestedPopup ? 0 : xDropDown) + AssociatedElement.ActualWidth + leaderWidth);
                                }

                                if (reversePopupExpandDirection)
                                    ElementPopupChild.HorizontalAlignment = HorizontalAlignment.Right;

                                SetupLeader(LeaderPopupContent, ElementPopupChildMaxRangeStackPanel, reversePopupExpandDirection, xDropDown, yDropDown, heightDropDown, AssociatedElement, isNestedPopup);
                            //}
                            //else
                            //{
                            //    SetPopupTop(ElementPopupChild, yBottomDropDown, heightRatio, yDropDown, popupHeight, applicationHeight, ExpandDirection);
                            //    SetPopupLeft(ElementPopupChild, applicationRemainWidth, widthRatio, popupWidth, maxPopupWidth, reversePopupExpandDirection, ExpandDirection, AssociatedControl, xDropDown);
                            //    SetupLeader(LeaderPopupContent, ElementPopupChild, reversePopupExpandDirection, xDropDown, yDropDown, heightDropDown, ExpandDirection, AssociatedControl, isNestedPopup);
                            //}
                        }
                    }
                }
            }
        }

        private static void SetupLeader(FrameworkElement leader, FrameworkElement popup, bool reversePopupExpandDirection, double xDropDown, double yDropDown, 
            double heightDropDown, FrameworkElement associatedElement, bool isNestedPopup)
        {
            if (leader != null && associatedElement != null)
            {
                leader.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                leader.VerticalAlignment = System.Windows.VerticalAlignment.Top;

                double leaderWidth = leader.ActualWidth;
                double leaderHeight = leader.ActualHeight;
                double leftBorder;
                //double d, topBorder;
                if ((leaderWidth != 0.0) && (leaderHeight != 0.0) && popup != null)
                {
                    #region Position leader, Offset dropdown to accommodate leader
                    //switch (expandDirection)
                    //{
                    //    case ExpandDirection.BottomLeft:
                    //        #region Left
                    //        d = Canvas.GetLeft(popup);
                    //        leftBorder = (associatedControl.BorderThickness != null) ? associatedControl.BorderThickness.Left : 0;
                    //        Canvas.SetLeft(leader, d + leftBorder * 2);
                    //        #endregion
                    //        #region Top
                    //        topBorder = (associatedControl.BorderThickness != null) ? associatedControl.BorderThickness.Top : 0;
                    //        d = Canvas.GetTop(popup);
                    //        Canvas.SetTop(leader, d + topBorder);//add border for overlap
                    //        Canvas.SetTop(popup, d + leaderHeight - topBorder);
                    //        #endregion
                    //        break;
                    //    case ExpandDirection.HorizontalCenter:

                            if (reversePopupExpandDirection)
                                RotateFrameworkElement(leader, 180);
                            else
                                RotateFrameworkElement(leader, 0);

                            #region Left
                            leftBorder = 1; //(associatedElement.BorderThickness != null) ? associatedElement.BorderThickness.Left : 0;
                            Canvas.SetLeft(leader, !reversePopupExpandDirection ? (isNestedPopup ? 0 : xDropDown) + associatedElement.ActualWidth + leftBorder : (isNestedPopup ? 0 : xDropDown) - leaderHeight - leftBorder);//add border for overlap
                            #endregion
                            #region Top
                            //topBorder = (associatedElement.BorderThickness != null) ? associatedElement.BorderThickness.Top : 0;

                            Canvas.SetTop(leader, isNestedPopup ? (heightDropDown - leaderHeight) / 2 : yDropDown + (heightDropDown - leaderHeight) / 2);
                            #endregion
                    //        break;
                    //}
                    #endregion
                }
            }
        }

        private static double GetFrameworkElementWidth(FrameworkElement frameworkElement)
        {
            if (frameworkElement != null)
            {
                if (!double.IsNaN(frameworkElement.ActualWidth) && !double.IsInfinity(frameworkElement.ActualWidth))
                    return frameworkElement.ActualWidth;
            }
            return 0;
        }
        private static void RotateFrameworkElement(FrameworkElement element, double degrees)
        {
            element.Projection = new PlaneProjection() { RotationZ = degrees };
        }
    }
}
