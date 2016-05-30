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
    public abstract class GetExtensionsHandlerBase : ServiceRequestHandlerBase
    {
        protected override void HandleRequest()
        {
            try
            {
                Extensions extensions = GetExtensions();
                if (extensions == null)
                    WriteError("Unable to retrieve extensions");                
                
                XmlDocument xDoc = new XmlDocument();
                XmlSerializer writer = new XmlSerializer(typeof(Extensions));

                // Add the baseUrl property which is not being serialized by the XmlSerializer
                MemoryStream ms = new MemoryStream();
                writer.Serialize(ms, extensions);
                ms.Seek(0, SeekOrigin.Begin);
                xDoc.Load(ms);
                xDoc.DocumentElement.SetAttribute("BaseUrl", extensions.BaseUrl);
                
                // Write response
                xDoc.Save(XmlWriter.Create(Response.OutputStream));
                Response.OutputStream.Flush();
            }
            catch (Exception ex)
            {
                WriteError(ex.Message);             
            }            
        }

        protected abstract Extensions GetExtensions();
    }
}
