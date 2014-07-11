/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.FeatureService.Symbols;
using System.Windows.Media;
using ESRI.ArcGIS.Mapping.Controls.Editor;

namespace ESRI.ArcGIS.Mapping.Controls.Utils
{
    public class EditorCommandUtility
    {
        private static EditGeometry _editGeometry;
        private static Graphic _editedGraphic;
        private static EditorWidget _widget = null;
        private static ESRI.ArcGIS.Client.Editor _editor = null;
        private static FeatureLayer _editedLayer;
        private static bool _movingSelectedGraphic = false;

        private static bool CreateEditGeometry()
        {
            if (_editGeometry == null && View.Instance != null && View.Instance.Map != null)
            {
                SimpleMarkerSymbol vertexSymbol = new SimpleMarkerSymbol()
                    {
                        Color = new SolidColorBrush(Color.FromArgb(255, 225, 225, 225)),
                        OutlineColor = new SolidColorBrush(Colors.Black),
                        OutlineThickness = 1,
                        Style = SimpleMarkerSymbol.SimpleMarkerStyle.Square,
                        Size = 10
                    };
                _editGeometry = new EditGeometry
                {
                    Map = View.Instance.Map,
                    IsEnabled = true,
                    VertexSymbol = vertexSymbol
                };
                return true;
            }
            return false;
        }
        internal static Client.Editor FindEditorInVisualTree()
        {
            ESRI.ArcGIS.Client.Editor editor = null;
            EditorWidget widget = View.Instance.Editor;
            if (widget != null)
            {
                SetEditors(widget);
                editor = _editor;
            }
            return editor;
        }
        internal static void SetEditorWidget(EditorWidget widget)
        {
            SetEditors(widget);
        }

        private static void OnEditorWidgetEditCompleted(object sender, ESRI.ArcGIS.Client.Editor.EditEventArgs e)
        {
            OnEditorEditCompleted(sender, e);
        }

        private static void OnEditorEditCompleted(object sender, ESRI.ArcGIS.Client.Editor.EditEventArgs e)
        {
            FindEditorInVisualTree();
            if (_editor == null || _widget == null) return;

            if (EditorConfiguration.Current.ShowAttributesOnAdd && e.Action == ESRI.ArcGIS.Client.Editor.EditAction.Add)
            {
                foreach (ESRI.ArcGIS.Client.Editor.Change change  in e.Edits)
                {
                    var layer = change.Layer as FeatureLayer;
                    if (layer != null && change.Graphic != null && View.Instance != null && View.Instance.Map != null)
                    {
                        OnClickPopupInfo vm = MapApplication.Current.GetPopup(change.Graphic, layer);
                        if (vm != null)
                        {
                            PopupItem localPopupItem = vm.PopupItem;
                            if (localPopupItem == null && vm.PopupItems != null && vm.PopupItems.Count >= 1)
                                localPopupItem = vm.PopupItems[0];

                            if (localPopupItem != null && localPopupItem.Graphic != null && localPopupItem.Graphic.Geometry != null)
                            {
                                MapPoint popupAnchor = null;
                                MapPoint pt = localPopupItem.Graphic.Geometry as MapPoint;
                                if (pt != null)
                                {
                                    // This is a point feature.  Show the popup just above it and ignore edit handles
                                    Point screenPoint = View.Instance.Map.MapToScreen(pt);
                                    popupAnchor = View.Instance.Map.ScreenToMap(new Point(screenPoint.X, screenPoint.Y - 4));
                                }
                                else if (localPopupItem.Graphic.Geometry is Polygon)
                                {
                                    if (_editor.MoveEnabled || _editor.RotateEnabled || _editor.ScaleEnabled)
                                    {
                                        // editing is enabled
                                        Envelope env = localPopupItem.Graphic.Geometry.Extent;
                                        MapPoint tmpPt = new MapPoint(env.XMax - ((env.XMax - env.XMin) / 2.0), env.YMax);
                                        // Show the popup at the top/center of the graphic extent minus 4pixels
                                        Point screenPoint = View.Instance.Map.MapToScreen(tmpPt);
                                        popupAnchor = View.Instance.Map.ScreenToMap(new Point(screenPoint.X, screenPoint.Y - 4));
                                    }
                                    else
                                    {
                                        // no edit handles - position to northern most vertex
                                        popupAnchor = GetNorthernMostPointPolygon(localPopupItem.Graphic.Geometry as Polygon);
                                    }
                                }
                                else if (localPopupItem.Graphic.Geometry is Polyline)
                                {
                                    if (_editor.MoveEnabled || _editor.RotateEnabled || _editor.ScaleEnabled)
                                    {
                                        // editing is enabled
                                        Envelope env = localPopupItem.Graphic.Geometry.Extent;
                                        MapPoint tmpPt = new MapPoint(env.XMax - ((env.XMax - env.XMin) / 2.0), env.YMax);
                                        // Show the popup at the top/center of the graphic extent minus 4pixels
                                        Point screenPoint = View.Instance.Map.MapToScreen(tmpPt);
                                        popupAnchor = View.Instance.Map.ScreenToMap(new Point(screenPoint.X, screenPoint.Y - 4));
                                    }
                                    else
                                    {
                                        // no edit handles - position to northern most vertex
                                        popupAnchor = GetNorthernMostPointPolyline(localPopupItem.Graphic.Geometry as Polyline);
                                    }
                                }

                                // Show the identify popup
                                if (popupAnchor != null && vm.Container !=null)
                                {
                                    EditValuesCommand cmd = PopupHelper.ShowPopup(vm, popupAnchor);

                                    vm.Container.Dispatcher.BeginInvoke(() =>
                                                                  {
                                                                      if (cmd==null) 
                                                                          cmd = new EditValuesCommand();
                                                                      cmd.Execute(vm);
                                                                  });
                                    _viewModel = vm;
                                    // wait until layer edits are done before editing the shape
                                    if (layer.AutoSave)
                                    {
                                        layer.EndSaveEdits -= layer_EndSaveEdits;
                                        layer.EndSaveEdits += layer_EndSaveEdits;
                                    }
                                    else
                                    {
                                        // in case auto save is off, wait until the popup has loaded
                                        vm.Container.Loaded -= OnIdentifyPopupLoaded;
                                        vm.Container.Loaded += OnIdentifyPopupLoaded;
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }

        private static MapPoint GetNorthernMostPointPolyline(Polyline polyline)
        {
            if (polyline == null) return null;

            bool firstTime = true;
            MapPoint northernMost = null;
            foreach (var collection in polyline.Paths)
            {
                foreach(var pt in collection)
                {
                    if (firstTime)
                    {
                        firstTime = false;
                        northernMost = pt;
                    }
                    else if (pt.Y > northernMost.Y)
                        northernMost = pt;
                }
            }
            return northernMost;
        }

        private static MapPoint GetNorthernMostPointPolygon(Polygon polygon)
        {
            if (polygon == null) return null;

            bool firstTime = true;
            MapPoint northernMost = null;
            foreach (var ring in polygon.Rings)
            {
                foreach (var pt in ring)
                {
                    if (firstTime)
                    {
                        firstTime = false;
                        northernMost = pt;
                    }
                    else if (pt.Y > northernMost.Y)
                        northernMost = pt;
                }
            }
            return northernMost;
        }

        private static  OnClickPopupInfo _viewModel = null;
        static void OnIdentifyPopupLoaded(object sender, RoutedEventArgs e)
        {
            EditShape(_viewModel);
        }

        static void layer_EndSaveEdits(object sender, Client.Tasks.EndEditEventArgs e)
        {
            EditShape(_viewModel);
            // let go of the viewmodel
            _viewModel = null;
        }

        #region Edit Shape support
        public static bool CanEditShape(PopupItem popup, ESRI.ArcGIS.Client.Editor editor = null)
        {
            if (editor == null)
                editor = FindEditorInVisualTree();
            var featureLayer = popup.Layer as FeatureLayer;
            if (popup == null
            || popup.Graphic == null
            || featureLayer == null
            || featureLayer.LayerInfo == null
            || !featureLayer.IsGeometryUpdateAllowed(popup.Graphic)
            || !(LayerProperties.GetIsEditable(featureLayer))
            || editor == null
            || editor.EditVertices == null
            || !editor.EditVerticesEnabled
            || View.Instance.Map == null) return false;
            
            return true;
        }

        public static void EditShape(OnClickPopupInfo popupInfo, ESRI.ArcGIS.Client.Editor editor = null, bool dismissPopup = false)
        {
            if (popupInfo == null) return;

            if (editor == null)
                editor = FindEditorInVisualTree();

            PopupItem popup = popupInfo.PopupItem;
            if (popup == null && popupInfo.PopupItems != null && popupInfo.PopupItems.Count >= 1)
                popup = popupInfo.PopupItems[0];

            if (!CanEditShape(popup, editor))
                return;

            Graphic graphic = popup.Graphic;
            var featureLayer = popup.Layer as FeatureLayer;
            if (featureLayer != null && featureLayer.LayerInfo != null)
            {
                // Dismiss popup so it does not get in the way of shape edits
                InfoWindow container = popupInfo.Container as InfoWindow;
                if (container != null && dismissPopup)
                    container.IsOpen = false;

                if (featureLayer.LayerInfo.GeometryType == GeometryType.Polygon
                    || featureLayer.LayerInfo.GeometryType == GeometryType.Polyline)
                {
                    EditGeometry(graphic, featureLayer);
                }
                else if (featureLayer.LayerInfo.GeometryType == Client.Tasks.GeometryType.Point)
                {
                    // edit point using Editor
                    EditPoint(editor, graphic);
                }
            }
        }
        private static void EditPoint(ESRI.ArcGIS.Client.Editor editor, Graphic graphic)
        {
            StopPointEditing(editor.Map);

            graphic.Selected = true;
            _editedGraphic = graphic;
            graphic.MouseLeftButtonDown += EditedPointGraphicMouseLeftButtonDown;
            graphic.MouseLeftButtonUp += EditedPointGraphicMouseLeftButtonUp;
            editor.Map.MouseClick += Map_MouseClick;
            editor.Map.MouseMove += Map_MouseMove;
        }

        static void EditedPointGraphicMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_editor != null)
                StopPointEditing(_editor.Map);
        }

        static void Map_MouseClick(object sender, Map.MouseEventArgs e)
        {
            StopPointEditing((Map)sender);
        }

        static void EditedPointGraphicMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            _movingSelectedGraphic = true;
        }

        static void Map_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_movingSelectedGraphic)
            {
                Map map = (Map)sender;
                if (_editedGraphic != null && _editedGraphic.Geometry is MapPoint)
                    _editedGraphic.Geometry = map.ScreenToMap(e.GetPosition(map));
            }
        }

        static void Map_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Map map = (Map)sender;
            StopPointEditing((Map) sender);
        }

        private static void StopPointEditing(Map map)
        {
            _movingSelectedGraphic = false;
            if (_editedGraphic != null)
            {
                _editedGraphic.Selected = false;
                _editedGraphic.MouseLeftButtonDown -= EditedPointGraphicMouseLeftButtonDown;
                _editedGraphic.MouseLeftButtonUp -= EditedPointGraphicMouseLeftButtonUp;
                map.MouseClick -= Map_MouseClick;
                map.MouseMove -= Map_MouseMove;
                _editedGraphic = null;
            }
        }

        private static void EditGeometry(Graphic graphic, FeatureLayer layer)
        {
            _editGeometry = null;
            if (!CreateEditGeometry()) return;
            
            _editGeometry.GeometryEdit -= editGeometry_GeometryEdit;
            _editGeometry.GeometryEdit += editGeometry_GeometryEdit;
            _editedGraphic = graphic;
            _editedLayer = layer;

            _editGeometry.StartEdit(_editedGraphic);
        }

        /// <summary>
        /// Event handler for EditGeometry action notifications
        /// </summary>
        static void editGeometry_GeometryEdit(object sender, EditGeometry.GeometryEditEventArgs e)
        {
            // Handle when the user clicks to complete the edit operation
            if (e.Action == Client.EditGeometry.Action.EditCompleted)
            {
                _editGeometry.GeometryEdit -= editGeometry_GeometryEdit;
                if (_editedGraphic != null)
                {
                    if (_editedGraphic.Geometry.SpatialReference == null && View.Instance.Map != null && View.Instance.Map.SpatialReference != null)
                    {
                        // add the spatial reference so the Simplify opertion does not fail
                        _editedGraphic.Geometry.SpatialReference = View.Instance.Map.SpatialReference;
                    }

                    ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.GeometryService.SimplifyGraphicAsync(_editedGraphic, OnSimplifyCompleted);
                }
            }
        }

        public static void StopEditing()
        {
            if (_editGeometry != null)
                _editGeometry.StopEdit();

            ESRI.ArcGIS.Client.Editor editor = FindEditorInVisualTree();
            if (editor.CancelActive.CanExecute(null))
                editor.CancelActive.Execute(null);
        }

        public static void OnSimplifyCompleted(object sender, GraphicsEventArgs e)
        {
            if (_editGeometry != null && _editedGraphic != null && e.Results != null && e.Results.Count>0 && e.Results[0] != null)
            {
                _editedGraphic.Geometry = e.Results[0].Geometry;
            }
            // cleanup
            _editedLayer = null;
            _editGeometry = null;
        }

        #endregion


        /// <summary>
        /// Sets the editors if the widget is non-null.  Otherwise,
        /// current editor settings and event handlers are left intact.
        /// </summary>
        private static void SetEditors(EditorWidget widget)
        {
            if (widget == null || widget.DataContext == null) return;

            // set the widget event hookup
            bool widgetExists = false;
            if (_widget != null)
            {
                if (widget != _widget)
                {
                    // unhook the event handler from the previous editor
                    _widget.EditCompleted -= OnEditorWidgetEditCompleted;
                }
                else
                    widgetExists = true;
            }
            if (!widgetExists)
            {
                // set our editor to the newly found one
                _widget = widget;
                _widget.EditCompleted -= OnEditorWidgetEditCompleted;
                _widget.EditCompleted += OnEditorWidgetEditCompleted;
            }

            // manage the editor event hookup
            ESRI.ArcGIS.Client.Editor editor = widget.DataContext as ESRI.ArcGIS.Client.Editor;
            if (editor != null)
            {
                bool existing = false;
                // check the last editor
                if (_editor != null)
                {
                    if (editor != _editor)
                    {
                        // unhook the event handler from the previous editor
                        _editor.EditCompleted -= OnEditorEditCompleted;
                    }
                    else
                        existing = true;
                }
                if (!existing)
                {
                    // set our editor to the newly found one
                    _editor = editor;
                    _editor.EditCompleted -= OnEditorEditCompleted;
                    _editor.EditCompleted += OnEditorEditCompleted;
                }
            }
        }
    }
}
