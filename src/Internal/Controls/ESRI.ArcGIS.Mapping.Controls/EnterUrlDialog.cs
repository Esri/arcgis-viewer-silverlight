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

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = "btnOk", Type = typeof(Button))]
    [TemplatePart(Name = "btnCancel", Type = typeof(Button))]
    [TemplatePart(Name = "txtUrl", Type = typeof(TextBox))]
    public class EnterUrlDialog : Control
    {
        internal TextBox txtUrl { get; private set; }
        internal Button btnOk { get; private set; }
        internal Button btnCancel { get; private set; }

        private string InitialUrl { get; set; }

        [Obsolete("It is not recommended that you use the default constructor for this control. It is merely provided as a means to allow drag drop onto designer surfaces such as Expression Blend and Visual Studio.")]
        public EnterUrlDialog() : this(null) { }

        public EnterUrlDialog(string initialUrl)
        {
            DefaultStyleKey = typeof(EnterUrlDialog);

            InitialUrl = initialUrl;           
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            txtUrl = GetTemplateChild("txtUrl") as TextBox;
            if (txtUrl != null)
            {                
                if (!string.IsNullOrEmpty(InitialUrl))
                    txtUrl.Text = InitialUrl;
                txtUrl.TextChanged += new TextChangedEventHandler(txtUrl_TextChanged);
                txtUrl.KeyDown += new KeyEventHandler(txtUrl_KeyDown);
                txtUrl.Focus();
            }

            if(btnOk != null)
                btnOk.Click -= OkButton_Click;

            btnOk = GetTemplateChild("btnOk") as Button;
            if (btnOk != null)
            {
                btnOk.Click += OkButton_Click;
                btnOk.IsEnabled = Uri.IsWellFormedUriString(InitialUrl, UriKind.Absolute);
            }

            if (btnCancel != null)
                btnCancel.Click -= CancelButton_Click;

            btnCancel = GetTemplateChild("btnCancel") as Button;
            if(btnCancel != null)
                btnCancel.Click += CancelButton_Click;

            if (InitCompleted != null)
                InitCompleted(this, EventArgs.Empty);
        }

        internal event EventHandler InitCompleted;

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            OnUrlChosen(new UrlChosenEventArgs() { Url = txtUrl.Text });
            txtUrl.Focus();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnCancelButtonClicked(EventArgs.Empty);
            txtUrl.Focus();
        }

        private void txtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                OnUrlChosen(new UrlChosenEventArgs() { Url = txtUrl.Text });
        }

        private void txtUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(btnOk != null)
                btnOk.IsEnabled = Uri.IsWellFormedUriString(txtUrl.Text, UriKind.Absolute);
        }

        protected void OnCancelButtonClicked(EventArgs args)
        {
            if (CancelButtonClicked != null)
                CancelButtonClicked(this, args);
        }

        protected void OnUrlChosen(UrlChosenEventArgs args)
        {
            if (UrlChosen != null)
                UrlChosen(this, args);
        }

        public event EventHandler<UrlChosenEventArgs> UrlChosen;
        public event EventHandler CancelButtonClicked;
    }

    public class UrlChosenEventArgs : EventArgs
    {
        public string Url { get; set; }
    }
}
