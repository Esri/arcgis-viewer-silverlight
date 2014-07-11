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

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class MapTipFillColorChangedBehavior : Behavior<ColorPalette>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.ColorPicked += AssociatedObject_ColorPicked;
            }
        }

        void AssociatedObject_ColorPicked(object sender, ColorChosenEventArgs e)
        {
            ControlTreeHelper helper = new ControlTreeHelper();
            Popup popup = helper.FindParentControl<Popup>(AssociatedObject);
            if (popup == null)
                return;

            DropDownButton dropDownButton = popup.Tag as DropDownButton;
            if (dropDownButton == null)
                return;

            MapTipsConfig mapTipsConfig = helper.FindAncestorOfType<MapTipsConfig>(dropDownButton);
            if (mapTipsConfig != null)
            {
                mapTipsConfig.MapTipFill = new SolidColorBrush(e.Color);
            }
            dropDownButton.ClosePopup();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.ColorPicked -= AssociatedObject_ColorPicked;
            }
        }
    }
}
