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

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class RequiredTextInTextBoxBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject != null)
            {
                AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
                AssociatedObject.LostFocus -= AssociatedObject_LostFocus;

                AssociatedObject.GotFocus += AssociatedObject_GotFocus;
                AssociatedObject.LostFocus += AssociatedObject_LostFocus;
            }
        }

        string textValue;
        void AssociatedObject_GotFocus(object sender, RoutedEventArgs e)
        {
            if(AssociatedObject != null)
                textValue = AssociatedObject.Text;
        }

        void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
        {
            if (AssociatedObject != null)
            {
                if(string.IsNullOrWhiteSpace(AssociatedObject.Text.Trim()))
                {
                    AssociatedObject.Text = textValue ?? string.Empty;
                }
            }
        }        

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
                AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
            }

            base.OnDetaching();
        }
    }
}
