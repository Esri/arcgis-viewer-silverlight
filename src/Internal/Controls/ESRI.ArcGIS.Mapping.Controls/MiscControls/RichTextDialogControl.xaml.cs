/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public partial class RichTextDialogControl : UserControl
    {
        public RichTextDialogControl()
        {
            InitializeComponent();            
        }

        #region RichTextXaml
        /// <summary>
        /// 
        /// </summary>
        public string RichTextXaml
        {
            get { return GetValue(RichTextXamlProperty) as string; }
            set { SetValue(RichTextXamlProperty, value); }
        }

        /// <summary>
        /// Identifies the RichTextXaml dependency property.
        /// </summary>
        public static readonly DependencyProperty RichTextXamlProperty =
            DependencyProperty.Register(
                "RichTextXaml",
                typeof(string),
                typeof(RichTextDialogControl),
                new PropertyMetadata(null));
        #endregion
        
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            MapApplication.Current.HideWindow(this);
        }

        private void RichTextDialogControl_Loaded(object sender, RoutedEventArgs e)
        {            
            if (!string.IsNullOrWhiteSpace(RichTextXaml))
                RichTextBox.Xaml = string.Format("<Section xml:space=\"preserve\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">{0}</Section>", RichTextXaml); 
        }
    }
}
