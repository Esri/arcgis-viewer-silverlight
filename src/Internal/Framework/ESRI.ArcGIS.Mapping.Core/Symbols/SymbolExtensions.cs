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
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Mapping.Core.Symbols;
using System.Windows.Media.Imaging;
using FS = ESRI.ArcGIS.Client.FeatureService.Symbols;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class SymbolExtensions
    {
        #region SymbolXaml
        /// <summary>
        /// Gets the value of the SymbolXaml attached property for a specified Symbol.
        /// </summary>
        /// <param name="element">The Symbol from which the property value is read.</param>
        /// <returns>The SymbolXaml property value for the Symbol.</returns>
        [Obsolete]
        public static string GetSymbolXaml(Symbol element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return element.GetValue(SymbolXamlProperty) as string;
        }

        /// <summary>
        /// Sets the value of the SymbolXaml attached property to a specified Symbol.
        /// </summary>
        /// <param name="element">The Symbol to which the attached property is written.</param>
        /// <param name="value">The needed SymbolXaml value.</param>
        [Obsolete]
        public static void SetSymbolXaml(Symbol element, string value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(SymbolXamlProperty, value);
        }

        /// <summary>
        /// Identifies the SymbolXaml dependency property.
        /// </summary>
        [Obsolete]
        public static readonly DependencyProperty SymbolXamlProperty =
            DependencyProperty.RegisterAttached(
                "SymbolXaml",
                typeof(string),
                typeof(SymbolExtensions),
                new PropertyMetadata(null));
        #endregion 


        #region SymbolScaleX
        /// <summary>
        /// Gets the value of the SymbolScaleX attached property for a specified Symbol.
        /// </summary>
        /// <returns>The SymbolScaleX property value for the Symbol.</returns>
        public static double GetSymbolScaleX(Symbol element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (double)element.GetValue(SymbolScaleXProperty);
        }

        /// <summary>
        /// Sets the value of the SymbolScaleX attached property to a specified Symbol.
        /// </summary>
        /// <param name="value">The needed SymbolScaleX value.</param>
        public static void SetSymbolScaleX(Symbol element, double value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(SymbolScaleXProperty, value);
        }

        /// <summary>
        /// Identifies the SymbolXaml dependency property.
        /// </summary>
        public static readonly DependencyProperty SymbolScaleXProperty =
            DependencyProperty.RegisterAttached(
                "SymbolScaleX",
                typeof(double),
                typeof(PathMarkerSymbol),
                new PropertyMetadata(1d));

        #endregion 

        #region SymbolScaleY
        /// <summary>
        /// Gets the value of the SymbolScaleY attached property for a specified Symbol.
        /// </summary>
        /// <returns>The SymbolScaleY property value for the Symbol.</returns>
        public static double GetSymbolScaleY(Symbol element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (double)element.GetValue(SymbolScaleYProperty);
        }

        /// <summary>
        /// Sets the value of the SymbolScaleY attached property to a specified Symbol.
        /// </summary>
        /// <param name="value">The needed SymbolScaleY value.</param>
        public static void SetSymbolScaleY(Symbol element, double value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(SymbolScaleYProperty, value);
        }

        /// <summary>
        /// Identifies the SymbolXaml dependency property.
        /// </summary>
        public static readonly DependencyProperty SymbolScaleYProperty =
            DependencyProperty.RegisterAttached(
                "SymbolScaleY",
                typeof(double),
                typeof(PathMarkerSymbol),
                new PropertyMetadata(1d));

        #endregion 

        #region public attached string OriginalSource
        /// <summary>
        /// Gets the value of the OriginalSource attached property for a specified PictureMarkerSymbol.
        /// </summary>
        /// <param name="element">The PictureMarkerSymbol from which the property value is read.</param>
        /// <returns>The OriginalSource property value for the PictureMarkerSymbol.</returns>
        public static string GetOriginalSource(PictureMarkerSymbol element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return element.GetValue(OriginalSourceProperty) as string;
        }

        /// <summary>
        /// Sets the value of the OriginalSource attached property to a specified PictureMarkerSymbol.
        /// </summary>
        /// <param name="element">The PictureMarkerSymbol to which the attached property is written.</param>
        /// <param name="value">The needed OriginalSource value.</param>
        public static void SetOriginalSource(PictureMarkerSymbol element, string value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(OriginalSourceProperty, value);
        }

        /// <summary>
        /// Identifies the OriginalSource dependency property.
        /// </summary>
        public static readonly DependencyProperty OriginalSourceProperty =
            DependencyProperty.RegisterAttached(
                "OriginalSource",
                typeof(string),
                typeof(SymbolExtensions),
                new PropertyMetadata(null, OnOriginalSourcePropertyChanged));

        /// <summary>
        /// OriginalSourceProperty property changed handler.
        /// </summary>
        /// <param name="d">SymbolExtensions that changed its OriginalSource.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnOriginalSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PictureMarkerSymbol pms = d as PictureMarkerSymbol;
            if (pms != null)
            {
                string value = e.NewValue as string;
                if(!string.IsNullOrEmpty(value))
                {
                    pms.Source = ImageUrlResolver.ResolveUrlForImage(value);
                }
            }
        }
        #endregion

        public static string GetJson(Symbol obj)
        {
            return (string)obj.GetValue(JsonProperty);
        }

        public static void SetJson(Symbol obj, string value)
        {
            obj.SetValue(JsonProperty, value);
        }

        // Using a DependencyProperty as the backing store for Json.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty JsonProperty =
            DependencyProperty.RegisterAttached("Json", typeof(string), typeof(Symbol), new PropertyMetadata(SetImagePropertiesFromJson));

        private static void SetImagePropertiesFromJson(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            Symbol symbol = o as Symbol;
            string json = GetJson(symbol);
            if (string.IsNullOrEmpty(json))
                return;

            SolidColorBrush selectionColor = new SolidColorBrush(Colors.Cyan);
            FS.PictureFillSymbol pfs = o as FS.PictureFillSymbol;
            if (pfs != null)
            {
                FS.PictureFillSymbol pfsN = Symbol.FromJson(json) as FS.PictureFillSymbol;
                if (pfsN != null)
                {
                    #region Copy Properties
                    pfs.Source = pfsN.Source;
                    //internal properties - for documentation
                    //pfs.Angle = pfsN.Angle;
                    //pfs.XOffset = pfsN.XOffset;
                    //pfs.YOffset = pfsN.YOffset;
                    //pfs.XScale = pfsN.XScale;
                    //pfs.YScale = pfsN.YScale;
                    pfs.BorderBrush = pfsN.BorderBrush;
                    pfs.BorderThickness = pfsN.BorderThickness;
                    pfs.BorderStyle = pfsN.BorderStyle;
                    pfs.Width = pfsN.Width;
                    pfs.Height = pfsN.Height;
                    pfs.Color = pfsN.Color;
                    pfs.Opacity = 0.75;
                    pfs.SelectionColor = selectionColor;
                    #endregion
                }
                return;
            }
            FS.PictureMarkerSymbol pms = o as FS.PictureMarkerSymbol;
            if (pms != null)
            {
                FS.PictureMarkerSymbol pmsN = Symbol.FromJson(json) as FS.PictureMarkerSymbol;
                if (pmsN != null)
                {
                    #region Copy Properties
                    pms.Source = pmsN.Source;
                    pms.Width = pmsN.Width;
                    pms.Height = pmsN.Height;
                    pms.XOffsetFromCenter = pmsN.XOffsetFromCenter;
                    pms.YOffsetFromCenter = pmsN.YOffsetFromCenter;
                    pms.Color = pmsN.Color;
                    pms.Opacity = pmsN.Opacity;
                    pms.Angle = pmsN.Angle;
                    pms.SelectionColor = selectionColor;
                    #endregion

                    Dictionary<string, object> jsonObject = new ESRI.ArcGIS.Client.Utils.JavaScriptSerializer().DeserializeObject(json) as Dictionary<string, object>;    
                    if (jsonObject != null)
                    {
                        if (jsonObject.ContainsKey("imageData"))
                            SetImageData(pms, jsonObject["imageData"] as string);
                        else if (jsonObject.ContainsKey("url"))
                            SetImageUrl(pms, jsonObject["url"] as string);
                    }
                }
                return;
            }
            FS.SimpleFillSymbol sfs = o as FS.SimpleFillSymbol;
            if (sfs != null)
            {
                FS.SimpleFillSymbol sfsN = Symbol.FromJson(json) as FS.SimpleFillSymbol;
                if (sfsN != null)
                {
                    #region Copy Properties
                    sfs.Fill = sfsN.Fill;
                    sfs.Style = sfsN.Style;
                    sfs.Color = sfsN.Color;
                    sfs.BorderBrush = sfsN.BorderBrush;
                    sfs.BorderThickness = sfsN.BorderThickness;
                    sfs.BorderStyle = sfsN.BorderStyle;
                    sfs.SelectionColor = selectionColor;
                    #endregion
                }
                return;
            }
            FS.SimpleLineSymbol sls = o as FS.SimpleLineSymbol;
            if (sls != null)
            {
                FS.SimpleLineSymbol slsN = Symbol.FromJson(json) as FS.SimpleLineSymbol;
                if (slsN != null)
                {
                    #region CopyProperties
                    sls.Style = slsN.Style;
                    sls.Color = slsN.Color;
                    sls.Width = slsN.Width;
                    sls.SelectionColor = selectionColor;
                    #endregion
                }
                return;
            }
            FS.SimpleMarkerSymbol sms = o as FS.SimpleMarkerSymbol;
            if (sms != null)
            {
                FS.SimpleMarkerSymbol smsN = Symbol.FromJson(json) as FS.SimpleMarkerSymbol;
                if (smsN != null)
                {
                    #region Copy properties
                    sms.Style = smsN.Style;
                    sms.Color = smsN.Color;
                    sms.Size = smsN.Size;
                    sms.XOffsetFromCenter = smsN.XOffsetFromCenter;
                    sms.YOffsetFromCenter = smsN.YOffsetFromCenter;
                    sms.Angle = smsN.Angle;
                    sms.OutlineColor = smsN.OutlineColor;
                    sms.OutlineThickness = smsN.OutlineThickness;
                    sms.OutlineStyle = smsN.OutlineStyle;
                    sms.SelectionColor = selectionColor;
                    #endregion
                }
                return;
            }
        }

        public static string GetImageData(Symbol obj)
        {
            return (string)obj.GetValue(ImageDataProperty);
        }

        public static void SetImageData(Symbol obj, string value)
        {
            obj.SetValue(ImageDataProperty, value);
        }

        // Using a DependencyProperty as the backing store for ImageData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageDataProperty =
            DependencyProperty.RegisterAttached("ImageData", typeof(string), typeof(Symbol), new PropertyMetadata(null));

        //public static string GetImageContentType(Symbol obj)
        //{
        //    return (string)obj.GetValue(ImageContentTypeProperty);
        //}

        //public static void SetImageContentType(Symbol obj, string value)
        //{
        //    obj.SetValue(ImageContentTypeProperty, value);
        //}

        //// Using a DependencyProperty as the backing store for ImageContentType.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ImageContentTypeProperty =
        //    DependencyProperty.RegisterAttached("ImageContentType", typeof(string), typeof(Symbol), new PropertyMetadata(null));
        
        public static string GetImageUrl(Symbol obj)
        {
            return (string)obj.GetValue(ImageUrlProperty);
        }

        public static void SetImageUrl(Symbol obj, string value)
        {
            obj.SetValue(ImageUrlProperty, value);
        }

        // Using a DependencyProperty as the backing store for ImageUrl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageUrlProperty =
            DependencyProperty.RegisterAttached("ImageUrl", typeof(string), typeof(Symbol), new PropertyMetadata(null));

        //private static void ChangeImageSource(DependencyObject o, DependencyPropertyChangedEventArgs args)
        //{
        //    Symbol symbol = o as Symbol;
        //    ESRI.ArcGIS.Client.FeatureService.Symbols.PictureFillSymbol pfs = o as ESRI.ArcGIS.Client.FeatureService.Symbols.PictureFillSymbol;
        //    ESRI.ArcGIS.Client.FeatureService.Symbols.PictureMarkerSymbol pms = o as ESRI.ArcGIS.Client.FeatureService.Symbols.PictureMarkerSymbol;
        //    if (pfs != null || pms != null)
        //    {
        //        ImageSource source = null;
        //        string url = GetImageUrl(symbol);
        //        string data = GetImageData(symbol);
        //        if (!string.IsNullOrEmpty(url))
        //        {
        //            Uri uri = null;
        //            if (url.StartsWith("http") || url.Contains("/"))
        //                uri = new Uri(url, UriKind.Absolute);
        //            BitmapImage bmi = new BitmapImage();
        //            bmi.UriSource = uri;
        //            source = bmi;
        //        }
        //        else if (!string.IsNullOrEmpty(data)
        //            && (GetImageContentType(symbol) == "image/png"))
        //        {
        //            byte[] contentInBytes =
        //                        System.Convert.FromBase64String(data);
        //            BitmapImage bmi = new BitmapImage();
        //            bmi.SetSource(new System.IO.MemoryStream(contentInBytes));
        //            source = bmi;
        //        }
        //        if (source != null)
        //        {
        //            if (pms != null)
        //                pms.SetValue(ESRI.ArcGIS.Client.FeatureService.Symbols.PictureMarkerSymbol.SourceProperty, source);
        //            else if (pfs != null)
        //                pfs.SetValue(ESRI.ArcGIS.Client.FeatureService.Symbols.PictureFillSymbol.SourceProperty, source);
        //        }
        //    }
        //}

        //static void setImageSource(Symbol symbol, string json)
        //{
        //    ESRI.ArcGIS.Client.FeatureService.Symbols.PictureFillSymbol pfs = symbol as ESRI.ArcGIS.Client.FeatureService.Symbols.PictureFillSymbol;
        //    ESRI.ArcGIS.Client.FeatureService.Symbols.PictureMarkerSymbol pms = symbol as ESRI.ArcGIS.Client.FeatureService.Symbols.PictureMarkerSymbol;
        //    if (pfs != null || pms != null)
        //    {
        //        string url = null;
        //        string data = null;
        //        string contentType = null;

        //        ESRI.ArcGIS.Client.Utils.JavaScriptSerializer jss = new ESRI.ArcGIS.Client.Utils.JavaScriptSerializer();
        //        Dictionary<string, object> dictionary = jss.DeserializeObject(json) as Dictionary<string, object>;
        //        if (dictionary.ContainsKey("contentType"))
        //            contentType = dictionary["contentType"] as string;
        //        if (dictionary.ContainsKey("url"))
        //            url = dictionary["url"] as string;
        //        if (dictionary.ContainsKey("imageData"))
        //            data = dictionary["imageData"] as string;

        //        ImageSource source = null;
        //        if (!string.IsNullOrEmpty(url))
        //        {
        //            Uri uri = null;
        //            if (url.StartsWith("http") || url.Contains("/"))
        //                uri = new Uri(url, UriKind.Absolute);
        //            BitmapImage bmi = new BitmapImage();
        //            bmi.UriSource = uri;
        //            source = bmi;
        //        }
        //        else if (!string.IsNullOrEmpty(data)
        //            && (contentType == "image/png"))
        //        {
        //            byte[] contentInBytes =
        //                        System.Convert.FromBase64String(data);
        //            BitmapImage bmi = new BitmapImage();
        //            bmi.SetSource(new System.IO.MemoryStream(contentInBytes));
        //            source = bmi;
        //        }
        //        if (source != null)
        //        {
        //            if (pms != null)
        //                pms.Source = source;
        //            else if (pfs != null)
        //                pfs.Source = source;
        //        }
        //    }
        //}
    }
}
