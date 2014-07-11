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
using System.Xml.Serialization;

namespace ESRI.ArcGIS.Mapping.Builder.Server
{
    public abstract class GetFilesRequestHandlerBase : ServiceRequestHandlerBase
    {
        protected override void HandleRequest()
        {
            string siteId = Request["siteId"];
            bool isTemplate = false;
            string template = Request["isTemplate"];
            bool.TryParse(template, out isTemplate);
            string relativePath = Request["relativePath"];
            string fileExt = Request["fileExts"];
            string[] fileExtenions = fileExt != null ? fileExt.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries) : null;

            FileDescriptor[] files = GetFiles(siteId, isTemplate, relativePath, fileExtenions);
            XmlSerializer serializer = new XmlSerializer(typeof(FileDescriptor[]));
            serializer.Serialize(Response.OutputStream, files);
        }

        protected abstract FileDescriptor[] GetFiles(string siteId, bool isTemplate, string relativePath, string[] fileExts);
    }
}
