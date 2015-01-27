using System;
using System.Net;
using System.Net.Browser;

namespace ESRI.ArcGIS.Mapping.Core
{
    /// <summary>
    /// Provides WebRequest objects that are differentiated depending on whether the requested Uri 
    /// resolves to an ArcGIS Server token generation endpoint
    /// </summary>
    public class ArcGISTokenWebRequestProvider : IWebRequestCreate
    {
        private static ArcGISTokenWebRequestProvider m_instance = new ArcGISTokenWebRequestProvider();

        /// <summary>
        /// Gets an ArcGISTokenWebRequestProvider instance
        /// </summary>
        public static ArcGISTokenWebRequestProvider Instance
        {
            get { return m_instance; }
        }

        /// <summary>
        /// Returns a WebRequest instance for the provided Uri
        /// </summary>
        public WebRequest Create(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            else if (!string.IsNullOrEmpty(uri.AbsolutePath))
            {
                // Get the last part of the URL
                var parts = uri.AbsolutePath.ToLower().Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
                var lastPart = parts[parts.Length - 1];
                if (lastPart == "tokens") // If the URL ends with "tokens," assume it is an ArcGIS Server token generation endpoint
                {
                    // Register all subsequent requests to the server to use Silverlight client HTTP handling.  This works around 
                    // authentication issues between Silverlight and ArcGIS Server in Chrome and Firefox.
                    // See http://msdn.microsoft.com/en-us/library/dd920295(v=vs.95).aspx for further information.
                    var result = WebRequest.RegisterPrefix(string.Format("http://{0}", uri.Host), WebRequestCreator.ClientHttp);
                    result = WebRequest.RegisterPrefix(string.Format("https://{0}", uri.Host), WebRequestCreator.ClientHttp);

                    // Return the Silverlight Client WebRequest object
                    return WebRequestCreator.ClientHttp.Create(uri);                    
                }
                else // Not a token endpoint, so use browser HTTP handling
                {
                    // Return the browser WebRequest object
                    return WebRequestCreator.BrowserHttp.Create(uri);
                }
            }
            else // No path included in the URL, so use browser HTTP handling
            {
                // Return the browser WebRequest object
                return WebRequestCreator.BrowserHttp.Create(uri);
            }
        }
    }
}
