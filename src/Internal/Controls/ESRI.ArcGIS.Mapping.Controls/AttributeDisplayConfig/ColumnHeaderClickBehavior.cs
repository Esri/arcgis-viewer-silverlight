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
using System.Windows.Interactivity;
using ESRI.ArcGIS.Mapping.Core;
using System.Windows.Controls.Primitives;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ColumnHeaderClickBehavior : Behavior<HyperlinkButton>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject != null)
            {
                AssociatedObject.Click -= AssociatedObject_Click;
                AssociatedObject.Click += AssociatedObject_Click;
            }
        }

        void AssociatedObject_Click(object sender, RoutedEventArgs e)
        {
            DataGridColumnHeader header = ControlTreeHelper.FindAncestorOfType<DataGridColumnHeader>(AssociatedObject);
            if (header == null)
                return;

            FieldInfo fieldInfo = header.Content as FieldInfo;
            if (fieldInfo == null)
                return;

            AttributeDisplay attrDisplay = ControlTreeHelper.FindAncestorOfType<AttributeDisplay>(AssociatedObject);
            if (attrDisplay != null)
                attrDisplay.RaiseSortedEvent(fieldInfo);
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
                AssociatedObject.Click -= AssociatedObject_Click;

            base.OnDetaching();
        }
    }
}
