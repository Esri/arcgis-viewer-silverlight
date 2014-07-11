/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;
using System.ComponentModel.Composition;
using System;

namespace ESRI.ArcGIS.Mapping.Controls.MapContents
{
    internal class RenameSelectedLayerCommand : ICommand
    {
        public RenameSelectedLayerCommand()
        {
            if(MapApplication.Current != null)
                MapApplication.Current.SelectedLayerChanged += SelectedLayer_Changed;
        }

        ~RenameSelectedLayerCommand()
        {
            if (MapApplication.Current != null)
                MapApplication.Current.SelectedLayerChanged -= SelectedLayer_Changed;
        }

        public MapContents MapContents
        {
            get;
            set;
        }

        void SelectedLayer_Changed(object sender, EventArgs e)
        {
            RaiseCanExecuteChangedEvent(sender, e);
        }

        public bool CanExecute(object parameter)
        {
            return MapApplication.Current != null && MapApplication.Current.SelectedLayer != null;
        }

        public void Execute(object parameter)
        {
            if (MapContents == null || MapApplication.Current == null || MapApplication.Current.SelectedLayer == null)
                return;

            MapContents.EnableRenameForLayer(MapApplication.Current.SelectedLayer);
        }

        protected void RaiseCanExecuteChangedEvent(object sender, EventArgs args)
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(sender, args);
        }
        public event System.EventHandler CanExecuteChanged;
    }
}
