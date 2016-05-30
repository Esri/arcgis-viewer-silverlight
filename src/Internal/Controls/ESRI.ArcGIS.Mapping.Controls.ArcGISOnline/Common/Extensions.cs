/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Globalization;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// Extension methods.
  /// </summary>
  public static class Extensions
  {

    /// <summary>
    /// Tries to parse a string to a formatted date string using the current culture.
    /// </summary>
    /// <param name="millisecondsString">The string that represents a UTC date in milliseconds since UNIX epoch.</param>
    /// <param name="formattedDateString">A user readable date string based on the current culture.</param>
    /// <returns>False if parsing fails, otherwise true.</returns>
    public static bool TryParseToDateString(this string millisecondsString, out string formattedDateString)
    {
      double dateMilliSecUTC = 0;
      formattedDateString = null;

      if (!double.TryParse(millisecondsString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out dateMilliSecUTC))
        return false;

      //unix epoch, UTC (also known as GMT)
      DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

      //add the specified milliseconds
      date = date.AddMilliseconds(dateMilliSecUTC);

      //format the date based on the current culture
      formattedDateString = date.ToString(System.Globalization.CultureInfo.CurrentCulture);

      return true;
    }
  }
}
