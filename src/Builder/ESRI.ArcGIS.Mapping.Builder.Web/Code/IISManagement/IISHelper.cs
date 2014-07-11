/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.DirectoryServices;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Runtime.Serialization;
#if !SILVERLIGHT
using System.Collections.ObjectModel;
#endif

namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    /// <summary>
    /// This is a utility class that contains a collection of function to interact with
    /// Internet Information Server, for example, to create and remove web applications.
    /// </summary>
	[Serializable]
    internal abstract class IISHelper
    {
        /// <summary>
        /// Check IIS Server for access permissions
        /// </summary>
        /// <param name="iisPath">The Active Directory path of the virtual directory. Use GetIISLocation method to get the path.</param>
        /// <param name="server">The name of the server.</param>
        /// <param name="url">The url.</param>
        /// <param name="physicalPath">The output physical path.</param>
        /// <param name="filePath">The UNC path for the physical path.</param>
        /// <param name="status">The output status message.</param>
        /// <returns>true if check server for access succeeds; false if not.</returns>
        public static bool CheckServerForAccess(string iisPath, string server, string url,
            out string physicalPath, out string filePath, out string status)
        {
            //get physical path
            filePath = null;
            if (!GetPhysicalPath(iisPath, out physicalPath, out status))
            {
                status = String.Format("No read access to {0}", url);
                return false;
            }

            #region For non - local machines, get file share path
            if (server.ToLower() != Environment.MachineName.ToLower())
            {
                char volumeSeparator = Path.VolumeSeparatorChar;
                if (physicalPath.IndexOf(new string(volumeSeparator,1), StringComparison.Ordinal) > 0)
                {
                    filePath = physicalPath.Replace(volumeSeparator, '$');
                    filePath = String.Format("\\\\{0}\\{1}", server, filePath);
                }
                else
                    filePath = String.Format("\\\\{0}\\wwwroot$", server);
            }
            else
                filePath = physicalPath;
            #endregion

            #region Test for read / write access
            string testDirectory = Path.Combine(physicalPath, "xf000weiur");
            try
            {
                DirectoryInfo dinfo = Directory.CreateDirectory(testDirectory);
                if (dinfo == null)
                {
                    status = String.Format("No read access to {0}", physicalPath);
                    return false;
                }
            }
            catch (Exception)
            {
                status = String.Format("No read access to {0}", physicalPath);
                return false;
            }
            #endregion

            #region test for Modify access
            try
            {
                string file = Path.Combine(testDirectory, "test.txt");
                FileStream fs = File.Create(file);
                if (fs == null)
                {
                    Directory.Delete(testDirectory);
                    status = String.Format("No read/write access to {0}", testDirectory);
                    return false;
                }
                else
                {
                    fs.Close();
                    File.Delete(file);
                    Directory.Delete(testDirectory, true);
                }
            }
            catch (Exception)
            {
                status = String.Format("No read/write access to {0}.", physicalPath);
                return false;
            }
            #endregion

            status = null;

            return true;
        }

        /// <summary>
        /// Get the physical path of a virtual directory.
        /// </summary>
        /// <param name="iisPath">The Active Directory path of the virtual directory. Use GetIISLocation method to get the path.</param>
        /// <param name="physicalPath">The output physical path.</param>
        /// <param name="status">The output status message.</param>
        /// <returns>true if getting physical path succeeds, false if not.</returns>
        public static bool GetPhysicalPath(string iisPath, out string physicalPath, out string status)
        {
            try
            {
                DirectoryEntry deRoot = new DirectoryEntry(iisPath);
                physicalPath = deRoot.Properties["Path"].Value.ToString();

                status = null;
            }
            catch (Exception ex)
            {
				status = "Error " + ex.Message;
                physicalPath = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get the root directory of the IIS Server specified
        /// </summary>
        /// <param name="server">IIS Server machine name</param>
        /// <returns>Root of the IIS server specified (e.g. c:\Inetpub\wwwroot)</returns>
        public static string GetPhysicalIISRootDirectory(string server)
        {
            string path = String.Format("IIS://{0}/w3svc/1/ROOT", server);
            DirectoryEntry de = new DirectoryEntry(path);
            string dir = "";
            try
            {
                dir = de.Properties["Path"].Value.ToString();
            }
            catch
            {
                dir = "c:\\inetpub\\wwwroot";
            }
            return dir;

        }

        /// <summary>
        /// Turn a virtual directory into an application.
        /// </summary>
        /// <param name="vdir">The DirectoryEntry class for the virtual directory.</param>
        public static void CreateApplicationFromVirtualDirectory(DirectoryEntry vdir)
        {
            //create application
            vdir.Invoke("AppCreate", true);

            // Save Changes
            vdir.CommitChanges();
            vdir.Close();

            return;
        }

        /// <summary>
        /// Check if a virtual directory is an application
        /// </summary>
        /// <param name="vdir">The DirectoryEntry class for the virtual directory.</param>
        /// <returns>true if it is an application, false if not.</returns>
        public static bool IsVirtualDirectoryAnApplication(DirectoryEntry vdir)
        {
            object o = vdir.Invoke("AppGetStatus2", null);
            int status = (int)o;
            if (status != 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Check if a virtual directory exists.
        /// </summary>
        /// <param name="iisLocation">The Active Directory path of the IIS website. Use GetIISLocation method to get the path.</param>
        /// <param name="virtualDirectoryName">The name of the virtual directory.</param>
        /// <param name="status">The output status message.</param>
        /// <returns>true if the virtual directory exists, false if not.</returns>
        public static bool VirtualDirectoryExists(string iisLocation, string virtualDirectoryName, out string status)
        {
            status = null;
            string path = Path.Combine(iisLocation, virtualDirectoryName);
            DirectoryEntry deVDir;
            try
            {
                deVDir = new DirectoryEntry(path);
                string test = (string)deVDir.Properties["AppRoot"][0];
            }
            catch (Exception ex)
            {
                status = ex.Message;
                return false;
            }

            if (deVDir == null)
                return false;
            else
            {
                deVDir.Close();
                return true;
            }
        }

        /// <summary>
        /// Check if the IIS server is available.
        /// </summary>
        /// <param name="server">The name of the server.</param>
        /// <returns>true if the server is available, false if not.</returns>
        public static bool IsIISAvailable(string server)
        {
            try
            {
                DirectoryEntry W3SVC = new DirectoryEntry(String.Format("IIS://{0}/W3SVC", server));
                if (W3SVC == null)
                    return false;
                string test = (string)W3SVC.Properties["AppRoot"][0]; //if iis is not available, this will throw an exception
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if the IIS server is available and running on a specified port
        /// </summary>
        /// <param name="server">Name of the server machine, for example, MyServer.</param>
        /// <param name="port">Port in the format ":PortNumber:", for example, ":80:".</param>
        /// <returns></returns>
        public static bool IsIISAvailable(string server, string port)
        {
            try
            {
                DirectoryEntry W3SVC = new DirectoryEntry(String.Format("IIS://{0}/W3SVC", server));
                if (W3SVC == null)
                    return false;
                string test = (string)W3SVC.Properties["AppRoot"][0]; //if iis is not available, this will throw an exception
                foreach (DirectoryEntry Site in W3SVC.Children) //foreach required to enumerate all websites
                {
                    if (Site.SchemaClassName == "IIsWebServer")
                    {
                        PropertyValueCollection bindings = Site.Properties["ServerBindings"];
                        PropertyValueCollection bindingsSSL = Site.Properties["SecureBindings"];
                        if (IsBoundToPort(bindings, bindingsSSL, port))
                        {
                            return true;                            
                        }
                    }
                }                
            }
            catch
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// Get the Active Directory path for the server and the port
        /// </summary>
        /// <param name="server">Name of the server machine, for example, MyServer.</param>
        /// <param name="port">Port in the format ":PortNumber:", for example, ":80:".</param>
        /// <param name="status">The output status message.</param>
        /// <returns>Active Directory Path, for example, "IIS://MyServer/W3SVC/1/ROOT".</returns>
        public static string GetIISLocation(string server, string port, out string status)
        {
            status = String.Empty;
			string iisLocation = "IIS://localhost/W3SVC/1";
			try
			{
				if (port != null && port != "")
				{
					DirectoryEntry W3SVC = new DirectoryEntry(String.Format("IIS://{0}/W3SVC", server));
					foreach (DirectoryEntry Site in W3SVC.Children) //foreach required
					{
						if (Site.SchemaClassName == "IIsWebServer")
						{
							PropertyValueCollection bindings = Site.Properties["ServerBindings"];
							PropertyValueCollection bindingsSSL = Site.Properties["SecureBindings"];
							if (IsBoundToPort(bindings, bindingsSSL, port))
							{
								iisLocation = String.Format("IIS://{0}/W3SVC/{1}", server, Site.Name);
								break;
							}
						}
					}
				}
				else
				{
					iisLocation = String.Format("IIS://{0}/W3SVC/1", server);
				}
				DirectoryEntry deRoot = new DirectoryEntry(iisLocation);
				int i = (int)deRoot.Properties["ServerState"][0];
				if (i == 2)
					return iisLocation;
				return iisLocation;
			}
			catch (System.Runtime.InteropServices.COMException comEx)
			{
				string errorMsg = String.Format("Error accessing website on {0}. ErrorMessage:- {1}", server, comEx.Message);
				if (comEx.ErrorCode == -2147024891)
				{
					throw new UnauthorizedAccessException(errorMsg);
				}
				else
				{
					status = errorMsg;
					return iisLocation;
				}
			}
			catch (Exception ex)
			{
                status = String.Format("Error accessing website on {0}. ErrorMessage:- {1}", server, ex.Message);
				return iisLocation;
			}
        }        

        /// <summary>
        /// Gets the Identifier used for the website. Useful for servers with multiple websites. This routine does not return the 
        /// Site number.
        /// </summary>
        /// <param name="server">Name of the server machine, for example, MyServer</param>
        /// <param name="port">Port in the format ":PortNumber:", for example, ":80:"</param>
        /// <param name="status">The output status message</param>
        /// <returns>A string identifier based on ServerComment property in Metabase.xml (Eg:- Default Web Site)</returns>
        public static string GetSiteIdentifier(string server, string port, out string status)
        {
            string defaultWebSite = "Default Web Site"; 

            status = string.Empty;

            string iisLocation = GetIISLocation(server, port, out status);
            if (!string.IsNullOrEmpty(status)) // error retrieving IIS location
                return defaultWebSite;

            DirectoryEntry deRoot = new DirectoryEntry(iisLocation);
            string serverComment = Convert.ToString(deRoot.Properties["ServerComment"].Value);
            if (string.IsNullOrEmpty(serverComment))
                return defaultWebSite;

            return serverComment;
        }

        /// <summary>
        /// Gets the Identifier (site number) used for the website. Useful for servers with multiple websites.         
        /// </summary>
        /// <param name="server">Name of the server machine, for example, MyServer</param>
        /// <param name="port">Port in the format ":PortNumber:", for example, ":80:"</param>
        /// <param name="status">The output status message</param>
        /// <returns>A string identifier based on Name property in ActiveDirectory entry (Eg:- 1)</returns>
        public static string GetSiteIdentifierNumber(string server, string port, out string status)
        {
            string defaultWebSiteId = "1";

            status = string.Empty;

            string iisLocation = GetIISLocation(server, port, out status);
            if (!string.IsNullOrEmpty(status)) // error retrieving IIS location
                return defaultWebSiteId;

            DirectoryEntry deRoot = new DirectoryEntry(iisLocation);
            if (deRoot != null)
                return deRoot.Name;

            return defaultWebSiteId;
        }

        private static bool IsBoundToPort(PropertyValueCollection serverBindings, PropertyValueCollection secureBindings, string port)
        {
            if (serverBindings != null)
            {
                foreach (object binding in serverBindings)
                {
                    string value = Convert.ToString(binding);
                    if (value.Contains(port))
                        return true;
                }
            }
            if (secureBindings != null)
            {
                foreach(object binding in secureBindings)
                {
                    string value = Convert.ToString(binding);
                    if (value.Contains(port))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Verify if a website is up.
        /// </summary>
        /// <param name="iisLocation">The Active Directory path of the IIS website. Use GetIISLocation method to get the path.</param>
        /// <param name="status">The output status message.</param>
        /// <returns>true if the web site is up, false if not.</returns>
        public static bool IsWebSiteUp(string iisLocation, out string status)
        {
            status = String.Empty;
            try
            {
                DirectoryEntry deRoot = new DirectoryEntry(iisLocation);
                int i = (int)deRoot.Properties["ServerState"][0];
                if (i == 2)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                status = String.Format("Error accessing website on {0}. ErrorMessage:- {1}", iisLocation, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Create a web application with the virtual directory name. The virtual directory is physically under the root of the web site.
        /// </summary>
        /// <param name="iisLocation">The Active Directory path of the IIS website. Use GetIISLocation method to get the path.</param>
        /// <param name="virtualDirectoryName">The name of the virtual directory.</param>
        /// <param name="status">The output status message.</param>
        /// <returns>true if vdir creation succeeds, false if not</returns>
        public static bool CreateVirtualDirectory(string iisLocation, string virtualDirectoryName, out string status)
        {
            //string strSchema = "IIsWebVirtualDir";
            //When creating an application under wwwroot without a physical path, should use IIsWebDirectory instead of IIsWebVirtualDir
            string strSchema = "IIsWebDirectory";
            DirectoryEntry deParent;
            string appRoot = "";
            status = null;
            try
            {
                deParent = new DirectoryEntry(iisLocation);
                appRoot = (string)deParent.Properties["AppRoot"][0]; //  /LM/W3SVC/1/Root
            }
            catch (Exception ex)
            {
                status = String.Format("Error accessing website on {0}. ErrorMessage:- {1}", iisLocation, ex.Message);
                return false;
            }

            try
            {
                deParent.RefreshCache();
                DirectoryEntry deNewVDir = deParent.Children.Add(virtualDirectoryName, strSchema);
                string path = String.Format("{0}/{1}", appRoot, virtualDirectoryName); //  /LM/W3SVC/1/Root/q7
                deNewVDir.Properties["AppRoot"].Insert(0, path);
                deNewVDir.Properties["AppFriendlyName"].Insert(0, virtualDirectoryName);
                deNewVDir.Properties["AppIsolated"].Insert(0, 2); //A value of 0 indicates that the application is to run in-process, a value of 1 indicates out-of-process, and a value of 2 indicates a pooled-process

                deNewVDir.CommitChanges();
                deParent.CommitChanges();

                // Create a Application
                deNewVDir.Invoke("AppCreate", true);

                // Save Changes
                deNewVDir.CommitChanges();
                deParent.CommitChanges();
                deNewVDir.Close();
                deParent.Close();

            }
            catch (Exception ex)
            {
                status = ex.Message;
                return false;
            }

            status = null;
            return true;
        }

        /// <summary>
        /// Create a web application with the virtual directory name. The physical path of the virtual directory is passed in as a parameter.
        /// </summary>
        /// <param name="iisLocation">The Active Directory path of the IIS website. Use GetIISLocation method to get the path.</param>
        /// <param name="virtualDirectoryName">The name of the virtual directory.</param>
        /// <param name="physicalPath">The physical path.</param>
        /// <param name="status">The output status message.</param>
        /// <returns>true if vdir creation succeeds, false if not</returns>
        public static bool CreateVirtualDirectory(string iisLocation, string virtualDirectoryName, string physicalPath, out string status)
        {
            string strSchema = "IIsWebVirtualDir";
            DirectoryEntry deParent;
            string appRoot = "";
            status = null;
            try
            {
                deParent = new DirectoryEntry(iisLocation);
                appRoot = (string)deParent.Properties["AppRoot"][0]; //  /LM/W3SVC/1/Root
            }
            catch (Exception ex)
            {
                status = String.Format("Error accessing website on {0}. ErrorMessage:- {1}", iisLocation, ex.Message);
                return false;
            }

            try
            {
                deParent.RefreshCache();
                DirectoryEntry deNewVDir = deParent.Children.Add(virtualDirectoryName, strSchema);
                string path = String.Format("{0}/{1}", appRoot, virtualDirectoryName); //  /LM/W3SVC/1/Root/q7
                deNewVDir.Properties["AppRoot"].Insert(0, path);
                deNewVDir.Properties["AppFriendlyName"].Insert(0, virtualDirectoryName);
                deNewVDir.Properties["AppIsolated"].Insert(0, 2); //A value of 0 indicates that the application is to run in-process, a value of 1 indicates out-of-process, and a value of 2 indicates a pooled-process
                deNewVDir.Properties["Path"][0] = physicalPath;

                deNewVDir.CommitChanges();
                deParent.CommitChanges();

                // Create a Application
                deNewVDir.Invoke("AppCreate", true);

                // Save Changes
                deNewVDir.CommitChanges();
                deParent.CommitChanges();
                deNewVDir.Close();
                deParent.Close();

            }
            catch (Exception ex)
            {
                status = ex.Message;
                return false;
            }

            status = null;
            return true;
        }

        /// <summary>
        /// Delete a virtual directory
        /// </summary>
        /// <param name="iisLocation">The Active Directory path of the IIS website. Use GetIISLocation method to get the path.</param>
        /// <param name="virtualDirectoryName">The name of the virtual directory.</param>
        /// <param name="status">The output status message.</param>
        /// <returns>true if vdir deletion succeeds, false if not</returns>
        public static bool DeleteVirtualDirectory(string iisLocation, string virtualDirectoryName, out string status)
        {
            if (virtualDirectoryName != null && virtualDirectoryName.StartsWith("/", StringComparison.Ordinal))
                virtualDirectoryName = virtualDirectoryName.Substring(1);
            if (VirtualDirectoryExists(iisLocation, virtualDirectoryName, out status))
            {
                try
                {
                    object[] objParams = new object[2] { "IISWebVirtualDir", virtualDirectoryName };
                    DirectoryEntry dirEntry = new DirectoryEntry(iisLocation);

                    dirEntry.Invoke("Delete", objParams);
                    dirEntry.CommitChanges();
                }
                catch (Exception ex)
                {
                    status = ex.Message;
                    return false;
                }
            }

            status = null;
            return true;
        }

        /// <summary>
        /// Rename a virtual directory.
        /// </summary>
        /// <param name="iisLocation">The Active Directory path of the IIS website. Use GetIISLocation method to get the path.</param>
        /// <param name="oldDirectoryName">The old virtual directory name.</param>
        /// <param name="newDirectoryName">The new virtual directory name.</param>
        /// <param name="status">The output status message.</param>
        /// <returns>true if vdir rename succeeds, false if not</returns>
        public static bool RenameVirtualDirectory(string iisLocation, string oldDirectoryName, string newDirectoryName, out string status)
        {
            status = null;
            try
            {
                DirectoryEntry iisEntry = new DirectoryEntry(iisLocation);

                DirectoryEntry oldDirectoryEntry = iisEntry.Children.Find(oldDirectoryName, "");
                oldDirectoryEntry.Rename(newDirectoryName);

                oldDirectoryEntry.CommitChanges();
                iisEntry.CommitChanges();
                oldDirectoryEntry.Close();
                iisEntry.Close();
            }
            catch (Exception ex)
            {
                status = ex.Message;
                return false;
            }

            status = null;
            return true;
        }

        /// <summary>
        /// Change the physical path of a virtual directory.
        /// </summary>
        /// <param name="iisLocation">The Active Directory path of the IIS website. Use GetIISLocation method to get the path.</param>
        /// <param name="virtualDirectoryName">The name of the virtual directory.</param>
        /// <param name="newPhysicalPath">The new physical path.</param>
        /// <param name="status">The output status message.</param>
        /// <returns>true if vdir rename succeeds, false if not.</returns>
        public static bool ChangePhysicalPathOfVirtualDirectory(string iisLocation, string virtualDirectoryName, string newPhysicalPath, out string status)
        {
            try
            {
                DirectoryEntry iisEntry = new DirectoryEntry(iisLocation);
                DirectoryEntry dirToChange = iisEntry.Children.Find(virtualDirectoryName, "");
                dirToChange.Properties["Path"][0] = newPhysicalPath;

                dirToChange.CommitChanges();
                iisEntry.CommitChanges();
            }
            catch (Exception ex)
            {
                status = ex.Message;
                return false;
            }

            status = null;
            return true;
        }

        /// <summary>
        /// Set a single propery of the virtual directory.
        /// </summary>
        /// <param name="iisLocation">The Active Directory path of the IIS website. Use GetIISLocation method to get the path.</param>
        /// <param name="virtualDirectoryName">The name of the virtual directory.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="newValue">The new value of the property.</param>
        /// <param name="status">The output status message.</param>
        /// <returns>true if setting property value succeeds, false if not.</returns>
        public static bool SetSingleProperty(string iisLocation, string virtualDirectoryName, string propertyName, object newValue, out string status)
        {
            //  iisLocation is of the form "IIS://<servername>/<path>"
            //    for example "IIS://localhost/W3SVC/1" 
            //  propertyName is of the form "<propertyName>", for example "ServerBindings"
            //  newValue is of the form "<intStringOrBool>", for example, ":80:"
            status = null;

            try
            {
                DirectoryEntry iisEntry = new DirectoryEntry(iisLocation);
                DirectoryEntry dirToChange = iisEntry.Children.Find(virtualDirectoryName, "");

                PropertyValueCollection propValues = dirToChange.Properties[propertyName];
                string oldType = propValues.Value.GetType().ToString();
                string newType = newValue.GetType().ToString();

                if (newType == oldType)
                {
                    dirToChange.Properties[propertyName][0] = newValue;
                    dirToChange.CommitChanges();
                    iisEntry.CommitChanges();
                }
                else
                {
                    status = "Error:- No property type match";
                    return false;
                }
            }
            catch (Exception ex)
            {
                if ("HRESULT 0x80005006" == ex.Message)
                    status = string.Format("Property {0} doesn't exist in {1}", propertyName, virtualDirectoryName);
                else
                    status = ex.Message;

                return false;
            }

            status = null;
            return true;
        }
        
        /// <summary>
        /// Retrieves the list of virtual directories from the default website on IIS
        /// </summary>
        /// <param name="server">Hostname of the IIS server</param>
        /// <returns>list of virtual directories from the default website on IIS</returns>
        public static List<IIsVirtualDirectoryInfo> GetIISVirtualDirectories(string server)
        {
            return GetIISVirtualDirectories(server, "1"); // get using default website
        }

        /// <summary>
        /// Retrieves the list of virtual directories from an IIS Website
        /// </summary>
        /// <param name="server">Hostname of the IIS server</param>
        /// <param name="webSiteID">Web virtual servers are identified in the metabase by their index numbers. The first Web server is number 1, the second is number 2, and so on.</param>        
        /// <returns>list of virtual directories from an IIS Website  in the form of IIsVirtualDirectoryInfo objects</returns>
        public static List<IIsVirtualDirectoryInfo> GetIISVirtualDirectories(string server, string webSiteID)
        {
            List<IIsVirtualDirectoryInfo> virtualDirs = new List<IIsVirtualDirectoryInfo>();
            DirectoryEntry obDirEntry = null;
            DirectoryEntries obDirEntList = null;
            try
            {
                string webSiteMetabasePath = string.Format("IIS://{0}/W3svc/{1}/Root", server, webSiteID);
                obDirEntry = new DirectoryEntry(webSiteMetabasePath);                
                obDirEntList = obDirEntry.Children;

                // ensure that the website path is UNC format (for remote IIS servers)
                string webSiteFilePath = obDirEntry.Properties["Path"].Value.ToString();
                string webSitePhysicalDirectory = webSiteFilePath;
                if (string.Compare(server, Environment.MachineName.ToLower(), true) != 0) // non local machines
                {
                    char volumeSeparator = System.IO.Path.VolumeSeparatorChar;
                    if (webSiteFilePath.IndexOf(new string(volumeSeparator,1), StringComparison.Ordinal) > 0)
                    {
                        webSitePhysicalDirectory = webSitePhysicalDirectory.Replace(volumeSeparator, '$');
                        webSitePhysicalDirectory = String.Format("\\\\{0}\\{1}", server, webSitePhysicalDirectory);
                    }
                }

                // Process each child entry and add the name of virtual directory
                foreach (DirectoryEntry objChildDE in obDirEntList)
                {
                    GetNestedVirtualDirectoriesForWebDirectory(virtualDirs, objChildDE, webSiteMetabasePath, webSitePhysicalDirectory);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return null;
            }
            return virtualDirs;
        }

        /// <summary>
        /// Recursive helper routine to look within IIsWebDirectories
        /// </summary>
        /// <param name="virtualDirs">List of virtual directory objects to append to.</param>
        /// <param name="objChildDE">The current Directory entry to look under.</param>
        /// <param name="webSiteMetabasePath">ADSI path for the website that houses the v-dir</param>
        /// <param name="webSitePhysicalDirectory">UNC path for the website that houses the v-dir</param>
        private static void GetNestedVirtualDirectoriesForWebDirectory(List<IIsVirtualDirectoryInfo> virtualDirs, DirectoryEntry objChildDE,
                                    string webSiteMetabasePath, string webSitePhysicalDirectory)
        {
            // Check if the schema class is IIsWebVirtualDir or not.
            if (0 == string.Compare(objChildDE.SchemaClassName, "IIsWebVirtualDir"))
            {
                IIsVirtualDirectoryInfo virtualDir = new IIsVirtualDirectoryInfo();

                string adsiPath = objChildDE.Path;

                // IIS://Localhost/W3svc/1/Root/Dir3/nested
                virtualDir.ADSIPath = adsiPath;
                virtualDir.Name = objChildDE.Name;

                // remove the website ADSI section to make it of the format /Dir3/nested
                string webAppVirtualPath = adsiPath.Replace(webSiteMetabasePath, string.Empty);
                virtualDir.VirtualPath = webAppVirtualPath;

                string webAppPath = (string)objChildDE.Properties["Path"].Value;
                if (!string.IsNullOrEmpty(webAppPath))
                {
                    virtualDir.PhysicalPath = webAppPath;
                }
                else // we couldn't get the path property
                {
                    // make it a physical path relative to IIS root
                    webAppPath = webAppVirtualPath.Replace("/", "\\");
                    virtualDir.PhysicalPath = string.Format("{0}{1}", webSitePhysicalDirectory, webAppPath);
                }

                virtualDirs.Add(virtualDir);
            }
            else if (0 == string.Compare(objChildDE.SchemaClassName, "IIsWebDirectory"))
            {
                DirectoryEntries obDirEntList = objChildDE.Children;
                // Process each child entry and add the name of virtual directory
                foreach (DirectoryEntry objChild in obDirEntList)
                {
                    GetNestedVirtualDirectoriesForWebDirectory(virtualDirs, objChild, webSiteMetabasePath, webSitePhysicalDirectory);
                }
            }
        }

		/// <summary>
		/// Retrieves the list of application directories from an IIS Website
		/// </summary>
		/// <param name="server">Hostname of the IIS server</param>
		/// <param name="webSiteID">Web virtual servers are identified in the metabase by their index numbers. The first Web server is number 1, the second is number 2, and so on.</param>        
		/// <returns>list of application directories from an IIS Website in the form of IIsVirtualDirectoryInfo objects</returns>
		public static List<IIsVirtualDirectoryInfo> GetIISApplicationDirectories(string server, string webSiteID)
		{
			List<IIsVirtualDirectoryInfo> virtualDirs = new List<IIsVirtualDirectoryInfo>();
			DirectoryEntry obDirEntry = null;
			DirectoryEntries obDirEntList = null;
			try
			{
				string webSiteMetabasePath = string.Format("IIS://{0}/W3svc/{1}/Root", server, webSiteID);
				obDirEntry = new DirectoryEntry(webSiteMetabasePath);
				obDirEntList = obDirEntry.Children;

				// ensure that the website path is UNC format (for remote IIS servers)
				string webSiteFilePath = obDirEntry.Properties["Path"].Value.ToString();
				string webSitePhysicalDirectory = webSiteFilePath;
				//if (string.Compare(server, Environment.MachineName.ToLower(), true) != 0) // non local machines
				//{
				//    char volumeSeparator = System.IO.Path.VolumeSeparatorChar;
				//    if (webSiteFilePath.IndexOf(volumeSeparator) > 0)
				//    {
				//        webSitePhysicalDirectory = webSitePhysicalDirectory.Replace(volumeSeparator, '$');
				//        webSitePhysicalDirectory = String.Format("\\\\{0}\\{1}", server, webSitePhysicalDirectory);
				//    }
				//}

				// Process each child entry and add the name of virtual directory
				foreach (DirectoryEntry objChildDE in obDirEntList)
				{
					GetNestedApplicationDirectoriesForWebDirectory(virtualDirs, objChildDE, webSiteMetabasePath, webSitePhysicalDirectory);
				}
			}
			catch (Exception ex)
			{
				Console.Write(ex.Message);
				return null;
			}
			return virtualDirs;
		}

		/// <summary>
		/// Recursive helper routine to look within IIsWebDirectories
		/// </summary>
        /// <param name="applicationDirs">List of application directory objects to append to.</param>
		/// <param name="objChildDE">The current Directory entry to look under.</param>
		/// <param name="webSiteMetabasePath">ADSI path for the website that houses the app-dir</param>
		/// <param name="webSitePhysicalDirectory">UNC path for the website that houses the app-dir</param>
		private static void GetNestedApplicationDirectoriesForWebDirectory(List<IIsVirtualDirectoryInfo> applicationDirs, DirectoryEntry objChildDE,
									string webSiteMetabasePath, string webSitePhysicalDirectory)
		{
			if(IsVirtualDirectoryAnApplication(objChildDE))
			{
				IIsVirtualDirectoryInfo virtualDir = new IIsVirtualDirectoryInfo();

				string adsiPath = objChildDE.Path;

				// IIS://Localhost/W3svc/1/Root/Dir3/nested
				virtualDir.ADSIPath = adsiPath;
                virtualDir.Name = objChildDE.Name;

				// remove the website ADSI section to make it of the format /Dir3/nested
				string webAppVirtualPath = adsiPath.Replace(webSiteMetabasePath, string.Empty);
				virtualDir.VirtualPath = webAppVirtualPath;

				string webAppPath = (string)objChildDE.Properties["Path"].Value;
				if (!string.IsNullOrEmpty(webAppPath))
				{
					virtualDir.PhysicalPath = webAppPath;
				}
				else // we couldn't get the path property
				{
					// make it a physical path relative to IIS root
					webAppPath = webAppVirtualPath.Replace("/", "\\");
					virtualDir.PhysicalPath = string.Format("{0}{1}", webSitePhysicalDirectory, webAppPath);
				}

				applicationDirs.Add(virtualDir);
			}
			else if (0 == string.Compare(objChildDE.SchemaClassName, "IIsWebDirectory") ||
					 0 == string.Compare(objChildDE.SchemaClassName, "IIsWebVirtualDir"))
			{
				DirectoryEntries obDirEntList = objChildDE.Children;
				// Process each child entry and add the name of virtual directory
				foreach (DirectoryEntry objChild in obDirEntList)
				{
					GetNestedApplicationDirectoriesForWebDirectory(applicationDirs, objChild, webSiteMetabasePath, webSitePhysicalDirectory);
				}
			}
		}

        /// <summary>
        /// Gets a list of websites on an IIS Server
        /// </summary>
        /// <param name="server">A list of IISWebsite information objects</param>
        /// <returns>list of websites on an IIS Server in the form of IIsWebsiteInfo objects</returns>
        public static List<IIsWebsiteInfo> GetWebsitesOnIISServer(string server)
        {
            List<IIsWebsiteInfo> webSites = new List<IIsWebsiteInfo>();
            DirectoryEntry W3SVC = new DirectoryEntry(string.Format("IIS://{0}/W3SVC", server));
            foreach (DirectoryEntry Site in W3SVC.Children)
            {
                if (0 == string.Compare(Site.SchemaClassName, "IIsWebServer"))
                {
                    IIsWebsiteInfo webSite = new IIsWebsiteInfo();
                    webSite.WebsiteID = Site.Name;
                    string serverComment = Convert.ToString(Site.Properties["ServerComment"].Value);
                    if (!string.IsNullOrEmpty(serverComment))
                    {
                        webSite.FriendlyName = serverComment;
                    }
                    List<string> ports = new List<string>();                    
                    PropertyValueCollection bindings = Site.Properties["ServerBindings"];
                    PropertyValueCollection bindingsSSL = Site.Properties["SecureBindings"];
                    if (bindings != null)
                    {
                        foreach (object binding in bindings)
                        {
                            ports.Add((string)binding);
                        }                        
                    }
                    if (bindingsSSL != null)
                    {
                        webSite.UsingSSL = true;
                        foreach (object binding in bindingsSSL)
                        {
                            ports.Add((string)binding);
                        }
                    }
                    webSite.Ports = new List<string>();
                    foreach (string port in ports)
                    {
                        webSite.Ports.Add(port);
                    }
                    webSites.Add(webSite);
                }
            }
            return webSites;
        }
    }    
}



