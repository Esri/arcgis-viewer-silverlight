/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ESRI.ArcGIS.Mapping.Controls.MapContents
{
    public enum Mode
    {
        Contents = 0, //layer visibility, legend swatches
        LayerList = 1, //layes visibility, no legend swatches
        Legend = 3, //no layer visibility, legend swatches
        TopLevelLayersOnly = 4, //top most layers only, layes visibility, no legend swatches
    }
}
