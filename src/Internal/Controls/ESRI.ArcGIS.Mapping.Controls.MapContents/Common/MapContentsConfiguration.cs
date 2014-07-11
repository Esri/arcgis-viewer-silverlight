/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Mapping.Controls.MapContents
{
    [DataContract]
    public class MapContentsConfiguration : INotifyPropertyChanged
    {
        #region Mode
        private Mode _mode;
        [DataMember(Name = "Mode", IsRequired = true, Order = 0)]
        public Mode Mode
        {
            get { return _mode; }
            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    OnPropertyChanged("Mode");
                }
            }
        }
        #endregion

        #region HideBasemaps
        private bool _hideBasemaps;
        [DataMember(Name = "HideBasemaps", IsRequired = false, EmitDefaultValue = false, Order = 1)]
        public bool HideBasemaps
        {
            get { return _hideBasemaps; }
            set
            {
                if (_hideBasemaps != value)
                {
                    _hideBasemaps = value;
                    OnPropertyChanged("HideBasemaps");
                }
            }
        }
        #endregion

        #region ShowLayersVisibleAtScale
        private bool _showLayersVisibleAtScale;
        [DataMember(Name = "ShowLayersVisibleAtScale", IsRequired = false, EmitDefaultValue = false, Order = 2)]
        public bool ShowLayersVisibleAtScale
        {
            get { return _showLayersVisibleAtScale; }
            set
            {
                if (_showLayersVisibleAtScale != value)
                {
                    _showLayersVisibleAtScale = value;
                    OnPropertyChanged("ShowLayersVisibleAtScale");
                }
            }
        }
        #endregion

        #region ContextMenuToolPanelName
        private string _contextMenuToolPanelName = "LayerConfigurationContextMenuContainer";
        [DataMember(Name = "ContextMenuToolPanelName", IsRequired = false, EmitDefaultValue=false, Order = 3)]
        public string ContextMenuToolPanelName
        {
            get { return _contextMenuToolPanelName; }
            set
            {
                if (_contextMenuToolPanelName != value)
                {
                    _contextMenuToolPanelName = value;
                    OnPropertyChanged("ContextMenuToolPanelName");
                }
            }
        }
        #endregion

        #region SelectedLayerId
        private string _selectedLayerId;
        [DataMember(Name = "SelectedLayerId", IsRequired = false, EmitDefaultValue = false, Order = 4)]
        public string SelectedLayerId
        {
            get { return _selectedLayerId; }
            set
            {
                if (_selectedLayerId != value)
                {
                    _selectedLayerId = value;
                    OnPropertyChanged("SelectedLayerId");
                }
            }
        }
        #endregion

        #region AllowLayerSelection
        private bool _allowLayerSelection = true;
        [DataMember(Name = "AllowLayerSelection", IsRequired = false, EmitDefaultValue = false, Order = 4)]
        public bool AllowLayerSelection
        {
            get { return _allowLayerSelection; }
            set
            {
                if (_allowLayerSelection != value)
                {
                    _allowLayerSelection = value;
                    OnPropertyChanged("AllowLayerSelection");
                }
            }
        }
        #endregion

        #region ExcludedLayerIds
        private string[] _excludedlayerIds;
        [DataMember(Name = "ExcludedLayerIds", IsRequired = false, EmitDefaultValue = false, Order = 5)]
        public string[] ExcludedLayerIds
        {
            get { return _excludedlayerIds; }
            set
            {
                if (_excludedlayerIds != value)
                {
                    _excludedlayerIds = value;
                    OnPropertyChanged("ExcludedLayerIds");
                }
            }
        }
        #endregion

        #region ExpandLayersOnAdd
        private bool _expandLayersOnAdd;
        [DataMember(Name = "ExpandLayersOnAdd", IsRequired = false, EmitDefaultValue = false, Order = 6)]
        public bool ExpandLayersOnAdd
        {
            get { return _expandLayersOnAdd; }
            set
            {
                if (_expandLayersOnAdd != value)
                {
                    _expandLayersOnAdd = value;
                    OnPropertyChanged("ExpandLayersOnAdd");
                }
            }
        }
        #endregion


        #region LayerIds
        private string[] _layerIds;
        public string[] LayerIds
        {
            get { return _layerIds; }
            set
            {
                if (_layerIds != value)
                {
                    _layerIds = value;
                    OnPropertyChanged("LayerIds");
                }
            }
        }
        #endregion

        #region LayerInfos
        private List<LayerInfo> _layerInfos;
        public List<LayerInfo> LayerInfos
        {
            get { return _layerInfos; }
            set
            {
                if (_layerInfos != value)
                {
                    _layerInfos = value;
                    OnPropertyChanged("LayerInfos");
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public override string ToString()
        {
            return DataContractSerializationHelper.Serialize<MapContentsConfiguration>(this);
        }
        public static MapContentsConfiguration FromString(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return new MapContentsConfiguration();
            else
                return DataContractSerializationHelper.Deserialize<MapContentsConfiguration>(str);
        }
    }
}
