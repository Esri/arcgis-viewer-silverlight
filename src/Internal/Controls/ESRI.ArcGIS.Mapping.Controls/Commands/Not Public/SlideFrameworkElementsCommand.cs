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
    public class SlideFrameworkElementsCommand : DependencyObject, ICommand
    {
        public enum SlideDirection
        {
            SlideLeftToRight,
            SlideRightToLeft,
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            StartAnimation();
        }

        #region Front element
        public static readonly DependencyProperty TargetElementProperty =
            DependencyProperty.Register("FrontElement", typeof(FrameworkElement),
                                        typeof(SlideFrameworkElementsCommand), new PropertyMetadata(null));

        public FrameworkElement FrontElement
        {
            get { return this.GetValue(TargetElementProperty) as FrameworkElement; }
            set { this.SetValue(TargetElementProperty, value); }
        }
        #endregion

        #region Back element
        public static readonly DependencyProperty BackElementProperty =
            DependencyProperty.Register("BackElement", typeof(FrameworkElement),
                                        typeof(SlideFrameworkElementsCommand), new PropertyMetadata(null));

        public FrameworkElement BackElement
        {
            get { return this.GetValue(BackElementProperty) as FrameworkElement; }
            set { this.SetValue(BackElementProperty, value); }
        }
        #endregion

        #region Source element
        public static readonly DependencyProperty SourceElementProperty =
            DependencyProperty.Register("SourceElement", typeof(FrameworkElement),
                                        typeof(SlideFrameworkElementsCommand), new PropertyMetadata(null));

        public FrameworkElement SourceElement
        {

            get { return this.GetValue(SourceElementProperty) as FrameworkElement; }
            set { this.SetValue(SourceElementProperty, value); }
        }
        #endregion

        #region Duration
        public Duration Duration { get; set; }
        #endregion

        #region Rotation Direction
        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register("Direction", typeof(SlideDirection),
                                        typeof(SlideFrameworkElementsCommand), new PropertyMetadata(SlideDirection.SlideLeftToRight));

        public SlideDirection Direction
        {
            get { return (SlideDirection)this.GetValue(DirectionProperty); }
            set { this.SetValue(DirectionProperty, value); }
        }
        #endregion

        protected void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
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

                if (FrontElement == null || BackElement == null || Duration == null || Duration.TimeSpan == null) return;

                Storyboard frontToBackStoryboard = new Storyboard() { Duration = this.Duration };
                // Rotation
                frontToBackStoryboard.Children.Add(CreateFrontElementSlideAnimation(Direction, FrontElement, Duration));
                frontToBackStoryboard.Children.Add(CreateFrontElementVisibilityAnimation(FrontElement, Direction == SlideDirection.SlideLeftToRight, Duration));
                frontToBackStoryboard.Children.Add(CreateBackElementSlideAnimation(Direction, BackElement, this.Duration));
                frontToBackStoryboard.Children.Add(CreateFrontElementVisibilityAnimation(BackElement, Direction == SlideDirection.SlideRightToLeft, Duration));

                // Visibility
                frontToBackStoryboard.Begin();
                frontToBackStoryboard.Completed += (o, e) =>
                {
                    if (SourceElement != null)
                        SourceElement.IsHitTestVisible = isHitTestVisible;
                };
            }
            catch
            {
                if (SourceElement != null)
                    SourceElement.IsHitTestVisible = isHitTestVisible;
            }
        }

        private static ObjectAnimationUsingKeyFrames CreateFrontElementVisibilityAnimation(FrameworkElement element, bool show, Duration duration)
        {
            var animation = new ObjectAnimationUsingKeyFrames();
            if(show)
                animation.BeginTime = new TimeSpan(0);
            else
                animation.BeginTime = new TimeSpan(duration.TimeSpan.Ticks);

            animation.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = new TimeSpan(0), Value = show ? 100 : 0 });
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
            Storyboard.SetTarget(animation, element);
            //as we toggle opacity, and not visibility, make sure that we disable the mouse capture of the element behind
            element.IsHitTestVisible = show;
            return animation;
        }

        private static DoubleAnimationUsingKeyFrames CreateBackElementSlideAnimation(SlideDirection direction, FrameworkElement elementToMove, Duration duration)
        {
            var animation = new DoubleAnimationUsingKeyFrames();
            double offset = elementToMove.ActualWidth;
            animation.BeginTime = new TimeSpan(0);
            animation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = new TimeSpan(0), Value = direction == SlideDirection.SlideLeftToRight ? 0 : offset });
            animation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = new TimeSpan(duration.TimeSpan.Ticks), Value = direction == SlideDirection.SlideLeftToRight ? offset : 0 });
            Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
            Storyboard.SetTarget(animation, elementToMove);
            return animation;
        }

        private static DoubleAnimationUsingKeyFrames CreateFrontElementSlideAnimation(SlideDirection direction, FrameworkElement elementToMove, Duration duration)
        {
            var animation = new DoubleAnimationUsingKeyFrames();
            double offset = elementToMove.ActualWidth;
            animation.BeginTime = new TimeSpan(0);
            animation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = new TimeSpan(0), Value = direction == SlideDirection.SlideLeftToRight ? -offset : 0 });
            animation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = new TimeSpan(duration.TimeSpan.Ticks), Value = direction == SlideDirection.SlideLeftToRight ? 0 : -offset });
            Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
            Storyboard.SetTarget(animation, elementToMove);
            return animation;
        }
    }
}
