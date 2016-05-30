/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class GetConfigurationCompletedEventArgs : EventArgs
    {
        public Map Map { get; set; }
        public object UserState { get; set; }
    }
}
