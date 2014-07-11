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
using ESRI.ArcGIS.Client;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Core
{
    public static class ApplicationMapTipExtensions
    {
        #region ApplicationMapTipContainerXaml
        /// <summary>
        /// Gets the value of the ApplicationMapTipContainerXaml attached property for a specified Map.
        /// </summary>
        /// <param name="element">The Map from which the property value is read.</param>
        /// <returns>The ApplicationMapTipContainerXaml property value for the Map.</returns>
        public static string GetApplicationMapTipContainerXaml(Map element)
        {
            return string.Empty;
        }

        /// <summary>
        /// Sets the value of the ApplicationMapTipContainerXaml attached property to a specified Map.
        /// </summary>
        /// <param name="element">The Map to which the attached property is written.</param>
        /// <param name="value">The needed ApplicationMapTipContainerXaml value.</param>
        public static void SetApplicationMapTipContainerXaml(Map element, string value)
        {
        }

        /// <summary>
        /// Identifies the ApplicationMapTipContainerXaml dependency property.
        /// </summary>
        public static readonly DependencyProperty ApplicationMapTipContainerXamlProperty =
            DependencyProperty.RegisterAttached(
                "ApplicationMapTipContainerXaml",
                typeof(string),
                typeof(Map),
                new PropertyMetadata(null));

        #endregion
    }

}
