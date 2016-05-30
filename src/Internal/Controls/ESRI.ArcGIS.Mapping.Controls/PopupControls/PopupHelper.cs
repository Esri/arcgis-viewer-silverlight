/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Mapping.Core;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class PopupHelper
    {
        private static OnClickPopupControl OnClickPopupControl = new OnClickPopupControl();
        private static bool _wasEditingValues = false;

        internal static OnClickPopupInfo GetPopup(IEnumerable<Graphic> graphics, Layer layer, int? layerId = null)
        {
            OnClickPopupInfo popupInfo = new OnClickPopupInfo();
            OnClickPopupControl.PopupInfo = popupInfo;

            if (graphics != null && layer != null)
            {
                foreach (PopupItem popupItem in GetPopupItems(graphics, layer, layerId))
                    popupInfo.PopupItems.Add(popupItem);

                if (popupInfo.SelectedIndex < 0 && popupInfo.PopupItems.Count > 0)
                    popupInfo.SelectedIndex = 0;
            }

            return popupInfo;
        }

        internal static void ShowPopup(Graphic graphic, Layer layer, int? layerId = null)
        {
            OnClickPopupInfo popupInfo = GetPopup(new[]{graphic}, layer, layerId);

            if (graphic.Geometry is MapPoint)
                ShowPopup(popupInfo, (MapPoint)graphic.Geometry);
            else if (graphic.Geometry is Polygon)
            {
                if (!string.IsNullOrEmpty(MapApplication.Current.Urls.GeometryServiceUrl))
                {
                    GeometryService geometryService = new GeometryService(MapApplication.Current.Urls.GeometryServiceUrl);
                    geometryService.LabelPointsCompleted += (s, args) =>
                    {
                        if (args.Results != null && args.Results.Count > 0)
                            ShowPopup(popupInfo, (MapPoint)args.Results[0].Geometry);
                        else
                            ShowPopup(popupInfo, graphic.Geometry);
                    };
                    geometryService.Failed += (s, args) =>
                    {
                        ShowPopup(popupInfo,graphic.Geometry);
                    };
                    geometryService.LabelPointsAsync(new[] { graphic });
                }
                else
                {
                    ShowPopup(popupInfo,graphic.Geometry);
                }
            }
            SyncPreviousDisplayMode(popupInfo);
        }
        internal static void SetDisplayMode(DisplayMode mode)
        {
            _wasEditingValues = mode == DisplayMode.EditValues;
        }

        /// <summary>
        /// Executes edit values command if that is the mode we are in
        /// </summary>
        /// <param name="popupInfo"></param>
        public static void SyncPreviousDisplayMode(OnClickPopupInfo popupInfo)
        {
            if (popupInfo == null || popupInfo.PopupItem == null) return;

            InfoWindow win = popupInfo.Container as InfoWindow;
            if (win==null || OnClickPopupControl==null) return;

            // find the edit values command associated with the active tool
            EditValuesCommand editValues = OnClickPopupControl.GetEditValuesToolCommand();

            bool isEditable = LayerProperties.GetIsEditable(popupInfo.PopupItem.Layer);
            if (isEditable)
            {
                if (editValues == null)
                {
                    //if we were showing edit values panel, show that again
                    if (_wasEditingValues && popupInfo != null && popupInfo.Container is InfoWindow)
                    {
                        win.Dispatcher.BeginInvoke(() =>
                                                       {
                                                           EditValuesCommand cmd =
                                                               new EditValuesCommand();
                                                           cmd.Execute(popupInfo);
                                                       });
                    }
                }
                else
                {
                    if (_wasEditingValues)
                    {
                        win.Dispatcher.BeginInvoke(() => { editValues.Execute(popupInfo); });
                    }
                    else
                    {
                        editValues.BackToOriginalContent(popupInfo);
                    }
                }
            }
            else if(editValues !=null)
            {
                editValues.BackToOriginalContent(popupInfo);
            }
        }

        private static void ShowPopup(OnClickPopupInfo popupInfo, Geometry geometry)
        {
            if (popupInfo.Container is InfoWindow)
            {
                MapPoint anchorPoint = GetLastPoint(geometry);

                if (anchorPoint != null)
                    ShowPopup(popupInfo, anchorPoint);
            }
        }

        public static EditValuesCommand ShowPopup(OnClickPopupInfo popupInfo, MapPoint anchorPoint)
        {
            if (popupInfo.Container is InfoWindow)
            {
                ((InfoWindow)popupInfo.Container).Show(anchorPoint);
                SyncPreviousDisplayMode(popupInfo);
                if (OnClickPopupControl != null)
                    return OnClickPopupControl.GetEditValuesToolCommand();
            }
            return null;
        }

        private static MapPoint GetLastPoint(Geometry geometry)
        {
            MapPoint lastPoint = null;
            IEnumerable<PointCollection> pointCollections = null;

            if (geometry is Polygon)
                pointCollections = ((Polygon)geometry).Rings;
            else if (geometry is Polyline)
                pointCollections = ((Polyline)geometry).Paths;

            if (pointCollections != null)
            {
                PointCollection lastPointCollection = pointCollections.LastOrDefault();
                if (lastPointCollection != null)
                    lastPoint = lastPointCollection.LastOrDefault();
            }

            return lastPoint;
        }
     
        public static List<PopupItem> GetPopupItems(IEnumerable<Graphic> graphics, Layer layer, int? layerId = null)
        {
            List<PopupItem> popupItems = new List<PopupItem>();

            IEnumerable<FieldInfo> fields = null;
            LayerInformation layerInfo = null;
            if (layer is GraphicsLayer)
                fields = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetFields(layer as GraphicsLayer);
            else if (layerId != null)
            {
                Collection<LayerInformation> layerInfos = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetLayerInfos(layer);
                if (layerInfos != null)
                {
                    layerInfo = layerInfos.FirstOrDefault(l => l.ID == layerId);
                    if (layerInfo != null)
                        fields = layerInfo.Fields;
                }
            }

            string layerName = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetLayerName(layer);

            foreach (Graphic graphic in graphics)
            {
                PopupItem popupItem = GetPopupItem(graphic, layer, fields, layerInfo, layerName);
                if (popupItem != null)
                    popupItems.Add(popupItem);
            }
            return popupItems;
        }

        public static PopupItem GetPopupItem(Graphic graphic, Layer layer, IEnumerable<FieldInfo> fields, LayerInformation layerInfo, string layerName, string title = null, int? layerId = null)
        {
            if (layerInfo != null || layer is GraphicsLayer)
            {
                PopupItem popupItem = new PopupItem()
                {
                    Graphic = graphic,
                    Layer = layer,
                    Title = title,
                    LayerName = layerName,
                };
                if (layerId.HasValue) popupItem.LayerId = layerId.Value;

                if (layerInfo != null) GetMappedAttributes(graphic.Attributes, layerInfo);
                
                popupItem.FieldInfos = MapTipsHelper.ToFieldSettings(fields);
                MapTipsHelper.GetTitle(popupItem, layerInfo);

                bool hasContent = true;
                DataTemplate dt = MapTipsHelper.BuildMapTipDataTemplate(popupItem, out hasContent, layerInfo);
                if (!hasContent)
                    popupItem.DataTemplate = null;
                else if (dt != null)
                    popupItem.DataTemplate = dt;
                
                if (hasContent || (!string.IsNullOrWhiteSpace(popupItem.Title)))
                    return popupItem;

            }
            return null;
        }

        private static void GetMappedAttributes(IDictionary<string, object> originalAttributes, LayerInformation info)
        {
            if (info == null || info.Fields == null)
                return;

            Dictionary<string, object> atts = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> origAtt in originalAttributes)
            {
                FieldInfo field = GetFieldByAliasOnServer(origAtt.Key, info.Fields);
                if (field != null)
                {
                    if (!atts.ContainsKey(field.Name))
                        atts.Add(field.Name, origAtt.Value);
                }
            }
            foreach (var item in atts)
                {
                if (!originalAttributes.ContainsKey(item.Key))
                    originalAttributes.Add(item);
                }

            return;
            }
 
        private static FieldInfo GetFieldByAliasOnServer(string aliasOnServer, Collection<FieldInfo> fields)
        {
            if (fields == null)
                return null;

            foreach (FieldInfo info in fields)
                if (info.AliasOnServer.Equals(aliasOnServer))
                    return info;

            return null;
        }

        internal static void Reset()
        {
            if (OnClickPopupControl != null)
            {
                OnClickPopupControl.ResetInfoWindow();
                OnClickPopupControl.Dispose();
                OnClickPopupControl = new OnClickPopupControl();
            }
        }

        public enum DisplayMode
        {
            ReadOnly,
            EditValues,
        }
    }
}
