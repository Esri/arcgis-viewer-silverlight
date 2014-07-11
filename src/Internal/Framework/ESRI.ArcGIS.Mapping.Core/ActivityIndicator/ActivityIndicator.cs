/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ESRI.ArcGIS.Mapping.Core
{
    [TemplatePart(Name = "LayoutRoot", Type = typeof(FrameworkElement))]
    public class ActivityIndicator : Control
    {
        private FrameworkElement LayoutRoot;
        public ActivityIndicator()
        {
            DefaultStyleKey = typeof(ActivityIndicator);
        }

        public bool AutoStartProgressAnimation { get; set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            LayoutRoot = GetTemplateChild("LayoutRoot") as FrameworkElement;

            if (AutoStartProgressAnimation)
            {
                if (LayoutRoot != null)
                {
                    Storyboard swirl = LayoutRoot.Resources["swirl"] as Storyboard;
                    if (swirl != null)
                        swirl.Begin();
                }
            }
        }

        public void StartProgressAnimation()
        {
            if (LayoutRoot != null)
            {
                Storyboard swirl = LayoutRoot.Resources["swirl"] as Storyboard;
                if (swirl != null)
                    swirl.Begin();
            }
        }

        public void StopProgressAnimation()
        {
            if (LayoutRoot != null)
            {
                Storyboard swirl = LayoutRoot.Resources["swirl"] as Storyboard;
                if (swirl != null)
                    swirl.Stop();
            }
        }
    }
}
