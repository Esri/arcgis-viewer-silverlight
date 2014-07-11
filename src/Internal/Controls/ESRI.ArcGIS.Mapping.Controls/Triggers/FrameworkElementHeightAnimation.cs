/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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
using System.Windows.Interactivity;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class FrameworkElementHeightAnimation : TargetedTriggerAction<FrameworkElement>
    {
        protected override void Invoke(object parameter)
        {
            FrameworkElement element = Target as FrameworkElement;
            if (element != null)
            {
                Storyboard frontToBackStoryboard = new Storyboard() { Duration = new Duration(new TimeSpan(1000)) };
                frontToBackStoryboard.Children.Add(CreateHeightAnimation(frontToBackStoryboard.Duration, element));
                frontToBackStoryboard.Begin();
            }
        }

        private static ObjectAnimationUsingKeyFrames CreateHeightAnimation(Duration duration, FrameworkElement element)
        {
            var animation = new ObjectAnimationUsingKeyFrames();
            animation.BeginTime = new TimeSpan(0);
            animation.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = new TimeSpan(0), Value = element.ActualHeight });
            animation.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = new TimeSpan(duration.TimeSpan.Ticks), Value = (element.ActualHeight == 0 ? 25 : 0) });
            Storyboard.SetTargetProperty(animation, new PropertyPath("Height"));
            Storyboard.SetTarget(animation, element);
            return animation;
        }
    }
}
