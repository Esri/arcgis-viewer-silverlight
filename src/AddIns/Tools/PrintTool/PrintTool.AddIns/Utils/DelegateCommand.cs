/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows.Input;

namespace PrintTool.AddIns
{
	internal class DelegateCommand : ICommand
	{
		private Predicate<object> _canExecute; private Action<object> _method;
		public event EventHandler CanExecuteChanged;
		public DelegateCommand(Action<object> method)
			: this(method, null)
		{
		}
		public DelegateCommand(Action<object> method, Predicate<object> canExecute)
		{
			_method = method; _canExecute = canExecute;
		}
		public bool CanExecute(object parameter)
		{
			if (_canExecute == null) { return true; } return _canExecute(parameter);
		}
		public void Execute(object parameter)
		{
            if (!CanExecute(parameter))
                throw new ArgumentException("Cannot execute command");
			_method.Invoke(parameter);
		}
		protected virtual void OnCanExecuteChanged(EventArgs e)
		{
			var canExecuteChanged = CanExecuteChanged; if (canExecuteChanged != null) canExecuteChanged(this, e);
		}
		public void RaiseCanExecuteChanged()
		{
			OnCanExecuteChanged(EventArgs.Empty);
		}
	}
}
