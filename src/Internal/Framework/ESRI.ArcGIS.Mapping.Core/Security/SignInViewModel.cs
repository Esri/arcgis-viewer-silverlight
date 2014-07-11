/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ESRI.ArcGIS.Mapping.Core
{
    /// <summary>
    /// Provides logic and UI support for authenticating against secure services
    /// </summary>
    public class SignInViewModel : DependencyObject, INotifyPropertyChanged
    {
        #region Public Properties 

        #region SignInLabel
        /// <summary>
        /// Backing DependencyProperty for the <see cref="SignInLabel"/> property
        /// </summary>
        public static readonly DependencyProperty SignInLabelProperty = DependencyProperty.Register(
            "SignInLabel", typeof(string), typeof(SignInViewModel), null);

        /// <summary>
        /// Gets or sets the descriptive text for the sign-in UI
        /// </summary>
        public string SignInLabel
        {
            get { return GetValue(SignInLabelProperty) as string; }
            set { SetValue(SignInLabelProperty, value); }
        }
        #endregion

        #region SigningIn
        /// <summary>
        /// Backing DependencyProperty for the <see cref="SigningIn"/> property
        /// </summary>
        public static readonly DependencyProperty SigningInProperty = DependencyProperty.Register(
            "SigningIn", typeof(bool), typeof(SignInViewModel), null);

        /// <summary>
        /// Gets or sets whether a sign-in operation is currently executing
        /// </summary>
        public bool SigningIn
        {
            get { return (bool)GetValue(SigningInProperty); }
            set { SetValue(SigningInProperty, value); }
        }
        #endregion

        #region PromptForUrl
        /// <summary>
        /// Backing DependencyProperty for the <see cref="PromptForUrl"/> property
        /// </summary>
        public static readonly DependencyProperty PromptForUrlProperty = DependencyProperty.Register(
            "PromptForUrl", typeof(bool), typeof(SignInViewModel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets whether the user should be prompted for the URL to attempt to authenticate against
        /// </summary>
        public bool PromptForUrl
        {
            get { return (bool)GetValue(PromptForUrlProperty); }
            set { SetValue(PromptForUrlProperty, value); }
        }
        #endregion

        #region Username
        /// <summary>
        /// Backing DependencyProperty for the <see cref="Username"/> property
        /// </summary>
        public static readonly DependencyProperty UsernameProperty = DependencyProperty.Register(
            "Username", typeof(string), typeof(SignInViewModel), new PropertyMetadata(OnUserNameChanged));

        /// <summary>
        /// Gets or sets the user name to use for authentication
        /// </summary>
        public string Username
        {
            get { return GetValue(UsernameProperty) as string; }
            set { SetValue(UsernameProperty, value); }
        }

        // Fires when the user name changes
        private static void OnUserNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SignInViewModel)d).OnPropertyChanged("UserName");
        }
        #endregion

        #region Password
        /// <summary>
        /// Backing DependencyProperty for the <see cref="Password"/> property
        /// </summary>
        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
            "Password", typeof(string), typeof(SignInViewModel), new PropertyMetadata(OnPasswordChanged));

        /// <summary>
        /// Gets or sets the password to use for authentication
        /// </summary>
        public string Password
        {
            get { return GetValue(PasswordProperty) as string; }
            set { SetValue(PasswordProperty, value); }
        }

        // Fires when the password changes
        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SignInViewModel)d).OnPropertyChanged("Password");
        }
        #endregion

        #region Url
        /// <summary>
        /// Backing DependencyProperty for the <see cref="Url"/> property
        /// </summary>
        public static readonly DependencyProperty UrlProperty = DependencyProperty.Register(
            "Url", typeof(string), typeof(SignInViewModel), new PropertyMetadata(OnUrlChanged));

        /// <summary>
        /// Gets or sets the URL of the secure resource
        /// </summary>
        public string Url
        {
            get { return GetValue(UrlProperty) as string; }
            set { SetValue(UrlProperty, value); }
        }

        // Fires when the secure resource URL changes
        private static void OnUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SignInViewModel)d).OnPropertyChanged("Url");
        }
        #endregion

        #region SignInError
        /// <summary>
        /// Backing DependencyProperty for the <see cref="SignInError"/> property
        /// </summary>
        public static readonly DependencyProperty SignInErrorProperty = DependencyProperty.Register(
            "SignInError", typeof(string), typeof(SignInViewModel), null);

        /// <summary>
        /// Gets or sets the error that occurred during a sign-in operation
        /// </summary>
        public string SignInError
        {
            get { return GetValue(SignInErrorProperty) as string; }
            set { SetValue(SignInErrorProperty, value); }
        }
        #endregion

        #region Commands - SignIn, Cancel

        /// <summary>
        /// Backing DependencyProperty for the <see cref="SignIn"/> property
        /// </summary>
        public static readonly DependencyProperty SignInProperty = DependencyProperty.Register(
            "SignIn", typeof(ICommand), typeof(SignInViewModel), null);

        /// <summary>
        /// Gets or sets the command used for signing in
        /// </summary>
        public ICommand SignIn
        {
            get { return GetValue(SignInProperty) as ICommand; }
            set { SetValue(SignInProperty, value); }
        }


        /// <summary>
        /// Backing DependencyProperty for the <see cref="Cancel"/> property
        /// </summary>
        public static readonly DependencyProperty CancelProperty = DependencyProperty.Register(
            "Cancel", typeof(ICommand), typeof(SignInViewModel), null);

        /// <summary>
        /// Gets the command used for cancelling the sign-in operation
        /// </summary>
        public ICommand Cancel
        {
            get { return GetValue(CancelProperty) as ICommand; }
            set { SetValue(CancelProperty, value); }
        }

        private void onCancel(object parameter)
        {
            OnCancelled();
        }

        #endregion

        #endregion

        #region Events - SignedIn, Cancelled, PropertyChanged

        /// <summary>
        /// Occurs when accessing the secure resource instance is successful
        /// </summary>
        public event EventHandler SignedIn;

        // Fires the SignedIn event
        private void OnSignedIn()
        {
            if (SignedIn != null)
                SignedIn(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when sign-in is cancelled
        /// </summary>
        public event EventHandler Cancelled;

        // Fires the Cancelled event
        private void OnCancelled()
        {
            if (Cancelled != null)
                Cancelled(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when a property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        // Fires the PropertyChanged event
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
