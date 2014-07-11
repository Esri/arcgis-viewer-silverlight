/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client.Extensibility;

namespace SearchTool
{
    /// <summary>
    /// Shows a window containing the targeted element
    /// </summary>
    public class ShowWindowAction : TargetedTriggerAction<FrameworkElement>
    {
        protected override void Invoke(object parameter)
        {
            if (Target != null && MapApplication.Current != null)
                MapApplication.Current.ShowWindow(Title, Target, Modal, OnClosing, OnClosed, WindowType);
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Title"/> property
        /// </summary>
        public static DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(ShowWindowAction), null);

        /// <summary>
        /// Gets or sets the title of the window
        /// </summary>
        public string Title
        {
            get { return GetValue(TitleProperty) as string; }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Modal"/> property
        /// </summary>
        public static DependencyProperty ModalProperty = DependencyProperty.Register(
            "Modal", typeof(bool), typeof(ShowWindowAction), null);

        /// <summary>
        /// Gets or sets whether the window is modal
        /// </summary>
        public bool Modal
        {
            get { return (bool)GetValue(ModalProperty); }
            set { SetValue(ModalProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="WindowType"/> property
        /// </summary>
        public static DependencyProperty WindowTypeProperty = DependencyProperty.Register(
            "WindowType", typeof(WindowType), typeof(ShowWindowAction), null);

        /// <summary>
        /// Gets or sets the type of window to show
        /// </summary>
        public WindowType WindowType
        {
            get { return (WindowType)GetValue(WindowTypeProperty); }
            set { SetValue(WindowTypeProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="AllowClose"/> property
        /// </summary>
        public static DependencyProperty AllowCloseProperty = DependencyProperty.Register(
            "AllowClose", typeof(bool), typeof(ShowWindowAction), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets whether to allow the window to be closed
        /// </summary>
        public bool AllowClose
        {
            get { return (bool)GetValue(AllowCloseProperty); }
            set { SetValue(AllowCloseProperty, value); }
        }

        /// <summary>
        /// Fires when the window is about to close.  Can be used to check a condition and modify the
        /// AllowClose property to prevent the window from being closed.
        /// </summary>
        public EventHandler<CancelEventArgs> Closing;

        private void OnClosing(object sender, CancelEventArgs e)
        {
            // Get the original cancel value
            bool cancel = e.Cancel;

            // Fire the closing event to allow listeners the chance to cancel
            if (Closing != null)
                Closing(this, e);

            // If the cancel value is unchanged by listeners, update it with the AllowClose property
            if (cancel == e.Cancel)
                e.Cancel = !AllowClose;
        }

        /// <summary>
        /// Fires when the window has been closed.
        /// </summary>
        public EventHandler Closed;

        private void OnClosed(object sender, EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }
    }
}
