/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;

namespace ESRI.ArcGIS.Mapping.Behaviors
{
    public class HyperlinkOrTextBlock : Control
    {
        public HyperlinkOrTextBlock()
        {
            this.DefaultStyleKey = typeof(HyperlinkOrTextBlock);
        }

        public object FieldValue
        {
            get { return (object)GetValue(FieldValueProperty); }
            set { SetValue(FieldValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FieldValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FieldValueProperty =
            DependencyProperty.Register("FieldValue", typeof(object), typeof(HyperlinkOrTextBlock), new PropertyMetadata(null, OnValueChange));

        static void OnValueChange(DependencyObject obj, DependencyPropertyChangedEventArgs a)
        {
            HyperlinkOrTextBlock self = obj as HyperlinkOrTextBlock;
            if (self != null)
            {
                self.HyperlinkVisibility = Visibility.Collapsed;
                self.TextBlockVisibility = Visibility.Visible;

                bool setText = false;
                if (a.NewValue != null)
                {
                    string newVal = a.NewValue as string;

                    if (!String.IsNullOrEmpty(newVal))
                    {
                        newVal = newVal.Trim();
                        bool isHyperlink = newVal.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                            || newVal.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                            || newVal.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase);

                        // If it looks like a hyperlink then parse it to make sure it passes that test as well. If it
                        // does pass the test, assign the Uri object to the appropriate property so it gets bound
                        // correctly to the hyperlink button control (which cannot bind to a string, it must be a Uri).
                        if (isHyperlink)
                        {
                            Uri targetUri = null;
                            if (!Uri.TryCreate(newVal, UriKind.Absolute, out targetUri))
                                isHyperlink = false;
                            else
                            {
                                // We need to actually assign this value to FieldValueAsText because we have transformed the
                                // raw input value (possibly) via calling "Trim" on it. The values used in both the Uri
                                // and the TextBlock control must remain in sync for proper appearances and is why this
                                // is necessary.
                                setText = true;
                                self.FieldValueAsText = newVal;
                                self.FieldValueAsUri = targetUri;
                            }
                        }

                        if (isHyperlink)
                        {
                            self.HyperlinkVisibility = Visibility.Visible;
                            self.TextBlockVisibility = Visibility.Collapsed;
                        }
                    }
                }

                if (setText == false)
                    self.FieldValueAsText = a.NewValue as string;
            }
        }

        public Uri FieldValueAsUri
        {
            get { return (Uri)GetValue(FieldValueAsUriProperty); }
            set { SetValue(FieldValueAsUriProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FieldValueAsUri.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FieldValueAsUriProperty =
            DependencyProperty.Register("FieldValueAsUri", typeof(Uri), typeof(HyperlinkOrTextBlock), new PropertyMetadata(null));

        public string FieldValueAsText
        {
            get { return (string)GetValue(FieldValueAsTextProperty); }
            set { SetValue(FieldValueAsTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FieldValueAsText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FieldValueAsTextProperty =
            DependencyProperty.Register("FieldValueAsText", typeof(string), typeof(HyperlinkOrTextBlock), new PropertyMetadata(null));

                
        public Visibility HyperlinkVisibility
        {
            get { return (Visibility)GetValue(HyperlinkVisibilityProperty); }
            set { SetValue(HyperlinkVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HyperlinkVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HyperlinkVisibilityProperty =
            DependencyProperty.Register("HyperlinkVisibility", typeof(Visibility), typeof(HyperlinkOrTextBlock), new PropertyMetadata(Visibility.Collapsed));

        public Visibility TextBlockVisibility
        {
            get { return (Visibility)GetValue(TextBlockVisibilityProperty); }
            set { SetValue(TextBlockVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextBlockVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextBlockVisibilityProperty =
            DependencyProperty.Register("TextBlockVisibility", typeof(Visibility), typeof(HyperlinkOrTextBlock), new PropertyMetadata(Visibility.Visible));
    }
}
