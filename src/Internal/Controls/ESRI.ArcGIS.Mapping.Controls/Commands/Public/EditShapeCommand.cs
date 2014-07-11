/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;
using System.ComponentModel.Composition;
using ESRI.ArcGIS.Mapping.Controls.Utils;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
    [DisplayName("EditShapeToolDisplayName")]
    [Category("CategoryPopup")]
    [Description("EditShapeToolDescription")]
    public class EditShapeCommand : PopupCommandBase
    {
        private ESRI.ArcGIS.Client.Editor _editor;
        private ESRI.ArcGIS.Client.Editor Editor { get { return _editor ?? (_editor = EditorCommandUtility.FindEditorInVisualTree()); } }

        public override bool CanExecute(object parameter)
        {
            var popupInfo = parameter as OnClickPopupInfo;
            PopupInfo = popupInfo;
            if (popupInfo == null
                || popupInfo.PopupItem == null) return false;

            return EditorCommandUtility.CanEditShape(popupInfo.PopupItem);
        }

        public override void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;

            EditorCommandUtility.EditShape(PopupInfo as OnClickPopupInfo, Editor, true);
        }
    }
}
