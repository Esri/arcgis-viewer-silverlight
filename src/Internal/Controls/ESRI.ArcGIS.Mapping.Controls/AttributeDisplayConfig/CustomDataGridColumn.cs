/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;
using System.Windows.Browser;

namespace ESRI.ArcGIS.Mapping.Controls
{
    class CustomDataGridColumn : DataGridTextColumn
    {
        public CustomDataGridColumn() 
        {
            
        }

        public ESRI.ArcGIS.Mapping.Core.FieldInfo FieldInfo { get; set; }

        public override string ToString()
        {
            return FieldInfo != null ? FieldInfo.DisplayName : FieldInfo.Name ?? base.ToString();
        }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            if(FieldInfo == null)
                return base.GenerateElement(cell, dataItem);

            FieldType fieldType = FieldInfo.FieldType;
            string actualFieldName = null;
            if (fieldType == FieldType.Attachment)
            {
                return new TextBlock();
                //AttachmentFieldValue attachmentFieldValue = getPropertyValue(FieldInfo.Name, dataItem, out actualFieldName) as AttachmentFieldValue;
                //if (attachmentFieldValue != null && attachmentFieldValue.AttachmentsProvider != null)
                //{                    
                    //return base.GenerateElement(cell, dataItem);
                    //StackPanel attachmentsStackPanel = new StackPanel();

                    //if (attachmentFieldValue.AttachmentsProvider.HasAlreadyRetrievedAttachments())
                    //{
                    //    MultipleAttachmentsInfo attachments = attachmentFieldValue.AttachmentsProvider.GetAttachments();
                    //    buildAttachmentsPanel(attachmentsStackPanel, attachments);
                    //}
                    //else
                    //{
                    //    HyperlinkButton attachmentsAnchor = createHyperlinkButton(null, "Retrieve");
                    //    attachmentsAnchor.DataContext = attachmentFieldValue;
                    //    attachmentsStackPanel.Children.Add(attachmentsAnchor);
                    //    StackPanel attachmentsContainer = new StackPanel();
                    //    attachmentsAnchor.Tag = attachmentsStackPanel;
                    //    attachmentsAnchor.Click += new RoutedEventHandler(attachmentsAnchor_Click);
                    //    attachmentsStackPanel.Children.Add(attachmentsContainer);
                    //}

                    //return attachmentsStackPanel;
                //}
            }
            else if (fieldType == FieldType.Entity)
            {
                EntityFieldValue entityFieldValue = getPropertyValue(FieldInfo.Name, dataItem, out actualFieldName) as EntityFieldValue;
                if (entityFieldValue != null)
                {
                    return getHyperlinkButton(string.Format("{0}.LinkUrl", actualFieldName));
                }
            }
            else if(fieldType == FieldType.Hyperlink)
            {
                HyperlinkFieldValue hyperLinkFieldValue = getPropertyValue(FieldInfo.Name, dataItem, out actualFieldName) as HyperlinkFieldValue;
                if (hyperLinkFieldValue != null)
                {
                    //TextBlock tblock = new TextBlock();
                    //tblock.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(string.Format("{0}.DisplayText", actualFieldName)));
                    //HyperlinkButton hyperlinkButton = new HyperlinkButton();
                    //hyperlinkButton.Click += new RoutedEventHandler(hyperlinkButton_Click);
                    //hyperlinkButton.Content = tblock;
                    //return hyperlinkButton;
                    //return tblock;                         
                    
                    return getHyperlinkButton(string.Format("{0}.LinkUrl", actualFieldName));

                    //TextBlock tblock = new TextBlock();
                    //tblock.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(string.Format("{0}.LinkUrl", actualFieldName)));
                    //return tblock;                    
                }
            }
            else if(fieldType == FieldType.Image)
            {
                HyperlinkImageFieldValue hyperLinkImageFieldValue = getPropertyValue(FieldInfo.Name, dataItem, out actualFieldName) as HyperlinkImageFieldValue;
                if (hyperLinkImageFieldValue != null)
                {
                    return getHyperlinkButton(string.Format("{0}.ImageUrl", actualFieldName));
                }
            }
            else if (fieldType == FieldType.Currency)
            {
                CurrencyFieldValue currencyFieldValue = getPropertyValue(FieldInfo.Name, dataItem, out actualFieldName) as CurrencyFieldValue;
                if (currencyFieldValue != null)
                {
                    TextBlock tblock = new TextBlock();
                    tblock.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(string.Format("{0}.FormattedValue", actualFieldName)));                    
                    return tblock;                    
                }
            }
            else if (fieldType == FieldType.DateTime)
            {
                DateTimeFieldValue dateTimeFieldValue = getPropertyValue(FieldInfo.Name, dataItem, out actualFieldName) as DateTimeFieldValue;
                if (dateTimeFieldValue != null)
                {
                    TextBlock tblock = new TextBlock();
                    tblock.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(string.Format("{0}.FormattedValue", actualFieldName)));                    
                    return tblock;                     
                }
            }

            return base.GenerateElement(cell, dataItem);
        }

        void hyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hButton = sender as HyperlinkButton;
            object dataContext = hButton.DataContext;
        }
       
        static void attachmentsAnchor_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton source = sender as HyperlinkButton;
            if (source == null)
                return;
            StackPanel container = source.Tag as StackPanel;
            if (container == null)
                return;
            AttachmentFieldValue attachmentFieldValue = source.DataContext as AttachmentFieldValue;
            if (attachmentFieldValue == null)
                return;
            if (attachmentFieldValue.AttachmentsProvider == null)
                return;

            attachmentFieldValue.AttachmentsProvider.LoadAttachments(attachmentFieldValue.LinkUrl, (o, args) =>
            {
                container.Dispatcher.BeginInvoke((Action)delegate
                {
                    if (args == null)
                        return;
                    container.Children.Clear();
                    buildAttachmentsPanel(container, args.AttachmentInfo);
                });
            }, (o, args) =>
            {
                container.Dispatcher.BeginInvoke((Action)delegate
                {
                    if (args == null || args.Exception == null)
                        return;
                    container.Children.Add(new TextBlock() { Text = args.Exception.Message });
                });
            }, null);
        }        

        private static void buildAttachmentsPanel(StackPanel attachmentsStackPanel, MultipleAttachmentsInfo attachments)
        {
            if (attachments != null && attachments.LinkUrls != null && attachments.DisplayTextValues != null)
            {
                int count = Math.Max(attachments.LinkUrls.Count(), attachments.DisplayTextValues.Count());
                for (int i = 0; i < count; i++)
                {
                    string linkUrl = attachments.LinkUrls.ElementAtOrDefault(i);
                    string link = attachments.DisplayTextValues.ElementAtOrDefault(i);
                    attachmentsStackPanel.Children.Add(createHyperlinkButton(linkUrl, link));
                }
            }
        }

        private object getPropertyValue(string fieldName, object dataItem, out string actualFieldName)
        {
            actualFieldName = fieldName;
            if (dataItem == null)
                return null;

            Type type = dataItem.GetType();
            if (type == null)
                return null;
            PropertyInfo property = type.GetProperty(fieldName);
            if (property == null)
            {
                actualFieldName = fieldName;
                property = type.GetProperty(normalizeFieldName(fieldName));                
            }
            if (property == null)
                return null;

            return property.GetValue(dataItem, null);
        }

        string normalizeFieldName(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                return fieldName;

            return fieldName.Replace('/', '_');
        }

        private static HyperlinkButton createHyperlinkButton(string url, string text)
        {
            HyperlinkButton element;
            Uri targetUri = null;
            if (!string.IsNullOrEmpty(url))
                targetUri = new Uri(url);
            TextBlock textBlock = new TextBlock()
            {
                Foreground = new SolidColorBrush(Colors.Blue),
                Margin = new Thickness(0, 0, 0, 5),
                TextDecorations = TextDecorations.Underline,
                Text = text,
                VerticalAlignment = VerticalAlignment.Top,
                FontWeight = FontWeights.Normal
            };

            element = new HyperlinkButton()
            {
                Content = textBlock,
                NavigateUri = targetUri,
                TargetName = "_blank",
                Background = new SolidColorBrush(Colors.Transparent),
                VerticalAlignment = VerticalAlignment.Top,
            };
            return element;
        }

        private HyperlinkButton getHyperlinkButton(string dataBindablePropertyName)
        {            
            HyperlinkButton hyperlink = null;
            try
            {                
                TextBlock textBlock = new TextBlock()
                {
                    Foreground = new SolidColorBrush(Colors.Blue),
                    Margin = new Thickness(0),
                    TextDecorations = TextDecorations.Underline,                    
                    VerticalAlignment = VerticalAlignment.Top,
                    FontWeight = FontWeights.Normal
                };
                textBlock.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(dataBindablePropertyName));

                hyperlink = new HyperlinkButton()
                {
                    Content = textBlock,
                    TargetName = "_blank",
                    Background = new SolidColorBrush(Colors.Transparent),
                    VerticalAlignment = VerticalAlignment.Top
                };
                hyperlink.SetBinding(HyperlinkButton.NavigateUriProperty, new System.Windows.Data.Binding(dataBindablePropertyName) { Converter = StringToUriConverter });
            }
            catch { }
            return hyperlink;
        }

        static StringToUriConverter StringToUriConverter = new StringToUriConverter();
    }
}
