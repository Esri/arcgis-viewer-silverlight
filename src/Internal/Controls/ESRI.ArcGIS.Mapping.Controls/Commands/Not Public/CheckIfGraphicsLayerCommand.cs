/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class CheckIfGraphicsLayerCommand : LayerCommandBase
    {
        #region ICommand Members

        public override bool CanExecute(object parameter)
        {
            return Layer is GraphicsLayer;
        }

        public override void Execute(object parameter)
        {
            // NO OP
        }

        #endregion
    }
}
