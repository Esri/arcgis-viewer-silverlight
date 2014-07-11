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

namespace ESRI.ArcGIS.Mapping.Builder.Server
{
    public abstract class SaveConfigurationStoreXmlHandlerBase : ServiceRequestHandlerBase
    {
        protected override void HandleRequest()
        {
            string xml = null;
            using (StreamReader reader = new StreamReader(Request.InputStream))
            {
                xml = reader.ReadToEnd();
            }
            SaveConfigurationStoreXml(xml);
        }

        protected abstract void SaveConfigurationStoreXml(string xml);
    }
}
