/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class FindNearbyDialog : Control
    {
        internal ComboBox OperationalLayersComboBox;
        internal TextBox DistanceTextBox;
        internal ComboBox UnitsCombobox;
        internal Button FindButton;
        internal ActivityIndicator ActivityIndicator;

        public FindNearbyDialog()
        {
            DefaultStyleKey = typeof(FindNearbyDialog);
        }

        public override void OnApplyTemplate()
        {
            if (FindButton != null)
                FindButton.Click -= FindButton_Click;

            base.OnApplyTemplate();

            OperationalLayersComboBox = GetTemplateChild("OperationalLayersComboBox") as ComboBox;

            DistanceTextBox = GetTemplateChild("DistanceTextBox") as TextBox;

            UnitsCombobox = GetTemplateChild("UnitsCombobox") as ComboBox;

            FindButton = GetTemplateChild("FindButton") as Button;
            if(FindButton != null)
                FindButton.Click += FindButton_Click;

            ActivityIndicator = GetTemplateChild("ActivityIndicator") as ActivityIndicator;

            bindUI();
        }

        internal void StartBusyIndicator()
        {
            if (FindButton != null)
                FindButton.IsEnabled = false;

            if (ActivityIndicator != null)
            {
                ActivityIndicator.Visibility = System.Windows.Visibility.Visible;
                ActivityIndicator.StartProgressAnimation();
            }
        }

        internal void StopBusyIndicator()
        {
            if (FindButton != null)
                FindButton.IsEnabled = true;

            if (ActivityIndicator != null)
            {                
                ActivityIndicator.StopProgressAnimation();
                ActivityIndicator.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        void FindButton_Click(object sender, RoutedEventArgs e)
        {
            double distance;
            if (DistanceTextBox == null || OperationalLayersComboBox == null || UnitsCombobox == null)
                return;

            // Parse the Userinput in the locale of the UI Culture. It is also important to specify the number style as float otherwise
            // an entry such as 50.2 gets interpreted/converted to 502
            if (!double.TryParse(DistanceTextBox.Text, System.Globalization.NumberStyles.Any, CultureHelper.GetCurrentCulture(), out distance))
            {
                MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.InvalidDistance, ESRI.ArcGIS.Mapping.Controls.Resources.Strings.InvalidDistanceCaption, MessageBoxButton.OK);
                return;
            }

            StartBusyIndicator();

            OnFindNearbyExecuted(new FindNearbyEventArgs() {
                     Distance = distance,
                     SelectedLayer = ((LayerDisplay)OperationalLayersComboBox.SelectedItem).Layer,
                     LayerDisplayName = ((LayerDisplay)OperationalLayersComboBox.SelectedItem).DisplayName,
                     LinearUnit = ((DistanceParameter)UnitsCombobox.SelectedItem).Unit,
                     EventId = Guid.NewGuid().ToString("N"),
                });
        }

        void bindUI()
        {
            buildUnitsComboBox();

            bindOperationsLayerComboBox();

            bindSelectedLayer();
        }

        private void buildUnitsComboBox()
        {
            if (UnitsCombobox != null)
            {
                UnitsCombobox.Items.Clear();
                UnitsCombobox.Items.Add(new DistanceParameter() { Unit = LinearUnit.Miles, DisplayName = ESRI.ArcGIS.Mapping.Controls.Resources.Strings.Miles });
                UnitsCombobox.Items.Add(new DistanceParameter() { Unit = LinearUnit.Meters, DisplayName = ESRI.ArcGIS.Mapping.Controls.Resources.Strings.Meters });
                UnitsCombobox.Items.Add(new DistanceParameter() { Unit = LinearUnit.Kilometers, DisplayName = ESRI.ArcGIS.Mapping.Controls.Resources.Strings.Kilometers });
                UnitsCombobox.SelectedIndex = 0;
            }
        }

        private void bindOperationsLayerComboBox()
        {
            if (OperationalLayersComboBox != null)
            {
                if (LayersInMap != null)
                {
                    List<LayerDisplay> layerDisplay = new List<LayerDisplay>();
                    int i = 0;
                    foreach (Layer layer in LayersInMap)
                    {
                        GraphicsLayer graphicsLayer = layer as GraphicsLayer;
                        if (graphicsLayer != null)
                        {
                            layerDisplay.Add(new LayerDisplay() { 
                                 Layer = graphicsLayer,
                                 DisplayName = graphicsLayer.GetValue(ESRI.ArcGIS.Client.Extensibility.MapApplication.LayerNameProperty) as string ?? 
                                 graphicsLayer.ID ?? "Layer " + i,
                            });
                        }
                        i++;
                    }
                    OperationalLayersComboBox.ItemsSource = layerDisplay;
                }
            }
        }               

        #region SelectedLayer
        /// <summary>
        /// 
        /// </summary>
        public Layer SelectedLayer
        {
            get { return GetValue(SelectedLayerProperty) as Layer; }
            set { SetValue(SelectedLayerProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectedLayer dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedLayerProperty =
            DependencyProperty.Register(
                "SelectedLayer",
                typeof(Layer),
                typeof(FindNearbyDialog),
                new PropertyMetadata(null, OnSelectedLayerPropertyChanged));

        /// <summary>
        /// SelectedLayerProperty property changed handler.
        /// </summary>
        /// <param name="d">FindNearbyDialog that changed its SelectedLayer.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnSelectedLayerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FindNearbyDialog source = d as FindNearbyDialog;
            source.OnSelectedLayerChanged();
        }
        #endregion

        private void OnSelectedLayerChanged()
        {
            bindSelectedLayer();
        }

        private void bindSelectedLayer()
        {
            if (OperationalLayersComboBox != null)
            {
                if (SelectedLayer != null)
                {
                    GraphicsLayer graphicsLayer = SelectedLayer as GraphicsLayer;
                    if (graphicsLayer != null)
                        OperationalLayersComboBox.SelectedItem = OperationalLayersComboBox.Items.Cast<LayerDisplay>().FirstOrDefault<LayerDisplay>(l => l.Layer == graphicsLayer);
                }
            }
        }

        #region LayersInMap
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<Layer> LayersInMap
        {
            get { return GetValue(LayersInMapProperty) as IEnumerable<Layer>; }
            set { SetValue(LayersInMapProperty, value); }
        }

        /// <summary>
        /// Identifies the LayersInMap dependency property.
        /// </summary>
        public static readonly DependencyProperty LayersInMapProperty =
            DependencyProperty.Register(
                "LayersInMap",
                typeof(IEnumerable<Layer>),
                typeof(FindNearbyDialog),
                new PropertyMetadata(null, OnLayersInMapPropertyChanged));

        /// <summary>
        /// LayersInMapProperty property changed handler.
        /// </summary>
        /// <param name="d">FindNearbyDialog that changed its LayersInMap.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnLayersInMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FindNearbyDialog source = d as FindNearbyDialog;
            source.OnLayersInMapChanged();
        }
        #endregion

        private void OnLayersInMapChanged()
        {
            bindOperationsLayerComboBox();
        }

        protected virtual void OnFindNearbyExecuted(FindNearbyEventArgs args)
        {
            if (FindNearbyExecuted != null)
                FindNearbyExecuted(this, args);
        }

        public event EventHandler<FindNearbyEventArgs> FindNearbyExecuted;
    }    

    public enum LinearUnit
    {
        Miles,
        Meters,
        Kilometers,
    }

    public class FindNearbyEventArgs : EventArgs
    {
        public GraphicsLayer SelectedLayer { get; set; }
        public string LayerDisplayName { get; set; }
        public LinearUnit LinearUnit { get; set; }
        public double Distance { get; set; }
        public string EventId { get; set; }
    }

    public class DistanceParameter
    {
        public LinearUnit Unit { get; set; }
        public string DisplayName { get; set; }
    }

    public class LayerDisplay
    {
        public GraphicsLayer Layer { get; set; }
        public string DisplayName { get; set; }
    }
}
