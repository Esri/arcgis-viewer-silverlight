/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;
using System.ComponentModel.Composition;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(FrameworkElement))]
	[DisplayName("BaseMapGalleryDisplayName")]
	[Category("CategoryMap")]
	[Description("BaseMapGalleryDescription")]
    [TemplatePart(Name = PART_BASEMAPSLIST, Type = (typeof(ListBox)))]
    public class BaseMapGallery : Control, ISupportsConfiguration
    {
        private const string PART_BASEMAPSLIST = "BaseMapsList";
        internal ListBox BaseMapsList { get; private set; }
        
        private const string PART_CONFIGUREBUTTON = "ConfigureButton";
        internal Button ConfigureButton { get; private set; }

        public BaseMapGallery()
        {
            DefaultStyleKey = typeof(BaseMapGallery);
            BaseMaps = ConfigurationStoreProvider.ReadStoreFromEmbeddedFile().BaseMaps;
            CurrentBaseMapName = getCurrentBaseMapName();
            View.ViewChanged += View_ViewChanged;
            try
            {
                if (MapApplication.Current != null)
                    IsEditMode = MapApplication.Current.IsEditMode && !DisallowConfigureMode;
            }
            catch {
                // In SharePoint, there is not map application context
                DisallowConfigureMode = true;
            }
        }

        #region BaseMaps
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<BaseMapInfo> BaseMaps
        {
            get { return GetValue(BaseMapsProperty) as ObservableCollection<BaseMapInfo>; }
            set { SetValue(BaseMapsProperty, value); }
        }

        /// <summary>
        /// Identifies the BaseMaps dependency property.
        /// </summary>
        public static readonly DependencyProperty BaseMapsProperty =
            DependencyProperty.Register(
                "BaseMaps",
                typeof(ObservableCollection<BaseMapInfo>),
                typeof(BaseMapGallery),
                new PropertyMetadata(null, OnBaseMapsPropertyChanged));

        /// <summary>
        /// BaseMapsProperty property changed handler.
        /// </summary>
        /// <param name="d">BaseMapGallery that changed its BaseMaps.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnBaseMapsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BaseMapGallery source = d as BaseMapGallery;
            ObservableCollection<BaseMapInfo> oldCollection = e.OldValue as ObservableCollection<BaseMapInfo>;
            if (oldCollection != null)
                oldCollection.CollectionChanged -= source.BaseMaps_CollectionChanged;
            source.OnBaseMapsChanged();
        }
        #endregion 

        private void OnBaseMapsChanged()
        {
            if (BaseMaps != null)
            {
                BaseMaps.CollectionChanged -= BaseMaps_CollectionChanged;
                BaseMaps.CollectionChanged += BaseMaps_CollectionChanged;

				// Listen for property changes in order to handle changes to the
				// currently selected basemap
                foreach (BaseMapInfo basemapInfo in BaseMaps)
                    basemapInfo.PropertyChanged += BasemapInfo_PropertyChanged;
            }

            if (BaseMapsList != null)
                BaseMapsList.ItemsSource = FilterBingBasemaps();
        }

        private void BaseMaps_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (BaseMapsList != null)
                BaseMapsList.ItemsSource = FilterBingBasemaps();

            // Stop listening for changes on removed basemaps
            if (e.OldItems != null)
            {
                foreach (BaseMapInfo basemapInfo in e.OldItems)
                    basemapInfo.PropertyChanged -= BasemapInfo_PropertyChanged;
            }

            // Listen for changes on added basemaps
            if (e.NewItems != null)
            {
                foreach (BaseMapInfo basemapInfo in e.NewItems)
                    basemapInfo.PropertyChanged += BasemapInfo_PropertyChanged;
            }
        }

        // Basemap properties which, when changed on the current basemap, cause the basemap selection to be cleared
        private string[] resetSelectionProperties = new string[] { "Url", "BaseMapType", "UseProxy", "Name" };
        private void BasemapInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Clear the current selection if a property that affects display of the current basemap is changed
            if (resetSelectionProperties.Contains(e.PropertyName) && BaseMapsList != null &&
            sender == BaseMapsList.SelectedItem)
                BaseMapsList.SelectedItem = null;
        }

        private string CurrentBaseMapName { get; set;}           
        
        public override void OnApplyTemplate()
        {
            if (ConfigureButton != null)
                ConfigureButton.Click -= ConfigureButton_Click;

            if (BaseMapsList != null)
                BaseMapsList.SelectionChanged -= BaseMapList_SelectionChanged;

            base.OnApplyTemplate();

            BaseMapsList = GetTemplateChild(PART_BASEMAPSLIST) as ListBox;
            BaseMaps = BaseMaps ?? getBaseMapsFromView();
            if (BaseMapsList != null)
            {
                if (BaseMaps != null)
                {
                    ObservableCollection<BaseMapInfo> filteredBasemaps = FilterBingBasemaps();
                    BaseMapsList.ItemsSource = filteredBasemaps;
                    if (!string.IsNullOrEmpty(CurrentBaseMapName))
                    {
                        BaseMapInfo baseMapInfo = filteredBasemaps.FirstOrDefault<BaseMapInfo>(b => b.Name == CurrentBaseMapName);
                        if (baseMapInfo != null)     
                            BaseMapsList.SelectedItem = baseMapInfo;                         
                    }                    
                }
                BaseMapsList.SelectionChanged += BaseMapList_SelectionChanged;
            }

            ConfigureButton = GetTemplateChild(PART_CONFIGUREBUTTON) as Button;
            if(ConfigureButton != null)
                ConfigureButton.Click += ConfigureButton_Click;

            if (InitCompleted != null)
                InitCompleted(this, EventArgs.Empty);
        }

        void ConfigureButton_Click(object sender, RoutedEventArgs e)
        {
            BaseMapsConfigControl configControl = new BaseMapsConfigControl()
            { 
                Description = "",
                BaseMaps = BaseMaps,
                Width = 450,
                ScrollViewerHeight = 295,
                Margin = new Thickness(15, 10, 15, 20),
				OkButtonText = ESRI.ArcGIS.Mapping.Controls.Resources.Strings.Close,
            };
            Binding b = new Binding("BingMapsAppId") { Source = ViewerApplicationControl.Instance.ViewerApplication };
            b.Mode = BindingMode.OneWay;
            BindingOperations.SetBinding(configControl, BaseMapsConfigControl.BingMapsAppIDProperty, b);
			configControl.OkButtonCommand = new DelegateCommand((args) =>
            {
                MapApplication.Current.HideWindow(configControl);
            });
			MapApplication.Current.ShowWindow(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ConfigureBasemaps, 
                configControl, true, null, null, WindowType.DesignTimeFloating);
        }

        internal event EventHandler InitCompleted;

        internal void SelectBaseMap(BaseMapInfo baseMap)
        {
            if (baseMap == null)
                throw new ArgumentNullException("baseMap");

            if (BaseMapsList != null)
                BaseMapsList.SelectedItem = baseMap;
        }

        private void BaseMapList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BaseMapInfo baseMapInfo = BaseMapsList.SelectedItem as BaseMapInfo;
            if (baseMapInfo == null)
                return;            
            OnBaseMapChanged(new BaseMapChangedEventArgs() {  BaseMapInfo = baseMapInfo});
        }
        
        protected void OnBaseMapChanged(BaseMapChangedEventArgs args)
        {
            if (View.Instance != null)
                View.Instance.ChangeBaseMap(args.BaseMapInfo);

            CurrentBaseMapName = args.BaseMapInfo.Name;
            if (BaseMapChanged != null)
                BaseMapChanged(this, args);
        }

        public event EventHandler<BaseMapChangedEventArgs> BaseMapChanged;

        private static ObservableCollection<BaseMapInfo> getBaseMapsFromView()
        {
            ObservableCollection<BaseMapInfo> baseMaps = null;
            if (View.Instance != null)
            {
                if (View.Instance.ConfigurationStore != null)
                    baseMaps = View.Instance.ConfigurationStore.BaseMaps;
            }
            return baseMaps;
        }

        private string getCurrentBaseMapName()
        {
            ObservableCollection<BaseMapInfo> baseMaps = getBaseMapsFromView();
            if (View.Instance == null || View.Instance.Map == null || View.Instance.Map.Layers.Count < 1 || baseMaps == null)
                return null;
            TileLayer bingLayer = View.Instance.Map.Layers[0] as TileLayer;
            if (bingLayer != null)
            {
                switch (bingLayer.LayerStyle)
                {
                    case TileLayer.LayerType.Road:
                        return "Roads";
                    case TileLayer.LayerType.Aerial:
                        return "Aerial";
                    case TileLayer.LayerType.AerialWithLabels:
                        return "Hybrid";
                }
            }
            ArcGISTiledMapServiceLayer agsLayer = View.Instance.Map.Layers[0] as ArcGISTiledMapServiceLayer;
            if (agsLayer != null)
            {
                if (baseMaps != null)
                {
                    BaseMapInfo baseMap = baseMaps.FirstOrDefault<BaseMapInfo>(b => b.Url == agsLayer.Url);
                    if (baseMap != null)
                        return baseMap.Name;
                }
            }
            return null;
        } 

        public void Configure()
        {
            ConfigureButton_Click(null, null);
        }

        public void LoadConfiguration(string configData)
        {
            if (string.IsNullOrEmpty(configData))
                return;

            try
            {
                XDocument xDoc = XDocument.Parse(configData);
                ObservableCollection<BaseMapInfo> baseMaps = new ObservableCollection<BaseMapInfo>();
                XElement rootNode = xDoc.Element("BaseMaps");
                if (rootNode != null)
                {
                    IEnumerable<XElement> baseMapNodes = from elem in rootNode.Elements("BaseMapInfo") select elem;
                    foreach (XElement baseMapNode in baseMapNodes)
                    {
                        BaseMapInfo baseMapInfo = new BaseMapInfo();
                        XElement DisplayName = baseMapNode.Element("DisplayName");
                        if (DisplayName != null)
                            baseMapInfo.DisplayName = DisplayName.Value;
                        XElement Name = baseMapNode.Element("Name");
                        if (Name != null)
                            baseMapInfo.Name = Name.Value;
                        XElement ThumbnailImage = baseMapNode.Element("ThumbnailImage");
                        if (ThumbnailImage != null)
                            baseMapInfo.ThumbnailImage = ThumbnailImage.Value;
                        XElement BaseMapType = baseMapNode.Element("BaseMapType");
                        if (BaseMapType != null)
                        {
                            BaseMapType outValue;
                            if (Enum.TryParse<BaseMapType>(BaseMapType.Value, out outValue))
                                baseMapInfo.BaseMapType = outValue;
                        }
                        XElement Url = baseMapNode.Element("Url");
                        if (Url != null)
                            baseMapInfo.Url = Url.Value;
                        baseMaps.Add(baseMapInfo);

                        // Get whether proxy is used
                        XElement UseProxy = baseMapNode.Element("UseProxy");
                        if (UseProxy != null)
                        {
                            bool useProxy;
                            bool.TryParse(UseProxy.Value, out useProxy);
                            baseMapInfo.UseProxy = useProxy;
                        }
                    }
                    BaseMaps = baseMaps;
                }
               
            }
            catch { }
        }

        public string SaveConfiguration()
        {
            if (BaseMaps == null)
                return null;

            XElement rootElem = new XElement("BaseMaps");
            XDocument xDoc = new XDocument(rootElem);
            foreach (BaseMapInfo baseMapInfo in BaseMaps)
            {
                XElement baseMapInfoElement = new XElement("BaseMapInfo");
                rootElem.Add(baseMapInfoElement);
                baseMapInfoElement.Add(new XElement("DisplayName", baseMapInfo.DisplayName));
                baseMapInfoElement.Add(new XElement("Name", baseMapInfo.Name));
                baseMapInfoElement.Add(new XElement("ThumbnailImage", baseMapInfo.ThumbnailImage));
                baseMapInfoElement.Add(new XElement("BaseMapType", baseMapInfo.BaseMapType));
                baseMapInfoElement.Add(new XElement("Url", baseMapInfo.Url));

                // Save whether proxy is used
                baseMapInfoElement.Add(new XElement("UseProxy", baseMapInfo.UseProxy));
            }
            return xDoc.ToString(SaveOptions.OmitDuplicateNamespaces);
        }

        public bool IsEditMode { get; private set; }

        public bool DisallowConfigureMode { get; set; }

        public string BingMapsAppId { get; set; }

        ObservableCollection<BaseMapInfo> FilterBingBasemaps()
        {
            string bingMapsAppId = BingMapsAppId;
            if (string.IsNullOrWhiteSpace(bingMapsAppId))
            {
                if (View.Instance != null)
                {
                    View.Instance.ConfigurationStoreChanged -= View_ConfigurationStoreChanged;
                    View.Instance.ConfigurationStoreChanged += View_ConfigurationStoreChanged;
                }

                if (View.Instance != null && View.Instance.ConfigurationStore != null)
                {
                    View.Instance.ConfigurationStore.PropertyChanged -= ConfigurationStore_PropertyChanged;
                    View.Instance.ConfigurationStore.PropertyChanged += ConfigurationStore_PropertyChanged;
                    bingMapsAppId = View.Instance.ConfigurationStore.BingMapsAppId;
                }
            }

            ObservableCollection<BaseMapInfo> filtered = new ObservableCollection<BaseMapInfo>();
            foreach (BaseMapInfo info in BaseMaps)
            {
                if (string.IsNullOrWhiteSpace(bingMapsAppId))
                {
                    if (info.BaseMapType != BaseMapType.BingMaps)
                        filtered.Add(info);
                }
                info.PropertyChanged -= info_PropertyChanged;
                info.PropertyChanged += info_PropertyChanged;
            }
            if (string.IsNullOrWhiteSpace(bingMapsAppId))
                return filtered;
            
            return BaseMaps;
        }

        void info_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BaseMapType")
            {
                OnBaseMapsChanged();
            }
        }

        void View_ViewChanged(object sender, EventArgs e)
        {
            OnBaseMapsChanged();
        }

        void View_ConfigurationStoreChanged(object sender, EventArgs e)
        {
            OnBaseMapsChanged();
        }

        void ConfigurationStore_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BingMapsAppId")
                OnBaseMapsChanged();
        }
    }

    public class BaseMapChangedEventArgs : EventArgs
    {
        public BaseMapInfo BaseMapInfo { get; set; }
    }

    public class ImageUrlToBitmapSourceConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string url = value as string;
            if (string.IsNullOrEmpty(url))
                return value;
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return new BitmapImage(new Uri(url, UriKind.Absolute));
            else
                return new BitmapImage(new Uri(url, UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
