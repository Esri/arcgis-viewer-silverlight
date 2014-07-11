/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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
using System.Collections.ObjectModel;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class ExtensionUploadWarningDialog : UserControl
    {
        public ExtensionUploadWarningDialog(IEnumerable<string> errors, IEnumerable<string> warnings)
        {
            InitializeComponent();

            DataContext = this;

            Messages = new ObservableCollection<Dictionary<string, string>>();
            foreach (string error in errors)
            {
                Dictionary<string, string> message = new Dictionary<string, string>();
                message.Add("message", error);
                message.Add("image", "/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/warning_icon.png");
                Messages.Add(message);
            }

            foreach (string warning in warnings)
            {
                Dictionary<string, string> message = new Dictionary<string, string>();
                message.Add("message", warning);
                message.Add("image", "/ESRI.ArcGIS.Mapping.Controls;component/Images/icons/caution16.png");
                Messages.Add(message);
            }

            HasErrors = errors.Count() > 0;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (OkClicked != null)
                OkClicked(this, EventArgs.Empty);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (CancelClicked != null)
                CancelClicked(this, EventArgs.Empty);
        }

        public event EventHandler OkClicked;
        public event EventHandler CancelClicked;

        public static readonly DependencyProperty MessagesProperty = DependencyProperty.Register(
            "Messages", typeof(ObservableCollection<Dictionary<string, string>>), typeof(ExtensionUploadWarningDialog),
            null);
        
        public ObservableCollection<Dictionary<string, string>> Messages 
        {
            get { return GetValue(MessagesProperty) as ObservableCollection<Dictionary<string, string>>; }
            set { SetValue(MessagesProperty, value); }
        }

        public static readonly DependencyProperty HasErrorsProperty = DependencyProperty.Register(
            "HasErrors", typeof(bool), typeof(ExtensionUploadWarningDialog),
            null);

        public bool HasErrors
        {
            get { return (bool)GetValue(HasErrorsProperty); }
            set { SetValue(HasErrorsProperty, value); }
        }

    }
}
