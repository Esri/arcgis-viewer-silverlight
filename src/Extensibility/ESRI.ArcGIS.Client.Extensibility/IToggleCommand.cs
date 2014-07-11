/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Enables implementation of a command that exposes toggling functionality.  In Esri products, raise the
    /// command's <see cref="ICommand.CanExecuteChanged"/> event to update the add-in's toggle state.
    /// </summary>
    public interface IToggleCommand : ICommand
    {
        /// <summary>
        /// Determines whether the add-in is toggled on or off.  Invoked by the application to determine the toggle
        /// state.
        /// </summary>
        /// <returns>The add-in's toggle state</returns>
        bool IsChecked();
    }
}
