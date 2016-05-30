/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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

namespace ESRI.ArcGIS.Mapping.Core.DataSources
{
    public abstract class BaseMapDataSource : DataSource, IBaseMapDataSource
    {        
        public abstract bool CanSwitchBaseMapLayer(Client.Layer layer);
        public abstract void SwitchBaseMapLayer(Client.Layer oldLayer, BaseMapInfo newBaseMapInfo);
        public abstract Client.TiledMapServiceLayer CreateBaseMapLayer(BaseMapInfo baseMapInfo);
        public abstract void GetMapServiceMetaDataAsync(string mapServiceUrl, object userState);

        protected virtual void OnGetMapServiceMetaDataCompleted(GetMapServiceMetaDataCompletedEventArgs args)
        {
            if (GetMapServiceMetaDataCompleted != null)
                GetMapServiceMetaDataCompleted(this, args);
        }

        protected virtual void OnGetMapServiceMetaDataFailed(ExceptionEventArgs args)
        {
            if (GetMapServiceMetaDataFailed != null)
                GetMapServiceMetaDataFailed(this, args);
        }

        public event EventHandler<GetMapServiceMetaDataCompletedEventArgs> GetMapServiceMetaDataCompleted;
        public event EventHandler<ExceptionEventArgs> GetMapServiceMetaDataFailed;
    }
}
