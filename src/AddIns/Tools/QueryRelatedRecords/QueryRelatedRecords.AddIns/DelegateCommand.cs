/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows.Input;

namespace QueryRelatedRecords
{
    /// <summary>
    /// Enables dynamic instantiation of a command given a method to call on Execute and (optionally) on CanExecute
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private Action<object> _execute; // Execute method
        private Predicate<object> _canExecute; // CanExecute method

        public event EventHandler CanExecuteChanged;

        #region Constructors

        public DelegateCommand(Action<object> method)
            : this(method, null)
        {
        }

        public DelegateCommand(Action<object> method, Predicate<object> canExecute)
        {
            _execute = method;
            _canExecute = canExecute;
        }

        #endregion

        /// <summary>
        /// Executes the command
        /// </summary>
        public void Execute(object parameter)
        {
            // Fire the command's execution delegate
            _execute.Invoke(parameter);
        }

        /// <summary>
        /// Checks whether the command is in an executable state
        /// </summary>
        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true; // Can always execute command that do not have a CanExecute delegate
            else
                return _canExecute(parameter); // Fire the CanExecute delegate
        }

        /// <summary>
        /// Invokes the command's <see cref="CanExecuteChanged"/> event
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged(EventArgs.Empty);
        }

        protected virtual void OnCanExecuteChanged(EventArgs e)
        {
            // Fire CanExecuteChanged
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, e);
        }
    }
}
