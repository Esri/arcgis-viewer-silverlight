/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace ESRI.ArcGIS.Mapping.Viewer
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.Name);
            Loaded += (s, e) => { FlowDirection = FlowDirection.LeftToRight; };
        }
    }
}
