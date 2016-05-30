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

namespace SearchTool
{
    public class Utils
    {
        /// <summary>
        /// Converts the passed in length into the specified units
        /// </summary>
        /// <param name="len">The length to convert, in meters</param>
        /// <param name="outUnits">The units of the input length</param>
        /// <param name="outUnits">The units to convert the length to</param>
        public static double ConvertLength(double len, LengthUnits inUnits, LengthUnits outUnits)
        {
            // convert from input units to meters
            double lengthInMeters = 0;
            switch (inUnits)
            {
                case LengthUnits.UnitsCentimeters:
                    lengthInMeters = len / 100;
                    break;
                case LengthUnits.UnitsMeters:
                    lengthInMeters = len;
                    break;
                case LengthUnits.UnitsKilometers:
                    lengthInMeters = len / 0.001;
                    break;
                case LengthUnits.UnitsInches:
                    lengthInMeters = len / 39.3700787;
                    break;
                case LengthUnits.UnitsFeet:
                    lengthInMeters = len / 3.2808399;
                    break;
                case LengthUnits.UnitsYards:
                    lengthInMeters = len / 1.0936133;
                    break;
                case LengthUnits.UnitsMiles:
                    lengthInMeters = len / 0.0006213700922;
                    break;
                case LengthUnits.UnitsNauticalMiles:
                    lengthInMeters = len / 0.000539956803;
                    break;
            }
            
            // convert from meters to output units
            switch (outUnits)
            {
                case LengthUnits.UnitsCentimeters:
                    return lengthInMeters * 100;
                case LengthUnits.UnitsMeters:
                    return lengthInMeters;
                case LengthUnits.UnitsKilometers :
                    return lengthInMeters * 0.001;
                case LengthUnits.UnitsInches :
                    return lengthInMeters * 39.3700787;
                case LengthUnits.UnitsFeet :
                    return lengthInMeters * 3.2808399;
                case LengthUnits.UnitsYards :
                    return lengthInMeters * 1.0936133;
                case LengthUnits.UnitsMiles :
                    return lengthInMeters * 0.0006213700922;
                case LengthUnits.UnitsNauticalMiles :
                    return lengthInMeters * 0.000539956803;
            }

            return len;
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
    }
}
