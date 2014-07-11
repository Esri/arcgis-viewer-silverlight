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

namespace ESRI.ArcGIS.Mapping.Controls
{
    public static class ViewHelper
    {
        public static View CurrentView
        {
            get
            {
                try
                {
                    if (CustomCurrentView != null)
                        return CustomCurrentView;

                    if (Application.Current == null)
                        return null;

                    View view = null;
                    IApplication application = Application.Current as IApplication;
                    if (application == null)
                    {
#if SILVERLIGHT
                        view = Application.Current.RootVisual as View;
#else

                        view = Application.Current.MainWindow.Content as View;
#endif
                    }
                    else
                        view = application.View;
                    return view;
                }
                catch
                {
                    return null;
                }
            }
        }

        public static View CustomCurrentView
        {
            get;
            set;
        }

        public static IntPtr ApplicationHandle
        {
            get;
            set;
        }
    }
}
