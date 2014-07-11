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
using ESRI.ArcGIS.Client.Extensibility;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ESRI.ArcGIS.Mapping.Controls.Editor;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using System.Collections;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls.Editor
{
    public class ConfigureEditorCommand : ISupportsWizardConfiguration, INotifyPropertyChanged
    {
        private ToolPanel _panel;
        public ConfigureEditorCommand()
        {
            if (ToolPanels.Current["EditorToolbarContainer"] == null)
            {
                _panel = new ToolPanel();
                _panel.ContainerName = "EditorToolbarContainer";
                _panel.Name = "EditorToolbar";
                _panel.CanSerialize = false;

                ToolPanels.Current.Add(_panel);
            }
        }

        #region Configuration
        private EditorConfiguration _configuration;
        public EditorConfiguration Configuration
        {
            get
            {
                return _configuration;
            }
            set
            {
                if (_configuration != value)
                {
                    _configuration = value;
                    EditorConfiguration.Current = _configuration;
                   
                    OnPropertyChanged("Configuration");
                }
            }
        }

        #endregion

        #region Map
        private Map _map;
        public Map Map
        {
            get
            {
                return _map;
            }
            set
            {
                if (_map != value)
                {
                    SubscribeToLayerChanged(_map != null ? _map.Layers : null, false);
                    HandleLayersChangedSubscription(false);
                    _map = value;
                    HandleLayersChangedSubscription(true);
                    SubscribeToLayerChanged(_map != null ? _map.Layers : null, true);

                    if (_map != null && _map.Layers != null && _map.Layers.Count > 0)
                        PopulateLayerInfos();

                    OnPropertyChanged("Map");
                }
            }
        }

        private void Layers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SubscribeToLayerChanged(e.NewItems, true);
            SubscribeToLayerChanged(e.OldItems, false);

            PopulateLayerInfos();
        }

        private void SubscribeToLayerChanged(IList list, bool subscribe)
        {
            if (list == null)
                return;

            foreach (Layer layer in list)
            {
                FeatureLayer lay = layer as FeatureLayer;
                if (lay == null)
                    continue;

                if (subscribe && !lay.IsInitialized)
                    lay.Initialized += Layer_Initialized;
                else
                    lay.Initialized -= Layer_Initialized;
            }
        }

        private void Layer_Initialized(object sender, EventArgs e)
        {
            PopulateLayerInfos();
            FeatureLayer layer = sender as FeatureLayer;
            if (layer != null)
                layer.Initialized -= Layer_Initialized;
        }

        private void HandleLayersChangedSubscription(bool subscribe)
        {
            if (Map != null && Map.Layers != null)
            {
                if (subscribe)
                    Map.Layers.CollectionChanged += Layers_CollectionChanged;
                else
                    Map.Layers.CollectionChanged -= Layers_CollectionChanged;
            }
        }

        private void PopulateLayerInfos()
        {
            if (Configuration != null)
                Configuration.RefreshLayerInfos(Map);
        }
        #endregion

        private WizardPage selectEditableLayers;
        private WizardPage editorOptions;

        private EditorConfiguration _backupConfiguration;
        public ObservableCollection<WizardPage> GetWizardPages()
        {
            if (selectEditableLayers == null)
            {
                SelectLayers selectLayers = new SelectLayers();
                selectLayers.DataContext = Configuration;
                selectEditableLayers = new WizardPage()
                {
                    Content = selectLayers,
                    Heading = ESRI.ArcGIS.Mapping.Controls.Editor.Resources.Strings.SelectLayersHeader,
                    InputValid = true
                };
            }

            if (editorOptions == null)
            {
                EditorOptions options = new EditorOptions();
                options.DataContext = Configuration;
                editorOptions = new WizardPage()
                {
                    Content = options,
                    Heading = ESRI.ArcGIS.Mapping.Controls.Editor.Resources.Strings.EditorOptionsHeader,
                    InputValid = true
                };
            }

            // Add pages to wizard
            ObservableCollection<WizardPage> pages = new ObservableCollection<WizardPage>();
            pages.Add(selectEditableLayers);
            pages.Add(editorOptions);
            return pages;
        }

        #region ISupportsConfiguration
        private WizardCommand wizardDialog;
        public void Configure()
        {
            if (wizardDialog == null)
            {
                wizardDialog = new WizardCommand();
                wizardDialog.WizardStyle = StyleHelper.Instance.GetStyle("NoConfigurationWizardStyle");
                wizardDialog.DialogTitle = ESRI.ArcGIS.Mapping.Controls.Editor.Resources.Strings.EditorWidgetConfiguration;
                wizardDialog.IsModal = false;
            }
            wizardDialog.Execute(this);
        }


        public void LoadConfiguration(string configData)
        {
            Configuration = EditorConfiguration.FromString(configData);
            _backupConfiguration = Configuration != null ? Configuration.Clone() : null;
            if (_panel != null)
                Configuration.ToolPanel = _panel;
        }

        public string SaveConfiguration()
        {
            return Configuration != null ? Configuration.ToString() : null;
        }
        #endregion

        #region ISupportsWizardConfiguration members
        public void OnCancelled()
        {
            //reset configuration
            Configuration = _backupConfiguration.Clone();
            Configuration.RefreshLayerInfos(Map);
            RefreshUI();
        }
        private void RefreshUI()
        {
            if (selectEditableLayers != null && selectEditableLayers.Content != null)
            {
                Control ctr = selectEditableLayers.Content as Control;
                ctr.DataContext = Configuration;
            }
            if (editorOptions != null && editorOptions.Content != null)
            {
                Control ctr = editorOptions.Content as Control;
                ctr.DataContext = Configuration;
            }
        }

        public void OnCompleted()
        {
            _backupConfiguration = Configuration != null ? Configuration.Clone() : null;
        }

        public WizardPage CurrentPage { get; set; }

        // Size of the Editor configuration UI
        private Size desiredSize = new Size(450, 335);
        public Size DesiredSize
        {
            get { return desiredSize; }
            set { desiredSize = value; }
        }

        private WizardPage lastPage;
        public bool PageChanging()
        {
            // Store reference to current wizard page before page changes
            lastPage = CurrentPage;
            return true;
        }

        private ObservableCollection<WizardPage> pages;
        public ObservableCollection<WizardPage> Pages
        {
            get
            {
                if (pages == null)
                    pages = GetWizardPages();

                return pages;
            }
            set { pages = value; }
        }

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
