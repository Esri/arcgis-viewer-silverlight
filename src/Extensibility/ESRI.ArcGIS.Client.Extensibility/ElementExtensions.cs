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

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Provides members that extend the capabilities of Silverlight elements
    /// </summary>
    public static class ElementExtensions
    {
        #region IsConfigurable
        /// <summary>
        /// Gets the value of the IsConfigurable attached property for a specified FrameworkElement.
        /// </summary>
        /// <param name="element">The FrameworkElement from which the property value is read.</param>
        /// <returns>The IsConfigurable property value for the FrameworkElement.</returns>
        public static bool GetIsConfigurable(FrameworkElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (bool)element.GetValue(IsConfigurableProperty);
        }

        /// <summary>
        /// Sets the value of the IsConfigurable attached property to a specified FrameworkElement.
        /// </summary>
        /// <param name="element">The FrameworkElement to which the attached property is written.</param>
        /// <param name="value">The needed IsConfigurable value.</param>
        public static void SetIsConfigurable(FrameworkElement element, bool value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(IsConfigurableProperty, value);
        }

        /// <summary>
        /// Identifies the IsConfigurable attached dependency property.
        /// </summary>
        public static readonly DependencyProperty IsConfigurableProperty =
            DependencyProperty.RegisterAttached(
                "IsConfigurable",
                typeof(bool),
                typeof(FrameworkElement),
                new PropertyMetadata(false, OnIsConfigurablePropertyChanged));

        /// <summary>
        /// IsConfigurableProperty property changed handler.
        /// </summary>
        /// <param name="d">MapApplication that changed its IsConfigurable.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnIsConfigurablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement elem = d as FrameworkElement;
            if (elem != null && MapApplication.Current != null && (bool)e.NewValue)
            {
                IApplicationAdmin appAdmin = MapApplication.Current as IApplicationAdmin;
                if (appAdmin != null && appAdmin.ConfigurableControls != null)
                    appAdmin.ConfigurableControls.Add(elem);
            }
        }
        #endregion

        #region DisplayName
        /// <summary>
        /// Gets the display name of a specified FrameworkElement. 
        /// </summary>
        /// <remarks>
        /// The display name is intended to be used in design environments where the control is listed by name and
        /// a user-friendly name is desired.
        /// </remarks>
        /// <param name="element">The FrameworkElement from which the property value is read.</param>
        /// <returns>The DisplayName property value for the FrameworkElement.</returns>
        public static string GetDisplayName(FrameworkElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return element.GetValue(DisplayNameProperty) as string;
        }

        /// <summary>
        /// Sets the display name of the specified FrameworkElement.
        /// </summary>
        /// <remarks>
        /// The display name is intended to be used in design environments where the control is listed by name and
        /// a user-friendly name is desired.
        /// </remarks>
        /// <param name="element">The FrameworkElement to which the attached property is written.</param>
        /// <param name="value">The display name</param>
        public static void SetDisplayName(FrameworkElement element, string value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(DisplayNameProperty, value);
        }

        /// <summary>
        /// Identifies the DisplayName attached dependency property.
        /// </summary>
        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.RegisterAttached(
                "DisplayName",
                typeof(string),
                typeof(FrameworkElement),
                null);

        #endregion

    }
}
