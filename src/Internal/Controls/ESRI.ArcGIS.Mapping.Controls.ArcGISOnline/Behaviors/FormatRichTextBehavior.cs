/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interactivity;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
    /// <summary>
    /// Substitutes a set of values into a format string and updates the content of the associated 
    /// <see cref="RichTextBlock"/> with the resulting value.
    /// </summary>
    /// <remarks>
    /// Values may include strings, <see cref="Inline"/> elements, or <see cref="UIElement"/> objects.  Note that HTML
    /// included in values is not parsed; the resulting text will include any HTML tags that were included in the original
    /// value.
    /// </remarks>
    public class FormatRichTextBehavior : Behavior<RichTextBlock>
    {
        public FormatRichTextBehavior()
        {
            Values = new ObservableCollection<object>();
        }

        #region Behavior Overrides

        protected override void OnAttached()
        {
            base.OnAttached();

            buildBlocks();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (Values != null)
                Values.CollectionChanged -= Values_CollectionChanged;
        }

        #endregion

        #region FormatString

        /// <summary>
        /// Backing DependencyProperty for the <see cref="FormatString"/> property
        /// </summary>
        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register(
            "FormatString", typeof(string), typeof(FormatRichTextBehavior),
            new PropertyMetadata(OnFormatStringChanged));

        /// <summary>
        /// Gets or sets the string to substitute values into
        /// </summary>
        public string FormatString
        {
            get { return GetValue(FormatStringProperty) as string; }
            set { SetValue(FormatStringProperty, value); }
        }

        private static void OnFormatStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FormatRichTextBehavior)d).buildBlocks();
        }

        #endregion

        #region Values

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Values"/> property
        /// </summary>
        public static readonly DependencyProperty ValuesProperty = DependencyProperty.Register(
            "Values", typeof(ObservableCollection<object>), typeof(FormatRichTextBehavior),
            new PropertyMetadata(OnValuesChanged));

        /// <summary>
        /// Gets or sets the collection of values to substitute into the format string
        /// </summary>
        public ObservableCollection<object> Values
        {
            get { return GetValue(ValuesProperty) as ObservableCollection<object>; }
            set { SetValue(ValuesProperty, value); }
        }

        // Fires when the Values property changes
        private static void OnValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Update event handlers
            FormatRichTextBehavior behavior = ((FormatRichTextBehavior)d);
            if (e.OldValue != null)
                ((ObservableCollection<object>)e.OldValue).CollectionChanged -= behavior.Values_CollectionChanged;

            if (e.NewValue != null)
                ((ObservableCollection<object>)e.NewValue).CollectionChanged += behavior.Values_CollectionChanged;

            // Rebuild formatted text
            behavior.buildBlocks();
        }

        #endregion

        // Fires when objects are added to or removed from the Values collection
        private void Values_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            buildBlocks();
        }

        // Constructs the formatted text
        private void buildBlocks()
        {
            if (AssociatedObject == null || FormatString == null || Values == null)
                return;

            int i = 0;
            int last = 0;
            Paragraph p = new Paragraph();

            // Loop through each value
            while (i <= Values.Count)
            {
                // Get the index of the current substitution placeholder in the format string
                int current = FormatString.IndexOf("{" + i + "}");

                // Get the section of the format string between the last placeholder and the current one
                string subSection = i < Values.Count || current >= 0 ? 
                    FormatString.Substring(last, current - last) : FormatString.Substring(last);

                // Add the section to the formatted text
                if (!string.IsNullOrEmpty(subSection))
                    p.Inlines.Add(new Run() { Text = subSection });

                // If the last value has already been processed, exit
                if (i == Values.Count || current < 0)
                    break;

                // Get the value and add it to the formatted text
                object val = Values[i];
                if (val is string)
                    p.Inlines.Add(new Run() { Text = (string)val });
                else if (val is Inline)
                    p.Inlines.Add((Inline)val);
                else if (val is UIElement)
                    p.Inlines.Add(new InlineUIContainer() { Child = (UIElement)val });

                last = current + 3;
                i++;
            }

            // Update the associated RichTextBlock's content
            AssociatedObject.Blocks.Clear();
            AssociatedObject.Blocks.Add(p);
        }
    }
}
