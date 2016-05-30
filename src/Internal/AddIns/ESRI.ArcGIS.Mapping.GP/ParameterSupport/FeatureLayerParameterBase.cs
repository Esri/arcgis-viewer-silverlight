/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows.Controls;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client;
using System.Windows;
using System;

namespace ESRI.ArcGIS.Mapping.GP.ParameterSupport
{
    public abstract class FeatureLayerParameterBase : ParameterBase
    {
        public Map Map { get; set; }

        internal FeatureLayerParameterConfig config
        {
            get { return Config as FeatureLayerParameterConfig; }
        }

        public string InputLayerID { get; internal set; }

    }
}
