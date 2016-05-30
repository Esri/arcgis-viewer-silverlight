/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;
using SearchTool.Resources;

namespace SearchTool
{
    /// <summary>
    /// Provider for executing text searches for places against an ArcGIS Locator service endpoint
    /// </summary>
    public abstract class SearchProviderBase : DependencyObject, ISearchProvider, INotifyPropertyChanged
    {
        public SearchProviderBase()
        {
            // Initialize commands
            search = new DelegateCommand(doSearch, canDoSearch);
            cancel = new DelegateCommand(doCancel, canDoCancel);

            // Initialize proxy URL with value from application environment if available
            if (MapApplication.Current != null && MapApplication.Current.Urls != null)
            {
                // Bind to the environment's proxy URL
                Binding b = new Binding("ProxyUrl") { Source = MapApplication.Current.Urls };
                BindingOperations.SetBinding(this, ProxyUrlProperty, b);
            }
        }

        #region Public Properties

        #region Read/Write

        private bool _useProxy;
        /// <summary>
        /// Gets or sets whether to use a proxy URL for communicating with the locator service
        /// </summary>
        public virtual bool UseProxy
        {
            get { return _useProxy; }
            set
            {
                if (_useProxy != value)
                {
                    _useProxy = value;
                    OnPropertyChanged("UseProxy");
                }
            }
        }

        private object _input;
        /// <summary>
        /// Gets or sets the search input
        /// </summary>
        public virtual object Input
        {
            get { return _input; }
            set
            {
                if (_input != value)
                {
                    _input = value;
                    OnPropertyChanged("Input");
                }
            }
        }

        /// <summary>
        /// Identifies the <see cref="ProxyUrl"/> DependencyProperty
        /// </summary>
        public static DependencyProperty ProxyUrlProperty =
            DependencyProperty.Register("ProxyUrl", typeof(string), 
            typeof(SearchProviderBase), null);

        /// <summary>
        /// Gets or sets the proxy URL to use for communicating with the locator service
        /// </summary>
        public virtual string ProxyUrl
        {
            get { return GetValue(ProxyUrlProperty) as string; }
            set { SetValue(ProxyUrlProperty, value); }
        }

        #endregion

        #region Read Only

        private DelegateCommand search;
        /// <summary>
        /// Gets the command for executing a search
        /// </summary>
        public virtual ICommand Search { get { return search; } }

        private DelegateCommand cancel;
        /// <summary>
        /// Gets the command for cancelling a search
        /// </summary>
        public virtual ICommand Cancel { get { return cancel; } }

        /// <summary>
        /// Gets whether a search is currently in progress
        /// </summary>
        private bool _isSearching;
        public virtual bool IsSearching 
        {
            get { return _isSearching; }
            protected set
            {
                if (_isSearching != value)
                {
                    _isSearching = value;

                    search.RaiseCanExecuteChanged();
                    cancel.RaiseCanExecuteChanged();

                    OnPropertyChanged("IsSearching");
                }
            }
        }

        /// <summary>
        /// Gets the UI for displaying results
        /// </summary>
        public virtual FrameworkElement ResultsView { get; protected set; }

        /// <summary>
        /// Gets the UI for taking search input
        /// </summary>
        public virtual FrameworkElement InputView { get; protected set; }

        /// <summary>
        /// Gets the latest set of search results
        /// </summary>
        public virtual IEnumerable Results { get; protected set; }

        /// <summary>
        /// Gets the latest set of paged search results
        /// </summary>
        public virtual PagedCollectionView PagedResults { get; protected set; }

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Raised when a search completes
        /// </summary>
        public event EventHandler SearchCompleted;

        // Raises the SearchCompleted event
        protected void OnSearchCompleted()
        {
            if (SearchCompleted != null)
                SearchCompleted(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raised when a search fails
        /// </summary>
        public event EventHandler<UnhandledExceptionEventArgs> SearchFailed;

        // Raises the SearchFailed event
        protected void OnSearchFailed(Exception ex)
        {
            if (SearchFailed != null)
                SearchFailed(this, new UnhandledExceptionEventArgs(ex, false));
        }

        /// <summary>
        /// Raised when a property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Command Execution

        // Executes the search
        protected virtual void doSearch(object parameter)
        {
            if (!canDoSearch(parameter))
                throw new Exception(Strings.CommandNotExecutable);
        }

        // Gets whether a search can be executed given the search parameter and object state
        protected virtual bool canDoSearch(object parameter)
        {
            return true;
        }

        // Cancels the current search
        protected virtual void doCancel(object parameter)
        {
            if (!canDoCancel(parameter))
                throw new Exception(Strings.CommandNotExecutable);
        }

        // Gets whether search can be cancelled
        protected virtual bool canDoCancel(object parameter)
        {
            return IsSearching;
        }

        #endregion
    }
}
