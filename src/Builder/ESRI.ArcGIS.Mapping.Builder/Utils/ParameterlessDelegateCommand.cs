/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows.Input;

namespace ESRI.ArcGIS.Mapping.Builder
{
	internal class ParameterlessDelegateCommand : ICommand
	{
		private Predicate<object> _canExecute; private Action _method;
		public event EventHandler CanExecuteChanged;
		public ParameterlessDelegateCommand(Action method)
			: this(method, null)
		{
		}
        public ParameterlessDelegateCommand(Action method, Predicate<object> canExecute)
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
                throw new ArgumentException("Command is not in an executable state");
			_method.Invoke();
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
