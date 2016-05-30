/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Globalization;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Core
{
    public static class CultureHelper
    {
        public static CultureInfo GetCurrentCulture()
        {
            // Default to the culture associated with the currently executing thread
            CultureInfo culture = System.Threading.Thread.CurrentThread.CurrentCulture;

            // If a view instance is available and the culture is set, then this should take precedence
            // since the code running this method may be on a thread that does not have the proper culture
            // (due to a Silverlight bug) such as a background worker thread, etc. If this bug should ever
            // be fixed, we can remove this code and fix the problem everywhere since all code will pass
            // through this logic when it needs the current culture.
            if (MapApplication.Current != null && MapApplication.Current.Culture != null)
                culture = MapApplication.Current.Culture;

            return culture;
        }
    }
}
