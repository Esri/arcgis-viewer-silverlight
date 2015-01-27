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
using ESRI.ArcGIS.Client.Toolkit;
using System.Threading.Tasks;
using System.Windows.Browser;

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
            bool supportsOAuth = false;
            var appPortal = MapApplication.Current != null ? MapApplication.Current.Portal : null;
            if (appPortal != null && appPortal.ArcGISPortalInfo != null)
                supportsOAuth = appPortal.ArcGISPortalInfo.SupportsOAuth;
            else if (ArcGISOnline != null && ArcGISOnline.PortalInfo != null)
                supportsOAuth = ArcGISOnline.PortalInfo.SupportsOAuth;
            else if (ArcGISOnlineEnvironment.ArcGISOnline != null && ArcGISOnlineEnvironment.ArcGISOnline.PortalInfo != null)
                supportsOAuth = ArcGISOnlineEnvironment.ArcGISOnline.PortalInfo.SupportsOAuth;

            string portalAppID = null;
            if (supportsOAuth)
            {
                var appInstance = ViewerApplicationControl.Instance;
                if (appInstance != null)
                {
                    // Get portal App ID from current Viewer app, if there is one
                    portalAppID = appInstance.ViewerApplication != null ? appInstance.ViewerApplication.PortalAppId : null;
                    if (string.IsNullOrEmpty(portalAppID) && appInstance.BuilderApplication != null)
                    {
                        // No current Viewer app, so get App ID from Builder settings
                        portalAppID = appInstance.BuilderApplication.PortalAppId;
                    }
                }
            }

            if (!string.IsNullOrEmpty(portalAppID))
            {
                onSignIn(portalAppID);
            }
            else
            {
                base.Execute(parameter);
            }

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
            if (viewModel == null && !(parameter is string))
                return;

            // Update busy state
            if (viewModel != null)
                viewModel.SigningIn = true;

            var portalAppID = parameter is string ? (string)parameter : null;
            var useOAuth = !string.IsNullOrEmpty(portalAppID);

            try
            {
                if (IdentityManager.Current != null)
                {
                    var cred = await generateCredential(portalAppID);
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
                            if (viewModel != null)
                            {
                                viewModel.SigningIn = false;
                                viewModel.SignInError = null;
                            }
                            closeWindow();
                            OnSignedIn(cred);
                        }
                        catch (Exception ex)
                        {
                            onSignInFailed(ex, useOAuth);
                        }
                    }
                    else
                    {
                        onSignInFailed(new Exception(agolStrings.Get("SignInFailed")), useOAuth);
                    }
                }
            }
            catch (Exception ex)
            {
                onSignInFailed(ex, useOAuth);
            }
        }

        private async Task<IdentityManager.Credential> generateCredential(string portalAppID)
        {
            // Get the ArcGIS Online or Portal URL to try to authenticate against
            string portalUrl = null;
            var appPortal = MapApplication.Current != null ? MapApplication.Current.Portal : null;
            if (appPortal != null && !string.IsNullOrEmpty(appPortal.Url))
                portalUrl = MapApplication.Current.Portal.Url;
            else if (ArcGISOnline != null)
                portalUrl = ArcGISOnline.Url; 
            else if (ArcGISOnlineEnvironment.ArcGISOnline != null)
                portalUrl = ArcGISOnlineEnvironment.ArcGISOnline.Url;

            IdentityManager.Credential cred = null;
            if (IdentityManager.Current != null && !string.IsNullOrEmpty(portalUrl))
            {
                portalUrl = portalUrl.TrimEnd('/');

                var options = new IdentityManager.GenerateTokenOptions() 
                { 
                    ProxyUrl = ProxyUrl,
                    TokenAuthenticationType = !string.IsNullOrEmpty(portalAppID) ? IdentityManager.TokenAuthenticationType.OAuthImplicit : 
                        IdentityManager.TokenAuthenticationType.ArcGISToken
                };

                if (!string.IsNullOrEmpty(portalAppID))
                {
                    var oauthAuthorize = new OAuthAuthorize() { UsePopup = true };
                    options.OAuthAuthorize = oauthAuthorize;
                    var oauthClientInfo = new IdentityManager.OAuthClientInfo()
                    {
                        ClientId = portalAppID, 
                        OAuthAuthorize = oauthAuthorize,
                        RedirectUri = HtmlPage.Document.DocumentUri.ToString()
                    };

                    var serverInfoRegistered = IdentityManager.Current.ServerInfos.Any(info => info.ServerUrl == portalUrl);
                    var serverInfo = serverInfoRegistered ? IdentityManager.Current.ServerInfos.First(info => info.ServerUrl == portalUrl) 
                        : new IdentityManager.ServerInfo();
                    serverInfo.ServerUrl = portalUrl;
                    serverInfo.OAuthClientInfo = oauthClientInfo;
                    serverInfo.TokenAuthenticationType = IdentityManager.TokenAuthenticationType.OAuthImplicit;
                    if (!serverInfoRegistered)
                        IdentityManager.Current.RegisterServers(new IdentityManager.ServerInfo[] { serverInfo });
                    cred = await IdentityManager.Current.GenerateCredentialTaskAsync(portalUrl, options);
                }
                else
                {
                    // Authenticate against ArcGIS Online/Portal to retrieve user token
                    cred = await IdentityManager.Current.GenerateCredentialTaskAsync(
                        portalUrl, viewModel.Username, viewModel.Password, options);
                }
            }
            return cred;
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
