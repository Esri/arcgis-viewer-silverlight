/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SearchTool.Resources;

namespace SearchTool
{
    /// <summary>
    /// Supports executing searches against one or more search providers
    /// </summary>
    public class SearchViewModel : DependencyObject
    {
        public SearchViewModel()
        {
            Search = new DelegateCommand(doSearch, canDoSearch);
            PlaceSearch = new DelegateCommand(doPlaceSearch, canDoPlaceSearch);
            SearchProviders = new ObservableCollection<ISearchProvider>();
            SearchProviders.CollectionChanged += SearchProviders_CollectionChanged;
        }

        /// <summary>
        /// Gets the search providers that searches can be executed against
        /// </summary>
        public ObservableCollection<ISearchProvider> SearchProviders { get; private set; }

        /// <summary>
        /// Gets the command for executing a search
        /// </summary>
        public ICommand Search { get; private set; }

        /// <summary>
        /// Gets the command for executing a place search
        /// </summary>
        public ICommand PlaceSearch { get; private set; }

        // Executes a search on each search provider
        private void doSearch(object parameter)
        {
            if (!canDoSearch(parameter))
                throw new Exception(Strings.CommandNotExecutable);

            foreach (ISearchProvider searchProvider in SearchProviders)
            {
                if (searchProvider.Search.CanExecute(parameter))
                    searchProvider.Search.Execute(parameter);
            }
        }

        // Checks whether a search can be executed.  Requires that the current search input is
        // valid for each search provider.
        private bool canDoSearch(object parameter)
        {
            if (SearchProviders == null)
                return false;

            foreach (ISearchProvider searchProvider in SearchProviders)
            {
                if (searchProvider.Search.CanExecute(parameter)) // Only need one to be executable
                    return true;
            }

            return false;
        }

        // Executes a search on each place search provider
        private void doPlaceSearch(object parameter)
        {
            if (!canDoPlaceSearch(parameter))
                throw new Exception(Strings.CommandNotExecutable);

            var placeSearchProviders = SearchProviders.OfType<ArcGISLocatorPlaceSearchProvider>();
            foreach (ArcGISLocatorPlaceSearchProvider placeSearchProvider in placeSearchProviders)
                placeSearchProvider.Search.Execute(parameter);
        }

        // Checks whether a search can be executed.  Requires that the current search input is
        // valid for each search provider.
        private bool canDoPlaceSearch(object parameter)
        {
            if (SearchProviders == null)
                return false;

            var placeSearchProviders = SearchProviders.OfType<ArcGISLocatorPlaceSearchProvider>();
            foreach (ArcGISLocatorPlaceSearchProvider placeSearchProvider in placeSearchProviders)
            {
                if (placeSearchProvider.Search.CanExecute(parameter)) // Only need one to be executable
                    return true;
            }

            return false;
        }

        // Raised when a search provider is added or removed
        private void SearchProviders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                object input = null;
                // Get existing input so new providers' inputs can be synced
                if (SearchProviders.Count > e.NewItems.Count)
                {
                    ISearchProvider preExistingProvider = SearchProviders.FirstOrDefault(p => !e.NewItems.Contains(p));
                    if (preExistingProvider != null)
                        input = preExistingProvider.Input;
                }

                foreach (ISearchProvider provider in e.NewItems)
                {
                    // Listen for property changes on any new search providers
                    provider.PropertyChanged += SearchProvider_PropertyChanged;

                    // Initialize input
                    provider.Input = input;
                }
            }

            if (e.OldItems != null)
            {
                // Unhook from property changes on any removed search providers
                foreach (ISearchProvider provider in e.OldItems)
                    provider.PropertyChanged -= SearchProvider_PropertyChanged;
            }

            raiseCanExecuteChanged();
        }

        private object lastInput = "";
        // Raised when a property changes on a search provider
        private void SearchProvider_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsUpdatingLocatorInfo")
            {
                raiseCanExecuteChanged();
                return;
            }
            else if (e.PropertyName != "IsSearching" && e.PropertyName != "Input")
            {
                return;
            }

            ISearchProvider originatingProvider = (ISearchProvider)sender;
            foreach (ISearchProvider provider in SearchProviders)
            {
                if (!provider.Equals(originatingProvider)) // Don't touch the provider that fired the property change
                {
                    if (e.PropertyName == "Input")
                    {
                        object input = originatingProvider.Input;
                        provider.Input = input; // Sync input across providers
                    }
                    else if (e.PropertyName == "IsSearching" && originatingProvider.IsSearching 
                        && provider.Search.CanExecute(provider.Input)) 
                    {
                        // A new search was executed on one search provider.        
                        // Execute across providers.
                        provider.Search.Execute(provider.Input);
                        lastInput = provider.Input;
                    }
                }
            }
        }

        private void raiseCanExecuteChanged()
        {
            ((DelegateCommand)Search).RaiseCanExecuteChanged();
            ((DelegateCommand)PlaceSearch).RaiseCanExecuteChanged();
        }
    }
}
