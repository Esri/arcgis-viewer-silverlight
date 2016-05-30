/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Mapping.Controls;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class ShowLayerConfigurationToolPanelCommand : DependencyObject, ICommand
    {
        #region Show Direction
        public static readonly DependencyProperty ShowProperty =
            DependencyProperty.Register("Show", typeof(bool),
                                        typeof(ShowLayerConfigurationToolPanelCommand), new PropertyMetadata(false));

        public bool Show { get; set; }
        #endregion

        #region Duration
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(Duration),
                                        typeof(ShowLayerConfigurationToolPanelCommand), new PropertyMetadata(null));

        public Duration Duration { get; set; }
        #endregion

        // This was added so that Builder can assign its toolPanel and have this logic make use of it instead of its
        // standard behavior which is to find a named toolPanel in the Viewer application and use that. Builder now has
        // its own panel with this control and all operations need to be performed in that UI and this allows the
        // Builder toolPanel to be manipulated properly.
        public FrameworkElement LayerConfigurationToolPanel { get; set; }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged(EventArgs args)
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, args);
        }

        public void Execute(object parameter)
        {
            ShowHideLayerConfigToolPanel();
        }

        private void ShowHideLayerConfigToolPanel()
        {
            if(LayerConfigurationToolPanel != null)
                ApplyToolPanelAnimation(LayerConfigurationToolPanel);
        }

        private void ApplyToolPanelAnimation(FrameworkElement element)
        {
            Storyboard frontToBackStoryboard = new Storyboard() { Duration = this.Duration };
            frontToBackStoryboard.Children.Add(CreateVisibilityAnimation(Duration, element, Show));
            frontToBackStoryboard.Begin();
        }

        private static ObjectAnimationUsingKeyFrames CreateVisibilityAnimation(Duration duration, DependencyObject element, bool show)
        {
            var animation = new ObjectAnimationUsingKeyFrames();
            animation.BeginTime = duration.HasTimeSpan ? duration.TimeSpan : new TimeSpan();
            animation.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = new TimeSpan(0), Value = (show ? Visibility.Visible : Visibility.Collapsed) });
            Storyboard.SetTargetProperty(animation, new PropertyPath("Visibility"));
            Storyboard.SetTarget(animation, element);
            return animation;
        }

    }
}
