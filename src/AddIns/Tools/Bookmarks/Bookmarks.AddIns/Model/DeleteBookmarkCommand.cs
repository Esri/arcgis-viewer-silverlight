/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Toolkit;
using Bookmarks.AddIns.Resources;

namespace Bookmarks.AddIns
{
    /// <summary>
    /// Removes the bookmark passed as the CommandParameter from the command's Bookmarks collection
    /// </summary>
    public class DeleteBookmarkCommand : ICommand
    {
        public DeleteBookmarkCommand(ObservableCollection<Bookmark.MapBookmark> bookmarks)
        {
            Bookmarks = bookmarks;
        }

        /// <summary>
        /// Gets whether the command can be executed.  Ensures that there is a bookmark to delete. 
        /// </summary>
        /// <param name="parameter">The bookmark to delete</param>
        /// <returns>Returns true if there is a bookmark to delete and it belongs to the <see cref="Bookmarks"/> collection</returns>
        public bool CanExecute(object parameter)
        {
            // Make sure the bookmarks collection is initialized and that the
            // passed-in bookmark is in the collection
            Bookmark.MapBookmark bookmark = parameter as Bookmark.MapBookmark;
            return Bookmarks != null && bookmark != null && Bookmarks.Contains(bookmark);
        }

        /// <summary>
        /// The collection of bookmarks from which to delete the bookmark.
        /// </summary>
        public ObservableCollection<Bookmark.MapBookmark> Bookmarks { get; private set; }

#pragma warning disable 67 // Disable warning for event not being used.  The ICommand interface requires that the event must be declared
        /// <summary>
        /// Raised when the execution state of the command changes
        /// </summary>
        public event EventHandler CanExecuteChanged;
#pragma warning restore 67

        /// <summary>
        /// Deletes the bookmark passed as the parameter. 
        /// </summary>
        /// <param name="parameter">Utilizes the DataContextProxy via Binding on the DeleteBookmark button 
        /// to specify the bookmark to delete.</param>
        public void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                throw new ArgumentException(Strings.CannotDeleteBookmarkError);

            Bookmark.MapBookmark bookmark = parameter as Bookmark.MapBookmark;
            Bookmarks.Remove(bookmark);
        }
    }
}
