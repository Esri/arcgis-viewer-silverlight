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
using System.ComponentModel.Composition;

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Specifies the category of an add-in
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CategoryAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryAttribute"/> class
        /// </summary>
        public CategoryAttribute() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryAttribute"/> class
        /// </summary>
        /// <param name="category">The name of the category</param>
        public CategoryAttribute(string category)
        {
			string localizeString = ESRI.ArcGIS.Client.Extensibility.Resources.Strings.ResourceManager.GetString(category, System.Threading.Thread.CurrentThread.CurrentUICulture);
			Category = string.IsNullOrEmpty(localizeString) ? category : localizeString;
        }

        /// <summary>
        /// Gets or sets the name of the category
        /// </summary>
        public string Category { get; set; }
    }
}
