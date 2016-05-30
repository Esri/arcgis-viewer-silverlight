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
using System.Diagnostics;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Portal;

namespace ESRI.ArcGIS.Mapping.Windowing
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Gets the stack trace with IL offsets
        /// </summary>
        public static string StackTraceIL(this Exception ex)
        {
            // Need to use IL offsets as proxy for line numbers in stack trace in Silverlight.  Taken from
            // http://liviutrifoi.wordpress.com/2011/04/21/silverlight-stack-trace-line-numbers/
            string stackTrace = null;
            try
            {
                StackTrace st = new System.Diagnostics.StackTrace(ex);

                if (st != null)
                {
                    foreach (StackFrame frame in st.GetFrames())
                    {
                        stackTrace = "at " + frame.GetMethod().Module.Name + "." + frame.GetMethod().ReflectedType.Name +
                                     "." + frame.GetMethod().Name + "  (IL offset: 0x" +
                                     frame.GetILOffset().ToString("x") + ")\n" + stackTrace;
                    }
                }
            }
            catch { }

            return stackTrace;
        }

        /// <summary>
        /// Extracts the domain of a URI
        /// </summary>
        public static string Domain(this Uri uri)
        {
            string host = uri.Host;
            string[] hostParts = host.Split('.');
            return hostParts.Length > 1 ? string.Format("{0}.{1}",
                hostParts[hostParts.Length - 2], hostParts[hostParts.Length - 1]) : host;
        }
    }
}
