/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Threading;
using ESRI.ArcGIS.Client;
using System.Windows.Threading;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Behaviors
{
    public class LayerAutoUpdateHelper
    {
        #region Fields

        private const int MinimumRefreshInterval = 1000;
        private volatile bool _refreshRunning;

        /// <summary>
        /// Low overhead timer that runs on worker thread
        /// </summary>
        private Timer _refreshTimer;

        /// <summary>
        /// Layer settings delay timer to filter quick ui changes (runs on Main Thread)
        /// </summary>
        private DispatcherTimer _delayTimer = new DispatcherTimer();
        private const int SettingsChangedDelay = 2;

        #endregion

        public LayerAutoUpdateHelper()
        {
            _delayTimer.Interval = new TimeSpan(0, 0, SettingsChangedDelay);
            _delayTimer.Tick += (s, e) =>
                                {
                                    _delayTimer.Stop();
                                    bool immediateUpdate = UpdateNow;
                                    UpdateNow = false;
                                    if (FilteredLayer != null)
                                    {
                                        Interval = (int)LayerExtensions.GetAutoUpdateInterval(FilteredLayer);
                                        if (IsEnabled)
                                        {
                                            StopRefreshTimer();
                                            if (immediateUpdate)
                                                UpdateLayer(FilteredLayer);

                                            if (Interval>0)
                                                StartRefreshTimer();
                                        }
                                    }
                                };
        }

        /// <summary>
        /// Flag indicating something requires an immediate update (perhaps the map extent has changed)
        /// </summary>
        public volatile bool UpdateNow = false;

        #region Property: ParentBehavior

        /// <summary>
        /// Behavior that owns this helper object
        /// </summary>
        public LayerAutoUpdateBehavior ParentBehavior
        {
            get { return _parentBehavior; }
            set
            {
                if (_parentBehavior != value)
                {
                    _parentBehavior = value;
                }
            }
        }
        private LayerAutoUpdateBehavior _parentBehavior = null;

        #endregion

        #region Cross-thread Layer refresh support

        /// <summary>
        /// Method that is called on the Main UI thread
        /// </summary>
        private void UpdateLayerUI(Layer layer)
        {
            // verify the layer has not been removed from the map already
            if (ParentBehavior == null || !ParentBehavior.MapContainsLayer(layer))
                return;

            if (layer != null && LayerAutoUpdateBehavior.IsLayerAutoUpdateCapable(layer))
            {
                try
                {
                    if (layer is FeatureLayer)
                    {
                        // We only need to make sure the client caching is off while we
                        // do this update, and then return it to the original state.
                        bool cachingDisabled = ((FeatureLayer) layer).DisableClientCaching;
                        if (!cachingDisabled)
                            ((FeatureLayer) layer).DisableClientCaching = true;

                        ((FeatureLayer) layer).Update();

                        if (!cachingDisabled)
                            ((FeatureLayer) layer).DisableClientCaching = false;
                    }
                    else if (layer is ArcGISDynamicMapServiceLayer)
                    {
                        // We only need to make sure the client caching is off while we
                        // do this update, and then return it to the original state.
                        bool cachingDisabled = ((ArcGISDynamicMapServiceLayer)layer).DisableClientCaching;
                        if (!cachingDisabled)
                            ((ArcGISDynamicMapServiceLayer)layer).DisableClientCaching = true;

                        ((ArcGISDynamicMapServiceLayer)layer).Refresh();

                        if (!cachingDisabled)
                            ((ArcGISDynamicMapServiceLayer)layer).DisableClientCaching = false;

                    }
                    else if (layer is ICustomGraphicsLayer)
                        ((ICustomGraphicsLayer)layer).Update();

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Layer Update failed.  " + ex);
                }

                _refreshRunning = false;
                // start the timer back up if the layer is still using auto updates
                if (LayerExtensions.GetAutoUpdateInterval(layer) > 0.0)
                    StartRefreshTimer();

            }
        }

        private void UpdateLayer(Layer layer)
        {
            if (FilteredLayer == null || !LayerAutoUpdateBehavior.IsLayerAutoUpdateCapable(layer))
                return;

           
            if (layer.Dispatcher.CheckAccess())
                UpdateLayerUI(layer);
            else
                layer.Dispatcher.BeginInvoke(new AutoUpdateRefreshDelegate(UpdateLayerUI), layer);
        }

        #endregion

        #region Nested type: AutoUpdateRefreshDelegate

        private delegate void AutoUpdateRefreshDelegate(Layer layer);

        #endregion

        #region Property: FilteredLayer

        /// <summary>
        /// Associated map layer that receives auto refresh 
        /// </summary>
        public Layer FilteredLayer { get; set; }

        #endregion

        #region Property: Interval

        private int _interval = (int)LayerExtensions.DefaultAutoUpdateInterval;

        /// <summary>
        /// Auto-refresh timer interval in milliseconds (default is 30000)
        /// </summary>
        public int Interval
        {
            get { return _interval; }
            set { _interval = value; }
        }

        #endregion

        #region Settings Delay Timer

        /// <summary>
        /// Tells the helper to check the layer settings and update the timer
        /// </summary>
        public void SettingsHaveChanged()
        {
            if (_delayTimer.IsEnabled)
                _delayTimer.Stop();

            _delayTimer.Start();
        }


        #endregion

        #region Property: IsEnabled

        /// <summary>
        /// Sets the enabled state of the refresh timer, and starts/stops
        /// the timer immediately.  Setting this to false will cause the 
        /// timer to stop and subsequent change notifications will have no 
        /// effect on the timer until IsEnabled is again set to true.  
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    if (_isEnabled)
                        StartRefreshTimer();
                    else
                        StopRefreshTimer();
                }
            }
        }
        private bool _isEnabled = false;

        #endregion

        #region Property: OriginalLayerClientCaching

        public bool OriginalLayerClientCaching
        {
            get { return _originalLayerClientCaching; }
            set
            {
                if (_originalLayerClientCaching != value)
                {
                    _originalLayerClientCaching = value;
                }
            }
        }
        private bool _originalLayerClientCaching = false;

        #endregion

        /// <summary>
        /// Starts the refresh timer.  Note that if the IsEnabled flag is false
        /// nothing will happen.  In that case, set the IsEnabled flag, and it 
        /// will cause the refresh timer to start (if the interval >= 1 second)
        /// </summary>
        public void StartRefreshTimer()
        {

            if (!IsEnabled || FilteredLayer == null || Interval < MinimumRefreshInterval)
                return;

            StopRefreshTimer();

            if (Interval >= MinimumRefreshInterval)
                _refreshTimer = new Timer(AutoUpdateTimerMethod, FilteredLayer, Interval, Interval);
        }

        public void StopRefreshTimer()
        {
            if (_refreshTimer != null)
            {
                _refreshTimer.Dispose();
                _refreshTimer = null;
            }
            _refreshRunning = false;
        }

        /// <summary>
        /// Stops the refresh timer and clears the layer
        /// </summary>
        /// <returns></returns>
        public Layer ResetRefresh()
        {
            StopRefreshTimer();
            var layer = FilteredLayer;
            FilteredLayer = null;
            IsEnabled = false;
            return layer;
        }

        /// <summary>
        /// Called by the timer when the interval elapsed
        /// </summary>
        /// <param name="stateInfo">FeatureLayer associated with this class</param>
        public void AutoUpdateTimerMethod(Object stateInfo)
        {
            if (_refreshRunning) return;

            // When the timer ticks, we stop it until the refresh is done
            StopRefreshTimer();
            // Prevent any straggling ticks from updating anything
            _refreshRunning = true;

            var layer = (Layer)stateInfo;

            if (layer.Dispatcher.CheckAccess())
                UpdateLayer(layer);
            else
                layer.Dispatcher.BeginInvoke(() => UpdateLayer(layer));
        }
    }
}
