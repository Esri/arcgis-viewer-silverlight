/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Runtime.Serialization;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer 
{
    [DataContract]
    public class MapServiceLayerInfo : INotifyPropertyChanged
    {
        private bool _defaultVisibility;
        [DataMember(Name = "defaultVisibility")]
        public bool DefaultVisibility 
        {
            get
            {
                return _defaultVisibility;
            }
            set
            {
                if (_defaultVisibility != value)
                {
                    _defaultVisibility = value;
                    OnPropertyChanged("DefaultVisibility");
                }
            }
        }

        [DataMember(Name = "id")]
        public int ID { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "type")]
        public string Type { get; set; }
        [DataMember(Name = "subLayerIds")]
        public int[] SubLayerIds { get; set; }
        [DataMember(Name = "parentLayerId")]
        public int ParentLayerId { get; set; }

        #region INotifyPropertyChanged Members

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public MapServiceLayerInfo Clone()
        {
            return MemberwiseClone() as MapServiceLayerInfo;
        }
    }

    [DataContract]
    public class FeatureServiceLayerInfo : INotifyPropertyChanged
    {
        [DataMember(Name = "id")]
        public int ID { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }

        #region INotifyPropertyChanged Members

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public FeatureServiceLayerInfo Clone()
        {
            return MemberwiseClone() as FeatureServiceLayerInfo;
        }
    }
}
