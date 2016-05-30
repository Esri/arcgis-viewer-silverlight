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
using ESRI.ArcGIS.Client.Tasks;
namespace ESRI.ArcGIS.Mapping.GP.ParameterSupport
{
    public class RasterParameter : ParameterBase
    {
        GPRasterData value
        {
            get { return Value as GPRasterData; }
        }
        public override void AddUI(Grid grid)
        {
            if (Config != null && Config.ShownAtRunTime)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                #region
                StackPanel g = new StackPanel() { Orientation = System.Windows.Controls.Orientation.Horizontal, Margin=new Thickness(5,0,0,0) };

                TextBlock lblUrl = new TextBlock() { VerticalAlignment = System.Windows.VerticalAlignment.Center, Margin = new Thickness(2) };
                lblUrl.Text = Resources.Strings.LabelUrl;
                g.Children.Add(lblUrl);

                TextBox tbFormat = new TextBox() { Margin = new Thickness(4,2,0,2), Width = 50, MaxWidth= 60 };
                TextBox tb = new TextBox() { 
                    Margin = new Thickness(2), 
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                tb.SetValue(ToolTipService.ToolTipProperty, Config.ToolTip);
                if (value != null)
                    tb.Text = value.Url.ToString();

                tb.TextChanged += (s, e) =>
                {
                    if (value == null)
                        Value = new GPRasterData(Config.Name, tb.Text, tbFormat.Text) { Format = tbFormat.Text };//Workaround for slapi bug
                    else
                        value.Url = tb.Text;
                    RaiseCanExecuteChanged();
                };
                g.Children.Add(tb);

                TextBlock lbl = new TextBlock() { VerticalAlignment = System.Windows.VerticalAlignment.Center, Margin = new Thickness(2) };
                lbl.Text = Resources.Strings.LabelFormat;
                g.Children.Add(lbl);

                if (Config is RasterDataParameterConfig)
                    tbFormat.SetValue(ToolTipService.ToolTipProperty, (Config as RasterDataParameterConfig).FormatToolTip);
                if (value != null)
                    tbFormat.Text = value.Format.ToString();
                tbFormat.TextChanged += (s, e) =>
                {
                    if (value == null)
                        Value = new GPRasterData(Config.Name, tb.Text, tbFormat.Text) { Format = tbFormat.Text };//Workaround for slapi bug
                    else
                        value.Format = tbFormat.Text;
                    RaiseCanExecuteChanged();
                };
                g.Children.Add(tbFormat);

                g.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                g.SetValue(Grid.ColumnProperty, 0);
                g.SetValue(Grid.ColumnSpanProperty, 2);
                grid.Children.Add(g);
                RaiseCanExecuteChanged();
                #endregion
            }
        }


    }
}
