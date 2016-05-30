/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/


namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
    public static class LocalizableStrings
    {
        private static StringResourcesManager StringResources = new StringResourcesManager();

        public static string GetString(string key)
        {
            return StringResources.Get(key);
        }

    }
}
