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
using System.ComponentModel;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Core
{
    public interface IApplicationServices
    {
        /// <summary>
        /// Browse the application folder for a file resource
        /// </summary>
        /// <param name="onComplete">Callback invoked when a file has been chosen</param>
        /// <param name="fileExts">Array of acceptable file extensions</param>
        /// <param name="startupFolderRelativePath">Folder to startup in</param>
        /// <param name="userState">User state</param>
        void BrowseForFile(EventHandler<BrowseCompleteEventArgs> onComplete, string[] fileExts = null, string startupFolderRelativePath = null, object userState = null);

        /// <summary>
        /// Browse for Map Layers 
        /// </summary>
        /// <param name="onComplete">Callback invoked when a file has been chosen</param>
        /// <param name="userState">User state</param>
        //void BrowseForLayer(EventHandler<BrowseForLayerCompletedEventArgs> onComplete, object userState = null);
    }

    /// <summary>
    /// Event arguments for the Browse event
    /// </summary>
    public class BrowseCompleteEventArgs : EventArgs
    {        
        /// <summary>
        /// Uri of the file
        /// </summary>
        public Uri Uri { get; set; }
        /// <summary>
        /// Relative Uri of the file
        /// </summary>
        public string RelativeUri { get; set; }
        /// <summary>
        /// UserState
        /// </summary>
        public object UserState { get; set; }
    }

    /// <summary>
    /// Event arguments for the browse event
    /// </summary>
    public class BrowseForLayerCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Layer selected in the browse dialog
        /// </summary>
        public Layer Layer { get; set; }
        /// <summary>
        /// UserState
        /// </summary>
        public object UserState { get; set; }
    }
}
