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
using ESRI.ArcGIS.Mapping.Builder.Common;

namespace ESRI.ArcGIS.Mapping.Builder.Service
{
    public class AsyncCompletedEventArgs : EventArgs
    {
        public bool Cancelled { get; set; }
        //
        // Summary:
        //     Gets a value that indicates which error occurred during an asynchronous operation.
        //
        // Returns:
        //     An System.Exception instance, if an error occurred during an asynchronous
        //     operation; otherwise null.
        public Exception Error { get; set; }
        //
        // Summary:
        //     Gets the unique identifier for the asynchronous task.
        //
        // Returns:
        //     An object reference that uniquely identifies the asynchronous task; otherwise,
        //     null if no value has been set.
        public object UserState { get; set; }
    }
}
