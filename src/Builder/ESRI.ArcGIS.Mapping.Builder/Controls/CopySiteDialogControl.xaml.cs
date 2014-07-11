/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Builder.ApplicationBuilder;
using ESRI.ArcGIS.Mapping.Builder.Common;
using ESRI.ArcGIS.Mapping.Controls;
using System.Windows.Input;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class CopySiteDialogControl : UserControl
    {
        private string sourceSiteId;

        public CopySiteDialogControl(string siteId)
        {
            // Copy site id to private class variable
            sourceSiteId = siteId;

            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Make sure input is valid
            if (validateInput())
            {
                // Disable UI, display progress indicator
                enableDisableUI(true);
                showHideProgressIndicator(false);

                // Asynchronously invoke server method to copy source site to target
                ApplicationBuilderClient client = WCFProxyFactory.CreateApplicationBuilderProxy();
                client.CopySiteCompleted += new EventHandler<CopySiteCompletedEventArgs>(client_CopySiteCompleted);
                client.CopySiteAsync(sourceSiteId, NameTextBox.Text.Trim(), DescriptionTextBox.Text.Trim());
            }
        }

        void client_CopySiteCompleted(object sender, CopySiteCompletedEventArgs e)
        {
            // Enable UI, hide progress indicator, hide this dialog
            enableDisableUI(false);
            showHideProgressIndicator(true);
            BuilderApplication.Instance.HideWindow(this);

            if (e.Cancelled)
                return;

            if (e.Error != null)
            {
                handleError(e.Error);
                return;
            }

            if (e.Site == null)
            {
                handleError(new Exception(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.FailedToCreateAndPublishApplication));
                return;
            }

            // Although technically impossible for the site collection to be null, check here and create if
            // necessary to ensure adding this new site will work properly.
            if (BuilderApplication.Instance.Sites == null)
                BuilderApplication.Instance.Sites = new System.Collections.ObjectModel.ObservableCollection<Site>();
            BuilderApplication.Instance.Sites.Add(e.Site);
        }

        private void handleError(Exception exception)
        {
            string errorMsg = "";
            if (exception != null)
            {
                if (!String.IsNullOrEmpty(exception.Message))
                    errorMsg = exception.Message;
                else
                    errorMsg = exception.ToString();
            }
            MessageBoxDialog.Show(exception != null ? errorMsg : ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ErrorDetailsNotKnown,
                ESRI.ArcGIS.Mapping.Builder.Resources.Strings.SiteCopyFailed, MessageBoxButton.OK);
        }

        private void enableDisableUI(bool disable)
        {
            OKButton.IsEnabled = CancelButton.IsEnabled = !disable;
        }

        private void showHideProgressIndicator(bool hide)
        {
            if (hide)
            {
                ProgressIndicator.StopProgressAnimation();
                ProgressIndicator.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                ProgressIndicator.Visibility = System.Windows.Visibility.Visible;
                ProgressIndicator.StartProgressAnimation();
            }
        }

        private bool validateInput()
        {
            string applicationName = NameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.EnterNameForApplication, ESRI.ArcGIS.Mapping.Builder.Resources.Strings.InvalidName, MessageBoxButton.OK);
                return false;
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            BuilderApplication.Instance.HideWindow(this);
        }

        private void CopySiteDialogControl_Loaded(object sender, RoutedEventArgs e)
        {
            NameTextBox.Text = "";
            DescriptionTextBox.Text = "";

            enableDisableUI(false);
            showHideProgressIndicator(true);
            NameTextBox.Focus();
        }

        private void CheckForEnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                OKButton_Click(this, null);
        }

    }
}
