/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;

namespace MeasureTool.Addins
{
    public class Utils
    {
        /// <summary>
        /// Converts the passed in length into the specified units
        /// </summary>
        /// <param name="len">The length to convert, in meters</param>
        /// <param name="units">The units to convert the length to</param>
        public static double? ConvertLength(double? len, LengthUnits units)
        {
            if (len == null)
                return null;

            switch (units)
            {
                case LengthUnits.UnitsCentimeters:
                    return len * 100;
                case LengthUnits.UnitsMeters:
                    return len;
                case LengthUnits.UnitsKilometers :
                    return len * 0.001;
                case LengthUnits.UnitsInches :
                    return len * 39.3700787;
                case LengthUnits.UnitsFeet :
                    return len * 3.2808399;
                case LengthUnits.UnitsYards :
                    return len * 1.0936133;
                case LengthUnits.UnitsMiles :
                    return len * 0.0006213700922;
                case LengthUnits.UnitsNauticalMiles :
                    return len * 0.000539956803;
            }

            return len;
        }

        /// <summary>
        /// Converts the passed in area into the specified units
        /// </summary>
        /// <param name="len">The area to convert, in square meters</param>
        /// <param name="units">The units to convert the area to</param>
        public static double? ConvertArea(double? area, AreaUnits units)
        {
            if (area == null)
                return null;

            switch (units)
            {
                case AreaUnits.UnitsSquareMiles :
                    return area * 0.0000003861003;
                case AreaUnits.UnitsSquareMeters :
                    return area;
                case AreaUnits.UnitsSquareKilometers :
                    return area * 0.000001;
                case AreaUnits.UnitsSquareFeet :
                    return area * 10.763911;
                case AreaUnits.UnitsAcres :
                    return area * 0.000247105381;
                case AreaUnits.UnitsHectares:
                    return area * 0.0001;
            }

            return area;
        }

        // Based on blog by Brandon Truong - http://brandontruong.blogspot.com/2010/04/use-enum-as-itemssource.html
        internal static Dictionary<T, string> GetEnumDescriptions<T>()
        {  
            var x = typeof(T).GetFields().Where(info => info.FieldType.Equals(typeof(T)));
            IEnumerable<KeyValuePair<T, string>> enumsAndDescriptions = from field in x
                   select new KeyValuePair<T, string>((T)Enum.Parse(typeof(T), field.Name, false), GetEnumDescription(field));
            Dictionary<T, string> dictionary = new Dictionary<T, string>();
            foreach (KeyValuePair<T, string> pair in enumsAndDescriptions)
                dictionary.Add(pair.Key, pair.Value);

            return dictionary;
        }

        // Based on blog by Brandon Truong - http://brandontruong.blogspot.com/2010/04/use-enum-as-itemssource.html
        internal static string GetEnumDescription(FieldInfo field)
        {
            System.ComponentModel.DescriptionAttribute[] attributes =
                (System.ComponentModel.DescriptionAttribute[])field.GetCustomAttributes(
                typeof(System.ComponentModel.DescriptionAttribute), false);  
            if (attributes.Length > 0)  
            {  
                return attributes[0].Description;  
            }  
            else  
            {  
                return field.Name;  
            }  
        }

        /// <summary>
        /// Attempts to return a type based on a string containing the type's assembly,namespace, and type.  
        /// String must be in the format &lt;Assembly&gt;~&lt;Namespace&gt;~&lt;Type&gt;
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns></returns>
        internal static Type GetTypeFromTypeInfoString(string typeInfo)
        {
            Type t = null;
            string[] splitTypeInfo = typeInfo.Split('~');
            if (splitTypeInfo.Length > 2)
            {
                string xaml = "<a:{0} xmlns:a='clr-namespace:{1};assembly={2}' />";
                xaml = string.Format(xaml, splitTypeInfo[2], splitTypeInfo[1], splitTypeInfo[0]);
                object o = XamlReader.Load(xaml);
                t = o.GetType();
            }
            return t;
        }
    }
}
