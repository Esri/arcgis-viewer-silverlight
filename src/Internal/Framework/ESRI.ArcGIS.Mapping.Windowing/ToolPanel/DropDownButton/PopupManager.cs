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
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace ESRI.ArcGIS.Client.Application.Controls
{
    public enum ExpandDirection
    {
        BottomLeft,
        HorizontalCenter
    }

    public enum PopupStyleName
    {
        PopupContentControl,
        PopupLeader,
    }

    public class PopupManager
    {
        private static double RIGHT_CENTER_TOP_BOTTOM_MARGIN = 20;

        private static PopupManager _instance;
        public static PopupManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PopupManager();
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        private PopupManager()
        {
        }

        public void ArrangePopup(Popup ElementPopup, 
                                Canvas ElementPopupChildCanvas, 
                                Canvas ElementOutsidePopup, 
                                FrameworkElement ElementPopupChild, 
                                ContentControl LeaderPopupContent, 
                                StackPanel ElementPopupChildMaxRangeStackPanel,
                                Control AssociatedControl,
                                ExpandDirection ExpandDirection,
                                bool EnforceMinWidth = true,
                                bool isNestedPopup = true
                            )
        {
            if (ElementPopup != null && AssociatedControl != null)
            {
                bool isRTL = (AssociatedControl.FlowDirection == FlowDirection.RightToLeft);
                System.Windows.Interop.Content content = System.Windows.Application.Current.Host.Content;
                double applicationWidth = content.ActualWidth;
                double applicationHeight = content.ActualHeight;
                double popupWidth = ElementPopupChild.ActualWidth;
                double popupHeight = ElementPopupChild.ActualHeight;
                if ((applicationHeight != 0.0) && (applicationWidth != 0.0))
                {
                    GeneralTransform transform = AssociatedControl.TransformToVisual(null);
                    if (isRTL && transform is MatrixTransform)
                    {
                        var mt = (MatrixTransform) transform;
                        transform = new MatrixTransform
                                            {
                                                Matrix = new Matrix
                                                             {
                                                                 M11 = mt.Matrix.M11,
                                                                 M12 = mt.Matrix.M12,
                                                                 M21 = mt.Matrix.M21,
                                                                 M22 = mt.Matrix.M22,
                                                                 OffsetX = mt.Matrix.OffsetX - AssociatedControl.ActualWidth,
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
                        double heightDropDown = AssociatedControl.ActualHeight * heightRatio;
                        double widthDropDown = AssociatedControl.ActualWidth * widthRatio;
                        double yBottomDropDown = yDropDown + heightDropDown;
                        if ((heightDropDown != 0.0) && (widthDropDown != 0.0))
                        {
                            popupWidth *= widthRatio;
                            popupHeight *= heightRatio;
                            double maxDropDownHeight = double.PositiveInfinity;
                            if (ExpandDirection == ExpandDirection.BottomLeft)
                            {
                                if (double.IsInfinity(maxDropDownHeight) || double.IsNaN(maxDropDownHeight))
                                    maxDropDownHeight = ((applicationHeight - heightDropDown) * 3.0) / 5.0;
                                bool flag = true;
                                if (applicationHeight < (yBottomDropDown + popupHeight))
                                {
                                    flag = false;
                                    yBottomDropDown = yDropDown - popupHeight;
                                    if (yBottomDropDown < 0.0)
                                    {
                                        if (yDropDown < ((applicationHeight - heightDropDown) / 2.0))
                                        {
                                            flag = true;
                                            yBottomDropDown = yDropDown + heightDropDown;
                                        }
                                        else
                                        {
                                            flag = false;
                                            yBottomDropDown = yDropDown - popupHeight;
                                        }
                                    }
                                }
                                if (popupHeight != 0.0)
                                {
                                    if (flag)
                                        maxDropDownHeight = Math.Min(applicationHeight - yBottomDropDown, maxDropDownHeight);
                                    else
                                        maxDropDownHeight = Math.Min(yDropDown, maxDropDownHeight);
                                }
                            }
                            else
                            {
                                if (double.IsInfinity(maxDropDownHeight) || double.IsNaN(maxDropDownHeight))
                                    maxDropDownHeight = applicationHeight - 2 * RIGHT_CENTER_TOP_BOTTOM_MARGIN;
                            }
                            popupWidth = Math.Min(popupWidth, applicationWidth);
                            popupHeight = Math.Min(popupHeight, maxDropDownHeight);
                            popupWidth = Math.Max(widthDropDown, popupWidth);
                            double applicationRemainWidth = 0.0;
                            double leaderWidth = GetFrameworkElementWidth(LeaderPopupContent);
                            if (double.IsNaN(leaderWidth) || double.IsInfinity(leaderWidth))
                                leaderWidth = 0;

                            if (AssociatedControl.FlowDirection == FlowDirection.LeftToRight)
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
                            if (ExpandDirection == ExpandDirection.BottomLeft)
                                maxPopupWidth = applicationWidth / widthRatio;
                            else
                            {
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
                            }

                            ElementPopupChild.MaxWidth = Math.Max(minWidthDropDown, maxPopupWidth);
                            ElementPopupChild.MinHeight = heightDropDown / heightRatio;
                            ElementPopupChild.MaxHeight = Math.Max((double)0.0, (double)(maxDropDownHeight / heightRatio));
                            ElementPopupChild.HorizontalAlignment = HorizontalAlignment.Left;
                            ElementPopupChild.VerticalAlignment = VerticalAlignment.Top;
                            ElementPopupChild.FlowDirection = AssociatedControl.FlowDirection;

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
                                    Canvas.SetLeft(ElementPopupChildMaxRangeStackPanel, (isNestedPopup ? 0: xDropDown) + AssociatedControl.ActualWidth + leaderWidth);
                                }

                                if (reversePopupExpandDirection)
                                    ElementPopupChild.HorizontalAlignment = HorizontalAlignment.Right;
                                
                                SetupLeader(LeaderPopupContent, ElementPopupChildMaxRangeStackPanel, reversePopupExpandDirection, xDropDown, yDropDown, heightDropDown, ExpandDirection, AssociatedControl, isNestedPopup);
                            }
                            else
                            {
                                SetPopupTop(ElementPopupChild, yBottomDropDown, heightRatio, yDropDown, popupHeight, applicationHeight, ExpandDirection);
                                SetPopupLeft(ElementPopupChild, applicationRemainWidth, widthRatio, popupWidth, maxPopupWidth, reversePopupExpandDirection, ExpandDirection, AssociatedControl, xDropDown);
                                SetupLeader(LeaderPopupContent, ElementPopupChild, reversePopupExpandDirection, xDropDown, yDropDown, heightDropDown, ExpandDirection, AssociatedControl, isNestedPopup);
                            }
                        }
                    }
                }
            }
        }
        #region Private Methods

        private double GetFrameworkElementWidth(FrameworkElement frameworkElement)
        {
            if (frameworkElement != null)
            {
                if (!double.IsNaN(frameworkElement.ActualWidth) && !double.IsInfinity(frameworkElement.ActualWidth))
                    return frameworkElement.ActualWidth;
            }
            return 0;
        }
        private void RotateFrameworkElement(FrameworkElement element, double degrees)
        {
            element.Projection = new PlaneProjection() { RotationZ = degrees };
        }

        private void SetupLeader(FrameworkElement leader, FrameworkElement popup, bool reversePopupExpandDirection, double xDropDown, double yDropDown, double heightDropDown, ExpandDirection expandDirection, Control associatedControl, bool isNestedPopup)
        {
            if (leader != null && associatedControl != null)
            {
                leader.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                leader.VerticalAlignment = System.Windows.VerticalAlignment.Top;

                double leaderWidth = leader.ActualWidth;
                double leaderHeight = leader.ActualHeight;
                double d, leftBorder, topBorder;
                if ((leaderWidth != 0.0) && (leaderHeight != 0.0) && popup != null)
                {
                    #region Position leader, Offset dropdown to accommodate leader
                    switch (expandDirection)
                    {
                        case ExpandDirection.BottomLeft:
                            #region Left
                            d = Canvas.GetLeft(popup);
                            leftBorder = (associatedControl.BorderThickness != null) ? associatedControl.BorderThickness.Left : 0;
                            Canvas.SetLeft(leader, d + leftBorder * 2);
                            #endregion
                            #region Top
                            topBorder = (associatedControl.BorderThickness != null) ? associatedControl.BorderThickness.Top : 0;
                            d = Canvas.GetTop(popup);
                            Canvas.SetTop(leader, d + topBorder);//add border for overlap
                            Canvas.SetTop(popup, d + leaderHeight - topBorder);
                            #endregion
                            break;
                        case ExpandDirection.HorizontalCenter:

                            if (reversePopupExpandDirection)
                                RotateFrameworkElement(leader, 180);
                            else
                                RotateFrameworkElement(leader, 0);

                            #region Left
                            leftBorder = (associatedControl.BorderThickness != null) ? associatedControl.BorderThickness.Left : 0;
                            Canvas.SetLeft(leader, !reversePopupExpandDirection ? (isNestedPopup ? 0 : xDropDown) + associatedControl.ActualWidth + leftBorder : (isNestedPopup ? 0 : xDropDown) - leaderHeight - leftBorder);//add border for overlap
                            #endregion
                            #region Top
                            topBorder = (associatedControl.BorderThickness != null) ? associatedControl.BorderThickness.Top : 0;

                            Canvas.SetTop(leader, isNestedPopup ? (heightDropDown - leaderHeight) / 2 : yDropDown + (heightDropDown - leaderHeight) / 2);
                            #endregion
                            break;
                    }
                    #endregion
                }
            }
        }
        private void SetPopupLeft(FrameworkElement element, double applicationRemainWidth, double widthRatio, double popupWidth, double maxPopupWidth, bool reversePopupExpandDirection, ExpandDirection direction, Control associatedControl, double xDropDown)
        {
            switch (direction)
            {
                case ExpandDirection.HorizontalCenter:
                    {
                        if (!reversePopupExpandDirection)
                        {
                            double d = applicationRemainWidth / widthRatio;

                            if (!double.IsNaN(associatedControl.ActualWidth))
                                d += associatedControl.ActualWidth;
                            Canvas.SetLeft(element, d);
                        }
                        else
                        {
                            double d = 0;
                            if (!double.IsNaN(element.ActualWidth) || !double.IsNaN(maxPopupWidth))
                            {
                                d -= Math.Max(Math.Min(element.ActualWidth, maxPopupWidth), Math.Min(popupWidth, maxPopupWidth));
                            }
                            Canvas.SetLeft(element, d);
                        }
                    } break;
                case ExpandDirection.BottomLeft:
                default:
                    {
                        Canvas.SetLeft(element, applicationRemainWidth / widthRatio);
                    } break;
            }
        }
        private void SetPopupTop(FrameworkElement element, double yBottomDropDown, double heightRatio, double yDropDown, double popupHeight, double applicationHeight, ExpandDirection direction)
        {
            switch (direction)
            {
                case ExpandDirection.HorizontalCenter:
                    {
                        double top = (yBottomDropDown - yDropDown - popupHeight) / 2;
                        double overflow = Math.Abs(Math.Min(applicationHeight - (yDropDown + (yBottomDropDown - yDropDown) / 2 + popupHeight / 2 + RIGHT_CENTER_TOP_BOTTOM_MARGIN), 0.0));
                        if (overflow > 0)
                        {
                            top -= overflow; //move up if overflowing underneath
                        }
                        top = Math.Max(top, -yDropDown + RIGHT_CENTER_TOP_BOTTOM_MARGIN); //did our best to calculate top, so set to top or application level x=0
                        Canvas.SetTop(element, top);
                    } break;
                case ExpandDirection.BottomLeft:
                default:
                    {
                        Canvas.SetTop(element, (yBottomDropDown - yDropDown) / heightRatio);
                    } break;
            }
        }
        
        #endregion
    }
}
