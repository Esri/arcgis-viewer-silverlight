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
using System.IO;
using System.Windows.Resources;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class ThemeProvider : DependencyObject
    {
        #region Constants

        public static string ThmxNamespace = @"http://schemas.openxmlformats.org/drawingml/2006/main";
        public static string ThmxThemeUri = @"theme/theme/theme1.xml";

        private static string thmx_dk1 = "dk1";
        private static string thmx_dk2 = "dk2";
        private static string thmx_lt1 = "lt1";
        private static string thmx_lt2 = "lt2";
        private static string thmx_a1 = "accent1";
        private static string thmx_a2 = "accent2";
        private static string thmx_a3 = "accent3";
        private static string thmx_a4 = "accent4";
        private static string thmx_a5 = "accent5";
        private static string thmx_a6 = "accent6";
        private static string thmx_hlink = "hlink";
        private static string thmx_flink = "folHlink";
        private static string thmx_majfont = "majorFont";
        private static string thmx_minfont = "minorFont";

        #endregion

        public string ThemeFileUrl { get; set; }

        public virtual void GetApplicationTheme(object userState, EventHandler onCompleted, EventHandler<ExceptionEventArgs> onFailed)
        {
            if (string.IsNullOrEmpty(ThemeFileUrl))
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    if (onFailed != null)
						onFailed(this, new ExceptionEventArgs(Resources.Strings.ExceptionNoThemeFileUrlSpecified, userState));
                });
                return;
            }

            Uri _requestUri = ThemeFileUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? new Uri(ThemeFileUrl, UriKind.Absolute) : new Uri(string.Format("{0}{1}", RootUri, ThemeFileUrl), UriKind.Absolute);            
            WebRequest request = WebRequest.Create(_requestUri);
            request.BeginGetResponse((iar) =>
            {
                WebResponse response = null;
                try
                {
                    response = request.EndGetResponse(iar);
                }
                catch(Exception ex)
                {
                    Dispatcher.BeginInvoke((Action)delegate
                    {
                        if (onFailed != null)
                            onFailed(this, new ExceptionEventArgs(ex, userState));
                    });
                    return;
                }                

                Stream responseStream = response.GetResponseStream();
                ThemeColorSet = getColorSetFromTheme(responseStream);
                responseStream.Close();

                Dispatcher.BeginInvoke((Action)delegate
                {
                    if (onCompleted != null)
                        onCompleted(this, EventArgs.Empty);
                });
            }, userState);
        }

        public ThemeColorSet ThemeColorSet { get; set; }

        static ThemeColorSet getColorSetFromTheme(Stream thmxFile)
        {
            XDocument themeDoc = null;
            StreamResourceInfo thmxSRI = new StreamResourceInfo(thmxFile, null);
            StreamResourceInfo sri = Application.GetResourceStream(thmxSRI, new Uri(ThmxThemeUri, UriKind.Relative));

            //Add the namespace manager
            NameTable nameTable = new NameTable();
            XmlNamespaceManager manager = new XmlNamespaceManager(nameTable);
            manager.AddNamespace("a", ThmxNamespace);

            //Create XDocument from thmx stream
            themeDoc = XDocument.Load(sri.Stream);
            XDocument xDocTheme = XDocument.Parse(themeDoc.ToString());

            //Get RGB Values for the 12 theme colors
            string dk1 = getRGBFromTheme(xDocTheme, thmx_dk1, true);  //Text
            string dk2 = getRGBFromTheme(xDocTheme, thmx_dk2);  //Main Dark Color
            string lt1 = getRGBFromTheme(xDocTheme, thmx_lt1, true);  //Window Background
            string lt2 = getRGBFromTheme(xDocTheme, thmx_lt2);  //Main Light Color
            string a1 = getRGBFromTheme(xDocTheme, thmx_a1);    //Accent1
            string a2 = getRGBFromTheme(xDocTheme, thmx_a2);    //Accent2
            string a3 = getRGBFromTheme(xDocTheme, thmx_a3);    //Accent3
            string a4 = getRGBFromTheme(xDocTheme, thmx_a4);    //Accent4
            string a5 = getRGBFromTheme(xDocTheme, thmx_a5);    //Accent5
            string a6 = getRGBFromTheme(xDocTheme, thmx_a6);    //Accent6
            string hlink = getRGBFromTheme(xDocTheme, thmx_hlink); //Hyperlink
            string flink = getRGBFromTheme(xDocTheme, thmx_flink); //Followed Link

            //Get the two fonts
            string majFont = getFontFamilyFromTheme(xDocTheme, thmx_majfont); //Heading Font
            string minFont = getFontFamilyFromTheme(xDocTheme, thmx_minfont);//Body Font

            ThemeColorSet theme = new ThemeColorSet()
            {
                TextBackgroundDark1 = fromHex(dk1),
                TextBackgroundDark2 = fromHex(dk2),
                TextBackgroundLight1 = fromHex(lt1),
                TextBackgroundLight2 = fromHex(lt2),
                Accent1 = fromHex(a1),
                Accent2 = fromHex(a2),
                Accent3 = fromHex(a3),
                Accent4 = fromHex(a4),
                Accent5 = fromHex(a5),
                Accent6 = fromHex(a6),
                Hyperlink = fromHex(hlink),
                FollowedHyperlink = fromHex(flink),
            };
            //Color bkStart = fromHex(dk2);
            //Color bkEnd = fromHex(lt2);
            //Color text = fromHex(dk1);
            //Color winBkgd = fromHex(lt1);
            //Color separator = fromHex(a1);
            //Color selected = fromHex(flink);
            //Color mouseOver = fromHex(hlink);

            //ApplicationColorSet appSet = new ApplicationColorSet()
            //{
            //    ChildWindowColorSet = new ChildWindowColorSet()
            //    {
            //        BackgroundStartGradientColor = bkStart,
            //        //BackgroundEndGradientColor = bkEnd,
            //        TitleBarColor = winBkgd,
            //        TitleBarForegroundColor = text,
            //    },
            //    SidePanelControlColorSet = new SidePanelControlColorSet()
            //    {
            //        BackgroundStartGradientColor = bkStart,
            //        //BackgroundEndGradientColor = bkEnd,
            //        MouseOverFillColor = mouseOver,
            //        SelectedStateFillColor = selected,
            //        SeparatorLineColor = separator,
            //    },
            //};

            return theme;
        }

        static string getRGBFromTheme(XDocument XDocTheme, string ElementName)
        {
            XNamespace ns = ThmxNamespace;
            try
            {
                return (from c in XDocTheme.Descendants(ns + ElementName).Descendants(ns + "srgbClr")
                        select new { c.Attribute("val").Value }).FirstOrDefault().Value;
            }
            catch
            {
                return (from c in XDocTheme.Descendants(ns + ElementName).Descendants(ns + "sysClr")
                        select new { c.Attribute("lastClr").Value }).FirstOrDefault().Value;
            }

        }
        static string getRGBFromTheme(XDocument XDocTheme, string ElementName, bool trySysColorFirst)
        {
            if (!trySysColorFirst)
                return getRGBFromTheme(XDocTheme, ElementName);
            XNamespace ns = ThmxNamespace;
            try
            {
                return (from c in XDocTheme.Descendants(ns + ElementName).Descendants(ns + "sysClr")
                        select new { c.Attribute("lastClr").Value }).FirstOrDefault().Value;
            }
            catch
            {
                return (from c in XDocTheme.Descendants(ns + ElementName).Descendants(ns + "srgbClr")
                        select new { c.Attribute("val").Value }).FirstOrDefault().Value;
            }

        }
        static string getFontFamilyFromTheme(XDocument XDocTheme, string ElementName)
        {
            XNamespace ns = ThmxNamespace;
            return (from c in XDocTheme.Descendants(ns + ElementName).Descendants(ns + "latin")
                    select new { c.Attribute("typeface").Value }).FirstOrDefault().Value;
        }

        static Color fromHex(string newColor)
        {
            byte position = 0;
            newColor = newColor.Replace("#", "");
            byte alpha = System.Convert.ToByte("ff", 16);

            if (newColor.Length == 8)
            {
                // get the alpha channel value
                alpha = System.Convert.ToByte(newColor.Substring(position, 2), 16);
                position = 2;
            }

            // get the red value
            byte red = System.Convert.ToByte(newColor.Substring(position, 2), 16);
            position += 2;

            // get the green value
            byte green = System.Convert.ToByte(newColor.Substring(position, 2), 16);
            position += 2;

            // get the blue value
            byte blue = System.Convert.ToByte(newColor.Substring(position, 2), 16);

            // create the SolidColorBrush object
            return Color.FromArgb(alpha, red, green, blue);
        }

        private string RootUri
        {
            get
            {
                string scheme = System.Windows.Browser.HtmlPage.Document.DocumentUri.Scheme;
                string host = System.Windows.Browser.HtmlPage.Document.DocumentUri.Host;
                int port = System.Windows.Browser.HtmlPage.Document.DocumentUri.Port;

                string template = "{0}://{1}";
                string uri = string.Format(template, scheme, host);

                if (port != 80 && port != 443)
                    uri += ":" + port;

                return uri;
            }
        }
    }
}
