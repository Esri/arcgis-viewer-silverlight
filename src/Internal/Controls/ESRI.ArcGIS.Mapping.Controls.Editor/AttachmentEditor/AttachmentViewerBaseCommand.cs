/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Toolkit;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System;
using System.Windows;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls.Editor
{
    public abstract class AttachmentBaseCommand : ICommand, ISupportsConfiguration
    {
        private FloatingWindow _window;
        private AttachmentEditor _attachmentEditor;

        #region ICommand Members
        public event EventHandler CanExecuteChanged;
        protected void OnCanExecuteChanged(EventArgs args)
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, args);
        }

        protected abstract FeatureLayer GetFeatureLayer(object parameter);
        protected virtual string StyleName
        {
            get
            {
                return null;
            }
        }

        public bool CanExecute(object parameter)
        {
            if (GetFeatureLayer(parameter) != null)
                return true;

            return false;
        }

        private Style _editorStyle;
        private Style AttachmentEditorStyle
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(StyleName) && _editorStyle == null)
                    _editorStyle = StyleHelper.Instance.GetStyle(StyleName);

                return _editorStyle;
            }
        }

        public void Execute(object parameter)
        {
            if (!string.IsNullOrWhiteSpace(StyleName) && AttachmentEditorStyle == null)
                return;

            FeatureLayer layer = GetFeatureLayer(parameter);
            if (layer == null)
                return;

            Graphic graphic = layer.SelectedGraphics != null ? layer.SelectedGraphics.ElementAt(0) : null;
            if (graphic == null)
                return;

            if (_configuration == null)
                _configuration = new AttachmentEditorConfiguration();

            if (_attachmentEditor == null)
            {
                _attachmentEditor = new AttachmentEditor();
                _attachmentEditor.Width = _configuration.Width;
                _attachmentEditor.Height = _configuration.Height;
                _attachmentEditor.FilterIndex = _configuration.FilterIndex;
                _attachmentEditor.Filter = _configuration.Filter;
                _attachmentEditor.Multiselect = _configuration.MultiSelect;
                if(AttachmentEditorStyle != null)
                    _attachmentEditor.Style = AttachmentEditorStyle;

                _window = new FloatingWindow();
                _window.Content = _attachmentEditor;
                _window.Title = Resources.Strings.AttachmentEditorHeader;
            }

            _attachmentEditor.DataContext = _configuration;
            _attachmentEditor.GraphicSource = null;
            _attachmentEditor.FeatureLayer = null;
            if (graphic != null)
            {
                _attachmentEditor.FeatureLayer = layer;
                _attachmentEditor.GraphicSource = graphic;
                _window.Show(true);
            }
        }
        #endregion

        private AttachmentEditorConfigControl _configControl;
        public void Configure()
        {
            if (_configControl == null)
            {
                _configControl = new AttachmentEditorConfigControl();
                _configControl.ConfigCompleted += _configControl_ConfigCompleted;
            }

            if (_configuration == null)
                _configuration = new AttachmentEditorConfiguration();

            _configControl.Configuration = _configuration;

            MapApplication.Current.ShowWindow(Resources.Strings.AttachmentEditorConfigHeader, _configControl,
                true, null, null, WindowType.DesignTimeFloating);
        }

        private void _configControl_ConfigCompleted(object sender, EventArgs e)
        {
            MapApplication.Current.HideWindow(_configControl);
        }

        protected AttachmentEditorConfiguration _configuration;
        public void LoadConfiguration(string configData)
        {
            _configuration = AttachmentEditorConfiguration.FromString(configData);
        }
        public string SaveConfiguration()
        {
            return _configuration.ToString();
        }
    }
}
