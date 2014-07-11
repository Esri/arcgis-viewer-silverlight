/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Tasks;

namespace SearchTool
{
    /// <summary>
    /// ViewModel to support selecting sub-layers within a map or feature service
    /// </summary>
    public class ChooseSublayersViewModel : DependencyObject, INotifyPropertyChanged
    {
        public ChooseSublayersViewModel()
        {
            // Instantiate layers collections and hook to changed event
            SelectedLayers = new ObservableCollection<FeatureLayer>();
            AvailableLayers = new ObservableCollection<FeatureLayer>();
            SelectedLayers.CollectionChanged += SelectedLayers_CollectionChanged;
        }

        /// <summary>
        /// Gets the collection of available layers
        /// </summary>
        public ObservableCollection<FeatureLayer> AvailableLayers { get; private set; }

        /// <summary>
        /// Gets the collection of layers that are currently selected
        /// </summary>
        public ObservableCollection<FeatureLayer> SelectedLayers { get; private set; }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Service"/> property
        /// </summary>
        public static DependencyProperty ServiceProperty = DependencyProperty.Register(
            "Service", typeof(ArcGISService), typeof(ChooseSublayersViewModel), null);

        /// <summary>
        /// Gets or sets the service containing the available layers
        /// </summary>
        public ArcGISService Service 
        {
            get { return GetValue(ServiceProperty) as ArcGISService; }
            set
            {
                if (Service != value)
                {
                    SetValue(ServiceProperty, value);

                    // Initialize the layers collections based on the new service
                    initializeLayers();

                    OnPropertyChanged("Service");
                }
            } 
        }

        private string _proxyUrl = null;
        /// <summary>
        /// Gets or sets the proxy URL to use for with the layer's service
        /// </summary>
        public string ProxyUrl
        {
            get { return _proxyUrl; }
            set
            {
                if (_proxyUrl != value)
                {
                    _proxyUrl = value;
                    OnPropertyChanged("ProxyUrl");
                }
            }
        }

        /// <summary>
        /// Raised when a property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        // Raises the PropertyChanged event
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private ThrottleTimer _layerOrderThrottler; // Throttles re-ordering of layers
        private void SelectedLayers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Initialize the re-ordering throttler, if necessary 
            if (_layerOrderThrottler == null)
                _layerOrderThrottler = new ThrottleTimer(10, () => { orderSelectedLayers(); });

            // Invoke the throttler.  When it executes, it will re-order the selected layers to match 
            // their order in the available layers collectino
            _layerOrderThrottler.Invoke();
        }

        // Initializes the available layers based on the parent service and clears the selected layers
        private void initializeLayers()
        {
            AvailableLayers.Clear();
            SelectedLayers.Clear();

            // Make sure the service is a type that contains sublayers
            if (!(Service is FeatureService) && !(Service is MapService))
                return; // Other service types don't have sublayers

            // Loop through each sublayer in the service
            dynamic serviceAsDynamic = Service;
            foreach (FeatureLayerDescription layer in serviceAsDynamic.Layers)
            {
                // Create a feature layer for the sublayer
                string id = Guid.NewGuid().ToString("N");
                FeatureLayer fLayer = new FeatureLayer()
                {
                    Url = string.Format("{0}/{1}", Service.Url, layer.Id),
                    OutFields = new OutFields() { "*" },
                    ProxyUrl = ProxyUrl,
                    ID = id
                };

                // Initialize the layer's name
                string name = !string.IsNullOrEmpty(layer.Name) ? layer.Name : id;
                MapApplication.SetLayerName(fLayer, name);

                // Add the layer to the set of available layers
                AvailableLayers.Add(fLayer);
            }
        }

        // Re-orders the selected layers
        private void orderSelectedLayers()
        {
            List<FeatureLayer> preSortLayers = SelectedLayers.ToList();
            SelectedLayers.Clear();
            foreach (FeatureLayer fLayer in AvailableLayers)
            {
                if (preSortLayers.Contains(fLayer))
                    SelectedLayers.Add(fLayer);
            }

            OnPropertyChanged("SelectedLayers");
        }
    }
}
