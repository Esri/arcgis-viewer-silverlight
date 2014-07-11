/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace SearchTool
{
    /// <summary>
    /// Describes an object that provide search capabilities, as well as the UI for search execution
    /// </summary>
    public interface ISearchProvider : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the command to execute the search
        /// </summary>
        ICommand Search { get; }

        /// <summary>
        /// Gets the command to cancel a search
        /// </summary>
        ICommand Cancel { get; }

        /// <summary>
        /// Gets whether a search operation is currently executing
        /// </summary>
        bool IsSearching { get; }

        /// <summary>
        /// Gets the most recent set of results returned by a search operation
        /// </summary>
        IEnumerable Results { get; }

        /// <summary>
        /// Gets the set of results in paged form
        /// </summary>
        PagedCollectionView PagedResults { get; }

        /// <summary>
        /// Raised when a search operation completes
        /// </summary>
        event EventHandler SearchCompleted;

        /// <summary>
        /// Raised when a search operation fails
        /// </summary>
        event EventHandler<UnhandledExceptionEventArgs> SearchFailed;

        /// <summary>
        /// Gets the UI for displaying results
        /// </summary>
        FrameworkElement ResultsView { get; }

        /// <summary>
        /// Gets the UI for specifying search input
        /// </summary>
        FrameworkElement InputView { get; }

        /// <summary>
        /// Gets or sets the input for the search operation
        /// </summary>
        object Input { get; set; }
    }
}
