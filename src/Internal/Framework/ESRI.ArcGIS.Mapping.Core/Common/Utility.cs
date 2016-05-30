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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Reflection;
using ESRI.ArcGIS.Client;
using System.Linq;

namespace ESRI.ArcGIS.Mapping.Core
{
    public static class Utility
    {
        public static T[] GetEnumValues<T>() where T : struct, IConvertible
        {
            try
            {
                if (!typeof(T).IsEnum)
                {
                    return null;
                }

                Type enumType = typeof(T);
                List<T> enumValues = new List<T>();
                foreach (System.Reflection.FieldInfo fi in enumType.GetFields(BindingFlags.Static | BindingFlags.Public))
                {
                    enumValues.Add((T)Enum.Parse(enumType, fi.Name, false));
                }
                return enumValues.ToArray();
            }
            catch { }
            return null;
        }
        public static bool IsMessageLimitExceededException(Exception ex)
        {
            return ex != null && ex is System.Net.WebException
                && ("The message length limit was exceeded".Equals(ex.Message) // error in developer runtime
                    || ex.Message.IndexOf("MessageLengthLimitExceeded", StringComparison.OrdinalIgnoreCase) > -1);  // error in non-developer runtime
        }

        public static void SetRTL(FrameworkElement element)
        {
            string twoLetterISOLanguageName = System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            if (twoLetterISOLanguageName == "ar" || twoLetterISOLanguageName == "he")
                element.FlowDirection = FlowDirection.RightToLeft;
        }

        /// <summary>
        /// Provides a way to bind a callback used for notification when an attached property 
        /// value changes.  (This method does not need the object to be a FrameworkElement to 
        /// participate).
        /// </summary>
        public static DependencyProperty NotifyOnAttachedPropertyChanged<T>(string propertyName, DependencyObject source,
                                                     PropertyChangedCallback callback, T defaultValue)
        {
            if (source == null)
                return null;

            var b = new Binding(propertyName) { Source = source };
            DependencyProperty prop = DependencyProperty.RegisterAttached(
                "ListenerProperty" + DateTime.Now.Ticks.ToString(),
                typeof(T),
                typeof(DependencyObject),
                new PropertyMetadata(defaultValue, callback));

            BindingOperations.SetBinding(source, prop, b);

            return prop;
        }

        public static void ClearAttachedPropertyListener(DependencyProperty property, DependencyObject source)
        {
            if (source == null || property == null)
                return;

            source.ClearValue(property);
        }

        public static string XamlTextEncode(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            return str.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        public static string UrlEncode(string str)
        {
            return System.Windows.Browser.HttpUtility.UrlEncode(str);
        }
        public static Uri CreateUriWithProxy(string proxyUrl, string url)
        {
            if (!string.IsNullOrEmpty(proxyUrl))
            {
                string proxy = proxyUrl.EndsWith("?", StringComparison.Ordinal) ? proxyUrl : proxyUrl + "?";
                return new System.Uri(proxy + ESRI.ArcGIS.Mapping.Core.Utility.UrlEncode(url));
            }
            return new Uri(url);

        }
        public static Uri CreateUriWithProxy(string proxyUrl, Uri uri)
        {
            return CreateUriWithProxy(proxyUrl, uri.ToString());
        }

        public static IEnumerable<Graphic> FindGraphicsInHostCoordinates(GraphicsLayer layer, Point intersectingPoint, bool includeClustered = false)
        {
            if (layer == null)
                return null;

            IEnumerable<Graphic> graphics = layer.FindGraphicsInHostCoordinates(intersectingPoint);
            FindGraphicsInHostCoordinates(layer, ref graphics, includeClustered);

            return graphics;
        }
        public static IEnumerable<Graphic> FindGraphicsInHostCoordinates(GraphicsLayer layer, Rect intersectingRect, bool includeClustered = false)
        {
            if (layer == null)
                return null;

            IEnumerable<Graphic> graphics = layer.FindGraphicsInHostCoordinates(intersectingRect);
            FindGraphicsInHostCoordinates(layer, ref graphics, includeClustered);

            return graphics;
        }
        private static void FindGraphicsInHostCoordinates(GraphicsLayer layer, ref IEnumerable<Graphic> graphics, bool includeClustered)
        {
            if (layer == null)
                return;

            if (layer.Clusterer == null)
                return;

            List<Graphic> matchedGraphics = new List<Graphic>();
            //try finding the mapping graphic of the clustered dgraphic
            FeatureLayer fLayer = layer as FeatureLayer;
            if (fLayer != null && fLayer.LayerInfo != null && !string.IsNullOrWhiteSpace(fLayer.LayerInfo.ObjectIdField))
            {
                //try oid matching for feature layer
                foreach (Graphic g in graphics)
                {
                    if (g.GetType().ToString() == "ESRI.ArcGIS.Client.Clusterer+DGraphic" &&
                        g.Attributes.ContainsKey(fLayer.LayerInfo.ObjectIdField))
                    {
                        Graphic match = fLayer.Graphics.FirstOrDefault(gr => gr.Attributes.ContainsKey(fLayer.LayerInfo.ObjectIdField) &&
                                                                             gr.Attributes[fLayer.LayerInfo.ObjectIdField] == g.Attributes[fLayer.LayerInfo.ObjectIdField]);
                        
                        matchedGraphics.Add(match != null ? match : g);
                    }
                    else
                        matchedGraphics.Add(g);
                }
            }
            else
            {
                //try attribute matching
                foreach (Graphic g in graphics)
                {
                    Graphic match = null;
                    if (g.GetType().ToString() == "ESRI.ArcGIS.Client.Clusterer+DGraphic")
                        match = layer.Graphics.FirstOrDefault(gr => gr.Attributes.Equals(g.Attributes) &&
                                                                     gr.Geometry.Equals(g.Geometry));
                    
                    matchedGraphics.Add(match != null ? match : g);
                }
            }
            graphics = matchedGraphics;
        }
    }
}
