/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel;

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Interface implemented by Esri products and accessed indirectly through MapApplication class to provide
    /// administrative services.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IApplicationAdmin
    {
        /// <summary>
        /// Provides access to the configurable controls in the application
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        ObservableCollection<FrameworkElement> ConfigurableControls { get; set; }
    }
}
