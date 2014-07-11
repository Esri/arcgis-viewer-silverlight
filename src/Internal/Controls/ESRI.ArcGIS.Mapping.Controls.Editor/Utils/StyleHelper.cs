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

namespace ESRI.ArcGIS.Mapping.Controls.Editor
{
    internal class StyleHelper
    {
        private StyleHelper() { }

        private static StyleHelper _instance;
        public static StyleHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StyleHelper();
                return _instance;
            }
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
                    if (System.Windows.Application.Current != null &&
                        System.Windows.Application.Current.Resources != null &&
                        System.Windows.Application.Current.Resources.Contains(styleName))
                    {
                        return System.Windows.Application.Current.Resources[styleName] as Style;
                    }
                }
            }
            return null;
        }

        ResourceDictionary rd;
        private ResourceDictionary Resources
        {
            get
            {
                if (rd == null)
                {
                    rd = new ResourceDictionary();
                    rd.Source = new Uri("/ESRI.ArcGIS.Mapping.Controls.Editor;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute);
                }

                return rd;
            }
        }
    }
}
