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
using System.Windows.Data;

namespace PrintTool.AddIns
{
    public class UpdateBindingOnTextChanged : Behavior<TextBox>
    {
        BindingExpression textBinding;

        protected override void OnAttached()
        {
            if (this.AssociatedObject == null)
                return;

            // Get the binding
            textBinding = this.AssociatedObject.GetBindingExpression(TextBox.TextProperty);

            // Subscribe to text changed events.
            this.AssociatedObject.TextChanged += AssociatedObject_TextChanged;

            base.OnAttached();
        }

        void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Update the binding source.
            if (textBinding != null)
                textBinding.UpdateSource();
        }

        protected override void OnDetaching()
        {
            if (this.AssociatedObject == null)
                return;

            // Clean up...
            this.textBinding = null;
            this.AssociatedObject.TextChanged -= AssociatedObject_TextChanged;

            base.OnDetaching();
        }
    }
}

