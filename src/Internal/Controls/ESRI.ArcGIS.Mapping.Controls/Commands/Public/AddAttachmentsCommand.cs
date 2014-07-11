/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Tasks;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Client.Toolkit;
using System.Windows.Media;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
    [DisplayName("AddAttachmentsToolDisplayName")]
    [Category("CategoryPopup")]
    [Description("AddAttachmentsToolDescription")]
    public class AddAttachmentsCommand : PopupCommandBase
    {
        public override bool CanExecute(object parameter)
        {
            var popupInfo = parameter as OnClickPopupInfo;
            if (popupInfo == null
            || popupInfo.PopupItem == null
            || !(popupInfo.PopupItem.Layer is FeatureLayer)
            || popupInfo.PopupItem.Graphic == null
            || !((FeatureLayer)popupInfo.PopupItem.Layer).IsAddAttachmentAllowed(popupInfo.PopupItem.Graphic))
                return false;

            if (popupInfo == null || popupInfo.PopupItem == null || popupInfo.PopupItem.Layer == null) return false;

            bool isEditable = LayerProperties.GetIsEditable(popupInfo.PopupItem.Layer);
            if (!isEditable) return false;

            FeatureLayer fl = popupInfo.PopupItem.Layer as FeatureLayer;
            if (fl == null) return false;

            return fl.LayerInfo.HasAttachments;
        }

        public override void Execute(object parameter)
        {
            var popupInfo = parameter as OnClickPopupInfo;
            if (popupInfo != null)
            {
                AttachmentEditor ae = popupInfo.AttachmentContainer as AttachmentEditor;
                if (ae != null)
                {
                    FrameworkElement root = VisualTreeHelper.GetChild(ae, 0) as FrameworkElement;
                    if (root != null)
                    {
                        Button button = root.FindName("AddNewButton") as Button;
                        if (button != null)
                        {
                            ButtonAutomationPeer buttonAutoPeer = new ButtonAutomationPeer(button);
                            IInvokeProvider invokeProvider = buttonAutoPeer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                            invokeProvider.Invoke();
                        }
                    }
                }
            }


        }

        protected virtual void OnCompleted(EventArgs args)
        {
            if (Completed != null)
                Completed(this, args);
        }

        public event EventHandler Completed;

        #region Properties
        /// <summary>
        /// Gets or sets a filter string that specifies the file types and descriptions
        /// to display in the System.Windows.Controls.OpenFileDialog.
        /// </summary>
        /// <value>
        /// A filter string that specifies the file types and descriptions to display in the OpenFileDialog.
        /// The default is <see cref="System.String.Empty"/>.
        /// </value>
        /// <exception cref="System.ArgumentException">
        /// The filter string does not contain at least one vertical bar (|).
        /// </exception>
        public string Filter { get; set; }

        /// <summary>
        /// Gets or sets the index of the selected item in the System.Windows.Controls.OpenFileDialog 
        /// filter drop-down list.
        /// </summary>
        /// <value>
        /// The index of the selected item in the System.Windows.Controls.OpenFileDialog 
        /// filter drop-down list. The default is 1.
        /// </value>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The filter index is less than 1.
        /// </exception>
        public int FilterIndex { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the System.Windows.Controls.OpenFileDialog 
        /// allows users to select multiple files.
        /// </summary>
        /// <value>
        /// <c>true</c> if multiple selections are allowed; otherwise, <c>false</c>. The default is 
        /// </value>
        public bool Multiselect { get; set; }

        #endregion
    }
}
