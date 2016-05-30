/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ESRI.ArcGIS.Mapping.GP.ParameterSupport
{
    public class RasterDataParameterConfig : ParameterConfig
    {
        public string FormatToolTip { get; set; }
        protected override void AddToJsonDictionary(ref Dictionary<string, object> dictionary)
        {
            base.AddToJsonDictionary(ref dictionary);
            dictionary.Add("formatToolTip", FormatToolTip);
        }

        protected override void FromJsonDictionary(IDictionary<string, object> dictionary)
        {
            base.FromJsonDictionary(dictionary);
            if (dictionary.ContainsKey("formatToolTip"))
                FormatToolTip = dictionary["formatToolTip"] as string;
        }

        public override void AddConfigUI(Grid grid)
        {
            TextBlock label;
            #region Header
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.RowDefinitions.Add(new RowDefinition());
            label = new TextBlock()
            {
                Text = DisplayName ?? Name,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new Thickness(2),
                FontWeight = FontWeights.Bold,
                TextTrimming = TextTrimming.WordEllipsis
            };
            g.Children.Add(label);

            if (Required)
            {
                label = new TextBlock()
                {
                    Text = Resources.Strings.Required,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    Margin = new Thickness(2),
                    FontWeight = FontWeights.Light,
                    FontStyle = FontStyles.Italic
                };
                label.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(label);
            }
            g.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            g.SetValue(Grid.ColumnSpanProperty, 2);
            grid.Children.Add(g);
            #endregion

            #region Type
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            label = new TextBlock()
            {
                Text = Resources.Strings.LabelType,
                Margin = new Thickness(2),
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            label.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            grid.Children.Add(label);
            label = new TextBlock()
            {
                Text = Type.ToString(),
                Margin = new Thickness(2),
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            label.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            label.SetValue(Grid.ColumnProperty, 1);
            grid.Children.Add(label);
            #endregion

            #region Label
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            label = new TextBlock()
            {
                Text = Resources.Strings.LabelLabel,
                Margin = new Thickness(2),
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            label.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            grid.Children.Add(label);
            TextBox labelTextBox = new TextBox()
            {
                Text = Label == null ? string.Empty : Label,
                Margin = new Thickness(2),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
            };
            labelTextBox.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
            labelTextBox.SetValue(Grid.ColumnProperty, 1);
            grid.Children.Add(labelTextBox);
            labelTextBox.TextChanged += (s, e) =>
            {
                Label = labelTextBox.Text;
            };
            #endregion

            if (Input)
            {
                #region Tooltip
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                label = new TextBlock()
                {
                    Text = Resources.Strings.LabelTooltip,
                    Margin = new Thickness(2),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                label.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                grid.Children.Add(label);
                TextBox tbToolTip = new TextBox()
                {
                    Margin = new Thickness(2),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                };
                if (!string.IsNullOrEmpty(ToolTip))
                    tbToolTip.Text = ToolTip;
                tbToolTip.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                tbToolTip.SetValue(Grid.ColumnProperty, 1);
                grid.Children.Add(tbToolTip);
                tbToolTip.TextChanged += (s, e) =>
                {
                    ToolTip = tbToolTip.Text;
                };
                #endregion

                #region Format Tooltip
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                label = new TextBlock()
                {
                    Text = Resources.Strings.LabelFormatTooltip,
                    Margin = new Thickness(2),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                label.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                grid.Children.Add(label);
                TextBox tbFormatToolTip = new TextBox()
                {
                    Margin = new Thickness(2),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                };
                if (!string.IsNullOrEmpty(FormatToolTip))
                    tbFormatToolTip.Text = FormatToolTip;
                tbFormatToolTip.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                tbFormatToolTip.SetValue(Grid.ColumnProperty, 1);
                grid.Children.Add(tbFormatToolTip);
                tbFormatToolTip.TextChanged += (s, e) =>
                {
                    FormatToolTip = tbFormatToolTip.Text;
                };
                #endregion

                #region ShownAtRuntime
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                label = new TextBlock()
                {
                    Text = Resources.Strings.LabelShowParameter,
                    Margin = new Thickness(2),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                label.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                grid.Children.Add(label);
                CheckBox chb = new CheckBox() { IsChecked = ShownAtRunTime, Margin = new Thickness(2) };
                chb.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                chb.SetValue(Grid.ColumnProperty, 1);
                grid.Children.Add(chb);
                chb.Checked += (s, e) => { ShownAtRunTime = true; };
                chb.Unchecked += (s, e) => { ShownAtRunTime = false; };
                #endregion

                #region Default Value
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                label = new TextBlock()
                {
                    Text = Resources.Strings.LabelDefaultValue,
                    Margin = new Thickness(2),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                label.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                grid.Children.Add(label);
                ParameterBase paramUI = ParameterFactory.Create(this, null);
                paramUI.AddUI(grid);
                paramUI.CanExecuteChanged += (a, b) =>
                {
                    this.DefaultValue = paramUI.Value;
                };
                #endregion
            }
        }
    }
}
