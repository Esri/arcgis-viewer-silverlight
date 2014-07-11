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
    /// Adds an item to the targeted dictionary
    /// </summary>
    public class AddDictionaryItemAction : TargetedTriggerAction<IDictionary>
    {
        protected override void Invoke(object parameter)
        {
            if (Target != null && Key != null && !Target.Contains(Key))
                Target.Add(Key, Value);
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Value"/> property
        /// </summary>
        public static DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(object), typeof(AddDictionaryItemAction), null);

        /// <summary>
        /// Gets or sets the value of the item to add
        /// </summary>
        public object Value
        {
            get { return GetValue(ValueProperty) as object; }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Key"/> property
        /// </summary>
        public static DependencyProperty KeyProperty = DependencyProperty.Register(
            "Key", typeof(string), typeof(AddDictionaryItemAction), null);

        /// <summary>
        /// Gets or sets the key of the item to add
        /// </summary>
        public string Key
        {
            get { return GetValue(KeyProperty) as string; }
            set { SetValue(KeyProperty, value); }
        }
    }
}
