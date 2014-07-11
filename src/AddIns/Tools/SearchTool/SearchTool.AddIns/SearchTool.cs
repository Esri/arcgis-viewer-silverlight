/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using ESRI.ArcGIS.Client.Extensibility;
using SearchTool.Resources;
using System.Windows.Interactivity;
using System.Collections.Generic;
using System.Windows.Browser;

namespace SearchTool
{
    [Export(typeof(ICommand))]
    [LocalizedDisplayName("SearchToolName")]
    [DefaultIcon("/SearchTool.AddIns;component/Images/search.png")]
    [LocalizedDescription("SearchDescription")]
    [LocalizedCategory("SearchCategory")]
    public class SearchTool : IToggleCommand, ISupportsWizardConfiguration
    {
        // whether to show the search UI in the side panel or in a floating window
        private bool m_showInSidePanel = true;

        // Well-known IDs of side panel elements
        private const string SIDE_PANEL_NAME = "SidePanelContainer";
        private const string SIDE_PANEL_BORDER_NAME = "SidePanelBorder";

        private SearchView m_searchView = null;
        private object m_searchItem = null;
        private SearchViewModel m_searchViewModel = null;
        private Grid m_searchViewContainer;
        private string m_savedConfiguration = null;
        private bool m_sidePanelIsTabControl = false;
        SearchConfigViewModel m_configViewModel;
        WizardPage m_searchSelectionPage;
        private bool m_addedToSidePanel;

        private bool m_searchQueryStringHandled = false;

        bool m_cancelled = false;

        // Timer to retry adding search UI to the side panel.  Necessary because the framework does not provide
        // notification that the application has initialized and the visual tree is ready to be manipulated.
        private DispatcherTimer addToSidePanelTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.5) };

        public SearchTool()
        {

        }

        #region ICommand members

        /// <summary>
        /// Toggles the search UI on or off
        /// </summary>
        public void Execute(object parameter)
        {
            ItemsControl sidePanel = null;
            Storyboard showStoryboard = null;
            Storyboard hideStoryboard = null;
            if (m_showInSidePanel)
            {
                // Get side panel if one is being used
                sidePanel = MapApplication.Current.FindObjectInLayout(SIDE_PANEL_NAME) as ItemsControl;

                // Get show/hide animations
                showStoryboard = sidePanel.FindStoryboard(SIDE_PANEL_NAME + "_Show");
                hideStoryboard = sidePanel.FindStoryboard(SIDE_PANEL_NAME + "_Hide");
            }

            if (!_isChecked) // Only execute if the button is toggled on
            {
                if (m_searchViewModel == null) // Initialize view model
                    initSearchViewModel();

                if (m_showInSidePanel)  // Check whether to show UI in side panel or window
                {
                    // Get search tab from side panel
                    ((dynamic)sidePanel).SelectedItem = m_searchItem;

                    // Stop hide animation if it's in progress
                    if (hideStoryboard != null && hideStoryboard.IsRunning())
                        hideStoryboard.Stop();

                    // Show side panel using visual state if possible, otherwise set to visible directly
                    if (showStoryboard != null)
                        showStoryboard.Begin();
                    else
                        sidePanel.Visibility = Visibility.Visible;
                }
                else
                {
                    // Show search UI in floating window
                    MapApplication.Current.ShowWindow(Strings.Search, m_searchView, false, null, (o, e) =>
                        {
                            _isChecked = false;
                            OnCanExecuteChanged();
                        });
                }

                _isChecked = true; // Update flag to indicate that button is toggled on
                OnCanExecuteChanged(); // Raise can execute changed so toggle state change gets picked up
            }
            else
            {
                if (m_showInSidePanel) // Check whether UI is shown in side panel or window
                {
                    // Hide side panel
                    if (((dynamic)sidePanel).SelectedItem == m_searchItem)
                    {
                        // Stop show animation if in progress
                        if (showStoryboard != null && showStoryboard.IsRunning())
                            showStoryboard.Stop();

                        if (hideStoryboard != null)
                            hideStoryboard.Begin();
                        else
                            sidePanel.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    MapApplication.Current.HideWindow(m_searchView);
                }
                _isChecked = false; // Update flag to indicate that button is toggled off
                OnCanExecuteChanged(); // Raise can execute changed so toggle state change gets picked up
            }
        }

        /// <summary>
        /// Checks whether the command is in an executable state
        /// </summary>
        public bool CanExecute(object parameter)
        {
            // Return true so that the command can always be executed
            return true;
        }

        /// <summary>
        /// Raised when the command's toggle or execution state changes
        /// </summary>
        public event EventHandler CanExecuteChanged;

        // Raises teh CanExecuteChanged event
        private void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        #endregion

        #region IToggleCommand Members

        private bool _isChecked = false;
        /// <summary>
        /// Gets whether the command is toggled on or off
        /// </summary>
        public bool IsChecked()
        {
            return _isChecked;
        }

        #endregion

        #region ISupportsConfiguration Members

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Configure() 
        {
            // No logic needed in this case
        }

        /// <summary>
        /// Initializes the search tool based on a configuration string
        /// </summary>
        /// <param name="configData"></param>
        public void LoadConfiguration(string configData)
        {
            if (!m_cancelled)
                initUI();

            // Initialize the search configuration view model
            initConfigViewModel(configData);

            // Save the configuration in case the tool needs to revert back to it
            m_savedConfiguration = configData;

            // Initialize the search view model
            initSearchViewModel();

            // Flag to track whether a search is being executed on startup
            bool executingSearch = false;

            #region handle search query string

            // Check whether query string is present
            if (HtmlPage.Document != null && HtmlPage.Document.QueryString != null 
                && !m_cancelled && !m_searchQueryStringHandled)
            {
                // Set flag to indicate that the search query string has been handled.  This logic should 
                // only execute once per app load.
                m_searchQueryStringHandled = true;

                // Put query string values in a case-insensitive dictionary
                var queryString = new Dictionary<string, string>(HtmlPage.Document.QueryString, 
                    StringComparer.InvariantCultureIgnoreCase);

                // Check whether query string contains value for search
                if (queryString.ContainsKey("search"))
                {
                    // get the search string
                    var searchString = queryString["search"];

                    // Set flag indicating a search is executing
                    executingSearch = true;

                    // Make sure the search can execute with the given input
                    if (m_searchViewModel != null)
                    {
                        if (m_searchViewModel.PlaceSearch.CanExecute(searchString))
                        {

                            // Do the search.  Embed execution in BeginInvoke in case search result comes back 
                            // immediately (for instance, if it is cached).  This will allow the UI to be rendered 
                            // before the result is returned, so UI-dependent events are triggered when the results 
                            // view(s) is/are updated.
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                m_searchViewModel.PlaceSearch.Execute(searchString);

                                // Apply search string to input of all search providers to allow UI to pick up input
                                foreach (ISearchProvider provider in m_searchViewModel.SearchProviders)
                                    provider.Input = searchString;
                            });
                        }
                        else
                        {
                            // Wait for PlaceSearch command to become available
                            EventHandler executeChangedHandler = null;
                            executeChangedHandler = (o, e) =>
                                {
                                    m_searchViewModel.PlaceSearch.CanExecuteChanged -= executeChangedHandler;

                                    if (m_searchViewModel.PlaceSearch.CanExecute(searchString))
                                    {
                                            m_searchViewModel.PlaceSearch.Execute(searchString);
                                            foreach (ISearchProvider provider in m_searchViewModel.SearchProviders)
                                                provider.Input = searchString;
                                    }
                                };
                            m_searchViewModel.PlaceSearch.CanExecuteChanged += executeChangedHandler;
                        }
                    }
                }
            }
            #endregion

            // Render the view to allow UI initialization logic to fire
            if (m_showInSidePanel && !m_cancelled)
                loadSearchVisualTree(executingSearch);
        }

        /// <summary>
        /// Serializes the search tool's configuration to a string
        /// </summary>
        public string SaveConfiguration()
        {
            return m_configViewModel.SaveConfiguration();
        }

        #endregion

        #region ISupportsWizardConfiguration Members

        private WizardPage currentPage;
        /// <summary>
        /// Gets or sets the current page in the tool's configuration wizard
        /// </summary>
        public WizardPage CurrentPage
        {
            get { return currentPage; }
            set
            {
                if (currentPage != value)
                    currentPage = value;
            }
        }

        private Size desiredSize = new Size(400, 410);
        /// <summary>
        /// Gets or sets the optimal size for display of the configuration pages
        /// </summary>
        public Size DesiredSize
        {
            get { return desiredSize; }
            set
            {
                if (desiredSize != value)
                    desiredSize = value;
            }
        }

        /// <summary>
        /// Cancels the current configuration, reverting to the last saved state
        /// </summary>
        public void OnCancelled()
        {
            // Saved configuration will be null when cancellation occurred while adding a new search tool
            if (m_savedConfiguration == null)
                return;

            m_cancelled = true;
            // Load the last saved configuration
            LoadConfiguration(m_savedConfiguration);
            m_cancelled = false;

            // Regenerate configuration pages
            Pages = createConfigPages();
        }

        /// <summary>
        /// Notifies the tool that configuration has completed
        /// </summary>
        public void OnCompleted()
        {
            // Check whether there is a saved configuration to see whether this is a new search tool
            if (m_savedConfiguration == null)
            {
                // Now that configuration has completed, initialize the search tool's visual tree
                initUI();
                loadSearchVisualTree(false);
            }

            // Update variable storing the last saved config
            m_savedConfiguration = m_configViewModel.SaveConfiguration();
        }

        /// <summary>
        /// Notifies the tool that the page is changing
        /// </summary>
        /// <returns>A boolean indicating whether the page change should be allowed</returns>
        public bool PageChanging()
        {
            return true;
        }

        ObservableCollection<WizardPage> pages;
        /// <summary>
        /// Gets the set of pages for configuring the search tool
        /// </summary>
        public ObservableCollection<WizardPage> Pages
        {
            get
            {
                if (pages == null)
                    pages = createConfigPages();

                return pages;
            }
            set
            {
                if (pages != value)
                    pages = value;
            }
        }

        #endregion

        #region Event Handlers

        // Raised when a search type is removed or added
        private void SelectedSearchProviders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (m_searchViewModel != null)
            {
                // Add newly added searches to the tool
                if (e.NewItems != null)
                {
                    foreach (ISearchProvider search in e.NewItems)
                        m_searchViewModel.SearchProviders.Add(search);
                }

                // Remove deleted searches from the tool
                if (e.OldItems != null)
                {
                    foreach (ISearchProvider search in e.OldItems)
                        m_searchViewModel.SearchProviders.Remove(search);
                }
            }
        }

        // Raised when the side panel's visiblity changes
        private static void OnSidePanelVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Get the side panel and the search tool
            ItemsControl sidePanel = (ItemsControl)d;
            SearchTool searchTool = (SearchTool)Properties.GetAttachedObject(sidePanel);

            // Update the search tool's toggle state if necessary
            if (sidePanel.Visibility == Visibility.Collapsed && searchTool._isChecked)
            {
                searchTool._isChecked = false;
                searchTool.OnCanExecuteChanged();
            }
            else if (sidePanel.Visibility == Visibility.Visible
                && ((dynamic)sidePanel).SelectedItem == searchTool.m_searchItem
                && !searchTool._isChecked)
            {
                searchTool._isChecked = true;
                searchTool.OnCanExecuteChanged();
            }
        }

        #endregion 

        #region Private Utility Methods

        private void initUI()
        {
            // Instantiate search UI
            m_searchView = new SearchView();

            // Check whether search UI is to be shown in side panel or floating window
            if (m_showInSidePanel)
            {
                initSidePanelContainer();

                // Try adding the UI to the side panel.  This will not succeed until the visual tree has loaded
                // and is ready for manipulation.
                if (!m_addedToSidePanel && !tryAddSearchToSidePanel())
                {
                    // The visual tree was not ready yet, so keep retrying
                    addToSidePanelTimer.Tick += (o, e) =>
                    {
                        if (tryAddSearchToSidePanel())
                        {
                            // adding the search UI to the side panel worked, so stop the retry timer
                            addToSidePanelTimer.Stop();

                            // Initialize alignment, margins, etc on the UI.  Done after determining whether the
                            // UI can be added to the side panel because these properties should be different
                            // depending on how the UI will be shown.
                            initSearchView();
                        }
                        else if (!m_showInSidePanel)
                        {
                            // Initialize alignment, margins, etc on the UI.
                            initSearchView();

                            // We know that adding the Search UI to the side panel won't work, so stop the timer
                            addToSidePanelTimer.Stop();
                        }
                    };
                }
                else
                {
                    // Search UI cannot be shown in the side panel - initialize the UI
                    initSearchView();
                }
            }
            else
            {
                // Search was intentionally set to not show in the side panel, so initialize the UI
                initSearchView();
            }
        }

        // Initialize margins, alignment, etc on the search view.  If shown in side panel, 
        // adds the view to a container grid
        private void initSearchView()
        {
            if (m_showInSidePanel)
            {
                m_searchView.HorizontalAlignment = HorizontalAlignment.Stretch;
                m_searchView.VerticalAlignment = VerticalAlignment.Stretch;
                m_searchView.Margin = new Thickness(10, 7, 10, 5);

                if (m_sidePanelIsTabControl)
                {
                    Grid.SetRow(m_searchView, 2);
                    m_searchViewContainer.Children.Add(m_searchView);
                }

                setColumnMinWidth();
            }
            else
            {
                m_searchView.Width = 280;
                m_searchView.Height = 630;
                m_searchView.Margin = new Thickness(15, 15, 15, 20);
            }
        }

        #region Side Panel

        // Initializes a container grid for holding the search view in the side panel.  Wraps
        // the search view with a title and separator line.
        private void initSidePanelContainer()
        {
            string containerXaml = @"
                    <Grid Margin=""2""
                        xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                        xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                        xmlns:local=""clr-namespace:SearchTool;assembly=SearchTool.AddIns"">
                        <Grid.Resources>
                            <local:StringResourcesManager x:Key=""Localization"" />
                        </Grid.Resources>
                        <Grid.RowDefinitions>
                            <RowDefinition Height=""Auto"" />
                            <RowDefinition Height=""2"" />
                            <RowDefinition Height=""*"" />
                        </Grid.RowDefinitions>

                        <TextBlock Text=""{Binding ConverterParameter=SearchToolName, 
                            Converter={StaticResource Localization}}""
                                    Foreground=""{StaticResource BackgroundTextColorBrush}""
                                    Margin=""7""
                                    FontSize=""14"" />

                        <Rectangle Grid.Row=""1""                                               
                                        Fill=""{StaticResource AccentColorGradientBrush}"" />
                    </Grid>";

            m_searchViewContainer = XamlReader.Load(containerXaml) as Grid;
        }

        // Attempts to add the search view to the application's side panel
        private bool tryAddSearchToSidePanel()
        {
            // try retrieving the side panel 
            DependencyObject d = MapApplication.Current.FindObjectInLayout(SIDE_PANEL_NAME);

            if (d != null && !(d is TabControl) && !(d is Accordion))
            {
                m_showInSidePanel = false; // Side panel is not tab control or accordion - show in window instead
            }
            else if (d != null)
            {
                SelectionChangedEventHandler selectionChanged = (o, e) =>
                {                    
                    // Set flag based on whether search is selected
                    if (((dynamic)o).SelectedItem == m_searchItem)
                        _isChecked = true;
                    else
                        _isChecked = false;

                    // Execute same logic as when button on toolbar is clicked
                    OnCanExecuteChanged();
                };

                if (d is TabControl)
                {
                    m_sidePanelIsTabControl = true;

                    TabControl sidePanelTabs = (TabControl)d;

                    // successfully retrieved side panel.  Create tab containing search view and
                    // add to the panel.
                    m_searchItem = new TabItem() { Content = m_searchViewContainer };
                    sidePanelTabs.Items.Add(m_searchItem);

                    // When the side panel tab is changed, toggle the search button on or off depending on
                    // whether search is currently shown
                    sidePanelTabs.SelectionChanged += selectionChanged;
                }
                else if (d is Accordion)
                {
                    Accordion sidePanelAccordion = (Accordion)d;

                    Style itemStyle = null;
                    if (sidePanelAccordion.Items.Count > 0 && sidePanelAccordion.Items[0] is AccordionItem)
                        itemStyle = ((AccordionItem)sidePanelAccordion.Items[0]).Style;

                    m_searchItem = new AccordionItem()
                    {
                        Content = m_searchView,
                        Header = Strings.Search,
                        Style = itemStyle,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        VerticalContentAlignment = VerticalAlignment.Stretch
                    };
                    sidePanelAccordion.Items.Add(m_searchItem);

                    sidePanelAccordion.SelectionChanged += selectionChanged;
                }

                // Associate the side panel container with the command
                Properties.SetAttachedObject(d, this);
                // Subscribe to changes in the side panel's visibility
                Properties.NotifyOnDependencyPropertyChanged("Visibility", d, OnSidePanelVisibilityChanged);

                m_addedToSidePanel = true;
            }

            return m_addedToSidePanel;
        }

        // Sets the minimum width of the grid column containing the search UI
        private void setColumnMinWidth()
        {
            DependencyObject dp = MapApplication.Current.FindObjectInLayout(SIDE_PANEL_NAME);
            int columnIndex = 0;
            
            // Find the first grid in the tree above the side panel that has multiple column definitions.
            // This must determine the width of the side panel's column.  Assume this also determines the 
            // width of the side panel
            if (dp == null)
                return;

            Stack<DependencyObject> parents = new Stack<DependencyObject>();
            while (!(dp is Grid) || ((Grid)dp).ColumnDefinitions.Count < 2)
            {
                dp = VisualTreeHelper.GetParent(dp);
                if (dp == null)
                    return;

                parents.Push(dp);
            }

            Grid layoutGrid = (Grid)dp;

            // find the parent closest to the grid with a non-zero column
            while (parents.Count > 0)
            {
                dp = parents.Pop();
                if (dp is FrameworkElement)
                {
                    columnIndex = Grid.GetColumn((FrameworkElement)dp);
                    if (columnIndex > 0)
                        break;
                }
            }

            if (m_searchView != null && m_searchView.ResultsTabControl != null)
                addColumnWidthBehavior(layoutGrid, columnIndex, m_searchView.ResultsTabControl);
        }

        private void addColumnWidthBehavior(Grid g, int columnIndex, TabControl tabControl)
        {
            BehaviorCollection behaviors = Interaction.GetBehaviors(g);
            foreach (Behavior b in behaviors)
            {
                if (b is AdjustColumnMinWidthToFitTabsBehavior)
                    return;
            }

            AdjustColumnMinWidthToFitTabsBehavior widthBehavior = new AdjustColumnMinWidthToFitTabsBehavior()
                {
                    Column = columnIndex,
                    TabControl = tabControl
                };

            behaviors.Add(widthBehavior);
        }

        // Quickly shows and hides the search view so the visual tree loads and the AdjustColumnMinWidthToFitTabsBehavior
        // initializes the minimum width of the column containing the side panel
        private void loadSearchVisualTree(bool keepVisible)
        {
            // Check whether the side panel is already visible.  If so, we want to set the tab back to the current one
            // once search has been shown
            ItemsControl sidePanel = MapApplication.Current.FindObjectInLayout(SIDE_PANEL_NAME) as ItemsControl;
            bool visible = sidePanel != null && sidePanel.Visibility == Visibility.Visible ? true : false;
            int selectedIndex = -1;
            if (visible)
                selectedIndex = ((dynamic)sidePanel).SelectedIndex;

            if (!keepVisible)
            {
                RoutedEventHandler loaded = null;
                // Loaded handler to revert side panel to state before showing search
                loaded = (o, e) =>
                {
                    // Unhook the handler
                    m_searchView.Loaded -= loaded;

                    if (visible) // Show the previously selected tab
                        ((dynamic)sidePanel).SelectedIndex = selectedIndex;
                    else // Execute the tool to hide the side panel
                        Execute(null);
                };
                m_searchView.Loaded += loaded;
            }

            // Execute the tool to show search in side panel and initiate a rendering pass
            Execute(null);
        }

        #endregion

        #region Configuration Wizard

        // Creates the configuration pages for the search tool
        private ObservableCollection<WizardPage> createConfigPages()
        {
            ObservableCollection<WizardPage> configPages = new ObservableCollection<WizardPage>();

            // Create the page for specifying which searches to include
            ProviderSelectionView providerView = new ProviderSelectionView()
            {
                Margin = new Thickness(10),
            };

            m_searchSelectionPage = new WizardPage()
            {
                Heading = Strings.SelectSearchTypes,
                Content = providerView
            };

            // Initialize configuration ViewModel if necessary
            if (m_configViewModel == null)
                initConfigViewModel();

            // Set page's data context to the ViewModel
            providerView.DataContext = m_configViewModel;

            m_searchSelectionPage.InputValid = m_configViewModel.SelectedSearchProviders.Count > 0;

            // Add the page
            configPages.Add(m_searchSelectionPage);

            // Incorporate wizard info from current search providers
            addNewWizardInfo(m_configViewModel.SelectedSearchProviders, configPages);

            return configPages;
        }

        // Initializes the view model used to represent tool configuration
        private void initConfigViewModel(string configData = null)
        {
            // Unhook event handlers if a config view model has already been initialized
            if (m_configViewModel != null)
                m_configViewModel.SelectedSearchProviders.CollectionChanged -= SelectedSearchProviders_CollectionChanged;

            if (configData == null)
                m_configViewModel = new SearchConfigViewModel(MapApplication.Current.Map);
            else
                m_configViewModel = new SearchConfigViewModel(MapApplication.Current.Map, configData);

            // Listen for search providers being added or removed
            m_configViewModel.SelectedSearchProviders.CollectionChanged += (o, e) =>
            {
                // Update validation state of the configuration page for selecting searches
                m_searchSelectionPage.InputValid = m_configViewModel.SelectedSearchProviders.Count > 0;

                // Update configuration wizard to reflect added and removed searches
                if (e.NewItems != null)
                    addNewWizardInfo(e.NewItems, Pages);

                if (e.OldItems != null)
                    removeWizardPages(e.OldItems, Pages);
            };

            if (m_searchSelectionPage != null && m_searchSelectionPage.Content is FrameworkElement)
                ((FrameworkElement)m_searchSelectionPage.Content).DataContext = m_configViewModel;
        }

        // Initializes the view model that backs the search UI
        private void initSearchViewModel()
        {
            m_searchViewModel = new SearchViewModel();

            // Add the configured search providers
            foreach (ISearchProvider search in m_configViewModel.SelectedSearchProviders)
                m_searchViewModel.SearchProviders.Add(search);

            // Listen for changes in the set of search providers
            m_configViewModel.SelectedSearchProviders.CollectionChanged +=
                SelectedSearchProviders_CollectionChanged;

            // Null out data context
            if (m_searchView.DataContext != null)
                m_searchView.DataContext = null;

            // Apply ViewModel to the search view
            m_searchView.DataContext = m_searchViewModel;
        }

        // Updates the search tool's configuration wizard to incorporate the configuration pages
        // and desired size of the passed-in search providers
        private void addNewWizardInfo(IEnumerable providers, ObservableCollection<WizardPage> pages)
        {
            foreach (ISearchProvider provider in providers)
            {
                ISupportsWizardConfiguration wizardInfo = provider as ISupportsWizardConfiguration;
                if (wizardInfo != null)
                {
                    foreach (WizardPage page in wizardInfo.Pages)
                        pages.Add(page);

                    if (wizardInfo.DesiredSize.Width > desiredSize.Width)
                        desiredSize.Width = wizardInfo.DesiredSize.Width;

                    if (wizardInfo.DesiredSize.Height > desiredSize.Height)
                        desiredSize.Height = wizardInfo.DesiredSize.Height;
                }
            }
        }

        // Removes the wizard pages of the passed-in search providers from the search tool's configuration wizard
        private void removeWizardPages(IEnumerable providers, ObservableCollection<WizardPage> pages)
        {
            foreach (ISearchProvider provider in providers)
            {
                ISupportsWizardConfiguration wizardInfo = provider as ISupportsWizardConfiguration;
                if (wizardInfo != null)
                {
                    foreach (WizardPage page in wizardInfo.Pages)
                    {
                        if (pages.Contains(page))
                            pages.Remove(page);
                    }
                }
            }
        }

        #endregion

        #endregion

    }
}
