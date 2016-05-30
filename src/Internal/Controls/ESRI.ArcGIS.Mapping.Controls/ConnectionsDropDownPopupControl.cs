/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.Core;
using System.Collections.ObjectModel;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = "ConnectionsListBox", Type = typeof(ListBox))]
    public class ConnectionsDropDownPopupControl : Control
    {   
        internal ListBox ConnectionsListBox { get; private set; }

        #region Connections
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<Connection> Connections
        {
            get { return GetValue(ConnectionsProperty) as ObservableCollection<Connection>; }
            set { SetValue(ConnectionsProperty, value); }
        }

        /// <summary>
        /// Identifies the Connections dependency property.
        /// </summary>
        public static readonly DependencyProperty ConnectionsProperty =
            DependencyProperty.Register(
                "Connections",
                typeof(ObservableCollection<Connection>),
                typeof(ConnectionsDropDownPopupControl),
                new PropertyMetadata(null, OnConnectionsPropertyChanged));

        /// <summary>
        /// ConnectionsProperty property changed handler.
        /// </summary>
        /// <param name="d">ConnectionsDropDownPopupControl that changed its Connections.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnConnectionsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ConnectionsDropDownPopupControl source = d as ConnectionsDropDownPopupControl;
            source.onConnectionsChanged();
        }

        private void onConnectionsChanged()
        {
            if (ConnectionsListBox != null && ConnectionsListBox.ItemsSource == null)
            {
                if (Connections != null)
                    ConnectionsListBox.ItemsSource = Connections;
            }
        }
        #endregion public ObservableCollection<Connection> Connections
                
        public ConnectionsDropDownPopupControl()
        {
            DefaultStyleKey = typeof(ConnectionsDropDownPopupControl);

            SetValue(ConnectionsProperty, new ObservableCollection<Connection>());
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (ConnectionsListBox != null)
                ConnectionsListBox.SelectionChanged -= ListBox_SelectionChanged;

            ConnectionsListBox = GetTemplateChild("ConnectionsListBox") as ListBox;
            if (ConnectionsListBox != null)
            {                
                if (Connections != null)
                    ConnectionsListBox.ItemsSource = Connections;
                ConnectionsListBox.SelectionChanged += ListBox_SelectionChanged;
            }
        }

        void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnConnectionSelected(new ConnectionEventArgs() { Connection = ConnectionsListBox.SelectedItem as Connection });
        }

        public void ClearSelection()
        {
            if (ConnectionsListBox != null)
                ConnectionsListBox.SelectedIndex = -1;
        }

        internal void DeleteConnection(Connection connectionToDelete)
        {
            if (connectionToDelete == null)
                return;

            OnConnectionDeleted(new ConnectionEventArgs() { Connection = connectionToDelete });
        }
        
        protected void OnConnectionSelected(ConnectionEventArgs args)
        {
            if (ConnectionSelected != null)
                ConnectionSelected(this, args);
        }

        protected void OnConnectionDeleted(ConnectionEventArgs args)
        {
            if (ConnectionDeleted != null)
                ConnectionDeleted(this, args);
        }

        public event EventHandler<ConnectionEventArgs> ConnectionSelected;
        public event EventHandler<ConnectionEventArgs> ConnectionDeleted;
    }

    public class ConnectionEventArgs : EventArgs
    {
        public Connection Connection { get; set; }
    }
}
