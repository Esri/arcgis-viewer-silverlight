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
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Threading;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Core;
using System.Windows.Markup;
using System.Windows.Controls;
using ESRI.ArcGIS.Mapping.Identify.Resources;

namespace ESRI.ArcGIS.Mapping.Identify
{
	[Export(typeof(Behavior<Map>))]
	[ESRI.ArcGIS.Client.Extensibility.DisplayName("IdentifyBehaviorDisplayName")]
	public class IdentifyBehavior : Behavior<Map>
	{
        OnClickPopupInfo _popupInfo;

		Map Map
		{
			get
			{
				return AssociatedObject;
			}
		}

		#region Behavior Overrides
		protected override void OnAttached()
		{
			base.OnAttached();

			if (AssociatedObject != null)
			{
				AssociatedObject.MouseClick += map_MouseClick;
                AssociatedObject.MouseLeftButtonDown += map_MouseLeftButtonDown;
				AssociatedObject.ExtentChanging += map_ExtentChanging;
                AssociatedObject.MapGesture += map_Gesture;
			}

			doubleClickTimer = new DispatcherTimer();
			doubleClickTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
			doubleClickTimer.Tick += doubleClickTimer_Tick;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			if (AssociatedObject != null)
			{
				AssociatedObject.MouseClick -= map_MouseClick;
                AssociatedObject.MouseLeftButtonDown -= map_MouseLeftButtonDown;
                AssociatedObject.ExtentChanging -= map_ExtentChanging;
                AssociatedObject.MapGesture -= map_Gesture;
			}

			if (doubleClickTimer != null)
				doubleClickTimer.Tick -= doubleClickTimer_Tick;

            //_onClickPopupControl = null;
		}
		#endregion

		#region Getting mouse click
		void doubleClickTimer_Tick(object sender, EventArgs e)
		{
			doubleClickTimer.Stop();
			waitedForDoubleClick = true;

            // Initialize layer containing activity indicator to display on the map while results are 
            // being retrieved.  Only add the activity indicator if there are outstanding identify
            // operations and no results have been returned yet.
            if (PendingIdentifies > 0 && (identifyTaskResults == null || identifyTaskResults.Count == 0))
            {
                if (busyLayer == null)
                    initializeBusyLayer();
                else
                    busyLayer.Graphics[0].Geometry = clickPoint;

                if (!Map.Layers.Contains(busyLayer))
                    Map.Layers.Add(busyLayer);
            }


			if (identifyTaskResults != null && identifyTaskResults.Count > 0)
			{
				foreach (object item in identifyTaskResults)
				{
					IdentifyEventArgs args = item as IdentifyEventArgs;
					if (args != null)
					{
						reportResults(args);
					}
					else
					{
						GraphicsLayerResult result = item as GraphicsLayerResult;
						if (result != null)
						{
							reportResults(result);
						}
					}
				}
			}
		}

		DispatcherTimer doubleClickTimer;
		bool waitedForDoubleClick;
		bool doNotShowResults;
        bool initiatedByTouch;
        void map_ExtentChanging(object sender, ExtentEventArgs e)
        {
            if (PendingIdentifies == 0) return;

            // If the clicked point goes outside the map extent, don't report results
            if (clickPoint.X > Map.Extent.XMax || clickPoint.X < Map.Extent.XMin
            || clickPoint.Y > Map.Extent.YMax || clickPoint.Y < Map.Extent.YMin)
            {
                removeBusyIndicator();
                doNotShowResults = true;
            }
        }
		List<object> identifyTaskResults;

		void map_MouseClick(object sender, Map.MouseEventArgs e)
		{
            if (e.Handled) 
                return;

            if (!doubleClickTimer.IsEnabled) // First click
            {
                doubleClickTimer.Start();

                Dispatcher.BeginInvoke(() =>
                    {
                        if (e.Handled)
                        {
                            doubleClickTimer.Stop();
                            if (Map.Layers.Contains(busyLayer))
                                removeBusyIndicator();
                            return;
                        }

                        doNotShowResults = false;
                        identifyTaskResults = null;
                        waitedForDoubleClick = false;
                        initiatedByTouch = false;

                        clickPoint = e.MapPoint;

                        DoIdentify();
                    });
            }

		}

        private void map_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Check for the 2nd click of a double-click.  We do this in MouseLeftButtonDown because
            // the 2nd MouseClick event does not fire consistently.
            if (doubleClickTimer.IsEnabled) 
            {
                doubleClickTimer.Stop();
                doNotShowResults = true;
                return;
            }
        }

        void map_Gesture(object sender, Map.MapGestureEventArgs e)
        {
            if (e.Gesture == GestureType.Tap || e.Gesture == GestureType.Hold)
            {
                doNotShowResults = false;
                identifyTaskResults = null;
                clickPoint = e.MapPoint;
                waitedForDoubleClick = false;
                initiatedByTouch = true;
                DoIdentify();
            }
        }
        #endregion

		#region Identify Implementation
		private int pendingIdentifies;

		public int PendingIdentifies
		{
			get { return pendingIdentifies; }
			set
			{
				pendingIdentifies = value;
				if (_popupInfo != null)
					_popupInfo.InProgress = (pendingIdentifies > 0);
			}
		}

        Collection<int> getIdentifyLayerIds(Layer layer)
        {
            if (!(layer is ArcGISDynamicMapServiceLayer) && !(layer is ArcGISTiledMapServiceLayer))
                throw new ArgumentException(Strings.LayerTypeError);

            // Get layer as dynamic to access GetLayerVisibility without type-casting
            dynamic dynLayer = layer as dynamic;

            IDictionary<int, string> templates = LayerProperties.GetPopupDataTemplates(layer);
            if (templates == null && ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetUsePopupFromWebMap(layer))
                templates = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetWebMapPopupDataTemplates(layer);

            Collection<int> ids = new Collection<int>();
            if (templates != null) // web map pop-ups are being used.  Base inclusion on whether sub-layer is visible.
            {
                foreach (var item in templates)
                {
                    if (!string.IsNullOrEmpty(item.Value) && dynLayer.GetLayerVisibility(item.Key))
                        ids.Add(item.Key);
                }
            }
            else
            {
                // Get sub-layers for which pop-ups have been enabled
                Collection<int> enabledIDs = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetIdentifyLayerIds(layer);
                
                // Filter sub-layers based on visibility
                foreach (int id in enabledIDs)
                {
                    // Check whether sub-layer and parent layers are visible
                    if (dynLayer.GetLayerVisibility(id) && parentLayersVisible(layer, id))
                        ids.Add(id);
                }
            }

            return ids;
        }

        private bool parentLayersVisible(Layer layer, int subLayerID)
        {
            Func<LayerInfo, bool> parentLayerSelector = l => l.SubLayerIds != null && 
                l.SubLayerIds.Contains(subLayerID);
            var parentLayers = layer is ArcGISDynamicMapServiceLayer ? 
                ((ArcGISDynamicMapServiceLayer)layer).Layers.Where(parentLayerSelector) :
                ((ArcGISTiledMapServiceLayer)layer).Layers.Where(parentLayerSelector);
            foreach (var parent in parentLayers)
            {
                if (!((dynamic)layer).GetLayerVisibility(parent.ID) || !parentLayersVisible(layer, parent.ID))
                    return false;
            }

            return true;
        }

		List<IdentifyTask> identifyTasks;
		MapPoint clickPoint;
        GraphicsLayer busyLayer;
		private void DoIdentify()
		{
			if (Map == null)
				return;

			if (PendingIdentifies > 0 && identifyTasks != null)
			{
				foreach (IdentifyTask task in identifyTasks)
				{
					task.CancelAsync();
				}
			}
			identifyTasks = new List<IdentifyTask>();
			List<IdentifyParameters> parameters = new List<IdentifyParameters>();
			List<Layer> layers = new List<Layer>();
			List<GraphicsLayerResult> graphicsLayerResults = new List<GraphicsLayerResult>();
			identifyTaskResults = new List<object>();
			IdentifyTask identifyTask;
            _popupInfo = MapApplication.Current.GetPopup(null, null);

            // Watch for property changes on the view model. The one in particular that this event handler is interested
            // in is when the popupitem changes. When this happens we will want to then establish a new event handler for
            // when properties change of the popupitem. In particular, when the Graphic property changes so that we can
            // reconstruct the header value for the popup if the value that changed was the field used to display in the
            // header.
            _popupInfo.PropertyChanged += _popupInfo_PropertyChanged;

			for (int i = Map.Layers.Count - 1; i >= 0; i--)
			{
				Layer layer = Map.Layers[i];
				if (!ESRI.ArcGIS.Client.Extensibility.LayerProperties.GetIsPopupEnabled(layer))
					continue;
				if (!layer.Visible)
					continue;
				if (layer is ArcGISDynamicMapServiceLayer || layer is ArcGISTiledMapServiceLayer)
				{

                    Collection<int> layerIds = getIdentifyLayerIds(layer);

                    // If layer is a dynamic map service layer, get layer definitions
                    IEnumerable<LayerDefinition> layerDefinitions = null;
                    IEnumerable<TimeOption> timeOptions = null;
                    if (layer is ArcGISDynamicMapServiceLayer)
                    {
                        var dynamicLayer = (ArcGISDynamicMapServiceLayer)layer;
                        if (dynamicLayer.LayerDefinitions != null)
                        {
                            layerDefinitions = dynamicLayer.LayerDefinitions.Where(
                                l => layerIds.Contains(l.LayerID));
                        }

                        if (dynamicLayer.LayerTimeOptions != null)
                        {
                            timeOptions = dynamicLayer.LayerTimeOptions.Where(
                                to => layerIds.Contains(int.Parse(to.LayerId)));
                        }
                        else if (dynamicLayer.TimeExtent != null)
                        {
                            timeOptions = dynamicLayer.Layers.Select(l => new TimeOption()
                            {
                                LayerId = l.ID.ToString(),
                                UseTime = true
                            });
                        }
                    }

                    if ((layer.GetValue(ESRI.ArcGIS.Mapping.Core.LayerExtensions.LayerInfosProperty) as Collection<LayerInformation>)
                    == null) //require layer infos
                        continue;
					if ((layerIds != null && layerIds.Count > 0))
					{
						identifyTask = new IdentifyTask(IdentifySupport.GetLayerUrl(layer));
						identifyTask.ExecuteCompleted += IdentifyTask_ExecuteCompleted;
						identifyTask.Failed += IdentifyTask_Failed;

                        string proxy = IdentifySupport.GetLayerProxyUrl(layer);
                        if (!string.IsNullOrEmpty(proxy)) identifyTask.ProxyURL = proxy;

						int width, height;
						if (!int.TryParse(Map.ActualHeight.ToString(), out height))
							height = 100;
						if (!int.TryParse(Map.ActualWidth.ToString(), out width))
							width = 100;
						IdentifyParameters identifyParams = new IdentifyParameters()
						{
							Geometry = clickPoint,
							MapExtent = Map.Extent,
							SpatialReference = Map.SpatialReference,
							LayerOption = LayerOption.visible,
							Width = width,
							Height = height, 
                            LayerDefinitions = layerDefinitions,
                            TimeExtent = AssociatedObject.TimeExtent,
                            TimeOptions = timeOptions
						};
						foreach (int item in layerIds)
							identifyParams.LayerIds.Add(item);
						identifyTasks.Add(identifyTask);
						parameters.Add(identifyParams);
						layers.Add(layer);
					}
				}
				else if (layer is GraphicsLayer &&
					ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetPopUpsOnClick(layer as GraphicsLayer))
				{
					GraphicsLayer graphicsLayer = layer as GraphicsLayer;
					GeneralTransform t = GetTransformToRoot(Map);
					IEnumerable<Graphic> selected = Core.Utility.FindGraphicsInHostCoordinates(graphicsLayer, t.Transform(Map.MapToScreen(clickPoint)));
					if (selected != null)
					{
						List<Graphic> results = new List<Graphic>(selected);
						if (results.Count > 0)
						{
							graphicsLayerResults.Add(new GraphicsLayerResult()
							{
								Layer = graphicsLayer,
								Results = results,
								ClickedPoint = clickPoint
							});
						}
					}
				}
			}
			PendingIdentifies = identifyTasks.Count + graphicsLayerResults.Count;
			if (PendingIdentifies > 0)
			{
				for (int i = 0; i < identifyTasks.Count; i++)
				{
					identifyTasks[i].ExecuteAsync(parameters[i], new UserState() { ClickedPoint = clickPoint, Layer = layers[i] });
				}
				for (int i = 0; i < graphicsLayerResults.Count; i++)
				{
					reportResults(graphicsLayerResults[i]);
				}

			}
		}
		/// <summary>
		/// Gets the root visual transform for both WPF, Silverlight and WinForms
		/// </summary>
		/// <param name="child"></param>
		/// <returns></returns>
		internal static GeneralTransform GetTransformToRoot(FrameworkElement child)
		{
			if (Application.Current != null)
			{
				if (child.FlowDirection == FlowDirection.RightToLeft)
					return child.TransformToVisual(null);
				return child.TransformToVisual(Application.Current.RootVisual);
			}
			else
				return child.TransformToVisual(null);

		}
		// Fires when the identify operation has completed successfully.  Updates the data shown in the
		// identify dialog.
		private void IdentifyTask_ExecuteCompleted(object sender, IdentifyEventArgs args)
		{
			PendingIdentifies--;
			if (!waitedForDoubleClick && !initiatedByTouch)
			{
				identifyTaskResults.Add(args);
				return;
			}
			reportResults(args);
		}


		void reportResults(GraphicsLayerResult result)
		{
			if (!waitedForDoubleClick && !initiatedByTouch)
			{
				identifyTaskResults.Add(result);
				return;
			}
            if (doNotShowResults) { removeBusyIndicator(); return; }
			PendingIdentifies--;

            if (result != null && result.Results != null)
			{
                List<PopupItem> popupItems = PopupHelper.GetPopupItems(result.Results, result.Layer);
                popupItems.ForEach(p => _popupInfo.PopupItems.Add(p));
            }

            removeBusyIndicator();
            if (_popupInfo.PopupItems.Count > 0)
            {
                if (_popupInfo.SelectedIndex < 0)
                    _popupInfo.SelectedIndex = 0;

                PopupHelper.ShowPopup(_popupInfo, clickPoint);
            }
		}

		void reportResults(IdentifyEventArgs args)
		{
            if (doNotShowResults) { removeBusyIndicator(); return; }

			if (args.IdentifyResults != null && args.IdentifyResults.Count > 0)
			{
				Layer layer = (args.UserState as UserState).Layer;
                Collection<LayerInformation> layerInfos = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetLayerInfos(layer);
				foreach (IdentifyResult result in args.IdentifyResults)
				{
                    LayerInformation layerInfo = layerInfos.FirstOrDefault(l => l.ID == result.LayerId);
                    if (layerInfo != null)
                    {
                        PopupItem popupItem = PopupHelper.GetPopupItem(result.Feature, layer, layerInfo.Fields, layerInfo, result.LayerName, result.Value.ToString(), result.LayerId);
                        if (popupItem != null)
                            _popupInfo.PopupItems.Add(popupItem);
                        }
                    }
				}

            removeBusyIndicator();
            if (_popupInfo.PopupItems.Count > 0)
		{
                if (_popupInfo.SelectedIndex < 0)
                    _popupInfo.SelectedIndex = 0;

                PopupHelper.ShowPopup(_popupInfo, clickPoint);
			}
		}

		private void IdentifyTask_Failed(object sender, TaskFailedEventArgs e)
		{
			PendingIdentifies--;
            if (PendingIdentifies == 0) removeBusyIndicator();
			UserState state = e.UserState as UserState;
			if (state != null && state.Layer != null)
			{
				string layer = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetLayerName(state.Layer);
				if (!string.IsNullOrWhiteSpace(layer))
				{
					ESRI.ArcGIS.Mapping.Controls.MessageBoxDialog.Show(string.Format(Resources.Strings.IdentifyFailedForLayer, layer),
						Resources.Strings.IdentifyError, MessageBoxButton.OK);
					return;
				}
			}
			ESRI.ArcGIS.Mapping.Controls.MessageBoxDialog.Show(Resources.Strings.IdentifyFailed, Resources.Strings.IdentifyError, MessageBoxButton.OK);
		}

        PopupItem _oldPopupItem;
        void _popupInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PopupItem")
            {
                // If the last used (old) item in the model is not null then remove the event handler which was
                // attached since it is likely to be a different item now.
                if (_oldPopupItem != null)
                    _oldPopupItem.PropertyChanged -= PopupItem_PropertyChanged;

                // If the currently selected item is not null, then we want to listen for when a property of
                // this object changes so establish the event handler.
                if (_popupInfo.PopupItem != null)
                    _popupInfo.PopupItem.PropertyChanged += PopupItem_PropertyChanged;

                // Whatever the now current item is, store it as the last used (old) item so we can unhook
                // event handlers when it is changed.
                _oldPopupItem = _popupInfo.PopupItem;
            }
        }

        void PopupItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // If the property of the popupitem that changed was the Graphic, then we need to reconstruct the title
            // since the attribute used for the header might have been one of the ones that was changed and these
            // need to be kept in sync.
            if (e.PropertyName == "Graphic")
            {
                _popupInfo.PopupItem.Title = MapTipsHelper.ConvertExpressionWithFieldNames(_popupInfo.PopupItem, _popupInfo.PopupItem.TitleExpression);
            }
        }

        private void initializeBusyLayer()
        {
            ESRI.ArcGIS.Client.Symbols.MarkerSymbol busySymbol = new Client.Symbols.MarkerSymbol()
                {
                    OffsetX = 9,
                    OffsetY = 9
                };
            busySymbol.ControlTemplate = XamlReader.Load(
                @"<ControlTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                    xmlns:mapping=""http://schemas.esri.com/arcgis/mapping/2009"">
                        <mapping:ActivityIndicator HorizontalAlignment=""Center"" VerticalAlignment=""Center"" 
                            AutoStartProgressAnimation=""True"">
                                <mapping:ActivityIndicator.Effect>
                                    <DropShadowEffect ShadowDepth=""0"" BlurRadius=""5"" Color=""White"" />
                                </mapping:ActivityIndicator.Effect>
                        </mapping:ActivityIndicator>
                </ControlTemplate>") as ControlTemplate;
            Graphic busyGraphic = new Graphic() { Geometry = clickPoint, Symbol = busySymbol };

            busyLayer = new GraphicsLayer() { RendererTakesPrecedence = false };
            busyLayer.SetValue(ESRI.ArcGIS.Mapping.Core.LayerExtensions.ExcludeSerializationProperty, true);
            busyLayer.Graphics.Add(busyGraphic);
        }

        private void removeBusyIndicator()
        {
            Map.Layers.Remove(busyLayer);
        }
		#endregion

		public class UserState
		{
			public MapPoint ClickedPoint { get; set; }
			public Layer Layer { get; set; }
		}

		public class GraphicsLayerResult
		{
			public GraphicsLayer Layer { get; set; }
			public List<Graphic> Results { get; set; }
			public MapPoint ClickedPoint { get; set; }
		}
	}
}

