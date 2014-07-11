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
    /// Adds an item to the targeted list
    /// </summary>
    public class AddItemAction : TargetedTriggerAction<IList>
    {
        protected override void Invoke(object parameter)
        {
            if (Target != null && Item != null && !Target.Contains(Item))
                Target.Add(Item);
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Item"/> property
        /// </summary>
        public static DependencyProperty ItemProperty = DependencyProperty.Register(
            "Item", typeof(object), typeof(AddItemAction), null);

        /// <summary>
        /// Gets or sets the item to add
        /// </summary>
        public object Item
        {
            get { return GetValue(ItemProperty) as object; }
            set { SetValue(ItemProperty, value); }
        }
    }
}
