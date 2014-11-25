/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.ObjectModel;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    // This utility class was made public so it can be utilized by the Builder application.
    public static class LayerUtils
    {
        public static Layer GetLayerToSelectAfterDelete(ObservableCollection<Layer> LayerCollection, Layer CurrentlySelectedLayer)
        {
            int selectedNewIndex = -1;
            if (LayerCollection != null && CurrentlySelectedLayer != null)
            {
                int currentIndex = LayerCollection.IndexOf(CurrentlySelectedLayer);
                if (currentIndex >= 0)//index found
                {
                    //if at least one item before current check is visible, mark for selection
                    if (currentIndex > 0)
                        selectedNewIndex = currentIndex - 1;

                    if (selectedNewIndex < 0 && currentIndex < LayerCollection.Count - 1)
                    {
                        //if no new index and there is at least one item after current check is visible, mark for selection
                        selectedNewIndex = currentIndex + 1;
                    }

                    if (selectedNewIndex >= 0 && selectedNewIndex < LayerCollection.Count)
                        return LayerCollection[selectedNewIndex];
                }
            }
            return null;
        }

        public static bool HasNonBasemapLayerBeforeAfterIndex(Layer layer, LayerCollection collection, bool before)
        {
            if (layer == null || collection == null)
                return false;

            int index = collection.IndexOf(layer);
            if (index < 0) //layer not found
                return false;

            if (before)
            {
                for(int i = index - 1; i >= 0; i--)
                {
                    if (!(bool)collection[i].GetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty))
                        return true;
                }
            }
            else
            {
                if (index < collection.Count - 1)
                {
                    for (int i = index + 1; i < collection.Count; i++)
                    {
                        if (!(bool)collection[i].GetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty))
                            return true;
                    }
                }

            }
            return false;
        }

        public static GeometryType GetGeometryTypeFromGraphic(Graphic graphic)
        {
            if (graphic != null)
            {
                if (graphic.Geometry is MapPoint)
                {
                    return GeometryType.Point;
                }
                else if (graphic.Geometry is Polyline)
                {
                    return GeometryType.Polyline;
                }
                else if (graphic.Geometry is Polygon || graphic.Geometry is Envelope)
                {
                    return GeometryType.Polygon;
                }
                else if (graphic.Geometry is MultiPoint)
                {
                    return GeometryType.MultiPoint;
                }
            }
            return GeometryType.Unknown;
        }
    }
}
