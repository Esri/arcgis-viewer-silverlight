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
using ESRI.ArcGIS.Mapping.Builder.Web;

namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    public class SaveSettingsRequestHandler : Server.SaveSettingsRequestHandlerBase
    {
        static object lockObject = new object();

        protected override void SaveSettingsFile(string xml, string path)
        {
            path = ServerUtility.MapPath(path);
            
            if (string.IsNullOrEmpty(path))
                return;
            if (File.Exists(path))
            {
                lock (lockObject)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
                    {
                        file.Write(xml);
                        file.Flush();
                    }
                }
            }
        }
    }
}
