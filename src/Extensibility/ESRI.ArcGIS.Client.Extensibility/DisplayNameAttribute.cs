/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel.Composition;

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Specifies the name to use when displaying the add-in textually
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DisplayNameAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayNameAttribute"/> class
        /// </summary>
        public DisplayNameAttribute() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayNameAttribute"/> class
        /// </summary>
        /// <param name="name">The display name</param>
        public DisplayNameAttribute(string name)
        {
			string localizeString = ESRI.ArcGIS.Client.Extensibility.Resources.Strings.ResourceManager.GetString(name, System.Threading.Thread.CurrentThread.CurrentUICulture);
			Name = string.IsNullOrEmpty(localizeString) ? name : localizeString;
        }

        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        public string Name { get; set; }
    }
}
