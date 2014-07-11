/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Runtime.Serialization;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client.Geometry;
using System.Collections.Generic;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Text;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{

  /// <summary>
  /// Represents the service description for a SubLayer.
  /// </summary>
  [DataContract]
  public class SubLayerDescription
  {
    [DataMember(Name = "extent")]
    public Envelope Extent { get; set; }

    [DataMember(Name = "displayField")]
    public string DisplayField { get; set; }

    [DataMember(Name = "fields")]
    public SubLayerField[] Fields { get; set; }

    [DataMember(Name = "type")]
    public string Type { get; set; }

    [DataMember(Name = "name")]
    public string Name { get; set; }

    public string Url { get; set; }

    /// <summary>
    /// Specifies whether or not the  url requires a proxy due to a missing
    /// client access policy file.
    /// </summary>
    public bool RequiresProxy { get; set; }

    /// <summary>
    /// Retrieves a SubLayerDescription asynchronously for the specified url.
    /// </summary>
    public static void GetServiceInfoAsync(string url, EventHandler<SubLayerEventArgs> callback)
    {
      WebUtil.OpenReadAsync(new Uri(url + "?f=json"), null, (sender, e) =>
      {
        SubLayerDescription description = WebUtil.ReadObject<SubLayerDescription>(e.Result);
        description.RequiresProxy = e.UsedProxy;

        // remove the geometry field
        //
        if (description.Fields != null)
        {
          List<SubLayerField> fields = new List<SubLayerField>();
          foreach (SubLayerField field in description.Fields)
            if (field.Type != "esriFieldTypeGeometry" && field.Type != "Microsoft.SqlServer.Types.SqlGeometry")//exclude shape field from AGS and SDS v1.x
              fields.Add(field);

          description.Fields = fields.ToArray();
        }

        description.Url = url;

        callback(null, new SubLayerEventArgs() { Description = description });
      });
    }
  }

  /// <summary>
  /// A Field within a SubLayer service description.
  /// </summary>
  [DataContract]
  public class SubLayerField
  {
    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "type")]
    public string Type { get; set; }

    [DataMember(Name = "alias")]
    public string Alias { get; set; }

    [DataMember(Name = "domain")]
    public AttributeDomain AttributeDomain { get; set; }
  }

  /// <summary>
  /// A attribute domain that can be assigned to a sublayer field.
  /// </summary>
  [DataContract]
  public class AttributeDomain
  {
    [DataMember(Name = "type")]
    public string Type { get; set; }
    [DataMember(Name = "codedValues")]
    public CodedValue[] CodedValues { get; set; }

    /// <summary>
    /// Returns the coded value alias for the specified code.
    /// </summary>
    public string this[object code] 
    {
      get 
      { 
        if(CodedValues == null)
          return null;

        foreach (CodedValue val in CodedValues)
          if (val.Code.ToString() == code.ToString())
            return val.Name;

        return null;
      }
    }
  }

  /// <summary>
  /// A value of a coded value domain.
  /// </summary>
  [DataContract]
  public class CodedValue
  {
    [DataMember(Name = "name")]
    public string Name { get; set; }
    [DataMember(Name = "code")]
    public object Code { get; set; }
  }

  /// <summary>
  /// Provides a response for a request for SubLayerDescription.
  /// </summary>
  public class SubLayerEventArgs : EventArgs
  {
    public SubLayerDescription Description { get; set; }
  }

  /// <summary>
  /// Provides a response for a request for multiple SubLayerDescriptions.
  /// </summary>
  public class SubLayersEventArgs : EventArgs
  {
    public SubLayerDescription[] Descriptions { get; set; }
}
}
