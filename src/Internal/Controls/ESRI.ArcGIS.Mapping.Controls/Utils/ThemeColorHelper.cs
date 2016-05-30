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

namespace ESRI.ArcGIS.Mapping.Controls
{
    public static class ThemeColorHelper
    {
        public static void ApplyColorProperty(ApplicationColorSet applicationColorSet, Color Color, ThemeColorProperty Property)
        {
            if (applicationColorSet == null)
                return;

            switch (Property)
            {
                case ThemeColorProperty.BackgroundEndGradientColor:
                    applicationColorSet.BackgroundEndGradientColor = Color;
                    break;
                case ThemeColorProperty.BackgroundStartGradientColor:
                    applicationColorSet.BackgroundStartGradientColor = Color;
                    break;
                case ThemeColorProperty.AccentColor:
                    applicationColorSet.AccentColor = Color;
                    break;
                case ThemeColorProperty.AccentTextColor:
                    applicationColorSet.AccentTextColor = Color;
                    break;
                case ThemeColorProperty.BackgroundTextColor:
                    applicationColorSet.BackgroundTextColor = Color;
                    break;
                case ThemeColorProperty.SelectionColor:
                    applicationColorSet.SelectionColor = Color;
                    break;
                case ThemeColorProperty.SelectionOutlineColor:
                    applicationColorSet.SelectionOutlineColor = Color;
                    break;
            }
        }
    }

    public enum ThemeColorProperty
    {
        BackgroundEndGradientColor,
        BackgroundStartGradientColor,
        BackgroundTextColor,
        AccentColor,
        AccentTextColor,
        SelectionColor,       
        SelectionOutlineColor,
    }
}
