/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.ComponentModel.Composition;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
    [DisplayName("DeleteGraphicToolDisplayName")]
    [Category("CategoryPopup")]
    [Description("DeleteGraphicToolDescription")]

    public class DeleteGraphicCommand : PopupCommandBase
    {
        public override bool CanExecute(object parameter)
        {
            var popupInfo = parameter as OnClickPopupInfo;
            PopupInfo = popupInfo;
            if (popupInfo == null
            || popupInfo.PopupItem == null
            || !(popupInfo.PopupItem.Layer is FeatureLayer)
            || popupInfo.PopupItem.Graphic == null
            || !((FeatureLayer)popupInfo.PopupItem.Layer).IsDeleteAllowed(popupInfo.PopupItem.Graphic)) 
                return false;

            bool isEditable = LayerProperties.GetIsEditable(popupInfo.PopupItem.Layer);
            return isEditable;
        }

        public override void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;



            MessageBoxDialog.Show(Resources.Strings.DeleteGraphicConfirmation, Resources.Strings.DeleteGraphicConfirmationTitle, System.Windows.MessageBoxButton.OKCancel,
                new MessageBoxClosedEventHandler(delegate(object obj, MessageBoxClosedArgs args1)
                {
                    if (args1.Result == System.Windows.MessageBoxResult.OK)
                    {
                        var popupInfo = PopupInfo as OnClickPopupInfo;
                        if (popupInfo != null)
                        {
                            // Delete the graphic from the layer
                            FeatureLayer layer = popupInfo.PopupItem.Layer as FeatureLayer;
                            if (layer != null)
                            {
                                layer.Graphics.Remove(popupInfo.PopupItem.Graphic);

                                // Adjust the current PopupItem and SelectedIndex
                                int revisedCount = popupInfo.PopupItems.Count - 1;
                                if (revisedCount <= 0)
                                {
                                    // close the popup window
                                    if (popupInfo.Container != null && popupInfo.Container is InfoWindow)
                                        ((InfoWindow) popupInfo.Container).IsOpen = false;

                                    return;
                                }
                                // If the current PopupItem was the end of the list, go to the first PopupItem in the collection
                                int revisedIndex = (popupInfo.SelectedIndex < revisedCount)
                                                       ? popupInfo.SelectedIndex
                                                       : 0;
                                var list = new ObservableCollection<PopupItem>();
                                foreach (PopupItem item in popupInfo.PopupItems)
                                {
                                    if (item != popupInfo.PopupItem)
                                        list.Add(item);
                                }
                                popupInfo.PopupItems = list;
                                popupInfo.SelectedIndex = revisedIndex;
                            }
                        }
                    }
                }));

        }
    }
}
