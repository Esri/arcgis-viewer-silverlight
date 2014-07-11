/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Controls.Resources;
using ESRI.ArcGIS.Mapping.Controls.ArcGISOnline;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Controls
{
    /// <summary>
    /// Provides a UI to sign into the application environment's ArcGIS Online or Portal instance and 
    /// ArcGIS Server instances and displays sign-in status
    /// </summary>
    public class SignInStatusControl : Control
    {
        ThrottleTimer _updateDisplayNameTimer; // Updates the display name when the throttle interval expires
        public SignInStatusControl()
        {
            DefaultStyleKey = typeof(SignInStatusControl);
            DataContext = this;

            // Initialize the display name update timer.  Multiple credential change events may fire in 
            // rapid succession.  Throttling them makes it so that the display name update only fires once,
            // after all the change events are complete.
            _updateDisplayNameTimer = new ThrottleTimer(20, () => updateUserDisplayName());

            updateSignOutVisibility();
        }

        /// <summary>
        /// Identifies the <see cref="PortalUser"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PortalUserProperty =
            DependencyProperty.Register(
                "PortalUser",
                typeof(User),
                typeof(SignInStatusControl), new PropertyMetadata(OnPortalUserChanged));

        /// <summary>
        /// Gets or sets the user to sign-in as
        /// </summary>
        public User PortalUser
        {
            get { return GetValue(PortalUserProperty) as User; }
            set { SetValue(PortalUserProperty, value); }
        }

        // Fires when the PortalUser is changed
        private static void OnPortalUserChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SignInStatusControl)d)._updateDisplayNameTimer.Invoke();
        }

        /// <summary>
        /// Identifies the <see cref="Credentials"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CredentialsProperty =
            DependencyProperty.Register(
                "Credentials",
                typeof(ObservableCollection<IdentityManager.Credential>),
                typeof(SignInStatusControl), new PropertyMetadata(OnCredentialsChanged));

        /// <summary>
        /// Gets or sets the set of credentials that are monitored by the control
        /// </summary>
        public ObservableCollection<IdentityManager.Credential> Credentials
        {
            get { return GetValue(CredentialsProperty) as ObservableCollection<IdentityManager.Credential>; }
            set { SetValue(CredentialsProperty, value); }
        }

        // Fires when the Credentials property is changed
        private static void OnCredentialsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SignInStatusControl o = (SignInStatusControl)d;
            o._updateDisplayNameTimer.Invoke();

            o.updateSignOutVisibility();

            // Unhook from the previous collection
            if (e.OldValue != null)
            {
                ((ObservableCollection<IdentityManager.Credential>)e.OldValue).CollectionChanged
                    -= o.Credentials_CollectionChanged;
            }

            // Hook to the new collection
            if (e.NewValue != null)
            {
                ((ObservableCollection<IdentityManager.Credential>)e.NewValue).CollectionChanged 
                    += o.Credentials_CollectionChanged;
            }
        }

        /// <summary>
        /// Identifies the <see cref="UserDisplayName"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty UserDisplayNameProperty =
            DependencyProperty.Register(
                "UserDisplayName",
                typeof(string),
                typeof(SignInStatusControl), null);

        /// <summary>
        /// Gets or sets the display name of the currently signed-in user
        /// </summary>
        public string UserDisplayName
        {
            get { return GetValue(UserDisplayNameProperty) as string; }
            set { SetValue(UserDisplayNameProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="PortalName"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PortalNameProperty =
            DependencyProperty.Register(
                "PortalName",
                typeof(string),
                typeof(SignInStatusControl), null);

        /// <summary>
        /// Gets or sets the display name of the current ArcGIS Online or Portal instance
        /// </summary>
        public string PortalName
        {
            get { return GetValue(PortalNameProperty) as string; }
            set { SetValue(PortalNameProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="SignOutVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SignOutVisibilityProperty =
            DependencyProperty.Register(
                "SignOutVisibility",
                typeof(Visibility),
                typeof(SignInStatusControl), 
                new PropertyMetadata(Visibility.Collapsed));

        /// <summary>
        /// Gets or sets the visibility of the sign out link
        /// </summary>
        public Visibility SignOutVisibility
        {
            get { return (Visibility)GetValue(SignOutVisibilityProperty); }
            set { SetValue(SignOutVisibilityProperty, value); }
        }

        // Fires when the current set of credentials is modified
        private void Credentials_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            updateSignOutVisibility();
            _updateDisplayNameTimer.Invoke();
        }

        // Updates the display name based on the state of the credentials being monitored by the control
        private void updateUserDisplayName()
        {
            if (PortalUser != null && !string.IsNullOrEmpty(PortalUser.FullName))
            {
                // If there is a portal/AGOL user, its name takes precedence because it is typically
                // the most user-friendly
                UserDisplayName = PortalUser.FullName;
            }
            else if (Credentials != null && Credentials.Count > 0)
            {
                // If there are any credentials, use the username from the first one that has a token
                IdentityManager.Credential cred = null;
                Func<IdentityManager.Credential, bool> condition = c => !string.IsNullOrEmpty(c.Token);
                if (Credentials.Any(condition))
                    cred = Credentials.First(condition);

                UserDisplayName = cred != null ? cred.UserName : null;
            }
            else
            {
                // No users, so set the label to prompt users to sign in
                UserDisplayName = Strings.SignIn;
            }
        }

        // Updates the visibility of the sign-out link based on the currently specified credentials
        private void updateSignOutVisibility()
        {
            if (Credentials == null)
                return;

            // Get credentials not related to authentication methods handled by the browser, such as PKI or IWA.
            // Browser auth credentials will have no token.
            IEnumerable<IdentityManager.Credential> credentials = Credentials.Where(c => c.Token != null);
            SignOutVisibility = credentials.Count() > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
