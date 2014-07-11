/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Web;
using ESRI.ArcGIS.Mapping.Builder.Server;
using ESRI.ArcGIS.Mapping.Builder.Common;

namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    public class GetFilesRequestHandler : GetFilesRequestHandlerBase
    {        
        protected override FileDescriptor[] GetFiles(string siteId, bool isTemplate, string relativePath, string[] fileExts)
        {
            FaultContract Fault = null;
            string physicalPath = FileUtils.GetPhysicalPathForId(siteId, isTemplate, relativePath, out Fault);
            if (Fault != null)
                throw new Exception(Fault.Message);

            if (!System.IO.Directory.Exists((physicalPath)))
                throw new Exception("Unable to find files");

            List<FileDescriptor> allFiles = new List<FileDescriptor>();
            string[] dirs = FileExplorerUtility.GetDirectories(physicalPath);
            if (dirs != null && dirs.Length > 0)
            {
                foreach (string dir in dirs)
                {
                    if (string.IsNullOrEmpty(dir))
                        continue;
                    FileDescriptor fileInfo = new FileDescriptor();
                    string fileName = dir;
                    int pos = fileName.LastIndexOf('\\');
                    if (pos > -1)
                        fileName = fileName.Substring(pos + 1);
                    fileInfo.FileName = fileName;
                    fileInfo.IsFolder = true;
                    if (!string.IsNullOrEmpty(relativePath))
                        fileInfo.RelativePath = string.Format("{0}/{1}", relativePath, fileName);
                    else
                        fileInfo.RelativePath = fileName;
                    allFiles.Add(fileInfo);
                }
            }

            string[] files = FileExplorerUtility.GetFiles(physicalPath, fileExts);
            if (files != null && files.Length > 0)
            {
                foreach (string file in files)
                {
                    FileDescriptor fileInfo = new FileDescriptor();
                    string fileName = file;
                    int pos = fileName.LastIndexOf('\\');
                    if (pos > -1)
                        fileName = fileName.Substring(pos + 1);
                    fileInfo.FileName = fileName;

                    if (!string.IsNullOrEmpty(relativePath))
                        fileInfo.RelativePath = string.Format("{0}/{1}", relativePath, fileName);
                    else
                        fileInfo.RelativePath = fileName;
                    allFiles.Add(fileInfo);
                }
            }

            return allFiles.ToArray();
        }

        
    }

    internal class FileUtils
    {
        private const string TemplateFolderPath = "~/Templates";
        public static string GetPhysicalPathForId(string siteId, bool isTemplate, string relativePath, out FaultContract Fault)
        {
            Fault = null;
            string physicalPath = null;
            if (isTemplate)
            {
                Template template = TemplateConfiguration.FindTemplateById(siteId);
                if (template == null)
                {
                    Fault = new FaultContract();
                    Fault.FaultType = "Error";
                    Fault.Message = "Unable to find template with ID = " + siteId;
                    return null;
                }
                else
                {
                    physicalPath = ServerUtility.MapPath(TemplateFolderPath) + "\\" + template.ID;
                    if (!string.IsNullOrEmpty(relativePath))
                        physicalPath += "\\" + relativePath.Replace("/", "\\");
                }
            }
            else
            {
                Site site = SiteConfiguration.FindExistingSiteByID(siteId);
                if (site == null)
                {
                    Fault = new FaultContract();
                    Fault.FaultType = "Error";
                    Fault.Message = "Unable to find Site with ID = " + siteId;
                    return null;
                }
                else
                {
                    physicalPath = site.PhysicalPath;
                    if (!string.IsNullOrEmpty(relativePath))
                        physicalPath += "\\" + relativePath.Replace("/", "\\");
                }
            }
            return physicalPath;
        }
    }
}
