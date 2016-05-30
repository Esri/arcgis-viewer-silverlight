/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Toolkit;
using System;

namespace ESRI.ArcGIS.Mapping.Controls
{
    internal class OnClickPopupControl : Control, IDisposable
    {
        // This is associated with the ScrollView control defined in the style XAML for the Identify control. This value
        // is assigned during OnTemplateApply so it can be assigned in the IdentifyBehavior when the Identify window is
        // displayed and positioned. By assigning this value, developers can customize the UI stored in this control
        // as it is exposed through the associated model view.
        private FrameworkElement AttributeContainer { get; set; }

        private ContentControl _popupToolbarContentControl;
        private const string PopupToolbarContainerName = "PopupToolbarContainer";
        private InfoWindow _infoWindow;
        private OnClickPopupInfo _popupInfo;

        public OnClickPopupInfo PopupInfo
        {
            get
            {
                return _popupInfo;
            }
            set
            {
                this.DataContext = this._popupInfo = value;

                if (this._infoWindow != null)
                    this._infoWindow.IsOpen = false;
                else
                    this.CreateInfoWindow();

                if (this.PopupInfo != null)
                {
                    this.PopupInfo.Container = _infoWindow;
                    this.PopupInfo.AttributeContainer = this.AttributeContainer;
                }
            }
        }

        public OnClickPopupControl()
        {
            this.Style = IdentifyLayoutStyleHelper.Instance.GetStyle("OnClickPopupContainerStyle");
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Assign the ScrollView called "AttributeContainer" to this variable
            AttributeContainer = GetTemplateChild("AttributeContainer") as FrameworkElement;
            // Dynamic Toolbar content control  (content comes from Tools.xml)
            _popupToolbarContentControl = GetTemplateChild(PopupToolbarContainerName) as ContentControl;

            PopulateToolbar();

            if (this.PopupInfo != null)
            {
                this.PopupInfo.AttributeContainer = this.AttributeContainer;
            }
            EditValuesCommand cmd = GetEditValuesToolCommand();
            if (cmd != null)
            {
                InfoWindow container = PopupInfo.Container as InfoWindow;
                cmd.OriginalPopupContent = container.Content;
            }
        }

        private void PopulateToolbar()
        {
            if (ToolPanels.Current == null || ToolPanels.Current.Count <= 0 || this._popupToolbarContentControl == null)
                return;

            ToolPanel popupToolPanel = ToolPanels.Current.FirstOrDefault(p => p.ContainerName == this._popupToolbarContentControl.Name);
            if (popupToolPanel != null)
                this._popupToolbarContentControl.Content = popupToolPanel;
        }

        internal void ResetInfoWindow()
        {
            if (_infoWindow != null)
            {
                if (_infoWindow.Content != null)
                    _infoWindow.Content = null;

                _infoWindow = null;
            }
        }
        internal EditValuesCommand GetEditValuesToolCommand()
        {
            if (_popupToolbarContentControl == null || _popupToolbarContentControl.Content == null) return null;

            ToolPanel tools = _popupToolbarContentControl.Content as ToolPanel;
            if (tools != null)
            {
                foreach(var tool in tools.AllButtons)
                {
                    if (tool.Command is EditValuesCommand)
                        return tool.Command as EditValuesCommand;
                }
            }
            return null;
        }

        private void CreateInfoWindow()
        {
            Map map = MapApplication.Current.Map;

            _infoWindow = new InfoWindow()
            {
                Map = map,
                Content = this
            };

            Panel parentPanel = FindAncestorOfType<Panel>(map);
            if (parentPanel != null)
                parentPanel.Children.Add(_infoWindow);

            #region Info Window has to have the same Grid/Canvas positioning as the Map in order to work properly
            if (parentPanel is Grid)
            {
                _infoWindow.SetValue(Grid.RowProperty, map.GetValue(Grid.RowProperty));
                _infoWindow.SetValue(Grid.ColumnProperty, map.GetValue(Grid.ColumnProperty));
                _infoWindow.SetValue(Grid.RowSpanProperty, map.GetValue(Grid.RowSpanProperty));
                _infoWindow.SetValue(Grid.ColumnSpanProperty, map.GetValue(Grid.ColumnSpanProperty));
            }
            else if (parentPanel is Canvas)
            {
                _infoWindow.SetValue(Canvas.LeftProperty, map.GetValue(Canvas.LeftProperty));
                _infoWindow.SetValue(Canvas.TopProperty, map.GetValue(Canvas.TopProperty));
            }
            #endregion
        }

        private static T FindAncestorOfType<T>(DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                obj = VisualTreeHelper.GetParent(obj);
                var objAsT = obj as T;
                if (objAsT != null)
                    return objAsT;
            }
            return null;
        }

        public void Dispose()
        {
            if (_infoWindow != null)
            {
                _infoWindow.IsOpen = false;
                _infoWindow.Content = null;
            }
        }
    }
}
