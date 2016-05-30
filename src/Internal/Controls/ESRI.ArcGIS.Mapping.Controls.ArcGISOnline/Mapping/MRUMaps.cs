/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Runtime.Serialization.Json;
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
  /// Represents a collection of id's of most recently used maps.
  /// </summary>
  public class MRUMaps
  {
    static ObservableCollection<ContentItem> _contentItems = new ObservableCollection<ContentItem>();
    static bool _initialized = false;
    const int _maxAmount = 10;
    const string _mru = "MRU";

    internal static void Clear()
    {
        _initialized = false;
    }
    /// <summary>
    /// Reads persited mru id's from isolated storage.
    /// </summary>
    static void Initialize()
    {
      if (_initialized)
        return;

      _initialized = true;

      if (!IsolatedStorageFile.IsEnabled)
        return;

      ArcGISOnlineEnvironment.ArcGISOnline.Content.ItemDeleted += new EventHandler<ContentItemEventArgs>(ArcGISOnline_ItemDeleted);

      //read mru map id's from isolated storage settings
      IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
      for (int i = 0; i < _maxAmount; ++i)
      {
        if (settings.Contains(_mru + i))
        {
          //create a empty ContentItem and initialize with id only
          //once the ContentItems are requested from a client we download them based on the id
          ContentItem item = new ContentItem();
          item.Id = (string)settings[_mru + i];
          _contentItems.Add(item);
        }
        else
          break;
      }
    }

    /// <summary>
    /// Raised when a map has been deleted.
    /// </summary>
    static void ArcGISOnline_ItemDeleted(object sender, ContentItemEventArgs e)
    {
      //remove maps that have been deleted
      ContentItem item = Find(e.Id);
      if (item != null)
        _contentItems.Remove(item);
      
      Save();
    }

    /// <summary>
    /// Returns a collection of mru maps.
    /// </summary>
    public static ObservableCollection<ContentItem> Items
    {
      get 
      {
        if (ArcGISOnlineEnvironment.ArcGISOnline != null)
        {
          //only initialize the very first time mru maps are requested
          Initialize();
          
          //download ContentItem download
          foreach (ContentItem item in _contentItems)
            ArcGISOnlineEnvironment.ArcGISOnline.Content.GetItem(item.Id, GetItemCompleted);
        }

        return _contentItems;
      }
    }

    /// <summary>
    /// Gets called when the async download of a ContentItem has completed.
    /// </summary>
    static void GetItemCompleted(object sender, ContentItemEventArgs e)
    {
      if (e.Error != null)
      {
        //an error occured - remove item from list
        //e.Id is populated when an error occurs
        ContentItem item = Find(e.Id);
        if (item != null)
          _contentItems.Remove(item);
        return;
      }

      //find the place holder item that corresponds to the downloaded item
      ContentItem placeHolderItem = Find(e.Item.Id);
      if (placeHolderItem == null)
        return;

      int indexToReplace = _contentItems.IndexOf(placeHolderItem);
      _contentItems.RemoveAt(indexToReplace);
      _contentItems.Insert(indexToReplace, e.Item);
    }

    /// <summary>
    /// Adds a map to the list of mru maps.
    /// </summary>
    public static void Add(ContentItem item)
    {
      Initialize();

      ContentItem existingItem = Find(item.Id);
      if (existingItem != null)
      {
        //move item to top
        _contentItems.Remove(existingItem);
      }

      if (_contentItems.Count == _maxAmount) //don't list more than a certain amount of maps
        _contentItems.RemoveAt(_maxAmount - 1); //remove oldest

      _contentItems.Insert(0, item);

      Save();
    }

    /// <summary>
    /// Searches for a ContentItem by ID.
    /// </summary>
    private static ContentItem Find(string id)
    {
      foreach (ContentItem item in _contentItems)
      {
        if (item.Id == id)
          return item;
      }
      return null;
    }

    /// <summary>
    /// Persists mru maps to disc in isolated storage settings.
    /// </summary>
    static void Save()
    {
      if (!IsolatedStorageFile.IsEnabled)
        return;
      
      IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

      //clear existing settings
      for (int i = 0; i < _maxAmount; ++i)
      {
        if (settings.Contains(_mru + i))
          settings.Remove(_mru + i);
      }

      //save each id as individual setting
      int index = 0;
      string key = _mru + index;
      foreach (ContentItem item in _contentItems)
      {
        if (!settings.Contains(key))
            settings.Add(_mru + index, item.Id);
        else
            settings[key] = item.Id;
        ++index;
      }
    }
  }
}
