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
    /// Specifies the description of an add-in
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DescriptionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DescriptionAttribute"/> class
        /// </summary>
        public DescriptionAttribute() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DescriptionAttribute"/> class
        /// </summary>
        /// <param name="description">String containing the description</param>
        public DescriptionAttribute(string description)
        {
			string localizeString = ESRI.ArcGIS.Client.Extensibility.Resources.Strings.ResourceManager.GetString(description, System.Threading.Thread.CurrentThread.CurrentUICulture);
			Description = string.IsNullOrEmpty(localizeString) ? description : localizeString;
        }

        /// <summary>
        /// Gets or sets the description of the add-in
        /// </summary>
        public string Description { get; set; }
    }
}
