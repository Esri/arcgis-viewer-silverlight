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
using System.Windows.Data;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class MessageBoxIconConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value == null)
                return null;

            MessageType type = (MessageType)value;
            switch (type)
            {
                case MessageType.Error:
                    return "/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/warning_icon.png";
                case MessageType.Warning:
                    return "/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/caution16.png";
                case MessageType.Information:
                    return "/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/GenericInformation16.png";
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class MessageBoxIconVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            MessageType type = (MessageType)value;
            switch (type)
            {
                case MessageType.None:
                    return Visibility.Collapsed;
                case MessageType.Error:
                case MessageType.Warning:
                case MessageType.Information:
                default:
                    return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class MessageBoxButtonsVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null || value == null)
                return Visibility.Collapsed;

            MessageBoxButton type = (MessageBoxButton)value;
            string buttonId = parameter as string;
            switch (type)
            {
                case MessageBoxButton.OK:
                    {
                        if (string.Compare(buttonId, "OK", StringComparison.InvariantCultureIgnoreCase) == 0)
                            return Visibility.Visible;
                        else if (string.Compare(buttonId, "Cancel", StringComparison.InvariantCultureIgnoreCase) == 0)
                            return Visibility.Collapsed;
                    } break;
                case MessageBoxButton.OKCancel:
                    {
                        if (string.Compare(buttonId, "OK", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                            string.Compare(buttonId, "Cancel", StringComparison.InvariantCultureIgnoreCase) == 0)
                            return Visibility.Visible;
                    } break;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
