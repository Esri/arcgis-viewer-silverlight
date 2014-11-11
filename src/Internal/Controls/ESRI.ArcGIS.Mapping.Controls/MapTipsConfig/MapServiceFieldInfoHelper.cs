/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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
using System.Runtime.Serialization;
using ESRI.ArcGIS.Mapping.DataSources.ArcGISServer;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Client.FeatureService;

namespace ESRI.ArcGIS.Mapping.Controls 
{
    internal class MapServiceLayerInfoHelper
    {
        public string Url { get; set; }
        public Layer Layer { get; set; }
        public string ProxyUrl { get; set; }
        public MapServiceLayerInfoHelper(string url, Layer layer, string proxyUrl)
        {
            Url = url;
            Layer = layer;
            ProxyUrl = proxyUrl;
        }

        ArcGISWebClient webClient;
        public void GetLayerInfos(object userState)
        {
            getLayerInfosForPre10Servers(userState);
            return;
        }

        private async void getLayerInfosForPre10Servers(object userState)
        {
            string url = Url + "?f=json";
            if (webClient == null)
                webClient = new ArcGISWebClient();

            if (webClient.IsBusy)
                webClient.CancelAsync();

            webClient.ProxyUrl = ProxyUrl;
            ArcGISWebClient.DownloadStringCompletedEventArgs result =  
                await webClient.DownloadStringTaskAsync(new Uri(url), userState);
            processPre10LayerInfoResult(result);
        }

        private void processPre10LayerInfoResult(ArcGISWebClient.DownloadStringCompletedEventArgs e)
        {
            #region Parse layer ids from json
            if (e.Cancelled)
                return;
            if (e.Error != null)
            {
                onLayerInfosCompleted(new LayerInfosEventArgs() { LayerInfos = null, UserState = e });
                return;
            }
            string json = null;
            try
            {
                json = e.Result;
            }
            catch (Exception exception)
            {
                if (exception != null)
                {
                    onLayerInfosCompleted(new LayerInfosEventArgs() { LayerInfos = null, UserState = e });
                    return;
                }
            }
            Exception ex = ESRI.ArcGIS.Mapping.DataSources.Utils.CheckJsonForException(json);
            if (ex != null)
            {
                onLayerInfosCompleted(new LayerInfosEventArgs() { LayerInfos = null, UserState = e });
                return;
            }

            MapServiceInfo mapServiceInfo = JsonSerializer.Deserialize<MapServiceInfo>(json);
            if (mapServiceInfo == null || mapServiceInfo.Layers == null)
            {
                onLayerInfosCompleted(new LayerInfosEventArgs() { LayerInfos = null, UserState = e });
                return;
            }
            singleRequestLayerIds = new List<int>();
            foreach (MapServiceLayerInfo layer in mapServiceInfo.Layers)
            {
                LayerInformation info = new LayerInformation()
                {
                    ID = layer.ID,
                    Name = layer.Name,
                    PopUpsEnabled = false
                };
                if (layer.SubLayerIds == null || layer.SubLayerIds.Length < 1)
                {
                    singleRequestLayerIds.Add(layer.ID);
                }
            }
            if (singleRequestLayerIds.Count < 1)
            {
                onLayerInfosCompleted(new LayerInfosEventArgs() { LayerInfos = null, UserState = e });
            }
            else
            {
                singleRequestLayerInfos = new List<LayerInformation>();
                singleRequestWebClients = new List<ArcGISWebClient>();
                cancelSingleRequests = false;
                pendingRequests = singleRequestLayerIds.Count;
                foreach (int id in singleRequestLayerIds)
                {
                    getLayerInfo(id, e.UserState);
                }
            }
            #endregion
        }

        void singleLayerRequestCompleted(LayerInformation info, object userState)
        {
            pendingRequests--;
            if (info != null)
                singleRequestLayerInfos.Add(info);
            if (pendingRequests < 1)
            {
                Collection<LayerInformation> layerinfos = new Collection<LayerInformation>();
                foreach (int id in singleRequestLayerIds)
                {
                    foreach (LayerInformation item in singleRequestLayerInfos)
                    {
                        if (item.ID == id)
                        {
                            layerinfos.Add(item);
                            break;
                        }
                    }
                }
                onLayerInfosCompleted(new LayerInfosEventArgs() { LayerInfos = layerinfos, UserState = userState });
            }
        }

        List<LayerInformation> singleRequestLayerInfos;
        List<ArcGISWebClient> singleRequestWebClients;
        int pendingRequests;
        bool cancelSingleRequests;
        List<int> singleRequestLayerIds;
        private async void getLayerInfo(int layerID, object userState)
        {
            if (cancelSingleRequests)
                return;
            string layerUrl = string.Format("{0}/{1}?f=pjson", Url, layerID);
            ArcGISWebClient webClient = new ArcGISWebClient() { ProxyUrl = ProxyUrl };
            singleRequestWebClients.Add(webClient);

            ArcGISWebClient.DownloadStringCompletedEventArgs result =
                await webClient.DownloadStringTaskAsync(new Uri(layerUrl), userState);
            processLayerInfoResult(result);
        }

        private void processLayerInfoResult(ArcGISWebClient.DownloadStringCompletedEventArgs e)
        {
            #region Parse layer info from json
            if (e.Cancelled)
                return;
            if (e.Error != null)
            {
                singleLayerRequestCompleted(null, e);
                return;
            }
            string json = null;
            try
            {
                json = e.Result;
            }
            catch (Exception exception)
            {
                if (exception != null)
                {
                    singleLayerRequestCompleted(null, e);
                    return;
                }
            }
            Exception ex = ESRI.ArcGIS.Mapping.DataSources.Utils.CheckJsonForException(json);
            if (ex != null)
            {
                singleLayerRequestCompleted(null, e);
                return;
            }
            json = "{\"layerDefinition\":" + json + "}";
            FeatureLayer featureLayer = FeatureLayer.FromJson(json);
            FeatureLayerInfo featurelayerinfo = featureLayer.LayerInfo;
            if (featurelayerinfo == null)
            {
                singleLayerRequestCompleted(null, e);
                return;
            }
            LayerInformation info = new LayerInformation()
            {
                ID = featurelayerinfo.Id,
                DisplayField = featurelayerinfo.DisplayField,
                Name = featurelayerinfo.Name,
                PopUpsEnabled = false,
                LayerJson = json,
                FeatureLayer = featureLayer
            };
            Collection<ESRI.ArcGIS.Mapping.Core.FieldInfo> fieldInfos = new Collection<ESRI.ArcGIS.Mapping.Core.FieldInfo>();
            if (featurelayerinfo.Fields != null)
            {
                foreach (ESRI.ArcGIS.Client.Field field in featurelayerinfo.Fields)
                {
                    if (FieldHelper.IsFieldFilteredOut(field.Type))
                        continue;
                    ESRI.ArcGIS.Mapping.Core.FieldInfo fieldInfo = ESRI.ArcGIS.Mapping.Core.FieldInfo.FieldInfoFromField(featureLayer, field);
                    fieldInfos.Add(fieldInfo);
                }
            }
            info.Fields = fieldInfos;
            if (fieldInfos.Count > 1)
                singleLayerRequestCompleted(info, e);
            else
                singleLayerRequestCompleted(null, e);
            #endregion
        }

        public void Cancel()
        {
            if (webClient != null && webClient.IsBusy)
                webClient.CancelAsync();
            cancelSingleRequests = true;
            if (singleRequestWebClients != null)
            {
                foreach (ArcGISWebClient client in singleRequestWebClients)
                {
                    if (client.IsBusy)
                        client.CancelAsync();
                }
            }
        }

        void onLayerInfosCompleted(LayerInfosEventArgs e)
        {
            LayerExtensions.SetLayerInfos(Layer, e.LayerInfos);      
            if (GetLayerInfosCompleted != null)
                GetLayerInfosCompleted(this, e);
        }

        public event EventHandler<LayerInfosEventArgs> GetLayerInfosCompleted;


        public class LayerInfosEventArgs : EventArgs
        {
            public object UserState { get; set; }
            public Collection<LayerInformation> LayerInfos { get; set; }
        }

    }

    [DataContract]
    public class AllLayersResponse
    {
        [DataMember(Name = "layers")]
        public List<MapServiceLayerInfoWithFields> Layers { get; set; }
    }

     [DataContract]
    public class MapServiceLayerInfoWithFields : MapServiceLayerInfo
    {
        [DataMember(Name = "displayField")]
        public string DisplayField { get; set; }
        [DataMember(Name = "fields")]
        public List<Core.Field> Fields { get; set; }
    }
}
