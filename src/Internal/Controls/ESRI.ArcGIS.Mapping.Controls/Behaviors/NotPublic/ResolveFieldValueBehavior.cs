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
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    /// <summary>
    /// Resolves the user-friendly value of a field and binds that value to a property on the 
    /// associated object. Performs domain and sub-type lookups and formats date and
    /// currency fields.
    /// </summary>
    /// <remarks>
    /// To resolve the field, the behavior makes use of 
    /// <see cref="ESRI.ArcGIS.Client.Extensibility.MapApplication.GetPopup"/> to retrieve field
    /// information.  It assumes a valid MapApplicationContext.  The field specified by the
    /// <see cref="ResolveFieldValueBehavior.FieldName"/> property must belong to the layer specified by the 
    /// <see cref="ResolveFieldValueBehavior.Layer"/> property.  The Layer must be a GraphicsLayer.
    /// </remarks>
    public class ResolveFieldValueBehavior : Behavior<DependencyObject>
    {
        #region Behavior Overrides

        protected override void OnAttached()
        {
            base.OnAttached();

            // Resolve the user-friendly field value.
            resolveValue();

            // Set binding to specified property on associated object
            if (!string.IsNullOrEmpty(TargetProperty))
                setValueBinding(TargetProperty);            
        }

        protected override void OnDetaching()
        {
            // Clear the binding
            if (!string.IsNullOrEmpty(TargetProperty))
                clearValueBinding(TargetProperty);

            base.OnDetaching();
        }

        #endregion

        #region Public Properties

        #region FieldName

        /// <summary>
        /// Backing DependencyProperty for the <see cref="FieldName"/> property
        /// </summary>
        public static readonly DependencyProperty FieldNameProperty = DependencyProperty.Register(
            "FieldName", typeof(string), typeof(ResolveFieldValueBehavior), 
            new PropertyMetadata(OnValuePropertiesChanged));

        /// <summary>
        /// Gets or sets the name of the field to get the user-friendly value for
        /// </summary>
        public string FieldName
        {
            get { return GetValue(FieldNameProperty) as string; }
            set { SetValue(FieldNameProperty, value); }
        }

        #endregion

        #region Layer

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Layer"/> property
        /// </summary>
        public static readonly DependencyProperty LayerProperty = DependencyProperty.Register(
            "Layer", typeof(Layer), typeof(ResolveFieldValueBehavior),
            new PropertyMetadata(OnValuePropertiesChanged));

        /// <summary>
        /// Gets or sets the layer containing the field specified by the <see cref="FieldName"/> property
        /// </summary>
        public Layer Layer
        {
            get { return GetValue(LayerProperty) as Layer; }
            set { SetValue(LayerProperty, value); }
        }

        #endregion

        #region Value

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Value"/> property
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(object), typeof(ResolveFieldValueBehavior),
            new PropertyMetadata(OnValuePropertiesChanged));

        /// <summary>
        /// Gets or sets the value to be resolved.  The value should belong to the field specified
        /// by the <see cref="FieldName"/> property and layer specified by the <see cref="Layer"/>
        /// property
        /// </summary>
        public object Value
        {
            get { return GetValue(ValueProperty) as object; }
            set { SetValue(ValueProperty, value); }
        }

        #endregion

        /// <summary>
        /// Backing DependencyProperty for the <see cref="TargetProperty"/> property
        /// </summary>
        public static readonly DependencyProperty TargetPropertyProperty = DependencyProperty.Register(
            "TargetProperty", typeof(string), typeof(ResolveFieldValueBehavior), 
            new PropertyMetadata(OnTargetPropertyChanged));

        /// <summary>
        /// Gets or sets the property to bind the resolved value to
        /// </summary>
        public string TargetProperty
        {
            get { return GetValue(TargetPropertyProperty) as string; }
            set { SetValue(TargetPropertyProperty, value); }
        }

        #endregion

        #region Property Changed Handlers

        private static void OnValuePropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Re-resolve the user-friendly value whenever one of the properties affecting 
            // this (Value, Layer, or FieldName) changes
            ((ResolveFieldValueBehavior)d).resolveValue();
        }

        private static void OnTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ResolveFieldValueBehavior behavior = (ResolveFieldValueBehavior)d;

            // Update bindings to reflect the new target property

            if (e.OldValue is string)
                behavior.clearValueBinding((string)e.OldValue);

            if (e.NewValue is string)
                behavior.setValueBinding((string)e.NewValue);
        }

        #endregion

        /// <summary>
        /// Backing DependencyProperty for the <see cref="ResolvedValue"/> property
        /// </summary>
        private static readonly DependencyProperty ResolvedValueProperty = DependencyProperty.Register(
            "ResolvedValue", typeof(object), typeof(ResolveFieldValueBehavior), null);

        /// <summary>
        /// Gets or sets the resolved user-friendly value
        /// </summary>
        private object ResolvedValue
        {
            get { return GetValue(ResolvedValueProperty) as object; }
            set { SetValue(ResolvedValueProperty, value); }
        }

        #region Private Methods 
        
        // Resolves a user-friendly value for a given input value, field, and layer
        private void resolveValue()
        {
            if (!string.IsNullOrEmpty(FieldName) && Value != null && Layer != null && MapApplication.Current != null)
            {
                // Create a dummy graphic with the value to resolve and the field the value belongs to
                Graphic g = new Graphic();
                g.Attributes.Add(FieldName, Value);

                // Get the popup info for the graphic and the layer to which the input value belongs.  This will initialize
                // and give access to field metadata for the layer
                OnClickPopupInfo info = MapApplication.Current.GetPopup(g, Layer);

                // Resolve the value into its user-friendly equivalent
                if (info != null)
                    ResolvedValue = ConverterUtil.GetValue(info.PopupItem, FieldName);
                else
                    ResolvedValue = null;
            }
            else
            {
                ResolvedValue = null;
            }
        }

        // Binds the resolved value to the target property on the behavior's associated object
        private void setValueBinding(string propertyName)
        {
            if (AssociatedObject == null)
                return;

            DependencyProperty dp = getDependencyProperty(AssociatedObject.GetType(), propertyName);
            if (dp != null)
            {
                Binding b = new Binding("ResolvedValue") { Source = this };
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
            return (fieldInfo != null) ? (DependencyProperty)fieldInfo.GetValue(null) : null;
        }

        #endregion
    }
}
