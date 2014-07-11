/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.ObjectModel;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit;
using System.ComponentModel;

namespace Bookmarks.AddIns
{
    /// <summary>
    /// Represents a ViewModel for interacting with bookmarks in a mapping application
    /// </summary>
    public class BookmarksViewModel : INotifyPropertyChanged
    {
        /// <summary>
        ///  Instantiates the view, including initialization of the Map and Bookmarks properties
        /// </summary>
        /// <param name="map">The map control that the bookmarks are meant to navigate</param>
        /// <param name="bookmarks">The collection of bookmarks</param>
        public BookmarksViewModel(Map map, ObservableCollection<Bookmark.MapBookmark> bookmarks)
        {
            Bookmarks = bookmarks;
            Map = map;
            DeleteBookmark = new DeleteBookmarkCommand(bookmarks);
        }

        private ObservableCollection<Bookmark.MapBookmark> _bookmarks;
        /// <summary>
        /// The collection of bookmarks
        /// </summary>
        public ObservableCollection<Bookmark.MapBookmark> Bookmarks 
        {
            get { return _bookmarks; }
            internal set
            {
                if (_bookmarks != value)
                {
                    _bookmarks = value;
                    OnPropertyChanged("Bookmarks");
                }
            }
        }

        /// <summary>
        /// The map control that the bookmarks are meant to navigate 
        /// </summary>
        public Map Map { get; private set; }

        /// <summary>
        /// Command that allows deletion of bookmarks from the collection reference by the Bookmarks property
        /// </summary>
        public DeleteBookmarkCommand DeleteBookmark { get; private set; }

        /// <summary>
        /// Raised when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
