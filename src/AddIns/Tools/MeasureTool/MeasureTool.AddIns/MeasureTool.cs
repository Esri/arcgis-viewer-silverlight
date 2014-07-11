/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;
using System.Windows;

namespace MeasureTool.Addins
{
    /// <summary>
    /// Command to initiate measure tool functionality
    /// </summary>
    [Export(typeof(ICommand))]
    [LocalizedDisplayName("MeasureTitle")]
    [LocalizedDescription("MeasureToolDescription")]
    [LocalizedCategory("MeasureToolCategory")]
    [DefaultIcon("/MeasureTool.Addins;component/images/Measure.png")]
    public class MeasureToolCommand : ICommand
    {
        private MeasureView view;
        private static bool toolExecuting;  // tracks whether the tool is executing (i.e. dialog open).
                                            // static so that only one instance of the tool can be used
                                            // at any time in any application.  

        public MeasureToolCommand()
        {
            view = new MeasureView() { Margin = new Thickness(10) };
        }

        #region ICommand members

        /// <summary>
        /// Opens the measure dialog
        /// </summary>
        /// <param name="parameter">Not used</param>
        public void Execute(object parameter)
        {
            // Update executable state
            toolExecuting = true;
            RaiseCanExecuteChanged();

            // Create ViewModel and update the view's DataContext
            MeasureViewModel viewModel = 
                new MeasureViewModel(MapApplication.Current.Map, 
                    MapApplication.Current.Urls.GeometryServiceUrl);
            view.DataContext = viewModel;

            // Show the dialog
            MapApplication.Current.ShowWindow(Resources.Strings.MeasureTitle, view, false, null,
                (o, e) => 
                { 
                    // Cleanup viewModel
                    viewModel.Deactivate();

                    // Update executable state
                    toolExecuting = false;
                    RaiseCanExecuteChanged();
                });
        }

        /// <summary>
        /// Checks whether the measure tool can be used.
        /// </summary>
        /// <param name="parameter">Not used</param>
        public bool CanExecute(object parameter)
        {
            // Return true only if the tool is not already executing
            return !toolExecuting;
        }

        public event EventHandler CanExecuteChanged;
        #endregion

        private void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}
