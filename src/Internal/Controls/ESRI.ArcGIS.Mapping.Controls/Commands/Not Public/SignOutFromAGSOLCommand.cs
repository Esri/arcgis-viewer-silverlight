/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls.ArcGISOnline;
using ESRI.ArcGIS.Mapping.Core;
using System.Linq;
using System.Collections.Generic;
using System;

namespace ESRI.ArcGIS.Mapping.Controls
{
    /// <summary>
    /// Signs out from the currently connected instance of ArcGIS Online or Portal
    /// </summary>
    public class SignOutFromAGSOLCommand : CommandBase
    {
        public async override void Execute(object parameter)
        {
            // Get the portal and check whether a user is signed in
            ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnline agol = ArcGISOnlineEnvironment.ArcGISOnline;
            if (agol != null && agol.User != null && agol.User.IsSignedIn)
            {
                // Remove the current user credentials from the application environment, if present
                IEnumerable<IdentityManager.Credential> credentials =
                    UserManagement.Current.Credentials.Where(c => 
                    c.Url.Equals(agol.Url, StringComparison.OrdinalIgnoreCase)
                    && c.UserName.Equals(agol.User.Current.Username, StringComparison.OrdinalIgnoreCase));

                if (credentials != null)
                {
                    int count = credentials.Count();
                    for (int i = 0; i < count; i++)
                        UserManagement.Current.Credentials.Remove(credentials.ElementAt(0));
                }

                // Update the user info on MapApplication.Current's portal instance
                if (MapApplication.Current != null && MapApplication.Current.Portal != null)
                {
                    MapApplication.Current.Portal.Token = null;
                    await MapApplication.Current.Portal.InitializeTaskAsync();
                }

                // Sign the user out from ArcGIS Portal
                ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnlineEnvironment.ArcGISOnline.User.SignOut();
            }
        }
    }
}
