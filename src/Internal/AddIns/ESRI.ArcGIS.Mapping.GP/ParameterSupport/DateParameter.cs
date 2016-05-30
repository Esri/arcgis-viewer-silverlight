/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows.Controls;
using ESRI.ArcGIS.Client.Tasks;

namespace ESRI.ArcGIS.Mapping.GP.ParameterSupport
{
    public class DateParameter : ParameterBase
    {
        GPDate value
        {
            get
            {
                GPDate date = Value as GPDate;

                // Assign "universal" date format that works for all locales with hours in military format so that it
                // does not require AM/PM which does not work in asian languages. This is a temporary fix until something
                // more permanent can be done in the Silverlight API.
                if (date != null)
                    date.Format = "M/d/yyyy HH:mm:ss";

                return date;
            }
        }
        public override void AddUI(Grid grid)
        {
            if (Config != null && Config.ShownAtRunTime)
            {
                #region
                DateTimePicker dp = new DateTimePicker() { Value = value.Value };
                dp.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                dp.SetValue(Grid.ColumnProperty, 1);
                dp.SetValue(ToolTipService.ToolTipProperty, Config.ToolTip);
                grid.Children.Add(dp);
                dp.ValueChanged += (a, b) =>
                     {
                         Value = new GPDate(Config.Name, dp.Value);
                         GPDate date = Value as GPDate;

                         // Assign "universal" date format that works for all locales with hours in military format so that it
                         // does not require AM/PM which does not work in asian languages. This is a temporary fix until something
                         // more permanent can be done in the Silverlight API.
                         if (date != null)
                             date.Format = "M/d/yyyy HH:mm:ss";

                         RaiseCanExecuteChanged();
                     };
                #endregion
            }
        }

        public object GetDefaultValue()
        {
            if (Config != null && !Config.ShownAtRunTime)
                return Config.DefaultValue;
            return null;
        }
    }
}
