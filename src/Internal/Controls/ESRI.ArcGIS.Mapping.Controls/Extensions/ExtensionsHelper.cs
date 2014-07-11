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
using ESRI.ArcGIS.Client;
using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.Core;
using System.Collections.ObjectModel;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ExtensionsHelper
    {
        private View view;
        public View View
        {
            get { return view; }
            set { view = value; }
        }

        private ExtensionsDataManager extensionsDataManager;

        public ExtensionsHelper(View view, ExtensionsConfigData extensionConfigData)
        {
            View = view;
            extensionsDataManager = new ExtensionsDataManager() { ExtensionsConfigData = extensionConfigData };
        }

        public Dictionary<string, object> GetCommandExecutionParameters(CustomCommand command, bool addResultsCollection)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (View != null)
            {
                dict.Add("Map", View.Map);
                dict.Add("SelectedLayer", View.SelectedLayer);
                if (View.Map != null)
                {
                    List<string> layerDisplayNames = new List<string>();
                    foreach (Layer layer in View.Map.Layers)
                    {
                        string layerDisplayName = null;
#if SILVERLIGHT
                        layerDisplayName = layer.GetValue(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.LayerProps.TitleProperty) as string;
#endif
                        if (string.IsNullOrEmpty(layerDisplayName))
                            layerDisplayName = layer.ID;
                        if (string.IsNullOrEmpty(layerDisplayName))
                        {
                            // assign an ID
                            layerDisplayName = layer.ID = new Guid().ToString();
                        }
                        layerDisplayNames.Add(layerDisplayName);
                    }
                    dict.Add("LayerDisplayNames", layerDisplayNames);
                }
            }
            if (command != null)
                dict.Add("ConfigString", command.ConfigString);
            if (addResultsCollection)
            {
                ObservableCollection<object> resultsCollection = new ObservableCollection<object>();
                resultsCollection.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(resultsCollection_CollectionChanged);
                dict.Add("ResultsCollection", resultsCollection);
            }
            return dict;
        }

        void resultsCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null)
                {
                    foreach (object result in e.NewItems)
                    {
                        processResult(result);
                    }
                }
            }
        }

        private void processResult(object result)
        {
            object[] resultArray = result as object[];
            if (resultArray == null || resultArray.Length < 1)
                return;

            string resultType = resultArray[0] as string;
            if (string.IsNullOrEmpty(resultType))
                return;

            switch (resultType)
            {
                case "AddResultLayer":
                    // Input
                    // AddResultLayer newLayer selected=true|false visibleInLayerList=true|false layerDisplayName 
                    Layer newLayer = null;
                    if (resultArray.Length > 1)
                        newLayer = resultArray[1] as Layer;
                    if (newLayer == null)
                        return;
                    bool isLayerSelected = false;
                    if (resultArray.Length > 2)
                    {
                        bool b;
                        if (bool.TryParse(resultArray[2] != null ? resultArray[2].ToString() : "", out b))
                            isLayerSelected = b;
                    }
                    bool isLayerVisibleInLayerList = false;
                    if (resultArray.Length > 3)
                    {
                        bool b;
                        if (bool.TryParse(resultArray[3] != null ? resultArray[3].ToString() : "", out b))
                            isLayerVisibleInLayerList = b;
                    }
                    string layerDisplayName = null;
                    if (resultArray.Length > 4)
                    {
                        layerDisplayName = resultArray[4] as string;
                    }
                    if (View != null)
                        View.AddLayerToMap(newLayer, isLayerSelected, isLayerVisibleInLayerList, layerDisplayName, null);
                    break;
                case "CopyLayerConfiguration":
                    GraphicsLayer source = null;
                    if (resultArray.Length > 1)
                        source = resultArray[1] as GraphicsLayer;
                    if (source == null)
                        return;
                    GraphicsLayer target = null;
                    if (resultArray.Length > 1)
                        target = resultArray[2] as GraphicsLayer;
                    if (target == null)
                        return;
                    if (View != null)
                        View.CopyLayerConfiguration(source, target);
                    break;
                case "DeleteResultLayer":
                    // Input
                    // DeleteResultLayer oldLayer
                    Layer oldLayer = null;
                    if (resultArray.Length > 1)
                        oldLayer = resultArray[1] as Layer;
                    if (oldLayer == null)
                        return;
                    if (View != null)
                        View.DeleteLayerFromMap(oldLayer);
                    break;
                case "RefreshRibbon":
                    OnRefreshRibbon(EventArgs.Empty);
                    break;
            }
        }

        protected virtual void OnRefreshRibbon(EventArgs args)
        {
            if (RefreshRibbon != null)
                RefreshRibbon(this, args);
        }

        public event EventHandler RefreshRibbon;
    }
}
