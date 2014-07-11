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
using System.Windows.Data;

namespace ESRI.ArcGIS.Mapping.Behaviors
{
    /// <summary>
    /// Forces bindings to update when the text changes.  Can be used on <see cref="TextBox"/> and
    /// <see cref="PasswordBox"/> controls
    /// </summary>
    public class UpdateBindingOnTextChanged : Behavior<Control>
    {
        BindingExpression textBinding;
    
        protected override void OnAttached()
        {
            if (!(AssociatedObject is TextBox) && !(AssociatedObject is PasswordBox))
                return;

            // Get the binding and subscribe to text changed events
            if (AssociatedObject is TextBox)
            {
                textBinding = AssociatedObject.GetBindingExpression(TextBox.TextProperty);
                ((TextBox)AssociatedObject).TextChanged += AssociatedObject_TextChanged;
            }
            else if (AssociatedObject is PasswordBox)
            {
                textBinding = AssociatedObject.GetBindingExpression(PasswordBox.PasswordProperty);
                ((PasswordBox)AssociatedObject).PasswordChanged += AssociatedObject_TextChanged;
            }

            base.OnAttached();
        }

        void AssociatedObject_TextChanged(object sender, EventArgs e)
        {
            // Update the binding source.
            if (textBinding != null)
                textBinding.UpdateSource();
        }

        protected override void OnDetaching()
        {
            if (!(AssociatedObject is TextBox) && !(AssociatedObject is PasswordBox))
                return;

            // Clean up...
            textBinding = null;
            if (AssociatedObject is TextBox)
                ((TextBox)AssociatedObject).TextChanged -= AssociatedObject_TextChanged;
            else if (AssociatedObject is PasswordBox)
                ((PasswordBox)AssociatedObject).PasswordChanged -= AssociatedObject_TextChanged;

            base.OnDetaching();
        }
    }
}
