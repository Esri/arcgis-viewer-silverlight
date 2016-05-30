/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Mapping.Builder.Common;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace ESRI.ArcGIS.Mapping.Builder.Server
{
    public abstract class SaveSettingsRequestHandlerBase : ServiceRequestHandlerBase
    {
        protected override void HandleRequest()
        {
            string path = Request["path"];
            string xml = null;
            using (StreamReader reader = new StreamReader(Request.InputStream))
            {
                xml = reader.ReadToEnd();
            }
            SaveSettingsFile(xml, path);
        }

        protected abstract void SaveSettingsFile(string xml, string path);
    }
}
