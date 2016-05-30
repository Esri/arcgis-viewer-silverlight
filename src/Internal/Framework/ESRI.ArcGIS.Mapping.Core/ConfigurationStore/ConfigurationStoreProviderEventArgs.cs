/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
