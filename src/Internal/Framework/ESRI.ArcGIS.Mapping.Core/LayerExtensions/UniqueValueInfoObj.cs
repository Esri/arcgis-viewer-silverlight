/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Globalization;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class UniqueValueInfoObj : UniqueValueInfo
    {        
        public object SerializedValue
        {
            get 
            { 
                return base.Value; 
            }
            set 
            {
                string val = value as string;
                if (val != null)
                {
                    if (FieldType == FieldType.DecimalNumber)
                    {
                        double d;
                        if (double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                        {
                            base.Value = d;
                            return;
                        }
                    }
                    else if (FieldType == FieldType.Integer)
                    {
                        int i;
                        if (int.TryParse(val, out i))
                        {
                            base.Value = i;
                            return;
                        }
                    }
                    else if (FieldType == FieldType.DateTime)
                    {
                        DateTime dt;
                        if (DateTime.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                        {
                            base.Value = dt;
                            return;
                        }
                    }
                }
                base.Value = value;
            }
        }

        public FieldType FieldType
        {
            get;
            set;
        }
    }

    //public class UniqueValueInfoObjTypeConverter : TypeConverter
    //{
    //    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    //    {
    //        return sourceType == typeof(string);
    //    }

    //    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    //    {
    //        return destinationType == typeof(double) || destinationType == typeof(DateTime);
    //    }

    //    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    //    {
    //        string val = value as string;
    //        if(val == null)
    //            return val;                        
    //        //decimal dec;
    //        //if (decimal.TryParse(val, out dec))
    //        //    return Convert.ToDouble(dec);
    //        double d;
    //        if (double.TryParse(val, out d))
    //        {                
    //            return d;
    //        }
    //        int i;
    //        if (int.TryParse(val, out i))
    //            return i;
    //        DateTime dt;
    //        if (DateTime.TryParse(val, out dt))
    //            return dt;            
    //        return value;
    //    }

    //    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    //    {
    //        return value;
    //    }
    //}
}
