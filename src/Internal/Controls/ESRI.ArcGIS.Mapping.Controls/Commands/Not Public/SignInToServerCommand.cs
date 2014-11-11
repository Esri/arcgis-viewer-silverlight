/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Linq;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Controls.Resources;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.DataSources.ArcGISServer;

namespace ESRI.ArcGIS.Mapping.Controls
{
    /// <summary>
    /// Presents the user with a UI for signing into ArcGIS Server
    /// </summary>
    public class SignInToServerCommand : SignInCommand
    {
        // Provides access to strings from the ESRI.ArcGIS.Mapping.Controls.ArcGISOnline assembly
        ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.StringResourcesManager agolStrings = new ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.StringResourcesManager();

        #region ICommand Members - CanExecute, Execute

        /// <summary>
        /// Gets whether the sign-in UI can be shown
        /// </summary>
        public override bool CanExecute(object parameter)
        {
            return !string.IsNullOrEmpty(Url) || PromptForUrl;
        }

        /// <summary>
        /// Shows the sign-in UI 
        /// </summary>
        public override void Execute(object parameter)
        {
            base.Execute(parameter);

            if (viewModel != null)
            {
                // Update whether or not the user should be required to enter the server URL
                viewModel.PromptForUrl = PromptForUrl;

                // Update the label on the UI with the server being signed-in to
                updateSignInLabel();
            }
        }

        #endregion

        #region Public Properties - Url, PromptForUrl

        private string _url;
        /// <summary>
        /// Gets or sets the URL to the ArcGIS Server to sign into
        /// </summary>
        public string Url
        {
            get { return _url; }
            set
            {
                _url = value;
                RaisePropertyChanged("Url");
                if (viewModel != null)
                    updateSignInLabel();
                RaiseCanExecuteChangedEvent(this, EventArgs.Empty);
            }
        }

        private bool _promptForUrl = false;
        /// <summary>
        /// Gets or sets whether to prompt the user for the ArcGIS Server URL
        /// </summary>
        public bool PromptForUrl
        {
            get { return _promptForUrl; }
            set
            {
                _promptForUrl = value;
                RaisePropertyChanged("PromptForUrl");
                if (viewModel != null)
                {
                    viewModel.PromptForUrl = value;
                    updateSignInLabel();
                }
                RaiseCanExecuteChangedEvent(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Command Handling - canSignIn, onSignIn

        /// <summary>
        /// Gets whether an authentication request can be made given the current state of the command
        /// </summary>
        protected override bool canSignIn(object parameter)
        {
            bool urlValid = !viewModel.PromptForUrl 
                || (viewModel.PromptForUrl && !string.IsNullOrEmpty(viewModel.Url));
            return base.canSignIn(parameter) && urlValid;
        }

        /// <summary>
        /// Attempts to authenticate with the credentials and against the server specified by the command
        /// </summary>
        protected async override void onSignIn(object parameter)
        {
            if (viewModel == null)
                return;

            // Set state to busy
            viewModel.SigningIn = true;

            try
            {
                string credentialUrl = Url;

                if (viewModel.Url != null)
                {
                    // Get the token URL for the ArcGIS Server
                    credentialUrl = await ArcGISServerDataSource.GetTokenURL(viewModel.Url, null);
                    if (credentialUrl == null)
                        onSignInFailed(new Exception(Strings.InvalidUrlUserPassword));
                }

                if (IdentityManager.Current != null)
                {
                    var options = new IdentityManager.GenerateTokenOptions() { ProxyUrl = ProxyUrl };
                    // Authenticate against the server to retrieve user token
                    IdentityManager.Credential cred =
                        await IdentityManager.Current.GenerateCredentialTaskAsync(
                        credentialUrl, viewModel.Username, viewModel.Password, options);

                    if (cred != null)
                    {
                        // Save the credential info so it can be used to try to access other services
                        if (!UserManagement.Current.Credentials.Any(c => c.Url != null
                        && c.Url.Equals(cred.Url, StringComparison.OrdinalIgnoreCase) 
                        && !string.IsNullOrEmpty(c.Token)))
                            UserManagement.Current.Credentials.Add(cred);

                        try
                        {
                            // Complete sign-in
                            viewModel.SigningIn = false;
                            viewModel.SignInError = null;
                            closeWindow();
                            OnSignedIn(cred);
                        }
                        catch (Exception ex)
                        {
                            onSignInFailed(ex);
                        }
                    }
                    else
                    {
                        onSignInFailed(new Exception(agolStrings.Get("SignInFailed")));
                    }
                }
            }
            catch (Exception ex)
            {
                onSignInFailed(ex);
            }
        }

        #endregion

        // Applies the current server URL to the label in the sign-in UI
        private void updateSignInLabel()
        {
            if (!PromptForUrl)
            {
                try
                {
                    Uri uri = new Uri(Url);
                    viewModel.SignInLabel = string.Format(Strings.SignInToArcGISServer, uri.Host);
                }
                catch
                {
                    viewModel.SignInLabel = string.Format(Strings.SignInToArcGISServer, Url);
                }
            }
            else
            {
                viewModel.SignInLabel = null;
            }
        }
    }
}
