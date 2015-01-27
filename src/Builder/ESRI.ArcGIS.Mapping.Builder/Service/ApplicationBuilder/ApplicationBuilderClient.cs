/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Xml.Linq;
using ESRI.ArcGIS.Mapping.Builder.Common;
using ESRI.ArcGIS.Mapping.Builder.Service;
using System.Text;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Windowing;
using System.Windows;
using ESRI.ArcGIS.Mapping.Builder.Resources;
using System.Windows.Browser;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace ESRI.ArcGIS.Mapping.Builder.ApplicationBuilder
{
    public class ApplicationBuilderClient : ServiceBase
    {
        private WebClient _webClient = new WebClient();

        public ApplicationBuilderClient(string UserId) : base(UserId) { }

        #region SaveConfigurationStore
        public void SaveConfigurationStoreXmlAsync(string xml, object userState = null)
        {
            Uri uri = CreateRestRequest("ConfigurationStore/Save");
            WebClient webClient = new WebClient();
            webClient.UploadStringCompleted += new UploadStringCompletedEventHandler(onSaveConfigurationStoreCompleted);
            webClient.UploadStringAsync(uri, "POST", xml, userState);
        }

        void onSaveConfigurationStoreCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            if (e.Cancelled || SaveConfigurationStoreXmlCompleted == null) 
                return;
            
            Exception error = e.Error;
            if (error == null) // Also check the response for errors
                error = GetErrorIfAny(e.Result);

            SaveConfigurationStoreXmlCompleted(this, new SaveConfigurationStoreXmlCompletedEventArgs()
            {
                Error = e.Error,
                UserState = e.UserState,
            });
        }

        public EventHandler<SaveConfigurationStoreXmlCompletedEventArgs> SaveConfigurationStoreXmlCompleted { get; set; }
        #endregion

        #region GetSites
        public void GetSitesAsync(object userState = null)
        {
            Uri uri = CreateRestRequest("Sites/Get");
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(onGetSitesCompleted);
            webClient.DownloadStringAsync(uri, userState);
        }

        void onGetSitesCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled || GetSitesCompleted == null)
                return;

            Exception error = e.Error;
            if (error == null)
                error = GetErrorIfAny(e.Result);

            ObservableCollection<Site> sites = new ObservableCollection<Site>();
            if (error == null)
            {
                try
                {
                    XDocument xDoc = XDocument.Parse(e.Result);
                    XElement sitesElem = xDoc.Element("Sites");
                    if (sitesElem != null)
                    {
                        IEnumerable<XElement> siteElems = sitesElem.Elements("Site");
                        if (siteElems != null)
                        {
                            foreach (XElement elem in siteElems)
                            {
                                sites.Add(new Site()
                                {
                                    ID = elem.Element("ID") != null ? elem.Element("ID").Value : null,
                                    Name = elem.Element("Name") != null ? elem.Element("Name").Value : null,
                                    Description = elem.Element("Description") != null ? elem.Element("Description").Value : null,
                                    Url = elem.Element("Url") != null ? elem.Element("Url").Value : null,
                                    ProductVersion = elem.Element("ProductVersion") != null 
                                        && !string.IsNullOrEmpty(elem.Element("ProductVersion").Value) ?
                                        elem.Element("ProductVersion").Value : null
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            }

            GetSitesCompleted(this, new GetSitesCompletedEventArgs()
            {
                Error = error,
                UserState = e.UserState,
                Sites = sites,
            });
        }

        public EventHandler<GetSitesCompletedEventArgs> GetSitesCompleted { get; set; }
        #endregion

        #region GetConfigurationStore
        public void GetConfigurationStoreXmlAsync(object userState = null)
        {
            Uri uri = CreateRestRequest("ConfigurationStore/Get");
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(onGetConfigurationStoreCompleted);
            webClient.DownloadStringAsync(uri, userState);
        }

        void onGetConfigurationStoreCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled || GetConfigurationStoreXmlCompleted == null)
                return;

            Exception error = e.Error;
            if (error == null)
                error = GetErrorIfAny(e.Result);

            GetConfigurationStoreXmlCompleted(this, new GetConfigurationStoreXmlCompletedEventArgs()
            {
                Error = error,
                UserState = e.UserState,
                ConfigurationStoreXml = error == null ? e.Result : null,
            });
        }

        public EventHandler<GetConfigurationStoreXmlCompletedEventArgs> GetConfigurationStoreXmlCompleted { get; set; }
        #endregion

        #region GetTemplates
        public void GetTemplatesAsync(object userState = null)
        {
            Uri uri = CreateRestRequest("Templates/Get");
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(onGetTemplatesCompleted);
            webClient.DownloadStringAsync(uri, userState);
        }

        void onGetTemplatesCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled || GetTemplatesCompleted == null)
                return;

            Exception error = e.Error;
            if (error == null)
                error = GetErrorIfAny(e.Result);

            Templates templates = new Templates();
            if (error == null)
            {
                try
                {
                    XDocument xDoc = XDocument.Parse(e.Result);
                    XElement templatesElem = xDoc.Element("Templates");
                    if (templatesElem != null)
                    {
                        IEnumerable<XElement> templateElems = templatesElem.Elements("Template");
                        if (templateElems != null)
                        {
                            foreach (XElement elem in templateElems)
                            {
                                templates.Add(new Template()
                                {
                                    BaseUrl = elem.Element("BaseUrl") != null ? elem.Element("BaseUrl").Value : null,
                                    DisplayName = elem.Element("DisplayName") != null ? elem.Element("DisplayName").Value : null,
                                    ID = elem.Element("ID") != null ? elem.Element("ID").Value : null,
                                    IsDefault = elem.Element("IsDefault").Value != null ? bool.Parse(elem.Element("IsDefault").Value) : false,
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            }

            GetTemplatesCompleted(this, new GetTemplatesCompletedEventArgs()
            {
                Templates = templates,
                UserState = e.UserState,
                Error = error,
            });
        }

        public EventHandler<GetTemplatesCompletedEventArgs> GetTemplatesCompleted { get; set; }
        #endregion

        #region DeleteSite
        public void DeleteWebSiteAsync(string siteId, object userState = null)
        {
            Uri uri = CreateRestRequest("Sites/Delete", "siteId=" + siteId);
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(onDeleteSiteCompleted);
            webClient.DownloadStringAsync(uri, userState);
        }

        void onDeleteSiteCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled || DeleteWebSiteCompleted == null)
                return;

            Exception error = e.Error;
            if (error == null)
                error = GetErrorIfAny(e.Result);

            DeleteWebSiteCompleted(this, new DeleteWebSiteCompletedEventArgs() {
                 Error = error,
                 UserState = e.UserState,
            });
        }

        public EventHandler<DeleteWebSiteCompletedEventArgs> DeleteWebSiteCompleted { get; set; }
        #endregion

        #region CreateViewerFromTemplate
        public void CreateViewerApplicationFromTemplateAsync(string siteName, string siteTitle, string description, string templateId, SitePublishInfo info, object userState = null)
        {
            XDocument xDoc = new XDocument();
            XElement rootElem = new XElement("CreateApplication");
            xDoc.Add(rootElem);

            rootElem.SetAttributeValue("Name", siteName);
            rootElem.SetAttributeValue("Description", description);
            rootElem.SetAttributeValue("TemplateId", templateId);
            rootElem.SetAttributeValue("Title", siteTitle);
            serializeSitePublishInfo(info, rootElem);

            Uri uri = CreateRestRequest("Sites/Create");
            WebClient webClient = new WebClient();
            webClient.UploadStringCompleted += new UploadStringCompletedEventHandler(onCreateApplicationCompleted);
            webClient.UploadStringAsync(uri, "POST", xDoc.ToString(SaveOptions.OmitDuplicateNamespaces), userState);
        }

        private void serializeSitePublishInfo(SitePublishInfo info, XElement rootElem)
        {
            if (!string.IsNullOrWhiteSpace(info.ApplicationXml))
                rootElem.Add(new XElement("ApplicationXml", info.ApplicationXml));
            if (!string.IsNullOrWhiteSpace(info.MapXaml))
                rootElem.Add(new XElement("MapXaml", info.MapXaml));
            if (!string.IsNullOrWhiteSpace(info.BehaviorsXml))
                rootElem.Add(new XElement("BehaviorsXml", info.BehaviorsXml));
            if (!string.IsNullOrWhiteSpace(info.ColorsXaml))
                rootElem.Add(new XElement("ColorsXaml", info.ColorsXaml));
            if (!string.IsNullOrWhiteSpace(info.ControlsXml))
                rootElem.Add(new XElement("ControlsXml", info.ControlsXml));
            if (!string.IsNullOrWhiteSpace(info.ToolsXml))
                rootElem.Add(new XElement("ToolsXml", info.ToolsXml));
            if (info.PreviewImageBytes != null)
                rootElem.Add(new XElement("PreviewImageBytes", info.PreviewImageBytes));
            if (info.ExtensionsXapsInUse != null)
                rootElem.Add(new XElement("ExtensionsXapsInUse", string.Join(",", info.ExtensionsXapsInUse)));
        }

        void onCreateApplicationCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            if (e.Cancelled || CreateViewerApplicationFromTemplateCompleted == null)
                return;

            Exception error = e.Error;
            if (error == null)
                error = GetErrorIfAny(e.Result);            

            Site site = null;
            if (error == null)
            {
                site = getSiteFromString(e.Result);
            }
            else
            {
                // This generic message is displayed because, previously, the message was "not beneficial". The
                // frequent case for failure here is because the size of the request made to the server exceeds
                // a buffer length. Until we can return a more specific error for that case, this generic message
                // must suffice.
                error = new Exception(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.UnableToDeployApplication, error);
            }

            CreateViewerApplicationFromTemplateCompleted(this, new CreateViewerApplicationFromTemplateCompletedEventArgs() { 
                 Site = site,
                 UserState = e.UserState,
                 Error = error,
            });
        }

        public EventHandler<CreateViewerApplicationFromTemplateCompletedEventArgs> CreateViewerApplicationFromTemplateCompleted { get; set; }
        #endregion

        #region SaveConfiguration
        public void SaveConfigurationForSiteAsync(string siteId, SitePublishInfo info, string siteTitle, object userState = null)
        {
            XDocument xDoc = new XDocument();
            XElement rootElem = new XElement("SaveApplication");
            xDoc.Add(rootElem);
            rootElem.SetAttributeValue("Title", siteTitle);

            serializeSitePublishInfo(info, rootElem);

            Uri uri = CreateRestRequest("Sites/Save", "siteId=" + siteId);
            WebClient webClient = new WebClient();
            webClient.UploadStringCompleted += new UploadStringCompletedEventHandler(onSaveConfigurationCompleted);
            webClient.UploadStringAsync(uri, "POST", xDoc.ToString(SaveOptions.OmitDuplicateNamespaces), userState);
        }

        void onSaveConfigurationCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            if (e.Cancelled || SaveConfigurationForSiteCompleted == null)
                return;

            Exception error = e.Error;
            if (error == null)
                error = GetErrorIfAny(e.Result);

            // Similar to deployment of a new application, if an error occurs on the server during saving of
            // changes, we simply display this generic message indicating that the changes could not be saved.
            // Previously, the error message would be archaic, misleading or difficult to interpret. It is 
            // better to have this message that confirms the suspicion that saving did not work and clearly
            // states as such without explaining why. A common reason for this is that the length of the
            // request to the server was too long, exceeding an internal Microsoft buffer limit and until we
            // can specifically return that message, this will suffice.
            if (error != null)
                error = new Exception(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.UnableToSaveApplicationChanges, error);

            SaveConfigurationForSiteCompleted(this, new SaveConfigurationForSiteCompletedEventArgs()
            {
                Error = error,
                UserState = e.UserState,
            });
        }

        public EventHandler<SaveConfigurationForSiteCompletedEventArgs> SaveConfigurationForSiteCompleted { get; set; }        
        #endregion

        #region GetSettings
        public void GetSettingsXmlAsync(object userState = null)
        {
            Uri uri = CreateRestRequest("Settings/Get");
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(onGetSettingsCompleted);
            webClient.DownloadStringAsync(uri, userState);
        }

        void onGetSettingsCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled || GetSettingsXmlCompleted == null)
                return;

            Exception error = e.Error;
            if (error == null)
                error = GetErrorIfAny(e.Result);

            GetSettingsXmlCompleted(this, new GetSettingsXmlCompletedEventArgs()
            {
                Error = error,
                UserState = e.UserState,
                SettingsXml = error == null ? e.Result : null,
            });
        }

        public EventHandler<GetSettingsXmlCompletedEventArgs> GetSettingsXmlCompleted { get; set; }
        #endregion

        #region SaveSettings
        public void SaveSettingsAsync(string settingsFileRelativePath, string xml, object userState = null)
        {
            Uri uri = CreateRestRequest("Settings/Save", "path=" + settingsFileRelativePath);
            WebClient webClient = new WebClient();
            webClient.UploadStringCompleted += new UploadStringCompletedEventHandler(onSaveSettingsCompleted);
            webClient.UploadStringAsync(uri, "POST", xml, userState);
        }

        void onSaveSettingsCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            if (e.Cancelled || SaveSettingsCompleted == null)
                return;
            SaveSettingsCompleted(this, EventArgs.Empty);
        }

        public EventHandler SaveSettingsCompleted { get; set; }
        #endregion

        #region CopySite
        public void CopySiteAsync(string sourceSiteId, string targetSiteName, string targetSiteDescription, object userState = null)
        {
            // Create XML document payload with proper root node for this operation
            XDocument xDoc = new XDocument();
            XElement rootElem = new XElement("CopySite");
            xDoc.Add(rootElem);

            // If the site name or description have been provided for the target site, then add to DOM
            if (!string.IsNullOrWhiteSpace(targetSiteName))
                rootElem.Add(new XElement("siteName", targetSiteName));
            if (!string.IsNullOrWhiteSpace(targetSiteDescription))
                rootElem.Add(new XElement("description", targetSiteDescription));

            // Create Uri to invoke server method
            Uri uri = CreateRestRequest("Sites/Copy", "siteId=" + sourceSiteId);
            WebClient webClient = new WebClient();
            webClient.UploadStringCompleted += new UploadStringCompletedEventHandler(onCopySiteCompleted);
            webClient.UploadStringAsync(uri, "POST", xDoc.ToString(SaveOptions.OmitDuplicateNamespaces), userState);
        }

        private void onCopySiteCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            if (e.Cancelled || CopySiteCompleted == null)
                return;

            Exception error = e.Error;
            if (error == null)
                error = GetErrorIfAny(e.Result);

            Site site = null;
            if (error == null)
                site = getSiteFromString(e.Result);

            // Execute event handlers while packaging Site object into event arguments
            CopySiteCompleted(this, new CopySiteCompletedEventArgs()
            {
                Site = site,
                UserState = e.UserState,
                Error = error,
            });
        }

        public EventHandler<CopySiteCompletedEventArgs> CopySiteCompleted { get; set; }
        #endregion

        #region GetExtensions
        public void GetExtensionLibrariesAsync(object userState = null)
        {
            Uri uri = CreateRestRequest("Extensions/Get");
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(onGetExtensionLibrariesCompleted);
            webClient.DownloadStringAsync(uri, userState);
        }

        void onGetExtensionLibrariesCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled || GetExtensionLibrariesCompleted == null)
                return;

            Exception error = e.Error;
            if (error == null)
                error = GetErrorIfAny(e.Result);

            ObservableCollection<Extension> extensions = new ObservableCollection<Extension>();
            string baseUrl = null;
            if (error == null)
            {
                XDocument xDoc = XDocument.Parse(e.Result);
                XElement extensionsElem = xDoc.Element("Extensions");
                baseUrl = xDoc.Root.Attribute("BaseUrl").Value;
                if (extensionsElem != null)
                {
                    IEnumerable<XElement> extensionElemns = extensionsElem.Elements("Extension");
                    if (extensionElemns != null)
                    {
                        foreach (XElement elem in extensionElemns)
                        {
                            bool required = false;
                            if (elem.Attribute("Required") != null)
                                bool.TryParse(elem.Attribute("Required").Value, out required);

                            Extension extension = new Extension()
                            {
                                Name = elem.Attribute("Name") != null ? elem.Attribute("Name").Value : null,
                                Url = elem.Attribute("Url") != null ? elem.Attribute("Url").Value : null,
                                Required = required         
                            };
                            XElement assembliesElem = elem.Element("Assemblies");
                            if (assembliesElem != null)
                            {
                                IEnumerable<XElement> assemblyElems = assembliesElem.Elements("Assembly");
                                if (assembliesElem != null)
                                {
                                    extension.Assemblies = new List<Assembly>();
                                    foreach (XElement assemElem in assemblyElems)
                                    {
                                        extension.Assemblies.Add(new Assembly()
                                        {
                                            Name = assemElem.Attribute("Name") != null ? assemElem.Attribute("Name").Value : null,
                                        });
                                    }
                                }
                            }
                            extensions.Add(extension);
                        }
                    }
                }
            }

            GetExtensionLibrariesCompleted(this, new GetExtensionLibrariesCompletedEventArgs() { 
                 UserState = e.UserState,
                 ExtensionsRepositoryBaseUrl = baseUrl,
                 Extensions = extensions,
                 Error = error,
            });
        }

        public EventHandler<GetExtensionLibrariesCompletedEventArgs> GetExtensionLibrariesCompleted { get; set; }
        #endregion

        #region UploadExtension
        public void UploadExtensionLibraryAsync(string selectedFileName, byte[] fileContents, ObservableCollection<string> assemblies, object userState)
        {
            Uri uri = CreateRestRequest("Extensions/Upload");

            var assembliesString = assemblies != null ? string.Join(",", assemblies) : null;
            var content = new MultipartFormDataContent();
            var ms = new MemoryStream(fileContents);
            var fileContent = new StreamContent(ms);

            // Specify the content disposition and content type - without this the form data will not
            // be included in the Request object in .NET 2.0 app pools
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"files\"",
                FileName = "\"" + selectedFileName + "\""
            };
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-silverlight-app");
            content.Add(fileContent);
            
            var stringContent = new StringContent(assembliesString);

            // Need to specify the content disposition and content type for .NET 2.0 compatibility here, too
            stringContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"assemblies\""
            };
            stringContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            content.Add(stringContent);

            var client = new HttpClient();
            client.PostAsync(uri, content).ContinueWith(t =>
            {
                stringContent.Dispose();
                fileContent.Dispose();
                ms.Dispose();
                content.Dispose();

                if (t.IsCanceled)
                    return;

                if (t.Exception != null)
                {
                    var errorDisplay = new ErrorDisplay();

                    // Attempt to get the stack trace with IL offsets
                    string stackTraceIL = t.Exception.StackTraceIL();

                    ErrorData data = new ErrorData()
                    {
                        Message = t.Exception.Message,
                        StackTrace = !string.IsNullOrEmpty(stackTraceIL) ? stackTraceIL :
                            t.Exception.StackTrace
                    };

                    errorDisplay.DataContext = data;

                    // Size the error UI
                    double width = Application.Current.RootVisual.RenderSize.Width * 0.67;
                    errorDisplay.Width = width > errorDisplay.MaxWidth ? errorDisplay.MaxWidth : width;
                    errorDisplay.Completed += (o, a) => BuilderApplication.Instance.HideWindow(errorDisplay);

                    // Show the error
                    BuilderApplication.Instance.ShowWindow(Strings.ErrorOccured, errorDisplay, false, null, null);
                }

                if (UploadExtensionLibraryCompleted != null)
                {
                    UploadExtensionLibraryCompleted(this, new UploadExtensionLibraryCompletedEventArgs()
                    {
                        Error = t.Exception,
                        UserState = userState,
                    });
                }

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public EventHandler<UploadExtensionLibraryCompletedEventArgs> UploadExtensionLibraryCompleted { get; set; }
        #endregion        

        #region DeleteExtension
        public void DeleteExtensionLibraryAsync(string extensionName, object userState)
        {
            Uri uri = CreateRestRequest("Extensions/Delete", string.Format("fileName={0}", extensionName));
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_DownloadStringCompleted);
            webClient.DownloadStringAsync(uri, userState);
        }

        void webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled || DeleteExtensionLibraryCompleted == null)
                return;

            Exception error = e.Error;
            if (error == null)
                error = GetErrorIfAny(e.Result);

            DeleteExtensionLibraryCompleted(this, new DeleteExtensionLibraryCompletedEventArgs() {
                Error = error,
                 UserState = e.UserState,
            });
        }
        public EventHandler<DeleteExtensionLibraryCompletedEventArgs> DeleteExtensionLibraryCompleted { get; set; }
        #endregion

        #region UpgradeSite

        /// <summary>
        /// Upgrades the site specified by the ID to the current version of the product
        /// </summary>
        internal void UpgradeSiteAsync(string siteId, string templateId, object userState = null)
        {
            // Issue upgrade request, including site and template ID
            Uri uri = CreateRestRequest("Sites/Upgrade", string.Format("siteId={0}&templateId={1}", siteId, templateId));
            _webClient.DownloadStringCompleted += onUpgradeSiteCompleted;
            _webClient.DownloadStringAsync(uri, userState);
        }

        private void onUpgradeSiteCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            _webClient.DownloadStringCompleted -= onUpgradeSiteCompleted;

            if (e.Cancelled || UpgradeSiteCompleted == null)
                return;

            // Retrieve error, if present
            Exception error = e.Error;
            if (e.Error != null)
            {
                if (UpgradeSiteCompleted != null)
                {
                    UpgradeSiteCompleted(this, new UpgradeSiteCompletedEventArgs(null)
                    {
                        Error = error,
                        UserState = e.UserState,
                    });
                }
                return;
            }
            if (error == null)
                error = GetErrorIfAny(e.Result);

            // Fire completed handler
            if (UpgradeSiteCompleted != null)
            {
                UpgradeSiteCompleted(this, new UpgradeSiteCompletedEventArgs(getSiteFromString(e.Result))
                {
                    Error = error,
                    UserState = e.UserState,
                });
            }
        }

        /// <summary>
        /// Fires when an upgrade operation has completed
        /// </summary>
        internal event EventHandler<UpgradeSiteCompletedEventArgs> UpgradeSiteCompleted;
        #endregion

        private Site getSiteFromString(string siteXml)
        {
            XDocument xDoc = XDocument.Parse(siteXml);
            XElement rootElem = xDoc.Element("Site");
            if (rootElem != null)
            {
                return new Site()
                {
                    ID = rootElem.Element("ID") != null ? rootElem.Element("ID").Value : null,
                    Description = rootElem.Element("Description") != null ? rootElem.Element("Description").Value : null,
                    Name = rootElem.Element("Name") != null ? rootElem.Element("Name").Value : null,
                    Url = rootElem.Element("Url") != null ? rootElem.Element("Url").Value : null,
                    ProductVersion = rootElem.Element("ProductVersion") != null
                        && !string.IsNullOrEmpty(rootElem.Element("ProductVersion").Value) ?
                        rootElem.Element("ProductVersion").Value : null
                };
            }

            return null;
        }
    }

    public class GetSitesCompletedEventArgs : AsyncCompletedEventArgs
    {
        public ObservableCollection<Site> Sites { get; set; }
    }

    public class CreateViewerApplicationFromTemplateCompletedEventArgs : AsyncCompletedEventArgs
    {
        public Site Site { get; set; }
    }

    public class SaveConfigurationForSiteCompletedEventArgs : AsyncCompletedEventArgs
    {
    }

    public class CopySiteCompletedEventArgs : AsyncCompletedEventArgs
    {
        public Site Site { get; set; }
    }

    public class GetExtensionLibrariesCompletedEventArgs : AsyncCompletedEventArgs
    {
        public string ExtensionsRepositoryBaseUrl { get; set; }

        public IEnumerable<Extension> Extensions { get; set; }
    }

    public class DeleteWebSiteCompletedEventArgs : AsyncCompletedEventArgs
    {
    }

    public class DeleteExtensionLibraryCompletedEventArgs : AsyncCompletedEventArgs
    {
    }

    public class UploadExtensionLibraryCompletedEventArgs : AsyncCompletedEventArgs
    {
    }

    public class SaveConfigurationStoreXmlCompletedEventArgs : AsyncCompletedEventArgs
    {
    }

    public class GetConfigurationStoreXmlCompletedEventArgs : AsyncCompletedEventArgs
    {
        public string ConfigurationStoreXml { get; set; }
    }

    public class GetSettingsXmlCompletedEventArgs : AsyncCompletedEventArgs
    {
        public string SettingsXml { get; set; }
    }

    public class GetTemplatesCompletedEventArgs : AsyncCompletedEventArgs
    {
        public Templates Templates { get; set; }
    }

    internal class UpgradeSiteCompletedEventArgs : AsyncCompletedEventArgs 
    {
        internal UpgradeSiteCompletedEventArgs(Site site) { Site = site; }
        internal Site Site { get; private set; }
    }
}
