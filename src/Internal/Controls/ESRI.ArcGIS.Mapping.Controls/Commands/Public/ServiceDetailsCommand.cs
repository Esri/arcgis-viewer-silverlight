/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Toolkit.DataSources;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
    [DisplayName("ServiceDetailsDisplayName")]
	[Category("CategoryLayer")]
	[Description("ServiceDetailsDescription")]
    public class ServiceDetailsCommand : LayerCommandBase
    {  
        public override bool CanExecute(object parameter)
        {
            return (Layer is FeatureLayer  && !string.IsNullOrEmpty(((FeatureLayer)Layer).Url)) 
                || Layer is ArcGISDynamicMapServiceLayer 
                || Layer is ArcGISImageServiceLayer 
                || Layer is ArcGISTiledMapServiceLayer
                || Layer is GeoRssLayer 
                || Layer is WmsLayer 
                || Layer is OpenStreetMapLayer 
                || Layer is ESRI.ArcGIS.Client.Bing.TileLayer
                || canNavigateToLayer();
        }

        private bool canNavigateToLayer()
        {
            ICustomGraphicsLayer customGraphicsLayer = Layer as ICustomGraphicsLayer;
            if (customGraphicsLayer != null)
                return customGraphicsLayer.SupportsNavigateToServiceDetailsUrl();
            return false;
        }

        public override void Execute(object parameter)
        {
            FeatureLayer fl = Layer as FeatureLayer;
            if (fl != null)
            {
                navigateToUrl(Utility.CreateUriWithProxy(fl.ProxyUrl, fl.Url));
                return;
            }

            ArcGISDynamicMapServiceLayer dynLayer = Layer as ArcGISDynamicMapServiceLayer;
            if (dynLayer != null)
            {
                navigateToUrl(Utility.CreateUriWithProxy(dynLayer.ProxyURL, dynLayer.Url));
                return;
            }

            ArcGISImageServiceLayer imageLayer = Layer as ArcGISImageServiceLayer;
            if (imageLayer != null)
            {
                navigateToUrl(Utility.CreateUriWithProxy(imageLayer.ProxyURL, imageLayer.Url));
                return;
            }

            ArcGISTiledMapServiceLayer tiledLayer = Layer as ArcGISTiledMapServiceLayer;
            if (tiledLayer != null)
            {
                navigateToUrl(Utility.CreateUriWithProxy(tiledLayer.ProxyURL, tiledLayer.Url));
                return;
            }

            GeoRssLayer geoRssLayer = Layer as GeoRssLayer;
            if (geoRssLayer != null)
            {
                if (geoRssLayer.Source != null)
                    navigateToUrl(geoRssLayer.Source.AbsoluteUri);
                return;
            }

            WmsLayer wmsLayer = Layer as WmsLayer;
            if (wmsLayer != null)
            {
                string url = wmsLayer.Url;
                if (!string.IsNullOrEmpty(url))
                {
                    if (!url.Contains("?"))
                        url += "?";
                    else
                        url += "&";
                    if (url.IndexOf("request=GetCapabilities", StringComparison.OrdinalIgnoreCase) < 0)
                        url += "request=GetCapabilities";
                    if (!string.IsNullOrEmpty(wmsLayer.Version) && url.IndexOf("version=", StringComparison.OrdinalIgnoreCase) < 0)
                        url += "&version=" + wmsLayer.Version;
                    navigateToUrl(Utility.CreateUriWithProxy(wmsLayer.ProxyUrl, url));
                }
                return;
            }

            OpenStreetMapLayer osmLayer = Layer as OpenStreetMapLayer;
            if (osmLayer != null)
            {
                navigateToUrl("http://www.openstreetmap.org");
                return;
            }

            ESRI.ArcGIS.Client.Bing.TileLayer bingLayer = Layer as ESRI.ArcGIS.Client.Bing.TileLayer;
            if (bingLayer != null)
            {
                navigateToUrl("http://www.bing.com/maps/");
                return;
            }           

            ICustomGraphicsLayer customGraphicsLayer = Layer as ICustomGraphicsLayer;
            if (customGraphicsLayer != null && customGraphicsLayer.SupportsNavigateToServiceDetailsUrl())
            {
                Uri uri = customGraphicsLayer.GetServiceDetailsUrl();
                if (uri != null)
                    navigateToUrl(uri.AbsoluteUri);
                return;
            }
        }

        private void navigateToUrl(string url)
        {
            System.Windows.Browser.HtmlPage.Window.Navigate(new Uri(url, UriKind.Absolute), "_blank");
        }
        private void navigateToUrl(Uri uri)
        {
            System.Windows.Browser.HtmlPage.Window.Navigate(uri, "_blank");
        }
    }
}
