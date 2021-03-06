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
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Behaviors
{
    public static class ApplicationHelper
    {
        //public static IApplication CurrentApplication
        //{
        //    get
        //    {
        //        try
        //        {
        //            if (CustomCurrentApplication != null)
        //                return CustomCurrentApplication;

        //            return Application.Current as IApplication;
        //        }
        //        catch
        //        {
        //            return null;
        //        }
        //    }
        //}

        //public static IApplication CustomCurrentApplication
        //{
        //    private get;
        //    set;
        //}

        public static IntPtr ApplicationHandle
        {
            get;
            set;
        }
    }
}
