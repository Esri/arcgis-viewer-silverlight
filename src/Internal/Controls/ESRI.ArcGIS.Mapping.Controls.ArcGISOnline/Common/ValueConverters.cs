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

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// Converts a string to Visibility - empty or null string becomes Visibility.Collapsed.
  /// </summary>
  public class EmptyTextVisibilityConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value is string)
        return string.IsNullOrEmpty((string)value) ? Visibility.Collapsed : Visibility.Visible;

      return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }

  /// <summary>
  /// Converts a string of milliseconds into a culture-specific date and time based on unix epoch.
  /// </summary>
  public class DateTimeFormatConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value == null)
        return "";

      double milliSecs = 0;
      if (!double.TryParse(value.ToString(), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out milliSecs))
        return "";

      //unix epoch, UTC (also known as GMT)
      System.DateTime date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

      //add the specified milliseconds
      date = date.AddMilliseconds(milliSecs);

      //time stamp is UTC (also known as GMT) -> convert to local time zone
      date = TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Local);

      //return a culture-specific formatted date

      // Get the format string for the current UI culture
      string dateFormatString = CultureInfo.CurrentUICulture.DateTimeFormat.FullDateTimePattern;

      // Remove day and seconds from format string
      dateFormatString = Util.RemoveDayFormat(dateFormatString);
      dateFormatString = dateFormatString.Replace(":ss", "");

      // Replace two digit day with single digit
      dateFormatString = dateFormatString.Replace("dd", "d");

       // Trim leading and trailing whitespace
      dateFormatString = dateFormatString.Trim();

        return date.ToString(dateFormatString, CultureInfo.CurrentUICulture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }

  /// <summary>
  /// Converts a string of milliseconds into a culture-specific date based on unix epoch.
  /// </summary>
  public class DateFormatConverter : IValueConverter
  {
      #region IValueConverter Members

      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
          if (value == null)
              return "";

          double milliSecs = 0;
          if (!double.TryParse(value.ToString(), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out milliSecs))
              return "";

          //unix epoch, UTC (also known as GMT)
          System.DateTime date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

          //add the specified milliseconds
          date = date.AddMilliseconds(milliSecs);

          //time stamp is UTC (also known as GMT) -> convert to local time zone
          date = TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Local);

          //return a culture-specific formatted date

          // Get the format string for the current UI culture
          string dateFormatString = CultureInfo.CurrentUICulture.DateTimeFormat.LongDatePattern;

          // Remove days from format string
          dateFormatString = Util.RemoveDayFormat(dateFormatString);

          // Replace two digit day with single digit
          dateFormatString = dateFormatString.Replace("dd", "d");

          // Trim leading and trailing whitespace
          dateFormatString = dateFormatString.Trim();

          return date.ToString(dateFormatString, CultureInfo.CurrentUICulture);
      }

      public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
          throw new NotImplementedException();
      }

      #endregion
  }

  /// <summary>
  /// Tests if the value is null or not and returns one string or the other. The string is passed 
  /// as the parameter and is of the form "Choice 1:Choice 2".
  /// </summary>
  public class IsNullToStringConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (parameter == null || !(parameter is string))
        return "";

      string[] strings = ((string)parameter).Split(new char[] { ':' });
      if (strings.Length < 2)
        return "";

      return value == null ? strings[0] : strings[1];
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }

  /// <summary>
  /// Inverts a boolean value.
  /// </summary>
  public class InvertBooleanConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      bool boolean = (bool)value;
      return !boolean;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }

  /// <summary>
  /// Inverts a Visibility value.
  /// </summary>
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

  /// <summary>
  /// Converts bool or nullable bool to Visibility.  Returns Visibility.Visible if value is true, Collapsed if 
  /// value is null or false.
  /// </summary>
  public class BoolToVisibilityConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {

      return value == null || !(bool)value ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }

  /// <summary>
  /// Converts bool or nullable bool to inverse Visibility.  Returns Visibility.Collapsed if value is true or null, 
  /// Visible if value is false.
  /// </summary>
  public class InvertBoolToVisibilityConverter : IValueConverter
  {
      #region IValueConverter Members

      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {

          return value == null || (bool)value ? Visibility.Collapsed : Visibility.Visible;
      }

      public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
          throw new NotImplementedException();
      }

      #endregion
  }

  /// <summary>
  /// Converts bool or nullable bool to inverse Visibility.  Returns Visibility.Collapsed if value is true, 
  /// Visible if value is false or null.
  /// </summary>
  public class InvertNullableBoolToVisibilityConverter : IValueConverter
  {
      #region IValueConverter Members

      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {

          return value == null || !(bool)value ? Visibility.Visible : Visibility.Collapsed;
      }

      public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
          throw new NotImplementedException();
      }

      #endregion
  }

    /// <summary>
  /// Removes the string specified by parameter from the value.
  /// </summary>
  public class RemoveStringConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      string valStr = value as string;
      string paramStr = parameter as string;

      return valStr.Replace(paramStr, "");
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }

}
