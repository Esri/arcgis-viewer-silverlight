/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Linq;
using System.Windows;
using ESRI.ArcGIS.Client;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Extensibility;
using System.ComponentModel.Composition;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
    [DisplayName("EditValuesToolDisplayName")]
    [Category("CategoryPopup")]
    [Description("EditValuesToolDescription")]

    public class EditValuesCommand : PopupCommandBase
    {
        public override bool CanExecute(object parameter)
        {
            var popupInfo = parameter as OnClickPopupInfo;
            PopupInfo = popupInfo;
            if (popupInfo == null
            || popupInfo.PopupItem == null
            || popupInfo.PopupItem.Layer == null
            || popupInfo.PopupItem.Graphic == null
            || !(popupInfo.PopupItem.Layer is FeatureLayer)
            || popupInfo.PopupItem.Graphic.Attributes == null
            || popupInfo.PopupItem.Graphic.Attributes.Count <= 0
            || !((FeatureLayer)popupInfo.PopupItem.Layer).IsUpdateAllowed(popupInfo.PopupItem.Graphic))
                return false;

            bool isEditable = LayerProperties.GetIsEditable(popupInfo.PopupItem.Layer);
            return isEditable;
        }

        public override void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;

            var popupInfo = PopupInfo as OnClickPopupInfo;
            if (popupInfo == null) return;

            InfoWindow win = popupInfo.Container as InfoWindow;
            if (win != null)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        FeatureLayer featureLayer = popupInfo.PopupItem.Layer as FeatureLayer;
                        if (featureLayer == null) return;

                        FeatureDataForm form = new FeatureDataForm
                            {
                                Width = win.ActualWidth,
                                Height = win.ActualHeight,
                                Style = PopupFeatureDataFormStyleHelper.Instance.GetStyle("PopupFeatureDataFormStyle"),
								CommitButtonContent = Resources.Strings.Apply,
                                GraphicSource = popupInfo.PopupItem.Graphic,
                                FeatureLayer = featureLayer,
                                DataContext = popupInfo,
                            };

                        form.Loaded += (s, e) =>
                            {
                                // enable switching back to the original popup content when the 'Back' button is clicked
                                List<Button> buttons = ControlTreeHelper.FindChildrenOfType<Button>(form, int.MaxValue);
                                if (buttons != null && buttons.Count > 0)
                                {
                                    Button backButton = buttons.Where(b => b.Name == "BackButton").FirstOrDefault();
                                    if (backButton != null)
                                    {
                                        backButton.Content = Resources.Strings.EditValuesCommandEditorBackButtonContent;
                                        backButton.Click +=(ss, ee) => BackToOriginalContent(popupInfo);
                                    }
                                    Button closeButton = buttons.Where(b => b.Name == "CloseButton").FirstOrDefault();
                                    if (closeButton != null)
                                    {
                                        closeButton.Click +=(ss, ee) =>BackToOriginalContent(popupInfo);
                                    }
                                }
                                PopupHelper.SetDisplayMode(PopupHelper.DisplayMode.EditValues);
                            };
                        win.Content = null;
                        win.Content = form;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                });
            }
        }

        internal void BackToOriginalContent(OnClickPopupInfo popupInfo)
        {
            if (_OriginalPopupContent != null)
            {
                InfoWindow win = popupInfo.Container as InfoWindow;
                if (win != null)
                {
                    Dispatcher.BeginInvoke(() =>
                                               {
                            win.Content = _OriginalPopupContent;
                            PopupHelper.SetDisplayMode(PopupHelper.DisplayMode.ReadOnly);
                        });
                }
            }
        }


        public object OriginalPopupContent
        {
            get { return _OriginalPopupContent; }
            set
            {
                if (_OriginalPopupContent != value)
                {
                    _OriginalPopupContent = value;
                }
            }
        }
        private object _OriginalPopupContent = null;

        
        protected virtual void OnCompleted(EventArgs args)
        {
            if (Completed != null)
                Completed(this, args);
        }

        public event EventHandler Completed;
    }
}
