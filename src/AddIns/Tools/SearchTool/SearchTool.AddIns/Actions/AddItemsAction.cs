/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections;
using System.Windows;
using System.Windows.Interactivity;

namespace SearchTool
{
    /// <summary>
    /// Adds a set of items to the targeted list
    /// </summary>
    public class AddItemsAction : TargetedTriggerAction<IList>
    {
        protected override void Invoke(object parameter)
        {
            if (Target != null && Items != null)
            {
                foreach (object o in Items)
                {
                    if (!Target.Contains(o))
                        Target.Add(o);
                }
            }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Items"/> property
        /// </summary>
        public static DependencyProperty ItemsProperty = DependencyProperty.Register(
            "Items", typeof(IEnumerable), typeof(AddItemsAction), null);

        /// <summary>
        /// Gets or sets the items to add
        /// </summary>
        public IEnumerable Items
        {
            get { return GetValue(ItemsProperty) as IEnumerable; }
            set { SetValue(ItemsProperty, value); }
        }
    }
}
