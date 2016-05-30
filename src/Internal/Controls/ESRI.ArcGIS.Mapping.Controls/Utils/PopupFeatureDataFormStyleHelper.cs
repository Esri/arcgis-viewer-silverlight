/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using ESRI.ArcGIS.Client.Application.Controls;

namespace ESRI.ArcGIS.Mapping.Controls
{
    internal class PopupFeatureDataFormStyleHelper
    {
        private PopupFeatureDataFormStyleHelper() { }

        private static PopupFeatureDataFormStyleHelper _instance;
        public static PopupFeatureDataFormStyleHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PopupFeatureDataFormStyleHelper();
                return _instance;
            }
        }

        public Style GetStyle(string styleName)
        {
            Style style = null;
            if (!string.IsNullOrEmpty(styleName))
            {
                if (LayoutManager.Current != null)
                    style = LayoutManager.Current.GetStyle(styleName);

                if (style == null)
                {
                    if (Application.Current != null &&
                        Application.Current.Resources != null &&
                        Application.Current.Resources.Contains(styleName))
                    {
                        style = Application.Current.Resources[styleName] as Style;
                    }

                    if (style == null && Resources != null)
                    {
                        if (Resources.Contains(styleName))
                            style = Resources[styleName] as Style;
                    }
                }
            }
            return style;
        }

        ResourceDictionary rd;
        private ResourceDictionary Resources
        {
            get
            {
                if (rd == null)
                {
                    rd = new ResourceDictionary
                             {
                                 Source = new Uri(
                                     "/ESRI.ArcGIS.Mapping.Controls;component/Themes/PopupFeatureDataFormStyle.Theme.xaml",
                                     UriKind.RelativeOrAbsolute)
                             };
                }

                return rd;
            }
        }
    }
}
