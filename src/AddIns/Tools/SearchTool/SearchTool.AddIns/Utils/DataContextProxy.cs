/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SearchTool
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
            Loaded += new RoutedEventHandler(DataContextProxy_Loaded);
        }

        void DataContextProxy_Loaded(object sender, RoutedEventArgs e)
        {
            Binding binding = new Binding();
            if (!String.IsNullOrEmpty(BindingPropertyName))
            {
                binding.Path = new PropertyPath(BindingPropertyName);
            }
            binding.Source = DataContext;
            binding.Mode = BindingMode;
            SetBinding(DataContextProxy.DataSourceProperty, binding);
        }

        public object DataSource
        {
            get { return GetValue(DataSourceProperty) as object; }
            set { SetValue(DataSourceProperty, value); }
        }

        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register("DataSource", typeof(object), typeof(DataContextProxy), null);


        public string BindingPropertyName { get; set; }

        public BindingMode BindingMode { get; set; }

    }
}
