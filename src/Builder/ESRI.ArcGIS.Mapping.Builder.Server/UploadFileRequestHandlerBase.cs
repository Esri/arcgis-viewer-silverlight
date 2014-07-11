/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Mapping.Builder.Common;
using System.IO;
using System.Xml.Serialization;

namespace ESRI.ArcGIS.Mapping.Builder.Server
{
    public abstract class UploadFileRequestHandlerBase : ServiceRequestHandlerBase
    {
        protected override void HandleRequest()
        {
            string siteId = Request["siteId"];
            bool isTemplate = false;
            string template = Request["isTemplate"];
            bool.TryParse(template, out isTemplate);
            string relativePath = Request["relativePath"];
            string fileName = Request["filename"];
            byte[] fileContents = null;
            using (BinaryReader reader = new BinaryReader(Request.InputStream))
            {
                fileContents = reader.ReadBytes((int)Request.InputStream.Length);
            }
            FileDescriptor file = UploadFile(siteId, isTemplate, relativePath, fileName, fileContents);
            XmlSerializer serializer = new XmlSerializer(typeof(FileDescriptor));
            serializer.Serialize(Response.OutputStream, file);
        }

        public abstract FileDescriptor UploadFile(string siteId, bool isTemplate, string relativePath, string fileName, byte[] fileContents);
    }
}
