/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Extensibility;

namespace QueryTool.AddIns
{
	/// <summary>
	/// This class represents the settings used to configure the query.
	/// </summary>
	[DataContract]
	public class QueryViewModel : INotifyPropertyChanged
	{
		
		/// <summary>
		/// Constructor for the QueryViewModel object.
		/// </summary>
		public QueryViewModel()
		{
			QueryExpressions = new ObservableCollection<ExpressionViewModel>();
			OutFields = new ObservableCollection<OutField>();
			AutoEnableDataGrid = true;
			AutoPinResults = false;
			AutoZoomToResults = true;
			UseServiceRenderer = false;
		}

		/// <summary>
		/// Copy constructor for the QueryViewModel object.
		/// </summary>
		/// <param name="source"></param>
		public QueryViewModel(QueryViewModel source)
		{			
			CopyAllSettings(source);
		}

		private bool enableReset = true;

		/// <summary>
		/// Updates properties of current object with properties of source object.
		/// </summary>
		/// <param name="source"></param>
		public void ApplyChanges(QueryViewModel source)
		{
			CopyAllSettings(source);
		}

		/// <summary>
		/// Resets query expressions to their default value and/or selection.
		/// </summary>
		public void ResetExpressions()
		{
			if (QueryExpressions != null)
			{
				foreach (var q in QueryExpressions)
				{
					q.Reset();
				}
			}
		}

        public void Initialize()
        {
            if (Connect.CanExecute(Constants.SAVED_CONFIGURATION))
                Connect.Execute(Constants.SAVED_CONFIGURATION);
        }

        QueryViewModel savedState;
        public void SaveState()
        {
            savedState = new QueryViewModel();
            savedState.ApplyChanges(this);
        }

        public void RevertState()
        {
            if (savedState != null)
            {
                if (CurrentExpression != null && CurrentExpression.Cancel.CanExecute(null))
                    CurrentExpression.Cancel.Execute(null);
                ApplyChanges(savedState);
                Initialize();
                initializeExpressions();
            }
        }

		/// <summary>
		/// Removes the result layer from map when not pinned.
		/// </summary>
		public void ClearResults()
		{
			Status = null;
			Error = null;
			if(!IsPinned || IsTable) // Tables should always be removed
			{
				if (RemoveLayerAction != null)
					RemoveLayerAction(resultLayer);
                if (UpdateDataGridVisibility != null)
                    UpdateDataGridVisibility(Visibility.Collapsed);
            }
		}
		
		/// <summary>
		/// Resets status/error messages.
		/// </summary>
		public void ResetStatus()
		{
			Status = null;
			Error = null;
		}
		
		/// <summary>
		/// Gets or sets the map control where query results will be associated rendered.
		/// </summary>
		public Map Map { get; set; }
				
		/// <summary>
		/// Gets or sets the URL to proxy request through.
		/// </summary>
		public string ProxyUrl { get; set; }

		/// <summary>
		/// Gets or sets the action for adding the layer to map.
		/// </summary>
		public Action<Layer> AddLayerAction { get; set; }

		/// <summary>
		/// Gets or sets the action for removing a layer from map.
		/// </summary>
		public Action<Layer> RemoveLayerAction { get; set; }

		/// <summary>
		/// Gets or sets the action for selecting a layer from map.
		/// </summary>
		public Action<Layer> SelectLayerAction { get; set; }

		/// <summary>
		/// Gets or sets the action for setting the layer name.
		/// </summary>
		public Action<Layer, string> SetLayerNameAction { get; set; }

		/// <summary>
		/// Gets or sets the action for zooming to geometry.
		/// </summary>
		public Action<ESRI.ArcGIS.Client.Geometry.Geometry> ZoomToExtentAction { get; set; }
		
		// Holds the action to be performed when layer has been initialized
		// This value different at the time of configuration and execution of query tool.
		private Action<FeatureLayer> initializedAction;

		private FeatureLayer _resultLayer;
		// Gets the layer where query is performed against.
		private FeatureLayer resultLayer
		{
			get{return _resultLayer;}
			set
			{
				if (_resultLayer != value)
				{
					Action unsubscribe = () =>
					{
						// Unhook from possible event handling.
						if (_resultLayer != null)
						{
							_resultLayer.InitializationFailed -= ResultLayer_InitializationFailed;
							_resultLayer.Initialized -= ResultLayer_Initialized;
							_resultLayer.UpdateFailed -= ResultLayer_UpdateFailed;
							_resultLayer.UpdateCompleted -= ResultLayer_UpdateCompleted;
						}
					};
					Action notify = () =>
					{
						// Raise property change for read-only properties that are dependent on the layer.
						OnPropertyChanged("Fields");
						OnPropertyChanged("HasResults");
						OnPropertyChanged("IsConnectionValid");
						OnPropertyChanged("IsExpressionValid");
						OnPropertyChanged("IsResultValid");
						PinChanged();
					};
					unsubscribe();
					_resultLayer = value;
					notify();
				}
			}
		}
		
		private Dictionary<int, List<int>> groupLookup;
		/// <summary>
		/// Gets the dictionary that stores grouping information of expressions.
		/// </summary>
		private Dictionary<int, List<int>> GroupLookup
		{
			get
			{
				if (groupLookup == null)
					groupLookup = new Dictionary<int, List<int>>();
				return groupLookup;
			}
		}
		
		/// <summary>
		/// Gets the Field from FeatureLayerInfo.
		/// </summary>
		public IEnumerable<Field> Fields
		{
			get
			{
				if (resultLayer != null && resultLayer.LayerInfo != null)
					return resultLayer.LayerInfo.Fields;
				return null;
			}
		}
		
		/// <summary>
		/// Gets a value indicating whether connection was successful.
		/// </summary>
		public bool IsConnectionValid
		{
			get
			{
				return IsConnected && Fields != null;
			}
		}

		/// <summary>
		/// Gets a value indicating whether expression builder is complete.
		/// </summary>
		public bool IsExpressionValid
		{
			get
			{
                if (IsEditingExpression)
                    return CurrentExpression.Save.CanExecute(null);
                else
                    return QueryExpressions != null && QueryExpressions.Any();
			}
		}

		/// <summary>
		/// Gets a value indicating whether result settings is valid.
		/// </summary>
		public bool IsResultValid
		{
			get
			{
				return OutFields != null && OutFields.Any(o => o.IsVisible);
			}
		}

		/// <summary>
		/// Gets a value indicating whether query yielded to a result.
		/// </summary>
		public bool HasResults
		{
			get 
			{
				return resultLayer != null &&
					resultLayer.Graphics.Count > 0; 
			}
		}
		
		/// <summary>
		/// Gets a value indicating whether result layer is a table.
		/// </summary>
		public bool IsTable
		{
			get
			{
				return resultLayer != null &&
					resultLayer.LayerInfo != null &&
					string.Equals(resultLayer.LayerInfo.Type, "Table", StringComparison.OrdinalIgnoreCase);
			}
		}

		/// <summary>
		/// Gets a value indicating whether query has at least one visible expression.
		/// </summary>
		public bool HasVisibleExpression
		{
			get
			{
				return QueryExpressions != null &&
					QueryExpressions.Any(q => q.IsVisible);
			}
		}

		/// <summary>
		///  Gets the default query title based on FeatureLayerInfo.Name.
		///  This property is used when QueryTitle is null or empty.
		/// </summary>
		private string DefaultQueryTitle
		{
			get
			{
				if (resultLayer != null && resultLayer.LayerInfo != null)
					return resultLayer.LayerInfo.Name;
				return null;
			}
		}

		#region INotifyPropertyChanged

		#region Serializable Properties

		private string queryTitle;
		/// <summary>
		/// Gets or sets the QueryTitle used as title for the result layer.
		/// </summary>
		[DataMember]
		public string QueryTitle
		{
			get
			{
				if (string.IsNullOrEmpty(queryTitle))
					return DefaultQueryTitle;
				return queryTitle;
			}
			set
			{
				if (queryTitle != value)
				{
					queryTitle = value;
					OnPropertyChanged("QueryTitle");
				}
			}
		}

		private string serviceUrl;
		/// <summary>
		/// Gets or sets the service URL where query is executed against.
		/// </summary>
		[DataMember]
		public string ServiceUrl
		{
			get { return serviceUrl; }
			set
			{
				if (serviceUrl != value)
				{
					Reset();
					serviceUrl = value;
					OnPropertyChanged("ServiceUrl");
					ConnectChanged();
				}
			}
		}

		private bool useProxy;
		/// <summary>
		/// Gets or sets a value indicating whether proxy is used in web requests.
		/// </summary>
		[DataMember]
		public bool UseProxy
		{
			get { return useProxy; }
			set
			{
				if (useProxy != value)
				{
					Reset();
					useProxy = value;
					OnPropertyChanged("UseProxy");
					ConnectChanged();
				}
			}
		}

		private ObservableCollection<ExpressionViewModel> queryExpressions;
		/// <summary>
		/// Gets or sets the expressions that composes the query.
		/// </summary>
		[DataMember]
		public ObservableCollection<ExpressionViewModel> QueryExpressions
		{
			get { return queryExpressions; }
			set
			{
				if (queryExpressions != value)
				{				
					Action unsubscribe = () =>
					{
						// Unsubscribes from possible event handling.
						if (queryExpressions != null)
						{
							foreach (var item in queryExpressions)
								item.PropertyChanged -= Expression_PropertyChanged;
							queryExpressions.CollectionChanged -= QueryExpressions_CollectionChanged;
						}
					};
					Action subscribe = () =>
					{
							OnPropertyChanged("QueryExpressions");
							OnPropertyChanged("HasVisibleExpression");
							OnPropertyChanged("IsExpressionValid");
							// Subscribes to Property and Collection change to be
							// able to update properties relying on this collection.
							if (queryExpressions != null)
							{
								foreach (var item in queryExpressions)
									item.PropertyChanged += Expression_PropertyChanged;
								queryExpressions.CollectionChanged += QueryExpressions_CollectionChanged;
							}
					};
					unsubscribe();
					queryExpressions = value;
					subscribe();
				}
			}
		}

		private ObservableCollection<OutField> outFields;
		/// <summary>
		/// Gets or sets the fields from service.
		/// </summary>
		[DataMember]
		public ObservableCollection<OutField> OutFields
		{
			get { return outFields; }
			set
			{
				if (outFields != value)
				{
					Action unsubscribe = () =>
					{
						// Unsubscribes from possible event handling.
						if (outFields != null)
						{
							foreach (var item in outFields)
								item.PropertyChanged -= OutField_PropertyChanged;
							outFields.CollectionChanged -= OutFields_CollectionChanged;
						}
					};
					Action subscribe = () =>
					{
						OnPropertyChanged("OutFields");
						OnPropertyChanged("IsResultsValid");
						// Subscribes to Property and Collection Changed events to be
						// able to update properties relying on this collection.
						if (outFields != null)
						{
							foreach (var item in outFields)
								item.PropertyChanged += OutField_PropertyChanged;
							outFields.CollectionChanged += OutFields_CollectionChanged;
						}
					};
					unsubscribe();
					outFields = value;
					subscribe();
				}
			}
		}

		private bool useServiceRenderer;
		/// <summary>
		/// Gets or sets a value indicating whether service-defined renderer is used.
		/// When this property is false, a single symbol is used.
		/// </summary>
		[DataMember]
		public bool UseServiceRenderer
		{
			get { return useServiceRenderer; }
			set
			{
				if (useServiceRenderer != value)
				{
					useServiceRenderer = value;					
					OnPropertyChanged("UseServiceRenderer");					
				}
			}
		}

		private bool autoEnableDataGrid;
		/// <summary>
		/// Gets or sets a value indicating whether data grid is displayed after 
		/// query is completed.
		/// </summary>
		[DataMember]
		public bool AutoEnableDataGrid
		{
			get { return autoEnableDataGrid; }
			set
			{
				if (autoEnableDataGrid != value)
				{
					autoEnableDataGrid = value;
					OnPropertyChanged("AutoEnableDataGrid");
				}
			}
		}

		private bool autoZoomToResults;
		/// <summary>
		/// Gets or sets a value indicating whether map should zoom to the results extent
		/// when query is completed.
		/// </summary>
		[DataMember]
		public bool AutoZoomToResults
		{
			get { return autoZoomToResults; }
			set
			{
				if (autoZoomToResults != value)
				{
					autoZoomToResults = value;
					OnPropertyChanged("AutoZoomToResults");
				}
			}
		}

		private bool autoPinResults;
		/// <summary>
		/// Gets or sets a value indicating whether query results are automatically pinned to map 
		/// is enabled in the query tool.
		/// </summary>
		[DataMember]
		public bool AutoPinResults
		{
			get { return autoPinResults; }
			set
			{
				if (autoPinResults != value)
				{
					autoPinResults = value;
					OnPropertyChanged("AutoPinResults");
					OnPropertyChanged("IsPinned");
					PinChanged();
				}
			}
		}

		#endregion

		#region Non-serializable Properties

		private bool isConnected;
		/// <summary>
		/// Gets or sets a value indicating whether connection to service URL was attempted.
		/// </summary>
		public bool IsConnected
		{
			get { return isConnected; }
			set
			{
				if (isConnected != value)
				{
					isConnected = value;
                    OnPropertyChanged("IsConnected");
                    OnPropertyChanged("Fields");
                    OnPropertyChanged("IsConnectionValid");
                    ConnectChanged();
				}
			}
		}

        private ExpressionViewModel currentExpression;
        public ExpressionViewModel CurrentExpression
        {
            get { return currentExpression; }
            private set
            {
                if (currentExpression != value)
                {
                    currentExpression = value;
                    OnPropertyChanged("CurrentExpression");
                }
            }
        }

        private bool isEditingExpression;
        public bool IsEditingExpression
        {
            get { return isEditingExpression; }
            private set
            {
                if (isEditingExpression != value)
                {
                    isEditingExpression = value;
                    OnPropertyChanged("IsEditingExpression");
                    OnPropertyChanged("IsExpressionValid");
                }
            }
        }

		private string status;
		/// <summary>
		/// Gets or sets the status of query tool.
		/// </summary>
		public string Status
		{
			get { return status; }
			private set
			{
				if (status != value)
				{
					status = value;
					if (!string.IsNullOrEmpty(status))
						Error = null;
					OnPropertyChanged("Status");
				}
			}
		}

		private Exception error;
		/// <summary>
		/// Gets or sets the exception thrown by query tool.
		/// </summary>
		public Exception Error
		{
			get { return error; }
			private set
			{
				if (error != value)
				{
					error = value;
					if (error != null)
						Status = null;
					OnPropertyChanged("Error");
				}
			}
		}
	
		private bool isBusy;
		/// <summary>
		/// Gets or sets a value indicating whether query tool is busy processing
		/// a web request (i.e. connecting to service or executing query).
		/// </summary>
		public bool IsBusy
		{
			get { return isBusy; }
			private set
			{
				if (isBusy != value)
				{
					isBusy = value;
					OnPropertyChanged("IsBusy");
				}
			}
		}

		private bool isPinned;
		/// <summary>
		/// Gets or sets a value indicating whether query results must be pinned to the map.
		/// </summary>
		public bool IsPinned
		{
			get 
			{
				if (AutoPinResults)
					return true;
				return isPinned; 
			}
			set
			{
				if (isPinned != value)
				{
					isPinned = value;
					OnPropertyChanged("IsPinned");
					PinChanged();
				}
			}
		}
		
		private string groupLabel = Resources.Strings.GroupLabel;
		/// <summary>
		/// Gets the label for the Group command.
		/// The Group will ungroup when the selected expressions already belong
		/// to the same group.
		/// </summary>
		public string GroupLabel
		{
			get { return groupLabel; }
			private set
			{
				if (groupLabel != value)
				{
					groupLabel = value;
					OnPropertyChanged("GroupLabel");
				}
			}
		}

        //private Action<ExpressionViewModel> editExpression;
        ///// <summary>
        ///// Gets or sets the action for updating the visibility of FeatureDataGrid.
        ///// </summary>
        //public Action<ExpressionViewModel> EditExpression
        //{
        //    get { return editExpression; }
        //    set
        //    {
        //        if (editExpression != value)
        //        {
        //            editExpression = value;
        //            OnPropertyChanged("EditExpression");
        //        }
        //    }
        //}

		private Action<Visibility> updateDataGridVisibility;
		/// <summary>
		/// Gets or sets the action for updating the visibility of FeatureDataGrid.
		/// </summary>
		public Action<Visibility> UpdateDataGridVisibility
		{
			get { return updateDataGridVisibility; }
			set
			{
				if (updateDataGridVisibility != value)
				{
					updateDataGridVisibility = value;
					OnPropertyChanged("UpdateDataGridVisibility");
				}
			}
		}

		#endregion

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string property)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}
		#endregion

		#region ICommand

		private DelegateCommand connectCommand;
		/// <summary>
		/// Gets the command responsible for connecting to the service
		/// and retrieving the fields information.
		/// </summary>
		public ICommand Connect
		{
			get
			{
				if (connectCommand == null)
				{
					connectCommand = new DelegateCommand(OnConnect, CanConnect);
				}
				return connectCommand;
			}
		}

		private void ConnectChanged()
		{
			if (connectCommand != null)
			{
				connectCommand.RaiseCanExecuteChanged();
			}
		}

		private bool CanConnect(object commandParameter)
		{
			return !string.IsNullOrEmpty(ServiceUrl) && !IsConnected;
		}

		/// <summary>
		/// Connects to the service to retrieve fields information.
		/// </summary>
		/// <param name="commandParameter"></param>
		private void OnConnect(object commandParameter)
		{
			if (!CanConnect(commandParameter)) return;
			try
			{
                bool savedConfiguration = commandParameter as string == Constants.SAVED_CONFIGURATION;
                if (!savedConfiguration)
    				Reset();

				IsBusy = true;

				initializedAction = (f) =>
				{
					IsConnected = true;
					if (f == null || f.LayerInfo == null || f.LayerInfo.Fields == null)
						Error = new Exception(Resources.Strings.LayerInfoMissing);
					if (IsTable)
					{
						UseServiceRenderer = false;
						AutoPinResults = false;
						AutoZoomToResults = false;
					}
					OnPropertyChanged("QueryTitle");
					OnPropertyChanged("Fields");
					OnPropertyChanged("HasResults");
					OnPropertyChanged("IsTable");
					OnPropertyChanged("IsConnectionValid");
					AddChanged();

                    if (!savedConfiguration)
                        InitalizeOutFields();
                    else
                        initializeExpressions();
				};

				var url = !ServiceUrl.StartsWith("http") ? string.Format("http://{0}", ServiceUrl) : ServiceUrl;

				resultLayer = new FeatureLayer()
				{
					DisableClientCaching = true,
					Url = url
				};
				if (UseProxy && !string.IsNullOrEmpty(ProxyUrl))
					resultLayer.ProxyUrl = ProxyUrl;

				// Subscribes to initialized event and initializes layer.
				resultLayer.InitializationFailed += ResultLayer_InitializationFailed;
				resultLayer.Initialized += ResultLayer_Initialized;

				resultLayer.Initialize();
			}
			catch (Exception ex)
			{
				IsConnected = true;
				IsBusy = false;
				if (ex is UriFormatException)
				{
					Error = new UriFormatException(string.Format(Resources.Strings.UriFormatError, ServiceUrl), ex);
				}
				else
				{
					Error = new Exception(string.Format(Resources.Strings.UnableToAccess, ServiceUrl), ex);
				}
			}
		}

		private DelegateCommand selectCommand;
		/// <summary>
		/// Gets the command responsible for marking QueryExpressions as selected.
		/// </summary>
		public ICommand Select
		{
			get
			{
				if (selectCommand == null)
				{
					selectCommand = new DelegateCommand(OnSelect, CanSelect);
				}
				return selectCommand;
			}
		}

		private void SelectChanged()
		{
			if (selectCommand != null)
			{
				selectCommand.RaiseCanExecuteChanged();
			}
		}

		private bool CanSelect(object commandParameter)
		{
			return QueryExpressions != null && QueryExpressions.Any();
		}

		/// <summary>
		/// Updates QueryExpression selection state based on items in the command parameter.
		/// </summary>
		/// <param name="commandParameter">Items to select.</param>
		private void OnSelect(object commandParameter)
		{
			if (!CanSelect(commandParameter)) return;
			var selectedItems = commandParameter as IList;
			if (selectedItems != null)
			{
				foreach(var item in QueryExpressions)
					item.IsSelected = false;
				foreach (var item in selectedItems)
				{
					var expression = QueryExpressions.FirstOrDefault(q => q == item as ExpressionViewModel);
					if (expression != null)
						expression.IsSelected = true;
				}
			}
			AddChanged();
			RemoveChanged();
			GroupChanged();
		}

		private DelegateCommand addCommand;
		/// <summary>
		/// Gets the command responsible for adding a new expression.
		/// </summary>
		public ICommand Add
		{
			get
			{
				if (addCommand == null)
				{
					addCommand = new DelegateCommand(OnAdd, CanAdd);
				}
				return addCommand;
			}
		}

		private void AddChanged()
		{
			if (addCommand != null)
			{
				addCommand.RaiseCanExecuteChanged();
			}
		}

		private bool CanAdd(object commandParameter)
		{
			return Fields != null && Fields.Any();
		}

		/// <summary>
		/// Adds new expression to the collection of expressions that form the query.
		/// </summary>
		/// <param name="commandParameter"></param>
		private void OnAdd(object commandParameter)
		{
			if (!CanAdd(commandParameter)) return;


            ExpressionViewModel newExpression = new ExpressionViewModel();
			// Create and initialize new expression.
            initializeExpression(newExpression);

			// Displays the Expression Builder.
            IsEditingExpression = true;
            //if (EditExpression != null)
            //    EditExpression(newExpression);
		}

        private void initializeExpression(ExpressionViewModel expression)
        {
            expression.ServiceUrl = ServiceUrl;
            expression.UseProxy = UseProxy;
            expression.ProxyUrl = ProxyUrl;
            expression.Fields = Fields;

            expression.PropertyChanged += Expression_PropertyChanged;

            // Creates a view with the new expression as its DataContext.

            // Create a copy of the old expression because updating CurrentExpression somehow changes properties
            // on it via binding
            ExpressionViewModel oldExpressionClone = CurrentExpression != null ? CurrentExpression.Clone() : null;
            ExpressionViewModel oldExpression = CurrentExpression;

            CurrentExpression = expression;

            // Re-apply the settings from before updating CurrentExpression to the previous CurrentExpression
            if (oldExpression != null)
                oldExpression.ApplyChanges(oldExpressionClone);

            expression.CancelAction = () =>
            {
                IsEditingExpression = false;
            };

            expression.SaveAction = () =>
            {
                var position = QueryExpressions != null ? QueryExpressions.Count : 0;
                var groupID = 0;

                // Performs an add where new expression is added
                // to the end of the collection.
                var last = QueryExpressions != null ? QueryExpressions.LastOrDefault() : null;
                if (last != null)
                    groupID = last.GroupID + 1;

                expression.GroupID = groupID;
                expression.Position = position;
                QueryExpressions.Insert(position, expression);

                IsEditingExpression = false;
            };

            expression.EditAction = () =>
            {
                // Creates a clone of the selected item.
                var selected = expression;
                var clone = new ExpressionViewModel(selected);
                CurrentExpression = clone;

                // Default value should be allowed to be empty while editing the expression
                if (CurrentExpression.ChoiceValues != null && CurrentExpression.ChoiceValues.DefaultValue != null)
                    CurrentExpression.ChoiceValues.DefaultValue.AllowEmptyValues = true;

                // Creates a view with the clone as DataContext

                clone.CancelAction = () =>
                {
                    // Hides the view without updating the expression.
                    IsEditingExpression = false;
                };

                clone.SaveAction = () =>
                {
                    selected.ApplyChanges(clone);
                    // Hides the view and updates the expression.
                    IsEditingExpression = false;
                };

                // Displays the Expression Builder.
                IsEditingExpression = true;
                //if (EditExpression != null)
                //    EditExpression(clone);
            };

            expression.RemoveAction = () =>
            {
                // Updates the collection by removing expression and updating the groupIDs
                var selected = expression;
                int groupID = selected.GroupID;
                QueryExpressions.Remove(selected);
                updateExpressionGrouping(groupID);
                selected.PropertyChanged -= Expression_PropertyChanged;
            };
        }

        private void initializeExpressions()
        {
            foreach (ExpressionViewModel expression in QueryExpressions)
                initializeExpression(expression);
        }

		private DelegateCommand removeCommand;
		// Gets the command responsible for deleting the selected expressions from the collection.
		public ICommand Remove
		{
			get
			{
				if (removeCommand == null)
				{
					removeCommand = new DelegateCommand(OnRemove, CanRemove);
				}
				return removeCommand;
			}
		}

		private void RemoveChanged()
		{
			if (removeCommand != null)
			{
				removeCommand.RaiseCanExecuteChanged();
			}
		}

		private bool CanRemove(object commandParameter)
		{
			return QueryExpressions != null && QueryExpressions.Any(q => q.IsSelected);
		}

		/// <summary>
		/// Removes the selected expressions from the collection of expressions that form the query.
		/// </summary>
		/// <param name="commandParameter"></param>
		private void OnRemove(object commandParameter)
		{
			if (!CanRemove(commandParameter)) return;
			// Determines the affected groups and updates the remaining expression's GroupID.
			var selected = QueryExpressions.Where(q => q.IsSelected).ToList();
			var groupIDs = new List<int>();
			foreach (var item in selected)
			{
                if (!groupIDs.Contains(item.GroupID))
                    groupIDs.Add(item.GroupID);
				QueryExpressions.Remove(item);
			}

            // Update affected groups
            foreach (int id in groupIDs)
                updateExpressionGrouping(id);
		}
		
		private DelegateCommand groupCommand;
		/// <summary>
		/// Gets the command responsible for grouping the selected expressions.
		/// </summary>
		public ICommand Group
		{
			get
			{
				if (groupCommand == null)
				{
					groupCommand = new DelegateCommand(OnGroup, CanGroup);
				}
				return groupCommand;
			}
		}

		private void GroupChanged()
		{
			if (groupCommand != null)
			{
				groupCommand.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// Group command is only enabled for consecutive selections.
		/// </summary>
		/// <param name="commandParameter"></param>
		/// <returns></returns>
		private bool CanGroup(object commandParameter)
		{
			var selectedItems = QueryExpressions.Where(q => q.IsSelected);
			if (selectedItems.Count() < 2) return false;
			var selectedIds = (from s in selectedItems select s.Position).ToList();
			bool canGroup = false;
			ExpressionViewModel previous = null;			
			foreach (var q in selectedItems)
			{
				// Determine operation (group or ungroup) based on their current group id.
				var groupID = q.GroupID;
				if (previous == null)
					previous = q;
				else if (previous.GroupID != groupID)
					canGroup = true;

				// Check that it is a consecutive selection.
				if (q.Position - previous.Position > 1)
				{
					return false;
				}

				// Check that selectedIds is a subset of another group.
				if (GroupLookup.ContainsKey(q.GroupID))
				{
					var ids = GroupLookup[groupID];
					foreach (var id in ids)
					{
						if (!selectedIds.Contains(id))
						{							
							return false;
						}
					}
				}
				previous = q;
			}
			GroupLabel = canGroup ? Resources.Strings.GroupLabel : Resources.Strings.UngroupLabel;
			return true;
		}

		/// <summary>
		/// Updates the GroupID of selected expressions.
		/// </summary>
		/// <param name="commandParameter"></param>
		private void OnGroup(object commandParameter)
		{
			if (!CanGroup(commandParameter)) return;
			var selectedItems = QueryExpressions.Where(q => q.IsSelected);
			var topItem = selectedItems.FirstOrDefault();
			var selectedIds = (from s in selectedItems select s.Position).ToList();
            //Action undoAction = null;
			if (GroupLabel == Resources.Strings.GroupLabel)
			{
				// Remove selected ids from their old group.
				foreach (var q in selectedItems)
				{
					var groupID = q.GroupID;
					if (GroupLookup.ContainsKey(groupID))
					{
						var ids = GroupLookup[groupID];						
						ids.Remove(groupID);
						if (ids.Count == 0)
						{
							GroupLookup.Remove(groupID);
						}
					}
				}


				// Add or update existing group to include selected ids.	
				int newGroupID = topItem != null && topItem.GroupID > -1 ? topItem.GroupID :
					(GroupLookup.Count > 0 ? GroupLookup.Last().Key + 1 : 0);
				while (QueryExpressions.Any(q => !selectedIds.Contains(q.Position) && q.GroupID == newGroupID))
					newGroupID++;
				if (GroupLookup.ContainsKey(newGroupID))
				{
					var ids = GroupLookup[newGroupID];
					ids.AddRange(selectedIds);
				}
				else
					GroupLookup[newGroupID] = new List<int>(selectedIds);

				// Update selection GroupID and flag position in group
				if (newGroupID > -1)
				{
                    int count = selectedItems.Count();
                    for (int i = 0; i < count; i++)  
					{
                        var item = selectedItems.ElementAt(i);
						var oldGroupID = item.GroupID;
						item.GroupID = newGroupID;				
                        if (i == 0)
                        {
                            item.FirstInGroup = true;
                            item.InMiddleOfGroup = false;
                            item.LastInGroup = false;
                        }
                        else if (i == count - 1)
                        {
                            item.FirstInGroup = false;
                            item.InMiddleOfGroup = false;
                            item.LastInGroup = true;
                        }
                        else
                        {
                            item.FirstInGroup = false;
                            item.InMiddleOfGroup = true;
                            item.LastInGroup = false;
                        }
					}
				}
			}
			else
			{
				// Remove selected ids from their old group.
				var oldGroupID = topItem != null ? topItem.GroupID : -1;				
				if (GroupLookup.ContainsKey(oldGroupID))
				{
					var ids = GroupLookup[oldGroupID];
					var count = ids.Count;
					for (int i = ids.Count-1; i >= 0; i--)
					{
						var id = ids[i];
						if (selectedIds.Contains(id))
							ids.Remove(id);
						if (ids.Count == 0)
							GroupLookup.Remove(oldGroupID);
					}
				}

				// Update selection GroupID
				int newGroupID = GroupLookup.Count > 0 ? GroupLookup.Last().Key + 1 : 0;				
				while (QueryExpressions.Any(q => !selectedIds.Contains(q.Position) && q.GroupID == newGroupID))
					newGroupID++;
				foreach (var item in selectedItems)
				{
					var oldGroup = item.GroupID;
					item.GroupID = newGroupID++;
                    item.FirstInGroup = false;
                    item.InMiddleOfGroup = false;
                    item.LastInGroup = false;
				}
			}
			GroupChanged();
		}

		private DelegateCommand executeCommand;
		/// <summary>
		/// Gets the command responsible for executing query.
		/// </summary>
		public ICommand Execute
		{
			get
			{
				if (executeCommand == null)
				{
					executeCommand = new DelegateCommand(OnExecute, CanExecute);
				}
				return executeCommand;
			}
		}

		private void ExecuteChanged()
		{
			if (executeCommand != null)
			{
				executeCommand.RaiseCanExecuteChanged();
			}
		}
		
		private bool CanExecute(object commandParameter)
		{
			return QueryExpressions != null && QueryExpressions.All(q => q.HasValueSet);
		}

		/// <summary>
		/// Executes the query on the feature or map service.
		/// </summary>
		/// <param name="commandParameter"></param>
		private void OnExecute(object commandParameter)
		{
			if (!CanExecute(commandParameter)) return;

			// Removes the result layer from map when not pinned.
			ClearResults();

			IsBusy = true;

			// Sets the initialize action which is executed after layer has initialized.
			initializedAction = (f) =>
			{
				if (f != null && f.LayerInfo != null)
				{
					IsBusy = true;

					// Updates renderer based on geometry type
					// when opting out of using service-defined renderer.
                    if (!UseServiceRenderer && !IsTable)
                        f.Renderer = new SimpleRenderer() { Symbol = GetSymbol(f.LayerInfo.GeometryType) };

                    if (IsTable) // hide tables from map contents
                        LayerProperties.SetIsVisibleInMapContents(f, false);

                    f.ID = Guid.NewGuid().ToString();
                    if (SetLayerNameAction != null)
                        SetLayerNameAction(f, QueryTitle);

					//  Add layer to map.
					if (AddLayerAction != null)
						AddLayerAction(resultLayer);

					// Subscribes and updates FeatureLayer to query the service.
					f.UpdateFailed += ResultLayer_UpdateFailed;
					f.UpdateCompleted += ResultLayer_UpdateCompleted;
					f.Update();
				}
			};

			var url = !ServiceUrl.StartsWith("http") ? string.Format("http://{0}", ServiceUrl) : ServiceUrl;
			// Creates a new result layer.
			resultLayer = new FeatureLayer()
			{
				DisableClientCaching = true,
				Url = url
			};
			if (UseProxy && !string.IsNullOrEmpty(ProxyUrl))
				resultLayer.ProxyUrl = ProxyUrl;
			
			// Updates FeatureLayer.OutFields with visible fields
			resultLayer.OutFields.AddRange(from f in OutFields where f.IsVisible select f.Name);

			// Updates FeatureLayer.Where clause with QueryExpressions
			if (QueryExpressions != null)
			{
				var whereClause = new StringBuilder();
				ExpressionViewModel previous = null;
				bool mustEnd = false;
				// Iterates through each expression determining its position in a group
				// and calls its ToString() method.
				for (int i = 0; i < QueryExpressions.Count; i++)
				{
					var current = QueryExpressions[i];
					ExpressionViewModel next = null;
					if (i + 1 < QueryExpressions.Count)
						next = QueryExpressions[i + 1];
					bool startOfGroup = previous == null ||
						(previous.GroupID != current.GroupID &&
						next != null && current.GroupID == next.GroupID);
					if (startOfGroup)
						mustEnd = true;
					bool endOfGroup = mustEnd &&
						(next == null || next.GroupID != current.GroupID);
					if (endOfGroup)
						mustEnd = false;
					if (current.HasValueSet)
						whereClause.Append(current.ToString(startOfGroup, endOfGroup));
					previous = current;
				}
				resultLayer.Where = whereClause.ToString();
			}

			// Subscribes to initialized event and initializes layer.
			resultLayer.InitializationFailed += ResultLayer_InitializationFailed;
			resultLayer.Initialized += ResultLayer_Initialized;
			resultLayer.Initialize();
		}
				
		private DelegateCommand pinCommand;
		/// <summary>
		/// Gets the command responsible for pinning query results to map.
		/// </summary>
		public ICommand Pin
		{
			get
			{
				if (pinCommand == null)
				{
					pinCommand = new DelegateCommand(OnPin, CanPin);
				}
				return pinCommand;
			}
		}

		private void PinChanged()
		{
			if (pinCommand != null)
			{
				pinCommand.RaiseCanExecuteChanged();
			}
		}

		private bool CanPin(object commandParameter)
		{
			return !AutoPinResults && !IsTable && HasResults && !IsPinned;
		}

		/// <summary>
		/// Marks query result layer pinned to prevent it from being cleared from map.
		/// </summary>
		/// <param name="commandParameter"></param>
		private void OnPin(object commandParameter)
		{
			IsPinned = true;
		}

		#endregion

		#region Utility Methods

		/// <summary>
		/// Resets properties to their default values.
		/// </summary>
		private void Reset()
		{
			if (enableReset)
			{
				ResetStatus();
				IsConnected = false;
				IsBusy = false;
				IsPinned = false;
				resultLayer = null;
				QueryTitle = null;
				if (QueryExpressions != null)
					QueryExpressions.Clear();
				if (OutFields != null)
					OutFields.Clear();
            }
		}

		/// <summary>
		/// Initializes OutFields based on FeatureLayerInfo.Fields.
		/// </summary>
		private void InitalizeOutFields()
		{
			if (Fields == null) return;
			OutFields.Clear();
			foreach (var f in Fields)
				OutFields.Add(new OutField(f));
		}
		
		/// <summary>
		/// Updates serializable properties with values from another QueryViewModel.
		/// </summary>
		/// <param name="source">The source QueryViewModel.</param>
		private void CopyAllSettings(QueryViewModel source)
		{
			if (source == null) return;
			enableReset = false;
			ServiceUrl = source.ServiceUrl;
			UseProxy = source.UseProxy;
			enableReset = true;
			IsConnected = source.IsConnected;
			QueryTitle = source.QueryTitle;

            ObservableCollection<ExpressionViewModel> expressions = new ObservableCollection<ExpressionViewModel>();
            if (source.QueryExpressions != null)
            {
                foreach (ExpressionViewModel e in source.queryExpressions)
                {
                    ExpressionViewModel newExp = e.Clone();
                    expressions.Add(newExp);
                }
            }
            QueryExpressions = expressions;
			OutFields = source.OutFields != null ?
				new ObservableCollection<OutField>(from o in source.OutFields select o.Clone()) :
				new ObservableCollection<OutField>();
			AutoEnableDataGrid = source.AutoEnableDataGrid;
			AutoPinResults = source.AutoPinResults;
			AutoZoomToResults = source.AutoZoomToResults;
			UseServiceRenderer = source.UseServiceRenderer;
		}

		/// <summary>
		/// Gets the symbol based on geometry type.
		/// </summary>
		/// <param name="geometryType">Geometry Type</param>
		/// <returns>Symbol for the geometry type.</returns>
		private static Symbol GetSymbol(GeometryType geometryType)
		{
			switch (geometryType)
			{
				case GeometryType.Envelope:
				case GeometryType.Polygon:
					return ResourceData.Dictionary["SelectFillSymbol"] as FillSymbol;
				case GeometryType.Polyline:
					return ResourceData.Dictionary["SelectLineSymbol"] as LineSymbol;
				case GeometryType.MultiPoint:
				case GeometryType.Point:
					return ResourceData.Dictionary["SelectMarkerSymbol"] as MarkerSymbol;
			}
			return null;
		}

        private void updateExpressionGrouping(int groupId)
        {
            if (QueryExpressions.Any(e => e.GroupID == groupId))
            {
                var itemsInGroup = QueryExpressions.Where(e => e.GroupID == groupId);
                var itemCount = itemsInGroup.Count();
                if (itemCount == 1)
                {
                    ExpressionViewModel expression = itemsInGroup.ElementAt(0);
                    expression.FirstInGroup = false;
                    expression.InMiddleOfGroup = false;
                    expression.LastInGroup = false;
                }
                else
                {
                    for (int i = 0; i < itemCount; i++)
                    {
                        ExpressionViewModel expression = itemsInGroup.ElementAt(i);
                        if (i == 0)
                        {
                            expression.FirstInGroup = true;
                            expression.InMiddleOfGroup = false;
                            expression.LastInGroup = false;
                        }
                        else if (i == itemCount - 1)
                        {
                            expression.FirstInGroup = false;
                            expression.InMiddleOfGroup = false;
                            expression.LastInGroup = true;
                        }
                        else
                        {
                            expression.FirstInGroup = false;
                            expression.InMiddleOfGroup = true;
                            expression.LastInGroup = false;
                        }
                    }
                }
            }
        }

		#endregion

		#region Event Handling

		private void ResultLayer_Initialized(object sender, EventArgs e)
		{		
			(sender as Layer).Initialized -= ResultLayer_Initialized;
			IsBusy = false;

			if ((sender as Layer).InitializationFailure == null && (sender as FeatureLayer).LayerInfo != null)
			{
				Status = string.Format(Resources.Strings.ConnectCompleted, (sender as FeatureLayer).LayerInfo.Name);
				if (initializedAction != null)
					initializedAction(sender as FeatureLayer);
			}
			else
			{
				var ex = (sender as Layer).InitializationFailure;
				Error = new Exception(string.Format(Resources.Strings.UnableToAccess, (sender as FeatureLayer).Url), ex);
			}
		}

		private void ResultLayer_InitializationFailed(object sender, EventArgs e)
		{
			IsConnected = true;
			(sender as Layer).InitializationFailed -= ResultLayer_InitializationFailed;
			(sender as Layer).Initialized -= ResultLayer_Initialized;
			IsBusy = false;
			var ex = (sender as Layer).InitializationFailure;
			Error = new Exception(string.Format(Resources.Strings.UnableToAccess, (sender as FeatureLayer).Url), ex);
		}

		private void ResultLayer_UpdateFailed(object sender, TaskFailedEventArgs e)
		{			
			(sender as FeatureLayer).UpdateFailed -= ResultLayer_UpdateFailed;
			(sender as FeatureLayer).UpdateCompleted -= ResultLayer_UpdateCompleted;
			IsBusy = false;
			Error = e.Error;
		}

		private void ResultLayer_UpdateCompleted(object sender, EventArgs e)
		{
            FeatureLayer layer = (FeatureLayer)sender;
			layer.UpdateFailed -= ResultLayer_UpdateFailed;
			layer.UpdateCompleted -= ResultLayer_UpdateCompleted;
			IsBusy = false;
			OnPropertyChanged("HasResults");			
			OnPropertyChanged("IsTable");
			PinChanged();
			var graphicsCount = layer.Graphics.Count;
			Status = string.Format(Resources.Strings.ResultsFound, graphicsCount);

            if (graphicsCount > 0)
            {
                // Shows attribute table when auto-enabled is set.
                if (AutoEnableDataGrid)
                {
                    if (SelectLayerAction != null)
                        SelectLayerAction(layer);
                    if (UpdateDataGridVisibility != null)
                        UpdateDataGridVisibility(Visibility.Visible);
                }

                // Zooms to result extent when auto-zoom is set.
                if (AutoZoomToResults)
                {
                    if (ZoomToExtentAction != null)
                        ZoomToExtentAction(layer.FullExtent);
                }
            }
            else if (RemoveLayerAction != null)
            {
                RemoveLayerAction(layer);
            }
		}

		private void OutField_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsVisible")
				OnPropertyChanged("IsResultValid");
		}

		private void OutFields_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{			
			OnPropertyChanged("IsResultValid");
		}

		private void Expression_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
            OnPropertyChanged(e.PropertyName);

            if (e.PropertyName == "IsVisible")
				OnPropertyChanged("HasVisibleExpression");

            if (IsEditingExpression)
                OnPropertyChanged("IsExpressionValid");

			ExecuteChanged();
		}

		private void QueryExpressions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
            if (e.OldItems != null)
            {
                foreach (ExpressionViewModel exp in e.OldItems)
                {
                    if (GroupLookup.ContainsKey(exp.GroupID))
                    {
                        List<int> ids = GroupLookup[exp.GroupID];

                    }
                }
            }

			foreach (var item in QueryExpressions)
				item.Position = QueryExpressions.IndexOf(item);

            // Rebuild group lookups
            GroupLookup.Clear();
            var processedGroups = new List<int>();
            foreach (var item in QueryExpressions)
            {
                // Make sure the group hasn't already been processed
                if (processedGroups.Contains(item.GroupID))
                    continue;

                // Get all the expressions in the same group as the current one
                var itemsInGroup = QueryExpressions.Where(exp => exp.GroupID == item.GroupID);

                // If there is more than one in the group, then add the IDs of the expressions belonging
                // to the group to the lookup collection
                if (itemsInGroup.Count() > 1)
                {
                    var idsInGroup = new List<int>();
                    foreach (var exp in itemsInGroup)
                        idsInGroup.Add(exp.Position);
                    groupLookup.Add(item.GroupID, idsInGroup);
                }
                processedGroups.Add(item.GroupID);
            }

            if (!IsEditingExpression)
    			OnPropertyChanged("IsExpressionValid");

			OnPropertyChanged("HasVisibleExpression");
			AddChanged();
			RemoveChanged();
			GroupChanged();
		}
		
		#endregion
	}
}
