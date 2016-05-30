/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PrintTool.AddIns
{
    /// <summary>
    /// The DataContextProxy class allows referencing a parent control's DataContext from within its children.
    /// This is useful, for instance, in binding to DataContext properties from within a DataTemplate.  Code
    /// courtesy of Dan Wahlin at http://weblogs.asp.net/dwahlin/archive/2009/08/20/creating-a-silverlight-datacontext-proxy-to-simplify-data-binding-in-nested-controls.aspx
    /// </summary>
    public class DataContextProxy : FrameworkElement
    {
        public DataContextProxy()
        {
            this.Loaded += new RoutedEventHandler(DataContextProxy_Loaded);
        }

        void DataContextProxy_Loaded(object sender, RoutedEventArgs e)
        {
            Binding binding = new Binding();
            if (!String.IsNullOrEmpty(BindingPropertyName))
            {
                binding.Path = new PropertyPath(BindingPropertyName);
            }
            binding.Source = this.DataContext;
            binding.Mode = BindingMode;
            this.SetBinding(DataContextProxy.DataSourceProperty, binding);
        }

        public Object DataSource
        {
            get { return (Object)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register("DataSource", typeof(Object), typeof(DataContextProxy), null);


        public string BindingPropertyName { get; set; }

        public BindingMode BindingMode { get; set; }

    }
}
