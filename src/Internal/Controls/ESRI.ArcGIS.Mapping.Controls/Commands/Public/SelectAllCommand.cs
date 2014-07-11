/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
	[DisplayName("SelectAllDisplayName")]
	[Category("CategorySelection")]
	[Description("SelectAllDescription")]
    public class SelectAllCommand : LayerCommandBase
    {
        #region ICommand Members

        public override bool CanExecute(object parameter)
        {
            return Layer is GraphicsLayer;
        }

        public override void Execute(object parameter)
        {
            if (Layer == null)
                return;

            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
                return;

            foreach (Graphic graphic in graphicsLayer)
            {
                if (!graphic.Selected)
                    graphic.Select();
            }

            OnCanExecuteChanged(EventArgs.Empty);            
        }
        #endregion        
    }
}
