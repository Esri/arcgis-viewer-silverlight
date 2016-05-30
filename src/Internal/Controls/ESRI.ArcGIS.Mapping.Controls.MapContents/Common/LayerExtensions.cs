/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls.MapContents
{
    public class Document
    {
        public static void SetIsBaseMap(DependencyObject obj, bool value)
        {
            if (obj != null)
                obj.SetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty, value);
        }
        public static bool GetIsBaseMap(DependencyObject layer)
        {
            if (layer != null)
                return (bool)layer.GetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty);
            
            return default(bool);
        }
    }

    public class CoreExtensions
    {
        #region IsSelected
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(Layer), null);
        public static void SetIsSelected(Layer layer, bool value)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            layer.SetValue(IsSelectedProperty, value);
        }
        public static bool GetIsSelected(Layer layer)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            return (bool)layer.GetValue(IsSelectedProperty);
        }
        #endregion

        #region IsEdit
        public static readonly DependencyProperty IsEditProperty = DependencyProperty.RegisterAttached("IsEdit", typeof(bool), typeof(UIElement), null);
        public static void SetIsEdit(UIElement uiElement, bool value)
        {
            if (uiElement == null)
            {
                throw new ArgumentNullException("uiElement");
            }
            uiElement.SetValue(IsEditProperty, value);
        }
        public static bool GetIsEdit(UIElement uiElement)
        {
            if (uiElement == null)
            {
                throw new ArgumentNullException("uiElement");
            }
            return (bool)uiElement.GetValue(IsEditProperty);
        }
        #endregion
    }
}
