/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class CheckIfLayerSupportsPopUpsCommand : LayerCommandBase
    {
        #region ICommand Members

        public override bool CanExecute(object parameter)
        {
            return ESRI.ArcGIS.Mapping.Core.IdentifySupport.LayerSupportsPopUps(Layer);
        }

        public override void Execute(object parameter)
        {
            // NO OP
        }

        #endregion
    }
}
