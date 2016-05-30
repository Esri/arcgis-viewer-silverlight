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
    /// Arguments passed to the callback for the <see cref="IMapApplication.LoadMap"/> method
    /// </summary>
    public class LoadMapCompletedEventArgs
    {
        /// <summary>
        /// Contains the exception thrown if LoadMap encounters an error
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// Contains the user-specified object to pass through to the callback method
        /// </summary>
        public object UserState { get; set; }
    }
}
