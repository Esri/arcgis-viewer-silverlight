/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client.Toolkit.Primitives;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls.MapContents
{
    public class RenameLayerCompletedBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
                this.AssociatedObject.LostFocus += AssociatedObject_LostFocus;

                this.AssociatedObject.IsEnabledChanged -= AssociatedObject_IsEnabledChanged;
                this.AssociatedObject.IsEnabledChanged += AssociatedObject_IsEnabledChanged;

                this.AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
                this.AssociatedObject.KeyDown += AssociatedObject_KeyDown;
            }
        }

        void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                StackPanel sp = AssociatedObject.Parent as StackPanel;
                if (sp == null)
                    return;

                LayerItemViewModel model = sp.DataContext as LayerItemViewModel;
                if (model == null || model.Layer == null ||
                    (model != null && model.Layer != null &&
                    LayerExtensions.GetInitialUpdateFailed(model.Layer)))
                    return;
                model.Layer.SetValue(MapApplication.LayerNameProperty, AssociatedObject.Text);

                sp.SetValue(CoreExtensions.IsEditProperty, false);
            }
        }

        void AssociatedObject_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
             StackPanel sp = AssociatedObject.Parent as StackPanel;
             if (sp == null)
                return;

            if (e.NewValue is bool && (bool)e.NewValue &&
                AssociatedObject.Visibility == Visibility.Visible)
            {
                if (e.NewValue != e.OldValue)
                {
                    AssociatedObject.Focus();
                    AssociatedObject.SelectAll();
                }
            }
        }

        void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
        {
            StackPanel sp = AssociatedObject.Parent as StackPanel;
            if (sp == null)
                return;

            LayerItemViewModel model = sp.DataContext as LayerItemViewModel;
            if (model == null || model.Layer == null || 
                (model != null && model.Layer != null &&
                LayerExtensions.GetInitialUpdateFailed(model.Layer)))
                return;

            model.Layer.SetValue(MapApplication.LayerNameProperty, AssociatedObject.Text);

            sp.SetValue(CoreExtensions.IsEditProperty, false);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
                this.AssociatedObject.IsEnabledChanged -= AssociatedObject_IsEnabledChanged;
                this.AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
            }
        }
    }
}
