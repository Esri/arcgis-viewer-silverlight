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
using System.ComponentModel;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.GP
{
    public class SingleWindow
    {
        static SingleWindow current;
        public static SingleWindow Current
        {
            get
            {
                if (current == null)
                    current = new SingleWindow();
                return current;
            }
        }

        public SingleWindow()
        {
            windowsBeingShown = new List<FrameworkElement>();
        }
        List<FrameworkElement> windowsBeingShown;
        public void ShowWindow(MapApplication mapApplication, string windowTitle, FrameworkElement windowContents, bool isModal = false, EventHandler<CancelEventArgs> onHidingHandler = null, EventHandler onHideHandler = null)
        {
            if (! windowsBeingShown.Contains(windowContents))
            {
                mapApplication.ShowWindow(windowTitle, windowContents, isModal, onHidingHandler, (sender, args) =>
                {
                    if (onHideHandler != null)
                        onHideHandler(sender, args);
                    windowsBeingShown.Remove(windowContents); 
                });
                windowsBeingShown.Add(windowContents);
            }
        }

    }
}
