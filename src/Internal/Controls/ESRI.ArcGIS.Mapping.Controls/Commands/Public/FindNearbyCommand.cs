/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
    [DisplayName("FindNearbyDisplayName")]
	[Category("CategorySelection")]
	[Description("FindNearbyDescription")]
    public class FindNearbyCommand : LayerCommandBase
    {
        private string GeometryServiceUrl 
        {
            get
            {
                if (View.Instance != null && View.Instance.ConfigurationStore != null
                    && View.Instance.ConfigurationStore.GeometryServices != null
                    && View.Instance.ConfigurationStore.GeometryServices.Count > 0)
                {
                    return View.Instance.ConfigurationStore.GeometryServices[0].Url;
                }

                return null;
            }
        }

        private Map Map
        {
            get
            {
                return MapApplication.Current.Map;
            }
        }       

        private void showFindNearbyToolWindow()
        {
            string title = Resources.Strings.FindNearbyTitle;
            
            findNearbyToolWindow = new FindNearbyDialog();
            findNearbyToolWindow.FindNearbyExecuted += new EventHandler<FindNearbyEventArgs>(findNearbyToolWindow_FindNearby);

            findNearbyToolWindow.LayersInMap = Map.Layers;
            findNearbyToolWindow.SelectedLayer = Layer;

            MapApplication.Current.ShowWindow(title, findNearbyToolWindow);
        }

        void findNearbyToolWindow_FindNearby(object sender, FindNearbyEventArgs e)
        {
            if (Layer == null)
                return;

            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
                return;

            if (graphicsLayer.SelectionCount < 1)
            {
                findNearbyToolWindow.StopBusyIndicator();
                MessageBoxDialog.Show(Resources.Strings.MsgNoFeaturesSelected, Resources.Strings.ErrorCaption, MessageBoxButton.OK);
                return;
            }

            BufferParameters bufferParams = new BufferParameters();
            switch (e.LinearUnit)
            {
                case LinearUnit.Miles:
                    bufferParams.Unit = ESRI.ArcGIS.Client.Tasks.LinearUnit.StatuteMile;
                    break;
                case LinearUnit.Meters:
                    bufferParams.Unit = ESRI.ArcGIS.Client.Tasks.LinearUnit.Meter;
                    break;
                case LinearUnit.Kilometers:
                    bufferParams.Unit = ESRI.ArcGIS.Client.Tasks.LinearUnit.Kilometer;
                    break;
            }
            bufferParams.UnionResults = true;
            bufferParams.OutSpatialReference = Map.SpatialReference;
            SpatialReference gcs = new SpatialReference(4326);
            bufferParams.BufferSpatialReference = gcs;
            bufferParams.Geodesic = true;
            bufferParams.Distances.Add(e.Distance);

            // Check the spatial reference of the first graphic
            Graphic firstGraphic = graphicsLayer.SelectedGraphics.ElementAt(0);
            bool isInGcs = firstGraphic.Geometry != null
                             && firstGraphic.Geometry.SpatialReference != null
                             && firstGraphic.Geometry.SpatialReference.Equals(gcs);

            // In order to perform geodesic buffering we need to pass geometries in GCS to the geom service
            if (isInGcs)
            {
                foreach (Graphic selectedGraphic in graphicsLayer.SelectedGraphics)
                    bufferParams.Features.Add(selectedGraphic);

                buffer(GeometryServiceUrl, bufferParams, e);
            }
            else
            {
                GeometryServiceOperationHelper helper = new GeometryServiceOperationHelper(GeometryServiceUrl);
                helper.ProjectGraphicsCompleted += (o, args) => {
                    foreach (Graphic selectedGraphic in args.Graphics)
                        bufferParams.Features.Add(selectedGraphic);
                    buffer(GeometryServiceUrl, bufferParams, e);
                };
                helper.ProjectGraphics(graphicsLayer.SelectedGraphics.ToList(), new SpatialReference(4326));
            }
        }        

        void buffer(string agsGeometryServerUrl, BufferParameters bufferParams, FindNearbyEventArgs findNearbyRequest)
        {
            if (string.IsNullOrEmpty(agsGeometryServerUrl))
                return;

            GeometryService geomService = new GeometryService
            {
                Url = agsGeometryServerUrl
            };
            geomService.BufferCompleted += GeometryService_BufferCompleted;
            geomService.Failed += (o, e) =>
            {
                if (findNearbyToolWindow != null)
                {
                    findNearbyToolWindow.StopBusyIndicator();
                    MapApplication.Current.HideWindow(findNearbyToolWindow);
                }
                MessageBoxDialog.Show(Resources.Strings.MsgErrorExecutingBufferOperation + Environment.NewLine + e.Error.ToString());                
            };
            geomService.BufferAsync(bufferParams, findNearbyRequest);
        }

        void GeometryService_BufferCompleted(object sender, GraphicsEventArgs args)
        {
            IList<Graphic> results = args.Results;
            if (results == null || results.Count < 1)
                return;

            FindNearbyEventArgs eventArgs = args.UserState as FindNearbyEventArgs;
            if (eventArgs == null)
                return;

            GraphicsLayer targetGraphicsLayer = eventArgs.SelectedLayer;
            if(targetGraphicsLayer == null)
                return;

            #region Draw the buffer graphics layer
            GraphicsLayer bufferGraphicsLayer = new GraphicsLayer() { ID = eventArgs.EventId };            

            // Add the circle graphic
            Graphic circleGraphic = results[0];
            circleGraphic.Attributes.Add(Resources.Strings.BufferDistance, string.Format("{0} {1}", eventArgs.Distance, eventArgs.LinearUnit.ToString()));
            SimpleFillSymbol defaultBufferSymbol = new SimpleFillSymbol()
            {
                BorderBrush = new SolidColorBrush(Colors.Blue),
                BorderThickness = 1,
                Fill = new SolidColorBrush(new Color() { A =(byte)102, R = (byte)255, G = (byte)0, B = (byte)0})
            };
            SimpleRenderer simpleRenderer = new SimpleRenderer() { 
                 Symbol = defaultBufferSymbol,
            };
            bufferGraphicsLayer.Renderer = simpleRenderer;
            Collection<FieldInfo> layerFields = Core.LayerExtensions.GetFields(bufferGraphicsLayer);
            if (layerFields != null)
                layerFields.Add(new FieldInfo() { DisplayName = Resources.Strings.BufferDistance, Name = Resources.Strings.BufferDistance, FieldType = FieldType.Text });             
            bufferGraphicsLayer.Graphics.Add(circleGraphic);
            #endregion

            Graphic graphicCircle = results[0];
            if (graphicCircle != null)
            {
                ESRI.ArcGIS.Client.Geometry.Geometry graphicCircleGeometry = graphicCircle.Geometry;
                if (graphicCircleGeometry != null)
                {
                    // Ensure that atleast the first graphic of the graphics layer has a spatial reference set
                    // because ESRI.ArcGIS.Client.Tasks checks it
                    if (targetGraphicsLayer.Graphics.Count > 0)
                    {
                        if (targetGraphicsLayer.Graphics[0].Geometry != null
                            && targetGraphicsLayer.Graphics[0].Geometry.SpatialReference == null && Map != null)
                        {
                            targetGraphicsLayer.Graphics[0].Geometry.SpatialReference = Map.SpatialReference;
                        }
                    }

                    // The extent of the result layer should be the extent of the circle
                    if(Map != null)
                        Map.ZoomTo(expandExtent(graphicCircleGeometry.Extent));

                    relationAsync(GeometryServiceUrl, targetGraphicsLayer.Graphics.ToList(), results, new object[] { eventArgs, bufferGraphicsLayer });
                }
            }
        }

        private Client.Geometry.Geometry expandExtent(Envelope extentUnion)
        {
            if (extentUnion == null)
                return null;

            // expand the extent by 20%
            double zoomFactor = 0.2;
            extentUnion.XMax += extentUnion.Width * zoomFactor;
            extentUnion.XMin -= extentUnion.Width * zoomFactor;
            extentUnion.YMax += extentUnion.Height * zoomFactor;
            extentUnion.YMin -= extentUnion.Height * zoomFactor;

            return extentUnion;
        }

        void relationAsync(string agsGeometryServerUrl, IList<Graphic> graphics1, IList<Graphic> graphics2, object userToken)
        {
            GeometryService geomService = new GeometryService
            {
                Url = agsGeometryServerUrl
            };
            geomService.RelationCompleted += geometryService_RelationCompleted;
            geomService.Failed += (o, e) =>
            {
                MessageBoxDialog.Show(Resources.Strings.MsgErrorExecutingRelationAsyncOperation + Environment.NewLine + e.Error.ToString());
                if (findNearbyToolWindow != null)
                {
                    findNearbyToolWindow.StopBusyIndicator();
                    MapApplication.Current.HideWindow(findNearbyToolWindow);
                }
            };
            geomService.RelationAsync(graphics1, graphics2, GeometryRelation.esriGeometryRelationIntersection, null, userToken);
        }

        void geometryService_RelationCompleted(object sender, RelationEventArgs e)
        {
            object[] state = e.UserState as object[];
            if (state == null || state.Length < 1)
                return;

            FindNearbyEventArgs eventArgs = state[0] as FindNearbyEventArgs;
            if (eventArgs == null)
                return;            

            GraphicsLayer layerToCompare = eventArgs.SelectedLayer;
            if (layerToCompare == null)
                return;

            GraphicsLayer resultsLayer = new GraphicsLayer() {                  
                 Opacity = layerToCompare.Opacity,                         
            };
            if (layerToCompare.Renderer != null)
                resultsLayer.Renderer = layerToCompare.Renderer.CloneRenderer();
            GraphicsLayer bufferGraphicsLayer = null;
            if(state.Length > 1)
                bufferGraphicsLayer = state[1] as GraphicsLayer;            
            
            foreach (GeometryRelationPair geomPair in e.Results)
            {
                int index = geomPair.Graphic1Index;
                if (index < 0 || index >= layerToCompare.Graphics.Count)
                    continue;
                Graphic intersectingGraphic = layerToCompare.Graphics[index];
                resultsLayer.Graphics.Add(copyGraphic(intersectingGraphic));
            }

            CopyLayerConfiguration(layerToCompare, resultsLayer);
            int insertIndex = Map.Layers.IndexOf(layerToCompare);
            string displayName = getDisplayNameForResultLayer(eventArgs.LayerDisplayName);
            AddResultLayer(bufferGraphicsLayer, true, Resources.Strings.FindNearbyBufferPrefix + displayName, ESRI.ArcGIS.Mapping.Core.GeometryType.Polygon, insertIndex);
            AddResultLayer(resultsLayer, true, Resources.Strings.FindNearbyPrefix + displayName, Core.LayerExtensions.GetGeometryType(layerToCompare), insertIndex);            
            if (findNearbyToolWindow != null)
            {
                findNearbyToolWindow.StopBusyIndicator();
                MapApplication.Current.HideWindow(findNearbyToolWindow);
            }
        }

        public string getDisplayNameForResultLayer(string input)
        {
            if (Map == null)
                return input;

            int index = 0;
            foreach (Layer layer in Map.Layers)
            {
                string layerDisplayName = layer.GetValue(ESRI.ArcGIS.Client.Extensibility.MapApplication.LayerNameProperty) as string;
                if (string.IsNullOrEmpty(layerDisplayName))
                    continue;

                if(layerDisplayName.StartsWith(Resources.Strings.FindNearbyPrefix + input, StringComparison.Ordinal))
                {
                    index++;
                }
            }
            if (index > 0)
                return string.Format(input + " (" + (index + 1) + ")");
            return input;
        }

        private void AddResultLayer(GraphicsLayer resultsLayer, bool isSelected, string displayName,
            ESRI.ArcGIS.Mapping.Core.GeometryType geometryType, int? index)
        {
            resultsLayer.SetValue(ESRI.ArcGIS.Client.Extensibility.MapApplication.LayerNameProperty, displayName);
            resultsLayer.ID = displayName;
            resultsLayer.SetValue(ESRI.ArcGIS.Client.Extensibility.LayerExtensions.InitialUpdateCompletedProperty, true);
            if (index == null || index.Value < 0 || index.Value >= Map.Layers.Count)
                Map.Layers.Add(resultsLayer);
            else
                Map.Layers.Insert(index.Value, resultsLayer);
        }

        private void CopyLayerConfiguration(GraphicsLayer source, GraphicsLayer target)
        {
            if (source == null || target == null)
                return;
            Core.LayerExtensions.SetFields(target, Core.LayerExtensions.GetFields(source));
            Core.LayerExtensions.SetGeometryType(target, Core.LayerExtensions.GetGeometryType(source));
            Core.LayerExtensions.SetGradientBrush(target, Core.LayerExtensions.GetGradientBrush(source));            
        }

        Graphic copyGraphic(Graphic graphic)
        {
            if (graphic == null)
                return null;
            Graphic newGraphic = new Graphic()
            {
                Geometry = graphic.Geometry,
                Symbol = graphic.Symbol,
                Selected = false,
                TimeExtent = graphic.TimeExtent,                
            };
            foreach (KeyValuePair<string, object> pair in graphic.Attributes)
                newGraphic.Attributes.Add(pair);
            return newGraphic;
        }

        #region ICommand Members

        public override bool CanExecute(object parameter)
        {            
            if (Layer == null)
                return false;

            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
            {
                closeWindowIfNoPointSelection();
                return false;
            }

            bool canExecute = graphicsLayer.SelectedGraphics.Count() > 0
                && graphicsLayer.SelectedGraphics.ElementAt(0).Geometry is MapPoint;
            if (!canExecute)
                closeWindowIfNoPointSelection();
            return canExecute;
        }

        private void closeWindowIfNoPointSelection()
        {
            if (findNearbyToolWindow != null)
                MapApplication.Current.HideWindow(findNearbyToolWindow);
        }

        FindNearbyDialog findNearbyToolWindow;        
        public override void Execute(object parameter)
        {
            if (Layer == null)
                return;

            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
                return;

            showFindNearbyToolWindow();
        }

        #endregion
    }    
}
