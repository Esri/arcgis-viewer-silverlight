/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Geometry;
using QueryTool.Resources;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Client;

namespace QueryTool.AddIns
{
	[Export(typeof(ICommand))]
	[LocalizedDisplayName("QueryToolName")]
	[DefaultIcon("/QueryTool.AddIns;component/Images/Query16.png")]
	[LocalizedDescription("QueryDescription")]
	[LocalizedCategory("QueryCategory")]
	public class QueryTool : IToggleCommand, ISupportsWizardConfiguration
	{
		// Provides access to the Attribute table in the layout.
		private const string CONTAINER_NAME = "FeatureDataGridContainer";
		private const double EXPAND_EXTENT_RATIO = .05;

		QueryToolView toolView;
		QueryViewModel savedConfiguration;		

		public QueryTool()
		{
			toolView = new QueryToolView() 
			{ 
				Margin = new Thickness(10),
				MinWidth = 300,
                HorizontalAlignment = HorizontalAlignment.Stretch
			};
		}

		#region ICommand members
		private bool toolExecuting;
		/// <summary>
		/// Gets or sets a value indicating whether query tool is executing (i.e. dialog open).
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

		private void RaiseCanExecuteChanged()
		{
			if (CanExecuteChanged != null)
				CanExecuteChanged(this, EventArgs.Empty);
		}

		/// <summary>
		/// Toggles the query UI on or off
		/// </summary>
		public void Execute(object parameter)
		{
			ToolExecuting = true;

			// Updates tool DataContext with savedConfiguration.			
			var toolViewModel = toolView.DataContext as QueryViewModel;			
			if (toolViewModel == null)
			{
				toolViewModel = savedConfiguration != null ? new QueryViewModel(savedConfiguration) : new QueryViewModel();
				toolView.DataContext = toolViewModel;
			}
			else if (savedConfiguration != null)
				toolViewModel.ApplyChanges(savedConfiguration);

			// Sets map and proxy url based on application settings.
			if (MapApplication.Current != null)
			{
				toolViewModel.Map = MapApplication.Current.Map;
				if (MapApplication.Current.Urls != null)
					toolViewModel.ProxyUrl = MapApplication.Current.Urls.ProxyUrl;
			}
			
			// Updates default/selection on each query expression.
			toolViewModel.ResetExpressions();

			// Sets the result layer name.
			toolViewModel.SetLayerNameAction = (layer, title) =>
			{
				if (layer != null && !string.IsNullOrEmpty(title))
				{
					var index = 1;
                    string layerName = title;
					if (MapApplication.Current != null && MapApplication.Current.Map != null && MapApplication.Current.Map.Layers != null)
					{
                        LayerCollection layers = MapApplication.Current.Map.Layers;
                        while (layers.Any(l => MapApplication.GetLayerName(l) == layerName))
                        {
                            index++;
                            layerName = string.Format("{0} ({1})", title, index);
                        }
					}

					MapApplication.SetLayerName(layer, layerName);
				}
			};

			// Adds result layer to map.
			toolViewModel.AddLayerAction = (layer) =>
			{
				if (layer != null)
				{
					if (MapApplication.Current.Map != null && MapApplication.Current.Map.Layers != null)
					{
						if (!MapApplication.Current.Map.Layers.Contains(layer))
							MapApplication.Current.Map.Layers.Add(layer);
					}
				}
			};

			// Removes result layer from map.
			toolViewModel.RemoveLayerAction = (layer) =>
			{
				if (layer != null)
				{
					if (MapApplication.Current.Map != null && MapApplication.Current.Map.Layers != null)
					{
						if (MapApplication.Current.Map.Layers.Contains(layer))
							MapApplication.Current.Map.Layers.Remove(layer);
					}
				}
			};	

			// Updates layer selection on map after query is executed.
			toolViewModel.SelectLayerAction = (layer) =>
			{
				if (layer != null)
					MapApplication.Current.SelectedLayer = layer;
			};

			// Zooms to result layer.
			toolViewModel.ZoomToExtentAction = (geometry) =>
			{
				if (geometry != null)
				{
					var env = geometry.Extent;
					if (env.Width > 0 || env.Height > 0)
					{
						env = new Envelope(env.XMin - env.Width * EXPAND_EXTENT_RATIO, env.YMin - env.Height * EXPAND_EXTENT_RATIO,
							env.XMax + env.Width * EXPAND_EXTENT_RATIO, env.YMax + env.Height * EXPAND_EXTENT_RATIO);
						if (MapApplication.Current.Map != null)
							MapApplication.Current.Map.ZoomTo(env);
					}
					else
					{
						if (MapApplication.Current.Map != null)
							MapApplication.Current.Map.PanTo(env);
					}
				}
			};

			// Updates visibility of data grid after query is expecuted.
			toolViewModel.UpdateDataGridVisibility = (visibility) =>
			{
				UpdateFeatureDataGridVisibility(visibility);
			};

			// Displays QueryToolView.
			MapApplication.Current.ShowWindow(toolViewModel.QueryTitle, toolView, false,
					null,
					(s, e) =>
					{
						ToolExecuting = false;
						// Clears map of query results when done.
						toolViewModel.ClearResults();

                        foreach (ExpressionViewModel exp in toolViewModel.QueryExpressions)
                            exp.ClearValidationExceptions();
					},
					WindowType.Floating);

			// Executes query on click when there are no visible expression.
			if (!toolViewModel.HasVisibleExpression)
			{
				if (toolViewModel.Execute.CanExecute(null))
					toolViewModel.Execute.Execute(null);
			}
		}

		/// <summary>
		/// Checks whether the command is in an executable state
		/// </summary>
		public bool CanExecute(object parameter)
		{
			return !ToolExecuting;
		}

		/// <summary>
		/// Raised when the command's toggle or execution state changes
		/// </summary>
		public event EventHandler CanExecuteChanged;

		// Raises teh CanExecuteChanged event
		private void OnCanExecuteChanged()
		{
			if (CanExecuteChanged != null)
				CanExecuteChanged(this, EventArgs.Empty);
		}

		#endregion

		#region IToggleCommand Members

		private bool _isChecked = false;
		/// <summary>
		/// Gets whether the command is toggled on or off
		/// </summary>
		public bool IsChecked()
		{
			return _isChecked;
		}

		#endregion

		#region ISupportsConfiguration Members

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Configure() 
		{
			// No logic needed in this case
		}
		/// <summary>
		/// Initializes the query tool based on a configuration string
		/// </summary>
		/// <param name="configData"></param>
		public void LoadConfiguration(string configData)
		{
			if (!string.IsNullOrEmpty(configData))
			{
				using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(configData)))
				{
					DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(QueryViewModel));
					memoryStream.Position = 0;
					savedConfiguration = (QueryViewModel)xmlSerializer.ReadObject(memoryStream);
					memoryStream.Close();
				}
			}
		}

		/// <summary>
		/// Serializes the query tool's configuration to a string
		/// </summary>
		public string SaveConfiguration()
		{
			string configData = null;
			if (savedConfiguration != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(QueryViewModel));
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

		#region ISupportsWizardConfiguration Members

		private WizardPage currentPage;
		/// <summary>
		/// Gets or sets the current page in the tool's configuration wizard
		/// </summary>
		public WizardPage CurrentPage
		{
			get { return currentPage; }
			set
			{
				if (currentPage != value)
				{
                    // Check whether the page has been moved forward or back from the query config page
                    // and take action accordingly.  Need to check this here instead of PageChanging because
                    // PageChanging does not provide any info on what the next page will be.
                    ICommand expressionCommand = null;
                    if (currentPage == queryPage 
                        && value != null 
                        && configViewModel.IsEditingExpression
                        && configViewModel.CurrentExpression != null
                        && pages != null)
                    {
                        int queryPageIndex = pages.IndexOf(queryPage);
                        int newPageIndex = pages.IndexOf(value);
                        if (newPageIndex < queryPageIndex) // Back button was clicked
                            expressionCommand = configViewModel.CurrentExpression.Cancel;
                        else if (newPageIndex > queryPageIndex) // Next button was clicked
                            expressionCommand = configViewModel.CurrentExpression.Save;
                    }

                    Wizard configWizard = null;
                    // Try to get configuration wizard before updating current page
                    if (currentPage != null && currentPage.Content is DependencyObject)
                        configWizard = ((DependencyObject)currentPage.Content).FindAncestorOfType<Wizard>();

					currentPage = value;
					if (currentPage != null)
					{
                        // Try to get configuration wizard after updating current page
                        if (configWizard == null && currentPage.Content is DependencyObject)
                            configWizard = ((DependencyObject)currentPage.Content).FindAncestorOfType<Wizard>();

						// Resets any status or error message on load.
						if (currentPage.Content is ConnectionConfigView)
						{
							var viewModel = (currentPage.Content as FrameworkElement).DataContext as QueryViewModel;
							if (viewModel != null)
								viewModel.ResetStatus();
						}
					}

                    if (expressionCommand != null && configWizard != null)
                    {
                        expressionCommand.Execute(null);

                        if (configViewModel.QueryExpressions.Count > 0)
                        {
                            configWizard.CurrentPage = queryPage;
                            queryPage.InputValid = configViewModel.IsExpressionValid;
                        }
                    }
				}
			}
		}

		private Size desiredSize = new Size(400, 410);
		/// <summary>
		/// Gets or sets the optimal size for display of the configuration pages
		/// </summary>
		public Size DesiredSize
		{
			get { return desiredSize; }
			set
			{
				if (desiredSize != value)
					desiredSize = value;
			}
		}

		/// <summary>
		/// Cancels the current configuration, reverting to the last saved state
		/// </summary>
		public void OnCancelled()
		{
            if (configViewModel != null)
                configViewModel.RevertState();

            // Updates tool DataContext with savedConfiguration.
            if (toolView != null)
            {
                var toolViewModel = toolView.DataContext as QueryViewModel;
                if (toolViewModel == null)
                {
                    toolViewModel = savedConfiguration != null ? new QueryViewModel(savedConfiguration) : new QueryViewModel();
                    toolView.DataContext = toolViewModel;
                }
                else if (savedConfiguration != null)
                    toolViewModel.ApplyChanges(savedConfiguration);
                // Updates default/selection on each query expression.
                toolViewModel.ResetExpressions();
            }
		}

		/// <summary>
		/// Notifies the tool that configuration has completed
		/// </summary>
		public void OnCompleted()
		{
            //var configPage = Pages != null && Pages.Count > 0 ? Pages[0] : null;			
            //if (configPage != null && configPage.Content is FrameworkElement)
            //{
            //    var configViewModel = (configPage.Content as FrameworkElement).DataContext as QueryViewModel;
				if (configViewModel != null)
				{
					// Updates saved configuration.
					if (savedConfiguration == null)
						savedConfiguration = new QueryViewModel(configViewModel);
					else
						savedConfiguration.ApplyChanges(configViewModel);

					// Updates tool DataContext with savedConfiguration.
					if (toolView != null)
					{
						var toolViewModel = toolView.DataContext as QueryViewModel;
						if (toolViewModel == null)
						{
							toolViewModel = savedConfiguration != null ? new QueryViewModel(savedConfiguration) : new QueryViewModel();
							toolView.DataContext = toolViewModel;
						}
						else if (savedConfiguration != null)
							toolViewModel.ApplyChanges(savedConfiguration);
						// Updates default/selection on each query expression.
						toolViewModel.ResetExpressions();
					}
                }
            //}
		}

		/// <summary>
		/// Notifies the tool that the page is changing
		/// </summary>
		/// <returns>A boolean indicating whether the page change should be allowed</returns>
		public bool PageChanging()
		{
            //if (CurrentPage == queryPage && configViewModel.IsEditingExpression)
            //{
            //    configViewModel.CurrentExpression.Save.Execute(null);
            //    queryPage.InputValid = configViewModel.IsExpressionValid;
            //    return false;
            //}
            //else
            //{
                return true;
            //}
		}

        //WizardPage expressionEditPage;
        //ExpressionViewModel currentExpression;
        WizardPage queryPage;
        QueryViewModel configViewModel;
		ObservableCollection<WizardPage> pages;
		/// <summary>
		/// Gets the set of pages for configuring the query tool
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

					// Sets the DataContext of the QueryConfigurationView 
					// with savedConfiguration if already configured.
					configViewModel = savedConfiguration != null ? 
						new QueryViewModel(savedConfiguration) : new QueryViewModel();
					
					// Sets map and proxy url based on application settings.
					if (MapApplication.Current != null)
					{
						configViewModel.Map = MapApplication.Current.Map;
						if (MapApplication.Current.Urls != null)
							configViewModel.ProxyUrl = MapApplication.Current.Urls.ProxyUrl;
					}
					
					// Configuration Page 1: Connection Configuration.
					var connectionConfigView = new ConnectionConfigView()
					{
						DataContext = configViewModel,
						Margin = new Thickness(10d)
					};

					// Create a new WizardPage collection and adds the configuration pages.
					pages = new ObservableCollection<WizardPage>();

					var connectionPage = new WizardPage()
					{
						Content = connectionConfigView,
						Heading = Strings.ConfigureQueryLayer,
						InputValid = configViewModel.IsConnectionValid,
					};
					pages.Add(connectionPage);

					// Configuration Page 2: Query Configuration
					var queryConfigView = new QueryConfigView()
					{
						DataContext = configViewModel,
						Margin = new Thickness(10d, 10d, 10d, 0d)
					};

					queryPage = new WizardPage()
					{
						Content = queryConfigView,
						Heading = Strings.BuildQueryExpressionsLabel,
						InputValid = configViewModel.IsExpressionValid,
					};				
					pages.Add(queryPage);

					// Configuration Page 3: Result Configuration.
					var resultConfigView = new ResultConfigView()
					{
						DataContext = configViewModel,
						Margin = new Thickness(10d)
					};

					var resultPage = new WizardPage()
					{
						Content = resultConfigView,
						Heading = Strings.ConfigureResultLabel,
						InputValid = configViewModel.IsResultValid,
					};
					pages.Add(resultPage);

                    // Configuration Page 4: dialog configuration
                    var dialogConfigView = new DialogConfigView()
                    {
                        DataContext = configViewModel,
                        Margin = new Thickness(10d)
                    };

                    Func<bool> dialogConfigValid = () => !string.IsNullOrEmpty(configViewModel.QueryTitle) &&
                        configViewModel.QueryExpressions.All(e => !string.IsNullOrEmpty(e.ExpressionLabel) || !e.IsVisible);
                    var dialogConfigPage = new WizardPage()
                    {
                        Content = dialogConfigView,
                        Heading = Strings.ConfigureDialogHeading,
                        InputValid = configViewModel.QueryExpressions != null && dialogConfigValid()
                    };
                    pages.Add(dialogConfigPage);

                    PropertyChangedEventHandler expressionPropChanged = (o, e) =>
                    {
                        if (CurrentPage == queryPage && configViewModel.IsEditingExpression)
                        {
                            queryPage.InputValid = configViewModel.CurrentExpression != null
                                && configViewModel.CurrentExpression.Save.CanExecute(null);
                        }
                    };

                    // Determines whether a configuration page has valid input.
                    PropertyChangedEventHandler viewModelPropChanged = (a, b) =>
					{
                        if (b.PropertyName == "IsConnectionValid")
                            connectionPage.InputValid = configViewModel.IsConnectionValid;
                        else if (b.PropertyName == "IsExpressionValid")
                            queryPage.InputValid = configViewModel.IsExpressionValid;
                        else if (b.PropertyName == "IsResultValid")
                            resultPage.InputValid = configViewModel.IsResultValid;
                        else if (b.PropertyName == "IsEditingExpression" && configViewModel.IsEditingExpression)
                        {
                            configViewModel.CurrentExpression.PropertyChanged -= expressionPropChanged;
                            configViewModel.CurrentExpression.PropertyChanged += expressionPropChanged;
                        }
                        else
                            dialogConfigPage.InputValid = dialogConfigValid();
					};
                    configViewModel.PropertyChanged -= viewModelPropChanged;
                    configViewModel.PropertyChanged += viewModelPropChanged;

                    if (savedConfiguration != null)
                        configViewModel.Initialize();
				}

                if (configViewModel != null)
                    configViewModel.SaveState();

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

		#region Utility Methods

		/// <summary>
		/// Updates visibility of attribute table.
		/// </summary>
		private void UpdateFeatureDataGridVisibility(Visibility visibility)
		{
			// Get the attribute table container
			FrameworkElement container = MapApplication.Current.FindObjectInLayout(CONTAINER_NAME) as FrameworkElement;
			if (container != null)
			{
				var cmd = visibility == Visibility.Visible ? "_Show" : "_Hide";
				// try to get storyboard (animation) for showing attribute table
				Storyboard showStoryboard = container.FindStoryboard(CONTAINER_NAME + cmd);
				if (showStoryboard != null)
					showStoryboard.Begin(); // use storyboard if available
				else
					container.Visibility = visibility; // no storyboard, so set visibility directly
			}
		}

		#endregion
	}
}
