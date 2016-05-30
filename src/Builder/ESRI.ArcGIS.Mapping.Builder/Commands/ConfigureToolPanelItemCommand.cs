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
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Mapping.Builder.Controls;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace ESRI.ArcGIS.Mapping.Builder
{
    /// <summary>
    /// Presents the UI to configure a command for adding to the application as a tool.  Handles
    /// presentation of additional configuration UI where the command implements ISupportsConfiguration
    /// or ISupportsWizardConfiguration.
    /// </summary>
    public class ConfigureToolPanelItemCommand : CommandBase
    {
        private int _locallyInsertedWizardPagesCount; //count of wizard pages inserted by this command
        private FrameworkElement configurationUI;

        // stores config wizards for tools that have already been configured
        private Dictionary<object, Wizard> toolConfigWizards = new Dictionary<object,Wizard>(); 

        /// <summary>
        /// The command to configure
        /// </summary>
        public object Class { get; set; } 

        public string DialogTitle { get; set; }
        public ButtonDisplayInfo DisplayInfo { get; set; }
        public bool AllowContainerSelection { get; set; }
        public bool AllowToolSelection { get; set; }

        public ToolPanel ToolPanel { get; private set; }

        private bool completed;
        private bool cancelInvokedLocally;
        private bool windowClosedLocally;

        #region ICommand members

        /// <summary>
        /// Displays the configuration UI
        /// </summary>
        /// <param name="parameter">The object to be configured as a tool panel item</param>
        public override void Execute(object parameter)
        {
            // Reset count of pages inserted by this command
            _locallyInsertedWizardPagesCount = 0;

            // Get the command to configure
            Class = parameter ?? Class;
            bool supportsWizardConfiguration = Class as ISupportsWizardConfiguration != null;

            // Unhook event handlers from previous config wizard before a new one is created
            if (configurationUI is Wizard)
            {
                Wizard wizard = (Wizard)configurationUI;
                wizard.PageChanged -= Wizard_PageChanged;
                wizard.PageChanging -= Wizard_PageChanging;
                wizard.Cancelled -= Wizard_Cancelled;
                wizard.Completed -= Wizard_Completed;
            }

            if (AllowToolSelection)
            {
                configurationUI = createWizard();
            }
            else if (supportsWizardConfiguration)
            {
                // Check whether the config wizard for the tool being configured has already been created.
                // Re-using the existing UI is important so that the contentn of a wizard page is never 
                // hosted in multiple elements, which throws an error.
                if (toolConfigWizards.ContainsKey(Class))
                {
                    // Use the existing wizard
                    configurationUI = toolConfigWizards[Class];
                }
                else
                {
                    // Create the wizard for the tool and save it for later use
                    configurationUI = createWizard();
                    toolConfigWizards.Add(Class, (Wizard)configurationUI);
                }
            }
            else
            {
                configurationUI = createToolPropertiesUI();
                configurationUI.Margin = new Thickness(10);
                configurationUI.Width = 280;
                ((EditToolbarItemControl)configurationUI).OkClicked += Wizard_Completed;
                ((EditToolbarItemControl)configurationUI).CancelClicked += Cancel_Clicked;
            }

            BuilderApplication.Instance.ShowWindow(DialogTitle, configurationUI, false, null, Window_Closed);
        }

        #endregion

        #region Event handling

        private void Wizard_Completed(object sender, EventArgs e)
        {
            // Get tool properties UI
            EditToolbarItemControl toolPropsUI = null;
            if (sender is EditToolbarItemControl)
                toolPropsUI = sender as EditToolbarItemControl;
            else
            {
                Wizard wizard = sender as Wizard;
                foreach (WizardPage page in wizard.Pages)
                {
                    toolPropsUI = page.Content as EditToolbarItemControl;
                    if (toolPropsUI != null)
                        break;
                }
            }
                
            // Update command properties with the configured values
            ToolPanel = toolPropsUI.SelectedToolPanel;
            DisplayInfo = DisplayInfo ?? new ButtonDisplayInfo();
            DisplayInfo.Label = toolPropsUI.Label;
            DisplayInfo.Description = toolPropsUI.Description;
            DisplayInfo.Icon = toolPropsUI.IconUrl;

            // Set completed flag so window closed handler knows that window was closed as a 
            // result of configuration being completed
            completed = true;
            // Hide configuration UI and fire the completed event
            BuilderApplication.Instance.HideWindow(configurationUI);
            ISupportsWizardConfiguration wizardConfigInterface = Class as ISupportsWizardConfiguration;
            if (wizardConfigInterface != null)
                wizardConfigInterface.OnCompleted();
            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }

        private void Wizard_Cancelled(object sender, EventArgs e)
        {
            if (!cancelInvokedLocally)
            {
                windowClosedLocally = true;
                BuilderApplication.Instance.HideWindow(configurationUI);
            }
        }

        private void Cancel_Clicked(object sender, EventArgs e)
        {
            BuilderApplication.Instance.HideWindow(configurationUI);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Only fire cancel event if window was not closed as a result of configuration
            // completing.
            if (!completed)
            {
                if (configurationUI is Wizard && !windowClosedLocally)
                {
                    // cancellation from window's close button

                    // Invoke cancellation on wizard
                    cancelInvokedLocally = true;
                    ((Wizard)configurationUI).Cancel.Execute(null);
                    cancelInvokedLocally = false;
                }
                else if (windowClosedLocally)
                {
                    windowClosedLocally = false;
                }

                if (Cancelled != null)
                    Cancelled(this, EventArgs.Empty);

                // Notify tool being configured of cancellation
                if (Class is ISupportsWizardConfiguration)
                    ((ISupportsWizardConfiguration)Class).OnCancelled();
            }
            else
            {
                completed = false;
            }
        }

        private void Wizard_PageChanged(object sender, EventArgs e)
        {
            Wizard wizard = sender as Wizard;
            // If underlying command implements ISupportsWizardConfiguration, update CurrentPage and
            // fire Configure method
            ISupportsWizardConfiguration wizardConfigInterface = Class as ISupportsWizardConfiguration;
            if (wizardConfigInterface != null)
            {
                wizardConfigInterface.CurrentPage = wizard.CurrentPage;
                wizardConfigInterface.Configure();
            }
        }

        private void Wizard_PageChanging(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Wizard wizard = sender as Wizard;
            // If underlying command implements ISupportsWizardConfiguration, fire PageChanging method 
            // and cancel wizard page change accordingly
            ISupportsWizardConfiguration wizardConfigInterface = Class as ISupportsWizardConfiguration;
            if (wizardConfigInterface != null)
                e.Cancel = !wizardConfigInterface.PageChanging();
        }

        // If the pages collection of the wizard configuration interface currently being configured changes,
        // update the pages collection of the wizard control hosting the config pages.
        private void WizardInterfacePages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Wizard wizard = configurationUI as Wizard;
            ObservableCollection<WizardPage> wizardPages = sender as ObservableCollection<WizardPage>;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    int i = 0;
                    // Add the number of wizard pages inserted by the command so that the start index for adding
                    // pages accounts for this
                    int insertStart = e.NewStartingIndex + _locallyInsertedWizardPagesCount; 
                    for (int m = insertStart; m < insertStart + e.NewItems.Count; m++)
                    {
                        if (m > 0 && m <= wizard.Pages.Count + 1)
                        {
                            wizard.Pages.Insert(m, e.NewItems[i] as WizardPage);
                            i++;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (WizardPage page in e.OldItems)
                        wizard.Pages.Remove(page);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (WizardPage page in e.OldItems)
                        wizard.Pages.Remove(page);

                    i = 0;
                    for (int m = e.NewStartingIndex; m < e.NewStartingIndex + e.NewItems.Count; m++)
                    {
                        if (_locallyInsertedWizardPagesCount > 0)
                            m = m + _locallyInsertedWizardPagesCount;
                        if (m > 0 && m <= wizard.Pages.Count)
                        {
                            wizard.Pages.Insert(m, e.NewItems[i] as WizardPage);
                            i++;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    int oldItemCount = wizard.Pages.Count;
                    foreach (WizardPage page in wizardPages)
                        wizard.Pages.Add(page);

                    for (i = 0; i < oldItemCount; i++)
                        wizard.Pages.RemoveAt(0);

                    break;
            }
        }

        #endregion

        public event EventHandler<EventArgs> Completed;
        public event EventHandler<EventArgs> Cancelled;

        #region UI Generation

        private Wizard createWizard()
        {
            Wizard wizard = new Wizard();

            // Include page to select tool
            if (AllowToolSelection)
            {
                AvailableToolbarItemsControl toolList = new AvailableToolbarItemsControl(false)
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        Margin = new Thickness(0,5,0,0)
                    };
                WizardPage toolSelectionPage = new WizardPage()
                {
                    Content = toolList,
                    Heading = ESRI.ArcGIS.Mapping.Builder.Resources.Strings.SelectAToolToAdd
                };
                wizard.Pages.Add(toolSelectionPage);
                _locallyInsertedWizardPagesCount++;

                toolList.SelectedItemChanged += (o, e) =>
                {
                    toolSelectionPage.InputValid = toolList.SelectedToolItemType != null;

                    // If a new command is selected, rebuild the wizard pages
                    if (toolSelectionPage.InputValid && toolList.SelectedClass != null && !toolList.SelectedClass.Equals(Class))
                    {
                        // Remove pages collection changed handler from previously selected tool
                        ISupportsWizardConfiguration previousWizardConfigInterface = 
                            Class as ISupportsWizardConfiguration;
                        if (previousWizardConfigInterface != null)
                            previousWizardConfigInterface.Pages.CollectionChanged -= WizardInterfacePages_CollectionChanged;

                        Class = toolList.SelectedClass;
                        DisplayInfo = toolList.SelectedItemDisplayInfo;

                        // All but the page showing the available tools will be removed, so reset the number of
                        // pages added to the wizard by the command to one.
                        _locallyInsertedWizardPagesCount = 1;

                        List<WizardPage> configPages = createWizardPages();
                        int pagesToRemove = wizard.Pages.Count - 1;

                        // Add the new config pages to the wizard, then remove the old ones
                        foreach (WizardPage page in configPages)
                            wizard.Pages.Add(page);

                        for (int i = 0; i < pagesToRemove; i++)
                            wizard.Pages.RemoveAt(1);

                        updateConfigureButton(wizard);

                        // Initialize CurrentPage if command implements ISupportsWizardConfiguration
                        ISupportsWizardConfiguration wizardConfigInterface = Class as ISupportsWizardConfiguration;
                        if (wizardConfigInterface != null)
                        {
                            wizardConfigInterface.Pages.CollectionChanged += WizardInterfacePages_CollectionChanged;
                            wizardConfigInterface.CurrentPage = toolSelectionPage;
                        }
                    }
                };

                Size maxPageSize = getMaxPageSize(toolList.AvailableItems);
                if (maxPageSize.Height > 0 && maxPageSize.Width > double.MinValue)
                {
                    wizard.ContentHeight = maxPageSize.Height;
                    wizard.ContentWidth = maxPageSize.Width > 400 ? maxPageSize.Width : 400;
                }
            }
            else
            {
                // Get wizard config interface and use to initialize wizard size
                ISupportsWizardConfiguration wizardConfigInterface = Class as ISupportsWizardConfiguration;
                if (wizardConfigInterface != null)
                {
                    wizard.ContentHeight = wizardConfigInterface.DesiredSize.Height;
                    wizard.ContentWidth = wizardConfigInterface.DesiredSize.Width;
                    wizardConfigInterface.Pages.CollectionChanged += WizardInterfacePages_CollectionChanged;
                }
            }

            // initialize wizard pages
            List<WizardPage> pages = createWizardPages();
            foreach (WizardPage page in pages)
                wizard.Pages.Add(page);

            updateConfigureButton(wizard);

            // Wire events
            wizard.PageChanging += Wizard_PageChanging;
            wizard.PageChanged += Wizard_PageChanged;
            wizard.Cancelled += Wizard_Cancelled;
            wizard.Completed += Wizard_Completed;

            return wizard;
        }

        /// <summary>
        /// Creates the configuration UI to edit common tool proerties (e.g. label, description, icon, etc)
        /// for the current command
        /// </summary>
        /// <returns>The control containing the configuration UI</returns>
        private EditToolbarItemControl createToolPropertiesUI()
        {
            bool supportsConfiguration = Class as ISupportsConfiguration != null;
            bool supportsWizardConfiguration = Class as ISupportsWizardConfiguration != null;

            EditToolbarItemControl toolPropsUI = new EditToolbarItemControl()
            {
                Label = DisplayInfo != null ? DisplayInfo.Label : null,
                Description = DisplayInfo != null ? DisplayInfo.Description : null,
                IconUrl = DisplayInfo != null ? DisplayInfo.Icon : null,
                ToolType = Class != null ? Class.GetType() : null,
                ToolInstance = Class,
                SupportsConfiguration = supportsConfiguration && !supportsWizardConfiguration,
                OkCancelButtonVisibility = supportsWizardConfiguration || AllowToolSelection ? Visibility.Collapsed : Visibility.Visible,
                ToolbarSelectionVisibility = AllowContainerSelection ? Visibility.Visible : Visibility.Collapsed
            };

            return toolPropsUI;
        }

        /// <summary>
        /// Creates configuration wizard pages for the current command
        /// </summary>
        /// <returns>The list of wizard pages</returns>
        private List<WizardPage> createWizardPages()
        {
            List<WizardPage> pages = new List<WizardPage>();

            EditToolbarItemControl toolPropsUI = createToolPropertiesUI();
            toolPropsUI.Margin = new Thickness(5, 8, 5, 0);
            // Encapsulate properties dialog in a wizard page
            WizardPage toolPropsPage = new WizardPage()
            {
                Content = toolPropsUI,
                Heading = ESRI.ArcGIS.Mapping.Builder.Resources.Strings.SpecifyToolProperties,
                InputValid = toolPropsUI.InputValid
            };

            // Update wizard page validation state on tool properties validation state change
            toolPropsUI.ValidationStateChanged += (o, e) => { toolPropsPage.InputValid = toolPropsUI.InputValid; };

            // initialize wizard pages
            pages.Add(toolPropsPage);
            _locallyInsertedWizardPagesCount++;


            // Get pages from wizard config interface and add to collection
            ISupportsWizardConfiguration wizardConfig = Class as ISupportsWizardConfiguration;
            if (wizardConfig != null)
            {
                foreach (WizardPage page in wizardConfig.Pages)
                    pages.Add(page);
            }

            return pages;
        }

        /// <summary>
        /// Gets the maximum requested width and height from the wizard interfaces of the passed-in 
        /// toolbar items
        /// </summary>
        /// <param name="items">The items to traverse</param>
        /// <returns>A Size object with the maximum width and height</returns>
        private Size getMaxPageSize(List<AddToolbarItem> items)
        {
            Size maxSize = new Size(0, 0);

            // Walk all items and get maximum wizard page size
            ISupportsWizardConfiguration itemWizardConfig;
            foreach (AddToolbarItem item in items)
            {
                itemWizardConfig = Activator.CreateInstance(item.ToolbarItemType) as ISupportsWizardConfiguration;
                if (itemWizardConfig != null)
                {
                    maxSize.Height = itemWizardConfig.DesiredSize.Height > maxSize.Height ? 
                        itemWizardConfig.DesiredSize.Height : maxSize.Height;
                    maxSize.Width = itemWizardConfig.DesiredSize.Width > maxSize.Width ? 
                        itemWizardConfig.DesiredSize.Width : maxSize.Width;
                }
            }

            return maxSize;
        }

        /// <summary>
        /// Updates attached properties that determine whether to show the configure button
        /// and what its behavior is on click
        /// </summary>
        private void updateConfigureButton(Wizard wizard)
        {
            ISupportsConfiguration configurableCommand = Class as ISupportsConfiguration;
            if (configurableCommand != null && Class as ISupportsWizardConfiguration == null)
            {
                ControlExtensions.SetExtendedUIVisibility(wizard, Visibility.Visible);
                ControlExtensions.SetExtendedCommand(wizard,
                    new ParameterlessDelegateCommand(configurableCommand.Configure, delegate { return true; }));
            }
            else
            {
                ControlExtensions.SetExtendedUIVisibility(wizard, Visibility.Collapsed);
                ControlExtensions.SetExtendedCommand(wizard, null);
            }
        }

        #endregion
    }
}
