/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class OpenItemCommand : ICommand
    {
        public OpenItemCommand()
        { 
        }
        public bool CanExecute(object parameter)
        {
            ESRI.ArcGIS.Client.Extensibility.PopupInfo param = parameter as ESRI.ArcGIS.Client.Extensibility.PopupInfo;
            if (param == null || param.PopupItem == null)
                return false;
            CustomGraphicsLayer cgl = param.PopupItem.Layer as CustomGraphicsLayer;
            if (cgl != null && param.PopupItem.Graphic != null)
                return true;
            return false;
        }

        public void Execute(object parameter)
        {
            ESRI.ArcGIS.Client.Extensibility.PopupInfo param = parameter as ESRI.ArcGIS.Client.Extensibility.PopupInfo;
            if (param == null || param.PopupItem == null)
                return;
            CustomGraphicsLayer cgl = param.PopupItem.Layer as CustomGraphicsLayer;
            if (cgl != null && param.PopupItem.Graphic != null)
                cgl.OnItemClicked(param.PopupItem.Graphic);
        }

#pragma warning disable 0067
		public event EventHandler CanExecuteChanged;
#pragma warning restore 0067
	}
}
