/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = "OkButton", Type = typeof(Button))]
    public class ErrorDisplay : Control
    {
        private Button okButton;
 
        public ErrorDisplay()
        {
            this.DefaultStyleKey = typeof(ErrorDisplay);
        }

        public override void OnApplyTemplate()
        {
             base.OnApplyTemplate();
            okButton = GetTemplateChild("OkButton") as Button;
            if (okButton != null)
                okButton.Click -= okButton_Click;
            okButton.Click += okButton_Click;
        }

        void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (Completed != null)
                Completed(this, EventArgs.Empty);

        }

        public event EventHandler<EventArgs> Completed;


    }

    
}
