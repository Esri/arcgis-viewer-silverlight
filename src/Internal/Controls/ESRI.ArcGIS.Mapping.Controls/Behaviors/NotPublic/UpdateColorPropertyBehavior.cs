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
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class UpdateColorPropertyBehavior : Behavior<SolidColorBrushSelector>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.ColorPicked -= AssociatedObject_ColorPicked;
            AssociatedObject.ColorPicked += AssociatedObject_ColorPicked;
        }

        #region ApplicationColorSet
        public ApplicationColorSet ApplicationColorSet
        {
            get
            {
                return View.Instance.ApplicationColorSet;
            }
        }
        #endregion


        public string PropertyName { get; set; }

        void AssociatedObject_ColorPicked(object sender, ColorChosenEventArgs e)
        {
            if (string.IsNullOrEmpty(PropertyName))
                return;
            ThemeColorProperty property = (ThemeColorProperty)Enum.Parse(typeof(ThemeColorProperty), PropertyName, true);
            ThemeColorHelper.ApplyColorProperty(ApplicationColorSet, e.Color, property);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.ColorPicked -= AssociatedObject_ColorPicked;
        }
    }
}
