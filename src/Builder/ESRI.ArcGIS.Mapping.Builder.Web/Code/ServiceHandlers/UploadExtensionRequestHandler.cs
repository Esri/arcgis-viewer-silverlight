/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Web;
using ESRI.ArcGIS.Mapping.Builder.Server;

namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    public class UploadExtensionRequestHandler : UploadExtensionRequestHandlerBase
    {
        protected override void UploadExtension(string fileName, string[] assemblies, byte[] fileBytes)
        {
            ExtensionsManager.SaveExtensionLibraryToDisk(fileName, fileBytes);
            ExtensionsManager.AddExtensionLibraryToCatalog(fileName, assemblies);
        }
    }
}
