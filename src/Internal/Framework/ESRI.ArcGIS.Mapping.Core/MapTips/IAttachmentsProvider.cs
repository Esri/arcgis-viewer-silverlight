/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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
using System.Collections.Generic;
using System.Windows.Threading;

namespace ESRI.ArcGIS.Mapping.Core
{
    public interface IAttachmentsProvider
    {
        void LoadAttachments(string linkUrl, EventHandler<MultipleAttachmentsInfoEventArgs> onCompleted, EventHandler<ExceptionEventArgs> onFailed, object userState);
        bool HasAlreadyRetrievedAttachments();
        MultipleAttachmentsInfo GetAttachments();
    }

    public class MultipleAttachmentsInfoEventArgs : EventArgs
    {
        public MultipleAttachmentsInfo AttachmentInfo { get; set; }
        public object UserState { get; set; }
    }
}
