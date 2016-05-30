/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client.Application.Layout.Converters;

namespace ESRI.ArcGIS.Mapping.Core
{
	public static class LocalizableStrings
    {
        public static string NoneInAngleBraces
        { get { return Resources.Strings.NoneInAngleBraces; } }
	}

	public class MappingCoreStringResourcesManager : LocalizationConverter
	{
		public override System.Reflection.Assembly Assembly
		{
			get { return typeof(MappingCoreStringResourcesManager).Assembly; }
		}

		public override string ResourceFileName
		{
			get { return "ESRI.ArcGIS.Mapping.Core.Resources.Strings"; }
		}
	}
}
