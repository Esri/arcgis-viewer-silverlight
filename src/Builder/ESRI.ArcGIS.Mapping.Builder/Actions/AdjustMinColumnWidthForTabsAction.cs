/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class AdjustMinColumnWidthForTabsAction : TargetedTriggerAction<Grid>
    {
        protected override void Invoke(object parameter)
        {
            adjustColumnMinWidth();
        }

        #region Column

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Column"/> property
        /// </summary>
        public static readonly DependencyProperty ColumnProperty = DependencyProperty.Register(
            "Column", typeof(int), typeof(AdjustMinColumnWidthForTabsAction),
            new PropertyMetadata(-1, OnColumnChanged));

        /// <summary>
        /// Gets or sets the index of the column to adjust the minimum width of
        /// </summary>
        public int Column
        {
            get { return (int)GetValue(ColumnProperty); }
            set { SetValue(ColumnProperty, value); }
        }

        private static void OnColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // redo column width adjustment
            ((AdjustMinColumnWidthForTabsAction)d).adjustColumnMinWidth();
        }

        #endregion

        #region TabControl

        /// <summary>
        /// Backing DependencyProperty for the <see cref="TabControl"/> property
        /// </summary>
        public static readonly DependencyProperty TabControlProperty = DependencyProperty.Register(
            "TabControl", typeof(TabControl), typeof(AdjustMinColumnWidthForTabsAction),
            new PropertyMetadata(OnTabControlChanged));

        /// <summary>
        /// Gets or sets the <see cref="TabControl"/> on which to base auto-fitting
        /// </summary>
        public TabControl TabControl
        {
            get { return GetValue(TabControlProperty) as TabControl; }
            set { SetValue(TabControlProperty, value); }
        }

        private static void OnTabControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AdjustMinColumnWidthForTabsAction behavior = (AdjustMinColumnWidthForTabsAction)d;

            if (e.NewValue is TabControl)
            {
                TabControl tabControl = (TabControl)e.NewValue;

                // Calculate and apply column width
                behavior.adjustColumnMinWidth();
            }
        }

        #endregion

        // Calculates and applies the minimum width necessary to accommodate the tabs in the 
        // specified tab control
        private void adjustColumnMinWidth()
        {
            if (Target == null || TabControl == null || Column < 0 
            || Column >= Target.ColumnDefinitions.Count 
            || TabControl.Items.Count == 0)
                return;

            ColumnDefinition cDef = Target.ColumnDefinitions[Column];

            // Difference between column width and tab control width. This difference needs to be
            // preserved when calculating the new column width.
            double tabControlMargin = cDef.ActualWidth - TabControl.ActualWidth;

            double aggregateTabWidths = 0;
            double tabPanelMargin = 0;

            // Calculate width for each tab
            for (int i = 0; i < TabControl.Items.Count; i++)
            {
                TabItem tab = TabControl.Items[i] as TabItem;
                if (tab == null)
                    return; // Unexpected object type - bail out

                double threshholdWidth = double.IsNaN(tab.MinWidth) ? 0 : tab.MinWidth;
                if (tab.ActualWidth > threshholdWidth)
                {
                    // Aggregate tab width and margin
                    aggregateTabWidths += tab.ActualWidth + tab.Margin.Left + tab.Margin.Right;

                    if (i == 0)
                    {
                        // Get the margin of the panel containing the tabs.  Note that this is the only
                        // dimension on an element external to the tabs that is taken into account, so
                        // if the TabControl's template has been modified to include other elements, this
                        // measurement will not be sufficient.

                        TabPanel tabPanel = tab.FindAncestorOfType<TabPanel>();
                        if (tabPanel != null)
                            tabPanelMargin += tabPanel.Margin.Left + tabPanel.Margin.Right;
                    }
                }
                else
                {
                    return; // nothing to measure - bail out
                }
            }

            // Apply calculated widths as column min width. The constant 2 is applied as this is necessary 
            // to prevent wrapping, but walking the visual tree and measuring could not account for these 
            // 2 pixels
            double newMinWidth = tabControlMargin + tabPanelMargin + aggregateTabWidths + 2;
            if (double.IsNaN(cDef.MinWidth) || newMinWidth > cDef.MinWidth)
                cDef.MinWidth = newMinWidth;
        }
    }
}
