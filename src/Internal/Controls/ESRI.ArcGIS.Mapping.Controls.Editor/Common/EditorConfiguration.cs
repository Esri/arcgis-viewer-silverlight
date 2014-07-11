/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using ESRI.ArcGIS.Client.Extensibility;
using System.Linq;
using ESRI.ArcGIS.Mapping.Controls.Editor;
using ESRI.ArcGIS.Mapping.Controls;
using System.Windows;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls.Editor
{
    [DataContract]
    public class EditorConfiguration : INotifyPropertyChanged
    {
        public static EditorConfiguration Current
        {
            get { return _current; }
            set { _current = value; }
        }

        private static EditorConfiguration _current = null;

        #region AlwaysDisplayDefaultTemplates
        private bool _alwaysDisplayDefaultTemplates;
        [DataMember(Name = "AlwaysDisplayDefaultTemplates", IsRequired = false, EmitDefaultValue = false, Order = 0)]
        public bool AlwaysDisplayDefaultTemplates
        {
            get { return _alwaysDisplayDefaultTemplates; }
            set
            {
                if (_alwaysDisplayDefaultTemplates != value)
                {
                    _alwaysDisplayDefaultTemplates = value;
                    OnPropertyChanged("AlwaysDisplayDefaultTemplates");
                }
            }
        }
        #endregion

        #region AutoComplete
        private bool _autoComplete;
        [DataMember(Name = "AutoComplete", IsRequired = false, EmitDefaultValue = false, Order = 1)]
        public bool AutoComplete
        {
            get { return _autoComplete; }
            set
            {
                if (_autoComplete != value)
                {
                    _autoComplete = value;
                    OnPropertyChanged("AutoComplete");
                }
            }
        }
        #endregion

        #region AutoSelect
        private bool _autoSelect = true;
        [DefaultValue(true)]
        [DataMember(Name = "AutoSelect", IsRequired = false, EmitDefaultValue = false, Order = 2)]
        public bool AutoSelect
        {
            get { return _autoSelect; }
            set
            {
                //if (_autoSelect != value)
                //{
                    _autoSelect = value;
                    OnPropertyChanged("AutoSelect");
                //}
            }
        }
        #endregion

        #region Continuous
        private bool _continuous;
        [DataMember(Name = "Continuous", IsRequired = false, EmitDefaultValue = false, Order = 3)]
        public bool Continuous
        {
            get { return _continuous; }
            set
            {
                if (_continuous != value)
                {
                    _continuous = value;
                    OnPropertyChanged("Continuous");
                }
            }
        }
        #endregion

        #region GeometryServiceUrl
        private string _geometryServiceUrl;
        public string GeometryServiceUrl
        {
            get
            {
                return _geometryServiceUrl;
            }
            set
            {
                if (_geometryServiceUrl != value)
                {
                    _geometryServiceUrl = value;
                    OnPropertyChanged("GeometryServiceUrl");
                }
            }
        }
        #endregion

        #region LayerIds
        private string[] _layerIds = new string[0];
        [DataMember(Name = "LayerIds", IsRequired = false, EmitDefaultValue = false, Order = 5)]
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
        private void RefreshLayerIds()
        {
            if (LayerInfos != null)
            {
                IEnumerable<string> idsAsStr = LayerInfos.Where(inf => inf.IsChecked == true).Select(info => info.Layer.ID).Distinct();
                if (idsAsStr != null)
                {
                    LayerIds = idsAsStr.ToArray();
                }

                foreach (LayerInfo info in LayerInfos)
                {
                    bool included = info.Layer != null && !string.IsNullOrWhiteSpace(info.Layer.ID) && LayerIds.Contains(info.Layer.ID);
                    info.Layer.SetValue(LayerProperties.IsEditableProperty, included);
                }
            }
        }
        #endregion

        #region UseDefaultLayerIds
        private bool _useDefaultLayerIds = false;
        public bool UseDefaultLayerIds
        {
            get { return _useDefaultLayerIds; }
            set
            {
                if (_useDefaultLayerIds != value)
                {
                    if (value && HasClusteredFeatureLayers())
                    {
                        MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Controls.Editor.Resources.Strings.DisableClusteringPrompt,
                            ESRI.ArcGIS.Mapping.Controls.Editor.Resources.Strings.DisableClustering, MessageBoxButton.OKCancel,
                            (obj, args1) =>
                            {
                                if (args1.Result == MessageBoxResult.OK)
                                {
                                    RemoveLayerClustering();

                    _useDefaultLayerIds = value;

                                    SetUseDefaultLayerIds();
                                }
                                OnPropertyChanged("UseDefaultLayerIds");
                            });
                    }
                    else
                    {
                        _useDefaultLayerIds = value;

                        SetUseDefaultLayerIds();

                        OnPropertyChanged("UseDefaultLayerIds");
                    }
                }
            }
        }
        private void SetUseDefaultLayerIds()
        {
                    if(!_useDefaultLayerIds)
                        LayerIds = new string[0];
                    else if (LayerIds != null)
                        LayerIds = null;

                    UpdateLayerInfosState();

                    SetFeatureLayersEditingProperties();
        }

        private void SetFeatureLayersEditingProperties()
        {
            if (LayerInfos == null)
                return;

            foreach (LayerInfo info in LayerInfos)
                SetFeatureLayerEditingProperties(info.Layer, UseDefaultLayerIds ? true : info.IsChecked);
        }
        private void SetFeatureLayerEditingProperties(FeatureLayer layer, bool selectedForEditing)
        {
            if (layer == null)
                return;

            layer.DisableClientCaching = selectedForEditing;
            if (selectedForEditing)
            {
                bool popupsOnClick = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetPopUpsOnClick(layer);
                bool newPopupsOnClick = selectedForEditing && SetLayerPopupOnClick;
                if (!popupsOnClick && popupsOnClick != newPopupsOnClick)
                {
                    layer.SetValue(ESRI.ArcGIS.Mapping.Core.LayerExtensions.PopUpsOnClickProperty, newPopupsOnClick);
                    layer.SetValue(ESRI.ArcGIS.Mapping.Core.LayerExtensions.IsMapTipDirtyProperty, true);
                }
            }
        }
        private bool HasClusteredFeatureLayers()
        {
            LayerInfo info = LayerInfos != null ?
                             LayerInfos.FirstOrDefault(li => li.Layer != null && li.Layer.Clusterer != null) :
                             null;
            return info != null;
        }
        private void RemoveLayerClustering()
        {
            if (LayerInfos != null)
            {
                foreach (LayerInfo info in LayerInfos)
                    if (info.Layer != null && info.Layer.Clusterer != null)
                        info.Layer.Clusterer = null;
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
                    SubscribeToLayerInfoChange(_layerInfos, false);
                    _layerInfos = value;
                    SetFeatureLayersEditingProperties();
                    ApplyAutoSaveOnLayers();
                    SubscribeToLayerInfoChange(_layerInfos, true);
                    OnPropertyChanged("LayerInfos");
                }
            }
        }

        public void RefreshLayerInfos(Map map)
        {
            LayerInfos = GetLayerInfos(map);
        }
        
        private void SubscribeToLayerInfoChange(List<LayerInfo> layerInfos, bool subscribe)
        {
            if (layerInfos == null)
                return;
            foreach (LayerInfo inf in layerInfos)
            {
                if(subscribe)
                    inf.PropertyChanged += LayerInfo_PropertyChanged;
                else
                    inf.PropertyChanged -= LayerInfo_PropertyChanged;
            }
        }
        private void LayerInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChecked")
            {
                LayerInfo info = sender as LayerInfo;
                if (info != null)
                    SetFeatureLayerEditingProperties(info.Layer, info.IsChecked);

                if (!UseDefaultLayerIds)
                    RefreshLayerIds();
            }
        }

        private List<LayerInfo> GetLayerInfos(Map map)
        {
            List<LayerInfo> layers = null;
            if (map != null && map.Layers != null)
            {
                foreach (Layer layer in map.Layers)
                {
                    FeatureLayer fLayer = layer as FeatureLayer;
                    if (fLayer != null && !string.IsNullOrWhiteSpace(fLayer.ID) && !fLayer.IsReadOnly)
                    {
                        if (layers == null)
                            layers = new List<LayerInfo>();

                        layers.Add(new LayerInfo() { Layer = fLayer, IsChecked = LayerIds != null ? LayerIds.Contains(layer.ID) : false });
                        layer.SetValue(LayerProperties.IsEditableProperty, LayerIds == null ? true : LayerIds.Contains(layer.ID));
                    }
                }
            }
            return layers;
        }
        private void UpdateLayerInfosState()
        {
            if (LayerInfos != null)
            {
                foreach (LayerInfo layerInfo in LayerInfos)
                {
                    layerInfo.IsChecked = LayerIds != null ? LayerIds.Contains(layerInfo.Layer.ID) : false;
                    if (layerInfo.Layer != null)
                        layerInfo.Layer.SetValue(LayerProperties.IsEditableProperty, LayerIds == null ? true : LayerIds.Contains(layerInfo.Layer.ID));
                }
            }
        }
        #endregion
 
        #region ShowAttributesOnAdd
        private bool _showAttributesOnAdd = true;
        [DefaultValue(true)]
        [DataMember(Name = "ShowAttributesOnAdd", IsRequired = false, EmitDefaultValue = false, Order = 6)]
        public bool ShowAttributesOnAdd
        {
            get { return _showAttributesOnAdd; }
            set
            {
                if (_showAttributesOnAdd != value)
                {
                    _showAttributesOnAdd = value;
                    OnPropertyChanged("ShowAttributesOnAdd");
                }
            }
        }
        #endregion

        #region EditVerticesEnabled
        private bool _editVerticesEnabled = true;
        public bool EditVerticesEnabled
        {
            get { return _editVerticesEnabled; }
            set
            {
                if (_editVerticesEnabled != value)
                {
                    _editVerticesEnabled = value;
                    OnPropertyChanged("EditVerticesEnabled");
                }
            }
        }
        #endregion

        #region MaintainAspectRatio
        private bool _maintainAspectRatio;
        [DataMember(Name = "MaintainAspectRatio", IsRequired = false, EmitDefaultValue = false, Order = 8)]
        public bool MaintainAspectRatio
        {
            get { return _maintainAspectRatio; }
            set
            {
                if (_maintainAspectRatio != value)
                {
                    _maintainAspectRatio = value;
                    OnPropertyChanged("MaintainAspectRatio");
                }
            }
        }
        #endregion

        #region MoveEnabled
        private bool _moveEnabled = true;
        public bool MoveEnabled
        {
            get { return _moveEnabled; }
            set
            {
                if (_moveEnabled != value)
                {
                    _moveEnabled = value;
                    OnPropertyChanged("MoveEnabled");
                }
            }
        }
        #endregion

        #region RotateEnabled
        private bool _rotateEnabled = true;
        public bool RotateEnabled
        {
            get { return _rotateEnabled; }
            set
            {
                if (_rotateEnabled != value)
                {
                    _rotateEnabled = value;
                    OnPropertyChanged("RotateEnabled");
                }
            }
        }
        #endregion

        #region ScaleEnabled
        private bool _scaleEnabled = true;
        public bool ScaleEnabled
        {
            get { return _scaleEnabled; }
            set
            {
                if (_scaleEnabled != value)
                {
                    _scaleEnabled = value;
                    OnPropertyChanged("ScaleEnabled");
                }
            }
        }
        #endregion

        #region AutoSave
        private bool _autoSave = true;
        [DefaultValue(true)]
        [DataMember(Name = "AutoSave", IsRequired = false, EmitDefaultValue = false, Order = 12)]
        public bool AutoSave
        {
            get { return _autoSave; }
            set
            {
                if (_autoSave != value)
                {
                    _autoSave = value;

                    ApplyAutoSaveOnLayers();

                    OnPropertyChanged("AutoSave");
                }
            }
        }
        private void ApplyAutoSaveOnLayers()
        {
            if (LayerInfos != null)
            {
                foreach (LayerInfo info in LayerInfos)
                {
                    if (info.Layer != null)
                        info.Layer.AutoSave = AutoSave;
                }
            }
        }
        #endregion

        #region SetLayerPopupOnClick
        private bool _setLayerPopupOnClick = true;
        public bool SetLayerPopupOnClick
        {
            get { return _setLayerPopupOnClick; }
            set
            {
                if (_setLayerPopupOnClick != value)
                {
                    _setLayerPopupOnClick = value;
                    SetFeatureLayersEditingProperties();
                    OnPropertyChanged("SetLayerPopupOnClick");
                }
            }
        }
        #endregion

        #region EditingShapesEnabled
        private bool _editShapesEnabled = true;
        [DefaultValue(true)]
        [DataMember(Name = "EditingShapesEnabled", IsRequired = false, EmitDefaultValue = false, Order = 13)]
        public bool EditingShapesEnabled
        {
            get { return _editShapesEnabled; }
            set
            {
                //if (_editShapesEnabled != value)
                //{
                    _editShapesEnabled = value;
                    MoveEnabled = value;
                    RotateEnabled = value;
                    EditVerticesEnabled = value;
                    ScaleEnabled = value;
                    if (value == false )
                        EditShapesTool = false;
                    OnPropertyChanged("EditingShapesEnabled");
                //}
            }
        }
        #endregion

        #region ToolPanel

        private ToolPanel _toolPanel;
        public ToolPanel ToolPanel
        {
            get { return _toolPanel; }
            set
            {
                if (_toolPanel != value)
                {
                    _toolPanel = value;
                    OnPropertyChanged("ToolPanel");
                }
            }

        }
        #endregion

        #region Tools

        #region SelectTool
        private bool _select;
        [DataMember(Name = "SelectTool", IsRequired = false, EmitDefaultValue = false)]
        public bool SelectTool
        {
            get { return _select; }
            set
            {
                if (_select != value)
                {
                    _select = value;
                    GetToolCount();
                    OnPropertyChanged("SelectTool");
                }
            }
        }
        #endregion

        #region AddToSelectionTool
        private bool _addToSelection;
        [DataMember(Name = "AddToSelectionTool", IsRequired = false, EmitDefaultValue = false)]
        public bool AddToSelectionTool
        {
            get { return _addToSelection; }
            set
            {
                if (_addToSelection != value)
                {
                    _addToSelection = value;
                    GetToolCount();
                    OnPropertyChanged("AddToSelectionTool");
                }
            }
        }
        #endregion

        #region RemoveFromSelectionTool
        private bool _removeFromSelection;
        [DataMember(Name = "RemoveFromSelectionTool", IsRequired = false, EmitDefaultValue = false)]
        public bool RemoveFromSelectionTool
        {
            get { return _removeFromSelection; }
            set
            {
                if (_removeFromSelection != value)
                {
                    _removeFromSelection = value;
                    GetToolCount();
                    OnPropertyChanged("RemoveFromSelectionTool");
                }
            }
        }
        #endregion

        #region ClearSelectionTool
        private bool _clearSelection;
        [DataMember(Name = "ClearSelectionTool", IsRequired = false, EmitDefaultValue = false)]
        public bool ClearSelectionTool
        {
            get { return _clearSelection; }
            set
            {
                if (_clearSelection != value)
                {
                    _clearSelection = value;
                    GetToolCount();
                    OnPropertyChanged("ClearSelectionTool");
                }
            }
        }
        #endregion

        #region DeleteTool
        private bool _delete;
        [DataMember(Name = "DeleteTool", IsRequired = false, EmitDefaultValue = false)]
        public bool DeleteTool
        {
            get { return _delete; }
            set
            {
                if (_delete != value)
                {
                    _delete = value;
                    GetToolCount();
                    OnPropertyChanged("DeleteTool");
                }
            }
        }
        #endregion

        #region EditValuesTool
        private bool _editValues;
        [DataMember(Name = "EditValuesTool", IsRequired = false, EmitDefaultValue = false)]
        public bool EditValuesTool
        {
            get { return _editValues; }
            set
            {
                if (_editValues != value)
                {
                    _editValues = value;
                    GetToolCount();
                    OnPropertyChanged("EditValuesTool");
                }
            }
        }
        #endregion

        #region EditShapesTool
        private bool _editShapes;
        [DataMember(Name = "EditShapesTool", IsRequired = false, EmitDefaultValue = false)]
        public bool EditShapesTool
        {
            get { return _editShapes; }
            set
            {
                //if (_editShapes != value)
                //{
                    _editShapes = value;
                    GetToolCount();
                    OnPropertyChanged("EditShapesTool");
                //}
            }
        }
        #endregion

        #region ReshapeTool
        private bool _reshape;
        [DataMember(Name = "ReshapeTool", IsRequired = false, EmitDefaultValue = false)]
        public bool ReshapeTool
        {
            get { return _reshape; }
            set
            {
                if (_reshape != value)
                {
                    _reshape = value;
                    GetToolCount();
                    OnPropertyChanged("ReshapeTool");
                }
            }
        }
        #endregion

        #region UnionTool
        private bool _union;
        [DataMember(Name = "UnionTool", IsRequired = false, EmitDefaultValue = false)]
        public bool UnionTool
        {
            get { return _union; }
            set
            {
                if (_union != value)
                {
                    _union = value;
                    GetToolCount();
                    OnPropertyChanged("UnionTool");
                }
            }
        }
        #endregion

        #region CutTool
        private bool _cut;
        [DataMember(Name = "CutTool", IsRequired = false, EmitDefaultValue = false)]
        public bool CutTool
        {
            get { return _cut; }
            set
            {
                if (_cut != value)
                {
                    _cut = value;
                    GetToolCount();
                    OnPropertyChanged("CutTool");
                }
            }
        }
        #endregion

        #region AutocompleteTool
        private bool _autocompleteTool;
        [DataMember(Name = "AutocompleteTool", IsRequired = false, EmitDefaultValue = false)]
        public bool AutocompleteTool
        {
            get { return _autocompleteTool; }
            set
            {
                if (_autocompleteTool != value)
                {
                    _autocompleteTool = value;
                    GetToolCount();
                    OnPropertyChanged("AutocompleteTool");
                }
            }
        }
        #endregion

        #region FreehandTool
        private bool _freehand;
        [DataMember(Name = "FreehandTool", IsRequired = false, EmitDefaultValue = false, Order = 4)]
        public bool FreehandTool
        {
            get { return _freehand; }
            set
            {
                if (_freehand != value)
                {
                    _freehand = value;
                    GetToolCount();
                    OnPropertyChanged("FreehandTool");
                }
            }
        }
        #endregion

        #region ClearTool
        private bool _clearTool;
        [DataMember(Name = "ClearTool", IsRequired = false, EmitDefaultValue = false)]
        public bool ClearTool
        {
            get { return _clearTool; }
            set
            {
                if (_clearTool != value)
                {
                    _clearTool = value;
                    GetToolCount();
                    OnPropertyChanged("ClearTool");
                }
            }
        }
        #endregion

        #endregion

        #region ToolCount

        private int _toolCount;
        public int ToolCount
        {
            get { return _toolCount; }
            set
            {
                if (_toolCount != value)
                {
                    _toolCount = value;
                    ToolbarVisibility = (_toolCount > 0) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    OnPropertyChanged("ToolCount");
                }
            }
        }

        private void GetToolCount()
        {
            int count = 0;
            if (SelectTool) count++;
            if (AddToSelectionTool) count++;
            if (RemoveFromSelectionTool) count++;
            if (ClearSelectionTool) count++;
            if (DeleteTool) count++;
            if (EditValuesTool) count++;
            if (EditShapesTool) count++;
            if (ReshapeTool) count++;
            if (UnionTool) count++;
            if (CutTool) count++;
            if (AutocompleteTool) count++;
            if (FreehandTool) count++;
            if (ClearTool) count++;
            ToolCount = count; 
        }

        #endregion

        #region ToolbarVisibility

        private System.Windows.Visibility _toolbarVisibility = System.Windows.Visibility.Visible;
        [DefaultValue(System.Windows.Visibility.Visible)]
        [DataMember(Name = "ToolbarVisibility", IsRequired = false, EmitDefaultValue = false)]
        public System.Windows.Visibility ToolbarVisibility
        {
            get { return _toolbarVisibility; }
            set
            {
                if (_toolbarVisibility != value)
                {
                    _toolbarVisibility = value;
                    OnPropertyChanged("ToolbarVisibility");
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

        #region Serialization
        public override string ToString()
        {
            return DataContractSerializationHelper.Serialize<EditorConfiguration>(this);
        }
        public static EditorConfiguration FromString(string str)
        {
            EditorConfiguration conf = new EditorConfiguration();
            if (string.IsNullOrWhiteSpace(str))
                return conf;
            else
            {
                EditorConfiguration temp = DataContractSerializationHelper.Deserialize<EditorConfiguration>(str);
                if (temp != null)
                {
                    conf.AlwaysDisplayDefaultTemplates = temp.AlwaysDisplayDefaultTemplates;
                    conf.AutoComplete = temp.AutoComplete;
                    conf.AutoSave = temp.AutoSave;
                    conf.AutoSelect = temp.AutoSelect;
                    conf.Continuous = temp.Continuous;
                    conf.UseDefaultLayerIds = temp.LayerIds == null;
                    conf.LayerIds = temp.LayerIds;
                    conf.MaintainAspectRatio = temp.MaintainAspectRatio;
                    conf.ShowAttributesOnAdd = temp.ShowAttributesOnAdd;
                    conf.SelectTool = temp.SelectTool;
                    conf.AddToSelectionTool = temp.AddToSelectionTool;
                    conf.RemoveFromSelectionTool = temp.RemoveFromSelectionTool;
                    conf.ClearSelectionTool = temp.ClearSelectionTool;
                    conf.DeleteTool = temp.DeleteTool;
                    conf.EditValuesTool = temp.EditValuesTool;
                    conf.EditShapesTool = temp.EditShapesTool;
                    conf.ReshapeTool = temp.ReshapeTool;
                    conf.UnionTool = temp.UnionTool;
                    conf.CutTool = temp.CutTool;
                    conf.FreehandTool = temp.FreehandTool;
                    conf.AutocompleteTool = temp.AutocompleteTool;
                    conf.ClearTool = temp.ClearTool;
                    conf.EditingShapesEnabled = temp.EditingShapesEnabled;

                }

                return conf;
            }
        }
        public EditorConfiguration Clone()
        {
            return EditorConfiguration.FromString(this.ToString());
        }
        #endregion
    }
}
