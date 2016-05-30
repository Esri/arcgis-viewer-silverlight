/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// Represents a response from a google search (ajax.googleapis.com)
  /// </summary>
  [DataContract]
  public class GoogleResponse
  {
    [DataMember(Name = "responseData")]
    public WebSearchResult ResponseData;
  }

  /// <summary>
  /// Represents the results of a google search.
  /// </summary>
  [DataContract]
  public class WebSearchResult
  {
    [DataMember(Name = "results")]
    public WebSearchResultItem[] Items { get; set; }
  }

  /// <summary>
  /// Represents a result from a google search.
  /// </summary>
  [DataContract]
  public class WebSearchResultItem
  {
    [DataMember(Name = "title")]
    public string Title { get; set; }

    [DataMember(Name = "url")]
    public string Url { get; set; }
  }
}
