/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Core.DataSources;
using System.IO;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer 
{
    internal class FeatureService : ServiceBase<FeatureServiceInfo>
    {
        public FeatureService(string uri, string proxyUrl)
            : base(uri, proxyUrl, ResourceType.FeatureServer)
        {
        }

        private List<Resource> _childResources = null;
        public override List<Resource> ChildResources
        {
            get
            {
                if (_childResources == null)
                {
                    _childResources = new List<Resource>();

                    if (ServiceInfo != null && ServiceInfo.Layers != null)
                    {
                        foreach (FeatureServiceLayerInfo layerInfo in ServiceInfo.Layers)
                        {
                            _childResources.Add(new Resource()
                            {
                                DisplayName = layerInfo.Name,
                                ResourceType = ResourceType.EditableLayer,
                                Url = string.Format("{0}/{1}", Uri, layerInfo.ID),
                                ProxyUrl = ProxyUrl,
                                Tag = layerInfo.ID,
                            });
                        }
                    }
                }
                return _childResources;
            }
            set
            {
                _childResources = value;
            }
        }

        public override bool IsServiceInfoNeededToApplyThisFilter(Filter filter)
        {
            // As you can see below, filtering logic never requires service info so return false so that it is not obtained.
            return false;
        }

        public override bool IsFilteredIn(Filter filter)
        {
            return (filter & Filter.None) == Filter.None || (filter & Filter.FeatureServices) == Filter.FeatureServices;
        }
    }
}
