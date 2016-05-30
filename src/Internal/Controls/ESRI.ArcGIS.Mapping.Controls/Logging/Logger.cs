/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Diagnostics;
using ESRI.ArcGIS.Mapping.Windowing;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class Logger
    {
        protected Logger() { }

        private static Logger _instance;
        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Logger();
                return _instance;
            }
        }

        public void LogError(Exception ex, string message = null)
        {
            if (ex == null)
                return;

            string stackTraceIL = ex.StackTraceIL();
            if (!string.IsNullOrEmpty(stackTraceIL))
                LogError(string.Format("{0}\n\n{1}", message ?? ex.Message, stackTraceIL));
            else
                LogError(string.Format("{0}\n\n{1}", message ?? ex.Message, ex.StackTrace));
        }

        public void LogError(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            if (System.Windows.Browser.HtmlPage.IsEnabled)
            {
                System.Windows.Browser.HtmlWindow window = System.Windows.Browser.HtmlPage.Window;
                var isConsoleAvailable = (bool)window.Eval("typeof(console) != 'undefined' && typeof(console.log) != 'undefined'");
                if (isConsoleAvailable)
                {
                    var console = (window.Eval("console.log") as System.Windows.Browser.ScriptObject);
                    if (console != null)
                    {
                        try
                        {
                            console.InvokeSelf(message);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
