/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.ComponentModel;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Client.Application.Layout;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class WindowManager : DependencyObject
    {
        public static string FloatingWindowStyleKey = "WindowStyle";

        private Dictionary<int, FloatingWindow> m_FloatingWindows = new Dictionary<int, FloatingWindow>();        

        /// <summary>
        /// Displays content in a window
        /// </summary>
        /// <param name="windowTitle">Title text shown in window</param>
        /// <param name="windowContents">Contents of the window</param>        
        /// <param name="isModal">Determines wheter the window is modal or not</param>
        /// <param name="onClosingHandler">Event handler invoked when window is closing, and can be handled to cancel window closure</param>
        /// <param name="windowType">The type of the window</param>
        /// <param name="onClosedHandler">Event handler invoked when window is closed</param>
        /// <param name="top">The distance from the top of the application at which to position the window</param>
        /// <param name="left">The distance from the left of the application at which to position the window</param>
        /// <returns>The window</returns>
        public object ShowWindow(string windowTitle, FrameworkElement windowContents, bool isModal = false, 
            EventHandler<CancelEventArgs> onClosingHandler = null, EventHandler onClosedHandler = null, 
            WindowType windowType = WindowType.Floating, double? top = null, double? left = null)            
        {                        
            if (windowContents == null)
                throw new ArgumentNullException("windowContents");

            int hashCode = windowContents.GetHashCode();
            FloatingWindow floatingWindow = null;
            if (!m_FloatingWindows.TryGetValue(hashCode, out floatingWindow))
            {
                // not existing yet
                floatingWindow = new FloatingWindow()
                {
                    Title = windowTitle ?? string.Empty,
                };

                switch (windowType)
                {
                    case WindowType.Floating:
                if (FloatingWindowStyle != null)
                    floatingWindow.Style = FloatingWindowStyle;
                        break;
                    case WindowType.DesignTimeFloating:
                        if (DesignTimeWindowStyle != null)
                            floatingWindow.Style = DesignTimeWindowStyle;
                        else if (FloatingWindowStyle != null) // fallback to FloatingWindowStyle
                            floatingWindow.Style = FloatingWindowStyle;
                        break;
                }

                floatingWindow.Closed += (o, e) =>
                {
                    if (onClosedHandler != null)
                        onClosedHandler.Invoke(o, e);

                    m_FloatingWindows.Remove(hashCode);

                    if (floatingWindow != null)
                        floatingWindow.Content = null;
                };

                if (onClosingHandler != null)
                    floatingWindow.Closing += onClosingHandler;
                floatingWindow.Content = windowContents;
                m_FloatingWindows.Add(hashCode, floatingWindow);
            }


            if (top != null)
                floatingWindow.VerticalOffset = (double)top;

            if (left != null)
                floatingWindow.HorizontalOffset = (double)left;

            floatingWindow.Show(isModal);

            return floatingWindow;
        }

        public Style FloatingWindowStyle
        {
            get;
            set;
        }

        public Style DesignTimeWindowStyle
        {
            get;
            set;
        }

        /// <summary>
        /// Hides the dialog window 
        /// </summary>
        /// <param name="windowContents">Contents of the window displayed earlier using ShowWindow</param>
        public void HideWindow(FrameworkElement windowContents)
        {
            if (windowContents == null)
                throw new ArgumentNullException("windowContents");

            int hashCode = windowContents.GetHashCode();
            FloatingWindow floatingWindow = null;
            if (m_FloatingWindows.TryGetValue(hashCode, out floatingWindow))
            {
                floatingWindow.Hide();
                // close will automatically remove the window from the hash table in floatingWindow_Closed
            }             
        }

        public void HideAllWindows()
        {
            if (m_FloatingWindows != null && m_FloatingWindows.Count > 0)
            {
                List<FloatingWindow> windowsToClose = new List<FloatingWindow>();
                foreach(FloatingWindow floatingWindow in m_FloatingWindows.Values)
                {
                    windowsToClose.Add(floatingWindow);
                }

                //close will automatically remove the window from the hash table in floatingWindow_Closed
                //close differs from hide by not executing closing storyboards (forced close)
                foreach(FloatingWindow window in windowsToClose)
                    window.Hide(false);
            }
        }
    }
}
