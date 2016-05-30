/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows.Input;

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Command that allows for specifying delegates for the <see cref="Execute"/> and <see cref="CanExecute"/>
    /// methods
    /// </summary>
    internal class DelegateCommand : ICommand
    {
        private Predicate<object> _canExecute; private Action<object> _method;

        /// <summary>
        /// Occurs when the execution state of the command has changed, as indicated by the 
        /// <see cref="CanExecute"/> method
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class
        /// </summary>
        /// <param name="method">The function to call when the command is executed</param>
        public DelegateCommand(Action<object> method)
            : this(method, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class
        /// </summary>
        /// <param name="method">The function to call when the command is executed</param>
        /// <param name="canExecute">The function to call to determine the command's execution state</param>
        public DelegateCommand(Action<object> method, Predicate<object> canExecute)
        {
            _method = method; _canExecute = canExecute;
        }

        /// <summary>
        /// Determines the execution state of the command
        /// </summary>
        /// <param name="parameter">The object to be passed to the command when it is executed</param>
        /// <returns>The execution state</returns>
        public bool CanExecute(object parameter)
        {
            if (_canExecute == null) { return true; } return _canExecute(parameter);
        }

        /// <summary>
        /// Invokes the command
        /// </summary>
        /// <param name="parameter">The object to use during invocation</param>
        public void Execute(object parameter)
        {
            _method.Invoke(parameter);
        }
        protected virtual void OnCanExecuteChanged(EventArgs e)
        {
            var canExecuteChanged = CanExecuteChanged; if (canExecuteChanged != null) canExecuteChanged(this, e);
        }

        /// <summary>
        /// Raises the command's <see cref="CanExecuteChanged"/> event
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged(EventArgs.Empty);
        }
    }
}
