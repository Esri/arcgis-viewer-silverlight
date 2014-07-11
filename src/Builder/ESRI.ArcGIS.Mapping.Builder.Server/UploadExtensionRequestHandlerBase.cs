/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;

namespace ESRI.ArcGIS.Mapping.Builder.Server
{
    public abstract class UploadExtensionRequestHandlerBase : ServiceRequestHandlerBase
    {
        protected override void HandleRequest()
        {
            if (Request.Files.Count == 0)
                throw new Exception("Add-In was not sent with upload request");

            var file = Request.Files[0];
            var fileName = file.FileName;
            var fileContents = new byte[file.InputStream.Length];
            file.InputStream.Read(fileContents, 0, fileContents.Length);

            string assembliesStr = Request["assemblies"];
            string [] assemblies = assembliesStr != null ? assembliesStr.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries) : null;

            UploadExtension(fileName, assemblies, fileContents);
        }

        protected abstract void UploadExtension(string fileName, string[] assemblies, byte[] fileBytes);
    }
}
