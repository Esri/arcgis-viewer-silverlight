/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using ESRI.ArcGIS.Client.WebMap;
using ESRI.ArcGIS.Client.Portal;

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Interface implemented and accessed indirectly through MapApplication class to provide application services.
    /// </summary>
    public interface IMapApplication
    {
        /// <summary>
        /// Provides access to the Map in the application
        /// </summary>
        Map Map { get; }

        /// <summary>
        /// Gets or set the application's currently selected layer
        /// </summary>
        Layer SelectedLayer { get; set; }

        /// <summary>
        /// Event raised when the selected layer changes
        /// </summary>
        event EventHandler SelectedLayerChanged;

        /// <summary>
        /// Event raised when the application has been initialized
        /// </summary>
        event EventHandler Initialized;

        /// <summary>
        /// Event raised when application initialization fails
        /// </summary>
        event EventHandler InitializationFailed;

        /// <summary>
        /// Displays content in an application window
        /// </summary>
        /// <param name="windowTitle">Title of the window</param>
        /// <param name="windowContents">Contents to be displayed in the window</param>        
        /// <param name="isModal">Determines wheter the window is modal or not</param>
        /// <param name="onHidingHandler">Event handler invoked when window is being hidden, and can be handled to cancel window closure</param>
        /// <param name="onHideHandler">Event handler invoked when the window is hidden</param>        
        /// <param name="windowType">Determines the type of the window</param>        
        /// <param name="top">The distance from the top of the application at which to position the window</param>
        /// <param name="left">The distance from the left of the application at which to position the window</param>
        /// <returns>The window</returns>
        object ShowWindow(string windowTitle, FrameworkElement windowContents, bool isModal, EventHandler<CancelEventArgs> onHidingHandler, EventHandler onHideHandler, WindowType windowType, double? top, double? left);

        /// <summary>
        /// Hides the application window hosting the given content
        /// </summary>
        /// <param name="windowContents">Element previously displayed in a window using ShowWindow</param>
        void HideWindow(FrameworkElement windowContents);

        /// <summary>
        /// Finds a DependencyObject in the layout of the application
        /// </summary>
        /// <param name="objectName">The name of the object</param>
        /// <returns>The object, or null if not found</returns>
        DependencyObject FindObjectInLayout(string objectName);

        /// <summary>
        /// Gets whether the application is in edit mode
        /// </summary>
        bool IsEditMode { get; }

        /// <summary>
        /// Resolves a relative URL to a fully-qualified application URL
        /// </summary>
        /// <param name="urlToBeResolved">The relative URL to be resolved</param>
        /// <returns>The resolved URL</returns>
        Uri ResolveUrl(string urlToBeResolved);

        /// <summary>
        /// The culture of the currently running map application. This must be assigned when the application begins
        /// execution. It is stored here so it can be accessed anywhere, including background worker threads that
        /// lose their culture information in Silverlight.
        /// </summary>
        CultureInfo Culture { get; }

        /// <summary>
        /// Gets the set of URL-accessible endpoints used by the application
        /// </summary>
        ApplicationUrls Urls { get; }

        /// <summary>
        /// Gets the ArcGIS portal endpoint used by the application
        /// </summary>
        ArcGISPortal Portal { get; }

        /// <summary>
        /// Gets the popup info for a given feature
        /// </summary>
        /// <param name="graphic">The graphic representing the feature to retrieve popup info for</param>
        /// <param name="layer">The layer containing the feature to retrieve popup info for</param>
        /// <param name="layerId">The ID of the sub-layer containing the feature to retrieve popup info for</param>
        /// <returns>The feature's popup info</returns>
        OnClickPopupInfo GetPopup(Graphic graphic, Layer layer, int? layerId = null);

        /// <summary>
        /// Display the popup for a given feature
        /// </summary>
        /// <param name="graphic">The graphic representing the feature to show the popup for</param>
        /// <param name="layer">The layer containing the feature to show the popup for</param>
        /// <param name="layerId">The ID of the sub-layer containing the feature to show the popup for</param>
        void ShowPopup(Graphic graphic, Layer layer, int? layerId = null);

        /// <summary>
        /// Loads the layers from a map into the application
        /// </summary>
        /// <param name="map">The <see cref="Map"/> to load layers from</param>
        /// <param name="callback">The method to fire once the map has been loaded</param>
        /// <param name="userToken">Object to pass through to the callback method</param>
        void LoadMap(Map map, Action<LoadMapCompletedEventArgs> callback = null, object userToken = null);

        /// <summary>
        /// Loads a web map into the application
        /// </summary>
        /// <param name="id">The id of the web map to load</param>
        /// <param name="document">The <see cref="Document"/> containing information about the web map to load</param>
        /// <param name="callback">The method to fire once the web map has been loaded</param>
        /// <param name="userToken">Object to pass through to the callback method</param>
        void LoadWebMap(string id, Document document = null, Action<GetMapCompletedEventArgs> callback = null, object userToken = null);        
    }
}
