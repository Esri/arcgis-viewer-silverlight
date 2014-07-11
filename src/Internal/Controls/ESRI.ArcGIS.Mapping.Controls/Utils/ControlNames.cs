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

namespace ESRI.ArcGIS.Mapping.Controls
{
    internal static class ControlNames
    {
        public const string MAPCONTENTS_CONTROL_NAME = "MapContents";
        public const string MAPCONTENTS_CONTROL_CONTAINER_NAME = "MapContentsContainer";
        public const string LAYER_CONFIGURATION_CONTROL_CONTAINER = "LayerConfigurationToolbarContainer";
        public const string ADDCONTENTCONTROLCONTAINER = "AddContentControlContainer";
        public const string SIDEPANELCONTAINER = "SidePanelContainer";
        public const string FEATUREDATAGRIDTABLECONTAINER = "FeatureDataGridContainer";
        public const string BROWSETABITEM = "BrowseTabItem";
		public const string MAPCONTENTSTABITEM = "MapContentsTabItem";
        public const string SEARCHTABITEM = "SearchTabItem";
        public const string EDITTABITEM = "EditTabItem";
        public const string EDITORWIDGET = "editorWidget";

        public const string VISUAL_STATE_HIDE_CONTENTCONTROL = "Hide";
        public const string VISUAL_STATE_SHOW_CONTENTCONTROL = "Show";
        
    }

    internal static class ControlDimensions
    {
        internal const double SIDEPANELWIDTH = 285;
    }
}
