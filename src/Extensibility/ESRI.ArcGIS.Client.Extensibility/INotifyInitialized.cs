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

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Provides members to notify external components of initialization state
    /// </summary>
    public interface INotifyInitialized
    {
        /// <summary>
        /// Raised when the component has initialized
        /// </summary>
        event EventHandler Initialized;

        /// <summary>
        /// Raised when component initialization has failed
        /// </summary>
        event EventHandler InitializationFailed;

        /// <summary>
        /// Gets whether the component is initialized
        /// </summary>
        bool IsInitialzed { get; }

        /// <summary>
        /// Gets the initialization exception.  Populated in the event initialization has failed
        /// </summary>
        Exception InitializationError { get; }
    }
}
