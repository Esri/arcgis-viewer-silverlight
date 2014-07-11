/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Controls.Resources;
using ESRI.ArcGIS.Mapping.Controls.ArcGISOnline;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    /// <summary>
    /// Provides application-wide management of users.  Allows monitoring of, manipulation of, and binding
    /// to users within the current application environment
    /// </summary>
    public class UserManagement : DependencyObject
    {
        static bool hookedToCredentialsChanged = false;
        public UserManagement()
        {
            if (!DesignerProperties.IsInDesignTool && ArcGISOnlineEnvironment.ArcGISOnline != null)
            {
                // try-catch is required for when UserManagement is used in applications that don't
                // leverage the ArcGISOnlineEnvironment.
                try
                {
                    // Bind the User property to the current ArcGISOnlineEnvironment user
                    Binding b = new Binding("User.Current") { Source = ArcGISOnlineEnvironment.ArcGISOnline };
                    BindingOperations.SetBinding(this, UserProperty, b);

                    // ArcGIS Portals have two names.  Where the actual title of the portal is stored is
                    // not consistent.  So to distill these into one display name, both properties need
                    // to be monitored, and rules need to be applied depending on how they are populated.

                    // To monitor the portal names, bind them to two private dependency properties
                    b = new Binding("PortalInfo.PortalName") { Source = ArcGISOnlineEnvironment.ArcGISOnline };
                    BindingOperations.SetBinding(this, InternalPortalNameProperty, b);
                    b = new Binding("PortalInfo.Name") { Source = ArcGISOnlineEnvironment.ArcGISOnline };
                    BindingOperations.SetBinding(this, FallbackPortalNameProperty, b);

                    if (!hookedToCredentialsChanged)
                    {
                        // Initialize the static credentials collection by copying the set of 
                        // IdentityManager credentials to it
                        copyIdentityManagerCredentials();

                        // Listen for changes on the static credentials collection
                        Credentials.CollectionChanged += Credentials_CollectionChanged;

                        // Set the flag so that the collection is only ever hooked to once, no matter how
                        // many instances of UserManagement are created
                        hookedToCredentialsChanged = true;
                    }
                }
                catch
                {
                    // swallow binding exceptions
                }
            }
        }

        private static UserManagement current;
        /// <summary>
        /// Gets the singleton instance of the application's user management environment
        /// </summary>
        public static UserManagement Current 
        { 
            get 
            {
                if (current == null)
                    current = new UserManagement();
                return current; 
            } 
        }

        /// <summary>
        /// Identifies the <see cref="PortalName"/> DependencyProperty
        /// </summary>
        public static DependencyProperty PortalNameProperty = DependencyProperty.Register(
            "PortalName", typeof(string), typeof(UserManagement), null);

        /// <summary>
        /// Gets the name of the application's currently connected ArcGIS Online or Portal instance
        /// </summary>
        public string PortalName
        {
            get { return GetValue(PortalNameProperty) as string; }
            private set { SetValue(PortalNameProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="InternalPortalName"/> DependencyProperty
        /// </summary>
        private static DependencyProperty InternalPortalNameProperty = DependencyProperty.Register(
            "InternalPortalName", typeof(string), typeof(UserManagement), 
            new PropertyMetadata(OnPortalNameChanged));

        /// <summary>
        /// Gets or sets the primary option to be used as the application environment's portal name
        /// </summary>
        private string InternalPortalName
        {
            get { return GetValue(InternalPortalNameProperty) as string; }
            set { SetValue(InternalPortalNameProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="FallbackPortalName"/> DependencyProperty
        /// </summary>
        private static DependencyProperty FallbackPortalNameProperty = DependencyProperty.Register(
            "FallbackPortalName", typeof(string), typeof(UserManagement), 
            new PropertyMetadata(OnPortalNameChanged));

        /// <summary>
        /// Gets or sets the secondary option to be used as the application environment's portal name
        /// </summary>
        private string FallbackPortalName
        {
            get { return GetValue(FallbackPortalNameProperty) as string; }
            set { SetValue(FallbackPortalNameProperty, value); }
        }

        // Fires when either of the properties changes that might store the current ArcGIS Portal's name
        private static void OnPortalNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UserManagement o = ((UserManagement)d);
            if (!string.IsNullOrEmpty(o.InternalPortalName)
            && o.InternalPortalName != Strings.ArcGISPortalDefaultName
            && o.InternalPortalName != Strings.ArcGISOnline)
                o.PortalName = o.InternalPortalName; // PortalName is the primary option
            else if (!string.IsNullOrEmpty(o.FallbackPortalName)
            && o.FallbackPortalName != Strings.ArcGISPortalDefaultName
            && o.FallbackPortalName != Strings.ArcGISOnline)
                o.PortalName = o.FallbackPortalName; // Name is the fallback
            else if (o.InternalPortalName != null)
                o.PortalName = o.InternalPortalName;
            else if (o.FallbackPortalName != null)
                o.PortalName = o.FallbackPortalName;
            else
                o.PortalName = Strings.ArcGISOnline; // If neither are populated, fall back to a generic 
                                                     // "ArcGIS Online" string
        }

        /// <summary>
        /// Identifies the <see cref="User"/> DependencyProperty
        /// </summary>
        public static DependencyProperty UserProperty = DependencyProperty.Register(
            "User", typeof(User), typeof(UserManagement), new PropertyMetadata(OnUserChanged));

        /// <summary>
        /// Gets the application environment's currently logged-in ArcGIS Online or Portal user
        /// </summary>
        public User User
        {
            get { return GetValue(UserProperty) as User; }
            private set { SetValue(UserProperty, value); }
        }

        // Fires when the current ArcGIS Online or Portal user changes
        private static void OnUserChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((UserManagement)d).copyIdentityManagerCredentials();
        }
        
        // Copies credentials from IdentityManager into UserManagement's static collection
        private void copyIdentityManagerCredentials()
        {
            if (IdentityManager.Current != null && IdentityManager.Current.Credentials != null)
            {
                int count = Credentials.Count;
                List<IdentityManager.Credential> credentialsToRemove = new List<IdentityManager.Credential>();
                
                // Go through the credentials in UserManagement's collection.  If the credential is not
                // present in IdentityManager's collection, flag it for removal
                foreach (IdentityManager.Credential cred in Credentials)
                {
                    if (!IdentityManager.Current.Credentials.Contains(cred))
                        credentialsToRemove.Add(cred);
                }
                // Remove the flagged credentials from UserManagement's collection
                credentialsToRemove.ForEach(c => Credentials.Remove(c));

                // Add any credential to UserManagement's collection that is present in IdentityManager but
                // not in UserManagement
                foreach (IdentityManager.Credential cred in IdentityManager.Current.Credentials)
                {
                    if (!Credentials.Any(c => c.Url.ToLower() == cred.Url.ToLower()))
                        Credentials.Add(cred);
                }
            }
        }

        /// <summary>
        /// Gets the credentials for all the user accounts currently logged in within the 
        /// application environment
        /// </summary>
        public ObservableCollection<IdentityManager.Credential> Credentials 
        { 
            get { return CredentialManagement.Current.Credentials; } 
        }

        // Fires when a change is made to the set of credentials
        private void Credentials_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IdentityManager.Current == null)
                return;

            // If the collection has been cleared, remove all credentials from IdentityManager
            if (e.Action == NotifyCollectionChangedAction.Reset && e.OldItems == null && e.NewItems == null)
            {
                int count = IdentityManager.Current.Credentials.Count();
                for (int i = 0; i < count; i++)
                    IdentityManager.Current.RemoveCredential(IdentityManager.Current.Credentials.ElementAt(0));

                return;
            }


            // Otherwise, sync removed or added credentials with IdentityManager

            if (e.OldItems != null)
            {
                foreach (IdentityManager.Credential cred in e.OldItems)
                    IdentityManager.Current.RemoveCredential(cred);
            }

            if (e.NewItems != null)
            {
                foreach (IdentityManager.Credential cred in e.NewItems)
                    IdentityManager.Current.AddCredential(cred);
            }
        }
    }
}
