/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using ESRI.ArcGIS.Mapping.Core.DataSources;
using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer 
{
    internal class ImageService : ServiceBase<ImageServiceInfo>
    {
        public ImageService(string uri, string proxyUrl)
            : base(uri, proxyUrl, ResourceType.ImageServer)
        {
        }

        public override List<Resource> ChildResources
        {
            get;
            set;
        }

        public override bool IsServiceInfoNeededToApplyThisFilter(Filter filter)
        {
            // As you can see below, filtering logic never requires service info so return false so that it is not obtained.
            return false;
        }

        public override bool IsFilteredIn(Filter filter)
        {
            return (filter & Filter.None) == Filter.None || (filter & Filter.ImageServices) == Filter.ImageServices;
        }
    }
}
