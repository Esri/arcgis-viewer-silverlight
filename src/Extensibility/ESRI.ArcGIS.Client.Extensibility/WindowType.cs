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
    /// Enumerates the types of windows supported by the application
    /// </summary>
    public enum WindowType
    {
        /// <summary>
        /// A default movable window
        /// </summary>
        Floating, 
        /// <summary>
        /// A movable window with the design-time style of the application
        /// </summary>
        DesignTimeFloating 
    }
}
