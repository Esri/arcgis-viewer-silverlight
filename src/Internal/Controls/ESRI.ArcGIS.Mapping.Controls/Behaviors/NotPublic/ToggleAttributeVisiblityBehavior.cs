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
using System.Windows.Controls.Primitives;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ToggleAttributeVisiblityBehavior : Behavior<ToggleButton>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.AssociatedObject == null)
                return;

            this.AssociatedObject.Click -= AssociatedObject_Clicked;
            this.AssociatedObject.Click += AssociatedObject_Clicked;
        }       

        void AssociatedObject_Clicked(object sender, RoutedEventArgs e)
        {
            AttributeDisplayConfig mapTipsConfig = ControlTreeHelper.FindAncestorOfType<AttributeDisplayConfig>(AssociatedObject);
            if (mapTipsConfig != null)
            {
                if(AssociatedObject.IsChecked.Value)
                    mapTipsConfig.FieldInfo_AttributeDisplayChecked(this.AssociatedObject.DataContext as FieldInfo);
                else
                    mapTipsConfig.FieldInfo_AttributeDisplayUnChecked(this.AssociatedObject.DataContext as FieldInfo);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (this.AssociatedObject == null)
                return;
            this.AssociatedObject.Click -= AssociatedObject_Clicked;
        }
    }
}
