/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class ExceptionEventArgs : EventArgs
    {
        public ExceptionEventArgs(string error, object userState) : this(new Exception(error), userState) { }
        public ExceptionEventArgs(Exception ex, object userState = null, int statusCode = -1)
        {
            Exception = ex;
            UserState = userState;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the exception that occurred during the event
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Gets the state object associated with the event
        /// </summary>
        public object UserState { get; private set; }

        /// <summary>
        /// Gets the HTTP status code of the error, if applicable
        /// </summary>
        public int StatusCode { get; private set; }
    }
}
