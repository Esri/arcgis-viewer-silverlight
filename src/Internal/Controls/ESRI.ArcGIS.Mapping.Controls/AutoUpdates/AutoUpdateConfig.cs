/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Mapping.Behaviors;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class AutoUpdateConfig : LayerConfigControl
    {
        #region Fields

        private const string PART_POLLFORUPDATES = "PollForUpdates";
        private const string PART_POLLINTERVAL = "PollInterval";
        private const string PART_INTERVALTYPE = "IntervalType";
        private const string PART_UPDATEONEXTENTCHANGED = "UpdateOnExtentChanged";
        internal ComboBox IntervalType;
        internal CheckBox PollForUpdates;
        internal NumericUpDown PollInterval;
        internal CheckBox UpdateOnExtentChanged;
        private bool _uiChanging = false;

        #endregion

        public AutoUpdateConfig()
        {
            DefaultStyleKey = typeof (AutoUpdateConfig);
        }

        public override void OnApplyTemplate()
        {
            if (PollForUpdates != null)
            {
                PollForUpdates.Checked -= PollForUpdates_Checked;
                PollForUpdates.Unchecked -= PollForUpdates_UnChecked;
            }

            if (PollInterval != null)
                PollInterval.ValueChanged -= PollInterval_ValueChanged;

            if (IntervalType != null)
                IntervalType.SelectionChanged -= IntervalType_SelectionChanged;

            if (UpdateOnExtentChanged != null)
            {
                UpdateOnExtentChanged.Checked -= UpdateOnExtentChanged_Checked;
                UpdateOnExtentChanged.Unchecked -= UpdateOnExtentChanged_Unchecked;
            }

            base.OnApplyTemplate();

            PollForUpdates = GetTemplateChild(PART_POLLFORUPDATES) as CheckBox;

            PollInterval = GetTemplateChild(PART_POLLINTERVAL) as NumericUpDown;

            IntervalType = GetTemplateChild(PART_INTERVALTYPE) as ComboBox;

            UpdateOnExtentChanged = GetTemplateChild(PART_UPDATEONEXTENTCHANGED) as CheckBox;

            bindUIToLayer();

            if (PollForUpdates != null)
            {
                PollForUpdates.Checked += PollForUpdates_Checked;
                PollForUpdates.Unchecked += PollForUpdates_UnChecked;
            }

            if (PollInterval != null)
                PollInterval.ValueChanged += PollInterval_ValueChanged;

            if (IntervalType != null)
                IntervalType.SelectionChanged += IntervalType_SelectionChanged;

            if (UpdateOnExtentChanged != null)
            {
                UpdateOnExtentChanged.Checked += UpdateOnExtentChanged_Checked;
                UpdateOnExtentChanged.Unchecked += UpdateOnExtentChanged_Unchecked;
            }
        }

        protected override void OnLayerChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            bindUIToLayer();
        }

        private void bindUIToLayer()
        {
            if (!(LayerAutoUpdateBehavior.IsLayerAutoUpdateCapable(Layer)))
            {
                IsEnabled = false;
                return;
            }
            IsEnabled = true;

            // Get the layer settings from LayerExtensions attached properties
            double interval = Math.Max(0, LayerExtensions.GetAutoUpdateInterval(Layer));
            bool autoUpdatesEnabled = (interval > 0.0d);

            if (PollForUpdates != null)
                PollForUpdates.IsChecked = autoUpdatesEnabled;

            if (PollInterval != null && IntervalType != null)
            {
                _uiChanging = true;
                try
                {
                    if (autoUpdatesEnabled)
                    {
                        // if the interval > 60000, we need to display the interval in minutes
                        if (interval >= 60000d)
                        {
                            IntervalType.SelectedIndex = 1; // minutes
                            PollInterval.Value = ((int)(interval/60000d));
                        }
                        else
                        {
                            IntervalType.SelectedIndex = 0;
                            PollInterval.Value = ((int)(interval / 1000d));
                        }
                    }
                    else
                    {
                        IntervalType.SelectedIndex = 0;
                        PollInterval.Value = 0;
                    }
                }
                finally
                {
                    _uiChanging = false;
                }
            }

            if (UpdateOnExtentChanged != null)
                UpdateOnExtentChanged.IsChecked = LayerExtensions.GetAutoUpdateOnExtentChanged(Layer);
        }

        private void UpdateOnExtentChanged_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!LayerAutoUpdateBehavior.IsLayerAutoUpdateCapable(Layer))
                return;

            LayerExtensions.SetAutoUpdateOnExtentChanged(Layer, false);
        }

        private void UpdateOnExtentChanged_Checked(object sender, RoutedEventArgs e)
        {
            if (!LayerAutoUpdateBehavior.IsLayerAutoUpdateCapable(Layer))
                return;

            LayerExtensions.SetAutoUpdateOnExtentChanged(Layer, true);
        }

        private void IntervalType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_uiChanging || PollInterval == null || IntervalType == null
                || !(LayerAutoUpdateBehavior.IsLayerAutoUpdateCapable(Layer)))
                return;

            double interval = GetIntervalFromUi();
            LayerExtensions.SetAutoUpdateInterval(Layer, interval);
        }

        private void PollInterval_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_uiChanging || PollInterval == null || IntervalType == null
                || !LayerAutoUpdateBehavior.IsLayerAutoUpdateCapable(Layer))
                return;

            double interval = GetIntervalFromUi();
            LayerExtensions.SetAutoUpdateInterval(Layer, interval);
        }

        private void PollForUpdates_Checked(object sender, RoutedEventArgs e)
        {
            if (!LayerAutoUpdateBehavior.IsLayerAutoUpdateCapable(Layer))
                return;

            // When the AutoUpdates checkbox is checked, it sets the interval to 30 seconds as default
            if (PollInterval != null)
            {
                _uiChanging = true;
                try
                {
                    PollInterval.Value = 30d;
                }
                finally
                {
                    _uiChanging = false;
                }
            }
            LayerExtensions.SetAutoUpdateInterval(Layer, LayerExtensions.DefaultAutoUpdateInterval);
        }

        private void PollForUpdates_UnChecked(object sender, RoutedEventArgs e)
        {
            if (!LayerAutoUpdateBehavior.IsLayerAutoUpdateCapable(Layer))
                return;

            // Auto updates are turned on/off by setting the interval value.  A value
            // less than or equal to zero indicates auto updates are off.
            if (PollInterval != null)
            {
                _uiChanging = true;
                try
                {
                    PollInterval.Value = 0d;
                }
                finally 
                {
                    _uiChanging = false;
                }                
            }
            LayerExtensions.SetAutoUpdateInterval(Layer, 0.0);
        }

        /// <summary>
        /// Returns the selected interval converted to milliseconds.
        /// </summary>
        private double GetIntervalFromUi()
        {
            if (!LayerAutoUpdateBehavior.IsLayerAutoUpdateCapable(Layer) || PollInterval == null || IntervalType == null)
                return 0.0;

            double interval = 0.0;
            try
            {
                switch (IntervalType.SelectedIndex)
                {
                    case 0:
                        interval = PollInterval.Value*1000;
                        break;
                    case 1:
                        interval = PollInterval.Value*60000;
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return interval;
        }
    }
}
