/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Mapping.Controls;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
    /// <summary>
    /// Implements the panel that shows details of a map in the BackStage.
    /// </summary>
    public partial class MapDetailsControl : Control
    {
        bool _initialized = false;
        bool _richTextLoaded = false;

        public MapDetailsControl()
        {
            DefaultStyleKey = typeof(MapDetailsControl);
            TagCommand = new DelegateCommand(TagButton_Click);
        }

        TabControl MapDetailsTabControl;
        TextBlock TitleTextBlock;
        TextBlock SummaryTextBlock;
        ListBox TagListBox;
        Rating MapRating;
        System.Windows.Shapes.Rectangle SeparatorRectangle;
        ListBox CommentsListBox;
        StackPanel FailedDescriptionPanel;
        HtmlTextBlock DescriptionRichTextBlock;
        HyperlinkButton CloseDetailsButton;
        HyperlinkButton OpenButton;
        HyperlinkButton OwnerHyperlinkButton;
        HyperlinkButton OpenDescriptionInBrowserButton;
        public override void OnApplyTemplate()
        {
            if (MapDetailsTabControl != null)
                MapDetailsTabControl.SelectionChanged -= MapDetailsTabControl_SelectionChanged;
            if (CloseDetailsButton != null)
                CloseDetailsButton.Click -= CloseDetailsButton_Click;
            if (OpenButton != null)
                OpenButton.Click -= OpenButton_Click;
            if (OwnerHyperlinkButton != null)
                OwnerHyperlinkButton.Click -= OwnerButton_Click;
            if (OpenDescriptionInBrowserButton != null)
                OpenDescriptionInBrowserButton.Click -= OpenDescriptionInBrowserButton_Click;

            base.OnApplyTemplate();

            MapDetailsTabControl = GetTemplateChild("MapDetailsTabControl") as TabControl;
            TitleTextBlock = GetTemplateChild("TitleTextBlock") as TextBlock;
            SummaryTextBlock = GetTemplateChild("SummaryTextBlock") as TextBlock;
            TagListBox = GetTemplateChild("TagListBox") as ListBox;
            MapRating = GetTemplateChild("MapRating") as Rating;
            SeparatorRectangle = GetTemplateChild("SeparatorRectangle") as System.Windows.Shapes.Rectangle;
            CommentsListBox = GetTemplateChild("CommentsListBox") as ListBox;
            FailedDescriptionPanel = GetTemplateChild("FailedDescriptionPanel") as StackPanel;
            DescriptionRichTextBlock = GetTemplateChild("DescriptionRichTextBlock") as HtmlTextBlock;
            CloseDetailsButton = GetTemplateChild("CloseDetailsButton") as HyperlinkButton;
            OpenButton = GetTemplateChild("OpenButton") as HyperlinkButton;
            OwnerHyperlinkButton = GetTemplateChild("OwnerHyperlinkButton") as HyperlinkButton;
            OpenDescriptionInBrowserButton = GetTemplateChild("OpenDescriptionInBrowserButton") as HyperlinkButton;

            if (MapDetailsTabControl != null)
                MapDetailsTabControl.SelectionChanged += MapDetailsTabControl_SelectionChanged;
            if (CloseDetailsButton != null)
                CloseDetailsButton.Click += CloseDetailsButton_Click;
            if (OpenButton != null)
                OpenButton.Click += OpenButton_Click;
            if (TagListBox != null)
                TagListBox.Tag = this;
            if (OwnerHyperlinkButton != null)
                OwnerHyperlinkButton.Click += OwnerButton_Click;
            if (OpenDescriptionInBrowserButton != null)
                OpenDescriptionInBrowserButton.Click += OpenDescriptionInBrowserButton_Click;
            if (DescriptionRichTextBlock != null)
                DescriptionRichTextBlock_Loaded();

            if (pendingActivate)
                Activate(pendingItem);
        }

        public ICommand TagCommand { get; set; }

        /// <summary>
        /// Hides the controls used to edit map details.
        /// </summary>
        private void HideEditingControls()
        {
            TitleTextBlock.Visibility = Visibility.Visible;

            SummaryTextBlock.Visibility = Visibility.Visible;

            TagListBox.Visibility = Visibility.Visible;
        }

        bool pendingActivate;
        ContentItem pendingItem;
        /// <summary>
        /// Initializes the control.
        /// </summary>
        public void Activate(ContentItem contentItem)
        {
            if (MapDetailsTabControl == null)
            {
                pendingActivate = true;
                pendingItem = contentItem;
                return;
            }

            pendingActivate = false;
            if (!_initialized)
            {
                _initialized = true;
            }

            HideEditingControls();

            //use a BindingWrapper to bind additional properties such as average rating, number of ratings..
            MapDetailsBindingWrapper<ContentItem> wrapper = new MapDetailsBindingWrapper<ContentItem>();
            //bind the control to the wrapper
            DataContext = wrapper;
            wrapper.Content = contentItem;
            InitializeBindingProperties(wrapper);

            //initialize the tabs
            MapDetailsTabControl_SelectionChanged(null, null);
        }

        void Refresh()
        {
            MapDetailsBindingWrapper<ContentItem> wrapper = (MapDetailsBindingWrapper<ContentItem>)DataContext;

            //get the content item from the server to make sure it is up to date
            ArcGISOnlineEnvironment.ArcGISOnline.Content.GetItem(wrapper.Content.Id, (object sender, ContentItemEventArgs e) =>
            {
                if (e.Error != null)
                    return;

                //preserve the folder information if it was already determined
                //
                e.Item.Folder = wrapper.Content.Folder;

                wrapper.Content = e.Item;
                InitializeBindingProperties(wrapper);

                //initialize the tabs
                MapDetailsTabControl_SelectionChanged(null, null);
            });

            //raise event
            RaiseMapDetailsChanged();
        }

        /// <summary>
        /// Raised when the details of a map have been modified.
        /// E.g. title, thumbnail, summary, tags, description etc..
        /// </summary>
        public event EventHandler MapDetailsChanged;

        /// <summary>
        /// Raises the MapDetailsChanged event.
        /// </summary>
        void RaiseMapDetailsChanged()
        {
            if (MapDetailsChanged != null)
                MapDetailsChanged(null, EventArgs.Empty);
        }

        /// <summary>
        /// Initializes additional binding properties for average rating, number of ratings etc..
        /// </summary>
        void InitializeBindingProperties(MapDetailsBindingWrapper<ContentItem> wrapper)
        {
            wrapper.RatingValue = wrapper.Content.AverageRating / (double)MapRating.ItemCount;
            wrapper.NumberOfRatings = string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.MapDetailsRating, wrapper.Content.NumberOfRatings);
            wrapper.EditButtonVisibility = CanEditMapDetails ? Visibility.Visible : Visibility.Collapsed;
            wrapper.RateButtonVisibility = CanRate ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Determines if the user can rate the current map.
        /// <remarks>
        /// The user can rate the map if he is not the owner of the map 
        /// and if this control is hosted by the MapInfoControl.
        /// </remarks>
        /// </summary>
        bool CanRate
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if the details of the current map can be edited.
        /// <remarks>
        /// The map details can be edited if the current user owns the map and 
        /// this control is hosted by the MapInfoControl.
        /// </remarks>
        /// </summary>
        bool CanEditMapDetails
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Occurs when the Open button is clicked.
        /// </summary>
        private void OpenButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            BindingWrapper<ContentItem> wrapper = (BindingWrapper<ContentItem>)DataContext;
            RaiseMapSelectedForOpening(this, new ContentItemEventArgs() { Item = wrapper.Content });
        }

        public event EventHandler<ContentItemEventArgs> MapSelectedForOpening;

        void RaiseMapSelectedForOpening(object sender, ContentItemEventArgs e)
        {
            if (MapSelectedForOpening != null)
                MapSelectedForOpening(this, e);
        }

        /// <summary>
        /// Occurs when a tag keyword has been clicked.
        /// </summary>
        private void TagButton_Click(object sender)
        {
            ContentControl button = (ContentControl)sender;
            BackStageControl backStage = Util.GetParentOfType<BackStageControl>(this);

            backStage.DoSearch((string)button.Content, SearchType.Maps);
        }

        /// <summary>
        /// Occurs when the owner keyword has been clicked.
        /// </summary>
        private void OwnerButton_Click(object sender, RoutedEventArgs e)
        {
            ContentControl button = (ContentControl)sender;
            BackStageControl backStage = Util.GetParentOfType<BackStageControl>(this);
            backStage.DoSearch("owner:" + (string)button.Content, SearchType.Maps);
        }

        /// <summary>
        /// Occurs when the CloseDetails button is clicked.
        /// </summary>
        internal void CloseDetailsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// Identifies the <see cref="IsSeparatorVisible"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsSeparatorVisibleProperty =
                DependencyProperty.Register("IsSeparatorVisible", typeof(Visibility), typeof(MapDetailsControl),
                new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Gets or sets IsSeparatorVisible.
        /// </summary>
        public Visibility IsSeparatorVisible
        {
            get { return (Visibility)GetValue(IsSeparatorVisibleProperty); }
            set
            {
                SetValue(IsSeparatorVisibleProperty, value);
            }
        }

        /// <summary>
        /// Occurs when the DescriptionRichTextBox is loaded.
        /// </summary>
        private void DescriptionRichTextBlock_Loaded()
        {
            _richTextLoaded = true;

            //text can now be loaded
            if (MapDetailsTabControl.SelectedIndex == 1)
                MapDetailsTabControl_SelectionChanged(null, null);
        }

        /// <summary>
        /// Occurs when the hyperlink button to show the map description in a separate browser tab has been clicked.
        /// </summary>
        private void OpenDescriptionInBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            BindingWrapper<ContentItem> wrapper = (BindingWrapper<ContentItem>)DataContext;
            string url = ArcGISOnlineEnvironment.ArcGISOnline.Content.GetItemUrl(wrapper.Content.Id);
            Util.Navigate(url, "_blank");
        }

        /// <summary>
        /// Raised when the tab of the MapDetailsTabControl changes.
        /// </summary>
        private void MapDetailsTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MapDetailsTabControl == null)
                return;

            switch (MapDetailsTabControl.SelectedIndex)
            {
                case 0: //properties
                    break;
                case 1: //decription

                    FailedDescriptionPanel.Visibility = Visibility.Collapsed;

                    if (_richTextLoaded)
                    {
                        BindingWrapper<ContentItem> wrapper = (BindingWrapper<ContentItem>)DataContext;
                        try
                        {
                            DescriptionRichTextBlock.Html = wrapper.Content.Description ?? "";
                        }
                        catch
                        {
                            // formatting to html has failed - show the message and hyperlink
                            FailedDescriptionPanel.Visibility = Visibility.Visible;
                        }
                    }
                    break;
                case 2: //comments

                    InitializeComments();
                    break;
            }
        }

        /// <summary>
        /// Initializes the comments tab
        /// </summary>
        private void InitializeComments()
        {
            //get up to date comments for the current map

            BindingWrapper<ContentItem> wrapper = (BindingWrapper<ContentItem>)DataContext;
            CommentsListBox.Visibility = Visibility.Collapsed;

            ArcGISOnlineEnvironment.ArcGISOnline.Content.GetComments(wrapper.Content.Id, (object sender2, CommentEventArgs e2) =>
            {
                if (e2.Error != null || e2.Comments == null || e2.Comments.Length == 0)
                    return;

                //sort comments by date since chronological order is not guaranteed by agol response
                //newer comments come first in the list
                //
                List<Comment> sortedComments = new List<Comment>(e2.Comments);
                sortedComments.Sort(CompareCommentsByDate);

                ObservableCollection<BindingWrapper<Comment>> wrappers = new ObservableCollection<BindingWrapper<Comment>>();
                foreach (Comment comment in sortedComments)
                {
                    BindingWrapper<Comment> commentWrapper = new BindingWrapper<Comment>();
                    commentWrapper.Content = comment;

                    User user = ArcGISOnlineEnvironment.ArcGISOnline.User.Current;
                    commentWrapper.Tag = Visibility.Collapsed;
                    wrappers.Add(commentWrapper);
                }
                CommentsListBox.ItemsSource = wrappers;
                CommentsListBox.Visibility = Visibility.Visible;
            });
        }

        /// <summary>
        /// Helper callback used to sort a collection of comments chronologically.
        /// </summary>
        static int CompareCommentsByDate(Comment c1, Comment c2)
        {
            return c2.Created.CompareTo(c1.Created);
        }
    }

    /// <summary>
    /// Contains state that UI elements of the MapDetailsControl bind to.
    /// </summary>
    public class MapDetailsBindingWrapper<T> : BindingWrapper<T>
    {
        double _ratingValue;
        string _numberOfRatings;
        Visibility _editButtonVisibility = Visibility.Collapsed;
        Visibility _addCommentButtonVisibility = Visibility.Collapsed;
        Visibility _rateButtonVisibility = Visibility.Collapsed;

        /// <summary>
        /// The rating value for the Rating control.
        /// </summary>
        public double RatingValue
        {
            get { return _ratingValue; }
            set
            {
                _ratingValue = value;
                NotifyPropertyChanged("RatingValue");
            }
        }

        /// <summary>
        /// A string that represents tha number of ratings.
        /// </summary>
        public string NumberOfRatings
        {
            get { return _numberOfRatings; }
            set
            {
                _numberOfRatings = value;
                NotifyPropertyChanged("NumberOfRatings");
            }
        }

        /// <summary>
        /// Determines if the edit buttons for UI elements on the MapDetailsControl are visible or not.
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
        /// Determines if the Add Comment button is visible or not.
        /// </summary>
        public Visibility AddCommentButtonVisibility
        {
            get { return _addCommentButtonVisibility; }
            set
            {
                _addCommentButtonVisibility = value;
                NotifyPropertyChanged("AddCommentButtonVisibility");
            }
        }

        /// <summary>
        /// Determines if the Rate button is visible or not.
        /// </summary>
        public Visibility RateButtonVisibility
        {
            get { return _rateButtonVisibility; }
            set
            {
                _rateButtonVisibility = value;
                NotifyPropertyChanged("RateButtonVisibility");
            }
        }
    }
}
