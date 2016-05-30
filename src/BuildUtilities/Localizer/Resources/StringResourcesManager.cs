/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Data;

namespace Localizer
{
    public class StringResourcesManager : IValueConverter
    {
        static StringResourcesManager instance;
        public static StringResourcesManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new StringResourcesManager();
                return instance;
            }
        }
        public Assembly Assembly
        {
            get
            {
                return this.GetType().Assembly;
            }
        }

        string resourceFileName;
        public string ResourceFileName
        {
            get
            {
                if (string.IsNullOrEmpty(resourceFileName))
                    resourceFileName = this.GetType().Namespace + ".Resources.Strings";
                return resourceFileName;
            }
        }

        ResourceManager resourceManager;
        /// <summary>
        /// The Resource Manager loads the resources out of the executing assembly (and the XAP File where there are embedded)
        /// </summary>
        public ResourceManager ResourceManager
        {
            get
            {
                if (resourceManager == null)
                {
                    if (string.IsNullOrEmpty(ResourceFileName))
                        throw new InvalidOperationException("Must specify ResourceFileName");
                    if (Assembly == null)
                        throw new InvalidOperationException("Must specify Assembly");
                    resourceManager = new ResourceManager(ResourceFileName, Assembly);
                }
                return resourceManager;
            }
        }

        /// <summary>
        /// This method returns the localized string of the given resource.
        /// </summary>
        public string Get(string resource)
        {
            return ResourceManager.GetString(resource, Thread.CurrentThread.CurrentUICulture);
        }

        #region IValueConverter Members

        /// <summary>
        /// This method is used to bind the silverlight component to the resource.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty((string)parameter))
                return null;
            return Get((string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ConvertBack is not used, because the Localization is only a One Way binding
            throw new NotImplementedException();
        }

        #endregion
    }
}
