/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections.ObjectModel;
using System.Windows;

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Interface for add-ins that support wizard-style configuration
    /// </summary>
    public interface ISupportsWizardConfiguration : ISupportsConfiguration
    {
        /// <summary>
        /// Gets or sets the collection of configuration pages to show in the wizard
        /// </summary>
        ObservableCollection<WizardPage> Pages { get; set; }

        /// <summary>
        /// Gets or sets the page currently shown by the wizard
        /// </summary>
        WizardPage CurrentPage { get; set; }

        /// <summary>
        /// Gets or sets the ideal size for the container of wizard page content
        /// </summary>
        Size DesiredSize { get; set; }

        /// <summary>
        /// Called by the framework when the page is about to be changed
        /// </summary>
        /// <returns>A boolean indicating whether or not to cancel the page change operation</returns>
        bool PageChanging();

        /// <summary>
        /// Called by the framework when the configuration has been completed
        /// </summary>
        void OnCompleted();

        /// <summary>
        /// Called by the framework when the configuration has been cancelled
        /// </summary>
        void OnCancelled();
    }
}
