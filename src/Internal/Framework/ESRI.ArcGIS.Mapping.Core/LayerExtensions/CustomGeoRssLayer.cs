/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using ESRI.ArcGIS.Client;
using System.Xml;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class CustomGeoRssLayer : CustomGraphicsLayer, ICustomLayer
    {
        Client.Toolkit.DataSources.GeoRssLayer geoRssLayer;

        public CustomGeoRssLayer()
        {
            geoRssLayer = new Client.Toolkit.DataSources.GeoRssLayer();
            geoRssLayer.Initialized += geoRssLayer_Initialized;
            LayerSpatialReference = new SpatialReference(4326);
        }

        public override void Initialize()
        {
            geoRssLayer.Initialize();
        }

        void geoRssLayer_Initialized(object sender, EventArgs e)
        {
            OnGraphicsCreated(geoRssLayer.Graphics, true, null);
            base.Initialize();
        }

        protected override void OnUpdateCompleted(EventArgs args)
        {
            IsInitComplete = true;
            base.OnUpdateCompleted(args);

            UpdateCursorForRenderer();
        }

        public Uri Source
        {
            get
            {
                return geoRssLayer.Source;
            }
            set
            {
                geoRssLayer.Source = value;
            }
        }

        public override bool SupportsItemOnClickBehavior
        {
            get
            {
                return true;
            }
        }

        public override void OnItemClicked(Graphic item)
        {
            if (item == null)
                return;
            object o = null;
            if (item.Attributes.TryGetValue("Link", out o))
            {
                Uri targetUri = null;
                if (Uri.TryCreate(o.ToString(), UriKind.Absolute, out targetUri))
                { 
                    System.Windows.Browser.HtmlPage.Window.Navigate(targetUri, "_blank");
                }  
            }
            else if (item.Attributes.TryGetValue("FeedItem", out o))
            {
                System.ServiceModel.Syndication.SyndicationItem feedItem = o as System.ServiceModel.Syndication.SyndicationItem;
                if (feedItem != null)
                {
                    System.Windows.Browser.HtmlPage.Window.Navigate(feedItem.BaseUri, "_blank");     
                }
            }
        }

        // GeoRss layers use graphics layer member composition - hence we need a seperate init property
        public bool IsInitComplete { get; set; }

        public override void ForceRefresh(EventHandler refreshCompletedHander, EventHandler<Client.Tasks.TaskFailedEventArgs> refreshFailedHandler)
        {
            refreshFeed();
            if (refreshCompletedHander != null)
                refreshCompletedHander(this, EventArgs.Empty);
        }

        void disposeOldLayer()
        {
            if (geoRssLayer != null)
            {
                geoRssLayer.Initialized -= geoRssLayer_Initialized;
                geoRssLayer.Graphics.CollectionChanged -= Graphics_CollectionChanged;
                geoRssLayer.Graphics.Clear();
                geoRssLayer.Source = null;
            }
            geoRssLayer = null;
        }

        protected override void CheckForUpdates()
        {
            refreshFeed();
        }

        private void refreshFeed()
        {
            Graphics.Clear();
            
            // add a query parameter that is unique so that it causes
            int idx = geoRssLayer.Source.OriginalString.IndexOf("?_ts=", StringComparison.Ordinal);
            if (idx <= 0)
                idx = geoRssLayer.Source.OriginalString.Length;

            string subUri = geoRssLayer.Source.OriginalString.Substring(0, idx);
            Uri noCacheSource = new Uri(subUri +  string.Format("?_ts={0}", DateTime.Now.Ticks));

            disposeOldLayer();
            geoRssLayer = new Client.Toolkit.DataSources.GeoRssLayer()
            {
                Source = noCacheSource
            };
            geoRssLayer.Initialized += geoRssLayer_Initialized;

            geoRssLayer.Initialize();
        }

        public override void UpdateOnMapExtentChanged(ExtentEventArgs e)
        {
            refreshFeed();
        }

        void Graphics_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null)
                {
                    GraphicCollection newGraphics = new GraphicCollection();
                    foreach (Graphic newGraphic in e.NewItems)
                        newGraphics.Add(newGraphic);
                    OnGraphicsCreated(newGraphics, false, null);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                Graphics.Clear();
            }
        }

        public override Uri GetServiceDetailsUrl()
        {
            return Source;
        }

        public override bool SupportsNavigateToServiceDetailsUrl()
        {
            return true;
        }

        #region ICustomLayer Members

        public void Serialize(XmlWriter writer, Dictionary<string, string> Namespaces)
        {
            CustomGeoRssLayerXamlWriter customGeoRssLayerWriter = new CustomGeoRssLayerXamlWriter(writer, Namespaces);
            customGeoRssLayerWriter.WriteLayer(this, "CustomGeoRssLayer", Constants.esriMappingNamespace);
        }

        #endregion
    }

    internal class CustomGeoRssLayerXamlWriter : GraphicsLayerXamlWriter
    {
        public CustomGeoRssLayerXamlWriter(XmlWriter writer, Dictionary<string, string> namespaces)
            : base(writer, namespaces)
        {

        }

        protected override void WriteAttributes(Layer layer)
        {
            base.WriteAttributes(layer);

            CustomGeoRssLayer geoRssLayer = layer as CustomGeoRssLayer;
            if (geoRssLayer != null)
            {
                base.WriteAttribute("IsItemOnClickBehaviorEnabled", geoRssLayer.IsItemOnClickBehaviorEnabled.ToString());

                if (geoRssLayer.Source != null)
                    writer.WriteAttributeString("Source", geoRssLayer.Source.AbsoluteUri);

                if (geoRssLayer.MapSpatialReference != null)
                    WriteSpatialReferenceAsAttribute(writer, Namespaces, "MapSpatialReference", geoRssLayer.MapSpatialReference);
                if (geoRssLayer.LayerSpatialReference != null)
                    WriteSpatialReferenceAsAttribute(writer, Namespaces, "LayerSpatialReference", geoRssLayer.LayerSpatialReference);
            }
        }

        private void WriteSpatialReferenceAsAttribute(XmlWriter writer, Dictionary<string, string> Namespaces, string propertyName, SpatialReference sRef)
        {
            if (sRef == null)
                return;
            if (!string.IsNullOrEmpty(sRef.WKT))
                writer.WriteAttributeString(propertyName, sRef.WKT);
            else if (sRef.WKID != default(int))
                writer.WriteAttributeString(propertyName, sRef.WKID.ToString());
        }
    }
}
