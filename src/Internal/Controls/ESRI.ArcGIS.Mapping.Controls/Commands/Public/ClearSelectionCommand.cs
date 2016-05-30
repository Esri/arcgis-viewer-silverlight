/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
	[DisplayName("ClearSelectionDisplayName")]
	[Category("CategorySelection")]
	[Description("ClearSelectionDescription")]
    public class ClearSelectionCommand : LayerCommandBase
    {   
        #region ICommand Members

        public override bool CanExecute(object parameter)
        {
            if (Layer == null)
                return false;

            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
                return false;

            if (graphicsLayer.Visible == false)
                return false;

            return graphicsLayer.SelectedGraphics.Count() > 0;            
        }

        public override void Execute(object parameter)
        {
            if (Layer == null)
                return;

            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
                return;

            List<Graphic> selectedGraphics = new List<Graphic>();
            foreach (Graphic selectedGraphic in graphicsLayer.SelectedGraphics)
            {
                selectedGraphics.Add(selectedGraphic);
            }
            foreach (Graphic graphic in selectedGraphics)
            {
                graphic.UnSelect();
            }

            OnCanExecuteChanged(EventArgs.Empty);
        }
        #endregion
    }
}
