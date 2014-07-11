/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// Represents a table that can be bound to a DataGrid and whose rows and columns are only known at runtime.
  /// <remarks>
  /// Once the table is created no columns can be added or removed. However rows can be added/removed at any time.
  /// </remarks>
  /// </summary>
  public class Table
  {
    ObservableCollection<Row> _rows;
    ReadOnlyCollection<DataGridTextColumn> _columns;

    /// <summary>
    /// Creates a table using the specified collection of strings as names for the column headers.
    /// </summary>
    /// <param name="fields">The names of the column headers.</param>
    public Table(string[] fields)
    {
      int index = 0;
      List<DataGridTextColumn> columns = new List<DataGridTextColumn>();
      foreach (string field in fields)
      {
        DataGridTextColumn textColumn = new DataGridTextColumn();
        textColumn.Header = field; //set the header name according to the corresponding field name
        textColumn.Binding = new Binding("Cells[" + index.ToString() + "]"); //setup a binding to a TextFileRow.Cells property

        columns.Add(textColumn);

        index++;
      }
      _columns = new ReadOnlyCollection<DataGridTextColumn>(columns);
    }

    /// <summary>
    /// Gets the columns of the table.
    /// </summary>
    public ReadOnlyCollection<DataGridTextColumn> Columns
    {
      get { return _columns; }
    }

    /// <summary>
    /// Gets the rows of the table.
    /// <remarks>
    /// The cells of a row have to match the columns of the table.
    /// A row can be added/removed at any time.
    /// </remarks>
    /// </summary>
    public ObservableCollection<Row> Rows
    {
      get 
      {
        if (_rows == null)
          _rows = new ObservableCollection<Row>();

        return _rows;
      }
    }
  }

  /// <summary>
  /// Represents a row of a Table object.
  /// </summary>
  public class Row
  {
    object[] _cells;

    /// <summary>
    /// Creates a row.
    /// </summary>
    /// <param name="cells">A collection of objects, each representing a cell of the row.</param>
    public Row(object[] cells)
    {
      _cells = cells;
    }

    /// <summary>
    /// Gets the collection of cells.
    /// </summary>
    public ReadOnlyCollection<object> Cells
    {
      get { return new ReadOnlyCollection<object>(_cells); }
    }
  }
}
