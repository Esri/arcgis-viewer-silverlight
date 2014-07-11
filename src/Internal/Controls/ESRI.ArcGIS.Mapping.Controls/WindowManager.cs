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
using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.Core;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class WindowManager : DependencyObject
    {        
        private Dictionary<int, ChildWindow> m_ChildWindows = new Dictionary<int, ChildWindow>();        

        /// <summary>
        /// Displays content in a window
        /// </summary>
        /// <param name="windowTitle">Title text shown in window</param>
        /// <param name="windowContents">Contents of the window</param>        
        /// <param name="isModal">Determines wheter the window is modal or not</param>
        /// <param name="onClosingHandler">Event handler invoked when window is closing, and can be handled to cancel window closure</param>
        /// <param name="onClosedHandler">Event handler invoked when window is closed</param>
        public void ShowWindow(string windowTitle, FrameworkElement windowContents, bool isModal = false, EventHandler<CancelEventArgs> onClosingHandler = null, EventHandler onClosedHandler = null)            
        {                        
            if (windowContents == null)
                throw new ArgumentNullException("windowContents");
            
            int hashCode = windowContents.GetHashCode();
            ChildWindow childWindow = null;
            if (!m_ChildWindows.TryGetValue(hashCode, out childWindow))
            {
                // not existing yet
                childWindow = new ChildWindow()
                {
                    Title = windowTitle ?? string.Empty,
                };                
                if (onClosedHandler != null)
                    childWindow.Closed += onClosedHandler;
                if (onClosingHandler != null)
                    childWindow.Closing += onClosingHandler;
                childWindow.CloseCompleted += new EventHandler(childWindow_CloseCompleted);
                childWindow.Content = windowContents;
                m_ChildWindows.Add(hashCode, childWindow);
                if (!isModal) 
                { 
                    windowContents.Loaded += (o, e) => {
                        DialogResizeHelper.CenterAndSizeDialog(windowContents, childWindow);
                    };
                    DialogResizeHelper.CenterAndSizeDialog(windowContents, childWindow);
                }
            }
            childWindow.ShowDialog(isModal);
        }

        void childWindow_CloseCompleted(object sender, EventArgs e)
        {
            // remove the window from hash table
            ChildWindow childWindow = sender as ChildWindow;
            if (childWindow != null && childWindow.Content != null)
            {
                int hashCode = childWindow.Content.GetHashCode();
                m_ChildWindows.Remove(hashCode);
                childWindow.Content = null;
            }
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
            ChildWindow childWindow = null;
            if (m_ChildWindows.TryGetValue(hashCode, out childWindow))
            {
                childWindow.Close();
                // close will automatically remove the window from the hash table in childWindow_Closed
            }             
        }                
    }
}
