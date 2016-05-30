/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using SearchTool.Resources;

namespace SearchTool
{
    /// <summary>
    /// Reverses the order of a list
    /// </summary>
    public class ReverseListConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList)
            {
                IList list = (IList)value;
                List<object> copiedList = new List<object>();
                foreach (object o in list)
                    copiedList.Add(o);

                list.Clear();
                for (int i = copiedList.Count - 1; i > -1; i--)
                    list.Add(copiedList[i]);

                return list;
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(Strings.CannotConvertBack);
        }

        #endregion
    }

}
