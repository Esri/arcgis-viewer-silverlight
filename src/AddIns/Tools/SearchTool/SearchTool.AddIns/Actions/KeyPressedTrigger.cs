/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace SearchTool
{
    /// <summary>
    /// Trigger that fires when the specified key is pressed
    /// </summary>
    public class KeyPressedTrigger : TriggerBase<UIElement>
    {
        // Tracks the keys that have been pressed.  Need to monitor this to avoid 
        // firing actions when key combinations are used.
        private List<Key> _depressedKeys = new List<Key>(); 

        protected override void OnAttached()
        {
            base.OnAttached();

            // Hook to the object's key pressed events
            AssociatedObject.KeyDown += AssociatedObject_KeyDown;
            AssociatedObject.KeyUp += AssociatedObject_KeyUp;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.KeyUp -= AssociatedObject_KeyUp;
            AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
        }

        private void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
        {
            // Update which keys are depressed
            if (!_depressedKeys.Contains(e.Key))
                _depressedKeys.Add(e.Key);

            // Fire associated actions if the trigger key is the only one pressed
            if (e.Key == Key && _depressedKeys.Count == 1)
                InvokeActions(null);
        }

        private void AssociatedObject_KeyUp(object sender, KeyEventArgs e)
        {
            // Update the collection of depressed keys
            if (_depressedKeys.Contains(e.Key))
                _depressedKeys.Remove(e.Key);
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Key"/> property
        /// </summary>
        public static DependencyProperty KeyProperty = DependencyProperty.Register(
            "Key", typeof(Key), typeof(KeyPressedTrigger), new PropertyMetadata(Key.Enter));

        /// <summary>
        /// Gets or sets which key will invoke actions when pressed
        /// </summary>
        public Key Key
        {
            get { return (Key)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }
    }
}
