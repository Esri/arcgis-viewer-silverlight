/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.ComponentModel;
using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Mapping.Core
{
    [DataContract]
    public class Connection : INotifyPropertyChanged
    {
        private string _name;
        [DataMember(Name="Name",IsRequired=false, Order=0)]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs("Name"));
                }
            }
        }

        private string _url;
        [DataMember(Name = "Url", IsRequired = false, Order = 1)]
        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                if (_url != value)
                {
                    _url = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs("Url"));
                }
            }
        }

        private string _proxyUrl;
        public string ProxyUrl
        {
            get
            {
                return _proxyUrl;
            }
            set
            {
                if (_proxyUrl != value)
                {
                    _proxyUrl = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs("ProxyUrl"));
                }
            }
        }

        private ConnectionType _connectionType;
        [DataMember(Name = "ConnectionType", IsRequired = false, Order = 2)]
        public ConnectionType ConnectionType
        {
            get { return _connectionType; }
            set
            {
                if (_connectionType != value)
                {
                    _connectionType = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs("ConnectionType"));
                }
            }
        }


        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, e);
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public enum ConnectionType
    {
        Unknown = 0,
        SharePoint = 1,
        ArcGISServer = 2,
        SpatialDataService = 3,
        OpenDataServer = 4,
    }
}
