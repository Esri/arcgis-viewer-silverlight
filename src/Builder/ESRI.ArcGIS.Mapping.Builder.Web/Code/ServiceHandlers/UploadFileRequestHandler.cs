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
    public class UploadFileRequestHandler : UploadFileRequestHandlerBase
    {
        public override FileDescriptor UploadFile(string siteId, bool isTemplate, string relativePath, string fileName, byte[] fileContents)
        {
            FaultContract Fault = null;
            string physicalPath = FileUtils.GetPhysicalPathForId(siteId, isTemplate, relativePath, out Fault);
            if (Fault != null)
                throw new Exception(Fault.Message);

            if (!System.IO.Directory.Exists((physicalPath)))
                throw new Exception("Unable to upload file");

            string filePath = string.Format("{0}\\{1}", physicalPath, fileName);
            FileExplorerUtility.UploadFile(filePath, fileContents, out Fault);
            FileDescriptor newFile = new FileDescriptor();
            newFile.FileName = fileName;
            if (relativePath != null)
                newFile.RelativePath = string.Format("{0}/{1}", relativePath, fileName);
            else
                newFile.RelativePath = fileName;
            return newFile;
        }
    }
}
