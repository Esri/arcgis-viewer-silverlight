/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using ESRI.ArcGIS.Client.Extensibility;
using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Client.Application.Controls;

namespace ESRI.ArcGIS.Mapping.Controls.Editor
{
    public class WizardCommand : ICommand, INotifyPropertyChanged
    {
        #region DialogTitle
        public string DialogTitle
        {
            get;
            set;
        }
        #endregion

        #region IsModal
        private bool _isModal = true;
        public bool IsModal
        {
            get
            {
                return _isModal;
            }
            set
            {
                _isModal = value;
            }
        }
        #endregion

        #region WizardConfiguration
        private ISupportsWizardConfiguration _wizardConfiguration;
        public ISupportsWizardConfiguration WizardConfiguration
        {
            get
            {
                return _wizardConfiguration;
            }
            set
            {
                _wizardConfiguration = value;
                OnPropertyChanged("WizardConfiguration");
            }
        }
        #endregion

        #region WizardStyle
        private Style _wizardStyle;
        public Style WizardStyle
        {
            get
            {
                return _wizardStyle;
            }
            set
            {
                _wizardStyle = value;
                OnPropertyChanged("WizardStyle");
            }
        }
        #endregion

        private Wizard configurationUI;
        private bool _configurationComplete = false;
        #region ICommand members

        /// <summary>
        /// Displays the configuration UI
        /// </summary>
        /// <param name="parameter">The object to be configured as a tool panel item</param>
        public void Execute(object parameter)
        {
            if (configurationUI == null)
            {
                // Get the command to configure
                WizardConfiguration = parameter as ISupportsWizardConfiguration ?? WizardConfiguration;
                if (WizardConfiguration != null)
                    configurationUI = createWizard();

            }

            if (configurationUI.CurrentPage == null && configurationUI.Pages != null &&
                configurationUI.Pages.Count > 0)
                configurationUI.CurrentPage = configurationUI.Pages[0];

            MapApplication.Current.ShowWindow(DialogTitle, configurationUI, IsModal, null, (e,o)=>
            {
                Wizard wiz = e as Wizard;
                if (wiz != null)
                    wiz.CurrentPage = null;

                if (_configurationComplete)
                {
                    // Hide configuration UI and fire the completed event
                    if (WizardConfiguration != null)
                        WizardConfiguration.OnCompleted();

                    if (Completed != null)
                        Completed(this, EventArgs.Empty);
                }
                else
                {
                    if (WizardConfiguration != null)
                        WizardConfiguration.OnCancelled();

                    if (Cancelled != null)
                        Cancelled(this, EventArgs.Empty);
                }
                _configurationComplete = false;
            }, WindowType.DesignTimeFloating);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        protected void OnCanExecuteChanged(EventArgs args)
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, args);
        }

        public event EventHandler CanExecuteChanged;

        #endregion

        #region Event handling

        private void Wizard_PageChanged(object sender, EventArgs e)
        {
            Wizard wizard = sender as Wizard;
            // If underlying command implements ISupportsWizardConfiguration, update CurrentPage and
            // fire Configure method
            if (WizardConfiguration != null)
                WizardConfiguration.CurrentPage = wizard.CurrentPage;
        }

        private void Wizard_PageChanging(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Wizard wizard = sender as Wizard;
            // If underlying command implements ISupportsWizardConfiguration, fire PageChanging method 
            // and cancel wizard page change accordingly
            if (WizardConfiguration != null)
                e.Cancel = !WizardConfiguration.PageChanging();
        }

        private void Configuration_Completed(object sender, EventArgs e)
        { 
            _configurationComplete = true;
            MapApplication.Current.HideWindow(configurationUI);
        }

        private void Configuration_Cancelled(object sender, EventArgs e)
        {
            _configurationComplete = false;
            MapApplication.Current.HideWindow(configurationUI);
        }

        #endregion

        public event EventHandler<EventArgs> Completed;
        public event EventHandler<EventArgs> Cancelled;

        #region UI Generation

        private Wizard createWizard()
        {
            Wizard wizard = new Wizard();
            if (_wizardStyle != null)
                wizard.Style = WizardStyle;
            // Get wizard config interface and use to initialize wizard size
            wizard.ContentHeight = WizardConfiguration.DesiredSize.Height;
            wizard.ContentWidth = WizardConfiguration.DesiredSize.Width;

            // initialize wizard pages
            IEnumerable<WizardPage> pages = WizardConfiguration.Pages;
            foreach (WizardPage page in pages)
                wizard.Pages.Add(page);

            // Wire events
            wizard.PageChanging += Wizard_PageChanging;
            wizard.PageChanged += Wizard_PageChanged;
            wizard.Cancelled += Configuration_Cancelled;
            wizard.Completed += Configuration_Completed;

            return wizard;
        }

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
