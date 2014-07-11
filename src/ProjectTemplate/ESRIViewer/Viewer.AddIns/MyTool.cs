/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;

namespace $safeprojectname$
{
    [Export(typeof(ICommand))]
    [DisplayName("My Tool")]
    public class MyTool : ICommand, ISupportsConfiguration
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
            configDialog.InputTextBox.Text = configData ?? "";
        }

        public string SaveConfiguration()
        {
            // Save the information from the configuration dialog
            return configDialog.InputTextBox.Text;
        }

        #endregion

        #region ICommand members
        public void Execute(object parameter)
        {
            // Show the configuration data
            MapApplication.Current.ShowWindow("My Tool", new TextBlock()
            {
                Text = "The saved configuration is: '" + configDialog.InputTextBox.Text + "'",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(30),
                MaxWidth = 480
            });
        }

        public bool CanExecute(object parameter)
        {
            // Return true so that the command can always be executed
            return true;
        }

        public event EventHandler CanExecuteChanged;

        #endregion
    }
}
