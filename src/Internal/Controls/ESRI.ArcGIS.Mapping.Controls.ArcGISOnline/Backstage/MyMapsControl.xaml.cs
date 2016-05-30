/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ESRI.ArcGIS.Mapping.Controls;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// Implements the panel in the BackStage that is used to browse My Maps.
  /// </summary>
  public partial class MyMapsControl : Control
  {
    bool _initialized = false;
    bool _isDirty = true;

    /// <summary>
    /// Creates the OpenMapControl.
    /// </summary>
    public MyMapsControl()
    {
        DefaultStyleKey = typeof(MyMapsControl);
        OwnerCommand = new DelegateCommand(OwnerButton_Click);
        DetailsCommand = new DelegateCommand(MoreDetailsButton_Click);
        OpenCommand = new DelegateCommand(OpenButton_Click);
    }

    public ICommand OwnerCommand { get; set; }
    public ICommand DetailsCommand { get; set; }
    public ICommand OpenCommand { get; set; }

    MapDetailsControl MapDetailsControl;
    ListBox ResultsListBox;
    TextBlock SearchResultsTextBlock;
    DataPager DataPager;

    public override void OnApplyTemplate()
    {
        if (MapDetailsControl != null)
        {
            MapDetailsControl.MapDetailsChanged -= RaiseMapDetailsChanged;
            MapDetailsControl.MapSelectedForOpening -= RaiseMapSelectedForOpening;
        }

        if (ResultsListBox != null)
            ResultsListBox.SelectionChanged -= ResultListBox_SelectionChanged;

        base.OnApplyTemplate();

        MapDetailsControl = GetTemplateChild("MapDetailsControl") as MapDetailsControl;
        ResultsListBox = GetTemplateChild("ResultsListBox") as ListBox;
        SearchResultsTextBlock = GetTemplateChild("SearchResultsTextBlock") as TextBlock;
        DataPager = GetTemplateChild("DataPager") as DataPager;

        if (MapDetailsControl != null)
        {
            MapDetailsControl.MapDetailsChanged += RaiseMapDetailsChanged;
            MapDetailsControl.MapSelectedForOpening += RaiseMapSelectedForOpening;
        }

        if (ResultsListBox != null)
        {
            ResultsListBox.SelectionChanged += ResultListBox_SelectionChanged;
            ResultsListBox.DataContext = this;
        }
        if (_isDirty)
            GenerateResults();
    }

    public event EventHandler<ContentItemEventArgs> MapSelectedForOpening;

    void RaiseMapSelectedForOpening(object sender, ContentItemEventArgs e)
    {
      if (MapSelectedForOpening != null)
        MapSelectedForOpening(this, e);
    }

    /// <summary>
    /// Raised by the ArcGISOnline when the user signs in or out.
    /// </summary>
    void ArcGISOnline_SignedInOut(object sender, EventArgs e)
    {
      if (Visibility == Visibility.Visible)
        GenerateResults();
      else
        _isDirty = true; //just set the dirty flag for next time the page is opened
    }

    /// <summary>
    /// Occurs when the Open button is clicked.
    /// </summary>
    void OpenButton_Click(object sender)
    {
      ListBoxItem lbi = Util.GetParentOfType<ListBoxItem>(sender as DependencyObject);
      lbi.IsSelected = true;

      RaiseMapSelectedForOpening(this, new ContentItemEventArgs()
      {
        Item = ResultsListBox.SelectedItem as ContentItem
      });
    }

    public object Items
    {
        get { return (object)GetValue(ItemsProperty); }
        set { SetValue(ItemsProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register("Items", typeof(object), typeof(MyMapsControl), null);

    
    /// <summary>	
    /// Peforms the previous search again.
    /// </summary>
    private void GenerateResults()
    {
        if (SearchResultsTextBlock != null && DataPager != null)
        {
            SearchResultsTextBlock.Text = "";
            DataPager.Visibility = Visibility.Collapsed;
        }
      Items = null;
      if (ArcGISOnlineEnvironment.ArcGISOnline.User.IsSignedIn)
        ArcGISOnlineEnvironment.ArcGISOnline.Content.Search(ArcGISOnlineEnvironment.WebMapTypeQualifier + " AND owner:" + ArcGISOnlineEnvironment.ArcGISOnline.User.Current.Username, (object sender, ContentSearchEventArgs e) =>
          {
            if (e.Error != null)
              return;

            _isDirty = false;

            Items = new PagedSearchResult(e.Result);
            DataPager.Visibility = DataPager.PageCount > 1 ? Visibility.Visible : Visibility.Collapsed;
            ResultsListBox.Visibility = e.Result.TotalCount > 0 ? Visibility.Visible : Visibility.Collapsed;

            //select first item in list and show/hide map details depending on 
            //if it was previously visible
            //
            if (e.Result.Items != null && e.Result.Items.Length > 0)
            {
              SearchResultsTextBlock.Text = string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.MyMapControlMaps, e.Result.TotalCount.ToString());
              ResultsListBox.SelectedItem = e.Result.Items[0];
              ResultsListBox.ScrollIntoView(e.Result.Items[0]);
              if (MapDetailsControl.Visibility == Visibility.Visible)
                MapDetailsControl.Activate(e.Result.Items[0]);
            }
            else
            {
              MapDetailsControl.Visibility = Visibility.Collapsed;
              SearchResultsTextBlock.Text = ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.MyMapControlZeroMaps;
            }
          });
    }


    internal void Clear()
    {
        _initialized = false;
        _isDirty = true;
        if (SearchResultsTextBlock != null && DataPager != null)
        {
            SearchResultsTextBlock.Text = "";
            DataPager.Visibility = Visibility.Collapsed;
        }
        Items = null;
        if (MapDetailsControl != null)
            MapDetailsControl.Visibility = Visibility.Collapsed;
    }
    /// <summary>
    /// Activates the panel, initializing the selection if necessary.
    /// </summary>
    internal void Activate()
    {
      if (!_initialized)
      {
        _initialized = true;

        ArcGISOnlineEnvironment.ArcGISOnline.User.SignedInOut += ArcGISOnline_SignedInOut;
      }

      if (_isDirty)
          GenerateResults();
    }

    void RaiseMapDetailsChanged(object sender, EventArgs e)
    {
      //set a flag to refresh the page next time it is activated
      _isDirty = true;

      if (MapDetailsChanged != null)
        MapDetailsChanged(null, EventArgs.Empty);
    }

    /// <summary>
    /// Raised when the details of a map have been modified.
    /// E.g. title, thumbnail, summary, tags, description etc..
    /// </summary>
    public event EventHandler MapDetailsChanged;


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
    internal void MoreDetailsButton_Click(object sender)
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
    private void ResultListBox_SelectionChanged(object sender, EventArgs e)
    {
      //update the item displayed in the detailed MapDetailsControl
      if (ResultsListBox.SelectedItem != null)
        MapDetailsControl.Activate((ContentItem)ResultsListBox.SelectedItem);
    }
  }

}
