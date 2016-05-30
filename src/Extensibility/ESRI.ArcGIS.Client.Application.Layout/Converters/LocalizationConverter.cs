/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Data;

namespace ESRI.ArcGIS.Client.Application.Layout.Converters
{
    /// <summary>
    /// Converts a resource key to the corresponding resource string for the current culture.  Can be inherited
    /// from to implement localization in third party code.
    /// </summary>
    public class LocalizationConverter : IValueConverter
    {
        /// <summary>
        /// Gets the assembly that resource strings are retrieved from
        /// </summary>
        public virtual System.Reflection.Assembly Assembly
        {
            get { return typeof(LocalizationConverter).Assembly; }
        }

        /// <summary>
        /// Gets the namespace-qualified name of the resource file that resource strings are retrieved from
        /// </summary>
        public virtual string ResourceFileName
        {
            get { return "ESRI.ArcGIS.Client.Application.Layout.Resources.Strings"; }
        }

        /// <summary>
        /// The Resource Manager loads the resources out of the executing assembly (and the XAP File where there are embedded)
        /// </summary>
        private ResourceManager resourceManager = null;

        /// <summary>
        /// Returns the localized string of the given resource.
        /// </summary>
        public string Get(string resource)
        {
            if (resourceManager == null)
            {
                if (string.IsNullOrEmpty(ResourceFileName))
                    throw new InvalidOperationException("Must specify ResourceFileName");
                if (Assembly == null)
                    throw new InvalidOperationException("Must specify Assembly");
                resourceManager = new ResourceManager(ResourceFileName, Assembly);
            }
            return resourceManager.GetString(resource, Thread.CurrentThread.CurrentUICulture);
        }

        #region IValueConverter Members

        /// <summary>
        /// Perform the conversion to a resource string
        /// </summary>
        /// <param name="value">The key of the resource string</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic</param>
        /// <param name="culture">The culture of the conversion</param>
        /// <returns>
        /// The resource string to be passed to the target dependency property
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty((string)parameter))
                return null;
            string resourceString = this.Get((string)parameter);

            string val = value != null ? value.ToString() : null;

            if (string.IsNullOrEmpty(resourceString))
                return null;

            // If the resource string contains one format parameter, substitute the bound value 
            if (resourceString.Contains("{0}") 
            && !resourceString.Contains("{1}") 
            && !string.IsNullOrEmpty(val)
            && val != value.GetType().FullName)
                return string.Format(resourceString, val);
            else
                return resourceString;
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ConvertBack is not used, because the Localization is only a One Way binding
            throw new NotImplementedException();
        }

        #endregion
    }

}
