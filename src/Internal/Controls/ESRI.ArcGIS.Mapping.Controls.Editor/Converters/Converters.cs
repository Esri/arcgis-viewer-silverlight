/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Collections;
using System.Linq;

namespace ESRI.ArcGIS.Mapping.Controls.Editor
{
    public class VisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;
            bool visibility = (bool)value;
            return visibility ? Visibility.Visible : Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            return (visibility == Visibility.Visible);
        }

        #endregion
    }

    public class IsNullOrEmptyCollectionToBoolConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ICollection collection = value as ICollection;
            if (collection == null || collection.Count == 0)
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Not supported");
        }

        #endregion
    }

    public class ReverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            bool boolValue = (bool)value;
            return !boolValue;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;
            return !boolValue;
        }

        #endregion
    }

    public class AddIntegerConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int val;
            int valToAdd;
            if (!int.TryParse(value.ToString(), out val) || !int.TryParse(parameter.ToString(), out valToAdd))
                return null;

            return val + valToAdd;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int val;
            int valToSubtract;
            if (!int.TryParse(value.ToString(), out val) || !int.TryParse(parameter.ToString(), out valToSubtract))
                return null;

            return val - valToSubtract;
        }

        #endregion

    }

    public class InvertVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility vis = (Visibility)value;
            return vis == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ReverseEnumerationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable && typeof(IEnumerable) == targetType)
                return (value as IEnumerable).OfType<object>().Reverse();

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
}
