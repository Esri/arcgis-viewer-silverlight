/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;

namespace PrintTool.AddIns
{
	/// <summary>
	/// Responsible for instantiating the View and ViewModel, calling Execute when the Tool on the toolbar
	/// is clicked, Saving and Loading the specified configuration, and creating the pages 
	/// for the Add Tool wizard.
	/// </summary>
	[Export(typeof(ICommand))]
	[LocalizedDisplayName("PrintToolTitle")]
	[LocalizedDescription("PrintToolDescription")]
	[LocalizedCategory("PrintToolCategory")]
	[DefaultIcon("/PrintTool.Addins;component/Images/Print32.png")]
	public class PrintTool : ICommand, ISupportsWizardConfiguration
	{
		/// <summary>
		/// This is the PrintToolConfigurationView, the UI that displays when configuring print settings.
		/// </summary>
		private PrintToolConfigurationView printToolConfigView;

		/// <summary>
		/// This is the PrintToolView, the UI that displays the configured print settings and allows
		/// the user to generate a printable output file.
		/// </summary>
		private PrintToolView printToolView;

		/// <summary>
		/// This is the PrintToolViewModel that holds the last saved configuration.
		/// </summary>
		private PrintToolViewModel savedConfiguration;
		
		private bool toolExecuting;
		/// <summary>
		/// Gets or sets a value indicating whether print tool is executing (i.e. dialog open).
		/// </summary>
		private bool ToolExecuting
		{
			get { return toolExecuting; }
			set
			{
				if (toolExecuting != value)
				{
					toolExecuting = value;
					RaiseCanExecuteChanged();
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PrintTool"/> class.
		/// </summary>
		public PrintTool()
		{
            printToolConfigView = new PrintToolConfigurationView() { Margin = new Thickness(10, 10, 10, 0) };
			printToolView = new PrintToolView() 
            { 
                Margin = new Thickness(15,15,15,10),
                Width = 300
            };
		}

		#region ICommand members
		/// <summary>
		/// Shows the PrintToolView when the icon on the toolbar is clicked.
		/// </summary>
		public void Execute(object parameter)
		{
			ToolExecuting = true;
			var toolViewModel = printToolView.DataContext as PrintToolViewModel;
			if (toolViewModel == null && savedConfiguration != null)
			{
				toolViewModel = new PrintToolViewModel(savedConfiguration);
				toolViewModel.Map = MapApplication.Current.Map;
				printToolView.DataContext = toolViewModel;
			}
			if (toolViewModel != null) toolViewModel.Activate();

			// If there is at least one visible setting, print tool view is shown. 
			// Otherwise, print command is executed without the intermediate dialog.
			if (toolViewModel.HasVisibleSetting)
			{
				MapApplication.Current.ShowWindow(Resources.Strings.PrintToolTitle, printToolView, false,
					null,
					(s, e) =>
					{
						ToolExecuting = false;
					},
					WindowType.Floating);
			}
			else
			{
				ToolExecuting = toolViewModel.IsPrinting;
				toolViewModel.PropertyChanged += (a, b) =>
					{
						if (b.PropertyName == "IsPrinting")
							ToolExecuting = toolViewModel.IsPrinting;
					};
				if (toolViewModel.Print.CanExecute(null))
					toolViewModel.Print.Execute(null);
			}
		}

		/// <summary>
		/// Determines whether the tool is available to execute.
		/// </summary>
		public bool CanExecute(object parameter)
		{
			return !ToolExecuting;
		}

		public event EventHandler CanExecuteChanged;

		private void RaiseCanExecuteChanged()
		{
			if (CanExecuteChanged != null)
				CanExecuteChanged(this, EventArgs.Empty);
		}
		#endregion

		#region ISupportsConfiguration members

		/// <summary>
		/// Fires when the page on the configuration Wizard changes.
		/// </summary>
		public void Configure()
		{
			//No need for logic on page change in this particular example.
		}

		/// <summary>
		/// Deserializes the saved print configuration settings.
		/// </summary>
		public void LoadConfiguration(string configData)
		{
            if (!string.IsNullOrEmpty(configData))
            {
				using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(configData)))
				{
					DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(PrintToolViewModel));
					memoryStream.Position = 0;
					savedConfiguration = (PrintToolViewModel)xmlSerializer.ReadObject(memoryStream);					
					memoryStream.Close();
				}
            }
		}

		/// <summary>
		/// Serializes the print configuration settings from the PrintToolConfigurationView.
		/// </summary>
		/// <returns></returns>
		public string SaveConfiguration()
		{
            string configData = null;
			if (savedConfiguration != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(PrintToolViewModel));
					serializer.WriteObject(memoryStream, savedConfiguration);
					memoryStream.Position = 0;
					using (StreamReader reader = new StreamReader(memoryStream, Encoding.UTF8))
					{
						configData = reader.ReadToEnd();
					}
				}
			}
            return configData;
		}

		#endregion

		#region ISupportsWizardConfiguration members

		private WizardPage currentPage;
		/// <summary>
		/// The page currently shown by the configuration wizard.
		/// </summary>
		public WizardPage CurrentPage
		{
			get { return currentPage; }
			set { currentPage = value; }
		}

		// Create a Size member variable for DesiredSize.  The Height and Width are set to the minimum values 
		// required to properly display the UI. Note that the configuration UI should also be flexible enough 
		// to fit any width or height beyond that specified by DesiredSize.
		private Size desiredSize = new Size() { Height = 430, Width = 380 };
		/// <summary>
		/// Tje desired size of the configuration dialog.
		/// </summary>
		public Size DesiredSize
		{
			get { return desiredSize; }
			set { desiredSize = value; }
		}

		/// <summary>
		/// Called by the framework when the page is about to be changed. 
		/// </summary>
		public bool PageChanging()
		{
			// Here you can check the validation state of the current page before the wizard goes to the next page
			// and cancel the page change if necessary (i.e. by returning false).
			return true;
		}

		/// <summary>
		/// Called when configuration wizard is canceled. 
		/// Reverts both PrintToolConfigurationView and PrintToolView to last saved configuration.
		/// </summary>
		public void OnCancelled()
		{
			var configViewModel = printToolConfigView.DataContext as PrintToolViewModel;
			if (configViewModel != null)
			{
				configViewModel.ApplyChanges(savedConfiguration ?? new PrintToolViewModel() { Map = MapApplication.Current.Map });
			}
		}

		/// <summary>
		/// Called when configuration wizard is completed.
		/// Updates last saved configuration.
		/// </summary>
		public void OnCompleted()
		{
			var configViewModel = printToolConfigView.DataContext as PrintToolViewModel;
			if (configViewModel != null)
			{
				if (savedConfiguration == null)
					savedConfiguration = new PrintToolViewModel(configViewModel);
				else
					savedConfiguration.ApplyChanges(configViewModel);

				// Tool view on builder must immediately reflect latest configuration
				// so changes are applied to this instance as well.
				var toolViewModel = printToolView.DataContext as PrintToolViewModel;
				if (toolViewModel != null)
					toolViewModel.ApplyChanges(savedConfiguration); 
			}
		}	
	
        private ObservableCollection<WizardPage> pages;
		/// <summary>
		/// The collection of configuration pages to show in the Wizard.
		/// </summary>
		public ObservableCollection<WizardPage> Pages
		{
			get
			{
				// The host application will retrieve the add-ins configuration UI from the Pages property and 
				// display it as it sees fit. It is the add-in's responsibility to return the collection of 
				// configuration pages to show via this property.  In this case, the add-in will nstantiate the 
				// Pages collection and add a single WizardPage to it that hosts the bookmarks configuration view 

				// Initialize configuration pages if not yet initialized
				if (pages == null)
				{ 
					// Create a new WizardPage; set the configView as the Content and set the Heading. 
					WizardPage page = new WizardPage();
					page.Content = printToolConfigView;
					page.Heading = Resources.Strings.PrintConfigTitle;

					// Create a new WizardPage collection and add the new page.
					pages = new ObservableCollection<WizardPage>();
					pages.Add(page);

					// Sets the DataContext of the PrintToolConfigurationView 
					// with savedConfiguration if already configured.
					PrintToolViewModel viewModel = null;
					if (savedConfiguration != null)
						viewModel = new PrintToolViewModel(savedConfiguration);
					else
					{
						viewModel = new PrintToolViewModel()
						{
							Map = MapApplication.Current.Map
						};
					}
					printToolConfigView.DataContext = viewModel;

					// Determine whether the page input is valid.  In this case, the input is valid if there 
					// is the service information is retrieved successfully from the print service. 
					// If InputValid is false, users will be prevented from completing configuraiton.
					page.InputValid = viewModel.IsValid;
					viewModel.PropertyChanged += (a, b) =>
					{
						if (b.PropertyName == "IsValid")
							page.InputValid = viewModel.IsValid;
					};
				}
				return pages;
			}
			set
			{
				//Simply set the pages member variable if the passed-in value does not match.
				if (pages != value)
					pages = value;
			}
		}

		#endregion
	}
}
