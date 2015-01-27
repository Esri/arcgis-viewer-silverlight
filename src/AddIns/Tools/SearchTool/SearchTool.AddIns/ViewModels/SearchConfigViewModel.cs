/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using SearchTool.Resources;
using System.Windows.Data;

namespace SearchTool
{
    [DataContract]
    public class SearchConfigViewModel : DependencyObject
    {
        // List of available search types.  New types implementing ISearchProvider 
        // can be added here.
        private Type[] availableSearchTypes = new Type[]
        {
            typeof(ArcGISLocatorPlaceSearchProvider),
            typeof(ArcGISPortalServiceSearchProvider),
            typeof(GoogleServiceSearchProvider)
        };

        private Map _map; // The map that will display spatial results

        public SearchConfigViewModel(Map map)
        {
            if (map == null)
                throw new ArgumentException();

            _map = map;

            initializeDefaultProviders();
            SelectedSearchProviders.CollectionChanged += SelectedProviders_CollectionChanged;
            addProvider = new DelegateCommand(doAddProvider, canAddProvider);
        }

        /// <summary>
        /// Create the view model from serialized configuration data
        /// </summary>
        public SearchConfigViewModel(Map map, string configData)
        {
            if (map == null)
                throw new ArgumentException();

            _map = map;

            if (!string.IsNullOrEmpty(configData))
                LoadConfiguration(configData);
            else
                initializeDefaultProviders();

            SelectedSearchProviders.CollectionChanged += SelectedProviders_CollectionChanged;
            addProvider = new DelegateCommand(doAddProvider, canAddProvider);
        }

        #region Search Providers

        private List<ISearchProvider> availableProviders;
        /// <summary>
        /// Gets the list of available search providers
        /// </summary>
        public List<ISearchProvider> AvailableSearchProviders 
        { 
            get 
            {
                if (availableProviders == null)
                {
                    availableProviders = new List<ISearchProvider>();
                    foreach (Type searchType in availableSearchTypes)
                        availableProviders.Add(instantiateSearchProvider(searchType));
                }

                return availableProviders; 
            } 
        }

        private ObservableCollection<ISearchProvider> selectedProviders = new ObservableCollection<ISearchProvider>();
        /// <summary>
        /// Gets the list of search providers selected for inclusion
        /// </summary>
        [DataMember]
        public ObservableCollection<ISearchProvider> SelectedSearchProviders { get { return selectedProviders; } }

        #endregion

        #region Add Provider Command

        private DelegateCommand addProvider;
        /// <summary>
        /// Command to add the currently selected provider to the set of selected search providers
        /// </summary>
        public ICommand AddProvider { get { return addProvider; } }

        // Adds a new instance of the passed-in search provider
        private void doAddProvider(object parameter)
        {
            if (!canAddProvider(parameter))
                throw new Exception(Strings.AddSearchError);

            SelectedSearchProviders.Add(instantiateSearchProvider(parameter.GetType()));
        }

        private bool canAddProvider(object parameter)
        {
            return parameter is ISearchProvider;
        }

        #endregion

        private void SelectedProviders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Initialize display names and descriptions of new search providers
            if (e.NewItems != null)
            {
                foreach (ISearchProvider provider in e.NewItems)
                {
                    DependencyObject d = provider as DependencyObject;
                    if (d != null)
                    {
                        string displayName = Properties.GetDisplayName(d);
                        if (string.IsNullOrEmpty(displayName))
                            Properties.SetDisplayName(d, d.GetDisplayNameFromAttribute());

                        string description = Properties.GetDescription(d);
                        if (string.IsNullOrEmpty(description))
                            Properties.SetDescription(d, d.GetDescription());
                    }
                }
            }
        }

        #region Configuration Persistence - LoadConfiguration/SaveConfiguration

        internal void LoadConfiguration(string configData)
        {
            // Remove providers individually because Clear does not raise CollectionChanged
            int count = SelectedSearchProviders.Count;
            for (int i = 0; i < count; i++)
                SelectedSearchProviders.RemoveAt(0);

            try
            {
                // Deserialize the configuration.  The configuration is represented as a list of KeyValuePairs - one
                // for each search provider.  The key is the fully qualified type name of the provider, so that it
                // can be instantiated from configuration data.  The value is a dictionary which stores the name,
                // description, and serialized configuration of each provider.
                List<KeyValuePair<string, Dictionary<string, string>>> configuration =
                    configData.DataContractDeserialize<List<KeyValuePair<string, Dictionary<string, string>>>>();

                if (configuration != null)
                {
                    // Loop through the configuration for each provider
                    foreach (KeyValuePair<string, Dictionary<string, string>> configEntry in configuration)
                    {
                        // Instanitate the provider
                        string typeName = configEntry.Key;
                        ISearchProvider provider = instantiateSearchProvider(Type.GetType(typeName));

                        // Get the stored name and description
                        Dictionary<string, string> configSettings = configEntry.Value;
                        if (provider is DependencyObject)
                        {
                            DependencyObject d = (DependencyObject)provider;

                            // TODO: Store name and description in default tool XML as resource string
                            //  names surrounded by a well-known prefix & suffix (e.g. "___").  Check for
                            //  prefix & suffix and populate from resource file if found.  Otherwise, use
                            //  value from config file directly

                            Properties.SetDisplayName(d, configSettings["Name"]);
                            Properties.SetDescription(d, configSettings["Description"]);
                        }

                        // Load the serialized provider's configuration, if it exists
                        if (provider is ISupportsConfiguration)
                            ((ISupportsConfiguration)provider).LoadConfiguration(configSettings["Configuration"]);

                        if (provider is ArcGISPortalServiceSearchProvider)
                        {
                            var portalSearch = (ArcGISPortalServiceSearchProvider)provider;
                            Binding b = new Binding("Portal") { Source = MapApplication.Current };
                            BindingOperations.SetBinding(portalSearch, ArcGISPortalServiceSearchProvider.PortalProperty, b);
                        }

                        SelectedSearchProviders.Add(provider);
                    }
                }
                else
                {
                    initializeDefaultProviders();
                }
            }
            catch
            {
                initializeDefaultProviders();
            }
        }

        internal string SaveConfiguration()
        {
            List<KeyValuePair<string, Dictionary<string, string>>> providerTypes = 
                new List<KeyValuePair<string, Dictionary<string, string>>>();

            // Store the configuration of each provider in a dictionary
            foreach (ISearchProvider search in SelectedSearchProviders)
            {
                // Store the provider's name and description
                Dictionary<string, string> configSettings = new Dictionary<string, string>();
                configSettings.Add("Name", Properties.GetDisplayName(search as DependencyObject));
                configSettings.Add("Description", Properties.GetDescription(search as DependencyObject));

                // Get the provider's serialized configuration if it supports doing so
                if (search is ISupportsConfiguration)
                    configSettings.Add("Configuration", ((ISupportsConfiguration)search).SaveConfiguration());

                // Create a configuration entry from the configuration settings and the provider's 
                // fully-qualified type name
                KeyValuePair<string, Dictionary<string, string>> providerConfigEntry =
                    new KeyValuePair<string, Dictionary<string, string>>(
                        search.GetType().AssemblyQualifiedName, configSettings);
                providerTypes.Add(providerConfigEntry);
            }

            // Serialize the configuration 
            return providerTypes.DataContractSerialize();
        }

        #endregion 

        #region Private Utility Methods - initializeDefaultProviders, instantiateSearchProvider

        // Creates a default set of searches and adds them to the selected set
        private void initializeDefaultProviders()
        {
            // Add provider for place search
            ArcGISLocatorPlaceSearchProvider placeSearch = 
                new ArcGISLocatorPlaceSearchProvider(_map, "http://geocode.arcgis.com/ArcGIS/rest/services/World/GeocodeServer");
            Properties.SetDescription(placeSearch, placeSearch.GetDescription());
            SelectedSearchProviders.Add(placeSearch);

            // Add provider for ArcGIS Portal search
            ArcGISPortalServiceSearchProvider portalSearch = new ArcGISPortalServiceSearchProvider();
            Binding b = new Binding("Portal") { Source = MapApplication.Current };
            BindingOperations.SetBinding(portalSearch, ArcGISPortalServiceSearchProvider.PortalProperty, b);

            Properties.SetDescription(portalSearch, portalSearch.GetDescription());
            SelectedSearchProviders.Add(portalSearch);

            // Add provider for web search
            GoogleServiceSearchProvider webSearch = new GoogleServiceSearchProvider();
            Properties.SetDescription(webSearch, webSearch.GetDescription());
            SelectedSearchProviders.Add(webSearch);
        }

        // Instantiates a search provider of the specified type
        private ISearchProvider instantiateSearchProvider(Type searchType)
        {
            Type[] mapType = new Type[] { typeof(Map) };

            // Check whether the search provider has a constructor that takes a map object
            ConstructorInfo constructorInfo = searchType.GetConstructor(mapType);

            if (constructorInfo == null) // Assume a parameterless constructor
                return Activator.CreateInstance(searchType) as ISearchProvider;
            else // Pass the map to the constructor
                return Activator.CreateInstance(searchType, new object[] { _map }) as ISearchProvider;
        }

        #endregion
    }
}
