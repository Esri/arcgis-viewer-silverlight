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
using ESRI.ArcGIS.Client.Application.Layout.Converters;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class StringResourcesManager : LocalizationConverter
    {
        static StringResourcesManager instance;
        public static StringResourcesManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new StringResourcesManager();
                return instance;
            }
        }

        public override System.Reflection.Assembly Assembly
        {
            get { return typeof(StringResourcesManager).Assembly; }
        }

        public override string ResourceFileName
        {
            get { return "ESRI.ArcGIS.Mapping.Controls.Resources.Strings"; }
        }
    }
}
