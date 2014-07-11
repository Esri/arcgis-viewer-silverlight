/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Net;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls;

namespace ESRI.ArcGIS.Mapping.Controls.Editor
{
    public class AttachmentEditorConfigControl : Control
    {
        public AttachmentEditorConfigControl()
        {
            DefaultStyleKey = typeof(AttachmentEditorConfigControl);
            Execute = new DelegateCommand(ConfigComplete, CanConfigComplete);
            this.DataContext = this;
        }

        public DelegateCommand Execute { get; set; }

        public AttachmentEditorConfiguration Configuration
        {
            get { return (AttachmentEditorConfiguration)GetValue(ConfigurationProperty); }
            set { SetValue(ConfigurationProperty, value); }
        }

        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register("Configuration", typeof(AttachmentEditorConfiguration), typeof(AttachmentEditorConfigControl), null);

        private void ConfigComplete(object parameter)
        {
            if (ConfigCompleted != null)
                ConfigCompleted(this, null);
        }

        private bool CanConfigComplete(object commandParameter)
        {
            if (Configuration != null && !(string.IsNullOrWhiteSpace(Configuration.Filter))
                && !(double.IsNaN(Configuration.FilterIndex) && Configuration.FilterIndex > 0)
                && !(double.IsNaN(Configuration.Width) && Configuration.Width > 0)
                && !(double.IsNaN(Configuration.Height) && Configuration.Height > 0))
                return true;
            return false;
        }

        public event EventHandler ConfigCompleted;
    }
}
