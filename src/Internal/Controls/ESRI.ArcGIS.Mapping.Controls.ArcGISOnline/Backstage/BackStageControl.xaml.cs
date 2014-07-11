/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ESRI.ArcGIS.Mapping.Controls;
using System.Windows.Shapes;
using System.Windows.Input;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
    /// <summary>
    /// Implements the BackStage panel. The term BackStage comes from Office 2010 and is
    /// the replacement for the application menu. The 520 backstage contains the controls
    /// that let the user manage map documents stored in AGOL.
    /// </summary>
    public partial class BackStageControl : Control
    {
        bool _initialized = false;
        FrameworkElement _currentPanel; //the panel that is currently selected

        public BackStageControl()
        {
            DefaultStyleKey = typeof(BackStageControl);
            GroupCommand = new DelegateCommand(GroupRadioButton_Click);
            if (!ArcGISOnlineEnvironment.LoadedConfig)
                ArcGISOnlineEnvironment.LoadConfig((a, b) =>
                    {
                        if (pendingActivate)
                            Activate();
                    }
                    );
        }

        public ICommand GroupCommand { get; set; }

        ListBox GroupsListBox;
        GroupControl GroupControl;
        FeaturedMapsControl FeaturedMapsControl;
        MyMapsControl MyMapsControl;
        SearchMapsControl SearchMapsControl;
        RecentMapsControl RecentMapsControl;
        MyGroupsControl MyGroupsControl;
        RadioButton FeaturedMapsRadioButton;
        RadioButton MyGroupsRadioButton;
        RadioButton MyOwnGroupsRadioButton;
        RadioButton MyOrgGroupsRadioButton;
        RadioButton MyOrgPublicGroupsRadioButton;
        RadioButton MyMapsRadioButton;
        RadioButton SearchRadioButton;
        RadioButton RecentRadioButton;
        FrameworkElement RadioButtonSeparator;
        Panel GroupsPanel;

        public override void OnApplyTemplate()
        {
            if (GroupControl != null)
            {
                GroupControl.GroupDetailsChanged -= GroupDetailsChanged;
                GroupControl.MapSelectedForOpening -= RaiseMapSelectedForOpening;
            }
            if (FeaturedMapsControl != null)
                FeaturedMapsControl.MapSelectedForOpening -= RaiseMapSelectedForOpening;
            if (MyMapsControl != null)
                MyMapsControl.MapSelectedForOpening -= RaiseMapSelectedForOpening;
            if (SearchMapsControl != null)
                SearchMapsControl.MapSelectedForOpening -= RaiseMapSelectedForOpening;
            if (RecentMapsControl != null)
                RecentMapsControl.MapSelectedForOpening -= RaiseMapSelectedForOpening;
            if (RecentRadioButton != null)
                RecentRadioButton.Click -= RecentRadioButton_Click;
            if (MyMapsRadioButton != null)
                MyMapsRadioButton.Click -= MyMapsRadioButton_Click;
            if (MyGroupsRadioButton != null)
                MyGroupsRadioButton.Click -= MyGroupsRadioButton_Click;
            if (MyOrgGroupsRadioButton != null)
                MyOrgGroupsRadioButton.Click -= MyOrgGroupsRadioButton_Click;
            if (MyOrgPublicGroupsRadioButton != null)
                MyOrgPublicGroupsRadioButton.Click -= MyOrgPublicGroupsRadioButton_Click;
            if (MyOwnGroupsRadioButton != null)
                MyOwnGroupsRadioButton.Click -= MyOwnGroupsRadioButton_Click;
            if (FeaturedMapsRadioButton != null)
                FeaturedMapsRadioButton.Click -= FeaturedMapsRadioButton_Click;
            if (SearchRadioButton != null)
                SearchRadioButton.Click -= SearchRadioButton_Click;

            base.OnApplyTemplate();
            GroupsListBox = GetTemplateChild("GroupsListBox") as ListBox;
            GroupControl = GetTemplateChild("GroupControl") as GroupControl;
            FeaturedMapsControl = GetTemplateChild("FeaturedMapsControl") as FeaturedMapsControl;
            MyMapsControl = GetTemplateChild("MyMapsControl") as MyMapsControl;
            SearchMapsControl = GetTemplateChild("SearchMapsControl") as SearchMapsControl;
            RecentMapsControl = GetTemplateChild("RecentMapsControl") as RecentMapsControl;
            MyGroupsControl = GetTemplateChild("MyGroupsControl") as MyGroupsControl;
            FeaturedMapsRadioButton = GetTemplateChild("FeaturedMapsRadioButton") as RadioButton;
            MyGroupsRadioButton = GetTemplateChild("MyGroupsRadioButton") as RadioButton;
            MyOrgGroupsRadioButton = GetTemplateChild("MyOrgGroupsRadioButton") as RadioButton;
            MyOrgPublicGroupsRadioButton = GetTemplateChild("MyOrgPublicGroupsRadioButton") as RadioButton;
            MyOwnGroupsRadioButton = GetTemplateChild("MyOwnGroupsRadioButton") as RadioButton;
            MyMapsRadioButton = GetTemplateChild("MyMapsRadioButton") as RadioButton;
            SearchRadioButton = GetTemplateChild("SearchRadioButton") as RadioButton;
            RadioButtonSeparator = GetTemplateChild("RadioButtonSeparator") as FrameworkElement;
            RecentRadioButton = GetTemplateChild("RecentRadioButton") as RadioButton;
            GroupsPanel = GetTemplateChild("GroupsPanel") as Panel;
            if (GroupsListBox != null)
            {
                GroupsListBox.ItemsSource = new ObservableCollection<GroupListBindingWrapper<Group>>();
                GroupsListBox.Tag = this;
            }
            if (GroupControl != null)
            {
                GroupControl.GroupDetailsChanged += GroupDetailsChanged;
                GroupControl.MapSelectedForOpening += RaiseMapSelectedForOpening;
            }
            if (FeaturedMapsControl != null)
                FeaturedMapsControl.MapSelectedForOpening += RaiseMapSelectedForOpening;
            if (MyMapsControl != null)
                MyMapsControl.MapSelectedForOpening += RaiseMapSelectedForOpening;
            if (SearchMapsControl != null)
                SearchMapsControl.MapSelectedForOpening += RaiseMapSelectedForOpening;
            if (RecentMapsControl != null)
                RecentMapsControl.MapSelectedForOpening += RaiseMapSelectedForOpening;
            if (RecentRadioButton != null)
                RecentRadioButton.Click += RecentRadioButton_Click;
            if (MyMapsRadioButton != null)
                MyMapsRadioButton.Click += MyMapsRadioButton_Click;
            if (MyGroupsRadioButton != null)
                MyGroupsRadioButton.Click += MyGroupsRadioButton_Click;
            if (MyOrgGroupsRadioButton != null)
                MyOrgGroupsRadioButton.Click += MyOrgGroupsRadioButton_Click;
            if (MyOrgPublicGroupsRadioButton != null)
                MyOrgPublicGroupsRadioButton.Click += MyOrgPublicGroupsRadioButton_Click;
            if (MyOwnGroupsRadioButton != null)
                MyOwnGroupsRadioButton.Click += MyOwnGroupsRadioButton_Click;
            if (FeaturedMapsRadioButton != null)
                FeaturedMapsRadioButton.Click += FeaturedMapsRadioButton_Click;
            if (SearchRadioButton != null)
                SearchRadioButton.Click += SearchRadioButton_Click;
            if (pendingActivate)
                Activate();
        }

        public event EventHandler<MapDocumentEventArgs> MapSelectedForOpening;

        void RaiseMapSelectedForOpening(object sender, ContentItemEventArgs e)
        {
            Cursor = Cursors.Wait;
            if (e != null && e.Item != null && !string.IsNullOrEmpty(e.Item.Id))
            {
                if (MapSelectedForOpening != null)
                {
                    string serverBaseUrl = System.IO.Path.Combine(ArcGISOnlineEnvironment.ConfigurationUrls.Sharing, "content");
                    MapSelectedForOpening(this, new MapDocumentEventArgs()
                    {
                        DocumentID = e.Item.Id,
                        Document = new ESRI.ArcGIS.Client.WebMap.Document()
                        {
                            ServerBaseUrl = serverBaseUrl,
                            BingToken = ArcGISOnlineEnvironment.BingToken,
                            Token = ArcGISOnlineEnvironment.ArcGISOnline.User.Token,
                            ProxyUrl = (string.IsNullOrWhiteSpace(ArcGISOnlineEnvironment.ConfigurationUrls.ProxyServerEncoded) ? 
                                                null : ArcGISOnlineEnvironment.ConfigurationUrls.ProxyServerEncoded),
                            GeometryServiceUrl = ArcGISOnlineEnvironment.ConfigurationUrls.GeometryServer
                        }
                    });
                }
                //cache map as most recently used map
                MRUMaps.Add(e.Item);
            }
            Cursor = Cursors.Arrow;
        }

        void ArcGISOnline_SignedInOut(object sender, EventArgs e)
        {
            if (MyMapsControl.Visibility == Visibility.Visible ||
                MyGroupsControl.Visibility == Visibility.Visible)
            {
                FeaturedMapsRadioButton.IsChecked = true;
                ShowPanel(FeaturedMapsControl);
            }

            MyMapsRadioButton.Visibility = ArcGISOnlineEnvironment.ArcGISOnline.User.IsSignedIn ? Visibility.Visible : Visibility.Collapsed;
            MyGroupsRadioButton.Visibility = ArcGISOnlineEnvironment.ArcGISOnline.User.IsSignedIn ? Visibility.Visible : Visibility.Collapsed;
            User user = ArcGISOnlineEnvironment.ArcGISOnline.User.Current;
            MyOrgGroupsRadioButton.Visibility = MyOrgPublicGroupsRadioButton.Visibility = (user != null && !string.IsNullOrEmpty(user.AccountId)) ?
                Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Activates the OpenMapControl and initiates a search based on the specified search term.
        /// </summary>
        /// <param name="searchTerm"></param>
        public void DoSearch(string searchTerm, SearchType searchType)
        {
            SearchRadioButton.IsChecked = true;
            ShowPanel(SearchMapsControl);
            SearchMapsControl.Activate();
            SearchMapsControl.DoSearch(searchTerm, searchType);
        }
        bool pendingActivate;
        /// <summary>
        /// Activates the BackStage panel, initializing the selection/activity.
        /// </summary>
        public void Activate(bool force = false)
        {
            if (force)
                clear();

            if (FeaturedMapsRadioButton == null || !ArcGISOnlineEnvironment.LoadedConfig)
            {
                pendingActivate = true;
                return;
            }
            pendingActivate = false;
            if (_currentPanel == null || force)
            {
                //if no other panel has been selected so far show Featured Maps
                FeaturedMapsRadioButton.IsChecked = true;
                ShowPanel(FeaturedMapsControl);
                FeaturedMapsControl.Activate();
            }

            if (!_initialized)
            {
                _initialized = true;
                ArcGISOnlineEnvironment.ArcGISOnline.User.SignedInOut += ArcGISOnline_SignedInOut;

                // simulate the sign in
                ArcGISOnline_SignedInOut(null, EventArgs.Empty);
            }
        }

        void clear()
        {
            _initialized = false;
            if (MyGroupsControl != null)
                MyGroupsControl.Clear();
            if (FeaturedMapsControl != null)
                FeaturedMapsControl.Clear();
            if (MyMapsControl != null)
                MyMapsControl.Clear();
            if (SearchMapsControl != null)
                SearchMapsControl.Clear();
            if (RecentMapsControl != null)
                RecentMapsControl.Clear();
        }

        /// <summary>
        /// Raised when the details of a group have changed.
        /// </summary>
        void GroupDetailsChanged(object sender, GroupEventArgs e)
        {
            //if the group that was changed is in the GroupsListBox update
            //the corresponding listbox item
            if (GroupsListBox.ItemsSource != null)
            {
                foreach (object obj in GroupsListBox.ItemsSource)
                {
                    GroupListBindingWrapper<Group> wrapper = (GroupListBindingWrapper<Group>)obj;
                    if (wrapper.Content.Id == e.Group.Id)
                    {
                        wrapper.Content = e.Group;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Shows the specified panel, hides the rest.
        /// </summary>
        /// <param name="userControl"></param>
        void ShowPanel(FrameworkElement panel)
        {
            MyMapsControl.Visibility = (panel == MyMapsControl) ? Visibility.Visible : Visibility.Collapsed;
            GroupsPanel.Visibility = Visibility.Collapsed;
            if (panel == MyGroupsControl)
            {
                GroupControl.Visibility = System.Windows.Visibility.Collapsed;
                GroupsPanel.Visibility = System.Windows.Visibility.Visible;
                MyGroupsControl.Visibility = Visibility.Visible;
            }
            FeaturedMapsControl.Visibility = (panel == FeaturedMapsControl) ? Visibility.Visible : Visibility.Collapsed;
            SearchMapsControl.Visibility = (panel == SearchMapsControl) ? Visibility.Visible : Visibility.Collapsed;
            RecentMapsControl.Visibility = (panel == RecentMapsControl) ? Visibility.Visible : Visibility.Collapsed;
            if (panel == GroupControl)
            {
                GroupControl.Visibility = System.Windows.Visibility.Visible;
                GroupsPanel.Visibility = System.Windows.Visibility.Visible;
                MyGroupsControl.Visibility = Visibility.Collapsed;
                MyGroupsControl.TypeOfGroups = ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.MyGroupsControl.GroupType.OneGroup;
            }

            _currentPanel = panel;
        }

        /// <summary>
        /// Shows the MyMaps panel.
        /// </summary>
        internal void MyMapsRadioButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(MyMapsControl);
            MyMapsControl.Activate();
        }

        /// <summary>
        /// Occurs when the Recent button has been clicked.
        /// </summary>
        internal void RecentRadioButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(RecentMapsControl);
            RecentMapsControl.Activate();
        }

        internal void SearchRadioButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(SearchMapsControl);
            SearchMapsControl.Activate();
        }

        internal void FeaturedMapsRadioButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(FeaturedMapsControl);
            FeaturedMapsControl.Activate();
        }

        internal void MyGroupsRadioButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(MyGroupsControl);
            if (MyGroupsControl.TypeOfGroups == ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.MyGroupsControl.GroupType.OneGroup)
            {
                ShowPanel(GroupControl);
            }
            else
            {
                if (MyGroupsControl.TypeOfGroups == ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.MyGroupsControl.GroupType.None)
                    MyOwnGroupsRadioButton.IsChecked = true;
                MyGroupsControl.Activate();
            }
        }

        internal void MyOrgGroupsRadioButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(MyGroupsControl);
            MyGroupsControl.Activate(MyGroupsControl.GroupType.MyOrgGroups);
        }

        internal void MyOrgPublicGroupsRadioButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(MyGroupsControl);
            MyGroupsControl.Activate(MyGroupsControl.GroupType.MyOrgPublicGroups);
        }

        internal void MyOwnGroupsRadioButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(MyGroupsControl);
            MyGroupsControl.Activate(MyGroupsControl.GroupType.MyGroups);
        }

        /// <summary>
        /// Occurs when the radio button for a group has been clicked.
        /// </summary>
        private void GroupRadioButton_Click(object sender)
        {
            GroupListBindingWrapper<Group> dataContext = (GroupListBindingWrapper<Group>)((FrameworkElement)sender).DataContext;
            dataContext.IsChecked = true;

            GroupsListBox.ScrollIntoView(dataContext);

            GroupControl.Activate(dataContext.Content);
            ShowPanel(GroupControl);
        }

        /// <summary>
        /// Adds a group to the GroupsListBox.
        /// </summary>
        public void AddGroup(Group group)
        {
            //fetch the group from AGOL to ensure it is up to date
            ArcGISOnlineEnvironment.ArcGISOnline.Group.GetGroup(group.Id, (object sender, GroupEventArgs e) =>
              {
                  GroupControl.Activate(e.Group);
                  ShowPanel(GroupControl);

                  ObservableCollection<GroupListBindingWrapper<Group>> groups = (ObservableCollection<GroupListBindingWrapper<Group>>)GroupsListBox.ItemsSource;
                  GroupListBindingWrapper<Group> dataContext = null;
                  if (!GroupListBoxContains(e.Group))
                  {
                      dataContext = new GroupListBindingWrapper<Group>() { Content = e.Group, IsChecked = true };
                      groups.Insert(0, dataContext);
                  }
                  else
                      dataContext = GetDataGontextForGroup(e.Group);

                  dataContext.IsChecked = true;

                  RadioButtonSeparator.Visibility = Visibility.Visible;
                  GroupsListBox.Visibility = Visibility.Visible;
                  GroupsListBox.ScrollIntoView(dataContext);
              });
        }

        /// <summary>
        /// Removes the specified group from the GroupsListBox and activates Search radio button.
        /// </summary>
        /// <param name="activatePage">If true activates the group above the closed group.</param>
        public void CloseGroup(Group group, bool activatePage)
        {
            ObservableCollection<GroupListBindingWrapper<Group>> groups = (ObservableCollection<GroupListBindingWrapper<Group>>)GroupsListBox.ItemsSource;

            int index = 0;
            foreach (GroupListBindingWrapper<Group> dataContext in groups)
            {
                if (dataContext.Content.Id == group.Id)
                {
                    groups.Remove(dataContext);
                    break;
                }
                index++;
            }

            if (activatePage)
            {
                if (groups.Count == 0)
                {
                    //activate "My Groups"
                    MyGroupsControl.TypeOfGroups = ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.MyGroupsControl.GroupType.None;
                    MyOwnGroupsRadioButton.IsChecked = true;
                    MyOwnGroupsRadioButton_Click(null, null);
                }
                else
                {
                    //activate group above group that has been removed
                    index = index > 0 ? index - 1 : 0;
                    AddGroup(groups[index].Content);
                }
            }

            if (groups.Count == 0)
                RadioButtonSeparator.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Retrieves the GroupListBoxItemDataContext for a specified Group from the GroupListBox.
        /// </summary>
        GroupListBindingWrapper<Group> GetDataGontextForGroup(Group group)
        {
            ObservableCollection<GroupListBindingWrapper<Group>> groups = (ObservableCollection<GroupListBindingWrapper<Group>>)GroupsListBox.ItemsSource;
            foreach (GroupListBindingWrapper<Group> groupDC in groups)
                if (groupDC.Content.Id == group.Id)
                    return groupDC;

            return null;
        }

        /// <summary>
        /// Checks if the GroupListBox contains the specified Group. 
        /// </summary>
        bool GroupListBoxContains(Group group)
        {
            ObservableCollection<GroupListBindingWrapper<Group>> groups = (ObservableCollection<GroupListBindingWrapper<Group>>)GroupsListBox.ItemsSource;
            foreach (GroupListBindingWrapper<Group> groupDC in groups)
                if (groupDC.Content.Id == group.Id)
                    return true;

            return false;
        }

    }

    /// <summary>
    /// Represents the DataContext a ListBoxItem in the GroupListBox is bound to.
    /// </summary>
    public class GroupListBindingWrapper<T> : BindingWrapper<T>
    {
        bool _isChecked = false;

        /// <summary>
        /// Gets or sets a boolean that specifies if a radio button in the GroupListBox is checked.
        /// </summary>
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (value == _isChecked)
                    return;

                _isChecked = value;

                NotifyPropertyChanged("IsChecked");
            }
        }
    }
}
