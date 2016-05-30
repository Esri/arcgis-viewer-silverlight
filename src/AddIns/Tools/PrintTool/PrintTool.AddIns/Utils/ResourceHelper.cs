/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Resources;
using System.Threading;
using System.Collections.Generic;

namespace PrintTool.AddIns
{
    /// <summary>
    /// Provides access to resources within the assembly
    /// </summary>
    public class ResourceHelper
    {
		private static readonly Resources.Strings _strings = new Resources.Strings();
        /// <summary>
        /// Gets the localized string resources contained in the assembly
        /// </summary>
        public Resources.Strings LocalizedStrings
        {
            get
            {
                return _strings;
            }
        }

        private static ResourceManager resourceManager =
            new ResourceManager("PrintTool.AddIns.Resources.Strings", typeof(ResourceHelper).Assembly);
        /// <summary>
        /// Gets the string resource with the specified name, if it exists
        /// </summary>
        public static string GetStringResource(string name)
        {
            return !string.IsNullOrEmpty(name) ? resourceManager.GetString(name, Thread.CurrentThread.CurrentUICulture) :
                string.Empty;
        }
    }
}
