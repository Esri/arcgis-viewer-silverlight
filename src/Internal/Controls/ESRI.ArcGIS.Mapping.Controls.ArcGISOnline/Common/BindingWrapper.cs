/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// Implements a generic wrapper for an object that will be used in a data binding and include
  /// additional properties - stored in the tag.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class BindingWrapper<T> : INotifyPropertyChanged
  {
    T _content;
    object _tag;

    public T Content
    {
      get { return _content; }
      set
      {
        _content = value;
        NotifyPropertyChanged("Content");
      }
    }

    public object Tag
    {
      get { return _tag; }
      set
      {
        _tag = value;
        NotifyPropertyChanged("Tag");
      }
    }

    protected void NotifyPropertyChanged(string propName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propName));
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
