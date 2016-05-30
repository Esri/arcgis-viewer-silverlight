/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Text;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Tasks;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.GP.ParameterSupport
{
    public class MultiValueStringParameter : ParameterBase
    {
        /// <summary>
        /// Sets the parameter configuration, and Value.  If we
        /// don't have enough information in the DefaultValue or
        /// ChoiceList, we return false
        /// </summary>
        /// <param name="config"></param>
        /// <returns>True = success</returns>
        public bool SetConfiguration(MultiValueStringConfig config)
        {
            Config = config as ParameterConfig;

            // Determine what the Value collection should be
            //
            // The rule is: if there is a ChoiceList, then that is
            // what is used for the working Value.  Otherwise, the 
            // DefaultValue is used if that is valid, otherwise we 
            // fail and return false.
            if (config.ChoiceList != null && config.ChoiceList.Count > 0)
            {
                List<GPString> list = new List<GPString>();
                foreach (Choice choice in config.ChoiceList)
                {
                    list.Add(choice.Value as GPString);
                }
                Value = new GPMultiValue<GPString>(config.Name, list);
                return true;
            }
            else if (config.DefaultValue != null)
            {
                Value = config.DefaultValue;
                return true;
            }
            return false;
        }

        #region GPMultiValue<GPString>

        /// <summary>
        /// List of currently enabled strings
        /// </summary>
        private List<GPString> _multiValue = null;

        private GPMultiValue<GPString> val
        {
            get
            {
                return Value as GPMultiValue<GPString>;
            }
            set
            {
                Value = value;
            }
        }

        #endregion

        public override void AddUI(Grid grid)
        {
            if (Config != null && Config.ShownAtRunTime)
            {
                grid.RowDefinitions.Add(new RowDefinition());

                var g = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(4, 5, 0, 12) };

                foreach (GPString choice in val.Value)
                {
                    var cb = new CheckBox
                                 {
                                     // default is not checked for all multivalue choices
                                     IsChecked = false,
                                     // tag holds the GPString
                                     Tag = choice
                                 };

                    var item = new TextBlock
                                   {
                                       Text = choice.Value,
                                       TextTrimming = TextTrimming.WordEllipsis,
                                       HorizontalAlignment = HorizontalAlignment.Left,
                                   };
                    ToolTipService.SetToolTip(item, choice.Value);
                    cb.Content = item;

                    cb.Checked += (s, e) =>
                                      {
                                          UpdateValue(cb.IsChecked, cb.Tag);
                                          RaiseCanExecuteChanged();
                                      };
                    cb.Unchecked += (s, e) =>
                                        {
                                            UpdateValue(cb.IsChecked, cb.Tag);
                                            RaiseCanExecuteChanged();
                                        };
                    g.Children.Add(cb);
                }

                // add to the last row (added above)
                g.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                g.SetValue(Grid.ColumnProperty, 0);
                g.SetValue(Grid.ColumnSpanProperty, 2);
                grid.Children.Add(g);
                RaiseCanExecuteChanged();
            }
        }

        private void UpdateValue(bool? addValue, object choiceValue)
        {
            var changedValue = choiceValue as GPString;
            if (changedValue == null)
                return;

            if (_multiValue == null)
                _multiValue = new List<GPString>();

            bool adding = (addValue.HasValue && addValue.Value);
            // check if the string in in the Value collection
            GPString obj = null;
            bool exists = false;
            foreach (GPString item in _multiValue)
            {
                if (item.Value.Equals(changedValue.Value))
                {
                    obj = item;
                    exists = true;
                    break;
                }
            }
            if (adding)
            {
                if (!exists)
                    _multiValue.Add(changedValue);
            }
            else
            {
                if (exists)
                    _multiValue.Remove(obj);
            }

            val = new GPMultiValue<GPString>(val.Name, _multiValue);
        }
    }
}
