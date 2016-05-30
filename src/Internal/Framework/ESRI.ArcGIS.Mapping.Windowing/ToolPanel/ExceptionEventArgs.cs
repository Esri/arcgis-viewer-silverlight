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

namespace ESRI.ArcGIS.Client.Application.Controls
{
    public class CoreExceptionEventArgs : EventArgs
    {
        public CoreExceptionEventArgs() { }
        public CoreExceptionEventArgs(string error, object userState) : this(new Exception(error), userState) { }
        public CoreExceptionEventArgs(Exception ex, object userState)
        {
            Exception = ex;
            UserState = userState;
        }

        public Exception Exception { get; set; }
        public object UserState { get; set; }
    }
}
