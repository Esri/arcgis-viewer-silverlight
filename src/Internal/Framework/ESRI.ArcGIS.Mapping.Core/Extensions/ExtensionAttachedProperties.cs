/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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
using System.Windows.Controls.Primitives;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Core
{
    public static class ExtensionAttachedProperties
    {
        #region ConfigData
        /// <summary>
        /// Gets the value of the ConfigData attached property for a specified ButtonBase.
        /// </summary>
        /// <param name="element">The ButtonBase from which the property value is read.</param>
        /// <returns>The ConfigData property value for the ButtonBase.</returns>
        public static string GetConfigData(ButtonBase element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return element.GetValue(ConfigDataProperty) as string;
        }

        /// <summary>
        /// Sets the value of the ConfigData attached property to a specified ButtonBase.
        /// </summary>
        /// <param name="element">The ButtonBase to which the attached property is written.</param>
        /// <param name="value">The needed ConfigData value.</param>
        public static void SetConfigData(ButtonBase element, string value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(ConfigDataProperty, value);
        }

        /// <summary>
        /// Identifies the ConfigData dependency property.
        /// </summary>
        public static readonly DependencyProperty ConfigDataProperty =
            DependencyProperty.RegisterAttached(
                "ConfigData",
                typeof(string),
                typeof(ButtonBase),
                new PropertyMetadata(null, OnConfigDataPropertyChanged));

        /// <summary>
        /// ConfigDataProperty property changed handler.
        /// </summary>
        /// <param name="d">ExtensionAttachedProperties that changed its ConfigData.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnConfigDataPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonBase source = d as ButtonBase;
            if (source == null)
                return;
            ISupportsConfiguration supportConfig = source.Command as ISupportsConfiguration;
            if (supportConfig != null)
            {
                string value = e.NewValue as string;
                try
                {
                    supportConfig.LoadConfiguration(value);
                }
                catch { }
            }
            
        }
        #endregion
    }
}
