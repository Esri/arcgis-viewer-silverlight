/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Mapping.Controls;


namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
    /// <summary>
    /// Implements the panel in the BackStage that is used to show a group.
    /// </summary>
    public partial class GroupControl : Control
    {

        public GroupControl()
        {
            DefaultStyleKey = typeof(GroupControl);
            TagCommand = new DelegateCommand(TagButton_Click);
            OwnerCommand = new DelegateCommand(OwnerButton_Click);
            DetailsCommand = new DelegateCommand(MoreDetailsButton_Click);
            OpenCommand = new DelegateCommand(OpenButton_Click);
            FeatureMapCommand = new DelegateCommand(FeatureThisMapButton_Click);

        }

        public ICommand TagCommand { get; set; }
        public ICommand OwnerCommand { get; set; }
        public ICommand DetailsCommand { get; set; }
        public ICommand OpenCommand { get; set; }
        public ICommand FeatureMapCommand { get; set; }

        MapDetailsControl MapDetailsControl;
        ListBox UsersOfGroupListBox;
        ListBox MapsOfGroupListBox;
        TabControl Tab;
        DataPager DataPager;
        StackPanel FailedDescriptionPanel;
        HtmlTextBlock DescriptionRichTextBlock;
        TextBlock NoFeaturedMapsTextBlock;
        ListBox FeaturedMapsOfGroupListBox;
        TextBlock NoMapsTextBlock;
        TextBlock OwnerTextBlock;
        ListBox TagListBox;
        HyperlinkButton CloseGroupButton;
        HyperlinkButton GroupOwnerButton;
        HyperlinkButton OpenDescriptionInBrowserButton;

        public override void OnApplyTemplate()
        {
            if (MapDetailsControl != null)
            {
                MapDetailsControl.MapDetailsChanged -= RaiseMapDetailsChanged;
                MapDetailsControl.MapSelectedForOpening -= RaiseMapSelectedForOpening;
            }
            if (FeaturedMapsOfGroupListBox != null)
                FeaturedMapsOfGroupListBox.SelectionChanged -= FeaturedMapsOfGroupListBox_SelectionChanged;
            if (MapsOfGroupListBox != null)
                MapsOfGroupListBox.SelectionChanged -= MapsOfGroupListBox_SelectionChanged;
            if (CloseGroupButton != null)
                CloseGroupButton.Click -= CloseGroupButton_Click;
            if (GroupOwnerButton != null)
                GroupOwnerButton.Click -= GroupOwnerButton_Click;
            if (Tab != null)
                Tab.SelectionChanged -= Tab_SelectionChanged;
            if (OpenDescriptionInBrowserButton != null)
                OpenDescriptionInBrowserButton.Click -= OpenDescriptionInBrowserButton_Click;
            if (DescriptionRichTextBlock != null)
               DescriptionRichTextBlock.Loaded -= new RoutedEventHandler(DescriptionRichTextBlock_Loaded);

            base.OnApplyTemplate();

            MapDetailsControl = GetTemplateChild("MapDetailsControl") as MapDetailsControl;
            UsersOfGroupListBox = GetTemplateChild("UsersOfGroupListBox") as ListBox;
            MapsOfGroupListBox = GetTemplateChild("MapsOfGroupListBox") as ListBox;
            Tab = GetTemplateChild("Tab") as TabControl;
            DataPager = GetTemplateChild("DataPager") as DataPager;
            FailedDescriptionPanel = GetTemplateChild("FailedDescriptionPanel") as StackPanel;
            DescriptionRichTextBlock = GetTemplateChild("DescriptionRichTextBlock") as HtmlTextBlock;
            NoFeaturedMapsTextBlock = GetTemplateChild("NoFeaturedMapsTextBlock") as TextBlock;
            FeaturedMapsOfGroupListBox = GetTemplateChild("FeaturedMapsOfGroupListBox") as ListBox;
            NoMapsTextBlock = GetTemplateChild("NoMapsTextBlock") as TextBlock;
            OwnerTextBlock = GetTemplateChild("OwnerTextBlock") as TextBlock;
            TagListBox = GetTemplateChild("TagListBox") as ListBox;
            CloseGroupButton = GetTemplateChild("CloseGroupButton") as HyperlinkButton;
            GroupOwnerButton = GetTemplateChild("GroupOwnerButton") as HyperlinkButton;
            OpenDescriptionInBrowserButton = GetTemplateChild("OpenDescriptionInBrowserButton") as HyperlinkButton;

            UsersOfGroupListBox.ItemsSource = new ObservableCollection<string>();
            GroupControl_Loaded();
            if (MapDetailsControl != null)
            {
                MapDetailsControl.MapDetailsChanged += RaiseMapDetailsChanged;
                MapDetailsControl.MapSelectedForOpening += RaiseMapSelectedForOpening;
            }
            if (TagListBox != null)
                TagListBox.Tag = this;
            if (FeaturedMapsOfGroupListBox != null)
            {
                FeaturedMapsOfGroupListBox.Tag = this;
                FeaturedMapsOfGroupListBox.SelectionChanged += FeaturedMapsOfGroupListBox_SelectionChanged;
            }
            if (MapsOfGroupListBox != null)
            {
                MapsOfGroupListBox.Tag = this;
                MapsOfGroupListBox.SelectionChanged += MapsOfGroupListBox_SelectionChanged;
            }
            if (CloseGroupButton != null)
                CloseGroupButton.Click += CloseGroupButton_Click;
            if (GroupOwnerButton != null)
                GroupOwnerButton.Click += GroupOwnerButton_Click;
            if (Tab != null)
                Tab.SelectionChanged += Tab_SelectionChanged;
            if (OpenDescriptionInBrowserButton != null)
                OpenDescriptionInBrowserButton.Click += OpenDescriptionInBrowserButton_Click;
           if (pendingActivation != null)
                Activate(pendingActivation);
           if (DescriptionRichTextBlock != null)
           {
               DescriptionRichTextBlock.Loaded += new RoutedEventHandler(DescriptionRichTextBlock_Loaded);
               DescriptionRichTextBlock_Loaded(null, null);
           }
        }

        void RaiseMapDetailsChanged(object sender, EventArgs e)
        {
            if (MapDetailsChanged != null)
                MapDetailsChanged(null, EventArgs.Empty);
        }

        /// <summary>
        /// Raised when the details of a map have been modified.
        /// E.g. title, thumbnail, summary, tags, description etc..
        /// </summary>
        public event EventHandler MapDetailsChanged;

        /// <summary>
        /// Determines whether the properties of the group can be edited.
        /// </summary>
        bool CanEdit
        {
            get
            {
                if (DataContext == null)
                    return false;

                GroupBindingWrapper wrapper = (GroupBindingWrapper)DataContext;

                User user = ArcGISOnlineEnvironment.ArcGISOnline.User.Current;
                bool signedIn = (user != null);
                bool userIsGroupOwner = signedIn ? (wrapper.Content.Owner == user.Username) : false;

                return userIsGroupOwner;
            }
        }

        /// <summary>
        /// Occurs when the GroupControl is loaded.
        /// </summary>
        void GroupControl_Loaded()
        {
            ArcGISOnlineEnvironment.ArcGISOnline.User.SignedInOut += new EventHandler(ArcGISOnline_SignedInOut);
        }

        /// <summary>
        /// Occurs when the user signs in or out from AGOL.
        /// </summary>
        void ArcGISOnline_SignedInOut(object sender, EventArgs e)
        {
            GroupBindingWrapper wrapper = DataContext as GroupBindingWrapper;
            if (wrapper == null)
                return;

            //show/hide edit icons
            //
            wrapper.EditButtonVisibility = CanEdit ? Visibility.Visible : Visibility.Collapsed;

            //show/hide join, invite and leave button
            //
            SetJoinLeaveButtonVisibility();

            //reinitialize tab
            //
            Tab_SelectionChanged(null, null);
        }

        /// <summary>
        /// Determines the visibility of FeatureThisMapButton. If the signed in user is the owner of 
        /// the current group the button is visible otherwise not.
        /// </summary>
        void SetFeatureThisMapButtonVisibility()
        {
            Group group = ((GroupBindingWrapper)DataContext).Content;
            if (group == null)
                return;

            Visibility visibility = Visibility.Collapsed;
            if (ArcGISOnlineEnvironment.ArcGISOnline.User.IsSignedIn && group.Owner.Equals(ArcGISOnlineEnvironment.ArcGISOnline.User.Current.Username))
                visibility = Visibility.Visible;
            else
                visibility = Visibility.Collapsed;

            if (MapsOfGroupListBox.ItemsSource != null)
                foreach (object obj in MapsOfGroupListBox.ItemsSource)
                {
                    GroupMapBindingWrapper wrapper = (GroupMapBindingWrapper)obj;
                    wrapper.FeatureButtonVisibility = visibility;
                }
        }

        /// <summary>
        /// Occurs when the rich text control is loaded.
        /// </summary>
        void DescriptionRichTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            //text can now be loaded
            if (Tab.SelectedIndex == 0)
                Tab_SelectionChanged(null, null);
        }

        Group pendingActivation;
        /// <summary>
        /// Activates the control.
        /// </summary>
        public void Activate(Group group)
        {
            if (MapDetailsControl == null)
            {
                pendingActivation = group;
                return;
            }
            pendingActivation = null;
            GroupBindingWrapper wrapper = new GroupBindingWrapper() { EditButtonVisibility = Visibility.Collapsed };
            DataContext = wrapper;
            wrapper.Content = group;
            wrapper.EditButtonVisibility = CanEdit ? Visibility.Visible : Visibility.Collapsed;

            if (Tab.SelectedIndex == 0 || Tab.SelectedIndex == 3)
                MapDetailsControl.Visibility = Visibility.Collapsed;
            else
                MapDetailsControl.DataContext = null;

            //initializes the tab control
            //
            Tab_SelectionChanged(null, null);

            //get the members of the group
            //
            ArcGISOnlineEnvironment.ArcGISOnline.Group.GetMembers(group.Id, GetGroupMembersCompleted);
        }

        /// <summary>
        /// Refreshes the page.
        /// </summary>
        void Refresh()
        {
            GroupBindingWrapper wrapper = (GroupBindingWrapper)DataContext;
            ArcGISOnlineEnvironment.ArcGISOnline.Group.GetGroup(wrapper.Content.Id, (object sender, GroupEventArgs e) =>
              {
                  wrapper.Content = e.Group;

                  //initializes the tab control
                  Tab_SelectionChanged(null, null);

                  //raise event
                  RaiseGroupDetailsChanged(e.Group);
              });
        }

        /// <summary>
        /// Raised when the details of a group have been modified.
        /// E.g. title, thumbnail, summary, tags, description etc..
        /// </summary>
        public event EventHandler<GroupEventArgs> GroupDetailsChanged;

        /// <summary>
        /// Raises the GroupDetailsChanged event.
        /// </summary>
        internal void RaiseGroupDetailsChanged(Group group)
        {
            if (GroupDetailsChanged != null)
                GroupDetailsChanged(null, new GroupEventArgs() { Group = group });
        }

        /// <summary>
        /// Occurs when the selected tab of the TabControl changes.
        /// </summary>
        private void Tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext == null)
                return;

            Group group = ((GroupBindingWrapper)DataContext).Content;
            if (group == null)
                return;

            DataPager.Visibility = Visibility.Collapsed;

            switch (Tab.SelectedIndex)
            {
                case 0: //description
                    FailedDescriptionPanel.Visibility = Visibility.Collapsed;
                    try
                    {
                        DescriptionRichTextBlock.Html = group.Description ?? "";
                    }
                    catch
                    {
                        // formatting to html has failed - show the message and hyperlink
                        FailedDescriptionPanel.Visibility = Visibility.Visible;
                    }
                    break;
                case 1: //featured maps
                    NoFeaturedMapsTextBlock.Visibility = group.FeaturedItemsId == null ? Visibility.Visible : Visibility.Collapsed;
                    FeaturedMapsOfGroupListBox.ItemsSource = null;

                    if (group.FeaturedItemsId != null)
                        ArcGISOnlineEnvironment.ArcGISOnline.Content.GetRelatedItems(group.FeaturedItemsId, "FeaturedItems2Item", "forward", GetGroupFeaturedMapsCompleted);
                    else if (MapDetailsControl.DataContext == null)    // if there is no selected map in the current group, collapse the map details panel
                        MapDetailsControl.Visibility = Visibility.Collapsed;
                    break;
                case 2: //all maps
                    NoMapsTextBlock.Visibility = Visibility.Collapsed;
                    MapsOfGroupListBox.ItemsSource = null;

                    string query = "group:" + group.Id + " AND " + ArcGISOnlineEnvironment.WebMapTypeQualifier;
                    ArcGISOnlineEnvironment.ArcGISOnline.Content.Search(query, GroupMapsSearchCompleted);
                    break;
                case 3: //members
                    break;
            }
        }

        /// <summary>
        /// Raised when the request for featured maps of a group completes.
        /// </summary>
        void GetGroupFeaturedMapsCompleted(object sender, ContentItemsEventArgs e)
        {
            if (e.Error != null || e.Items == null)
            {
                NoFeaturedMapsTextBlock.Visibility = Visibility.Visible;

                if (MapDetailsControl.DataContext == null)
                    MapDetailsControl.Visibility = Visibility.Collapsed;

                //set the content of the FeatureThisMapButtons of the maps in MapsOfGroupListBox
                if (MapsOfGroupListBox.ItemsSource != null)
                    foreach (object obj in MapsOfGroupListBox.ItemsSource)
                    {
                        GroupMapBindingWrapper wrapper = (GroupMapBindingWrapper)obj;
                        wrapper.IsFeatured = false;
                    }
                return;
            }

            ObservableCollection<ContentItem> featuredMaps = new ObservableCollection<ContentItem>();
            foreach (ContentItem item in e.Items)
                if (item.Type == "Web Map")
                    featuredMaps.Add(item);

            //set the content of the FeatureThisMapButtons of the maps in MapsOfGroupListBox
            if (MapsOfGroupListBox.ItemsSource != null)
                foreach (object obj in MapsOfGroupListBox.ItemsSource)
                {
                    GroupMapBindingWrapper wrapper = (GroupMapBindingWrapper)obj;
                    ContentItem item = (ContentItem)wrapper.Item;
                    bool itemFound = false;
                    foreach (ContentItem featuredItem in featuredMaps)
                        if (featuredItem.Id == item.Id)
                        {
                            itemFound = true;
                            wrapper.IsFeatured = true;
                            break;
                        }
                    if (!itemFound)
                        wrapper.IsFeatured = false;
                }

            if (featuredMaps.Count > 0)
            {
                FeaturedMapsOfGroupListBox.ItemsSource = featuredMaps;
                FeaturedMapsOfGroupListBox.SelectedItem = featuredMaps[0];
                FeaturedMapsOfGroupListBox.ScrollIntoView(featuredMaps[0]);
                if (MapDetailsControl.Visibility == Visibility.Visible && Tab.SelectedIndex == 1)
                    MapDetailsControl.Activate(featuredMaps[0]);
            }
            else
            {
                NoFeaturedMapsTextBlock.Visibility = Visibility.Visible;
                if (MapDetailsControl.DataContext == null)
                    MapDetailsControl.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Raised when the request for group members completes.
        /// </summary>
        void GetGroupMembersCompleted(object sender, GroupMembersEventArgs e)
        {
            if (e.Error != null)
                return;

            ObservableCollection<string> users = (ObservableCollection<string>)UsersOfGroupListBox.ItemsSource;
            users.Clear();
            OwnerTextBlock.Text = "";
            OwnerTextBlock.Visibility = Visibility.Collapsed;

            GroupMembers groupMembers = (GroupMembers)e.GroupMembers;
            if (groupMembers.Owner == null)
                return;

            foreach (string user in groupMembers.Users)
                users.Add(user);

            OwnerTextBlock.Text = e.GroupMembers.Owner;
            OwnerTextBlock.Visibility = Visibility.Visible;

            //show/hide join, invite and leave button
            //
            SetJoinLeaveButtonVisibility();
        }


        /// <summary>
        /// Shows/hides the join, and leave buttons.
        /// </summary>
        void SetJoinLeaveButtonVisibility()
        {
            GroupBindingWrapper wrapper = (GroupBindingWrapper)DataContext;
            wrapper.JoinButtonVisibility = Visibility.Collapsed;
            wrapper.LeaveButtonVisibility = Visibility.Collapsed;

            ObservableCollection<string> users = (ObservableCollection<string>)UsersOfGroupListBox.ItemsSource;

            User currentUser = ArcGISOnlineEnvironment.ArcGISOnline.User.Current;
            if (currentUser != null && currentUser.Username != OwnerTextBlock.Text)
            {
                foreach (string user in users)
                    if (user == currentUser.Username) //current user is already member of group
                    {
                        wrapper.LeaveButtonVisibility = Visibility.Visible;
                        break;
                    }

                if (wrapper.Content.IsInvitationOnly)
                    wrapper.JoinButtonVisibility = Visibility.Collapsed; //don't show join button if group is set for joining by invitation only
                else
                    wrapper.JoinButtonVisibility = wrapper.LeaveButtonVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Raised when the search for group maps completes.
        /// </summary>
        void GroupMapsSearchCompleted(object sender, ContentSearchEventArgs e)
        {
            if (e.Error != null)
            {
                NoMapsTextBlock.Visibility = Visibility.Visible;
                return;
            }

            PagedSearchResult pSRes = new PagedSearchResult(e.Result, new WrapperFactoryDelegate(WrapContentItem));
            DataPager.DataContext = pSRes;
            DataPager.Visibility = DataPager.PageCount > 1 ? Visibility.Visible : Visibility.Collapsed;

            MapsOfGroupListBox.ItemsSource = pSRes;

            SetFeatureThisMapButtonVisibility();

            //check which maps of this group are featured to set the content of the FeatureThisMapButton
            ArcGISOnlineEnvironment.ArcGISOnline.Content.GetRelatedItems(((GroupBindingWrapper)DataContext).Content.FeaturedItemsId, "FeaturedItems2Item", "forward", GetGroupFeaturedMapsCompleted);

            //select first item in list and show/hide map details depending on 
            //if it was previously visible
            //
            if (e.Result.Items != null && e.Result.Items.Length > 0)
            {
                MapsOfGroupListBox.SelectedItem = pSRes[0];
                MapsOfGroupListBox.ScrollIntoView(pSRes[0]);
                if (MapDetailsControl.Visibility == Visibility.Visible && Tab.SelectedIndex == 2)
                    MapDetailsControl.Activate(((GroupMapBindingWrapper)MapsOfGroupListBox.SelectedItem).Item);
            }
            else
            {
                NoMapsTextBlock.Visibility = Visibility.Visible;
                MapDetailsControl.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Wraps the specified object in a GroupMapBindingWrapper which is bound to a listbox item.
        /// </summary>
        /// <param name="contentItem"></param>
        /// <returns></returns>
        object WrapContentItem(object contentItem)
        {
            if (!(contentItem is ContentItem))
                return contentItem;

            return new GroupMapBindingWrapper((ContentItem)contentItem);
        }

        /// <summary>
        /// Occurs when an owner hyperlink button is clicked - performs a search
        /// for other maps from that owner.
        /// </summary>
        private void OwnerButton_Click(object sender)
        {
            string owner = null;
            if (sender is HyperlinkButton)
                owner = ((HyperlinkButton)sender).Content.ToString();
            else if (sender is string)
                owner = (string)sender;
            else
                return;

            BackStageControl backStage = Util.GetParentOfType<BackStageControl>(this);
            // DO NOT LOCALIZE "owner" - THIS IS A QUERY PARAMETER
            backStage.DoSearch(string.Format("owner: {0}", owner), SearchType.Maps);
        }

        /// <summary>
        /// Occurs when the More Details button is clicked.
        /// </summary>
        private void MoreDetailsButton_Click(object sender)
        {
            ListBoxItem lbi = Util.GetParentOfType<ListBoxItem>((FrameworkElement)sender);

            switch (Tab.SelectedIndex)
            {
                case 1: //featured maps
                    FeaturedMapsOfGroupListBox.SelectedItem = lbi.DataContext;
                    MapDetailsControl.Activate((ContentItem)lbi.DataContext);
                    break;
                case 2: //all maps
                    MapsOfGroupListBox.SelectedItem = lbi.DataContext;
                    MapDetailsControl.Activate(((GroupMapBindingWrapper)lbi.DataContext).Item);
                    break;
                default:
                    return;
            }

            MapDetailsControl.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Occurs when the Open button is clicked.
        /// </summary>
        private void OpenButton_Click(object sender)
        {
            ListBoxItem lbi = Util.GetParentOfType<ListBoxItem>(sender as DependencyObject);
            lbi.IsSelected = true;

            switch (Tab.SelectedIndex)
            {
                case 1: //featured maps
                    RaiseMapSelectedForOpening(this, new ContentItemEventArgs() { Item = FeaturedMapsOfGroupListBox.SelectedItem as ContentItem });
                    break;
                case 2: //all maps
                    RaiseMapSelectedForOpening(this, new ContentItemEventArgs() { Item = ((GroupMapBindingWrapper)MapsOfGroupListBox.SelectedItem).Item });
                    break;
                default:
                    return;
            }
        }

        public event EventHandler<ContentItemEventArgs> MapSelectedForOpening;

        void RaiseMapSelectedForOpening(object sender, ContentItemEventArgs e)
        {
            if (MapSelectedForOpening != null)
                MapSelectedForOpening(this, e);
        }

        /// <summary>
        /// Occurs when the selection in the MapsOfGroupListBox changes.
        /// </summary>
        private void MapsOfGroupListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Tab.SelectedIndex == 2)
            {
                //update the item displayed in the detailed MapDetailsControl
                if (MapsOfGroupListBox.SelectedItem != null)
                    MapDetailsControl.Activate(((GroupMapBindingWrapper)MapsOfGroupListBox.SelectedItem).Item);
            }
        }

        /// <summary>
        /// Occurs when the selection in the FeaturedMapsOfGroupListBox changes.
        /// </summary>
        private void FeaturedMapsOfGroupListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Tab.SelectedIndex == 1)
            {
                //update the item displayed in the detailed MapDetailsControl
                if (FeaturedMapsOfGroupListBox.SelectedItem != null)
                    MapDetailsControl.Activate((ContentItem)FeaturedMapsOfGroupListBox.SelectedItem);
            }
        }

        /// <summary>
        /// Occurs when the close button is clicked.
        /// </summary>
        internal void CloseGroupButton_Click(object sender, RoutedEventArgs e)
        {
            BackStageControl backStage = Util.GetParentOfType<BackStageControl>(this);
            backStage.CloseGroup(((GroupBindingWrapper)DataContext).Content, true);
        }

        /// <summary>
        /// Occurs when the FeatureThisMapButton is clicked.
        /// Features/unfeatures the associated map in the group.
        /// </summary>
        private void FeatureThisMapButton_Click(object sender)
        {
            Group group = ((GroupBindingWrapper)DataContext).Content;
            FrameworkElement button = (FrameworkElement)sender;
            GroupMapBindingWrapper wrapper = (GroupMapBindingWrapper)button.DataContext;
            ContentItem map = (ContentItem)wrapper.Item;

            if (wrapper.IsFeatured)
                ArcGISOnlineEnvironment.ArcGISOnline.Group.UnfeatureMap(group.Id, map.Id, (object sender2, RequestEventArgs e2) =>
                  {
                      if (e2.Error != null)
                          return;

                      wrapper.IsFeatured = false;
                  });
            else
                ArcGISOnlineEnvironment.ArcGISOnline.Group.FeatureMap(group.Id, map.Id, (object sender2, RequestEventArgs e2) =>
                  {
                      if (e2.Error != null)
                          return;

                      wrapper.IsFeatured = true;
                  });
        }

        /// <summary>
        /// Occurs when the group owner hyperlink button is clicked - performs a search
        /// for other groups from that owner.
        /// </summary>
        private void GroupOwnerButton_Click(object sender, RoutedEventArgs e)
        {
            string owner = ((HyperlinkButton)sender).Content.ToString();
            BackStageControl backStage = Util.GetParentOfType<BackStageControl>(this);
            // DO NOT LOCALIZE "owner" - THIS IS A QUERY PARAMETER
            backStage.DoSearch(string.Format("owner: {0}", owner), SearchType.Groups);
        }

        /// <summary>
        /// Occurs when the hyperlink button to show the group description in a separate browser tab has been clicked.
        /// </summary>
        private void OpenDescriptionInBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            string url = ArcGISOnlineEnvironment.ArcGISOnline.Group.GetGroupUrl(((GroupBindingWrapper)DataContext).Content.Id);
            Util.Navigate(url, "_blank");
        }

        /// <summary>
        /// Raised when a tag hyperlink button has been clicked - performs a search for
        /// other groups with this tag.
        /// </summary>
        private void TagButton_Click(object sender)
        {
            ContentControl button = (ContentControl)sender;
            BackStageControl backStage = Util.GetParentOfType<BackStageControl>(this);

            backStage.DoSearch((string)button.Content, SearchType.Groups);
        }

    }

    /// <summary>
    /// Contains state that UI elements of the GroupControl bind to.
    /// </summary>
    public class GroupBindingWrapper : BindingWrapper<Group>
    {
        Visibility _editButtonVisibility = Visibility.Collapsed;
        Visibility _joinButtonVisibility = Visibility.Collapsed;
        Visibility _leaveButtonVisibility = Visibility.Collapsed;

        /// <summary>
        /// Gets or sets the visibility of the edit icons.
        /// </summary>
        public Visibility EditButtonVisibility
        {
            get { return _editButtonVisibility; }
            set
            {
                _editButtonVisibility = value;
                NotifyPropertyChanged("EditButtonVisibility");
            }
        }

        /// <summary>
        /// Gets or sets the visibility of the join group button.
        /// </summary>
        public Visibility JoinButtonVisibility
        {
            get { return _joinButtonVisibility; }
            set
            {
                _joinButtonVisibility = value;
                NotifyPropertyChanged("JoinButtonVisibility");
            }
        }

        /// <summary>
        /// Gets or sets the visibility of the leave group button.
        /// </summary>
        public Visibility LeaveButtonVisibility
        {
            get { return _leaveButtonVisibility; }
            set
            {
                _leaveButtonVisibility = value;
                NotifyPropertyChanged("LeaveButtonVisibility");
            }
        }
    }

    /// <summary>
    /// Wraps a ContentItem for Binding and provides an additional FeatureButtonText property which is bound to
    /// the FeatureThisMapButton content.
    /// </summary>
    public class GroupMapBindingWrapper : INotifyPropertyChanged
    {
        ContentItem _item;

        string _dontFeatureMapStr = "Don't feature this Map";
        string _featureMapStr = "Feature this Map";
        bool _isFeatured = false;
        Visibility _featureBtnVisibility = Visibility.Collapsed;

        public GroupMapBindingWrapper(ContentItem item)
        {
            _item = item;
        }

        /// <summary>
        /// Returns the wrapped ContentItem.
        /// </summary>
        public ContentItem Item { get { return _item; } }

        /// <summary>
        /// Gets the text for the FeatureThisMapButton.
        /// </summary>
        public string FeatureButtonText
        {
            get { return (_isFeatured == true) ? _dontFeatureMapStr : _featureMapStr; }
        }

        /// <summary>
        /// Gets or sets a boolean that indicates wheather the map is featured or not.
        /// </summary>
        public bool IsFeatured
        {
            get { return _isFeatured; }
            set
            {
                _isFeatured = value;
                NotifyPropertyChanged("FeatureButtonText"); //notify so the text of the feature button changes
            }
        }

        /// <summary>
        /// Gets or sets the visibility for the FeatureThisMapButton.
        /// </summary>
        public Visibility FeatureButtonVisibility
        {
            get { return _featureBtnVisibility; }
            set
            {
                _featureBtnVisibility = value;
                NotifyPropertyChanged("FeatureButtonVisibility");
            }
        }

        void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
