/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Threading;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Toolkit.Primitives;

namespace ESRI.ArcGIS.Mapping.Controls.MapContents
{
    public class RenameLayerOnDoubleClickBehavior : Behavior<FrameworkElement>
    {
        private bool doubleClick = false;
        private bool lazyClick = false;
        private bool isSelected = false;
        private DispatcherTimer timer;
        private int doubleClickInterval = 400;

        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(doubleClickInterval);
                timer.Tick += new EventHandler(timer_Tick);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            if ((lazyClick || !doubleClick) && isSelected)
            //if user invoked lazyClick or
            //if it has already been selected and is now a single click
            {
                Grid grid = AssociatedObject.Parent as Grid;
                if (grid == null)
                    return;

                StackPanel sp = grid.FindName("layerStackPanel") as StackPanel;
                if (sp == null)
                    return;

                sp.SetValue(CoreExtensions.IsEditProperty, true);
            }
            doubleClick = false; //not a doubleclick as time ellapsed
        }

        private void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!doubleClick) //single click
            {
                timer.Start();
                doubleClick = true; //mark as doubleclick until timer ellapses

                if (this.AssociatedObject != null)
                {
                    LayerItemViewModel layerViewModel = AssociatedObject.DataContext as LayerItemViewModel;
                    if (layerViewModel != null)
                    {
                        if ((MapApplication.Current != null && !MapApplication.Current.IsEditMode) ||
                            layerViewModel.Layer == null ||
                            (layerViewModel.Layer != null && LayerExtensions.GetInitialUpdateFailed(layerViewModel.Layer)))
                            return;

                        isSelected = MapApplication.Current != null ? MapApplication.Current.SelectedLayer == layerViewModel.Layer : false;//has already been selected programmatically (non-user selection)
                        if (layerViewModel.Layer != null)
                        {
                            lazyClick = isSelected; //if user selects between multiple layers on map contents, reset lazyclick
                        }
                    }
                }
            }
            else //double click
            {
                lazyClick = false;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
                timer.Stop();
                timer = null;
            }
        }
    }
}
