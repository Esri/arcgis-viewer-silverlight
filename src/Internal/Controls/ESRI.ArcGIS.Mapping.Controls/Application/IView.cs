/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/


using ESRI.ArcGIS.Client;
namespace ESRI.ArcGIS.Mapping.Core
{
    public interface IView
    {
        void RaiseEvent(string eventName, object eventArgument);        
        Map Map { get; }
        void ChangeBaseMap(BaseMapInfo basemapInfo);
    }
}
