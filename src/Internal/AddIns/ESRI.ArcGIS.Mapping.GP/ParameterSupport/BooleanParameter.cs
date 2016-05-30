/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows.Controls;
using ESRI.ArcGIS.Client.Tasks;
using System;
using System.Windows;
namespace ESRI.ArcGIS.Mapping.GP.ParameterSupport
{
    public class BooleanParameter : ParameterBase
    {
        GPBoolean value
        {
            get
            {
                return Value as GPBoolean;
            }
        }
        public override void AddUI(Grid grid)
        {
            if (Config != null && Config.ShownAtRunTime)
            {
                CheckBox chb = new CheckBox() { IsChecked = value == null ? new bool?() : value.Value, Margin = new Thickness(2) };
                chb.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                chb.SetValue(Grid.ColumnProperty, 1);
                chb.SetValue(ToolTipService.ToolTipProperty, Config.ToolTip);
                grid.Children.Add(chb);
                chb.Checked += (s, e) => { value.Value = true; RaiseCanExecuteChanged(); };
                chb.Unchecked += (s, e) => { value.Value = false; RaiseCanExecuteChanged(); };
            }
        }
    }
}
