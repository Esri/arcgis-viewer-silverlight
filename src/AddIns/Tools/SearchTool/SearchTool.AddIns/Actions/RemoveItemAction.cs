/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections;
using System.Windows;
using System.Windows.Interactivity;

namespace SearchTool
{
    /// <summary>
    /// Removes an item from the targeted list
    /// </summary>
    public class RemoveItemAction : TargetedTriggerAction<IList>
    {
        protected override void Invoke(object parameter)
        {
            if (Target != null && Item != null && Target.Contains(Item))
            {
                try
                {
                    Target.Remove(Item);
                }
                catch
                {
                    // Sometimes calling Remove throws an exception during layout changes
                }
            }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Item"/> property
        /// </summary>
        public static DependencyProperty ItemProperty = DependencyProperty.Register(
            "Item", typeof(object), typeof(RemoveItemAction), null);

        /// <summary>
        /// Gets or sets the item to remove
        /// </summary>
        public object Item
        {
            get { return GetValue(ItemProperty) as object; }
            set { SetValue(ItemProperty, value); }
        }
    }
}
