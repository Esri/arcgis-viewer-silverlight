/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;

namespace $safeprojectname$
{
    [Export(typeof(Behavior<Map>))]
    [DisplayName("My Behavior")]
    public class MyBehavior : Behavior<Map>, ISupportsConfiguration
    {
        private MyConfigDialog configDialog = new MyConfigDialog();

        #region ISupportsConfiguration members

        public void Configure()
        {
            // When the dialog opens, it shows the information saved from the last configuration
            MapApplication.Current.ShowWindow("Configuration", configDialog);
        }

        public void LoadConfiguration(string configData)
        {
            // Initialize the behavior's configuration with the saved configuration data. 
            // The dialog's textbox is used to store the configuration.
            if (configData == null)
                configData = string.Empty;
            configDialog.InputTextBox.Text = configData;
        }

        public string SaveConfiguration()
        {
            // Save the information from the configuration dialog
            return configDialog.InputTextBox.Text;
        }

        #endregion

        #region Behavior Overrides
        protected override void OnAttached()
        {
            base.OnAttached();
            // Show saved configuration text
            MapApplication.Current.ShowWindow("My Behavior", new TextBlock()
            {
                Text = "The saved configuration string is: '" + configDialog.InputTextBox.Text + "'",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(30),
                MaxWidth = 480
            });
        }
        #endregion
    }
}
