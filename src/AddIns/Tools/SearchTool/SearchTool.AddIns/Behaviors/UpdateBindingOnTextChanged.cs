/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace SearchTool
{
    /// <summary>
    /// Forces bindings to the targeted TextBox's Text property to update on the TextChanged event
    /// </summary>
    public class UpdateBindingOnTextChanged : Behavior<TextBox>
    {
        BindingExpression textBinding;

        protected override void OnAttached()
        {
            if (AssociatedObject == null)
                return;

            // Get the binding
            textBinding = AssociatedObject.GetBindingExpression(TextBox.TextProperty);

            // Subscribe to text changed events.
            AssociatedObject.TextChanged += AssociatedObject_TextChanged;

            base.OnAttached();
        }

        private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
        {
            // The target TextBox has changed, so update the binding source.  This forces
            // properties bound to TextBox.Text to update.
            if (textBinding != null)
                textBinding.UpdateSource();
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject == null)
                return;

            textBinding = null;
            AssociatedObject.TextChanged -= AssociatedObject_TextChanged;

            base.OnDetaching();
        }
    }
}

