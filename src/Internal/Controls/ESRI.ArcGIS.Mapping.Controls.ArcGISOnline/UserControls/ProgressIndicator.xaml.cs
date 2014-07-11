/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// Simple control that indicates activity by animating with a spinning effect.
  /// </summary>
  public partial class ProgressIndicator : Control
  {
    DispatcherTimer _timer = new DispatcherTimer();
    int _current = 0;

    /// <summary>
    /// Creates a new ProgressIndictator.
    /// </summary>
    public ProgressIndicator()
    {
        DefaultStyleKey = typeof(ProgressIndicator);
      _timer.Interval = new TimeSpan(0, 0, 0, 0, 250);
      _timer.Tick += new EventHandler(_timer_Tick);
      if (Visibility == Visibility.Visible)
        _timer.Start();
    }

    Grid LayoutRoot;
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        LayoutRoot = GetTemplateChild("LayoutRoot") as Grid;
    }

    /// <summary>
    /// Occurs when the timer fires. Hides the current image and shows the next.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void _timer_Tick(object sender, EventArgs e)
    {
      if (LayoutRoot == null) return;
      int previous = _current;
      _current = (_current + 1) % LayoutRoot.Children.Count;

      LayoutRoot.Children[_current].Visibility = Visibility.Visible;
      LayoutRoot.Children[previous].Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Controls the visibility of the ProgressIndicator and starts/stops the timer accordingly.
    /// </summary>
    public new Visibility Visibility
    {
      get { return base.Visibility; }
      set
      {
        if (base.Visibility == value)
          return;

        base.Visibility = value;
        if (value == Visibility.Visible)
          _timer.Start();
        else
          _timer.Stop();
      }
    }
  }
}
