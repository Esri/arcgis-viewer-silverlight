/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using ESRI.ArcGIS.Client.Geometry;
using System.Windows.Browser;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// Represents the fundamental information for a query based on a query task.
  /// </summary>
  /// <remarks>
  /// The QueryDescription corresponds to the JSON object that is stored in the AGOL Web Map
  /// for queryTasks.
  /// </remarks>
  [DataContract]
  public class QueryDescription
  {
    public QueryDescription()
    {
      Parameters = new List<QueryParameter>();
    }

    [DataMember(Name = "url")]
    public string Url { get; set; }

    [DataMember(Name = "name")]
    public string Name { get; set; }

    /// <summary>
    /// The where clause.
    /// </summary>
    [DataMember(Name = "whereClause")]
    public string WhereClause { get; set; }

    [DataMember(Name = "visibleFields", EmitDefaultValue = false)]
    public List<string> VisibleFields { get; set; }

    [DataMember(Name = "parameters")]
    public List<QueryParameter> Parameters { get; set; }

    [DataMember(Name = "displayField", EmitDefaultValue = false)]
    public string DisplayField { get; set; }

    /// <summary>
    /// Specifies whether or not the url requires a proxy due to a missing
    /// client access policy file. If the value is null, it has not yet
    /// been determined.
    /// </summary>
    public bool? RequiresProxy { get; set; }

    /// <summary>
    /// Helper method to do a deep clone of the QueryDescription.
    /// </summary>
    internal QueryDescription Clone()
    {
      QueryDescription qd = this.MemberwiseClone() as QueryDescription;
      
      List<QueryParameter> parameters = new List<QueryParameter>();
      foreach (QueryParameter param in qd.Parameters)
        parameters.Add(param.Clone());
      qd.Parameters = parameters;

      qd.VisibleFields = new List<string>(qd.VisibleFields);

      return qd;
    }
  }

  /// <summary>
  /// Represents a single parameter within a query. A parameter is a means by which
  /// the end user can provide input to a query at run time. The properties of the parameter
  /// serve to describe the input that is required to generate a sub expression such
  /// as 'ACRES > 10000'.
  /// </summary>
  [DataContract]
  public class QueryParameter : INotifyPropertyChanged
  {
    string _prompt;
    string _field;
    string _operator;
    string _helpTip;
    string _defaultValue;
    
    bool _hasCodedValueDomain;
    CodedValue[] _codedValues;
    CodedValue _selectedCodedValue;

    string _fieldType;
    string _dataSource;

    [DataMember(Name = "prompt")]
    public string Prompt
    {
      get { return _prompt; }
      set
      {
        _prompt = value;
        NotifyPropertyChanged("Prompt");
      }
    }

    [DataMember(Name = "field")]
    public string Field
    {
      get { return _field; }
      set
      {
        _field = value;
        NotifyPropertyChanged("Field");
      }
    }

    /// <summary>
    /// Gets or sets the type of the field.
    /// </summary>
    public string FieldType
    {
      get { return _fieldType; }
      set
      {
        _fieldType = value;
        NotifyPropertyChanged("FieldType");
      }
    }

    /// <summary>
    /// Gets or sets the underlying data source the query is executed against.
    /// </summary>
    public string DataSource
    {
      get { return _dataSource; }
      set
      {
        _dataSource = value;
        NotifyPropertyChanged("DataSource");
      }
    }

    /// <summary>
    /// Gets or sets a boolean that specifies wheather the field is tied to a coded value domain.
    /// </summary>
    public bool HasCodedValueDomain
    {
      get { return _hasCodedValueDomain; }
      set
      {
        _hasCodedValueDomain = value;
        NotifyPropertyChanged("HasCodedValueDomain");
      }
    }

    /// <summary>
    /// Gets or sets the coded values if the field is tied to a coded value domain.
    /// </summary>
    public CodedValue[] CodedValues 
    {
      get { return _codedValues; }
      set
      {
        _codedValues = value;
        NotifyPropertyChanged("CodedValues");

        //set the SelectedCodedValue
        //
        bool selected = false;
        foreach(CodedValue val in _codedValues)
          if (val.Name == DefaultValue)
          {
            SelectedCodedValue = val;
            selected = true;
            break;
          }
        if(!selected && _codedValues != null)
          SelectedCodedValue = _codedValues[0]; //select first item
      }
    }

    /// <summary>
    /// Gets or sets the selected coded value if the field is tied to a coded value domain.
    /// </summary>
    public CodedValue SelectedCodedValue
    {
      get { return _selectedCodedValue; }
      set
      {
        if (value == null)
          return;

        _selectedCodedValue = value;
        NotifyPropertyChanged("SelectedCodedValue");

        //set the DefaultValue
        //
        DefaultValue = _selectedCodedValue.Name;
      }
    }

    /// <summary>
    /// The operator.
    /// </summary>
    [DataMember(Name = "operator")]
    public string Operator
    {
      get { return _operator; }
      set
      {
        _operator = value;
        NotifyPropertyChanged("Operator");
      }
    }

    /// <summary>
    /// Helper method to clone a parameter.
    /// </summary>
    internal QueryParameter Clone()
    {
      return MemberwiseClone() as QueryParameter;
    }

    [DataMember(Name = "defaultValue")]
    public string DefaultValue
    {
      get { return _defaultValue; }
      set
      {
        _defaultValue = value;
        NotifyPropertyChanged("DefaultValue");
      }
    }

    [DataMember(Name = "helpTip")]
    public string HelpTip
    {
      get { return _helpTip; }
      set
      {
        _helpTip = value;
        NotifyPropertyChanged("HelpTip");
      }
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    protected void NotifyPropertyChanged(string propName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propName));
    }
    #endregion
  }

  /// <summary>
  /// Represents a collection of queries.
  /// </summary>
  public class QueryTaskCollection : ObservableCollection<QueryDescription>
  {
  }

  /// <summary>
  /// Represents the set of tasks that can be stored in a map. For now, only query tasks
  /// are defined.
  /// </summary>
  [DataContract]
  public class TaskCollection
  {
    [DataMember(Name = "queryTasks")]
    public QueryTaskCollection QueryTasks { get; set; }
  }
}
