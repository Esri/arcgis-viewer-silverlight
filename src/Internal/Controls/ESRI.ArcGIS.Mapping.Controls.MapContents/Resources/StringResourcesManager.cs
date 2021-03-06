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
using ESRI.ArcGIS.Client.Application.Layout.Converters;

namespace ESRI.ArcGIS.Mapping.Controls.MapContents
{
    public abstract class StringResourcesManagerBase : IValueConverter
    {
        public abstract Assembly Assembly { get; }
        public abstract string ResourceFileName { get; }

        /// <summary>
        /// The Resource Manager loads the resources out of the executing assembly (and the XAP File where there are embedded)
        /// </summary>
        private ResourceManager resourceManager = null;

        /// <summary>
        /// This method returns the localized string of the given resource.
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
        /// This method is used to bind the silverlight component to the resource.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty((string)parameter))
                return null;
            StringResourcesManagerBase reader = value as StringResourcesManagerBase;
            if (reader == null)
                return null;
            return reader.Get((string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ConvertBack is not used, because the Localization is only a One Way binding
            throw new NotImplementedException();
        }

        #endregion
    }

    public class StringResourcesManager : StringResourcesManagerBase
    {
        public override System.Reflection.Assembly Assembly
        {
            get { return typeof(StringResourcesManager).Assembly; }
        }

        public override string ResourceFileName
        {
            get { return "ESRI.ArcGIS.Mapping.Controls.MapContents.Resources.Strings"; }
        }
    }
}
