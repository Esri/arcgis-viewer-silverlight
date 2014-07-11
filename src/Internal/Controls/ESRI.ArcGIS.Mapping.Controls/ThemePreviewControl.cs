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

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ThemePreviewControl : Control
    {
        internal Button CloseButton;

        public ThemePreviewControl()
        {
            DefaultStyleKey = typeof(ThemePreviewControl);
        }

        public override void OnApplyTemplate()
        {
            if (CloseButton != null)
                CloseButton.Click -= CloseButton_Click;

            base.OnApplyTemplate();

            CloseButton = GetTemplateChild("CloseButton") as Button;
            if(CloseButton != null)
                CloseButton.Click += CloseButton_Click;
        }

        void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            OnClosed(EventArgs.Empty);
        }

        protected virtual void OnClosed(EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }

        public event EventHandler Closed;
    }
}
