/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Mapping.Core;
using System.Windows.Media;

namespace ESRI.ArcGIS.Mapping.GP.ParameterSupport
{
    public class SimpleParameter : ParameterBase
    {
        public override void AddUI(Grid grid)
        {
            if (Config != null && Config.ShownAtRunTime)
            {
                if (Config.Type == GPParameterType.String && Config.ChoiceList != null &&
                    Config.ChoiceList.Count > 1)
                {
                    #region Create combo box
                    ComboBox cb = new ComboBox()
                    {
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        Width = 125,
                        Height = 24,
                        Margin = new Thickness(2),
                        Foreground = new SolidColorBrush(Colors.Black)
                    };
                    #region Populate items
                    foreach (Choice choice in Config.ChoiceList)
                    {
                        TextBlock item = new TextBlock()
                            {
                                Text = choice.DisplayText,
                                Width = 210,
                                TextTrimming = System.Windows.TextTrimming.WordEllipsis,
                                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                                Tag = choice.Value
                            };
                        ToolTipService.SetToolTip(item, choice.DisplayText);
                        GPString value = Value as GPString;
                        GPString choiceValue = choice.Value as GPString;
                        cb.Items.Add(item);
                        if (value != null && choiceValue != null && value.Value == choiceValue.Value)
                        {
                            cb.SelectedIndex = cb.Items.Count - 1;
                            cb.SelectedItem = item;
                        }
                    }
                    #endregion
                    cb.SelectionChanged += (a, b) =>
                    {
                        Value = (GPParameter)((cb.SelectedItem as TextBlock).Tag);
                        RaiseCanExecuteChanged();
                    };
                    cb.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                    cb.SetValue(Grid.ColumnProperty, 1);
                    cb.SetValue(ToolTipService.ToolTipProperty, Config.ToolTip);
                    grid.Children.Add(cb);
                    #endregion
                }
                else
                {
                    #region
                    string text = ParameterToString(Config.Type, Value, CultureHelper.GetCurrentCulture());
                    TextBox tb = new TextBox()
                    {
                        Text = text  == null ? string.Empty : text,
                        Margin = new Thickness(2),
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch
                    };
                    tb.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                    tb.SetValue(Grid.ColumnProperty, 1);
                    tb.SetValue(ToolTipService.ToolTipProperty, Config.ToolTip);
                    grid.Children.Add(tb);
                    tb.TextChanged += (s, e) =>
                    {
                        Value = StringToParameter(Config.Name, Config.Type, tb.Text, CultureHelper.GetCurrentCulture());
                        RaiseCanExecuteChanged();
                    };
                    #endregion
                }
                RaiseCanExecuteChanged();
            }
        }


    }
}
