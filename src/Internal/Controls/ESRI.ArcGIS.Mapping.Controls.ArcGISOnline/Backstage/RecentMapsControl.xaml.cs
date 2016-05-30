/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
  /// Represents a view of the most recently used maps.
  /// </summary>
  public partial class RecentMapsControl : Control
  {
    public RecentMapsControl()
    {
        DefaultStyleKey = typeof(RecentMapsControl);
        OpenCommand = new DelegateCommand(open);
        OwnerCommand = new DelegateCommand(owner);
    }
    ListBox RecentMapsListBox;
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        RecentMapsListBox = GetTemplateChild("RecentMapsListBox") as ListBox;
        if (RecentMapsListBox != null)
        {
            RecentMapsListBox.ItemsSource = MRUMaps.Items;
            RecentMapsListBox.DataContext = this;
        }
    }

    internal void Clear()
    {
        MRUMaps.Clear();
        if (RecentMapsListBox != null)
        {
            RecentMapsListBox.ItemsSource = null;
            RecentMapsListBox.DataContext = null;
        }
    }
    public void Activate()
    {
        if (RecentMapsListBox != null)
            RecentMapsListBox.ItemsSource = MRUMaps.Items;
    }

    private void open(object commandParameter)
    {
        if (commandParameter is DependencyObject)
        {
            ListBoxItem lbi = Util.GetParentOfType<ListBoxItem>((DependencyObject)commandParameter);
            RaiseMapSelectedForOpening(this, new ContentItemEventArgs() { Item = (ContentItem)lbi.DataContext });
        }
    }
    public ICommand OpenCommand { get; set; }

    public event EventHandler<ContentItemEventArgs> MapSelectedForOpening;

    void RaiseMapSelectedForOpening(object sender, ContentItemEventArgs e)
    {
      if (MapSelectedForOpening != null)
        MapSelectedForOpening(this, e);
    }


    /// <summary>
    /// Occurs when an owner hyperlink button is clicked - performs a search
    /// for other maps from that owner.
    /// </summary>
    private void owner(object commandParameter)
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
    public ICommand OwnerCommand { get; set; }
  }
}
