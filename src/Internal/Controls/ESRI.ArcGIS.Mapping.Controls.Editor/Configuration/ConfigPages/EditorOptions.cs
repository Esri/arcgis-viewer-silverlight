/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Mapping.Controls;

namespace ESRI.ArcGIS.Mapping.Controls.Editor
{
    public class EditorOptions : Control
    {
        public EditorOptions()
        {
            this.DefaultStyleKey = typeof(EditorOptions);
        }
    }
}
