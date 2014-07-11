/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Interactivity;
using System.Windows.Data;
using System.ComponentModel;
using ESRI.ArcGIS.Mapping.Controls.MapContents.Resources;

namespace ESRI.ArcGIS.Mapping.Controls.MapContents
{
    public class MapContentsConfigControl : Control
    {
        public MapContentsConfigControl()
        {
            this.DefaultStyleKey = typeof(MapContentsConfigControl);
        }

        private List<MapContentsMode> _modes;
        public List<MapContentsMode> MapContentsModes
        {
            get
            {
                if (_modes == null)
                {
                    _modes = new List<MapContentsMode>();
                    _modes.Add(new MapContentsMode() { Mode = Mode.Contents, DisplayName = Strings.Contents });
                    _modes.Add(new MapContentsMode() { Mode = Mode.LayerList, DisplayName = Strings.LayerList });
                    _modes.Add(new MapContentsMode() { Mode = Mode.Legend, DisplayName = Strings.Legend });
                }

                return _modes;
            }
        }

        #region OkButtonText
        /// <summary>
        /// 
        /// </summary>
        public string OkButtonText
        {
            get { return GetValue(OkButtonTextProperty) as string; }
            set { SetValue(OkButtonTextProperty, value); }
        }

        /// <summary>
        /// Identifies the OkButtonText dependency property.
        /// </summary>
        public static readonly DependencyProperty OkButtonTextProperty =
            DependencyProperty.Register(
                "OkButtonText",
                typeof(string),
                typeof(MapContentsConfigControl),
                new PropertyMetadata(null));
        #endregion

        #region OkButtonCommand
        /// <summary>
        /// 
        /// </summary>
        public ICommand OkButtonCommand
        {
            get { return GetValue(OkButtonCommandProperty) as ICommand; }
            set { SetValue(OkButtonCommandProperty, value); }
        }

        /// <summary>
        /// Identifies the OkButtonCommand dependency property.
        /// </summary>
        public static readonly DependencyProperty OkButtonCommandProperty =
            DependencyProperty.Register(
                "OkButtonCommand",
                typeof(ICommand),
                typeof(MapContentsConfigControl),
                new PropertyMetadata(null));
        #endregion
    }

    public class MapContentsMode
    {
        public string DisplayName { get; set; }
        public Mode Mode {get; set; }
    }
}
