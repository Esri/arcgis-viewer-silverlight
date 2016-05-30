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
using ESRI.ArcGIS.Mapping.GP.ParameterSupport;
using System.Windows.Data;

namespace ESRI.ArcGIS.Mapping.GP
{
    internal class GPConfigUIBuilder
    {
        internal static ContentControl GenerateParameterUI(List<ParameterConfig> parameters)
        {
            ContentControl paramUI = new ContentControl()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };

            Grid paramContainer = new Grid();
            paramContainer.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            paramContainer.ColumnDefinitions.Add(new ColumnDefinition());

            foreach (ParameterConfig parameter in parameters)
                parameter.AddConfigUI(paramContainer);

            paramUI.Content = paramContainer;
            return paramUI;
        }
    }
}
