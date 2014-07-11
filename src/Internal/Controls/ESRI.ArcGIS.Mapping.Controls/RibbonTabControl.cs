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

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class RibbonTabControl : TabItem
    {
        private const string PART_ROOT = "Root";
        FrameworkElement root = null;
        public RibbonTabControl()
        {
            DefaultStyleKey = typeof(RibbonTabControl);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            root = GetTemplateChild(PART_ROOT) as FrameworkElement;
            if (root != null)
            {
                ControlTemplate template = root.Resources["ContentTemplateStyle"] as ControlTemplate;
                ContentControl elem = Content as ContentControl;
                if (elem != null)
                    elem.Template = template;
            }
        }
    }
}
