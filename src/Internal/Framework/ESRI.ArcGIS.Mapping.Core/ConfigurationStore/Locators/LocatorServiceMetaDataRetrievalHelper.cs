/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Mapping.Core;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using ESRI.ArcGIS.Mapping.Controls;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class LocatorServiceMetaDataRetrievalHelper
    {
        public string LocatorUrl { get; set; }

        public void GetServiceMetadata(object userState)
        {
            string geocoderUrl = LocatorUrl;
            if (string.IsNullOrEmpty(geocoderUrl))
                throw new InvalidOperationException(Resources.Strings.ExceptionMustSpecifyLocatorUrl);

            geocoderUrl += "/?f=json";
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error != null)
                {
                    string err = e.Error.Message ?? e.Error.ToString();
                    MessageBoxDialog.Show(err);
                    return;
                }
                GeocodeServiceInfo service = null;
                byte[] bytes = Encoding.Unicode.GetBytes(e.Result);
                using (MemoryStream memoryStream = new MemoryStream(bytes))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GeocodeServiceInfo), new Type[] { typeof(GeocodeServiceInfo), typeof(LocatorAddressField) });
                    service = serializer.ReadObject(memoryStream) as GeocodeServiceInfo;
                    memoryStream.Close();
                }
                if (service != null)
                    OnGetServiceMetadataCompleted(new GetServiceMetadataCompletedEventArgs() { GeocodeServiceInfo = service, UserState = userState });
                else
                    OnGetServiceMetadataFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionCannotDeserializeResponse), userState));                
            };
            webClient.DownloadStringAsync(new Uri(geocoderUrl, UriKind.Absolute));
        }

        protected void OnGetServiceMetadataCompleted(GetServiceMetadataCompletedEventArgs args)
        {
            if (GetServiceMetadataCompleted != null)
                GetServiceMetadataCompleted(this, args);
        }

        protected void OnGetServiceMetadataFailed(ExceptionEventArgs args)
        {
            if (GetServiceMetadataFailed != null)
                GetServiceMetadataFailed(this, args);
        }

        public event EventHandler<ExceptionEventArgs> GetServiceMetadataFailed;
        public event EventHandler<GetServiceMetadataCompletedEventArgs> GetServiceMetadataCompleted;
    }

    public class GetServiceMetadataCompletedEventArgs : EventArgs
    {
        public GeocodeServiceInfo GeocodeServiceInfo { get; set; }
        public object UserState { get; set; }
    }
}
