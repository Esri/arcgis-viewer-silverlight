/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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
using System.Windows.Data;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class MapContentsDialog : Control
    {
        private Button btnOk;
        private MapContentsControl MapContentsControl;

        public MapContentsDialog()
        {
            DefaultStyleKey = typeof(MapContentsDialog);
        }

        public Map Map
        {
            get
            {
                return (Map)this.GetValue(MapProperty);
            }
            set
            {
                this.SetValue(MapProperty, value);
            }
        }

        public static readonly DependencyProperty MapProperty = DependencyProperty.Register("Map", typeof(Map), typeof(MapContentsDialog), new PropertyMetadata(OnMapPropertyChanged));

        private static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapContentsDialog list = d as MapContentsDialog;
            Map map = e.NewValue as Map;
            if (map == null)
                return;
            list.SetMap(map);            
        }

        internal void SetMap(Map map)
        {
            if (MapContentsControl != null)
                MapContentsControl.Map = Map;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if(btnOk != null)
                btnOk.Click -= OKButton_Click;

            btnOk = GetTemplateChild("btnOk") as Button;
            if (btnOk != null)
                btnOk.Click += OKButton_Click;

            MapContentsControl = GetTemplateChild("MapContentsControl") as MapContentsControl;
            if (MapContentsControl != null)
                MapContentsControl.Map = Map;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            OnOkButtonClicked(EventArgs.Empty);
        }

        protected virtual void OnOkButtonClicked(EventArgs args)
        {
            if (OkButtonClicked != null)
                OkButtonClicked(this, args);
        }

        public event EventHandler OkButtonClicked;
    }   
}
