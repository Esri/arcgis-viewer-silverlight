/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Controls
{
	public class OpenWebMapCommand : CommandBase
	{
        MapCenter mapCenter;
		public override void Execute(object parameter)
		{
            //always start with new map center to reflect changes to urls ( arcgis online, proxy, geometry service)
            View view = View.Instance;
            mapCenter = new MapCenter();
            mapCenter.Height = view.ActualHeight - 100;
            mapCenter.Width = view.ActualWidth - 100;
            mapCenter.InitialVisibility = System.Windows.Visibility.Visible;
            mapCenter.MapSelectedForOpening += mapCenter_MapSelectedForOpening;
            
            ESRI.ArcGIS.Client.Extensibility.MapApplication.Current.ShowWindow(Resources.Strings.OpenWebMapFromArcGISCom,
                mapCenter);
		}

        void mapCenter_MapSelectedForOpening(object sender, MapCenter.MapEventArgs e)
        {
            MapCenter.OpenMap(e);
            ESRI.ArcGIS.Client.Extensibility.MapApplication.Current.HideWindow(mapCenter);
        }
	}
}
