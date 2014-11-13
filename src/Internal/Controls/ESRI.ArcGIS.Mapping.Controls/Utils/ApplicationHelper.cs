/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Json;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.DataSources.ArcGISServer;
using ESRI.ArcGIS.Mapping.Windowing;
using ESRI.ArcGIS.Mapping.Controls.ArcGISOnline;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public static class ApplicationHelper
    {
        public static string GetViewerApplicationTitle()
        {
            if (ViewerApplicationControl.Instance != null && ViewerApplicationControl.Instance.ViewerApplication != null)
                return ViewerApplicationControl.Instance.ViewerApplication.TitleText;

            return null;
        }

        public static string GetViewerApplicationVersion()
        {
            if (ViewerApplicationControl.Instance != null && ViewerApplicationControl.Instance.ViewerApplication != null)
                return ViewerApplicationControl.Instance.ViewerApplication.Version;

            return null;
        }

        public static string GetExecutingAssemblyVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            if (assembly != null)
            {
                AssemblyName ver = new AssemblyName(assembly.FullName);
                if (ver != null)
                    return ver.Version.ToString();
            }
            return null;
        }

        public static string GetProductVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            if (assembly != null)
            {
                AssemblyName ver = new AssemblyName(assembly.FullName);
                if (ver != null)
                {
                    Version version = new Version(ver.Version.Major, ver.Version.Minor, 
                        ver.Version.Build, ver.Version.Revision);
                    return version.ToString();
                }
            }
            return null;
        }

        public static string GetSilverlightAPIVersion()
        {
            Type mapType = typeof(ESRI.ArcGIS.Client.Map);
            if (mapType != null && mapType.Assembly != null)
            {
                string build = null;

                object[] attributes = mapType.Assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
                if (attributes != null && attributes.Length > 0)
                {
                    AssemblyFileVersionAttribute fileVersionAttribute = (AssemblyFileVersionAttribute)attributes[0];
                    string version = fileVersionAttribute.Version;
                    if (!string.IsNullOrEmpty(version))
                    {
                        string[] versionParts = version.Split('.');
                        if (versionParts.Length > 0)
                            build = versionParts[versionParts.Length - 1];
                    }
                }

                if (build == null)
                {
                    AssemblyName ver = new AssemblyName(mapType.Assembly.FullName);
                    if (ver != null)
                        build = ver.Version.Build.ToString();
                    else 
                        build = "0";
                }

                return string.Format("3.3.0.{0}", build);
            }

            return null;
        }

        public static bool IsSharePoint
        {
            get
            {
                return (ViewerApplicationControl.Instance == null);
            }
        }

        /// <summary>
        /// Attempt to use application environment credentials to authenticate against the specified URL
        /// </summary>
        public static async Task<IdentityManager.Credential> TryExistingCredentials(string requestUrl)
        {
            if (string.IsNullOrEmpty(requestUrl) || UserManagement.Current.Credentials.Count == 0)
                return null;

            bool isPortalUrl = requestUrl.IsPortalUrl();
            string credentialUrl = requestUrl;

            // Get the token auth endpoint if the requested URL is not an ArcGIS Online/Portal URL
            if (!isPortalUrl)
                credentialUrl = await ArcGISServerDataSource.GetTokenURL(requestUrl, null);

            // Check whether there's already a credential for the url
            foreach (IdentityManager.Credential cred in UserManagement.Current.Credentials)
            {
                IdentityManager.Credential newCred = null;
                if (isPortalUrl && !string.IsNullOrEmpty(cred.Url) && cred.Url.IsPortalUrl())
                {
                    newCred = cred; // If a portal credential already exists, try it
                }
                else if (!string.IsNullOrEmpty(cred.Url) 
                && cred.Url.Equals(credentialUrl, StringComparison.OrdinalIgnoreCase))
                {
                    newCred = cred; // If a credential that matches the requested URL exists, try it                
                }
                else if (!string.IsNullOrEmpty(cred.UserName) && !string.IsNullOrEmpty(cred.Password))
                {
                    try
                    {
                        // Try authenticating with the user name and password
                        newCred = await IdentityManager.Current.GenerateCredentialTaskAsync(credentialUrl, cred.UserName, cred.Password, null);
                    }
                    catch { } // Intentionally trying credentials that may not work, so swallow exceptions
                }

                if (newCred != null)
                {
                    // Try the original request URL with the new credential's token
                    string testUrl = requestUrl;

                    // Construct the URL with the token
                    if (testUrl.Contains("?"))
                        testUrl += string.Format("&token={0}&f=json", newCred.Token);
                    else
                        testUrl += string.Format("?token={0}&f=json", newCred.Token);
                    WebClient wc = new WebClient();
                    string result = null;
                    try
                    {
                        // Issue the request
                        result = await wc.DownloadStringTaskAsync(testUrl);
                    }
                    catch
                    {
                        continue;
                    }

                    if (result != null)
                    {
                        try
                        {
                            // Check whether the response contains a JSON error
                            JsonObject o = (JsonObject)JsonObject.Parse(result);
                            if (o.ContainsKey("error"))
                            {
                                o = (JsonObject)o["error"];

                                // Check the error code
                                if (o.ContainsKey("code"))
                                {
                                    int statusCode = o["code"];
                                    // Server should return 401 Unauthorized, 403 Forbidden, 498 Invalid Token, or 499
                                    // Token Required if the token was insufficient authorization.  Other errors should 
                                    // mean that the resource was accessible with the token, but was just not used 
                                    // properly.
                                    if (statusCode == 401 || statusCode == 403 || statusCode == 498 || statusCode == 499) 
                                        continue;                               
                                }
                            }
                        }
                        catch
                        {
                            // could not parse response, so it's probably HTML or an image.  Assume the 
                            // credential is valid since these types of responses are generally not returned 
                            // by Portal/Server if there is an error.
                            return newCred;
                        }
                        return newCred;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Signs into the application environment's current ArcGIS Online/Portal instance using the 
        /// specified credential
        /// </summary>
        public static async Task SignInToPortal(IdentityManager.Credential cred, ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnline agol = null)
        {
            ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnline portal = agol ?? ArcGISOnlineEnvironment.ArcGISOnline;
            if (portal.User.Current != null && !string.IsNullOrEmpty(portal.User.Current.FullName))
                return; // User is already signed in

            await portal.User.InitializeTaskAsync(cred.UserName, cred.Token);

            if (MapApplication.Current != null && MapApplication.Current.Portal != null)
            {
                MapApplication.Current.Portal.Token = cred.Token;
                await MapApplication.Current.Portal.InitializeTaskAsync();
            }
        }
    }
}
