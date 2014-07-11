/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Client.Extensibility;
using System.Windows;
using System.Collections.ObjectModel;
using System;

namespace ESRI.ArcGIS.Mapping.Controls.Editor
{
    public class GenericConfigControl : Control, ICommand, ISupportsConfiguration
    {
        public GenericConfigControl()
        {
            this.DefaultStyleKey = typeof(GenericConfigControl);
        }

        #region ConfigCommand
        public ISupportsConfiguration Command
        {
            get { return (ISupportsConfiguration)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ISupportsConfiguration), typeof(GenericConfigControl), null);

        #endregion

        #region Title
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(GenericConfigControl), null);

        #endregion

        #region ISupportsConfiguration
        public void Configure()
        {
            if (Command != null)
                Command.Configure();
        }

        public void LoadConfiguration(string configData)
        {
            if (Command != null)
                Command.LoadConfiguration(configData);
        }

        public string SaveConfiguration()
        {
            if (Command != null)
                return Command.SaveConfiguration();

            return null;
        }

        #endregion

        #region ICommand
        public bool CanExecute(object parameter)
        {
            //return true;
            return MapApplication.Current.IsEditMode;
        }

        public event System.EventHandler CanExecuteChanged;
        protected void OnCanExecuteChanged(EventArgs args)
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, args);
        }

        public void Execute(object parameter)
        {
            Configure();
        }
        #endregion
    }
}
