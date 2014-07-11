/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using ESRI.ArcGIS.Mapping.Controls;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
    /// <summary>
    /// Specifies the type of search.
    /// </summary>
    public enum SearchType
    {
        Maps,
        Groups
    }

    /// <summary>
    /// Implements the panel in the BackStage that is used to search for maps.
    /// </summary>
    public partial class SearchMapsControl : Control
    {
        bool _initialized = false;
        string _sortOption = "";

        /// <summary>
        /// Creates the OpenMapControl.
        /// </summary>
        public SearchMapsControl()
        {
            DefaultStyleKey = typeof(SearchMapsControl);
            OpenCommand = new DelegateCommand(open);
            OwnerClickCommand = new DelegateCommand(ownerClick);
            MoreDetailsCommand = new DelegateCommand(moreDetails);
            OpenGroupCommand = new DelegateCommand(openGroup);
            GroupOwnerCommand = new DelegateCommand(groupOwnerOpen);
        }

        #region Commands
        public ICommand OpenCommand { get; set; }
        public ICommand OwnerClickCommand { get; set; }
        public ICommand MoreDetailsCommand { get; set; }
        public ICommand OpenGroupCommand { get; set; }
        public ICommand GroupOwnerCommand { get; set; }
        #endregion

        public object Items
        {
            get { return (object)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(object), typeof(SearchMapsControl), null);

        public event EventHandler<ContentItemEventArgs> MapSelectedForOpening;

        void RaiseMapSelectedForOpening(object sender, ContentItemEventArgs e)
        {
            if (MapSelectedForOpening != null)
                MapSelectedForOpening(this, e);
        }

        MapDetailsControl MapDetailsControl;
        TextBox SearchTextBox;
        RadioButton SearchMapsButton;
        Button SearchButton;
        ProgressIndicator ProgressIndicator;
        DataPager DataPager;
        ListBox GroupResultsListBox;
        TextBlock SearchResultsTextBlock;
        ListBox MapResultsListBox;
        Canvas SearchResultsHeaderCanvas;
        ToggleButton SortByToggleButton;
        Popup SortByMenuPopup;
        Border SortByMenuBorder;
        StackPanel SortByToggleButtonStackPanel;
        RadioButton SearchGroupsButton;
        ToggleButton MostRelevant;
        ToggleButton MostPopular;
        ToggleButton HighestRated;
        ToggleButton MostRecentlyAdded;
        ToggleButton TitleAtoZ;
        ToggleButton TitleZtoA;
        ToggleButton MostComments;

        public override void OnApplyTemplate()
        {
            if (MapDetailsControl != null)
                MapDetailsControl.MapSelectedForOpening -= RaiseMapSelectedForOpening;
            if (SearchTextBox != null)
                SearchTextBox.KeyDown -= SearchTextBox_KeyDown;
            if (SearchButton != null)
                SearchButton.Click -= SearchButton_Click;
            if (MapResultsListBox != null)
                MapResultsListBox.SelectionChanged -= ResultListBox_SelectionChanged;
            if (SearchMapsButton != null)
                SearchMapsButton.Click -= SearchMapsButton_Click;
            if (SearchGroupsButton != null)
                SearchGroupsButton.Click -= SearchGroupsButton_Click;
            if (MostRelevant != null)
                MostRelevant.Click -= SortByMenuToggleButton_Click;
            if (MostPopular != null)
                MostPopular.Click -= SortByMenuToggleButton_Click;
            if (HighestRated != null)
                HighestRated.Click -= SortByMenuToggleButton_Click;
            if (MostRecentlyAdded != null)
                MostRecentlyAdded.Click -= SortByMenuToggleButton_Click;
            if (TitleAtoZ != null)
                TitleAtoZ.Click -= SortByMenuToggleButton_Click;
            if (TitleZtoA != null)
                TitleZtoA.Click -= SortByMenuToggleButton_Click;
            if (MostComments != null)
                MostComments.Click -= SortByMenuToggleButton_Click;
            if (SortByToggleButton != null)
                SortByToggleButton.Click -= SortByToggleButton_Click;
            if (SortByMenuPopup != null)
                SortByMenuPopup.Closed -= SortByMenuPopup_Closed;
            if (SortByToggleButtonStackPanel != null)
                SortByToggleButtonStackPanel.LostFocus -= SortByToggleButtonStackPanel_LostFocus;

            base.OnApplyTemplate();

            MapDetailsControl = GetTemplateChild("MapDetailsControl") as MapDetailsControl;
            SearchTextBox = GetTemplateChild("SearchTextBox") as TextBox;
            SearchMapsButton = GetTemplateChild("SearchMapsButton") as RadioButton;
            SearchButton = GetTemplateChild("SearchButton") as Button;
            ProgressIndicator = GetTemplateChild("ProgressIndicator") as ProgressIndicator;
            DataPager = GetTemplateChild("DataPager") as DataPager;
            GroupResultsListBox = GetTemplateChild("GroupResultsListBox") as ListBox;
            SearchResultsTextBlock = GetTemplateChild("SearchResultsTextBlock") as TextBlock;
            MapResultsListBox = GetTemplateChild("MapResultsListBox") as ListBox;
            SearchResultsHeaderCanvas = GetTemplateChild("SearchResultsHeaderCanvas") as Canvas;
            SortByToggleButton = GetTemplateChild("SortByToggleButton") as ToggleButton;
            SortByMenuPopup = GetTemplateChild("SortByMenuPopup") as Popup;
            SortByMenuBorder = GetTemplateChild("SortByMenuBorder") as Border;
            SortByToggleButtonStackPanel = GetTemplateChild("SortByToggleButtonStackPanel") as StackPanel;
            SearchGroupsButton = GetTemplateChild("SearchGroupsButton") as RadioButton;
            MostRelevant = GetTemplateChild("MostRelevant") as ToggleButton;
            MostPopular = GetTemplateChild("MostPopular") as ToggleButton;
            HighestRated = GetTemplateChild("HighestRated") as ToggleButton;
            MostRecentlyAdded = GetTemplateChild("MostRecentlyAdded") as ToggleButton;
            TitleAtoZ = GetTemplateChild("TitleAtoZ") as ToggleButton;
            TitleZtoA = GetTemplateChild("TitleZtoA") as ToggleButton;
            MostComments = GetTemplateChild("MostComments") as ToggleButton;

            if (MapDetailsControl != null)
                MapDetailsControl.MapSelectedForOpening += RaiseMapSelectedForOpening;
            if (SearchTextBox != null)
            {
                SearchTextBox.KeyDown += SearchTextBox_KeyDown;
                SearchTextBox.Focus();
            }
            if (SearchButton != null)
                SearchButton.Click += SearchButton_Click;
            if (MapResultsListBox != null)
            {
                MapResultsListBox.SelectionChanged += ResultListBox_SelectionChanged;
                MapResultsListBox.DataContext = this;
            }
            if (GroupResultsListBox != null)
                GroupResultsListBox.DataContext = this;
            if (SearchMapsButton != null)
                SearchMapsButton.Click += SearchMapsButton_Click;
            if (SearchGroupsButton != null)
                SearchGroupsButton.Click += SearchGroupsButton_Click;
            if (MostRelevant != null)
                MostRelevant.Click += SortByMenuToggleButton_Click;
            if (MostPopular != null)
                MostPopular.Click += SortByMenuToggleButton_Click;
            if (HighestRated != null)
                HighestRated.Click += SortByMenuToggleButton_Click;
            if (MostRecentlyAdded != null)
                MostRecentlyAdded.Click += SortByMenuToggleButton_Click;
            if (TitleAtoZ != null)
                TitleAtoZ.Click += SortByMenuToggleButton_Click;
            if (TitleZtoA != null)
                TitleZtoA.Click += SortByMenuToggleButton_Click;
            if (MostComments != null)
                MostComments.Click += SortByMenuToggleButton_Click;
            if (SortByToggleButton != null)
                SortByToggleButton.Click += SortByToggleButton_Click;
            if (SortByMenuPopup != null)
                SortByMenuPopup.Closed += SortByMenuPopup_Closed;
            if (SortByToggleButtonStackPanel != null)
                SortByToggleButtonStackPanel.LostFocus += SortByToggleButtonStackPanel_LostFocus;
            if (pendingSearch != null && SearchTextBox != null && SearchMapsButton != null)
            {
                DoSearch(pendingSearch.Term, pendingSearch.Type);
                pendingSearch = null;
            }
        }

        #region Helpers
        class PendingSearch
        {
            public string Term { get; set; }
            public SearchType Type { get; set; }
        }
        PendingSearch pendingSearch;
        /// <summary>
        /// Initiates a search based on the specified search term.
        /// </summary>
        /// <param name="searchTerm"></param>
        public void DoSearch(string searchTerm, SearchType searchType)
        {
            if (SearchTextBox == null)
            {
                pendingSearch = new PendingSearch() { Term = searchTerm, Type = searchType };
                return;
            }
            SearchTextBox.Text = searchTerm;

            if (searchType == SearchType.Maps)
                SearchMapsButton.IsChecked = true;
            else
                SearchGroupsButton.IsChecked = true;

            DoSearch();
        }

        /// <summary>
        /// Raised by the ContentService when the user signs in or out.
        /// </summary>
        void ContentService_SignedInOut(object sender, EventArgs e)
        {
            if (SearchTextBox.Text.Length > 0)
                GenerateResults(true);
        }

        /// <summary>
        /// Raised when the asynchronous call to search using the ContentService has completed.
        /// </summary>
        void SearchCompleted(object sender, ContentSearchEventArgs e)
        {
            SearchButton.IsEnabled = true;
            SearchTextBox.IsEnabled = true;
            ProgressIndicator.Visibility = Visibility.Collapsed;

            if (e.Error != null)
                return;

            Items = new PagedSearchResult(e.Result);
            DataPager.Visibility = DataPager.PageCount > 1 ? Visibility.Visible : Visibility.Collapsed;
            MapResultsListBox.Visibility = e.Result.TotalCount > 0 ? Visibility.Visible : Visibility.Collapsed;
            SortByToggleButton.Visibility = e.Result.TotalCount > 0 ? Visibility.Visible : Visibility.Collapsed;

            SearchResultsTextBlock.Text = string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.SearchMapResultsFor, e.Result.TotalCount.ToString(), SearchTextBox.Text);
            SearchTextBox.SelectAll();
            SearchTextBox.Focus();

            //select first item in list and show/hide map details depending on 
            //if it was previously visible
            //
            if (e.Result.Items.Length > 0)
            {
                MapResultsListBox.SelectedItem = e.Result.Items[0];
                MapResultsListBox.ScrollIntoView(e.Result.Items[0]);
                if (MapDetailsControl.Visibility == Visibility.Visible)
                    MapDetailsControl.Activate(e.Result.Items[0]);
            }
            else
                MapDetailsControl.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Raised when the asynchronous call to search for Groups on AGOL has completed.
        /// </summary>
        void GroupsSearchCompleted(object sender, GroupSearchEventArgs e)
        {
            SearchButton.IsEnabled = true;
            SearchTextBox.IsEnabled = true;
            ProgressIndicator.Visibility = Visibility.Collapsed;

            if (e.Error != null)
                return;

            Items = new PagedSearchResult(e.Result);
            DataPager.Visibility = DataPager.PageCount > 1 ? Visibility.Visible : Visibility.Collapsed;
            GroupResultsListBox.Visibility = e.Result.TotalCount > 0 ? Visibility.Visible : Visibility.Collapsed;

            SearchResultsTextBlock.Text = string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.SearchMapResultsFor, e.Result.TotalCount.ToString(), SearchTextBox.Text);
            SearchTextBox.SelectAll();
            SearchTextBox.Focus();

            //select first item in list and show/hide map details depending on 
            //if it was previously visible
            //
            if (e.Result.Items.Length > 0)
            {
                GroupResultsListBox.SelectedItem = e.Result.Items[0];
                GroupResultsListBox.ScrollIntoView(e.Result.Items[0]);
            }
        }

        /// <summary>	
        /// Peforms the previous search again.
        /// </summary>
        private void GenerateResults(bool doSearch)
        {
            DataPager.Visibility = Visibility.Collapsed;
            Items = null;
            SearchResultsTextBlock.Text = "";

            if (doSearch)
                DoSearch();
        }
        #endregion

        #region Commands
        #region Open Command
        private void open(object commandParameter)
        {
            ListBoxItem lbi = Util.GetParentOfType<ListBoxItem>(commandParameter as DependencyObject);
            if (lbi != null)
            {
                lbi.IsSelected = true;
                RaiseMapSelectedForOpening(this, new ContentItemEventArgs() { Item = MapResultsListBox.SelectedItem as ContentItem });
            }
        }
        #endregion
        #endregion


        #region Event handlers
        /// <summary>
        /// Checks if the Enter key is pressed and initiates a search.
        /// </summary>
        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && SearchButton.IsEnabled)
                DoSearch();
        }

        /// <summary>
        /// Occurs when the Search button is clicked.
        /// </summary>
        internal void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            DoSearch();
        }

        /// <summary>
        /// Performs a search for maps or groups asynchronously.
        /// </summary>
        private void DoSearch()
        {
            SearchResultsHeaderCanvas.Visibility = Visibility.Visible;
            SearchResultsTextBlock.Text = "";

            if (SearchMapsButton.IsChecked == true)
            {
                showMapResults();

                // search for maps
                string searchString = SearchTextBox.Text.Length > 0 ? SearchTextBox.Text + " AND " + ArcGISOnlineEnvironment.WebMapTypeQualifier : ArcGISOnlineEnvironment.WebMapTypeQualifier;
                ArcGISOnlineEnvironment.ArcGISOnline.Content.Search(searchString, _sortOption, SearchCompleted);
            }
            else //search for groups
            {
                showGroupResults();

                if (SearchTextBox.Text.Length > 0)
                    ArcGISOnlineEnvironment.ArcGISOnline.Group.Search(SearchTextBox.Text, GroupsSearchCompleted);
                else
                    return;
            }

            SearchTextBox.IsEnabled = false;
            SearchButton.IsEnabled = false;
            ProgressIndicator.Visibility = Visibility.Visible;
        }

        internal void Clear()
        {
            _initialized = false;
            if (SearchTextBox != null)
                SearchTextBox.Text = "";
            Items = null;
            if (DataPager != null)
                DataPager.Visibility = Visibility.Collapsed;
            if (MapResultsListBox != null)
                MapResultsListBox.Visibility = Visibility.Collapsed;
            if (SortByToggleButton != null)
                SortByToggleButton.Visibility = Visibility.Collapsed;
            if (SearchResultsTextBlock != null)
                SearchResultsTextBlock.Text = "";
            if (MapDetailsControl != null)
                MapDetailsControl.Visibility = Visibility.Collapsed;
            if (GroupResultsListBox != null)
                GroupResultsListBox.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Activates the panel, initializing the selection if necessary.
        /// </summary>
        internal void Activate()
        {
            if (!_initialized)
            {
                _initialized = true;

                ArcGISOnlineEnvironment.ArcGISOnline.User.SignedInOut += ContentService_SignedInOut;
            }
            if (SearchTextBox != null)
                SearchTextBox.Focus();
        }

        #region OwnerClick Command
        private void ownerClick(object commandParameter)
        {
            string owner = null;
            if (commandParameter is HyperlinkButton)
                owner = ((HyperlinkButton)commandParameter).Content.ToString();
            else if (commandParameter is string)
                owner = (string)commandParameter;
            else
                return;

            BackStageControl backStage = Util.GetParentOfType<BackStageControl>(this);
            // DO NOT LOCALIZE "owner" - THIS IS A QUERY PARAMETER
            backStage.DoSearch(string.Format("owner: {0}", owner), SearchType.Maps);
        }
        #endregion

        #region MoreDetailsCommand
        private void moreDetails(object commandParameter)
        {
            if (commandParameter is HyperlinkButton)
            {
                HyperlinkButton button = (HyperlinkButton)commandParameter;
                ListBoxItem lbi = Util.GetParentOfType<ListBoxItem>(button);

                MapResultsListBox.SelectedItem = lbi.DataContext;
                MapDetailsControl.Visibility = Visibility.Visible;
                MapDetailsControl.Activate((ContentItem)lbi.DataContext);
            }
        }
        #endregion

        /// <summary>
        /// Occurs when the selection in the ResultListBox changes.
        /// </summary>
        private void ResultListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //update the item displayed in the detailed MapDetailsControl
            if (MapResultsListBox.SelectedItem != null)
                MapDetailsControl.Activate((ContentItem)MapResultsListBox.SelectedItem);
        }

        /// <summary>
        /// Occurs when the Maps hyperlink button is clicked.
        /// </summary>
        internal void SearchMapsButton_Click(object sender, RoutedEventArgs e)
        {
            showMapResults();

            Items = null;
            if (SearchTextBox.Text.Length > 0)
                DoSearch();
        }

        /// <summary>
        /// Occurs when the Groups hyperlink button is clicked.
        /// </summary>
        internal void SearchGroupsButton_Click(object sender, RoutedEventArgs e)
        {
            showGroupResults();

            Items = null;
            if (SearchTextBox.Text.Length > 0)
                DoSearch();
        }

        #region OpenGroupCommand
        private void openGroup(object commandParameter)
        {
            if (commandParameter is DependencyObject)
            {
                ListBoxItem lbi = Util.GetParentOfType<ListBoxItem>((DependencyObject)commandParameter);

                BackStageControl backStage = Util.GetParentOfType<BackStageControl>(this);
                backStage.AddGroup((Group)lbi.DataContext);

            }
        }
        #endregion

        #region GroupOwnerCommand
        private void groupOwnerOpen(object commandParameter)
        {
            string owner = null;
            if (commandParameter is ContentControl)
                owner = ((ContentControl)commandParameter).Content.ToString();
            else if (commandParameter is string)
                owner = (string)commandParameter;
            else
                return;

            BackStageControl backStage = Util.GetParentOfType<BackStageControl>(this);
            // DO NOT LOCALIZE "owner" - THIS IS A QUERY PARAMETER
            backStage.DoSearch(string.Format("owner: {0}", owner), SearchType.Groups);
        }
        #endregion
        /// <summary>
        /// Occurs when one of the the Sort By toggle buttons is clicked - makes it the only checked
        /// sort button, sets the sortOption to its tag and re-runs the search
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortByMenuToggleButton_Click(object sender, RoutedEventArgs e)
        {
            SortByMenuPopup.IsOpen = false;

            foreach (UIElement uiElement in SortByToggleButtonStackPanel.Children)
                if (uiElement is ToggleButton)
                    ((ToggleButton)uiElement).IsChecked = false;

            ((ToggleButton)sender).IsChecked = true;
            _sortOption = ((ToggleButton)sender).Tag.ToString();
            DoSearch();
        }

        /// <summary>
        /// Occurs when the the Sort By toggle button is clicked - shows the sortby popup menu
        /// </summary>
        private void SortByToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (SortByMenuPopup.IsOpen)
                SortByMenuPopup.IsOpen = false;
            else
            {
                // calculate the location of the popup and open it
                ToggleButton toggleButton = sender as ToggleButton;

                GeneralTransform gt = toggleButton.TransformToVisual(this);
                Point offset = gt.Transform(new Point(0, 0));

                SortByMenuPopup.VerticalOffset = offset.Y + toggleButton.ActualHeight;
                SortByMenuBorder.Opacity = 0.01;
                SortByMenuPopup.IsOpen = true;

                // give the first toggle button focus so the SortByToggleButtonStackPanel can lose focus
                ((ToggleButton)SortByToggleButtonStackPanel.Children[0]).Focus();

                Dispatcher.BeginInvoke(() =>
                {
                    SortByMenuPopup.HorizontalOffset = offset.X - SortByMenuBorder.ActualWidth + toggleButton.ActualWidth;
                    SortByMenuBorder.Opacity = 1;
                });

            }
        }

        /// <summary>
        /// Occurs when the sort by popup menu is closed - ensure the sort by toggle button is not checked
        /// </summary>
        private void SortByMenuPopup_Closed(object sender, EventArgs e)
        {
            SortByToggleButton.IsChecked = false;
        }

        /// <summary>
        /// If the popup loses focus, then close it if its not the sort by button itself or any of the sort option buttons
        /// </summary>
        private void SortByToggleButtonStackPanel_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((System.Windows.Input.FocusManager.GetFocusedElement() == SortByToggleButton))
                return;

            if ((System.Windows.Input.FocusManager.GetFocusedElement() is ToggleButton))
            {
                if (((ToggleButton)System.Windows.Input.FocusManager.GetFocusedElement()).Parent != SortByToggleButtonStackPanel)
                    SortByMenuPopup.IsOpen = false;
            }
            else
                SortByMenuPopup.IsOpen = false;
        }
        #endregion

        private void showMapResults()
        {
            MapResultsListBox.Visibility = Visibility.Visible;
            GroupResultsListBox.Visibility = Visibility.Collapsed;
        }

        private void showGroupResults()
        {
            MapResultsListBox.Visibility = Visibility.Collapsed;
            SortByToggleButton.Visibility = Visibility.Collapsed;
            GroupResultsListBox.Visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// Converts tab indices to Visibility values.
    /// </summary>
    public class TabToVisibilityConverter : IValueConverter
    {
        public TabToVisibilityConverter()
        {
        }

        #region IValueConverter Members

        /// <summary>
        /// For My Maps tab returns Visible otherwise Collapsed.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int tabIndex = int.Parse(parameter as string);
            int selectedTabIndex = (int)value;
            if (tabIndex == selectedTabIndex)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

