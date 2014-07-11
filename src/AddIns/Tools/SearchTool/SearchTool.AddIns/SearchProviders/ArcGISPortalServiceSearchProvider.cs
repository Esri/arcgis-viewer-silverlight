/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using ESRI.ArcGIS.Client.Portal;
using System.Windows;

namespace SearchTool
{
    /// <summary>
    /// Provider for executing text searches for map, image, and feature services against an ArcGIS Portal endpoint
    /// </summary>
    [LocalizedDisplayName("AGOL")]
    [LocalizedDescription("SearchArcGISCom")]
    public class ArcGISPortalServiceSearchProvider : SearchProviderBase
    {
        #region Member Variables

        // String that will  be appended to searches to limit the types of items to search for.
        private string _serviceTypeQualifier = 
            "(type:\"Map Service\" OR type:\"Image Service\" OR type:\"Feature Service\")";
        private int _pageSize = 12; // The size of the page to show results on
        private int _searchStartIndex = 1; // The index for getting (more) results
        private bool _pageChangeSearch = false; // Whether the search operation was initiated because of a page change
        private List<int> _pagesRetrieved = new List<int>(); // Collection of pages that results have been retrieved for
        private string _lastSearch; // Stores the most recently executed search input
        private bool _cancelled = false; // Flag for whether the most recent operation has been cancelled
                
        // Stores the most recent set of results
        private ObservableCollection<SearchResultViewModel> _results = 
            new ObservableCollection<SearchResultViewModel>();

        #endregion

        public ArcGISPortalServiceSearchProvider()
        {
            Results = _results;
            PagedResults = new PagedCollectionView(Results) { PageSize = _pageSize, };
            PagedResults.PageChanging += PagedResults_PageChanging;
            ResultsView = new ServiceSearchResultsView();
            InputView = new SingleLineSearchInputView();
            Properties.SetDisplayName(this, this.GetDisplayName());
        }

        public static readonly DependencyProperty PortalProperty = DependencyProperty.Register(
            "PortalProperty", typeof(ArcGISPortal), typeof(ArcGISPortalServiceSearchProvider),
            new PropertyMetadata(OnPortalChanged));

        /// <summary>
        /// Gets or sets the ArcGIS Portal endpoint to search against
        /// </summary>
        public ArcGISPortal Portal
        {
            get { return GetValue(PortalProperty) as ArcGISPortal; }
            set { SetValue(PortalProperty, value); }
        }

        private static void OnPortalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ArcGISPortalServiceSearchProvider provider = (ArcGISPortalServiceSearchProvider)d;
            ArcGISPortal oldPortal = e.OldValue as ArcGISPortal;
            ArcGISPortal newPortal = e.NewValue as ArcGISPortal;

            // Unhook from old portal
            if (oldPortal != null)
                oldPortal.PropertyChanged -= provider.Portal_PropertyChanged;

            if (newPortal != null)
            {
                // Initialize display name if portal is initialized.  Otherwise, listen for property changes
                // and wait for portal initialization.
                if (newPortal.IsInitialized && string.IsNullOrEmpty(Properties.GetDisplayName(provider)))
                    provider.initDisplayName();
                else
                    newPortal.PropertyChanged += provider.Portal_PropertyChanged;
            }

            provider.OnPropertyChanged("Portal");
        }

        #region Command Execution 

        // Executes the search
        protected override void doSearch(object parameter)
        {
            base.doSearch(parameter);

            IsSearching = true; // Update busy state so new searches can't be initiated until the current one completes or fails

            string input = parameter as string != null ? parameter as string : Input as string;
            string searchString = input.ToString().Trim();

            if (searchString == "*")
                searchString = _serviceTypeQualifier;
            else
                searchString += " AND " + _serviceTypeQualifier;

            if (!_pageChangeSearch)
            {
                // Search was not the result of a page change, but rather was user-initiated.  Reset search data.
                _lastSearch = searchString; // Store the initial search string so it can be used when the page changes.
                _searchStartIndex = 1;
                _pagesRetrieved.Clear();
                _pagesRetrieved.Add(0);
                _results.Clear(); // Clear results from previous searches
            }

            // Reset flag for whether operation was cancelled
            _cancelled = false;

            // Initialize search parameters and execute
            SearchParameters searchParams = new SearchParameters()
            {
                Limit = _pageSize,
                StartIndex = _searchStartIndex,
                QueryString = !_pageChangeSearch ? searchString : _lastSearch,
                SortField = "NumViews",
                SortOrder = QuerySortOrder.Descending
            };

            Portal.SearchItemsAsync(searchParams, SearchItemsCompleted);
        }

        // Gets whether a search can be executed given the search parameter and object state
        protected override bool canDoSearch(object parameter)
        {
            string input = parameter as string != null ? parameter as string : Input as string;

            return input != null && !string.IsNullOrEmpty(input.ToString().Trim())
                && !IsSearching && Portal != null && Portal.IsInitialized;
        }

        // Cancels the current search
        protected override void doCancel(object parameter)
        {
            base.doCancel(parameter);

            // Set flag to let other logic know operation has been cancelled
            _cancelled = true;

            // Check whether search is executing as a result of page change
            if (_pageChangeSearch)
            {
                // Remove page currently being retrieved from list of those retrieved
                _pagesRetrieved.RemoveAt(_pagesRetrieved.Count - 1);
                // Reset flag
                _pageChangeSearch = false;
            }

            IsSearching = false;
            OnSearchCompleted();
        }

        #endregion

        #region Event Handlers

        // Raised when the asynchronous call to search the ArcGIS Portal has completed.
        private void SearchItemsCompleted(SearchResultInfo<ArcGISPortalItem> result, Exception ex)
        {
            if (ex != null) // Error occurred
            {
                IsSearching = false; // Reset busy state
                OnSearchFailed(ex); // Fire failed event
                return;
            }

            if (_cancelled) // search was cancelled
            {
                _cancelled = false;
                return;
            }

            if (!_pageChangeSearch) // Result of a new search
            {
                // Add and initialize results
                foreach (ArcGISPortalItem item in result.Results)
                {
                    SearchResultViewModel viewModel = new SearchResultViewModel(item) 
                        { ProxyUrl = this.UseProxy ? this.ProxyUrl : null };
                    _results.Add(viewModel);

                    viewModel.Initialize();
                }

                // Add empty results for each of the pages not yet returned
                for (int i = 0; i < result.TotalCount - _pageSize; i++)
                {
                    SearchResultViewModel viewModel = new SearchResultViewModel()
                        { ProxyUrl = this.UseProxy ? this.ProxyUrl : null };
                    _results.Add(viewModel);
                }

                OnSearchCompleted(); // Fire completed event
            }
            else // Result of a search initiated by a page change
            {
                // index in the entire collection of the first result on the current page
                int i = _searchStartIndex - 1;
                foreach (ArcGISPortalItem item in result.Results)
                {
                    // Update the empty result corresponding to the return portal item
                    SearchResultViewModel viewModel = _results[i];
                    viewModel.Result = item;
                    viewModel.Initialize();
                    i++;
                }

                // Execute the requested page change
                PagedResults.MoveToPage(_pagesRetrieved[_pagesRetrieved.Count - 1]);

                // Reset flag
                _pageChangeSearch = false;
            }

            IsSearching = false; // Reset busy state
        }

        // Fires when the page is changed
        private void PagedResults_PageChanging(object sender, PageChangingEventArgs e)
        {
            // Don't do anything if the requested page's results have already been retrieved
            if (_pagesRetrieved.Contains(e.NewPageIndex))
                return;

            // Set flag indicating that the ensuing search is due to the page being changed
            _pageChangeSearch = true;
            if (e.NewPageIndex == 0) // Should never be the case, but coding defensively here
                _searchStartIndex = 1;
            else 
                _searchStartIndex = e.NewPageIndex * _pageSize + 1;

            // Add the requested page to the list of those that have been retrieved
            _pagesRetrieved.Add(e.NewPageIndex);

            // Request results for the page
            doSearch(_lastSearch);

            // Prevent the page from changing.  Once the results are returned, it will be changed
            // programmatically
            e.Cancel = true;
        }

        // Raised when a propert on the provider's ArcGIS Portal changes.  Used to detect portal
        // initialization and subequently initialize display name.
        private void Portal_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Deployment.Current.Dispatcher.CheckAccess())
            {
                if (e.PropertyName == "IsInitialized" && Portal.IsInitialized
                && string.IsNullOrEmpty(Properties.GetDisplayName(this)))
                    initDisplayName();
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (e.PropertyName == "IsInitialized" && Portal.IsInitialized
                    && string.IsNullOrEmpty(Properties.GetDisplayName(this)))
                        initDisplayName();
                });
            }
        }

        #endregion

        // Initializes the search provider's display name based on the portal's name
        private void initDisplayName()
        {
            string name = Portal.ArcGISPortalInfo.Name ?? Portal.ArcGISPortalInfo.PortalName;
            Properties.SetDisplayName(this, name);
        }
    }
}
