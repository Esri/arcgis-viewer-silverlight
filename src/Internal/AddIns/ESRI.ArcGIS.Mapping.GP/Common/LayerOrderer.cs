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
using ESRI.ArcGIS.Client;
using System.Collections.ObjectModel;

namespace ESRI.ArcGIS.Mapping.GP
{
    public class LayerOrderer
    {
        public static void OrderLayers(Map map, ObservableCollection<string> layerOrder, Dictionary<string, string> layerParamNameIDLookup)
        {
            if (layerOrder != null && layerOrder.Count > 1 && layerOrder != null && layerParamNameIDLookup != null &&
                layerOrder.Count == layerParamNameIDLookup.Count)
            {
                string prevLayerId = null;
                for (int i = 0; i < layerOrder.Count; i++)
                {
                    if (prevLayerId == null)
                    {
                        prevLayerId = getLayerID(layerOrder[i], layerParamNameIDLookup);
                        continue;
                    }
                    orderLayer(map, prevLayerId, getLayerID(layerOrder[i], layerParamNameIDLookup));
                    prevLayerId = getLayerID(layerOrder[i], layerParamNameIDLookup);
                }
            }
        }

        static string getLayerID(string layerParamName, Dictionary<string, string> layerParamNameIDLookup)
        {
            if (!string.IsNullOrEmpty(layerParamName) && layerParamNameIDLookup.ContainsKey(layerParamName)
                                       && !string.IsNullOrEmpty(layerParamNameIDLookup[layerParamName]))
                return layerParamNameIDLookup[layerParamName];
            return null;
        }

        static void orderLayer(Map map, string topLayerParamID, string bottomLayerID)
        {
            Layer layerToMove = map.Layers[bottomLayerID];
            if (layerToMove != null && getLayerIndex(map, topLayerParamID) > -1)
            {
                map.Layers.Remove(layerToMove);
                int topLayerIndex = getLayerIndex(map, topLayerParamID);
                if (topLayerIndex > -1)
                    map.Layers.Insert(topLayerIndex, layerToMove);
            }
        }

        static int getLayerIndex(Map map, string layerID)
        {
            for (int i = 0; i < map.Layers.Count; i++)
            {
                if (map.Layers[i].ID == layerID)
                    return i;
            }
            return -1;
        }
    }
}
