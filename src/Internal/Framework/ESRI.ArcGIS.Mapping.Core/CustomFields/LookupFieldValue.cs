/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
    public class LookupFieldValue : IComparable, IConvertible
    {
        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            if (obj is LookupFieldValue)
                return ToString().CompareTo((obj as LookupFieldValue).ToString());
            else
                throw new Exception(string.Format(Resources.Strings.ExceptionCannotCompareWith, obj.GetType().Name));
        }

        public string DisplayText { get; set; }
        public string LinkUrl { get; set; }

        public override string  ToString()
        {
 	         return DisplayText ?? LinkUrl;
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
            LookupFieldValue other = obj as LookupFieldValue;
            if (other != null)
                return DisplayText == other.DisplayText && LinkUrl == other.LinkUrl;
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
