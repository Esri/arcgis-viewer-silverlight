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
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client;
using System.Windows.Threading;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class FlipFrameworkElementsCommand : DependencyObject, ICommand
    {
        public enum RotationDirection
        {
            LeftToRight,
            RightToLeft,
            TopToBottom,
            BottomToTop
        }

        public class RotationDef
        {
            public double FromDegrees { get; set; }
            public double MidDegrees { get; set; }
            public double ToDegrees { get; set; }
            public string RotationProperty { get; set; }
            public PlaneProjection PlaneProjection { get; set; }
            public Duration AnimationDuration { get; set; }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
        public event EventHandler RotationCompleted;

        public void Execute(object parameter)
        {
            StartAnimation();
        }

        #region Front element
        public static readonly DependencyProperty FrontElementProperty =
            DependencyProperty.Register("FrontElement", typeof(FrameworkElement),
                                        typeof(FlipFrameworkElementsCommand), new PropertyMetadata(null));

        public FrameworkElement FrontElement
        {
            get { return this.GetValue(FrontElementProperty) as FrameworkElement; }
            set { this.SetValue(FrontElementProperty, value); }
        }
        #endregion

        #region Back element
        public static readonly DependencyProperty BackElementProperty =
            DependencyProperty.Register("BackElement", typeof(FrameworkElement),
                                        typeof(FlipFrameworkElementsCommand), new PropertyMetadata(null));

        public FrameworkElement BackElement
        {

            get { return this.GetValue(BackElementProperty) as FrameworkElement; }
            set { this.SetValue(BackElementProperty, value); }
        }
        #endregion

        #region Source element
        public static readonly DependencyProperty SourceElementProperty =
            DependencyProperty.Register("SourceElement", typeof(FrameworkElement),
                                        typeof(FlipFrameworkElementsCommand), new PropertyMetadata(null));

        public FrameworkElement SourceElement
        {

            get { return this.GetValue(SourceElementProperty) as FrameworkElement; }
            set { this.SetValue(SourceElementProperty, value); }
        }
        #endregion

        #region Duration
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(Duration),
                                        typeof(FlipFrameworkElementsCommand), new PropertyMetadata(null));

        public Duration Duration { get; set; }
        #endregion

        #region Rotation Direction
        public static readonly DependencyProperty RotationProperty =
            DependencyProperty.Register("Rotation", typeof(RotationDirection),
                                        typeof(FlipFrameworkElementsCommand), new PropertyMetadata(RotationDirection.LeftToRight));

        public RotationDirection Rotation { get; set; }
        #endregion

        private void OnFrameworkElementChanged()
        {
            OnCanExecuteChanged();
        }


        protected void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        protected void OnStoryboardCompleted()
        {
            if (RotationCompleted != null)
                RotationCompleted(this, EventArgs.Empty);
        }

        private void StartAnimation()
        {
            bool isHitTestVisible = true;
            try
            {
                if (SourceElement != null)
                {
                    isHitTestVisible = SourceElement.IsHitTestVisible;
                    SourceElement.IsHitTestVisible = false;
                }
                Projection frontBackup = null;
                Projection backBackup = null;

                if (FrontElement == null || BackElement == null) return;

                //Backup in case projections are used
                if (FrontElement.Projection != null)
                    frontBackup = FrontElement.Projection;
                if (BackElement.Projection != null)
                    backBackup = BackElement.Projection;

                FrontElement.Projection = null;
                BackElement.Projection = null;

                FrontElement.Projection = new PlaneProjection() { CenterOfRotationY = .5 };
                FrontElement.RenderTransformOrigin = new Point(.5, .5);
                FrontElement.Opacity = 0;
                BackElement.Projection = new PlaneProjection() { CenterOfRotationY = .5, RotationY = 180.0 };
                BackElement.RenderTransformOrigin = new Point(.5, .5);
                BackElement.Opacity = 0;

                RotationDef showBackRotation = null;
                RotationDef hideFrontRotation = null;

                var frontPP = new PlaneProjection();
                var backPP = new PlaneProjection();

                switch (Rotation)
                {
                    case RotationDirection.LeftToRight:
                        backPP.CenterOfRotationY = frontPP.CenterOfRotationY = 0.5;
                        showBackRotation = new RotationDef { FromDegrees = 180.0, MidDegrees = 90.0, ToDegrees = 0.0, RotationProperty = "RotationY", PlaneProjection = backPP, AnimationDuration = this.Duration };
                        hideFrontRotation = new RotationDef { FromDegrees = 0.0, MidDegrees = -90.0, ToDegrees = -180.0, RotationProperty = "RotationY", PlaneProjection = frontPP, AnimationDuration = this.Duration };
                        break;

                    case RotationDirection.RightToLeft:
                        backPP.CenterOfRotationY = frontPP.CenterOfRotationY = 0.5;
                        showBackRotation = new RotationDef { FromDegrees = -180.0, MidDegrees = -90.0, ToDegrees = 0.0, RotationProperty = "RotationY", PlaneProjection = backPP, AnimationDuration = this.Duration };
                        hideFrontRotation = new RotationDef { FromDegrees = 0.0, MidDegrees = 90.0, ToDegrees = 180.0, RotationProperty = "RotationY", PlaneProjection = frontPP, AnimationDuration = this.Duration };
                        break;

                    case RotationDirection.BottomToTop:
                        backPP.CenterOfRotationX = frontPP.CenterOfRotationX = 0.5;
                        showBackRotation = new RotationDef { FromDegrees = 180.0, MidDegrees = 90.0, ToDegrees = 0.0, RotationProperty = "RotationX", PlaneProjection = backPP, AnimationDuration = this.Duration };
                        hideFrontRotation = new RotationDef { FromDegrees = 0.0, MidDegrees = -90.0, ToDegrees = -180.0, RotationProperty = "RotationX", PlaneProjection = frontPP, AnimationDuration = this.Duration };
                        break;

                    case RotationDirection.TopToBottom:
                        backPP.CenterOfRotationX = frontPP.CenterOfRotationX = 0.5;
                        showBackRotation = new RotationDef { FromDegrees = -180.0, MidDegrees = -90.0, ToDegrees = 0.0, RotationProperty = "RotationX", PlaneProjection = backPP, AnimationDuration = this.Duration };
                        hideFrontRotation = new RotationDef { FromDegrees = 0.0, MidDegrees = 90.0, ToDegrees = 180.0, RotationProperty = "RotationX", PlaneProjection = frontPP, AnimationDuration = this.Duration };
                        break;
                }

                FrontElement.RenderTransformOrigin = new Point(.5, .5);
                BackElement.RenderTransformOrigin = new Point(.5, .5);
                FrontElement.Projection = frontPP;
                BackElement.Projection = backPP;

                Storyboard frontToBackStoryboard = new Storyboard() { Duration = this.Duration };
                // Rotation
                frontToBackStoryboard.Children.Add(CreateRotationAnimation(showBackRotation));
                frontToBackStoryboard.Children.Add(CreateRotationAnimation(hideFrontRotation));

                // Visibility
                frontToBackStoryboard.Children.Add(CreateVisibilityAnimation(showBackRotation.AnimationDuration, FrontElement, false));
                frontToBackStoryboard.Children.Add(CreateVisibilityAnimation(hideFrontRotation.AnimationDuration, BackElement, true));
                frontToBackStoryboard.Begin();
                frontToBackStoryboard.Completed += (o, e) =>
                {
                    if (SourceElement != null)
                        SourceElement.IsHitTestVisible = isHitTestVisible;

                    OnStoryboardCompleted();
                };
            }
            catch
            {
                if (SourceElement != null)
                    SourceElement.IsHitTestVisible = isHitTestVisible;
            }
        }

        private static ObjectAnimationUsingKeyFrames CreateVisibilityAnimation(Duration duration, DependencyObject element, bool show)
        {
            var animation = new ObjectAnimationUsingKeyFrames();
            animation.BeginTime = new TimeSpan(0);
            animation.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = new TimeSpan(0), Value = (show ? 0 : 100) });
            animation.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = new TimeSpan(duration.TimeSpan.Ticks / 2), Value = (show ? 100 : 0) });
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
            Storyboard.SetTarget(animation, element);
            return animation;
        }

        private static DoubleAnimation CreateRotationAnimation(RotationDef rd)
        {
            DoubleAnimation rotateAnimation = new DoubleAnimation();
            rotateAnimation.BeginTime = new TimeSpan(0);
            rotateAnimation.Duration = new TimeSpan(rd.AnimationDuration.TimeSpan.Ticks);
            rotateAnimation.From = rd.FromDegrees;
            rotateAnimation.To = rd.ToDegrees;
            rotateAnimation.EasingFunction = new QuarticEase() { EasingMode = System.Windows.Media.Animation.EasingMode.EaseInOut };

            Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath(rd.RotationProperty));
            Storyboard.SetTarget(rotateAnimation, rd.PlaneProjection);
            return rotateAnimation;
        }
    }
}
