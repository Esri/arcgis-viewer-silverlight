/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Bing;
using System.Text;
using System.Windows.Browser;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// Provides static utility methods.
  /// </summary>
  public class Util
  {
    /// <summary>
    /// Walks up the visual tree of the specified DependencyObject until a parent
    /// of the specified type is found. Returns the parent of that type.
    /// </summary>
    /// <remarks>
    /// Example: When using an ItemTemplate with a ListBox, use the following in an
    /// event handler for a contained control to get the ListBoxItem.
    /// 
    ///  ListBoxItem lbi = Util.GetParentOfType<ListBoxItem>(sender as DependencyObject);
    /// </remarks>
    public static T GetParentOfType<T>(DependencyObject item) where T : class
    {
      if (item == null)
        return null;

      DependencyObject parent = VisualTreeHelper.GetParent(item);
      if (typeof(T).IsInstanceOfType(parent))
        return parent as T;

      return GetParentOfType<T>(parent);
    }

    /// <summary>
    /// Opens the specified page in the specified browser instance.
    /// </summary>
    /// <remarks>
    /// Work-around for the issue that HtmlPage.Window.Navigate does
    /// not work out-of-browser.
    /// </remarks>
    public static void Navigate(string url, string target)
    {
      new HyperlinkButton2(url, target).ClickMe();
    }

    /// <summary>
    /// Helper class to implement the Navigate method that works out-of-browser.
    /// </summary>
    class HyperlinkButton2 : HyperlinkButton
    {
      public HyperlinkButton2(string url, string targetName)
      {
        base.NavigateUri = new Uri(url, UriKind.Absolute);
        TargetName = targetName;
      }

      public void ClickMe()
      {
        base.OnClick();
      }
    }

      /// <summary>
      /// Removes the textual day format values (e.g. for Monday, Tuesday, etc) from the input format string, along 
      /// with any delimeters
      /// </summary>
      /// <param name="format"></param>
      /// <returns></returns>
    internal static string RemoveDayFormat(string format)
    {
        format = format.Replace("dddd,", "");
        format = format.Replace("dddd.", "");
        format = format.Replace(",dddd", "");
        format = format.Replace(".dddd", "");
        format = format.Replace("dddd", "");
        format = format.Replace("ddd,", "");
        format = format.Replace("ddd.", "");
        format = format.Replace(",ddd", "");
        format = format.Replace(".ddd", "");
        format = format.Replace("ddd", "");

        return format;
    }
  }

  /// <summary>
  /// Simple class used to package a callback delegate and additional data for use with a delagate.
  /// </summary>
  class CallState
  {
    public object Callback { get; set; }
    public object Data { get; set; }
    public object UserState { get; set; }
  }
}
  
