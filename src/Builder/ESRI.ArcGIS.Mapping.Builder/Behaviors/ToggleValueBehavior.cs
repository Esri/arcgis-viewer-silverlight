/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace ESRI.ArcGIS.Mapping.Builder
{
    /// <summary>
    /// Toggles a property value on the associated object when two properties on the behavior match.  When 
    /// the comparison properties do not match, the target property value reverts to its previous value. 
    /// </summary>
    public class ToggleValueBehavior : Behavior<DependencyObject>
    {
        private object _lastTargetValue;
        private DependencyProperty _targetProperty;
        private ThrottleTimer _setBindingThrottler;

        #region Behavior Overrides

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject != null)
            {
                // Retrieve the dependency property specified by TargetProperty
                if (_targetProperty == null && !string.IsNullOrEmpty(TargetProperty))
                    updateTargetDependencyProperty(TargetProperty);

                // Check whether the comparison values match
                compareValues();
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (!string.IsNullOrEmpty(TargetProperty))
                clearValueBinding(TargetProperty);
        }

        #endregion

        #region ToggleValue

        /// <summary>
        /// Backing DependencyProperty for the <see cref="ToggleValue"/> property
        /// </summary>
        public static readonly DependencyProperty ToggleValueProperty = DependencyProperty.Register(
            "ToggleValue", typeof(object), typeof(ToggleValueBehavior),
            new PropertyMetadata(OnValuePropertyChanged));

        /// <summary>
        /// Gets or sets the value to set the target property to when the comparison properties match
        /// </summary>
        public object ToggleValue
        {
            get { return GetValue(ToggleValueProperty) as object; }
            set { SetValue(ToggleValueProperty, value); }
        }

        #endregion

        #region ValueOne

        /// <summary>
        /// Backing DependencyProperty for the <see cref="ValueOne"/> property
        /// </summary>
        public static readonly DependencyProperty ValueOneProperty = DependencyProperty.Register(
            "ValueOne", typeof(object), typeof(ToggleValueBehavior),
            new PropertyMetadata(OnValuePropertyChanged));

        /// <summary>
        /// Gets or sets the first comparison value
        /// </summary>
        public object ValueOne
        {
            get { return GetValue(ValueOneProperty) as object; }
            set { SetValue(ValueOneProperty, value); }
        }

        #endregion

        #region ValueTwo

        /// <summary>
        /// Backing DependencyProperty for the <see cref="ValueTwo"/> property
        /// </summary>
        public static readonly DependencyProperty ValueTwoProperty = DependencyProperty.Register(
            "ValueTwo", typeof(object), typeof(ToggleValueBehavior),
            new PropertyMetadata(OnValuePropertyChanged));

        /// <summary>
        /// Gets or sets the second comparison value
        /// </summary>
        public object ValueTwo
        {
            get { return GetValue(ValueTwoProperty) as object; }
            set { SetValue(ValueTwoProperty, value); }
        }

        #endregion

        #region TargetProperty

        /// <summary>
        /// Backing DependencyProperty for the <see cref="TargetProperty"/> property
        /// </summary>
        public static readonly DependencyProperty TargetPropertyProperty = DependencyProperty.Register(
            "TargetProperty", typeof(string), typeof(ToggleValueBehavior),
            new PropertyMetadata(OnTargetPropertyChanged));

        /// <summary>
        /// Gets or sets the property to apply <see cref="ToggleValue"/> to
        /// </summary>
        public string TargetProperty
        {
            get { return GetValue(TargetPropertyProperty) as string; }
            set { SetValue(TargetPropertyProperty, value); }
        }

        #endregion

        // Fires when one of the properties changes that affects the comparison or the toggle value
        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Check whether the comparison values are equal and set the target value accordingly
            ((ToggleValueBehavior)d).compareValues();
        }

        // Fires when the property to update is changed
        private static void OnTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleValueBehavior behavior = (ToggleValueBehavior)d;

            // Update bindings to reflect the new target property

            if (e.OldValue is string)
                behavior.clearValueBinding((string)e.OldValue);

            if (e.NewValue is string)
            {
                string propertyName = (string)e.NewValue;
                behavior.updateTargetDependencyProperty(propertyName);
                behavior.compareValues();
            }
        }


        /// <summary>
        /// Backing DependencyProperty for the <see cref="TargetValue"/> property
        /// </summary>
        private static readonly DependencyProperty TargetValueProperty = DependencyProperty.Register(
            "TargetValue", typeof(object), typeof(ToggleValueBehavior), null);

        /// <summary>
        /// Gets or sets the output of the value comparison.  This property is bound to the property specified
        /// by TargetProperty.
        /// </summary>
        private object TargetValue
        {
            get { return GetValue(TargetValueProperty) as object; }
            set { SetValue(TargetValueProperty, value); }
        }


        #region Private Methods 

        // Binds the output value to the target property on the behavior's associated object
        private void setValueBinding(string propertyName)
        {
            if (AssociatedObject == null)
                return;

            if (_targetProperty != null)
            {
                Binding b = new Binding("TargetValue") { Source = this };
                BindingOperations.SetBinding(AssociatedObject, _targetProperty, b);
            }
        }

        // Clears any binding from the target property on the behavior's associated object
        private void clearValueBinding(string propertyName)
        {
            if (AssociatedObject == null)
                return;

            if (_targetProperty != null)
            {
                Binding b = new Binding();
                BindingOperations.SetBinding(AssociatedObject, _targetProperty, b);
            }
        }

        // Retrieve the DependencyProperty specified by TargetProperty
        private void updateTargetDependencyProperty(string propertyName)
        {
            if (AssociatedObject == null)
                return;
            _targetProperty = getDependencyProperty(AssociatedObject.GetType(), propertyName);
            if (_targetProperty != null && AssociatedObject != null)
                _lastTargetValue = AssociatedObject.GetValue(_targetProperty);
        }

        // Retrieves the dependency property corresponding to the passed-in property name
        private DependencyProperty getDependencyProperty(Type type, string name)
        {
            string originalName = name;

            // By convention, dependency property names end with "Property"
            if (!name.EndsWith("Property"))
                name += "Property";

            // Attempt to get the field for the property
            System.Reflection.FieldInfo fieldInfo = type.GetField(name, BindingFlags.Public | BindingFlags.Static);

            // If the attempt failed, append "Property" one more time and try again.  Necessary in the case where
            // the non-dependency property name already ends with property (e.g. TargetProperty)
            if (fieldInfo == null)
            {
                name += "Property";
                fieldInfo = type.GetField(name, BindingFlags.Public | BindingFlags.Static);
            }

            if (fieldInfo != null) // return the property if found
                return (DependencyProperty)fieldInfo.GetValue(null);
            else if (type.BaseType != null) // look for the property on the base type if not
                return getDependencyProperty(type.BaseType, originalName);
            else // return null if property not found and there is no base type
                return null;
        }

        // Evaluate the comparison values and set the target value accordingly
        private void compareValues()
        {
            if (AssociatedObject == null || _targetProperty == null)
                return;

            // Re-establish the binding between TargetValue and the target property.  Throttle the call to avoid
            // errors resulting from multiple calls stepping on one another.
            if (_setBindingThrottler == null)
                _setBindingThrottler = new ThrottleTimer(10) { Action = () => { setValueBinding(TargetProperty); } };
            _setBindingThrottler.Invoke();

            if (ValueTwo == ValueOne || (ValueTwo != null && ValueTwo.Equals(ValueOne)))
            {
                // Comparison values are equivalent

                // Hold on to the current value so it can be restored later
                _lastTargetValue = AssociatedObject.GetValue(_targetProperty);

                // Apply the target value
                TargetValue = ToggleValue;
            }
            else
            {
                // Comparison values are not equivalent.  Apply the previous value.
                TargetValue = _lastTargetValue;
            }
        }

        #endregion
    }
}
