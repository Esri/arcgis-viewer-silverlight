/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Globalization;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.GP.ParameterSupport
{
    public abstract class ParameterBase
    {
        public abstract void AddUI(Grid grid);

        public ParameterConfig Config { get; set; }

        public virtual ESRI.ArcGIS.Client.Tasks.GPParameter Value { get; set; }

        public virtual bool CanExecute
        {
            get
            {
                if (Config != null && Config.Required)
                {
                    if (Value == null)
                        return false;
                    switch (Config.Type)
                    {
                        case GPParameterType.Boolean:
                            return Value is GPBoolean;
                        case GPParameterType.Double:
                            return Value is GPDouble;
                        case GPParameterType.Long:
                            return Value is GPLong;
                        case GPParameterType.String:
                            return Value is GPString;
                        case GPParameterType.Date:
                            return Value is GPDate;
                        case GPParameterType.LinearUnit:
                            GPLinearUnit lu = Value as GPLinearUnit;
                            return (lu != null && !double.IsNaN(lu.Distance));
                        case GPParameterType.FeatureLayer:
                            return Value is GPFeatureRecordSetLayer;
                        case GPParameterType.RecordSet:
                            return Value is GPRecordSet;
                        case GPParameterType.DataFile:
                            return Value is GPDataFile;
                        case GPParameterType.RasterData:
                        case GPParameterType.RasterDataLayer:
                            return Value is GPRasterData;
                        case GPParameterType.MultiValueString:
                            return Value is GPMultiValue<GPString>;
                        default:
                            break;
                    }
                }
                return true;
            }
        }

        public event EventHandler CanExecuteChanged;

        internal void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, null);
        }
        public static string ParameterToDisplayString(GPParameterType type, GPParameter param)
        {
            if (param == null)
                return string.Empty;
            switch (type)
            {
                case GPParameterType.Date:
                    GPDate date = param as GPDate;
                    if (date != null && date.Value != null)
                        return date.Value.ToString();
                    return null;
                case GPParameterType.LinearUnit:
                    GPLinearUnit lu = param as GPLinearUnit;
                    if (lu != null)
                        return string.Format("{0} {1}", lu.Distance, lu.Unit.ToString().Replace("esri", ""));
                    return null;
                case GPParameterType.Boolean:
                case GPParameterType.Double:
                case GPParameterType.Long:
                case GPParameterType.String:
                case GPParameterType.FeatureLayer:
                case GPParameterType.RecordSet:
                case GPParameterType.DataFile:
                    return ParameterToString(type, param, CultureHelper.GetCurrentCulture());
                case GPParameterType.RasterData:
                case GPParameterType.RasterDataLayer:
                    GPRasterData ras = param as GPRasterData;
                    if (ras != null)
                        return ras.Url;
                    return null;
            }
            return string.Empty;
        }

        public static string ParameterToString(GPParameterType type, GPParameter param, CultureInfo culture)
        {
            if (param == null)
                return string.Empty;
            switch (type)
            {
                case GPParameterType.Boolean:
                    GPBoolean boo = param as GPBoolean;
                    if (boo != null)
                        return boo.Value.ToString();
                    else
                        return string.Empty;
                case GPParameterType.Date:
                    GPDate date = param as GPDate;
                    if (date != null && date.Value != null)
                        return date.Value.Ticks.ToString();
                    else
                        return string.Empty;
                case GPParameterType.Double:
                    GPDouble dub = param as GPDouble;
                    if (dub != null && !double.IsNaN(dub.Value))
                        return dub.Value.ToString(culture);
                    else
                        return string.Empty;
                case GPParameterType.Long:
                    GPLong lon = param as GPLong;
                    if (lon != null)
                        return lon.Value.ToString();
                    else
                        return string.Empty;
                case GPParameterType.String:
                    GPString stir = param as GPString;
                    if (stir != null && stir.Value != null)
                        return stir.Value;
                    else
                        return string.Empty;
                case GPParameterType.FeatureLayer:
                    GPFeatureRecordSetLayer feat = param as GPFeatureRecordSetLayer;
                    if (feat != null && feat.Url != null)
                        return feat.Url;
                    else
                        return string.Empty;
                case GPParameterType.RecordSet:
                    GPRecordSet rsl = param as GPRecordSet;
                    if (rsl != null && rsl.Url != null)
                        return rsl.Url;
                    else
                        return string.Empty;
                case GPParameterType.DataFile:
                    GPDataFile dat = param as GPDataFile;
                    if (dat != null && dat.Url != null)
                        return dat.Url;
                    else
                        return string.Empty;
            }
            return string.Empty;
        }

        public static GPParameter StringToParameter(string name, GPParameterType type, string newValue, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                return null;
            }
            switch (type)
            {
                case GPParameterType.Boolean:
                    bool boo;
                    if (bool.TryParse(newValue, out boo))
                        return new GPBoolean(name, boo);
                    else
                        return null;
                case GPParameterType.Date:
                    long lon;
                    if (long.TryParse(newValue, out lon))
                        return new GPDate(name,  new DateTime(lon));
                    else
                        return null;
                case GPParameterType.Double:
                    double d;
                    if (double.TryParse(newValue, System.Globalization.NumberStyles.Any, culture, out d) && !double.IsNaN(d))
                        return new GPDouble(name, d);
                    else
                        return null;
                case GPParameterType.Long:
                    int l;
                    if (int.TryParse(newValue, out l))
                        return new GPLong(name, l);
                    else
                        return null;
                case GPParameterType.String:
                    return new GPString(name, newValue);
                case GPParameterType.FeatureLayer:
                    return new GPFeatureRecordSetLayer(name, newValue);
                case GPParameterType.RecordSet:
                    return new GPRecordSet(name, newValue);
                case GPParameterType.DataFile:
                    return new GPDataFile(name, newValue);
            }
            return null;
        }

    }
}
