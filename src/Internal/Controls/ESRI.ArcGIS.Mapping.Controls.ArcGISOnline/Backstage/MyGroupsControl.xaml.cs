/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Mapping.Controls;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
    /// <summary>
    /// Implements the panel in the BackStage that is used to show groups that the signed in user owns or is a member of.
    /// </summary>
    public partial class MyGroupsControl : Control
    {
        public MyGroupsControl()
        {
            DefaultStyleKey = typeof(MyGroupsControl);
            OwnerCommand = new DelegateCommand(OwnerHyperlinkButton_Click);
            OpenCommand = new DelegateCommand(OpenThisGroupButton_Click);
        }

        public ICommand OwnerCommand { get; set; }
        public ICommand OpenCommand { get; set; }

        ListBox MyGroupsListBox;
        ProgressIndicator ProgressIndicator;
        TextBlock SearchResultsTextBlock;
        TextBlock Title;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            MyGroupsListBox = GetTemplateChild("MyGroupsListBox") as ListBox;
            ProgressIndicator = GetTemplateChild("ProgressIndicator") as ProgressIndicator;
            SearchResultsTextBlock = GetTemplateChild("SearchResultsTextBlock") as TextBlock;
            Title = GetTemplateChild("Title") as TextBlock;

            if (MyGroupsListBox != null)
                MyGroupsListBox.DataContext = this;
            ArcGISOnlineEnvironment.ArcGISOnline.User.SignedInOut += new EventHandler(ArcGISOnline_SignedInOut);
            if (pendingActivation)
                Activate(TypeOfGroups);
        }

        /// <summary>
        /// Raised when the user signs in or out. Updates the My Groups page.
        /// </summary>
        void ArcGISOnline_SignedInOut(object sender, EventArgs e)
        {
            Activate(TypeOfGroups); //initialize the control
        }

        public object Items
        {
            get { return (object)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(object), typeof(MyGroupsControl), null);

        internal void Clear()
        {
            TypeOfGroups = GroupType.None;
            if (MyGroupsListBox != null)
            {
                ObservableCollection<BindingWrapper<Group>> wrappers = MyGroupsListBox.ItemsSource as
                    ObservableCollection<BindingWrapper<Group>>;
                if (wrappers != null)
                {
                    BackStageControl backStage = Util.GetParentOfType<BackStageControl>(this);
                    if (backStage != null)
                    {
                        foreach (BindingWrapper<Group> item in wrappers)
                        {
                            backStage.CloseGroup(item.Content, false);
                        }
                    }
                }
                MyGroupsListBox.ItemsSource = null;
                MyGroupsListBox.Visibility = Visibility.Collapsed;
            }
            if (ProgressIndicator != null)
                ProgressIndicator.Visibility = Visibility.Visible;
            if (SearchResultsTextBlock != null)
                SearchResultsTextBlock.Text = "";
        }

        internal enum GroupType
        {
            None,
            OneGroup,
            MyGroups,
            MyOrgGroups,
            MyOrgPublicGroups
        }

        bool pendingActivation = false;
        internal GroupType TypeOfGroups = GroupType.None;

        internal void Activate()
        {
            if (TypeOfGroups == GroupType.None)
                Activate(GroupType.MyGroups);
            else
                Activate(TypeOfGroups);
        }

        /// <summary>
        /// Initializes the My Groups page.
        /// </summary>
        internal void Activate(GroupType type)
        {
            TypeOfGroups = type;
            if (MyGroupsListBox == null)
            {
                pendingActivation = true;
                return;
            }
            pendingActivation = false;
            if (TypeOfGroups == GroupType.OneGroup)
                return;
            Items = null;
            MyGroupsListBox.ItemsSource = null;
            MyGroupsListBox.Visibility = Visibility.Collapsed;
            ProgressIndicator.Visibility = Visibility.Visible;
            SearchResultsTextBlock.Text = "";

            

            ObservableCollection<BindingWrapper<Group>> wrappers = new ObservableCollection<BindingWrapper<Group>>();
            MyGroupsListBox.ItemsSource = wrappers;

            //Get the signed in user from the server to make sure it is up to date
            ArcGISOnlineEnvironment.ArcGISOnline.User.RefreshCurrent((object sender, RequestEventArgs e) =>
            {
                ProgressIndicator.Visibility = Visibility.Collapsed;

                if (e.Error != null)
                {
                    Items = null;
                    return;
                }

                if (type == GroupType.None)
                    return;

                User user = ArcGISOnlineEnvironment.ArcGISOnline.User.Current;
                Items = user;
                if (user == null)
                {
                    populateGroups(wrappers, null);
                    Title.Text = ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.BackStageControlMyGroups;
                }
                else
                {
                    switch (type)
                    {
                        case GroupType.MyGroups:
                            populateGroups(wrappers, user.Groups);
                            Title.Text = ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.BackStageControlMyGroups;
                            break;
                        case GroupType.MyOrgGroups:
                            Title.Text = ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.BackStageControlMyOrgGroups;
                            ArcGISOnlineEnvironment.ArcGISOnline.User.GetMyOrgGroups((object s1, GroupsEventArgs ge) =>
                            {
                                if (ge == null || ge.Error != null)
                                {
                                    Items = null;
                                    return;
                                }
                                populateGroups(wrappers, ge.Groups);
                            });
                            break;
                        case GroupType.MyOrgPublicGroups:
                            Title.Text = ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.BackStageControlMyOrgPublicGroups;
                            ArcGISOnlineEnvironment.ArcGISOnline.User.GetPublicOrgGroups((object s1, GroupsEventArgs ge) =>
                            {
                                if (ge == null || ge.Error != null)
                                {
                                    Items = null;
                                    return;
                                }
                                populateGroups(wrappers, ge.Groups);
                            });
                            break;
                        default:
                            break;
                    }
                }
            });

        }

        private void populateGroups(ObservableCollection<BindingWrapper<Group>> wrappers, Group[] groups)
        {
            if (groups != null && groups.Length > 0)
            {
                foreach (Group group in groups)
                {
                    BindingWrapper<Group> wrapper = new BindingWrapper<Group>();
                    wrapper.Content = group;
                    wrapper.Tag = IsUserOwnerOfGroup(group) ? Visibility.Visible : Visibility.Collapsed;

                    wrappers.Add(wrapper);
                }

                MyGroupsListBox.ItemsSource = wrappers;
                MyGroupsListBox.Visibility = Visibility.Visible;
                SearchResultsTextBlock.Text = string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.MyGroupControlGroups, groups.Length);
            }
            else
                SearchResultsTextBlock.Text = ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.MyGroupControlZeroGroups;
        }

        /// <summary>
        /// Determines if the current user owns the specified group.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        bool IsUserOwnerOfGroup(Group group)
        {
            User user = ArcGISOnlineEnvironment.ArcGISOnline.User.Current;
            if (user == null)
                return false;

            return user.Username == group.Owner;
        }

        /// <summary>
        /// Occurs when the OpenThisGroupButton is clicked.
        /// </summary>
        void OpenThisGroupButton_Click(object sender)
        {
            ListBoxItem lbi = Util.GetParentOfType<ListBoxItem>((DependencyObject)sender);

            BackStageControl backStage = Util.GetParentOfType<BackStageControl>(this);
            backStage.AddGroup(((BindingWrapper<Group>)lbi.DataContext).Content);
        }

        /// <summary>
        /// Occurs when the owner hyperlink button of a group is clicked - searches for other groups of that owner.
        /// </summary>
        void OwnerHyperlinkButton_Click(object sender)
        {
            string owner = null;
            if (sender is ContentControl)
                owner = ((ContentControl)sender).Content.ToString();
            else if (sender is string)
                owner = (string)sender;
            else
                return;

            BackStageControl backStage = Util.GetParentOfType<BackStageControl>(this);
            // DO NOT LOCALIZE "owner" - THIS IS A QUERY PARAMETER
            backStage.DoSearch(string.Format("owner: {0}", owner), SearchType.Groups);
        }

    }
}
