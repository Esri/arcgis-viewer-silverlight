/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using System.Collections.Generic;
using System.Windows;
using System;
using System.ComponentModel.Composition;
using ESRI.ArcGIS.Client.Extensibility;
using System.Windows.Input;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
    [DisplayName("ConfigureLayerDisplayName")]
    [Category("CategoryLayer")]
    [Description("ConfigureLayerDescription")]
    public class ConfigureLayerCommand : CommandBase
	{
        LayerConfigurationDialog control;
		public override void Execute(object parameter)
		{
            if (control == null)
            {
                control = new LayerConfigurationDialog()
                {
                    View = View.Instance
                };
            }
            ESRI.ArcGIS.Client.Extensibility.MapApplication.Current.ShowWindow(Resources.Strings.ConfigureSelectedLayer,
                control, false, null,
                 (o, e) => 
                 {
                     if (control != null)
                         control.View = null;
                     control = null;
                 });//Disassociate View from the control so that there is no overhead on every selected layer change
            //when config control react to the change.
		}
	}
}
