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
using ESRI.ArcGIS.Client.Extensibility;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Mapping.Core;
using System.Linq;

namespace ESRI.ArcGIS.Mapping.Behaviors
{
    public class AttachmentsPanel: Control
    {
        public AttachmentsPanel()
        {
            this.DefaultStyleKey = typeof(AttachmentsPanel);
        }

        public object FieldValue
        {
            get { return (object)GetValue(FieldValueProperty); }
            set { SetValue(FieldValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FieldValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FieldValueProperty =
            DependencyProperty.Register("FieldValue", typeof(object), typeof(AttachmentsPanel), new PropertyMetadata(null, OnValueChange));

        static void OnValueChange(DependencyObject obj, DependencyPropertyChangedEventArgs a)
        {
            AttachmentsPanel panel = obj as AttachmentsPanel;
            if (panel.FieldValue is AttachmentFieldValue)
                setupAttachments(panel, panel.FieldValue as AttachmentFieldValue);
        }

         private static void setupAttachments(AttachmentsPanel panel, AttachmentFieldValue attachmentFieldValue)
        {
            if (attachmentFieldValue != null && attachmentFieldValue.AttachmentsProvider != null)
            {
                if (attachmentFieldValue.AttachmentsProvider.HasAlreadyRetrievedAttachments())
                {
                    MultipleAttachmentsInfo attachments = attachmentFieldValue.AttachmentsProvider.GetAttachments();
                    panel.setAttachments(attachments);
                }
                else
                {
                    attachmentFieldValue.AttachmentsProvider.LoadAttachments(attachmentFieldValue.LinkUrl, (o, args) =>
                    {
                        panel.Dispatcher.BeginInvoke((Action)delegate
                        {
                            if (args == null)
                                return;
                            AttachmentsPanel pnl = args.UserState as AttachmentsPanel;
                            pnl.setAttachments(args.AttachmentInfo);
                        });

                    }, (o, args) =>
                    {
                        panel.Dispatcher.BeginInvoke((Action)delegate
                        {
                            if (args == null || args.Exception == null)
                                return;
                            AttachmentsPanel pnl = args.UserState as AttachmentsPanel;
                            pnl.Error = args.Exception.Message;
                            pnl.ErrorVisibility = Visibility.Visible;
                        });
                    }, panel);
                }
            }
        }

        void setAttachments(MultipleAttachmentsInfo attachments)
        {
            if (attachments != null && attachments.LinkUrls != null && attachments.DisplayTextValues != null)
            {
                int count = Math.Max(attachments.LinkUrls.Count(), attachments.DisplayTextValues.Count());
                if (Attachments == null)
                    Attachments = new ObservableCollection<AttachmentInfo>();
                else
                    Attachments.Clear();
                for (int i = 0; i < count; i++)
                {
                    string url = attachments.LinkUrls.ElementAtOrDefault(i);
                    string name = attachments.DisplayTextValues.ElementAtOrDefault(i);
                    Attachments.Add(new AttachmentInfo(){ Name = name, Url = url});
                }
            }
        }

        #region View model
        public ObservableCollection<AttachmentInfo> Attachments
        {
            get { return (ObservableCollection<AttachmentInfo>)GetValue(AttachmentsProperty); }
            set { SetValue(AttachmentsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Attachments.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AttachmentsProperty =
            DependencyProperty.Register("Attachments", typeof(ObservableCollection<AttachmentInfo>), typeof(AttachmentsPanel), null);

        public string Error
        {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Error.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register("Error", typeof(string), typeof(AttachmentsPanel), null);

        public Visibility ErrorVisibility
        {
            get { return (Visibility)GetValue(ErrorVisibilityProperty); }
            set { SetValue(ErrorVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ErrorVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorVisibilityProperty =
            DependencyProperty.Register("ErrorVisibility", typeof(Visibility), typeof(AttachmentsPanel), new PropertyMetadata(Visibility.Collapsed));
        #endregion

        public class AttachmentInfo
        {
            public string Name { get; set; }
            public string Url { get; set; }
        }
        
    }
}
