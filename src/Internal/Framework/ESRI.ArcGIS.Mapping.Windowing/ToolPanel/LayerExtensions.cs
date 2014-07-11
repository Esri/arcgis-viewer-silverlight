/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ESRI.ArcGIS.Client.Extensibility
{
    public class LayerExtensions
    {
        #region InitialUpdateCompleted
        public static readonly DependencyProperty InitialUpdateCompletedProperty = DependencyProperty.RegisterAttached("InitialUpdateCompleted", typeof(bool), typeof(Layer), null);
        public static void SetInitialUpdateCompleted(Layer layer, bool value)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            layer.SetValue(ESRI.ArcGIS.Client.Extensibility.LayerExtensions.InitialUpdateCompletedProperty, value);
        }
        public static bool GetInitialUpdateCompleted(Layer layer)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            return (bool)layer.GetValue(ESRI.ArcGIS.Client.Extensibility.LayerExtensions.InitialUpdateCompletedProperty);
        }
        #endregion

        #region InitialUpdateFailed
        public static readonly DependencyProperty InitialUpdateFailedProperty = DependencyProperty.RegisterAttached("InitialUpdateFailed", typeof(bool), typeof(Layer), null);
        public static void SetInitialUpdateFailed(Layer layer, bool value)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            layer.SetValue(ESRI.ArcGIS.Client.Extensibility.LayerExtensions.InitialUpdateFailedProperty, value);
        }
        public static bool GetInitialUpdateFailed(Layer layer)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            return (bool)layer.GetValue(ESRI.ArcGIS.Client.Extensibility.LayerExtensions.InitialUpdateFailedProperty);
        }
        #endregion

        #region ErrorMessage
        public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.RegisterAttached("ErrorMessage", typeof(string), typeof(Layer), null);
        public static void SetErrorMessage(Layer layer, string value)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            layer.SetValue(ESRI.ArcGIS.Client.Extensibility.LayerExtensions.ErrorMessageProperty, value);
        }
        public static string GetErrorMessage(Layer layer)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            return (string)layer.GetValue(ESRI.ArcGIS.Client.Extensibility.LayerExtensions.ErrorMessageProperty);
        }
        #endregion
    }
}
