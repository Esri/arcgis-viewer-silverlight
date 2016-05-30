/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client.Extensibility;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO.IsolatedStorage;
using ESRI.ArcGIS.Client.Application.Controls;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = PART_NOTIFICATIONS_THREAD, Type = typeof(ListBox))]
    [TemplatePart(Name = PART_DO_NOT_SHOW_AGAIN_CHECKBOX, Type = typeof(CheckBox))]
    public class NotificationPanel : Control
    {
        private const string OPT_OUT_OF_NOTIFICATION_KEY = "esri.arcgis.mapping.builder.optOutOfNotification.key";
        private const string PART_NOTIFICATIONS_THREAD = "NotificationsThread";
        private const string PART_DO_NOT_SHOW_AGAIN_CHECKBOX = "DontShowAgainCheckBox";

        ListBox NotificationsThreadListBox;
        CheckBox chkDoNotShowAgain;

        private static NotificationPanel _instance;

        public static NotificationPanel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NotificationPanel();
                }

                return _instance;
            }
        }

        private NotificationPanel()
        {
            this.DefaultStyleKey = typeof(NotificationPanel);
        }

        public void Initialize()
        {
            if (ViewerApplicationControl.Instance != null)
            {
                if (ViewerApplicationControl.Instance.ToolPanels != null)
                {
                    ViewerApplicationControl.Instance.ToolPanels.ToolClassLoadException -= Instance_ToolClassLoadException;
                    ViewerApplicationControl.Instance.ToolPanels.ToolClassLoadException += Instance_ToolClassLoadException;
                    ViewerApplicationControl.Instance.ToolPanels.ToolClassLoadConfigurationException -= Instance_ToolClassLoadConfigurationException;
                    ViewerApplicationControl.Instance.ToolPanels.ToolClassLoadConfigurationException += Instance_ToolClassLoadConfigurationException;
                }
                if (ViewerApplicationControl.Instance.BehaviorsConfiguration != null)
                {
                    ViewerApplicationControl.Instance.BehaviorsConfiguration.BehaviorClassLoadException -= Instance_BehaviorClassLoadException;
                    ViewerApplicationControl.Instance.BehaviorsConfiguration.BehaviorClassLoadException += Instance_BehaviorClassLoadException;
                    ViewerApplicationControl.Instance.BehaviorsConfiguration.BehaviorClassLoadConfigurationException -= Instance_BehaviorClassLoadConfigurationException;
                    ViewerApplicationControl.Instance.BehaviorsConfiguration.BehaviorClassLoadConfigurationException += Instance_BehaviorClassLoadConfigurationException;
                }
                ViewerApplicationControl.Instance.ExtensionLoadFailed -= ExtensionsProvider_ExtensionLoadFailed;
                ViewerApplicationControl.Instance.ExtensionLoadFailed += ExtensionsProvider_ExtensionLoadFailed;
            }
            else if (View.Instance != null)
            {
                View.Instance.ExtensionLoadFailed -= ExtensionsProvider_ExtensionLoadFailed;
                View.Instance.ExtensionLoadFailed += ExtensionsProvider_ExtensionLoadFailed;
            } 
        }

        public override void OnApplyTemplate()
        {
            NotificationsThreadListBox = GetTemplateChild(PART_NOTIFICATIONS_THREAD) as ListBox;
            if(NotificationsThreadListBox != null)
                NotificationsThreadListBox.ItemsSource = Notifications;

            chkDoNotShowAgain = GetTemplateChild(PART_DO_NOT_SHOW_AGAIN_CHECKBOX) as CheckBox;
            if (chkDoNotShowAgain != null)
            {
                chkDoNotShowAgain.Checked -= chkDoNotShowAgain_Checked;
                chkDoNotShowAgain.Unchecked -= chkDoNotShowAgain_Checked;
                chkDoNotShowAgain.Checked += chkDoNotShowAgain_Checked;
                chkDoNotShowAgain.Unchecked += chkDoNotShowAgain_Checked;
            }
        }

        void chkDoNotShowAgain_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk != null)
               OptedOutOfNotification = chk.IsChecked == true;
        }

        #region Notifications
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<Notification> Notifications
        {
            get { return GetValue(NotificationsProperty) as ObservableCollection<Notification>; }
            set { SetValue(NotificationsProperty, value); }
        }

        /// <summary>
        /// Identifies the Map dependency property.
        /// </summary>
        public static readonly DependencyProperty NotificationsProperty =
                DependencyProperty.Register(
                        "Notifications",
                        typeof(ObservableCollection<Notification>),
                        typeof(NotificationPanel),
                        new PropertyMetadata(new ObservableCollection<Notification>(), OnNotificationsPropertyChanged));

        /// <summary>
        /// ConnectionsProperty property changed handler.
        /// </summary>
        /// <param name="d">ConnectionsDropDownPopupControl that changed its Connections.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnNotificationsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NotificationPanel source = d as NotificationPanel;
            source.onNotificationsChanged();
        }

        private void onNotificationsChanged()
        {
            if (NotificationsThreadListBox != null)
                NotificationsThreadListBox.ItemsSource = Notifications;
        }
        #endregion Map

        public bool OptedOutOfNotification
        {
            get
            {
                bool b;
                if (IsolatedStorageSettings.ApplicationSettings.TryGetValue(OPT_OUT_OF_NOTIFICATION_KEY, out b))
                    return b;
                return false;
            }
            set
            {
                if (!IsolatedStorageSettings.ApplicationSettings.Contains(OPT_OUT_OF_NOTIFICATION_KEY))
                    IsolatedStorageSettings.ApplicationSettings.Add(OPT_OUT_OF_NOTIFICATION_KEY, value);
                else
                    IsolatedStorageSettings.ApplicationSettings[OPT_OUT_OF_NOTIFICATION_KEY] = value;
            }
        }

        public void AddNotification(string caption, string message, string details, MessageType type)
        {
            Notification not = new Notification();
            not.Header = caption;
            not.Message = message;
            not.Type = type;
            not.Details = details;
            if(!Notifications.Contains(not))
                Notifications.Add(not);
            OnNotificationsUpdated();
        }

        void Instance_ToolClassLoadConfigurationException(object sender, CoreExceptionEventArgs e)
        {
            AddNotification(LocalizableStrings.ToolClassLoadConfigurationFailedExceptionHeader, e != null && e.Exception != null ? e.Exception.ToString() : "", e.Exception != null ? e.Exception.ToString() : null, MessageType.Warning);
        }

        void Instance_ToolClassLoadException(object sender, CoreExceptionEventArgs e)
        {
            string message = string.Empty;
            if (e != null && e.Exception != null)
            {
                message = e.Exception.Message;
            }

            AddNotification(LocalizableStrings.ToolClassLoadFailedExceptionHeader, message, e.Exception != null ? e.Exception.ToString() : null, MessageType.Warning);
        }

        void Instance_BehaviorClassLoadConfigurationException(object sender, ExceptionEventArgs e)
        {
            AddNotification(LocalizableStrings.GetString("BehaviorToolClassLoadConfigurationFailed"), LocalizableStrings.GetString("BehaviorToolClassLoadConfigurationFailedDescription"), e.Exception != null ? e.Exception.ToString() : null, MessageType.Warning);
        }

        void Instance_BehaviorClassLoadException(object sender, ExceptionEventArgs e)
        {
            string message = string.Empty;
            if (e != null && e.Exception != null)
            {
                message = e.Exception.Message;
            }

            AddNotification(LocalizableStrings.GetString("BehaviorClassLoadFailed"), message, e.Exception != null ? e.Exception.ToString() : null, MessageType.Warning);
        }

        void ExtensionsProvider_ExtensionLoadFailed(object sender, ExceptionEventArgs e)
        {
            RaiseExtensionLoadFailed(sender, e);
        }

        public void RaiseExtensionLoadFailed(object sender, ExceptionEventArgs e)
        {
            string url = e.UserState as string;
            string message = null;
            string description = null;            
            if (e.Exception != null)
            {
                // Two Kinds of exceptions are possible
                // 1. WebException:- When the .xap was not found
                // 2. ReflectionException:- When the .xap was found, but the assemblies in it were problematic (eg:- versioning issues)

                message = e.Exception.Message;
                System.Net.WebException webException = e.Exception as System.Net.WebException;
                if (webException != null)
                {
                    message = LocalizableStrings.ExtensionInitializationFailed + " Url: " + url;
                    description = webException.Message;
                }
                else
                {
                    System.Reflection.ReflectionTypeLoadException reflectionException = e.Exception as System.Reflection.ReflectionTypeLoadException;
                    if (reflectionException != null)
                    {
                        if (reflectionException.LoaderExceptions != null)
                        {
                            description = string.Empty;
                            foreach (Exception exceptions in reflectionException.LoaderExceptions)
                                description += exceptions.Message;
                        }
                    }
                    else
                    {
                        message = e.Exception.Message;
                        description = e.Exception.ToString();
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(url))
                    message = LocalizableStrings.ExtensionInitializationFailed + " Url: " + url;
            }

            if(string.IsNullOrEmpty(message))
                message = e.Exception != null ? e.Exception.Message : "";

            AddNotification(LocalizableStrings.ExtensionLoadFailedExceptionHeader, message, description, MessageType.Warning);
        }

        protected virtual void OnNotificationsUpdated()
        {
            EventHandler handler = NotificationsUpdated;
            if (handler != null)
                handler(this, null);
        }
        public event EventHandler NotificationsUpdated;
    }

    #region Converters
    public class MoreLessContentByVisibilityConverter : System.Windows.Data.IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility vis = (Visibility)value;
            string content = LocalizableStrings.GetString("More");
            if (vis == Visibility.Visible)
            {
                content = LocalizableStrings.GetString("Less");
            }
            return content;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        #endregion
    }
    public class ExpandCollapseImageByVisibilityConverter : System.Windows.Data.IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility vis = (Visibility)value;
            string imageUrl = "/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/expand.png";
            if (vis == Visibility.Visible)
            {
                imageUrl = "/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/collapse.png";
            }
            return imageUrl;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        #endregion
    }
    #endregion
}
