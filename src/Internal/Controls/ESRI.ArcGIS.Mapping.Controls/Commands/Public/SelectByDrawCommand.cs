/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
	[DisplayName("SelectByDrawDisplayName")]
	[Category("CategorySelection")]
	[Description("SelectByDrawDescription")]
    public class SelectByDrawCommand : LayerCommandBase
    {
        private ESRI.ArcGIS.Client.Editor _editor;

        private ESRI.ArcGIS.Client.Editor editor 
        {
            get
            {
                if (_editor == null)
                    _editor = new ESRI.ArcGIS.Client.Editor();

                return _editor;
            }
        }

        public override bool CanExecute(object parameter)
        {
            return GraphicsLayer != null && GraphicsLayer.Visible;
        }

        public override void Execute(object parameter)
        {
            // Configure the editor to use our map and layer
            editor.Map = Map;
            editor.LayerIDs = new List<string>() { GraphicsLayer.ID };

            // Force selection to be done with a rectangle
            editor.SelectionMode = DrawMode.Rectangle;

            // When selection is done, tell us about it so we can tell others
            editor.EditCompleted += new EventHandler<ESRI.ArcGIS.Client.Editor.EditEventArgs>(editor_EditCompleted);

            // Tell Editor that we want add another selection to any existing selections
            if (editor.Select.CanExecute("Add"))
                editor.Select.Execute("Add");
        }

        void editor_EditCompleted(object sender, ESRI.ArcGIS.Client.Editor.EditEventArgs e)
        {
            // Notify others listening for our event
            OnCanExecuteChanged(EventArgs.Empty);
            OnDrawCompleted(EventArgs.Empty);
        }

        GraphicsLayer GraphicsLayer
        {
            get { return Layer as GraphicsLayer; }
        }

        private Map Map
        {
            get
            {
                return MapApplication.Current.Map;
            }
        }

        protected virtual void OnDrawCompleted(EventArgs args)
        {
            if (DrawCompleted != null)
                DrawCompleted(this, args);
        }

        public event EventHandler DrawCompleted;
    }
}
