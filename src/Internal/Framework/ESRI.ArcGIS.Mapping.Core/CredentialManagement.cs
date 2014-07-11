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

namespace ESRI.ArcGIS.Mapping.Core
{
    /// <summary>
    /// Provides an application-wide credential store
    /// </summary>
    public class CredentialManagement : DependencyObject
    {
        public CredentialManagement()
        {
            if (DesignerProperties.IsInDesignTool)
                return;
        }

        private static CredentialManagement current;
        /// <summary>
        /// Gets the singleton instance of the application's credential store
        /// </summary>
        public static CredentialManagement Current
        {
            get
            {
                if (current == null)
                    current = new CredentialManagement();
                return current;
            }
        }

        private static ObservableCollection<IdentityManager.Credential> credentials = 
            new ObservableCollection<IdentityManager.Credential>();
        /// <summary>
        /// Gets the credentials for all the user accounts currently logged in within the 
        /// application environment
        /// </summary>
        public ObservableCollection<IdentityManager.Credential> Credentials { get { return credentials; } }
    }
}
