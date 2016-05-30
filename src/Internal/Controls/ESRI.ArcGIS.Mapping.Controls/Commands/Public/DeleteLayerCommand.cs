/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
	[DisplayName("DeleteLayerDisplayName")]
	[Category("CategoryLayer")]
	[Description("DeleteLayerDescription")]
    public class DeleteLayerCommand : LayerCommandBase
    {
        #region ICommand Members

        public override bool CanExecute(object parameter)
        {
            return Layer != null && !(bool)Layer.GetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty);
        }

        private Map Map
        {
            get
            {
                return MapApplication.Current.Map;
            }
        }

        internal bool IsRunningInTestHarness { get; set; }

        public override void Execute(object parameter)
        {
            if (Layer == null || Map == null)
                return;

            if (IsRunningInTestHarness) // if running inside the test suite, remove the layer without prompting the user
            {
                removeSeletctedLayerFromMap();
                return;
            }
            string layerName = Layer.GetValue(MapApplication.LayerNameProperty) as string;
            string message = string.Format(LocalizableStrings.RemoveLayerPrompt, !string.IsNullOrWhiteSpace(layerName) ? layerName : LocalizableStrings.RemoveLayerNameSubstitude);
            MessageBoxDialog.Show(message, LocalizableStrings.RemoveLayerCaption, System.Windows.MessageBoxButton.OKCancel,
                            new MessageBoxClosedEventHandler(delegate(object obj, MessageBoxClosedArgs args1)
                            {
                                if (args1.Result == System.Windows.MessageBoxResult.OK)
                                {
                                    removeSeletctedLayerFromMap();
                                }
                            }));
        }

        private void removeSeletctedLayerFromMap()
        {
            int layerToDeleteIndex = Map.Layers.IndexOf(Layer);
            Map.Layers.Remove(Layer);
            if (Map.Layers.Count > 0)//make sure we try to select the next layer in line
            {
                if (layerToDeleteIndex >= Map.Layers.Count)
                    layerToDeleteIndex = Map.Layers.Count - 1;
                if(layerToDeleteIndex >= 0 && layerToDeleteIndex < Map.Layers.Count)
                    View.Instance.SelectedLayer = Map.Layers[layerToDeleteIndex];
            }
            OnCanExecuteChanged(EventArgs.Empty);
        }
        #endregion
    }
}
