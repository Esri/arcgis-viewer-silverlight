/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
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

    public class AsyncEventArgs : EventArgs
    {
        public bool Succeeded { get; set; }
    }
  /// <summary>
  /// User state event args
  /// </summary>
  public class UserStateEventArgs : EventArgs
  {
    public object UserState { get; set; }
  }

  /// <summary>
  /// Layer event args
  /// </summary>
  public class LayerEventArgs : UserStateEventArgs
  {
    public ESRI.ArcGIS.Client.Layer Layer { get; set; }
  }

  public class MapDocumentEventArgs : EventArgs
  {
    public string DocumentID { get; set; }
    public ESRI.ArcGIS.Client.WebMap.Document Document { get; set; }
  }

  public class QueryMapItemEventArgs : EventArgs
  {
    public Query Query { get; set; }
  }

}
