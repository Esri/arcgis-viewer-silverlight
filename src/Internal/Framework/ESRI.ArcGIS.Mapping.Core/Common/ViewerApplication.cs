/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.WebMap;

namespace ESRI.ArcGIS.Mapping.Core
{    
    public class ViewerApplication : INotifyPropertyChanged
    {
        public ViewerApplication(){ }

        public ViewerApplication(string xml)
        {
            FromXml(xml);
        }       
      
        #region TitleText
        private string _titleText;

        public string TitleText
        {
            get { return _titleText; }
            set
            {
                if (_titleText != value)
                {
                    _titleText = value;
                    RaisePropertyChanged("TitleText");
                }
            }
        }
        #endregion       
 
        #region Version
        private string _version;

        public string Version
        {
            get { return _version; }
            set
            {
                if (_version != value)
                {
                    _version = value;
                    RaisePropertyChanged("Version");
                }
            }
        }
        #endregion  
      
        #region LogoFilePath
        private string _logoFilePath;

        public string LogoFilePath
        {
            get { return _logoFilePath; }
            set
            {
                if (_logoFilePath != value)
                {
                    _logoFilePath = value;
                    RaisePropertyChanged("LogoFilePath");
                }
            }
        }
        #endregion

        #region LayoutFilePath 
        private string _layoutFilePath;

        public string LayoutFilePath
        {
            get { return _layoutFilePath; }
            set
            {
                if (_layoutFilePath != value)
                {
                    _layoutFilePath = value;
                    RaisePropertyChanged("LayoutFilePath");
                }
            }
        }
        #endregion        
        
        public ObservableCollection<HelpLink> HelpLinks { get; set; }

        public List<string> Extensions { get; set; }
        
        public string AboutXaml { get; set; }

        string arcGISOnlineSharing;
        public string ArcGISOnlineSharing
        {
            get { return arcGISOnlineSharing; }
            set
            {
                arcGISOnlineSharing = value;
                RaisePropertyChanged("ArcGISOnlineSharing");
            }
        }

        string arcGISOnlineSecure;
        public string ArcGISOnlineSecure
        {
            get { return arcGISOnlineSecure; }
            set
            {
                arcGISOnlineSecure = value;
                RaisePropertyChanged("ArcGISOnlineSecure");
            }
        }

        string arcGISOnlineProxy;
        public string ArcGISOnlineProxy
        {
            get { return arcGISOnlineProxy; }
            set
            {
                arcGISOnlineProxy = value;
                RaisePropertyChanged("ArcGISOnlineProxy");
            }
        }
        
        private string geometryService;
        public string GeometryService
        {
            get { return geometryService; }
            set { geometryService = value;  RaisePropertyChanged("GeometryService");}
        }
        
        private string proxy;
        public string Proxy
        {
            get { return proxy; }
            set { 
                proxy = value; 
                RaisePropertyChanged("Proxy");
            }
        }

        private string bingMapsAppId;
        public string BingMapsAppId
        {
            get { return bingMapsAppId; }
            set { bingMapsAppId = value; RaisePropertyChanged("BingMapsAppId"); }
        }

        private string portalAppId;
        public string PortalAppId
        {
            get { return portalAppId; }
            set { portalAppId = value; RaisePropertyChanged("PortalAppId"); }
        }

        private static WebMapSettings webMapSettings = new WebMapSettings();
        public static WebMapSettings WebMapSettings { get { return webMapSettings; } }

        private const string TITLETEXT = "TitleText";
        private const string LOGOFILEPATH = "LogoFilePath";
        private const string LAYOUTFILEPATH = "LayoutFilePath";
        private const string HELPLINKS = "HelpLinks";
        private const string HELPLINK = "HelpLink";
        private const string DISPLAYTEXT = "DisplayText";
        protected const string URL = "Url";
        private const string EXTENSIONS = "Extensions";
        protected const string EXTENSION = "Extension";
        protected const string VERSION = "Version";
        private const string ABOUTXAML = "AboutXaml";
        private const string ARCGISONLINESHARING = "ArcGISOnlineSharing";
        private const string ARCGISONLINESECURE = "ArcGISOnlineSharingSecure";
        private const string ARCGISONLINEPROXY = "ArcGISOnlineProxy";
        private const string GEOMETRYSERVICE = "GeometryService";
        private const string PROXY = "Proxy";
        private const string BINGMAPSPAPPID = "BingMapsAppId";
        private const string PORTALAPPID = "PortalAppId";

#region WebMap element constants
        private const string WEBMAP = "WebMap";
        private const string WEBMAP_ID = "ID";
        private const string WEBMAP_DOCUMENT = "Document";
        private const string WEBMAP_BINGTOKEN = "BingToken";
        private const string WEBMAP_GEOMETRYSERVICEURL = "GeometryServiceUrl";
        private const string WEBMAP_PROXYURL = "ProxyUrl";
        private const string WEBMAP_SERVERBASEURL = "ServerBaseUrl";
        private const string WEBMAP_TOKEN = "Token";
        private const string WEBMAP_LINKED = "Linked";
        private const string WEBMAP_TITLE = "Title";
#endregion
        
        protected void FromXml(string xml)
        {
            XDocument xDoc = XDocument.Parse(xml);
            XElement rootElement = xDoc.FirstNode as XElement;
            XElement elem = rootElement.Element(TITLETEXT);
            if (elem != null)
                TitleText = elem.Value;

            if(rootElement.Attribute(LOGOFILEPATH) != null)
                LogoFilePath = rootElement.Attribute(LOGOFILEPATH).Value;
            if(rootElement.Attribute(LAYOUTFILEPATH) != null)
                LayoutFilePath = rootElement.Attribute(LAYOUTFILEPATH).Value;

            elem = rootElement.Element(HELPLINKS);
            if (elem != null && elem.HasElements)
            {
                IEnumerable<HelpLink> helpLinks = from c in elem.Elements(HELPLINK)
                                                  select new HelpLink
                                                  {
                                                      DisplayText = c.Element(DISPLAYTEXT) != null ? c.Element(DISPLAYTEXT).Value : "",
                                                      Url = c.Element(URL) != null ? c.Element(URL).Value : "",
                                                  };                
                if (helpLinks != null)
                {
                    HelpLinks = new ObservableCollection<HelpLink>();
                    foreach (HelpLink helpLink in helpLinks)
                        HelpLinks.Add(helpLink);
                }
            }

            elem = rootElement.Element(EXTENSIONS);
            if (elem != null && elem.HasElements)
            {
                ParseExtensions(elem);                
            }

            elem = rootElement.Element(VERSION);
            if (elem != null)
                Version = elem.Value;

            elem = rootElement.Element(ABOUTXAML);
            if (elem != null && elem.FirstNode != null)
                AboutXaml = elem.FirstNode.ToString();
            
            elem = rootElement.Element(ARCGISONLINESHARING);
            if (elem != null)
                ArcGISOnlineSharing = elem.Value;

            elem = rootElement.Element(ARCGISONLINESECURE);
            if (elem != null)
                ArcGISOnlineSecure = elem.Value;
            
            elem = rootElement.Element(ARCGISONLINEPROXY);
            if (elem != null)
                ArcGISOnlineProxy = elem.Value;

		    elem = rootElement.Element(GEOMETRYSERVICE);
            if (elem != null)
                GeometryService = elem.Value;

            elem = rootElement.Element(PROXY);
            if (elem != null)
                Proxy = elem.Value;
            else
                Proxy = null;

            elem = rootElement.Element(BINGMAPSPAPPID);
            if (elem != null)
                BingMapsAppId = elem.Value;

            elem = rootElement.Element(PORTALAPPID);
            if (elem != null)
                PortalAppId = elem.Value;

            // Web map settings
            elem = rootElement.Element(WEBMAP);
            if (elem != null)
            {
                XElement idElem = elem.Element(WEBMAP_ID);
                // Only process web map properties if ID is present
                if (idElem != null && !string.IsNullOrEmpty(idElem.Value))
                {
                    ViewerApplication.WebMapSettings.ID = idElem.Value;

                    XElement titleElem = elem.Element(WEBMAP_TITLE);
                    if (titleElem != null)
                        ViewerApplication.WebMapSettings.Title = titleElem.Value;
                    else
                        ViewerApplication.WebMapSettings.Title = null;

                    XElement documentElem = elem.Element(WEBMAP_DOCUMENT);
                    if (documentElem != null)
                    {
                        ViewerApplication.WebMapSettings.Document = new Client.WebMap.Document();
                        XElement docPropElem = documentElem.Element(WEBMAP_BINGTOKEN);
                        if (docPropElem != null && !string.IsNullOrEmpty(docPropElem.Value))
                            ViewerApplication.WebMapSettings.Document.BingToken = docPropElem.Value;

                        if (string.IsNullOrEmpty(ViewerApplication.WebMapSettings.Document.BingToken)) ViewerApplication.WebMapSettings.Document.BingToken = BingMapsAppId;

                        docPropElem = documentElem.Element(WEBMAP_GEOMETRYSERVICEURL);
                        if (docPropElem != null && !string.IsNullOrEmpty(docPropElem.Value))
                            ViewerApplication.WebMapSettings.Document.GeometryServiceUrl = docPropElem.Value;

                        docPropElem = documentElem.Element(WEBMAP_PROXYURL);
                        if (docPropElem != null && !string.IsNullOrEmpty(docPropElem.Value))
                            ViewerApplication.WebMapSettings.Document.ProxyUrl = docPropElem.Value;

                        docPropElem = documentElem.Element(WEBMAP_SERVERBASEURL);
                        if (docPropElem != null && !string.IsNullOrEmpty(docPropElem.Value))
                            ViewerApplication.WebMapSettings.Document.ServerBaseUrl = docPropElem.Value;

                        docPropElem = documentElem.Element(WEBMAP_TOKEN);
                        if (docPropElem != null && !string.IsNullOrEmpty(docPropElem.Value))
                            ViewerApplication.WebMapSettings.Document.Token = docPropElem.Value;
                    }
                    else
                    {
                        ViewerApplication.WebMapSettings.Document = null;
                    }

                    XElement linkedElem = elem.Element(WEBMAP_LINKED);
                    if (linkedElem != null)
                    {
                        bool linked = false;
                        if (bool.TryParse(linkedElem.Value, out linked))
                            ViewerApplication.WebMapSettings.Linked = linked;
                        else
                            ViewerApplication.WebMapSettings.Linked = null;
                    }
                    else
                    {
                        ViewerApplication.WebMapSettings.Linked = null;
                    }
                }
            }
        }

        protected virtual void ParseExtensions(XElement elem)
        {
            if (elem == null || !elem.HasElements)
                return;

            IEnumerable<string> extensions = from c in elem.Elements(EXTENSION)
                                             select c.Attribute(URL) != null ? c.Attribute(URL).Value : "";
            if (extensions != null)
            {
                Extensions = new List<string>();
                foreach (string extension in extensions)
                {
                    if (string.IsNullOrWhiteSpace(extension))
                        continue;
                    Extensions.Add(extension);
                }
            }
        }
        protected XDocument ToXDocument()
        {
            XDocument xDoc = new XDocument();

            XElement rootElement = new XElement("ViewerApplication");
            xDoc.Add(rootElement);

            if (!string.IsNullOrWhiteSpace(TitleText))
                rootElement.Add(new XElement(TITLETEXT, TitleText));

            if (!string.IsNullOrWhiteSpace(Version))
                rootElement.Add(new XElement(VERSION) { Value = Version });

            if (!string.IsNullOrWhiteSpace(LogoFilePath))
                rootElement.SetAttributeValue(LOGOFILEPATH, LogoFilePath);

            if (!string.IsNullOrWhiteSpace(LayoutFilePath))
                rootElement.SetAttributeValue(LAYOUTFILEPATH, LayoutFilePath);

            XElement helpLinksElement = new XElement(HELPLINKS);
            rootElement.Add(helpLinksElement);
            if (HelpLinks != null)
            {
                foreach (HelpLink helpLink in HelpLinks)
                {
                    XElement helpLinkElement = new XElement(HELPLINK);
                    if (!string.IsNullOrWhiteSpace(helpLink.DisplayText))
                        helpLinkElement.Add(new XElement(URL, helpLink.Url));
                    if (!string.IsNullOrWhiteSpace(helpLink.Url))
                        helpLinkElement.Add(new XElement(DISPLAYTEXT, helpLink.DisplayText));
                    helpLinksElement.Add(helpLinkElement);
                }
            }

            XElement extensionsElement = new XElement(EXTENSIONS);
            rootElement.Add(extensionsElement);

            ExtensionsToXml(extensionsElement);

            if (!string.IsNullOrWhiteSpace(AboutXaml))
            {
                AboutXaml = AboutXaml.Trim();
                XElement abtXaml = new XElement(ABOUTXAML);
                try
                {
                    if (AboutXaml.StartsWith("<", StringComparison.Ordinal) && AboutXaml.EndsWith(">", StringComparison.Ordinal))
                    {
                        XDocument xDoc2 = XDocument.Parse(AboutXaml);
                        abtXaml.Add(xDoc2.Root);
                    }
                    else
                    {
                        abtXaml.Value = AboutXaml;
                    }
                }
                catch
                {
                    abtXaml.Value = AboutXaml;
                }
                rootElement.Add(abtXaml);
            }

            rootElement.Add(new XElement(ARCGISONLINESHARING) { 
                Value = string.IsNullOrEmpty(ArcGISOnlineSharing) ? string.Empty : ArcGISOnlineSharing });

            rootElement.Add(new XElement(ARCGISONLINESECURE) { 
                Value = string.IsNullOrEmpty(ArcGISOnlineSecure) ? string.Empty : ArcGISOnlineSecure });

            rootElement.Add(new XElement(ARCGISONLINEPROXY) { 
                Value = string.IsNullOrEmpty(ArcGISOnlineProxy) ? string.Empty : ArcGISOnlineProxy });

            rootElement.Add(new XElement(GEOMETRYSERVICE) { 
                Value = string.IsNullOrEmpty(GeometryService) ? string.Empty : GeometryService });

            rootElement.Add(new XElement(PROXY) { 
                Value = string.IsNullOrEmpty(Proxy) ? string.Empty : Proxy });

            rootElement.Add(new XElement(BINGMAPSPAPPID) { 
                Value = string.IsNullOrEmpty(BingMapsAppId) ? string.Empty : BingMapsAppId });

            rootElement.Add(new XElement(PORTALAPPID) {
                Value = string.IsNullOrEmpty(PortalAppId) ? string.Empty : PortalAppId });

            XElement webMapElem = new XElement(WEBMAP); 
            rootElement.Add(webMapElem);
            webMapToXml(webMapElem);

            return xDoc;
        }

        public virtual string ToXml()
        {
            XDocument xDoc = ToXDocument();
            if (xDoc != null)
                return xDoc.ToString(SaveOptions.OmitDuplicateNamespaces);
            else
                return null;
        }

        protected virtual void ExtensionsToXml(XElement parentElement)
        {
            if (Extensions != null)
            {
                foreach (string extension in Extensions)
                {
                    if (string.IsNullOrWhiteSpace(extension))
                        continue;

                    XElement extensionElement = new XElement(EXTENSION);
                    extensionElement.SetAttributeValue(URL, extension);
                    parentElement.Add(extensionElement);
                }
            }
        }

        private void webMapToXml(XElement root)
        {
            root.Add(new XElement(WEBMAP_ID) {
                Value = string.IsNullOrEmpty(WebMapSettings.ID) ? string.Empty : WebMapSettings.ID });

            root.Add(new XElement(WEBMAP_TITLE) {
                Value = string.IsNullOrEmpty(WebMapSettings.Title) ? string.Empty : WebMapSettings.Title });

            Document doc = WebMapSettings.Document;
            if (doc != null)
            {
                XElement docElem = new XElement(WEBMAP_DOCUMENT);
                root.Add(docElem);

                // Note that the web map token is not serialized, as this would introduce a security risk
                docElem.Add(new XElement(WEBMAP_BINGTOKEN) { Value = doc.BingToken ?? string.Empty });
                docElem.Add(new XElement(WEBMAP_GEOMETRYSERVICEURL) { Value = doc.GeometryServiceUrl ?? string.Empty });
                docElem.Add(new XElement(WEBMAP_PROXYURL) { Value = doc.ProxyUrl ?? string.Empty });
                docElem.Add(new XElement(WEBMAP_SERVERBASEURL) { Value = doc.ServerBaseUrl ?? string.Empty });
            }

            root.Add(new XElement(WEBMAP_LINKED) { 
                Value = WebMapSettings.Linked == null ? string.Empty : WebMapSettings.Linked.ToString() });
        }

        #region INotifyPropertyChanged values

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
 
    }
}
