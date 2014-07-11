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
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Portal;
using System.Linq;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
    /// <summary>
    /// Implements the panel in the BackStage that is used to browse featured maps.
    /// </summary>
    public partial class FeaturedMapsControl : Control
    {
        bool _initialized = false;

        /// <summary>
        /// Creates the FeaturedMapsControl.
        /// </summary>
        public FeaturedMapsControl()
        {
            DefaultStyleKey = typeof(FeaturedMapsControl);
            OwnerCommand = new DelegateCommand(OwnerButton_Click);
            DetailsCommand = new DelegateCommand(MoreDetailsButton_Click);
            OpenCommand = new DelegateCommand(OpenButton_Click);

            // Re-activate control (re-initialize maps) if control has already been initialized
            // and AGOL is re-initialized.  This can happen, for example, if a user signs in or out.
            if (ArcGISOnlineEnvironment.ArcGISOnline != null)
            {
                ArcGISOnlineEnvironment.ArcGISOnline.Initialized += (o, e) =>
                {
                    if (_initialized)
                    {
                        _initialized = false;
                        Activate();
                    }
                };
            }
        }

        public ICommand OwnerCommand { get; set; }
        public ICommand DetailsCommand { get; set; }
        public ICommand OpenCommand { get; set; }

        MapDetailsControl MapDetailsControl;
        ListBox ResultsListBox;

        public override void OnApplyTemplate()
        {
            if (ResultsListBox != null)
                ResultsListBox.SelectionChanged -= ResultListBox_SelectionChanged;
            if (MapDetailsControl != null)
            {
                MapDetailsControl.MapDetailsChanged -= RaiseMapDetailsChanged;
                MapDetailsControl.MapSelectedForOpening -= RaiseMapSelectedForOpening;
            }
            base.OnApplyTemplate();
            MapDetailsControl = GetTemplateChild("MapDetailsControl") as MapDetailsControl;
            ResultsListBox = GetTemplateChild("ResultsListBox") as ListBox;

            if (ResultsListBox != null)
            {
                ResultsListBox.SelectionChanged += ResultListBox_SelectionChanged;
                ResultsListBox.Tag = this;
            }

            if (MapDetailsControl != null)
            {
                MapDetailsControl.MapDetailsChanged += RaiseMapDetailsChanged;
                MapDetailsControl.MapSelectedForOpening += RaiseMapSelectedForOpening;
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
        /// Occurs when the Open button is clicked.
        /// </summary>
        private void OpenButton_Click(object sender)
        {
            ListBoxItem lbi = Util.GetParentOfType<ListBoxItem>(sender as DependencyObject);
            lbi.IsSelected = true;

            RaiseMapSelectedForOpening(this, new ContentItemEventArgs() { Item = ResultsListBox.SelectedItem as ContentItem });
        }

        public event EventHandler<ContentItemEventArgs> MapSelectedForOpening;

        void RaiseMapSelectedForOpening(object sender, ContentItemEventArgs e)
        {
            if (MapSelectedForOpening != null)
                MapSelectedForOpening(this, e);
        }

        internal void Clear()
        {
            _initialized = false;
            DataContext = null;
            if (MapDetailsControl != null)
                MapDetailsControl.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Activates the panel, initializing the selection if necessary.
        /// </summary>
        internal void Activate()
        {
            if (_initialized)
                return;

            Clear();

            _initialized = true;
            LoadingMaps = true;

            // Retrieve featured maps based on featuredItemsGroupQuery
            if (ArcGISOnlineEnvironment.ArcGISOnline.PortalInfo != null
            && !string.IsNullOrEmpty(ArcGISOnlineEnvironment.ArcGISOnline.PortalInfo.FeaturedItemsGroupQuery))
            {
                // Get the group specified by the query
                ArcGISOnlineEnvironment.ArcGISOnline.Group.Search(
                    ArcGISOnlineEnvironment.ArcGISOnline.PortalInfo.FeaturedItemsGroupQuery,
                    (sender, e) =>
                    {
                        // Result will be null if connection to AGOL/ArcGIS portal endpoint fails
                        if (e.Result != null && e.Result.Items != null && e.Result.Items.Length > 0)
                        {
                            // Get the items from the group
                            string query = string.Format("group:{0}", e.Result.Items[0].Id);
                            ArcGISOnlineEnvironment.ArcGISOnline.Content.Search(query, "", 0, 100, (sender2, e2) =>
                              {
                                  if (e2.Error != null)
                                  {
                                      LoadingMaps = false;
                                      return;
                                  }

                                  // Add the group's web maps
                                  ObservableCollection<ContentItem> featuredMaps = new ObservableCollection<ContentItem>();
                                  if (e2.Result != null && e2.Result.Items != null)
                                  {
                                      foreach (ContentItem item in e2.Result.Items)
                                          if (item.Type == "Web Map")
                                              featuredMaps.Add(item);
                                  }
                                  DataContext = featuredMaps;
                                  LoadingMaps = false;
                              });
                        }
                        else
                        {
                            LoadingMaps = false;
                        }
                    });
            }
            else
            {
                LoadingMaps = false;
            }
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
        /// Occurs when the More Details button is clicked. Shows more details of a map in the MapDetailsControl in the Backstage.
        /// </summary>
        private void MoreDetailsButton_Click(object sender)
        {
            HyperlinkButton button = (HyperlinkButton)sender;
            ListBoxItem lbi = Util.GetParentOfType<ListBoxItem>(button);

            ResultsListBox.SelectedItem = lbi.DataContext;
            MapDetailsControl.Visibility = Visibility.Visible;
            MapDetailsControl.Activate((ContentItem)lbi.DataContext);
        }

        /// <summary>
        /// Occurs when the selection in the ResultListBox changes.
        /// </summary>
        private void ResultListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //update the item displayed in the detailed MapDetailsControl
            if (ResultsListBox.SelectedItem != null && MapDetailsControl.Visibility == Visibility.Visible)
                MapDetailsControl.Activate((ContentItem)ResultsListBox.SelectedItem);
        }

        /// <summary>
        /// Backing dependency property for <see cref="LoadingMaps"/>
        /// </summary>
        private static readonly DependencyProperty LoadingMapsProperty = DependencyProperty.Register(
            "LoadingMaps", typeof(bool), typeof(FeaturedMapsControl), null);

        /// <summary>
        /// Gets or sets whether the set of maps is being loaded
        /// </summary>
        private bool LoadingMaps
        {
            get { return (bool)GetValue(LoadingMapsProperty); }
            set { SetValue(LoadingMapsProperty, value); }
        }
    }
}
