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
    public class BingMapsTokenLoadedEventArgs : EventArgs
    {
        public string Token { get; set; }
        public object UserState { get; set; }
    }    

    public class GetConfigurationStoreCompletedEventArgs : EventArgs
    {
        public ConfigurationStore ConfigurationStore { get; set; }
        public object UserState { get; set; }
    }    
}
