/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows.Data;
using ESRI.ArcGIS.Mapping.Controls;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class WatermarkConverter : WatermarkConverterBase
    {
        public WatermarkConverter()
        {
            ResourceManager = new StringResourcesManager();
        }
    }
}
