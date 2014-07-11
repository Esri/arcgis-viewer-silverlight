/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class GetConnectionsCompletedEventArgs : EventArgs
    {
        public List<Connection> Connections { get; set; }
        public object UserState { get; set; }
    }
}
