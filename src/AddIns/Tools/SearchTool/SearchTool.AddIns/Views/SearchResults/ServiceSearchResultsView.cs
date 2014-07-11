/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

// ===============================================
// CODE-BEHIND CLASS FOR VIEW - DO NOT ADD CODE
// ===============================================

using System.Windows;
using System.Windows.Controls;

namespace SearchTool
{
    /// <summary>
    /// Provides a UI for ArcGIS Portal and web search results
    /// </summary>
    public class ServiceSearchResultsView : Control
    {
        public ServiceSearchResultsView()
        {
            this.DefaultStyleKey = typeof(ServiceSearchResultsView);
        }

        // Although this is a View class, it is implemented as a control to enable exposing aspects of the
        // View's appearance to styling.  Since there is no logic in these properties, and they only pertain
        // to the appearance of the UI, they do not break with the MVVM pattern.

        /// <summary>
        /// Backing DependencyProperty for the <see cref="ResultDetailsLeaderStyle"/> property
        /// </summary>
        public static DependencyProperty ResultDetailsLeaderStyleProperty = DependencyProperty.Register(
            "ResultDetailsLeaderStyle", typeof(Style), typeof(ServiceSearchResultsView), null);

        /// <summary>
        /// Gets or sets the style of the leader for the popups showing details of search results
        /// </summary>
        public Style ResultDetailsLeaderStyle
        {
            get { return GetValue(ResultDetailsLeaderStyleProperty) as Style; }
            set { SetValue(ResultDetailsLeaderStyleProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="ResultDetailsContainerStyle"/> property
        /// </summary>
        public static DependencyProperty ResultDetailsContainerStyleProperty = DependencyProperty.Register(
            "ResultDetailsContainerStyle", typeof(Style), typeof(ServiceSearchResultsView), null);

        /// <summary>
        /// Gets or sets the style of the container for the popups showing details of search results
        /// </summary>
        public Style ResultDetailsContainerStyle
        {
            get { return GetValue(ResultDetailsContainerStyleProperty) as Style; }
            set { SetValue(ResultDetailsContainerStyleProperty, value); }
        }
    }
}
