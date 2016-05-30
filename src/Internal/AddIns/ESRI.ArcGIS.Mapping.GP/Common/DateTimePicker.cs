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

namespace ESRI.ArcGIS.Mapping.GP
{
    [TemplatePart(Name = PART_DatePicker, Type = typeof(DatePicker))]
    [TemplatePart(Name = PART_TimePicker, Type = typeof(TimePicker))]
    public class DateTimePicker : Control
    {
        private const string PART_DatePicker = "DatePicker";
        private const string PART_TimePicker = "TimePicker";
        DatePicker datePicker;
        TimePicker timePicker;
        public DateTimePicker()
        {
            DefaultStyleKey = typeof(DateTimePicker);
        }

        public DateTime Value
        {
            get { return (DateTime)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(DateTime), typeof(DateTimePicker), new PropertyMetadata(DateTime.Now, OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DateTimePicker obj = (DateTimePicker)d;
            DateTime val = obj.Value;
            if (obj.datePicker != null)
                obj.datePicker.SelectedDate = val;
            if (obj.timePicker != null)
                obj.timePicker.Value = val;
        }
        public event EventHandler ValueChanged;

        public override void OnApplyTemplate()
        {
            if (datePicker != null)
                datePicker.SelectedDateChanged -= datePicker_SelectedDateChanged;
            if (timePicker != null)
                timePicker.ValueChanged -= timePicker_ValueChanged;

            base.OnApplyTemplate();

            datePicker = GetTemplateChild(PART_DatePicker) as DatePicker;
            timePicker = GetTemplateChild(PART_TimePicker) as TimePicker;

            if (datePicker != null)
            {
                datePicker.SelectedDate = Value;
                datePicker.SelectedDateChanged += datePicker_SelectedDateChanged;
            }
            if (timePicker != null)
            {
                timePicker.Value = Value;
                timePicker.ValueChanged += timePicker_ValueChanged;
            }
        }

        void timePicker_ValueChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            updateDateTime();
        }

        void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            updateDateTime();
        }

        void updateDateTime()
        {
            if (datePicker != null && timePicker != null)
            {
                if (datePicker.SelectedDate != null && datePicker.SelectedDate.HasValue)
                {
                    DateTime date = datePicker.SelectedDate.Value.Date;

                    if (timePicker.Value != null && timePicker.Value.HasValue)
                    {
                        Value = date + timePicker.Value.Value.TimeOfDay;
                        if (ValueChanged != null)
                            ValueChanged(this, null);
                    }
                }
            }
        }


    }
}
