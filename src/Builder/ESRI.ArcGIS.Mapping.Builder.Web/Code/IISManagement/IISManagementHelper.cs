/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.IO;
using ESRI.ArcGIS.Mapping.Builder.Common;

namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    public class IISManagementHelper 
    {
        public IIsVirtualDirectoryInfo CreateNewSite(Site site, string webSiteId, out FaultContract Fault)
        {
            string status = string.Empty;
            Fault = null;
            if (!string.IsNullOrEmpty(site.IISPath) && !string.IsNullOrEmpty(site.IISHost))
            {
                string iisLocation = string.Empty;

                // Check if we can query IIS
                string pathToQuery = string.Format("IIS://{0}/W3SVC", site.IISHost);                

                // Get the location of the the website running at this host and that port .. as Active Directory entry
                iisLocation = IISHelper.GetIISLocation(site.IISHost, string.Format(":{0}:", site.IISPort), out status);

                string websitePhysicalPath = GetPhysicalPathForSite(site, false, out Fault);
                if (!string.IsNullOrEmpty(status))
                {
                    Fault = new FaultContract
                    {
                        FaultType = "Error",
                        Message = "Error retrieving physical path for site.Server:- " + site.IISHost + " Port:- " + site.IISPort + " Status:- " + status
                    };
                    return null;
                }

                // When doing File I/O .. we need UNC paths .. but for IIS Helper .. we need a normal path
                string websiteUNCPath = GetPhysicalPathForSite(site, true, out Fault);
                if (!Directory.Exists(websiteUNCPath))
                {
                    Directory.CreateDirectory(websiteUNCPath);
                }
                if (IISHelper.VirtualDirectoryExists(iisLocation, site.IISPath, out status))
                {
                    Fault = new FaultContract
                    {
                        FaultType = "Error",
                        Message = "Virtual Directory already exists" + status
                    };
                    return null;
                }
                if (IISHelper.CreateVirtualDirectory(iisLocation + "/Root", site.IISPath, websitePhysicalPath, out status))
                {
                    IIsVirtualDirectoryInfo vDir = new IIsVirtualDirectoryInfo {
                        ADSIPath = pathToQuery + "/" + site.IISPath,
                         Name = site.Name,
                        PhysicalPath = websiteUNCPath,
                         VirtualPath = site.IISPath
                    };
                    return vDir;
                }
            }
            return null;
        }

        public bool DeleteSite(Site site, out FaultContract Fault)
        {
            Fault = null;
            string iisLocation = "";
            string innerStatus = "";

            try
            {
                iisLocation = IISHelper.GetIISLocation(site.IISHost, String.Format(":{0}:", site.IISPort), out innerStatus);
                if (!string.IsNullOrEmpty(iisLocation))
                {
                    string siteName = site.IISPath; //of the format Root/Folder/SubFolder
                    if (IISHelper.DeleteVirtualDirectory(iisLocation + "/Root", siteName, out innerStatus))
                    {
                        string iisPhysicalPath = "";
                        string websitePhysPath = "";

                        siteName = siteName.Replace("/", "\\");
                        if (IISHelper.GetPhysicalPath(iisLocation + "/Root", out iisPhysicalPath, out innerStatus))
                        {
                            websitePhysPath = iisPhysicalPath + "\\" + siteName;
                            websitePhysPath = ConvertToUNCPath(site.IISHost, websitePhysPath);
                            try
                            {
                                if (Directory.Exists(websitePhysPath))
                                {
                                    Directory.Delete(websitePhysPath, true);
                                }
                            }
                            catch (IOException)
                            {
                                // Sometimes the delete operations fail on first attempt on IIS 7 machines
                                // So retry once more
                                try
                                {
                                    if (Directory.Exists(websitePhysPath))
                                    {
                                        Directory.Delete(websitePhysPath, true);
                                    }
                                }
                                catch (IOException ioe)
                                {
                                    Fault = new FaultContract
                                    {
                                        FaultType = "Error",
                                        Message = "Error deleting directory " + ioe.ToString()
                                    };
                                    return false;
                                }
                            }
                        }
                    }
                    else
                    {                        
                        return false;
                    }
                }
                else
                {                    
                    return false;
                }
            }            
            catch (Exception)
            {   
                return false;
            }            
            return true;
        }

        public bool RestartSite(Site site)
        {
            string iisLocation = "";
            string innerStatus = "";
            string status = "";
            try
            {
                iisLocation = IISHelper.GetIISLocation(site.IISHost, String.Format(":{0}:", site.IISPort), out innerStatus);
                if (!string.IsNullOrEmpty(iisLocation))
                {
                    string siteName = site.IISPath; //of the format Root/Folder/SubFolder

                    string iisPhysicalPath = "";
                    string websitePhysPath = "";

                    siteName = siteName.Replace("/", "\\");
                    if (IISHelper.GetPhysicalPath(iisLocation + "/Root", out iisPhysicalPath, out innerStatus))
                    {
                        websitePhysPath = iisPhysicalPath + "\\" + siteName;
                        websitePhysPath = ConvertToUNCPath(site.IISHost, websitePhysPath);

                        FileInfo info = new FileInfo(Path.Combine(websitePhysPath, "web.config"));
                        if (info.Exists)
                        {
                            info.LastWriteTimeUtc = DateTime.Now.ToUniversalTime();
                        }
                        else
                        {                            
                            return false;
                        }
                    }
                }
                else
                {
                    status = innerStatus;
                    return false;
                }
            }
            catch (Exception ex)
            {                
                status = ex.Message;
                return false;
            }
            status = string.Empty;
            return true;
        }

        public string CopySite(Site site, string newSiteName, out FaultContract Fault)
        {
            Fault = null;
            string status = string.Empty;
            string newSitePhysPath = string.Empty;
            if (CopyWebApplication(site, site.Name, newSiteName, out newSitePhysPath, out status, false))
                return newSitePhysPath;
            else
            {
                Fault = new FaultContract
                {
                    FaultType = "Error",
                    Message = "Error copying site " + status
                };
                return null;
            }
        }

        public string RenameSite(Site site, string oldSiteName, string newSiteName, out FaultContract Fault)
        {
            Fault = null;
            string status = string.Empty;
            string newSitePhysPath = string.Empty;
            if (CopyWebApplication(site, oldSiteName, newSiteName, out newSitePhysPath, out status, true))
                return newSitePhysPath;
            else
            {
                Fault = new FaultContract
                {
                    FaultType = "Error",
                    Message = "Error renaming site " + status
                };
                return null;
            }
        }

        private bool CopyWebApplication(Site site, string oldSiteName, string newSiteName, out string newSitePhysPath, out string status, bool deleteOldSite)
        {
            // Values that need to be returned to caller
            newSitePhysPath = string.Empty;
            status = string.Empty;

            // Gets the active Directory location for IIS
            string iisLocation = string.Empty;
            iisLocation = IISHelper.GetIISLocation(site.IISHost, String.Format(":{0}:", site.IISPort), out status);
            if (string.IsNullOrEmpty(iisLocation))
                return false;

            // Get the physical location of IIS
            string iisPhysicalPath = string.Empty;
            if (!IISHelper.GetPhysicalPath(iisLocation + "/Root", out iisPhysicalPath, out status))
                return false;

            // The passed in structure is Root/NewFolder/SubFolder .. change it to windows path Root\NewFolder\SubFolder
            string oldSiteWinPath = oldSiteName.Replace("/", "\\");
            string newSiteWinPath = newSiteName.Replace("/", "\\");

            // Get the full path
            string oldSitePhysPath = iisPhysicalPath + "\\" + oldSiteWinPath;
            newSitePhysPath = iisPhysicalPath + "\\" + newSiteWinPath;

            // Ensure that the paths are UNC paths
            oldSitePhysPath = ConvertToUNCPath(site.IISHost, oldSitePhysPath);
            newSitePhysPath = ConvertToUNCPath(site.IISHost, newSitePhysPath);

            // Create the directory
            try
            {
                Directory.CreateDirectory(newSitePhysPath);
            }
            catch (Exception ex)
            {
                status = ex.Message;
                return false;
            }

            if (isTryingToNest(oldSitePhysPath, newSitePhysPath))
            {
                try
                {
                    Directory.Delete(newSitePhysPath);
                }
                catch { }
                status = "Trying to nest folders";
                return false;
            }

            // Create the virtual directory for this new location
            if (!IISHelper.CreateVirtualDirectory(iisLocation + "/Root", newSiteName, out status))
                return false;

            if (deleteOldSite)
            {
                if (!IISHelper.DeleteVirtualDirectory(iisLocation + "/Root", oldSiteName, out status))
                    return false;
            }

            // Ensure that the paths end with \\
            if (!oldSitePhysPath.EndsWith("\\", StringComparison.Ordinal)) oldSitePhysPath += "\\";
            if (!newSitePhysPath.EndsWith("\\", StringComparison.Ordinal)) newSitePhysPath += "\\";

            try
            {
                IOHelper.CopyDirectoryContents(oldSitePhysPath, newSitePhysPath, true);
            }
            catch (Exception exception)
            {
                status = exception.Message;
                return false;
            }

            if (deleteOldSite) 
            {
                // Delete old directory may be considered non fatal
                try
                {
                    Directory.Delete(oldSitePhysPath, true);
                }
                catch (Exception)
                {
                    //TODO:- log
                }
            }
            return true;
        }

        public string GetPhysicalPathForSite(Site site, bool makeUNCPath, out FaultContract Fault)
        {
            Fault = null;
            string status = string.Empty;
            string iisLocation = string.Empty;
            // Get the location of the the website running at this host and that port .. as Active Directory entry
            iisLocation = IISHelper.GetIISLocation(site.IISHost, String.Format(":{0}:", site.IISPort), out status);
            if (!string.IsNullOrEmpty(status))
            {
                Fault = new FaultContract
                {
                    FaultType = "Error",
                    Message = "Unable to retrieve IIS location for " + site.IISHost + " Port " + site.IISPort
                };
                return null;
            }
            string iisPhysicalPath = string.Empty;
            if (!string.IsNullOrEmpty(iisLocation))
            {
                string websitePhysicalPath = string.Empty;

                // Check if we can query IIS
                string pathToQuery = iisLocation + "/Root";

                // get the physical location of IIS on the server
                IISHelper.GetPhysicalPath(iisLocation + "/Root", out iisPhysicalPath, out status);
                if (string.IsNullOrEmpty(iisPhysicalPath))
                {
                    Fault = new FaultContract
                    {
                        FaultType = "Error",
                        Message = "Unable to retrieve IIS location for " + site.IISHost + " Port " + site.IISPort
                    };
                    return null;    
                }
                if (!iisPhysicalPath.EndsWith("\\", StringComparison.Ordinal))
                    iisPhysicalPath = iisPhysicalPath + "\\";

                string virtualDir = site.IISPath;
                virtualDir = virtualDir.Replace("/", "\\"); // replace forward slashes to backward slashes for path

                if (isRemoteServer(site.IISHost) && makeUNCPath)
                {
                    // We need a UNC path ... eg:- \\maestro\C$\Inetput\wwwroot
                    iisPhysicalPath = "\\\\" + site.IISHost + "\\" + iisPhysicalPath.Replace(':', '$');
                    websitePhysicalPath = iisPhysicalPath + virtualDir;
                }
                else
                {
                    websitePhysicalPath = Path.Combine(iisPhysicalPath, virtualDir);
                }

                return websitePhysicalPath;
            }
            else
            {
                Fault = new FaultContract
                {
                    FaultType = "Error",
                    Message = "Unable to retrieve IIS location for " + site.IISHost + " Port " + site.IISPort
                };
                return null;                
            }            
        }

        public bool ValidateWebServerOnHost(string hostname, int port)
        {
            try
            {
                string portNum = string.Format(":{0}:", port);
                return IISHelper.IsIISAvailable(hostname, portNum);
            }
            catch { return false; }
        }

        public string GetIISLocation(string server, int port, out FaultContract Fault)
        {
            Fault = null;
            string status;
            string iisLocation = IISHelper.GetIISLocation(server, string.Format(":{0}:", port), out status);
            if (!string.IsNullOrEmpty(status))
            {
                Fault = new FaultContract() { 
                     FaultType = "Error",
                      Message = "Unable to retrieve IIS location",
                };
                return null;
            }
            return iisLocation;
        }

        public IIsWebsiteInfo[] GetWebSitesOnIIsWebServer(string server, out FaultContract fault)
        {            
            fault = null;
            try
            {
                return IISHelper.GetWebsitesOnIISServer(server).ToArray();
            }
            catch (Exception ex)
            {
                fault = new FaultContract
                {
                    Message = ex.ToString(),
                    FaultType = "Error retrieving websites on " + server
                };
                return null;
            }
        }

        public IIsVirtualDirectoryInfo[] GetVirtualDirectoriesOnIIsWebSite(string server, string webSiteIdentifier)
        {            
            return IISHelper.GetIISVirtualDirectories(server, webSiteIdentifier).ToArray();
        }

        #region Private Helpers
        private bool isRemoteServer(string host)
        {
            if (host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase))
                return false;

            if (host.Equals(Environment.MachineName, StringComparison.InvariantCultureIgnoreCase))
                return false;

            return true;
        }

        private string ConvertToUNCPath(string hostName, string localPath)
        {
            hostName = ReplaceLocalhostWithMachineName(hostName);
            // when building application on local machine .. do not convert to UNC, since it won't work in disconnected envirorment.
            if (string.Compare(hostName, Environment.MachineName, true) == 0) 
            {
                return localPath;
            }
            return @"\\" + hostName + @"\" + localPath.Replace(':', '$');
        }

        private string ReplaceLocalhostWithMachineName(string hostName)
        {
            if (string.Compare(hostName, "localhost", true) == 0)
            {
                return Environment.MachineName;
            }
            else
            {
                return hostName;
            }
        }

        private bool isTryingToNest(string oldPath, string newPath)
        {
            DirectoryInfo oldDir = new DirectoryInfo(oldPath);
            DirectoryInfo newDir = new DirectoryInfo(newPath);

            bool isNested = false;
            while (newDir != null)
            {
                if (string.Compare(newDir.FullName, oldDir.FullName, true) == 0) // directories match
                {
                    isNested = true;
                    break;
                }
                newDir = newDir.Parent;
            }

            return isNested;
        }
        #endregion
    }
}
