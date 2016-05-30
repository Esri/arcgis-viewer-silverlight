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
using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client.Application.Controls;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class LayoutStyleHelper : ILayerStyleManager
    {
        private LayoutStyleHelper() { }

        private static LayoutStyleHelper _instance;
        public static LayoutStyleHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LayoutStyleHelper();
                    LayoutManager.Current = _instance;
                }
                return _instance;
            }
        }

        public Style GetPopupStyle(string containerName, PopupStyleName style)
        {
            if (!string.IsNullOrEmpty(containerName))
            {
                string resourceKey = containerName + "_" + style.ToString();
                if (Resources != null && Resources.Contains(resourceKey))
                    return Resources[resourceKey] as Style;
            }

            if (Resources != null && Resources.Contains(style.ToString()))
                return Resources[style.ToString()] as Style;

            if(Application.Current != null && Application.Current.Resources.Contains(style.ToString()))
                return Application.Current.Resources[style.ToString()] as Style;

            return null;
        }

        public Style GetStyle(string styleName)
        {
            if (!string.IsNullOrEmpty(styleName))
            {
                if (Resources != null)
                {
                    if (Resources.Contains(styleName))
                        return Resources[styleName] as Style;
                }
                else
                {
                    if (Application.Current != null &&
                        Application.Current.Resources != null &&
                        Application.Current.Resources.Contains(styleName))
                    {
                        return Application.Current.Resources[styleName] as Style;
                    }
                }
            }
            return null;
        }

        private ResourceDictionary Resources 
        {
            get
            {
                if(ViewerApplicationControl.Instance != null && ViewerApplicationControl.Instance.LayoutElement != null)
                    return ViewerApplicationControl.Instance.LayoutElement.Resources;

                return null;
            }
        }
    }
}
