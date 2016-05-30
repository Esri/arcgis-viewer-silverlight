/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SearchTool.Resources;

namespace SearchTool
{
    /// <summary>
    /// Monitors whether a collection contains an item and sets the specified property on the associated
    /// object with a specified value
    /// </summary>
    public class ContainsItemBehavior : Behavior<DependencyObject>
    {
        private Type _expectedItemType;

        #region Behavior Overrides

        protected override void OnAttached()
        {
            base.OnAttached();

            // Establish binding of contains result to target property on associated object
            if (AssociatedObject != null && !string.IsNullOrEmpty(TargetProperty))
                setValueBinding(TargetProperty);
        }

        protected override void OnDetaching()
        {
            // Clear the binding
            if (!string.IsNullOrEmpty(TargetProperty))
                clearValueBinding(TargetProperty);

            // Remove collection changed handler
            if (Collection is INotifyCollectionChanged)
                Collection.CollectionChanged -= OnCollectionChanged;

            base.OnDetaching();
        }

        #endregion

        #region Public Properties

        #region Collection

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Collection"/> property
        /// </summary>
        public static readonly DependencyProperty CollectionProperty = DependencyProperty.Register(
            "Collection", typeof(INotifyCollectionChanged), typeof(ContainsItemBehavior),
            new PropertyMetadata(OnCollectionPropertyChanged));

        /// <summary>
        /// Gets or sets the collection to monitor
        /// </summary>
        public INotifyCollectionChanged Collection
        {
            get { return GetValue(CollectionProperty) as INotifyCollectionChanged; }
            set { SetValue(CollectionProperty, value); }
        }

        #endregion

        #region Item

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Item"/> property
        /// </summary>
        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(
            "Item", typeof(object), typeof(ContainsItemBehavior),
            new PropertyMetadata(OnValuePropertiesChanged));

        /// <summary>
        /// Gets or sets the item to check whether the collection contains
        /// </summary>
        public object Item
        {
            get { return GetValue(ItemProperty) as object; }
            set { SetValue(ItemProperty, value); }
        }

        #endregion

        #region TrueValue

        /// <summary>
        /// Backing DependencyProperty for the <see cref="TrueValue"/> property
        /// </summary>
        public static readonly DependencyProperty TrueValueProperty = DependencyProperty.Register(
            "TrueValue", typeof(object), typeof(ContainsItemBehavior),
            new PropertyMetadata(OnValuePropertiesChanged));

        /// <summary>
        /// Gets or sets the value to set the target property to when the item is present in the collection
        /// </summary>
        public object TrueValue
        {
            get { return GetValue(TrueValueProperty) as object; }
            set { SetValue(TrueValueProperty, value); }
        }

        #endregion

        #region FalseValue

        /// <summary>
        /// Backing DependencyProperty for the <see cref="FalseValue"/> property
        /// </summary>
        public static readonly DependencyProperty FalseValueProperty = DependencyProperty.Register(
            "FalseValue", typeof(object), typeof(ContainsItemBehavior),
            new PropertyMetadata(OnValuePropertiesChanged));

        /// <summary>
        /// Gets or sets the value to set the target property to when the item is not present in the collection
        /// </summary>
        public object FalseValue
        {
            get { return GetValue(FalseValueProperty) as object; }
            set { SetValue(FalseValueProperty, value); }
        }

        #endregion

        #region TargetProperty

        /// <summary>
        /// Backing DependencyProperty for the <see cref="TargetProperty"/> property
        /// </summary>
        public static readonly DependencyProperty TargetPropertyProperty = DependencyProperty.Register(
            "TargetProperty", typeof(string), typeof(ContainsItemBehavior), 
            new PropertyMetadata(OnTargetPropertyChanged));

        /// <summary>
        /// Gets or sets the property to apply <see cref="TrueValue"/> or <see cref="FalseValue"/> to
        /// </summary>
        public string TargetProperty
        {
            get { return GetValue(TargetPropertyProperty) as string; }
            set { SetValue(TargetPropertyProperty, value); }
        }

        #endregion

        #endregion

        #region Property Changed Handlers

        // Fires when a collection is specified
        private static void OnCollectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ContainsItemBehavior behavior = (ContainsItemBehavior)d;

            if (e.NewValue is INotifyCollectionChanged)
            {
                // Validate the new collection to ensure that it has a Contains method
                // with the expected signature
                MethodInfo info = e.NewValue.GetType().GetMethod("Contains");
                if (info == null)
                    throw new ArgumentException(Strings.ContainsErrorMessage);

                if (info.ReturnType != typeof(bool) && info.ReturnType != typeof(bool?))
                    throw new ArgumentException(Strings.ContainsErrorMessage);

                ParameterInfo[] parameters = info.GetParameters();
                if (parameters.Length != 1)
                    throw new ArgumentException(Strings.ContainsErrorMessage);

                // Store the type of item the Contains method expects
                behavior._expectedItemType = parameters[0].ParameterType;

                // Hook-up to check whether the collection contains the item when the collection changes
                ((INotifyCollectionChanged)e.NewValue).CollectionChanged += behavior.OnCollectionChanged;

                // Check whether the item is present in the collection
                behavior.checkContains();
            }

            // Unhook from the previous collection
            if (e.OldValue is INotifyCollectionChanged)
                ((INotifyCollectionChanged)e.OldValue).CollectionChanged -= behavior.OnCollectionChanged;
        }

        // Fires when the specified collection changes
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            checkContains();
        }

        // Fires when one of the properties changes that affects the contains check or the output values 
        private static void OnValuePropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Check whether the collection contains the item and update the output value
            ((ContainsItemBehavior)d).checkContains();
        }

        // Fires when the property to update is changed
        private static void OnTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ContainsItemBehavior behavior = (ContainsItemBehavior)d;

            // Update bindings to reflect the new target property

            if (e.OldValue is string)
                behavior.clearValueBinding((string)e.OldValue);

            if (e.NewValue is string)
                behavior.setValueBinding((string)e.NewValue);
        }

        #endregion

        /// <summary>
        /// Backing DependencyProperty for the <see cref="OutputValue"/> property
        /// </summary>
        private static readonly DependencyProperty OutputValueProperty = DependencyProperty.Register(
            "OutputValue", typeof(object), typeof(ContainsItemBehavior), null);

        /// <summary>
        /// Gets or sets the output of the contains check
        /// </summary>
        private object OutputValue
        {
            get { return GetValue(OutputValueProperty) as object; }
            set { SetValue(OutputValueProperty, value); }
        }

        #region Private Methods 
        
        // Checks whether the specified collection contains the specified item and updates the output
        // value accordingly
        private void checkContains()
        {
            if (Collection != null && Item != null)
            {
                // Make sure the specified item is of a type that can be casted to the type expected
                // by the collection's Contains method
                if (!_expectedItemType.IsAssignableFrom(Item.GetType()))
                {
                    string errorMessage = string.Format(Strings.ExpectedItemTypeMismatch,
                        Collection.GetType().AssemblyQualifiedName, _expectedItemType.AssemblyQualifiedName, 
                        Item.GetType().AssemblyQualifiedName);
                    throw new ArgumentException(errorMessage);
                }

                // Cast the item and collection to dynamically resolved items.  This allows the framework
                // to do type-checking and casting.
                dynamic item = (dynamic)Item;
                dynamic coll = (dynamic)Collection;

                // Update the output value
                if (coll.Contains(item))
                    OutputValue = TrueValue;
                else
                    OutputValue = FalseValue;
            }
            else
            {
                OutputValue = FalseValue;
            }
        }

        // Binds the output value to the target property on the behavior's associated object
        private void setValueBinding(string propertyName)
        {
            if (AssociatedObject == null)
                return;

            DependencyProperty dp = getDependencyProperty(AssociatedObject.GetType(), propertyName);
            if (dp != null)
            {
                Binding b = new Binding("OutputValue") { Source = this };
                BindingOperations.SetBinding(AssociatedObject, dp, b);
            }
        }

        // Clears any binding from the target property on the behavior's associated object
        private void clearValueBinding(string propertyName)
        {
            if (AssociatedObject == null)
                return;

            DependencyProperty dp = getDependencyProperty(AssociatedObject.GetType(), propertyName);
            if (dp != null)
            {
                Binding b = new Binding();
                BindingOperations.SetBinding(AssociatedObject, dp, b);
            }
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

        #endregion
    }
}
