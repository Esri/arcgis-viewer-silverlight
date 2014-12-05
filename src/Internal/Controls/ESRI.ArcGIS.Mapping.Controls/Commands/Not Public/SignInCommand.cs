/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls.Resources;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    /// <summary>
    /// When executed, presents a sign-in prompt to the end user
    /// </summary>
    public abstract class SignInCommand : CommandBase
    {
        protected SignInViewModel viewModel = null;
        private SignInView view = null;
        private bool _windowClosedBySelf = false; // Tracks whether the window was closed by logic internal
                                                  // to the command (e.g. not by clicking the window's close button)
        // Presents the user with a sign-in dialog
        public override void Execute(object parameter)
        {
            // Initialize the viewmodel if not yet done
            if (viewModel == null)
            {
                // Instantiate the viewmodel and sign-in/cancel commands
                viewModel = new SignInViewModel();
                DelegateCommand signInCommand = new DelegateCommand(onSignIn, canSignIn);
                viewModel.SignIn = signInCommand;
                viewModel.Cancel = new DelegateCommand(onCancel);

                // When a property changes on the viewmodel, have the sign-in command check its executable state
                viewModel.PropertyChanged += (o, e) => signInCommand.RaiseCanExecuteChanged();

                // instantiate the view
                view = new SignInView() { Margin = new Thickness(10) };

                // plug the viewmodel into the view
                view.DataContext = viewModel;
            }

            // Prompt for sign-in
            if (MapApplication.Current != null)
                MapApplication.Current.ShowWindow(Strings.SignIn, view, true, null, onWindowClosed,
                    WindowType.DesignTimeFloating);
        }

        private string _proxyUrl;
        /// <summary>
        /// Gets or sets the URL to the ArcGIS Server to sign into
        /// </summary>
        public string ProxyUrl
        {
            get { return _proxyUrl; }
            set
            {
                _proxyUrl = value;
                RaisePropertyChanged("ProxyUrl");
            }
        }

        #region Command Handling - canSignIn, onSignIn, onCancel

        /// <summary>
        /// Gets whether authentication is possible given the current credentials
        /// </summary>
        protected virtual bool canSignIn(object parameter)
        {
            return viewModel != null 
                && !string.IsNullOrEmpty(viewModel.Username)
                && !string.IsNullOrEmpty(viewModel.Password);
        }

        /// <summary>
        /// Attempts to authenticate based on user-provided credentials
        /// </summary>
        protected abstract void onSignIn(object parameter);
        
        /// <summary>
        /// Cancels the sign-in operation
        /// </summary>
        protected virtual void onCancel(object parameter)
        {
            if (viewModel != null)
                viewModel.SigningIn = false;
            closeWindow();
            OnCancelled();
        }

        #endregion

        #region Window Management - onWindowClosed, closeWindow

        /// <summary>
        /// Fires when the window holding the sign-in UI is closed
        /// </summary>
        protected void onWindowClosed(object sender, EventArgs e)
        {
            if (!_windowClosedBySelf)
                OnCancelled();
            _windowClosedBySelf = false;
        }

        /// <summary>
        /// Closes the window holding the sign-in UI
        /// </summary>
        protected void closeWindow()
        {
            if (view == null)
                return;

            _windowClosedBySelf = true;
            MapApplication.Current.HideWindow(view);
        }

        #endregion

        #region Events - SignedIn, Cancelled

        /// <summary>
        /// Occurs when sign-in to the current ArcGIS Online or Portal instance is successful
        /// </summary>
        public event EventHandler<SignedInEventArgs> SignedIn;

        /// <summary>
        /// Fires the <see cref="SignedIn"/> event
        /// </summary>
        protected virtual void OnSignedIn(IdentityManager.Credential cred)
        {
            if (SignedIn != null)
                SignedIn(this, new SignedInEventArgs(cred));
        }

        /// <summary>
        /// Occurs when sign-in is cancelled
        /// </summary>
        public event EventHandler Cancelled;

        /// <summary>
        /// Fires the <see cref="Cancelled"/> event
        /// </summary>
        protected virtual void OnCancelled()
        {
            if (Cancelled != null)
                Cancelled(this, EventArgs.Empty);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Executes sign-in failure logic
        /// </summary>
        protected virtual void onSignInFailed(Exception ex, bool showWindow = false)
        {
            var message = StringResourcesManager.Instance.Get("SignInFailed");
            if (viewModel != null)
            {
                viewModel.SignInError = message;
                viewModel.SigningIn = false;
            }

            if (showWindow)
            {
                MessageBoxDialog.Show(message, StringResourcesManager.Instance.Get("ErrorCaption"), MessageType.Error, MessageBoxButton.OK, (o, e) =>
                    {
                        // Raise cancelled event once failed message box has been dismissed
                        OnCancelled();
                    });
            }

            Logger.Instance.LogError(ex);
        }

        #endregion
    }

    /// <summary>
    /// Represents the results of a sign-in operation
    /// </summary>
    public class SignedInEventArgs : EventArgs
    {
        public SignedInEventArgs(IdentityManager.Credential cred)
        {
            Credential = cred;
        }

        /// <summary>
        /// Gets the credential retrieved by signing in
        /// </summary>
        public IdentityManager.Credential Credential { get; private set; }
    }
}
