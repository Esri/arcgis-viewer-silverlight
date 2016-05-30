/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using SearchTool.Resources;

namespace SearchTool
{
    /// <summary>
    /// Provider for executing text searches for map services against Google
    /// </summary>
    [LocalizedDisplayName("TheWeb")]
    [LocalizedDescription("SearchTheWeb")]
    public class GoogleServiceSearchProvider : SearchProviderBase
    {
        #region Member Variables

        private const int _NUMREQUESTS = 8; // Number of search requests to make
        private const int _NUMRESULTSPERSEARCH = 8; // Number of results to request per search
        private int _numberWebResults; // Number of web requests for which responses have been received
        private bool _cancelled = false; // Flag tracking whether the most recent operation was cancelled

        // Limits refreshes of results to no more than once within a certain interval
        ThrottleTimer _updateResultsTimer = null; 

        // Search endpoint
        private const string _SEARCHURL = @"http://ajax.googleapis.com/ajax/services/search/"; 

        // stores the most recent set of search results
        private ObservableCollection<SearchResultViewModel> _results =
            new ObservableCollection<SearchResultViewModel>();

        // stores the validated set of search results
        List<SearchResultViewModel> _checkedResults = new List<SearchResultViewModel>();

        #endregion

        public GoogleServiceSearchProvider()
        {
            Results = _results;
            PagedResults = new PagedCollectionView(Results)
            {
                PageSize = 12,
                Filter = (o) => { return ((SearchResultViewModel)o).IsInitialized; }
            };
            Properties.SetDisplayName(this, this.GetDisplayNameFromAttribute());
            ResultsView = new ServiceSearchResultsView();
            InputView = new SingleLineSearchInputView();
        }

        #region Command Execution

        // Executes the search
        protected override void doSearch(object parameter)
        {
            base.doSearch(parameter);

            IsSearching = true; // Update busy state so searches aren't initiated until search is completed
            _results.Clear(); // Clear previous results
            _checkedResults.Clear();

            // Reset the counter for number of responses received
            _numberWebResults = 0;

            // Get the search input
            string searchString = parameter.ToString().Trim();

            string query = "";

            // Reset flag for whether operation was cancelled
            _cancelled = false;

            // Calculate the number to iterate to based on the number of requests and number of results per request
            int loopCeiling = (_NUMREQUESTS * _NUMRESULTSPERSEARCH) - (_NUMRESULTSPERSEARCH - 1);
            //request 8 pages of search results, max number of results from google is 64
            for (int i = 0; i < loopCeiling; i += _NUMRESULTSPERSEARCH)
            {
                // Construct the search query for one page of results
                query = "web?q=" + searchString + "%20inurl%3Arest%20inurl%3Aservices%20%22inurl%3AMapServer" +
                    "%22%20Supported%20Operations%22%20%22Supported%20Interfaces&v=1.0&rsz=8&start=" + i;
                WebUtil.OpenReadAsync(_SEARCHURL + query, null, WebSearchCompleted); // execute the search
            }
        }

        // Gets whether a search can be executed given the search parameter and object state
        protected override bool canDoSearch(object parameter)
        {
            return parameter != null && !string.IsNullOrEmpty(parameter.ToString().Trim()) && !IsSearching;
        }

        // Cancels the current search
        protected override void doCancel(object parameter)
        {
            base.doCancel(parameter);

            // Set flag so other logic knows the operation was cancelled
            _cancelled = true;
            _results.Clear(); // Clear any results that have already been added

            IsSearching = false;
            OnSearchCompleted();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Raised when the asynchronous call to search the web has completed.
        /// </summary>
        private void WebSearchCompleted(object sender, OpenReadEventArgs e)
        {
            // Received another response, so increment the counter
            _numberWebResults++;

            if (e.Error == null && !_cancelled) // No error, no cancellation - handle result as normal
            {
                GoogleResponse result = WebUtil.ReadObject<GoogleResponse>(e.Result);
                if (result != null && result.ResponseData != null && result.ResponseData.Items.Length > 0)
                {
                    // before services from the web get added we need to check if they are accessible
                    foreach (WebSearchResultItem item in result.ResponseData.Items)
                    {
                        Uri url = new Uri(item.Url);
                        ArcGISService.GetServiceInfoAsync(item.Url, item, CheckServiceAccessCompleted);
                    }
                    return;
                }
            }

            // There was an error, the response was empty, or the operation was cancelled.  Check whether this is the last response.
            if (_numberWebResults == _NUMREQUESTS)
            {
                // All requests have returned.  Go through completeion routine.

                if (!_cancelled)
                {
                    IsSearching = false; // Reset busy state
                    OnSearchCompleted(); // Raise completed event
                }
            }

        }

        /// <summary>
        /// Raised when the asynchronous call to check if the map service is accessible has completed.
        /// </summary>
        private void CheckServiceAccessCompleted(object sender, ServiceEventArgs e)
        {
            if (e.Service == null)
                return; //the map service is not accessible

            // Check the current result against the ones already received.  Duplicates are often found.
            bool alreadyAdded = false;
            foreach (SearchResultViewModel result in _checkedResults)
            {
                ArcGISService webSearchRes = (ArcGISService)result.Result;
                if (webSearchRes.Title.Equals(e.Service.Title) && webSearchRes.Url.Equals(e.Service.Url))
                {
                    alreadyAdded = true; //add search results only once
                    break;
                }
            }

            // If it's a new service, initialize the result viewModel and add it to the collection of
            // results that have had their service access checked
            if (!alreadyAdded && !_cancelled)
            {
                SearchResultViewModel viewModel = new SearchResultViewModel(e.Service)
                    { ProxyUrl = this.UseProxy ? this.ProxyUrl : null };
                _checkedResults.Add(viewModel);

                viewModel.Initialized += resultViewModel_Initialized;
                viewModel.Initialize();
            }
        }

        // Raised when each result's view model initializes
        private void resultViewModel_Initialized(object o, EventArgs args)
        {
            if (_updateResultsTimer == null)
            {
                // Use a throttler so refreshing the paged collection, which can be UI intensive, 
                // doesn't happen too often
                _updateResultsTimer = new ThrottleTimer(500, () =>
                {
                    if (!_cancelled)
                    {
                    // Add the initialized results to the public-facing collection
                    foreach (SearchResultViewModel viewModel in _checkedResults)
                    {
                        if (!_results.Contains(viewModel) && viewModel.IsInitialized)
                            _results.Add(viewModel);
                    }

                    // Update the paged collection of results
                    PagedResults.Refresh();

                    if (_numberWebResults == _NUMREQUESTS) // All requests have returned
                    {
                        IsSearching = false; // Reset busy state
                        OnSearchCompleted(); // Raise completed event
                        }
                    }
                });
            }
            _updateResultsTimer.Invoke();
        }

        #endregion
    }
}
