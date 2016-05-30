/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Builder.Common;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Core;
using System.Xml.Linq;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.WebMap;
using ESRI.ArcGIS.Client.Portal;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class BuilderApplication : ViewerApplication, IMapApplication
    {
        static BuilderApplication()
        {
            Instance = new BuilderApplication();
        }
        public BuilderApplication()
        {
            CatalogScreenVisibility = Visibility.Visible;
            BuilderScreenVisibility = Visibility.Collapsed;
            NewappScreenVisibility = Visibility.Collapsed;
            SitesCatalogVisibility = Visibility.Collapsed;
            WelcomeVisibility = Visibility.Collapsed;
            SettingsPageVisibility = Visibility.Collapsed;
            AllExtensions = new ObservableCollection<Extension>();
            WindowManager = new WindowManager();
            Culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            urls = new ApplicationUrls();
            Urls.ProxyUrl = string.Empty;
            BaseUrl = Application.Current.Host.Source.ToString();
        }

        /// <summary>
        /// This event handler is called whenever a change is made to the Sites observable collection. It will
        /// determine which sections of the catalog screen should be visible or collapsed amongst the welcome
        /// message and the list of created sites.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event arguments.</param>
        public void Sites_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshCatalogVisibility();
        }

        public void RefreshCatalogVisibility()
        {
            // If the list of sites has been determined and there is at least one site, then the sites catalog
            // section should be visible, otherwise collapsed.
            SitesCatalogVisibility = (Sites != null && Sites.Count > 0) ? Visibility.Visible : Visibility.Collapsed;

            // Determination of whether to display the welcome message is a bit complicated so this method is
            // called which takes into consideration user local storage settings and how many sites exist.
            WelcomeVisibility = GetWelcomeVisibility();
        }

        /// <summary>
        /// Determines whether or not display the welcome message portion of the user interface.
        /// </summary>
        /// <returns>The proper visibility setting for this UI section.</returns>
        private Visibility GetWelcomeVisibility()
        {
            // If there are sites, then collapse the welcome message portion of the UI
            if (Sites != null && Sites.Count > 0)
                return Visibility.Collapsed;

            // If there are NO sites, then make the welcome message portion of the UI visible
            return Visibility.Visible;
        }

        public static BuilderApplication Instance { get; private set; }
        public static void InitFromXml(string xml)
        {
            Instance.FromXml(xml);
        }

        public override string ToXml()
        {
            XDocument doc = ToXDocument();
            doc.Root.Name = "Builder";
            doc.Declaration = new XDeclaration("1.0", "utf-8", "yes");
            return doc.ToString();
        }

        public ObservableCollection<Extension> AllExtensions { get; set; }
        public string ExtensionsRepositoryBaseUrl { get; set; }

        public Templates Templates { get; set; }

        #region ConfigurationStore
        private ConfigurationStore _configurationStore;

        public ConfigurationStore ConfigurationStore
        {
            get { return _configurationStore; }
            set
            {
                if (_configurationStore != value)
                {
                    _configurationStore = value;
                    RaisePropertyChanged("ConfigurationStore");
                }
            }
        }
        #endregion

        public ObservableCollection<Site> Sites { get; set; }

        #region CurrentSite
        private Site _currentSite;

        public Site CurrentSite
        {
            get { return _currentSite; }
            set
            {
                if (_currentSite != value)
                {
                    _currentSite = value;
                    RaisePropertyChanged("CurrentSite");
                }
            }
        }
        #endregion

        private Visibility _sitesCatalogVisibility = Visibility.Collapsed;
        public Visibility SitesCatalogVisibility
        {
            get
            {
                return _sitesCatalogVisibility;
            }
            set
            {
                if (_sitesCatalogVisibility != value)
                {
                    _sitesCatalogVisibility = value;
                    RaisePropertyChanged("SitesCatalogVisibility");
                }
            }
        }

        private Visibility _welcomeVisibility = Visibility.Collapsed;
        public Visibility WelcomeVisibility
        {
            get
            {
                return _welcomeVisibility;
            }
            set
            {
                if (_welcomeVisibility != value)
                {
                    _welcomeVisibility = value;
                    RaisePropertyChanged("WelcomeVisibility");
                }
            }
        }

        private Visibility _catalogScreenVisibility = Visibility.Visible;
        public Visibility CatalogScreenVisibility
        {
            get
            {
                return _catalogScreenVisibility;
            }
            set
            {
                if (_catalogScreenVisibility != value)
                {
                    _catalogScreenVisibility = value;
                    RaisePropertyChanged("CatalogScreenVisibility");
                }
            }
        }

        private UIElement _loadingOverlay;
        /// <summary>
        /// Element to display on top of Viewer preview during Viewer initialization
        /// </summary>
        public UIElement LoadingOverlay
        {
            get
            {
                return _loadingOverlay;
            }
            set
            {
                if (_loadingOverlay != value)
                {
                    _loadingOverlay = value;
                    RaisePropertyChanged("LoadingOverlay");
                }
            }
        }

        #region CurrentTemplate (INotifyPropertyChanged Property)
        private Template _currentTemplate;

        public Template CurrentTemplate
        {
            get { return _currentTemplate; }
            set
            {
                if (_currentTemplate != value)
                {
                    _currentTemplate = value;
                    RaisePropertyChanged("CurrentTemplate");
                }
            }
        }
        #endregion

        public string UserId { get; set; }

        private Visibility _newappScreenVisibility = Visibility.Collapsed;
        public Visibility NewappScreenVisibility
        {
            get { return _newappScreenVisibility; }
            set
            {
                if (_newappScreenVisibility != value)
                {
                    _newappScreenVisibility = value;
                    RaisePropertyChanged("NewappScreenVisibility");
                }
            }
        }

        private bool _mapCenterRequiresRefresh;
        public bool MapCenterRequiresRefresh
        {
            get { return _mapCenterRequiresRefresh; }
            set { _mapCenterRequiresRefresh = value; RaisePropertyChanged("MapCenterRequiresRefresh"); }
        }


        private Visibility _builderScreenVisibility = Visibility.Collapsed;
        public Visibility BuilderScreenVisibility
        {
            get { return _builderScreenVisibility; }
            set
            {
                if (_builderScreenVisibility != value)
                {
                    _builderScreenVisibility = value;
                    RaisePropertyChanged("BuilderScreenVisibility");
                }
            }
        }



        #region SettingsPageVisibility 
        private Visibility _settingsPageVisibility = Visibility.Collapsed;
        public Visibility SettingsPageVisibility
        {
            get { return _settingsPageVisibility; }
            set
            {
                if (_settingsPageVisibility !=  value)
                {
                    _settingsPageVisibility = value;
                    RaisePropertyChanged("SettingsPageVisibility");
                }
            }
        }
        #endregion


        private bool _isInitialized;
        public bool IsInitialized
        {
            get { return _isInitialized; }
            set
            {
                if (_isInitialized != value)
                {
                    _isInitialized = value;
                    RaisePropertyChanged("IsInitialized");
                }
            }
        }

        private bool _updatingSettings;
        public bool UpdatingSettings
        {
            get { return _updatingSettings; }
            set
            {
                if (_updatingSettings != value)
                {
                    _updatingSettings = value;
                    RaisePropertyChanged("UpdatingSettings");
                }
            }
        }

        #region IMapApplication Methods
        public void HideWindow(FrameworkElement windowContents)
        {
            if (WindowManager != null)
                WindowManager.HideWindow(windowContents);
        }

        // Cannot throw exception here as MapApplication.Current is BuilderApplication at the beginning of
        // a Builder session, and there is code that checks whether MapApplication.Current.Map is null
        public Client.Map Map
        {
            get { return null; }
        }

        public Client.Layer SelectedLayer
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public event EventHandler SelectedLayerChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }
        public event EventHandler Initialized
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }
        public event EventHandler InitializationFailed
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        public object ShowWindow(string windowTitle, FrameworkElement windowContents, bool isModal = false, 
            EventHandler<System.ComponentModel.CancelEventArgs> onHidingHandler = null, 
            EventHandler onHideHandler = null, WindowType windowType = WindowType.Floating, 
            double? top = null, double? left = null)
        {
            if (WindowManager == null)
                WindowManager = new ESRI.ArcGIS.Mapping.Controls.WindowManager() { FloatingWindowStyle = Application.Current.Resources["BuilderWindowStyle"] as Style };
            else if (WindowManager.FloatingWindowStyle == null)
                WindowManager.FloatingWindowStyle = Application.Current.Resources["BuilderWindowStyle"] as Style;
            return WindowManager.ShowWindow(windowTitle, windowContents, isModal, onHidingHandler, onHideHandler, 
                windowType, top, left);
        }

        public void LoadMap(Map map, Action<LoadMapCompletedEventArgs> callback = null, object userToken = null)
        {
            throw new NotImplementedException();
        }

        public void LoadWebMap(string id, Document document = null, Action<GetMapCompletedEventArgs> callback = null,
            object userToken = null)
        {
            throw new NotImplementedException();
        }

        public DependencyObject FindObjectInLayout(string controlName)
        {
            throw new NotImplementedException();
        }

        public Uri ResolveUrl(string urlToBeResolved)
        {
            return ImageUrlResolver.ResolveUrl(urlToBeResolved);
        }

        public OnClickPopupInfo GetPopup(Graphic graphic, Layer layer, int? layerId = null)
        {
            throw new NotImplementedException();
        }

        public void ShowPopup(Graphic graphic, Layer layer, int? layerId = null)
        {
            throw new NotImplementedException();
        }

        #endregion
                
        public ObservableCollection<string> GetXapsInUse(out string behaviorsXml)
        {
            ESRI.ArcGIS.Mapping.Controls.ViewerApplicationControl va = ESRI.ArcGIS.Mapping.Controls.ViewerApplicationControl.Instance;
            ObservableCollection<string> usedXaps = new ObservableCollection<string>();
            IEnumerable<string> assemblies = va.GetAllAssembliesNamesInUse(out behaviorsXml);
            if (assemblies != null)
            {
                foreach (string assembly in assemblies)
                {
                    string containerXap = getXapNameForAssembly(assembly);
                    if (string.IsNullOrEmpty(containerXap))
                        continue;
                    usedXaps.Add(containerXap);
                }
            }

            return usedXaps;
        }

        public void SyncExtensionsInUse(IEnumerable<string> usedXaps)
        {
            ESRI.ArcGIS.Mapping.Controls.ViewerApplicationControl va = ESRI.ArcGIS.Mapping.Controls.ViewerApplicationControl.Instance;
            List<string> extensions = new List<string>();            
            
            foreach (string containerXap in usedXaps)
            {
                string url = string.Format("Extensions/{0}.xap", containerXap);
                if (!extensions.Contains(url))
                    extensions.Add(url);
            }

            va.ViewerApplication.Extensions = extensions;
        }        

        private string getXapNameForAssembly(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
                return null;
            if (AllExtensions != null)
            {
                foreach (Extension extensionXap in AllExtensions)
                {
                    if (extensionXap.Assemblies != null)
                    {
                        Assembly assem = extensionXap.Assemblies.FirstOrDefault<Assembly>(a => a.Name == assemblyName);
                        if (assem != null)
                            return extensionXap.Name;
                    }
                }
            }
            return null;
        }        

        internal byte[] GetPreviewImage()
        {
            return null;
            // The code below doesnt work unfortunately because of cross domain security issues
            //byte[] binaryData = null;
            //WriteableBitmap bitmap = new WriteableBitmap(ESRI.ArcGIS.Mapping.Controls.View.Instance.Content as UIElement, new TranslateTransform());            
            //EditableImage imageData = new EditableImage(bitmap.PixelWidth, bitmap.PixelHeight);
            //for (int y = 0; y < bitmap.PixelHeight; ++y)
            //{
            //    for (int x = 0; x < bitmap.PixelWidth; ++x)
            //    {
            //        int pixel = bitmap.Pixels[bitmap.PixelWidth * y + x];
            //        imageData.SetPixel(x, y, (byte)((pixel >> 16) & 0xFF), (byte)((pixel >> 8) & 0xFF), (byte)(pixel & 0xFF), (byte)((pixel >> 24) & 0xFF));
            //    }
            //}
            //using (System.IO.Stream pngStream = imageData.GetStream())
            //{
            //    binaryData = new Byte[pngStream.Length];                    
            //}

            //return binaryData;
        }

        private Visibility _gettingStartedVisibility = Visibility.Collapsed;
        public Visibility GettingStartedVisibility
        {
            get { return _gettingStartedVisibility; }
            set
            {
                if (_gettingStartedVisibility != value)
                {
                    _gettingStartedVisibility = value;
                    RaisePropertyChanged("GettingStartedVisibility");
                    RaisePropertyChanged("GettingStartedToggleButtonVisibility");
                }
            }
        }

        public Visibility GettingStartedToggleButtonVisibility
        {
            get
            {
                return _gettingStartedVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private TutorialDialogControl _tutorialDialogControl;
        public TutorialDialogControl TutorialDialogControl
        {
            get
            {
                return _tutorialDialogControl;
            }
            set
            {
                _tutorialDialogControl = value;
            }
        }

        private MapCenter _mapCenter;
        public MapCenter MapCenter
        {
            get
            {
                return _mapCenter;
            }
            set
            {
                _mapCenter = value;
            }
        }

        public AddContentDialog AddContentDialog { get; set; }

        public WindowManager WindowManager { get; private set; }

        public string GeometryServiceUrl
        {
            get { return Urls.GeometryServiceUrl; }
        }

        public string ProxyUrl
        {
            get { return Urls.ProxyUrl; }
        }

        public string BaseUrl
        {
            get
            {
                return Urls.BaseUrl;
            }
            set
            {
                Urls.BaseUrl = value;
            }
        }
        
        public bool IsEditMode
        {
            get { return false; }
        }

        public CultureInfo Culture { get; set; }

        ApplicationUrls urls;
        public ApplicationUrls Urls
        {
            get
            {
                return urls;
            }
        }

        public ArcGISPortal Portal
        {
            get
            {
                return null;
            }
        }

    }
}
