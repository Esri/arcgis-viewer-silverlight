/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// Represents a query that can be executed using a service that supports query tasks.
  /// </summary>
  /// <remarks>
  /// The persistent properties that make up the query are stored in a separate QueryDescription object.
  /// </remarks>
  public class Query
  {

    public Query(QueryDescription queryDesc)
    {
      QueryDescription = queryDesc;
    }

    /// <summary>
    /// The basic information about the query.
    /// </summary>
    public QueryDescription QueryDescription { get; set; }

    /// <summary>
    /// The expression used to constrain the query.
    /// </summary>
    public string WhereClause
    {
      get { return QueryDescription.WhereClause; }
      set
      {
        QueryDescription.WhereClause = value;
      }
    }

    /// <summary>
    /// Gets/sets the collection of fields that should be displayed.
    /// </summary>
    public List<string> VisibleFields
    {
      get { return QueryDescription.VisibleFields; }
      set
      {
        QueryDescription.VisibleFields = value;
      }
    }
		
  }
}
