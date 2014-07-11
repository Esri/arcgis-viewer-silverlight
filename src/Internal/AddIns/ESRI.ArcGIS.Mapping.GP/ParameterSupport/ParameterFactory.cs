/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;

namespace ESRI.ArcGIS.Mapping.GP.ParameterSupport
{
    public class ParameterFactory
    {
        public static ParameterBase Create(ParameterConfig config, Map map)
        {
            switch (config.Type)
            {
                case GPParameterType.Boolean:
                    return new BooleanParameter() { Config = config, Value = config.DefaultValue as GPParameter };
                case GPParameterType.Double:
                case GPParameterType.Long:
                case GPParameterType.String:
                case GPParameterType.RecordSet:
                case GPParameterType.DataFile:
                    return new SimpleParameter() { Config = config, Value = config.DefaultValue as GPParameter };
                case GPParameterType.RasterData:
                case GPParameterType.RasterDataLayer:
                    return new RasterParameter() { Config = config, Value = config.DefaultValue as GPParameter };
                case GPParameterType.Date:
                    return new DateParameter() { Config = config, Value = config.DefaultValue as GPParameter };
                case GPParameterType.LinearUnit:
                    return new LinearUnitParameter() { Config = config, Value = config.DefaultValue as GPParameter };
                case GPParameterType.MultiValueString:
                    var m = new MultiValueStringParameter();
                    return m.SetConfiguration(config as MultiValueStringConfig) ? m : null;
                case GPParameterType.FeatureLayer:
                    FeatureLayerParameterConfig flConfig = config as FeatureLayerParameterConfig;
                    switch (flConfig.Mode)
                    {
                        case FeatureLayerParameterConfig.InputMode.SketchLayer:
                            return new SketchLayerParameter() { Config = config, Value = config.DefaultValue as GPParameter, Map = map };
                        case FeatureLayerParameterConfig.InputMode.SelectExistingLayer:
                            return new SelectExistingLayerParameter() { Config = config, Value = config.DefaultValue as GPParameter, Map = map };
                        case FeatureLayerParameterConfig.InputMode.Url:
                            return new SimpleParameter() { Config = config, Value = config.DefaultValue as GPParameter };
                        case FeatureLayerParameterConfig.InputMode.CurrentExtent:
                            return new CurrentExtentLayerParameter() { Config = config, Value = config.DefaultValue as GPParameter, Map = map };
                    }
                    break;
            }
            return null;
        }
    }
}
