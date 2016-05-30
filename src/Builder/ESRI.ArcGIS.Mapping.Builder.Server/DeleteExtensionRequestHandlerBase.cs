/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace ESRI.ArcGIS.Mapping.Builder.Server
{
    public abstract class DeleteExtensionRequestHandlerBase : ServiceRequestHandlerBase
    {
        protected override void HandleRequest()
        {
            DeleteExtension(Request["fileName"]);
        }

        protected abstract void DeleteExtension(string extensionFileName);
    }
}
