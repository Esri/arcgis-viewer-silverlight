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
using ESRI.ArcGIS.Client.Extensibility;
using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
    [DisplayName("BrowseDisplayName")]
    [Category("CategoryMap")]
    [Description("BrowseDescription")]
    public class AddContentCommand : CommandBase
	{
        AddContentDialog browseDialog;
        public override void Execute(object parameter)
        {
            if (browseDialog == null)
            {
                browseDialog = new AddContentDialog()
                {
                    DataSourceProvider = GetDataSourceProvider(),
                    ConnectionsProvider = GetConnectionsProvider(),
                    Width = 300,
                    Margin = new Thickness(10),
                    Map = MapApplication.Current.Map
                };
                browseDialog.LayerAdded += browseDialog_LayerAdded;
                browseDialog.LayerAddFailed +=browseDialog_LayerAddFailed;
            }
            else
                browseDialog.Reset();
            double height = GetViewHeight() - 100;
            if (height < 100) height = 100;
            browseDialog.Height = height > 300 ? 300 : height;
            MapApplication.Current.ShowWindow(Resources.Strings.SelectLayerToAdd, browseDialog, false, null, null);
        }

        void browseDialog_LayerAdded(object sender, LayerAddedEventArgs e)
        {
            View.Instance.AddContentDialog_LayerAdded(this, e);
        }

        void browseDialog_LayerAddFailed(object sender, ExceptionEventArgs e)
        {
            View.Instance.AddContentDialog_LayerAddFailed(this, e);
        }

        public static double GetViewHeight()
        {
            View view = View.Instance;
            return view != null ? view.ActualHeight : 300;
        }

        public static ESRI.ArcGIS.Mapping.DataSources.DataSourceProvider GetDataSourceProvider()
        {
            View view = View.Instance;
            return view != null ? view.DataSourceProvider : null;
        }

        public static ESRI.ArcGIS.Mapping.Core.ConnectionsProvider GetConnectionsProvider()
        {
            View view = View.Instance;
            return view != null ? view.ConnectionsProvider : null;
        }


        
    }
}
