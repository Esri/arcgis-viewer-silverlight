/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// A base class that implemnts the necessary interfaces such that derived classes can be used as the DataContext
  /// for a DataPager control.
  /// <remarks>
  /// Derived classes implement OnPageChanged to acquire the items for the current page and GetEnumerator to return the items.
  /// </remarks>
  internal abstract class PagedCollection : IPagedCollectionView, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
  {
    int _itemCount;   // total number of items
    int _pageSize;    // number of items per page
    int _curPage = 0; // current page

    bool _pageChanging = false;

    /// <summary>
    /// Instantiates the PagedCollection.
    /// </summary>
    protected PagedCollection(int itemCount, int pageSize)
    {
      _itemCount = itemCount;
      _pageSize = pageSize;
    }

    /// <summary>
    /// Gets the number of pages.
    /// </summary>
    int PageCount
    {
      get
      {
        return (ItemCount + (PageSize - ItemCount % PageSize)) / PageSize;
      }
    }

    #region IPagedCollectionView Members

    public int PageSize
    {
      get { return _pageSize; }

      set { }
    }

    public int TotalItemCount
    {
      get { return _itemCount; }
    }

    public bool CanChangePage
    {
      get
      {
        return _itemCount > _pageSize;
      }
    }

    public bool IsPageChanging
    {
      get
      {
        return _pageChanging;
      }
    }

    public int ItemCount
    {
      get
      {
        return _itemCount;
      }
    }

    public bool MoveToFirstPage()
    {
      return MoveToPage(0);
    }

    public bool MoveToLastPage()
    {
      return MoveToPage(PageCount - 1);
    }

    public int PageIndex
    {
      get 
      {
        return _curPage;
      }
    }

    public bool MoveToNextPage()
    {
      return MoveToPage(_curPage + 1);
    }

    public bool MoveToPreviousPage()
    {
      return MoveToPage(PageIndex - 1);
    }

    /// <summary>
    /// Moves to the specified page index. This method is used to implement all page navigation methods.
    /// </summary>
    public bool MoveToPage(int pageIndex)
    {
      if (pageIndex < 0 || pageIndex >= PageCount)
        return false;

      if (pageIndex == _curPage)
        return false;

      _pageChanging = true;
      NotifyPropertyChanged("IsPageChanging");

      _curPage = pageIndex;
      OnBeginPageChange(pageIndex);  // call the derived class to fill the data for the new current page
      if (PageChanging != null)
        PageChanging(this, new PageChangingEventArgs(pageIndex));
      
      _pageChanging = false;

      NotifyPropertyChanged("IsPageChanging");
      NotifyPropertyChanged("PageIndex");
      return true;
    }

    /// <summary>
    /// Overridden by derived classes to receive notification that the current page has changed
    /// and the data for the new page should be acquired.
    /// </summary>
    protected abstract void OnBeginPageChange(int pageIndex);

    /// <summary>
    /// Called by the derived class when the data for the current page has been retrieved
    /// and the page change process is complete.
    /// </summary>
    protected void CompletePageChange()
    {
      if (CollectionChanged != null)
        CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

      NotifyPropertyChanged("Count");
      NotifyPropertyChanged("CurrentItem");
      if (PageChanged != null)
        PageChanged(this, EventArgs.Empty);
    }

    public event EventHandler<EventArgs> PageChanged;

    public event EventHandler<PageChangingEventArgs> PageChanging;

    #endregion

    #region IEnumerable Members

    /// <summary>
    /// Overridden by derived classes to return the contents of the current page.
    /// </summary>
    public abstract IEnumerator GetEnumerator();

    void NotifyPropertyChanged(string propName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propName));
    }

    #endregion

    #region INotifyCollectionChanged Members

    public event System.Collections.Specialized.NotifyCollectionChangedEventHandler CollectionChanged;

    #endregion


    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }

}
