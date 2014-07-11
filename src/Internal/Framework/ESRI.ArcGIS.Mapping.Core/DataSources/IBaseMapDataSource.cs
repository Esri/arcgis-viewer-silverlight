/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Core.DataSources
{
    public interface IBaseMapDataSource
    {
        bool CanSwitchBaseMapLayer(Layer layer);
        void SwitchBaseMapLayer(Layer oldLayer, BaseMapInfo newBaseMapInfo);
        TiledMapServiceLayer CreateBaseMapLayer(BaseMapInfo baseMapInfo);
        void GetMapServiceMetaDataAsync(string mapServiceUrl, object userState);

        event EventHandler<GetMapServiceMetaDataCompletedEventArgs> GetMapServiceMetaDataCompleted;
        event EventHandler<ExceptionEventArgs> GetMapServiceMetaDataFailed;
    }
}
