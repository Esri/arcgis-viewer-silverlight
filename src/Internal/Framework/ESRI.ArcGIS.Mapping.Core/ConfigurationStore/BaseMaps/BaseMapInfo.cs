/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Runtime.Serialization;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Core
{
    [DataContract]
    public class BaseMapInfo: INotifyPropertyChanged
    {        
        #region DisplayName
        private string _displayName;
        [DataMember(IsRequired = false, Name = "DisplayName", Order = 0)]
        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    RaisePropertyChanged("DisplayName");
                }
            }
        }
        #endregion        

        private string _name;
        /// <summary>
        /// Gets or sets the name of the basemap.  Used for sets of basemaps with 
        /// well-known names such as Bing or OpenStreetMap.
        /// </summary>
        [DataMember(IsRequired=false, Name="Name", Order=1)]       
        public string Name 
        { 
            get { return _name; } 
            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged("Name");
                }
            } 
        }        
        
        #region ThumbnailImage 
        private string _thumbnailImage;
        [DataMember(IsRequired = false, Name = "ThumbnailImage", Order = 2)]
        public string ThumbnailImage
        {
            get { return _thumbnailImage; }
            set
            {
                if (_thumbnailImage != value)
                {
                    _thumbnailImage = value;
                    RaisePropertyChanged("ThumbnailImage");
                }
            }
        }
        #endregion  
        
        #region Url
        private string _url;
        [DataMember(IsRequired = false, Name = "Url", Order = 3)]
        public string Url
        {
            get { return _url; }
            set
            {
                if (_url != value)
                {
                    _url = value;
                    RaisePropertyChanged("Url");
                }
            }
        }
        #endregion        

        #region BaseMapType
        private BaseMapType _baseMapType;
        [DataMember(IsRequired = false, Name = "BaseMapType", Order = 4)]
        public BaseMapType BaseMapType
        {
            get { return _baseMapType; }
            set
            {
                if (_baseMapType != value)
                {                    
                    _baseMapType = value;
                    updateRequiresBingKey();
                    RaisePropertyChanged("BaseMapType");
                }
            }
        }
        #endregion                        

        #region INotifyPropertyChanged 

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion            

        #region Bing Key
        private string appID;

        //Not a data member
        public string BingMapsAppID
        {
            get { return appID; }
            set
            { 
                appID = value;
                updateRequiresBingKey();
                RaisePropertyChanged("BingMapsAppID"); 
            }
        }

        void updateRequiresBingKey()
        {
            if (BaseMapType == BaseMapType.BingMaps && string.IsNullOrEmpty(BingMapsAppID))
                RequiresBingKey = true;
            else
                RequiresBingKey = false;
        }

        private bool requiresBingKey;

        public bool RequiresBingKey
        {
            get { return requiresBingKey; }
            set { requiresBingKey = value; RaisePropertyChanged("RequiresBingKey"); }
        }
        
        #endregion

        private bool useProxy;
        /// <summary>
        /// Gets or sets whether to send requests to basemap services through a proxy server
        /// </summary>
        [DataMember(IsRequired = false, Name = "UseProxy", Order = 5)]
        public bool UseProxy
        {
            get { return useProxy; }
            set 
            {
                if (useProxy != value)
                {
                    useProxy = value;
                    RaisePropertyChanged("UseProxy");
                }
            }
        }

        private string proxyUrl;

        /// <summary>
        /// Gets or sets the proxy endpoint through which to route basemap service requests
        /// </summary>
        public string ProxyUrl
        {
            get { return proxyUrl; }
            set
            {
                proxyUrl = value;
                RaisePropertyChanged("ProxyUrl");
            }
        }

        #region CodedUI Support
        public override string ToString()
        {
            return Name;
        }
        #endregion

    }
}
