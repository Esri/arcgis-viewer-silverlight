/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
using System.ComponentModel;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public abstract class LayerCommandBase : CommandBase
    {
        protected LayerCommandBase()
        {
            if(MapApplication.Current != null)
                MapApplication.Current.SelectedLayerChanged += SelectedLayer_Changed;

            if (ViewerApplicationControl.Instance != null)
            {
                ViewerApplicationControl.Instance.ViewInitialized += ViewInitialized;
                ViewerApplicationControl.Instance.ViewDisposed += ViewDisposed;
            }
        }

        ~LayerCommandBase()
        {
            if (MapApplication.Current != null)
                MapApplication.Current.SelectedLayerChanged -= SelectedLayer_Changed;
            if (ViewerApplicationControl.Instance != null)
            {
                ViewerApplicationControl.Instance.ViewInitialized -= ViewInitialized;
				ViewerApplicationControl.Instance.ViewDisposed -= ViewDisposed;
            }
        }

        void ViewInitialized(object sender, ViewerApplicationControl.ViewEventArgs e)
        {
            if (e != null && e.View != null)
                e.View.SelectedLayerChanged += SelectedLayer_Changed;
        }
        void ViewDisposed(object sender, ViewerApplicationControl.ViewEventArgs e)
        {
            if (e != null && e.View != null)
                e.View.SelectedLayerChanged -= SelectedLayer_Changed;
        }

        void SelectedLayer_Changed(object sender, EventArgs e)
        {
            Application.Current.RootVisual.Dispatcher.BeginInvoke(() =>
            {
                this.Layer = MapApplication.Current.SelectedLayer;
                OnLayerChanged(sender, e);
            });
        }

        private Layer layer;
        public Layer Layer
        {
            get
            {
                if (MapApplication.Current == null && DesignerProperties.IsInDesignTool)
                    return new FeatureLayer() { Url = "http://serverapps.esri.com/ArcGIS/rest/services/California/MapServer/0" };
                else if (layer == null)
                    return MapApplication.Current.SelectedLayer;
                return layer;
            }
            set
            {
                if (layer != null)
                    layer.PropertyChanged -= layer_PropertyChanged;

                layer = value;

                if (layer != null)
                    layer.PropertyChanged += layer_PropertyChanged;

                OnLayerChanged(this, null);
            }
        }

        void layer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Visible"))
                OnLayerChanged(this, null);
        }

        protected virtual void OnLayerChanged(object sender, EventArgs e)
        {
            RaiseCanExecuteChangedEvent(sender, e);
        }
    }
}
