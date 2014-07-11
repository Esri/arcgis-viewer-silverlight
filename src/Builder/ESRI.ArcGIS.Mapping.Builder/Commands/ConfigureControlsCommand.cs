/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Builder.Controls;
using ESRI.ArcGIS.Mapping.Controls;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class ConfigureControlsCommand : CommandBase
    {
        ConfigureControlsControl configureControlsControl;

        public override void Execute(object parameter)
        {
            // Create UI control
            configureControlsControl = new ConfigureControlsControl();

            // Create view model that interacts with UI and associate it as the data context so all elements
            // of the UI can use XAML binding to utilize the view model.
            ConfigureControlsViewModel vm = new ConfigureControlsViewModel();

            IApplicationAdmin appAdmin = MapApplication.Current as IApplicationAdmin;
            if (appAdmin != null && appAdmin.ConfigurableControls != null)
            {
            // Process each element in the configurable controls collection, looking for those that support configuration
                foreach (FrameworkElement elem in appAdmin.ConfigurableControls)
            {
                if (string.IsNullOrWhiteSpace(elem.Name))
                    continue;

                ISupportsConfiguration supportsConfig = elem as ISupportsConfiguration;
                if (supportsConfig != null)
                {
                    string displayName = ElementExtensions.GetDisplayName(elem);
                    ConfigureControlDataItem ccdi = new ConfigureControlDataItem()
                    {
                        Label = displayName ?? elem.Name.InsertSpaces(),
                        Element = elem
                    };

                    // Add this element to the list within the view model
                    vm.ConfigurableItems.Add(ccdi);
                    }
                }
            }

            // Associate close command with method that will hide the UI
            vm.Closed += new System.EventHandler(vm_Closed);

            // Assign view model as data context for UI
            configureControlsControl.DataContext = vm;

            // Display UI
            BuilderApplication.Instance.ShowWindow(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ConfigureControls,
                configureControlsControl, true);
        }

        void vm_Closed(object sender, System.EventArgs e)
        {
            BuilderApplication.Instance.HideWindow(configureControlsControl);
            configureControlsControl = null;
        }
    }

    /// <summary>
    /// Command to configure a control when the Configure button is clicked on an element in the list. Each element is
    /// typically displayed in a ListBox, each item having its own configure button associated with this command. The
    /// parameter to this function is the associated data item for the selected UI item and is used to obtain the
    /// framework element/control in question and its corresponding configuration interface to launch the configuration
    /// user interface.
    /// </summary>
    public class ConfigureControlCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            ConfigureControlDataItem ccdi = parameter as ConfigureControlDataItem;
            if (ccdi != null)
            {
                ISupportsConfiguration supportsConfig = ccdi.Element as ISupportsConfiguration;
                if (supportsConfig != null)
                {
                    supportsConfig.Configure();
                }
            }
        }
    }
}
