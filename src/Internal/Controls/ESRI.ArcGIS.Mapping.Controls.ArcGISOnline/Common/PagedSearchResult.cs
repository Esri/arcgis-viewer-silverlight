/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// A delegate that points to a method to wrap objects for data binding.
  /// </summary>
  /// <param name="objectToWrap">The object to be wrapped, e.g. ContentItem, Group.</param>
  /// <returns>The object that wraps the specified object.</returns>
  public delegate object WrapperFactoryDelegate(object objectToWrap);

  /// <summary>
  /// Implements an IPagedCollectionView for an AGOL SearchResult. 
  /// </summary>
  internal class PagedSearchResult : PagedCollection
  {
    SearchResult _result;
    WrapperFactoryDelegate _factoryDelegate;
    List<object> _wrappedObjects = new List<object>();

    public PagedSearchResult(SearchResult result)
      : base(result.TotalCount, result.Count)
    {
      _result = result;
    }

    /// <summary>
    /// Creates a PagedRearchResult.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="factoryDelegate">A pointer to a function that wraps the specified object in a wrapper used for binding.</param>
    public PagedSearchResult(SearchResult result, WrapperFactoryDelegate factoryDelegate)
      : base(result.TotalCount, result.Count)
    {
      _result = result;
      _factoryDelegate = factoryDelegate;

      WrapItems();
    }

    /// <summary>
    /// Wraps the items by calling the factory delegate.
    /// </summary>
    private void WrapItems()
    {
      _wrappedObjects.Clear();

      object[] resultItems = GetItems();

      if (resultItems != null && _factoryDelegate != null)
        foreach (object item in resultItems)
          _wrappedObjects.Add(_factoryDelegate(item));
    }

    /// <summary>
    /// Helper method to retrieve the list of items.
    /// </summary>
    private object[] GetItems()
    {
      object[] resultItems = null;
      if (_result is ContentSearchResult)
        resultItems = ((ContentSearchResult)_result).Items;
      else if (_result is GroupSearchResult)
        resultItems = ((GroupSearchResult)_result).Items;
      return resultItems;
    }

    /// <summary>
    /// Indexed property. Returns the item or wrapped item specified by the index.
    /// </summary>
    public object this[int index]
    {
      get
      {
        if (_factoryDelegate != null)
          return _wrappedObjects[index];

        object[] resultItems = GetItems();
        return (resultItems != null) ? resultItems[index] : null;
      }
    }

    /// <summary>
    /// Overrides GetEnumerator to return the items or wrapped items for the current page.
    /// </summary>
    /// <returns></returns>
    public override System.Collections.IEnumerator GetEnumerator()
    {
      // if a factory delegate was specified return an enumerator
      // on the list of wrapped items
      if (_factoryDelegate != null)
        return _wrappedObjects.GetEnumerator();

      object[] resultItems = GetItems();
      return (resultItems == null) ? new List<object>().GetEnumerator() : resultItems.GetEnumerator();
    }

    /// <summary>
    /// Issues the AGOL query for the specified page.
    /// </summary>
    protected override void OnBeginPageChange(int pageIndex)
    {
      if (_result is ContentSearchResult)
      {
        string sort = ((ContentSearchResult)_result).Sort;
        ArcGISOnlineEnvironment.ArcGISOnline.Content.Search(_result.Query, sort, pageIndex * _result.Count + 1, _result.Count, (object sender, ContentSearchEventArgs e) =>
          {
            Complete(e.Result);
          });
      }
      else if (_result is GroupSearchResult)
        ArcGISOnlineEnvironment.ArcGISOnline.Group.Search(_result.Query, pageIndex * _result.Count + 1, _result.Count, (object sender, GroupSearchEventArgs e) =>
          {
            Complete(e.Result);
          });
    }

    /// <summary>
    /// Called when a page change completes.
    /// </summary>
    private void Complete(SearchResult result)
    {
      _result = result;
      WrapItems();
      CompletePageChange();
    }
  }
}
