/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using ESRI.ArcGIS.Client.Tasks;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// Helper class that formats dates for SQL queries depending on the format required by the underlying data source.
  /// </summary>
  public class QueryDateHelper
  {
    static List<DateFormat> _dateFieldFormats = new List<DateFormat>(); //a collection of date formats in prioritised order

    static QueryDateHelper()
    {
      //add the formats required by different kinds of data sources
      //these are added by priority, higher priority comes first - this way a client is able to request
      //date formats in prioritised order
      //
      _dateFieldFormats.Add(new DateFormat("Oracle/File gdb", "yyyy-MM-dd", "date '{0}'"));
      _dateFieldFormats.Add(new DateFormat("SQL Server", "yyyy-MM-dd", "'{0}'"));
      _dateFieldFormats.Add(new DateFormat("Informix", "yyyy-MM-dd hh:mm:ss", "'{0}'"));
      _dateFieldFormats.Add(new DateFormat("IBM DB2", "yyyy-MM-dd hh:mm:ss", "TO_DATE('{0}','YYYY-MM-DD HH24:MI:SS')"));
      _dateFieldFormats.Add(new DateFormat("PostgreSQL", "YYYY-MM-DD HH24:MI:SS", "TIMESTAMP '{0}'"));
      _dateFieldFormats.Add(new DateFormat("Personal gdb", "MM-dd-yyyy", "#{0}#"));
    }

    /// <summary>
    /// Gets the data source types that are supported for formatting dates.
    /// <remarks>
    /// The order of data sources returned is prioritised with the highest priority first.
    /// </remarks>
    /// </summary>
    public static string[] SupportedDataSources 
    {
      get
      {
        List<string> dataSources = new List<string>();
        foreach (DateFormat dF in _dateFieldFormats)
          dataSources.Add(dF.DataSource);

        return dataSources.ToArray();
      }
    }

    /// <summary>
    /// Formats the specified DateTime into a date string that is required for queries against the specified type of data source. 
    /// </summary>
    /// <param name="dateTime">A DateTime object that needs to be formatted to a date string for a SQL query.</param>
    /// <param name="dataSource">The type of data source against which the SQL query is going to be executed.</param>
    /// <returns>A date string in the format required by the specified data source.</returns>
    public static string Format(DateTime dateTime, string dataSource)
    {
      DateFormat dF = FindDateFormat(dataSource);
      if (dF == null)
        return null;

      return dF.GetQueryFormat(dateTime);
    }

    /// <summary>
    /// Determines the data source type asynchronously from a date field by running a series of test queries against it.
    /// </summary>
    /// <remarks>
    /// If a test query succeeds the data source type is reported back to the client via the callback. It is possible that
    /// multiple queries succeed so the client is called back multiple times.
    /// </remarks>
    /// <param name="dataSetUrl">The data set that contains the dateField.</param>
    /// <param name="dateField">A date field used to determine the data source type.</param>
    /// <param name="callback">A event handler that is called back when a test query succeeds. Multiple callbacks are possible.</param>
    public static void GetDataSource(string dataSetUrl, bool useProxy, SubLayerField dateField, EventHandler<DataSourceEventArgs> callback)
    {
      foreach (string dataSource in SupportedDataSources)
        QueryDate(dataSetUrl, useProxy, dateField, dataSource, callback);
    }

    /// <summary>
    /// Runs a test query against the specified date field to determine the type of data source.
    /// </summary>
    private static void QueryDate(string dataSetUrl, bool useProxy, SubLayerField dateField, string dataSource, EventHandler<DataSourceEventArgs> callback)
    {
      DateTime testDate = new DateTime(1977, 12, 01, 0, 0, 0, 0);

      QueryTask qT = new QueryTask(dataSetUrl);
      if (useProxy)
        qT.ProxyURL = ArcGISOnlineEnvironment.ConfigurationUrls.ProxyServerEncoded;

      ESRI.ArcGIS.Client.Tasks.Query q = new ESRI.ArcGIS.Client.Tasks.Query();
      q.OutFields.Add(dateField.Name);
      q.Where = dateField.Name + " = " + Format(testDate, dataSource);
      q.ReturnGeometry = false;

      EventHandler<ESRI.ArcGIS.Client.Tasks.QueryEventArgs> completedHandler = null;
			completedHandler = (object sender, ESRI.ArcGIS.Client.Tasks.QueryEventArgs e) =>
      {
        callback(null, new DataSourceEventArgs() { DataSource = dataSource });
      };

      qT.ExecuteCompleted += completedHandler;

      //execute the query
      //
      qT.ExecuteAsync(q);
    }

    static DateFormat FindDateFormat(string dataSource)
    {
      foreach (DateFormat dF in _dateFieldFormats)
        if (dF.DataSource == dataSource)
          return dF;

      return null;
    }

    /// <summary>
    /// A helper class that holds date formatting information for a specific data source.
    /// </summary>
    class DateFormat
    {
      string _dataSource, _dateFormatting, _queryFormatting;

      /// <summary>
      /// Creates a DateFormat helper class.
      /// </summary>
      /// <param name="dataSource">The type of the data source.</param>
      /// <param name="dateFormatting">A format pattern for the date, e.g. "yyyy-MM-dd".</param>
      /// <param name="queryFormatting">A format pattern that is required by dates for a SQL query, e.g. "date '{0}'".</param>
      public DateFormat(string dataSource, string dateFormatting, string queryFormatting)
      {
        _dataSource = dataSource;
        _dateFormatting = dateFormatting;
        _queryFormatting = queryFormatting;
      }

      /// <summary>
      /// Gets the data source to which the date format applies.
      /// </summary>
      public string DataSource { get { return _dataSource; } }

      /// <summary>
      /// Formats the specified DateTime to a date string according to a data sourc e specific date format pattern and a
      /// query format pattern.
      /// </summary>
      /// <param name="dateTime">The DateTime to be formatted.</param>
      /// <returns>A formatted date string.</returns>
      public string GetQueryFormat(DateTime dateTime)
      {
        return string.Format(_queryFormatting, dateTime.ToString(_dateFormatting));        
      }
    }
  }

  public class DataSourceEventArgs : EventArgs
  {
    public string DataSource { get; set; }
  }
}
