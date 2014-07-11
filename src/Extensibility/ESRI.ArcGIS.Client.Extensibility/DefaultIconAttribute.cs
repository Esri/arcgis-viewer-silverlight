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
using System.ComponentModel.Composition;

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Specifes the default icon to use for an add-in
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DefaultIconAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultIconAttribute"/> class
        /// </summary>
        public DefaultIconAttribute() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultIconAttribute"/> class
        /// </summary>
        /// <param name="defaultIcon">The URL to the icon</param>
        public DefaultIconAttribute(string defaultIcon)
        {
            DefaultIcon = defaultIcon;
        }

        /// <summary>
        /// Gets or sets the URL to the icon
        /// </summary>
        public string DefaultIcon { get; set; }
    }
}
