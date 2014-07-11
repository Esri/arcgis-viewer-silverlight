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
using System.ComponentModel;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public delegate void MessageBoxClosedEventHandler(object sender, MessageBoxClosedArgs e);

    public class MessageBoxClosedArgs : EventArgs
    {
        public MessageBoxResult Result { get; set; }
    }

    public enum MessageType
    {
        None,
        Information,
        Error,
        Warning
    }

    [TemplatePart(Name = OK_BUTTON_NAME, Type = typeof(Button))]
    [TemplatePart(Name = CANCEL_BUTTON_NAME, Type = typeof(Button))]
    public class MessageBoxDialog : Control
    {
        private const string OK_BUTTON_NAME = "btnOK";
        private const string CANCEL_BUTTON_NAME = "btnCancel";

        private Button btnOK;
        private Button btnCancel;

        private MessageBoxDialog(string text, MessageType msgType, MessageBoxButton btns, MessageBoxClosedEventHandler onClosedHandler)
        {
            DefaultStyleKey = typeof(MessageBoxDialog);
            Text = text;
            MessageType = msgType;
            MessageBoxButton = btns;
            OnClosedHandler = onClosedHandler;
        }

        public override void OnApplyTemplate()
        {
            FloatingWindow parent = this.Parent as FloatingWindow;
            if (parent != null)
            {
                parent.KeyDown -= MessageBoxDialogParent_KeyDown;
            }

            if (btnOK != null)
                btnOK.Click -= button_Click;

            if (btnCancel != null)
                btnCancel.Click -= button_Click;

            base.OnApplyTemplate();

            btnOK = GetTemplateChild(OK_BUTTON_NAME) as Button;
            if (btnOK != null)
            {
                btnOK.Click += button_Click;
                btnOK.Focus();
            }
            btnCancel = GetTemplateChild(CANCEL_BUTTON_NAME) as Button;
            if (btnCancel != null)
                btnCancel.Click += button_Click;

            parent = this.Parent as FloatingWindow;
            if (parent != null)
            {
                parent.KeyDown -= MessageBoxDialogParent_KeyDown;
                parent.KeyDown += MessageBoxDialogParent_KeyDown;
            }
        }

        void MessageBoxDialogParent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Result = MessageBoxResult.Cancel;
                e.Handled = true;
                if (MapApplication.Current != null)
                    MapApplication.Current.HideWindow(this);
                else
                    WindowManager.HideWindow(this);
            }
        }

        #region Events

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                if (button.Name == OK_BUTTON_NAME)
                    this.Result = MessageBoxResult.OK;
                else if (button.Name == CANCEL_BUTTON_NAME)
                    this.Result = MessageBoxResult.Cancel;
            }
            if (MapApplication.Current != null)
                MapApplication.Current.HideWindow(this);
            else
                WindowManager.HideWindow(this);
        }

        private static void OnMessageBoxClosed(MessageBoxDialog control)
        {
            if (control != null && control.OnClosedHandler != null)
                control.OnClosedHandler.Invoke(control, new MessageBoxClosedArgs() { Result = control.Result == MessageBoxResult.None ? MessageBoxResult.Cancel : control.Result });
        }

        #endregion

        #region Properties

        public string Text { get; private set; }
        public MessageType MessageType { get; private set; }
        public MessageBoxButton MessageBoxButton { get; private set; }
        public MessageBoxResult Result { get; private set; }
        public MessageBoxClosedEventHandler OnClosedHandler { get; private set; }

        private static WindowManager _windowManager;
        private static WindowManager WindowManager
        {
            get
            {
                if (_windowManager == null)
                {
                    _windowManager = new WindowManager();
                }
                return _windowManager;
            }
        }

        #endregion

        #region Static Methods

        public static void Show(string message, MessageBoxClosedEventHandler onClosedHandler = null, bool isModal = true)
        {
            Show(message, "", MessageBoxButton.OK, onClosedHandler, isModal);
        }

        public static void Show(string message, string caption, MessageBoxButton messageButtons, MessageBoxClosedEventHandler onClosedHandler = null, bool isModal = true)
        {
            Show(message, caption, MessageType.None, messageButtons, onClosedHandler, isModal);
        }

        public static void Show(string message, string caption, MessageType messageType, MessageBoxButton messageButtons, MessageBoxClosedEventHandler onClosedHandler = null, bool isModal = true)
        {
            MessageBoxDialog control = new MessageBoxDialog(message, messageType, messageButtons, onClosedHandler);
            Size maxSize = WindowSizeUtility.GetWindowMaxSize();
            if (maxSize != Size.Empty)
            {
                control.MaxWidth = maxSize.Width;
                control.MaxHeight = maxSize.Height;
            }

            WindowType windowType = MapApplication.Current != null && MapApplication.Current.IsEditMode ?
                WindowType.DesignTimeFloating : WindowType.Floating;
            if (MapApplication.Current != null)
                MapApplication.Current.ShowWindow(caption, control, isModal, null, 
                    delegate { OnMessageBoxClosed(control); }, windowType);
            else
                WindowManager.ShowWindow(caption, control, isModal, null, 
                    delegate { OnMessageBoxClosed(control); }, windowType);
        }

        #endregion
    }
}
