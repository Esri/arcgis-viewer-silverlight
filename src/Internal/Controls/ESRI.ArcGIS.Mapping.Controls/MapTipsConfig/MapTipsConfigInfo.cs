/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.Core;
using System.ComponentModel;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class MapTipsConfigInfo : INotifyPropertyChanged
    {
        public MapTipsConfigInfo()
        {
            PopUpsOnClick = false;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        void onPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        Collection<LayerInformation> layers;
        public Collection<LayerInformation> Layers
        {
            get { return layers; }
            set
            {
                layers = value; 
                onPropertyChanged("Layers");
                if (layers != null)
                {
                    foreach (LayerInformation layer in layers)
                    {
                        layer.PropertyChanged += layer_PropertyChanged;
                    }
                }
            }
        }

        void layer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            onPropertyChanged(e.PropertyName);
        }

        private LayerInformation selectedItem;

        public LayerInformation SelectedItem
        {
            get { return selectedItem; }
            set { selectedItem = value; onPropertyChanged("SelectedItem"); }
        }

        private bool layerSelectionVisibility;

        public bool LayerSelectionVisibility
        {
            get { return layerSelectionVisibility; }
            set { layerSelectionVisibility = value; onPropertyChanged("LayerSelectionVisibility"); }
        }

        private bool? popUpsOnClick;

        public bool? PopUpsOnClick
        {
            get { return popUpsOnClick; }
            set { popUpsOnClick = value; onPropertyChanged("PopUpsOnClick"); }
        }

        bool supportsOnClick;
        public bool SupportsOnClick
        {
            get { return supportsOnClick; }
            set { supportsOnClick = value; onPropertyChanged("SupportsOnClick"); }
        }

		bool isPopupEnabled;
		public bool IsPopupEnabled
		{
			get { return isPopupEnabled; }
			set { isPopupEnabled = value; onPropertyChanged("IsPopupEnabled"); }
		}
		
		private Layer layer;

        public Layer Layer
        {
            get { return layer; }
            set { layer = value; onPropertyChanged("Layer"); }
        }

        private bool fromWebMap;

        public bool FromWebMap
        {
            get { return fromWebMap; }
            set { fromWebMap = value;  onPropertyChanged("FromWebMap"); }
        }

        private Visibility webMapPopupVisibility;

        public Visibility WebMapPopupVisibility
        {
            get { return webMapPopupVisibility; }
            set { webMapPopupVisibility = value;  onPropertyChanged("WebMapPopupVisibility"); }
        }
        
        
    }

}
