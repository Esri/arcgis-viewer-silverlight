/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Xml.Serialization;

namespace PrintTool.AddIns
{
	/// <summary>
	/// Choice list for print layouts when ArcGIS 10.1 Server printing is not used.
	/// </summary>
	[XmlRoot("PrintLayouts")]
	public class PrintLayouts : Collection<PrintLayout>,  INotifyPropertyChanged
	{
		#region Properties

		private PrintLayout selectedLayout;
		/// <summary>
		/// Gets or sets the selected print layout.
		/// </summary>
		public PrintLayout SelectedLayout
		{
			get { return selectedLayout; }
			set
			{
				if (selectedLayout != value)
				{
					selectedLayout = value;
					OnPropertyChanged("SelectedLayout");
					MoveNextChanged();
					MovePreviousChanged();
				}
			}
		}

		private bool isApplied;
		/// <summary>
		/// Gets or sets a value indicating whether selected template has been applied.
		/// </summary>
		public bool IsApplied
		{
			get { return isApplied; }
			set
			{
				if (isApplied != value)
				{
					isApplied = value;
					OnPropertyChanged("IsApplied");
				}
			}
		}

		private bool isConfig;
		/// <summary>
		/// Gets or sets a value indicating whether model has been activated from builder (as configuration).
		/// </summary>
		public bool IsConfig
		{
			get { return isConfig; }
			set
			{
				if (isConfig != value)
				{
					isConfig = value;
					OnPropertyChanged("IsConfig");
				}
			}
		}

		#region Commands

		private DelegateCommand applyCommand;
		/// <summary>
		/// Command used to apply current layout template.
		/// </summary>
		public ICommand Apply
		{
			get
			{
				if (applyCommand == null)
				{
					applyCommand = new DelegateCommand(OnApply, CanApply);
				}
				return applyCommand;
			}
		}

		private bool CanApply(object commandParameter)
		{
			return SelectedLayout != null;
		}

		/// <summary>
		/// Applies current layout template.
		/// </summary>
		private void OnApply(object commandParameter)
		{
			if (!CanApply(commandParameter)) return;
			IsApplied = true;
		}

		private DelegateCommand moveNextCommand;
		/// <summary>
		/// Command used to load next layout template.
		/// </summary>
		public ICommand MoveNext
		{
			get
			{
				if (moveNextCommand == null)
				{
					moveNextCommand = new DelegateCommand(OnMoveNext, CanMoveNext);
				}
				return moveNextCommand;
			}
		}

		private void MoveNextChanged()
		{
			if (moveNextCommand != null)
			{
				moveNextCommand.RaiseCanExecuteChanged();
			}
		}

		private bool CanMoveNext(object commandParameter)
		{
			return (Count > 0 && SelectedLayout != null && IndexOf(SelectedLayout) < Count - 1);
		}

		/// <summary>
		/// Gets the next layout template.
		/// </summary>
		private void OnMoveNext(object commandParameter)
		{
			if (!CanMoveNext(commandParameter)) return;
			var index = IndexOf(SelectedLayout) + 1;
			if (index >= 0 && index <= Count - 1)
				SelectedLayout = Items[index];
		}

		private DelegateCommand movePreviousCommand;
		/// <summary>
		/// Command used to load previous layout template.
		/// </summary>
		public ICommand MovePrevious
		{
			get
			{
				if (movePreviousCommand == null)
				{
					movePreviousCommand = new DelegateCommand(OnMovePrevious, CanMovePrevious);
				}
				return movePreviousCommand;
			}
		}

		private void MovePreviousChanged()
		{
			if (movePreviousCommand != null)
			{
				movePreviousCommand.RaiseCanExecuteChanged();
			}
		}

		private bool CanMovePrevious(object commandParameter)
		{
			return (Count > 0 && SelectedLayout != null && IndexOf(SelectedLayout) > 0);
		}

		/// <summary>
		/// Gets the previous layout template.
		/// </summary>
		private void OnMovePrevious(object commandParameter)
		{
			if (!CanMovePrevious(commandParameter)) return;
			var index = IndexOf(SelectedLayout) - 1;
			if (index >= 0 && index <= Count -1)
				SelectedLayout = Items[index];

		}

		#endregion

		#endregion

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string property)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}

		#endregion

		#region Overrides Collection

		/// <summary>
		/// Selects the first inserted print layout and updates commands.
		/// </summary>
		protected override void InsertItem(int index, PrintLayout item)
		{
			base.InsertItem(index, item);
			if (index == 0) SelectedLayout = item;
			MoveNextChanged();
			MovePreviousChanged();
		}

		#endregion 
	}
}
