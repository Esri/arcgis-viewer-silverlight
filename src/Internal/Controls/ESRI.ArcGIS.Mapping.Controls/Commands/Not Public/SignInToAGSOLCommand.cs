/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Linq;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls.Resources;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Controls.ArcGISOnline;

namespace ESRI.ArcGIS.Mapping.Controls
{
    /// <summary>
    /// Presents the user with a UI for signing in to ArcGIS Online or Portal
    /// </summary>
    public class SignInToAGSOLCommand : SignInCommand
    {
        // Provides access to strings from the ESRI.ArcGIS.Mapping.Controls.ArcGISOnline assembly
        ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.StringResourcesManager agolStrings = new ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.StringResourcesManager();

        #region ICommand Members - CanExecute, Execute

        /// <summary>
        /// Gets whether the sign-in UI can be shown
        /// </summary>
        public override bool CanExecute(object parameter)
        {
            return ArcGISOnline != null || ArcGISOnlineEnvironment.ArcGISOnline != null;
        }

        /// <summary>
        /// Shows the sign-in UI 
        /// </summary>
        public override void Execute(object parameter)
        {
            base.Execute(parameter);

            if (viewModel != null)
                updateSignInLabel();
        }

        #endregion

        #region Public Properties - ArcGISOnline

        private ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnline _agol;
        /// <summary>
        /// Gets or sets the ArcGIS Online or Portal instance to sign into
        /// </summary>
        public ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnline ArcGISOnline
        {
            get { return _agol; }
            set
            {
                _agol = value;
                RaisePropertyChanged("ArcGISOnline");
                RaiseCanExecuteChangedEvent(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Command Handling - onSignIn

        /// <summary>
        /// Attempts to authenticate against the ArcGIS Online or Portal instance 
        /// with the credentials specified by the command
        /// </summary>
        protected async override void onSignIn(object parameter)
        {
            if (viewModel == null)
                return;

            // Update busy state
            viewModel.SigningIn = true;

            // Get the ArcGIS Online or Portal URL to try to authenticate against
            string portalUrl = null;
            if (MapApplication.Current != null &&
            MapApplication.Current.Portal != null &&
            !string.IsNullOrEmpty(MapApplication.Current.Portal.Url))
                portalUrl = MapApplication.Current.Portal.Url;
            else
                portalUrl = ArcGISOnline != null ? ArcGISOnline.Url : ArcGISOnlineEnvironment.ArcGISOnline.Url;

            try
            {
                if (IdentityManager.Current != null)
                {
                    var options = new IdentityManager.GenerateTokenOptions() { ProxyUrl = ProxyUrl };
                    // Authenticate against ArcGIS Online/Portal to retrieve user token
                    IdentityManager.Credential cred =
                        await IdentityManager.Current.GenerateCredentialTaskAsync(
                        portalUrl, viewModel.Username, viewModel.Password, options);

                    if (cred != null)
                    {
                        // Save the credential info so it can be used to try to access other services
                        if (!UserManagement.Current.Credentials.Any(c => c.Url != null
                        && c.Url.Equals(cred.Url, StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrEmpty(c.Token)))
                            UserManagement.Current.Credentials.Add(cred);

                        try
                        {
                            // Use the token to sign in to the application environment's 
                            // ArcGIS Online/Portal instance
                            await ApplicationHelper.SignInToPortal(cred, ArcGISOnline);

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

        #region Utility Methods

        // Applies the current ArcGIS Online or Portal name to the label in the sign-in UI
        private void updateSignInLabel()
        {
            ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnline agol = ArcGISOnline ?? ArcGISOnlineEnvironment.ArcGISOnline;
            if (viewModel != null && agol != null)
            {
                // Determine portal name
                string portalName = Strings.ArcGISOnline;
                if (agol.PortalInfo != null)
                {
                    PortalInfo info = agol.PortalInfo;
                    if (!string.IsNullOrEmpty(info.PortalName)
                    && info.PortalName != Strings.ArcGISPortalDefaultName
                    && info.PortalName != Strings.ArcGISOnline)
                        portalName = info.PortalName; // PortalName property takes precedence                    
                    else if (!string.IsNullOrEmpty(info.Name)
                    && info.Name != Strings.ArcGISPortalDefaultName
                    && info.Name != Strings.ArcGISOnline)
                        portalName = info.Name; // Some instances store their instance name in the Name property
                    else if (!string.IsNullOrEmpty(info.PortalName))
                        portalName = info.PortalName;
                    else if (!string.IsNullOrEmpty(info.Name))
                        portalName = info.Name;
                }
                viewModel.SignInLabel = string.Format(agolStrings.Get("SignInLabel"), portalName);
            }
        }

        #endregion
    }
}
