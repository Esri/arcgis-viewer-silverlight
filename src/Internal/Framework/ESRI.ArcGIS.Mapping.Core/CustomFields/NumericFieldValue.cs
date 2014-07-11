/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class NumericFieldValue : IComparable, IConvertible
    {
        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            if (obj is NumericFieldValue)
                return Value.CompareTo((obj as NumericFieldValue).Value);
            else
                throw new Exception(string.Format(Resources.Strings.ExceptionCannotCompareWith, obj.GetType().Name));
        }

        public double Value { get; set;}
        public string FormattedValue { get; set; }

        public override string ToString()
        {
 	         //return FormattedValue ?? (Value == null ? null : Value.ToString());
            return !double.IsNaN(Value) ? Value.ToString() : FormattedValue;
        }
        
        public override int GetHashCode()
        {
            string ser = ToString();
            if (ser != null)
                return ser.GetHashCode();
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            NumericFieldValue other = obj as NumericFieldValue;
            if (other != null)
            {
                return other.Value == Value && other.FormattedValue == FormattedValue;
            }
            return base.Equals(obj);
        }

        #region IConvertible
        public TypeCode GetTypeCode()
        {
            return TypeCode.Empty;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return default(bool);
        }

        public byte ToByte(IFormatProvider provider)
        {
            return default(byte);
        }

        public char ToChar(IFormatProvider provider)
        {
            return default(char);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return default(DateTime);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return default(decimal);
        }

        public double ToDouble(IFormatProvider provider)
        {
            return default(double);
        }

        public short ToInt16(IFormatProvider provider)
        {
            return default(short);
        }

        public int ToInt32(IFormatProvider provider)
        {
            return default(int);
        }

        public long ToInt64(IFormatProvider provider)
        {
            return default(long);
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return default(sbyte);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return default(float);
        }

        public string ToString(IFormatProvider provider)
        {
            return default(string);
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return default(object);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return default(ushort);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return default(uint);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return default(ulong);
        }
        #endregion
    }
}
