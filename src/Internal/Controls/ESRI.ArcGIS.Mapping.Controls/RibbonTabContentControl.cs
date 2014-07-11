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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class RibbonTabContentControl : ContentControl
    {
        public RibbonTabContentControl()
        {
            DefaultStyleKey = typeof(RibbonTabContentControl);
            RibbonGroups = new ObservableCollection<RibbonGroupControl>();
        }

        #region public List<RibbonGroup> RibbonGroups
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<RibbonGroupControl> RibbonGroups
        {
            get { return GetValue(RibbonGroupsProperty) as ObservableCollection<RibbonGroupControl>; }
            set { SetValue(RibbonGroupsProperty, value); }
        }

        /// <summary>
        /// Identifies the RibbonGroups dependency property.
        /// </summary>
        public static readonly DependencyProperty RibbonGroupsProperty =
            DependencyProperty.Register(
                "RibbonGroups",
                typeof(ObservableCollection<RibbonGroupControl>),
                typeof(RibbonTabContentControl),
                new PropertyMetadata(null));
        #endregion 
    }
}
