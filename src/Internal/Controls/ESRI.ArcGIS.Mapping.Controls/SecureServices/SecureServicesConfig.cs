/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client;
using System.Windows.Controls;
using System.Windows.Input;
using System;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class SecureServicesConfig : Control, IDisposable
    {
        SecureServicesConfigViewModel viewModel;
        public SecureServicesConfig()
        {
            DefaultStyleKey = typeof(SecureServicesConfig);
            this.DataContext = viewModel = new SecureServicesConfigViewModel();
        }

        #region Focus Behavior
        TextBox textBox;
        public override void OnApplyTemplate()
        {
            if (textBox != null)
                textBox.KeyDown -= textBox_KeyDown;
            base.OnApplyTemplate();
            textBox = GetTemplateChild("TextBox") as TextBox;
            if (textBox != null)
                textBox.Focus();
            textBox.KeyDown += textBox_KeyDown;
        }
        #endregion

        #region Close on Enter Behavior
        void textBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (textBox != null)
                {
                    viewModel.OKCommand.Execute(textBox.Text);
                    new CloseWindowCommand().Execute(this);
                }
            }
        }
        #endregion


        public void Dispose()
        {
            if (viewModel != null)
                viewModel.Dispose();
        }
    }
}
