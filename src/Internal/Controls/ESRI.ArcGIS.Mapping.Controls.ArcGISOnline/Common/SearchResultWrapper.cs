/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// Represents a wrapper around a specific search result such as ContentItem, WebSearchResult or ArcGISService.
  /// </summary>
  public class SearchResultWrapper : INotifyPropertyChanged
  {
    Layer _layer;
    object _result;

    public SearchResultWrapper(object result, ICommand addCommand)
    {
      _result = result;
      AddCommand = addCommand;
    }

    /// <summary>
    /// Gets a specific search result such as ContentItem, WebSearchResult or ArcGISService.
    /// </summary>
    public object Result
    {
      get { return _result; }
    }

    /// <summary>
    /// Gets or sets the layer associated with this search result.
    /// </summary>
    internal Layer Layer 
    {
      get { return _layer; }
      set
      {
        if (_layer == value)
          return;

        _layer = value;
        NotifyPropertyChanged("Layer");
      }
    }

    /// <summary>
    /// Gets the Url of a search result.
    /// </summary>
    public string Url
    {
      get
      {
        if (Result is ContentItem)
          return ((ContentItem)Result).Item;
        else if (Result is WebSearchResultItem)
          return ((WebSearchResultItem)Result).Url;
        else if (Result is ArcGISService)
          return ((ArcGISService)Result).Url;

        return null;
      }
    }

    /// <summary>
    /// Gets the title of a search result.
    /// </summary>
    public string Title
    {
      get
      {
        if (Result is ContentItem)
          return ((ContentItem)Result).Title;
        else if (Result is WebSearchResultItem)
          return ((WebSearchResultItem)Result).Title;
        else if (Result is ArcGISService)
          return ((ArcGISService)Result).Title;

        return null;
      }
    }

    /// <summary>
    /// Returns the thumbnail for a map service.
    /// </summary>
    public ImageSource Thumbnail
    {
      get 
      {
        if(Result is ContentItem)
          return ((ContentItem)Result).Thumbnail;

        if (Result is FeatureService)
          return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls.ArcGISOnline;component/Images/featureService.png", UriKind.Relative));

        if (Result is ImageService)
          return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls.ArcGISOnline;component/Images/imageService.png", UriKind.Relative));

        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls.ArcGISOnline;component/Images/mapService.png", UriKind.Relative)); 
      }
    }

    public ICommand AddCommand { get; set; }

    void NotifyPropertyChanged(string propName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propName));
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
