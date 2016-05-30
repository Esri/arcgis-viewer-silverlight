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

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Enables configurability for an add-in
    /// </summary>
    public interface ISupportsConfiguration
    {
        /// <summary>
        /// Invoked by the application when the add-in is to be configured
        /// </summary>
        void Configure();

        /// <summary>
        /// Invoked by the application when the add-in is to be loaded
        /// </summary>
        /// <param name="configData">Contains persisted configuration data</param>
        void LoadConfiguration(string configData);

        /// <summary>
        /// Invoked by the application when the add-in is to be saved.  The application stores the configuration 
        /// returned by this method and passes it to the <see cref="LoadConfiguration"/> method when the add-in
        /// is subsequently loaded.
        /// </summary>
        /// <returns>The add-in's serialized configuration</returns>
        string SaveConfiguration();
    }
}
