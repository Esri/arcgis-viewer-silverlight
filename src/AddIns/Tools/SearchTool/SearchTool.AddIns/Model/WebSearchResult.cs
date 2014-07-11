/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Runtime.Serialization;

namespace SearchTool
{
  /// <summary>
  /// Represents a response from a Google search (ajax.googleapis.com)
  /// </summary>
  [DataContract]
  public class GoogleResponse
  {
    [DataMember(Name = "responseData")]
    public WebSearchResult ResponseData;
  }

  /// <summary>
  /// Represents the results of a Google search.
  /// </summary>
  [DataContract]
  public class WebSearchResult
  {
    [DataMember(Name = "results")]
    public WebSearchResultItem[] Items { get; set; }
  }

  /// <summary>
  /// Represents a result from a Google search.
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
