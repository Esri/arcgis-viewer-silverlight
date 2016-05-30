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
using ESRI.ArcGIS.Mapping.GP.Resources;
using System.Windows.Media;

namespace ESRI.ArcGIS.Mapping.GP.ParameterSupport
{
    public class LinearUnitParameter : ParameterBase
    {
        GPLinearUnit value
        {
            get
            {
                return Value as GPLinearUnit;
            }
        }

        public override void AddUI(Grid grid)
        {
            if (Config != null && Config.ShownAtRunTime)
            {
                #region
                StackPanel panel = new StackPanel()
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left
                };
                TextBox tb = new TextBox() { Margin = new Thickness(2), Width = 50 };
                tb.SetValue(Grid.ColumnProperty, 0);
                tb.SetValue(ToolTipService.ToolTipProperty, Config.ToolTip);
               if (value != null)
                    tb.Text = value.Distance.ToString();
               tb.TextChanged += (s, e) =>
               {
                   double val = double.NaN;
                   if (double.TryParse(tb.Text, System.Globalization.NumberStyles.Any, CultureHelper.GetCurrentCulture(), out val))
                   {
                       if (value == null)
                           Value = new GPLinearUnit(Config.Name, esriUnits.esriUnknownUnits, val);
                       else
                           value.Distance = val;
                   }
                   else
                   {
                       if (value == null)
                           Value = new GPLinearUnit(Config.Name, esriUnits.esriUnknownUnits, double.NaN);
                       else
                           value.Distance = double.NaN;
                   }
                   RaiseCanExecuteChanged();
               };
               panel.Children.Add(tb);

               ComboBox cb = new ComboBox()
               {
                   HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                   Width = 125,
                   Height = 24,
                   Margin = new Thickness(2),
                   Foreground = new SolidColorBrush(Colors.Black)
               };
                cb.SetValue(Grid.ColumnProperty, 1);
                cb.Items.Add(new TextBlock() { Text = Strings.Unknown, Tag = esriUnits.esriUnknownUnits});
                cb.Items.Add(new TextBlock() { Text = Strings.Inches, Tag = esriUnits.esriInches });
                cb.Items.Add(new TextBlock() { Text = Strings.Points, Tag = esriUnits.esriPoints });
                cb.Items.Add(new TextBlock() { Text = Strings.Feet, Tag = esriUnits.esriFeet });
                cb.Items.Add(new TextBlock() { Text = Strings.Yards, Tag = esriUnits.esriYards });
                cb.Items.Add(new TextBlock() { Text = Strings.Miles, Tag = esriUnits.esriMiles });
                cb.Items.Add(new TextBlock() { Text = Strings.NauticalMiles, Tag = esriUnits.esriNauticalMiles });
                cb.Items.Add(new TextBlock() { Text = Strings.Millimeters, Tag = esriUnits.esriMillimeters });
                cb.Items.Add(new TextBlock() { Text = Strings.Centimeters, Tag = esriUnits.esriCentimeters });
                cb.Items.Add(new TextBlock() { Text = Strings.Meters, Tag = esriUnits.esriMeters });
                cb.Items.Add(new TextBlock() { Text = Strings.Kilometers, Tag = esriUnits.esriKilometers });
                cb.Items.Add(new TextBlock() { Text = Strings.DecimalDegrees, Tag = esriUnits.esriDecimalDegrees });
                cb.Items.Add(new TextBlock() { Text = Strings.Decimeters, Tag = esriUnits.esriDecimeters });

                TextBlock item;                
                for (int i = 0; i < cb.Items.Count; i++)
                {
                    item = cb.Items[i] as TextBlock;
                    if (((esriUnits)item.Tag) == value.Unit)
                    {
                        cb.SelectedIndex = i;
                        cb.SelectedItem = item;
                    }
                }
                cb.SelectionChanged += (a, b) =>
                    {
                        value.Unit = (esriUnits)((cb.SelectedItem as TextBlock).Tag);
                        RaiseCanExecuteChanged();
                    };
                panel.Children.Add(cb);
                panel.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                panel.SetValue(Grid.ColumnProperty, 1);
                grid.Children.Add(panel);
                #endregion
                RaiseCanExecuteChanged();
            }
        }

    }
}
