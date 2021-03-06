/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;

namespace ESRI.ArcGIS.Mapping.Controls.MapContents
{
    public static class MapContentsControlHelper
    {
        public static bool IsTopMostLayerType(string layerType)
        {
            if (string.Compare(layerType, "MapLayer Layer", StringComparison.InvariantCultureIgnoreCase) == 0)
                return true;
            return false;
        }
    }
}
