/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client.Application.Layout.Converters;

namespace Esri.ArcGIS.Client.Application.AddIns
{
    /// <summary>
    /// Provides access to resource strings by name in XAML or code
    /// </summary>
    public class StringResourcesManager : LocalizationConverter
    {
        public override System.Reflection.Assembly Assembly
        {
            get { return typeof(StringResourcesManager).Assembly; }
        }

        public override string ResourceFileName
        {
            get { return "Esri.ArcGIS.Client.Application.AddIns.Resources.Strings"; }
        }

        private static StringResourcesManager _resourceManager = new StringResourcesManager();
        public static string GetResource(string resource)
        {
            return _resourceManager.Get(resource);
        }
    }
}
