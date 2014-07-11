/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls.ArcGISOnline;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    /// <summary>
    /// Signs out all logged-in users
    /// </summary>
    public class SignOutAllCommand : CommandBase
    {
        private SignOutFromAGSOLCommand signOutAgol;
        public override void Execute(object parameter)
        {
            // Sign out from the current ArcGIS Online or Portal instance
            if (signOutAgol == null)
                signOutAgol = new SignOutFromAGSOLCommand();
            signOutAgol.Execute(parameter);

            // Clear the application environment's set of credentials
            UserManagement.Current.Credentials.Clear();
        }
    }
}
