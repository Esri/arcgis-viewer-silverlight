/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.FeatureService;
using ESRI.ArcGIS.Client.Tasks;
using QueryTool.Resources;

namespace QueryTool.AddIns
{
	/// <summary>
	/// This class represents the expression of query, which consists of field, comparison operator and value.
	/// </summary>
	[DataContract]
	public class ExpressionViewModel : INotifyPropertyChanged
	{
		// Separates operators in categories of generic, nullable, string-only, and logical.  Not that IN and NOT IN require
        // special handling and are being omitted for now.  When implemented, users should have the ability to specify sets of
        // values to check against.
        private static string[] GENERIC_OPERATORS = new string[] { "=", "<", "<=", "<>", ">", ">=" }; //, "IN", "NOT IN" };
		private static string[] NULLABLE_OPERATORS = new string[] { "IS NULL", "IS NOT NULL" };
		private static string[] STRING_ONLY_OPERATORS = new string[] { "LIKE", "NOT LIKE" };
		private static string[] LOGICAL_OPERATORS = new string[] { "and", "or" };
	
		
		/// <summary>
		/// Constructor for the ExpressionViewModel object.
		/// </summary>
		public ExpressionViewModel()
		{
			IsVisible = true;
			JoinedBy = "and";
			UseDefaultValue = true;
		}

		/// <summary>
		/// Copy constructor for the ExpressionViewModel object.
		/// </summary>
		/// <param name="source"></param>
		public ExpressionViewModel(ExpressionViewModel source)
		{			
			CopyAllSettings(source);
		}
		
		/// <summary>
		/// Updates properties of current object with properties of source object.
		/// </summary>
		/// <param name="source"></param>
		public void ApplyChanges(ExpressionViewModel source)
		{
			CopyAllSettings(source);
		}

        public ExpressionViewModel Clone()
        {
            return new ExpressionViewModel(this);
        }

		/// <summary>
		/// Resets Expression to default value/choices.
		/// </summary>
		public void Reset()
		{
			if (ChoiceValues != null)
			{
				ChoiceValues.Reset(UseDefaultValue);
			}
		}
		
		/// <summary>
		/// Gets or sets the URL to proxy request through.
		/// </summary>
		public string ProxyUrl { get; set; }

		/// <summary>
		/// Returns ExpressionViewModel into string format with 
		/// its position in a group considered.
		/// </summary>
		/// <param name="startOfGroup"></param>
		/// <param name="endOfGroup"></param>
		/// <returns></returns>
		public string ToString(bool startOfGroup, bool endOfGroup)
		{
			if (string.IsNullOrEmpty(FieldName) || ChoiceValues == null || string.IsNullOrEmpty(comparisonOperator)) return null;
			var expression = new StringBuilder();

            // Get value to compare against.  If SelectedChoice is not null, then the user has been given options to select from,
            // so use that.  If it is null, then the user has been left to define their own value - use CurrentValue.
            FieldValue value = ChoiceValues.SelectedChoice ?? ChoiceValues.CurrentValue;

			expression.AppendFormat(" {0} {1} {2} ",
				IsFirst ? string.Empty : JoinedBy, startOfGroup ? "(" : string.Empty,
				FieldName);
			if (!UseMultipleValues && value == null)
				expression.Append("IS NULL");
			else
				expression.Append(ComparisonOperator);
			if (!IsNullValue)
			{
                if (!UseMultipleValues && value != null && value.Value != null)
				{
                    expression.AppendFormat(" {0}", value.ToString());
				}
				else if (UseMultipleValues)
				{
					if (ChoiceValues.Choices != null && ChoiceValues.Choices.Any())
					{
						var values = new List<object>();
						values.AddRange(from item in ChoiceValues.Choices
										select item.ToString());
						expression.AppendFormat(" ({0})", string.Join(" , ",
							from v in values
							select v));
					}
                    else if (value != null && value.Value != null)
                        expression.AppendFormat(" ({0})", value.ToString());
				}
			}
			if (endOfGroup)
				expression.Append(")");
			return expression.ToString();
		}

		/// <summary>
		/// Gets a value indicating whether expression has its value set.
		/// </summary>
		public bool HasValueSet
		{
			get
			{
				return IsNullValue // IS NULL or IS NOT NULL operator
                    || (ChoiceValues != null // otherwise ChoiceValues must be non-null

                    // Default value is being used (i.e. user can type-in value)
                    && ((UseDefaultValue && (ChoiceValues.CurrentValue == null 
                        || (ChoiceValues.CurrentValue.Value != null && ChoiceValues.CurrentValue.ValidationException == null))) 
                    
                    // Pick list is being used
                    || (ChoiceValues.SelectedChoice != null && ChoiceValues.SelectedChoice.Value != null 
                        && ChoiceValues.SelectedChoice.ValidationException == null) 

                    // Multi-value is being used (not currently implemented)
                    || (UseMultipleValues && ChoiceValues.Choices != null && ChoiceValues.Choices.Any() 
                        && !ChoiceValues.Choices.Any(c => c.IsEditing || c.ValidationException != null))));
			}
		}
		
		/// <summary>
		/// Gets a value indicating whether expression has its choices set.
		/// </summary>
		public bool HasChoices
		{
			get
			{
				return ChoiceValues != null &&
					(UseDefaultValue && (ChoiceValues.DefaultValue == null ||  ChoiceValues.DefaultValue.ValidationException == null)) ||
					(!UseDefaultValue && ChoiceValues.Choices != null && ChoiceValues.Choices.Any() && !ChoiceValues.Choices.Any(c => c.IsEditing || c.ValidationException != null));
			}
		}

		/// <summary>
		/// Gets a value indicating whether expression is pending validation.
		/// </summary>
		public bool IsValidating
		{
			get
			{
				return Field != null && ChoiceValues != null && ChoiceValues.IsValidating;
			}
		}
				
		/// <summary>
		/// Gets a value indicating whether expression field has domain information.
		/// </summary>
		public bool HasDomain
		{
			get
			{
				return (Field != null && Field.Domain != null)
					|| (OutField != null && OutField.Domain != null);
			}
		}

		/// <summary>
		/// Gets a value indicating whether this expression is first in the
		/// collection of expressions.
		/// </summary>		
		public bool IsFirst
		{
			get { return Position == 0; }
		}
		private bool ignoreFieldUpdate = false;
		private bool ignoreComparisonUpdate = false;

		private class ChoiceOptions
		{
			public bool UseDefault;
			public string ComparisonOperator;
			public IEnumerable<string> ComparisonOperators;
			public ChoiceValues ChoiceValues;
		}

		private Dictionary<OutField, ChoiceOptions> choiceValuesCached;
		/// <summary>
		/// Gets the dictionary that stores choice values for caching.
		/// </summary>
		private Dictionary<OutField, ChoiceOptions> ChoiceValuesCached
		{
			get
			{
				if (choiceValuesCached == null)
					choiceValuesCached = new Dictionary<OutField, ChoiceOptions>();
				return choiceValuesCached;
			}
		}
	
		/// <summary>
		/// Gets the logical operators for joining this expression with other expressions
		/// </summary>
		public IEnumerable<string> LogicalOperators
		{
			get
			{
				foreach (var item in LOGICAL_OPERATORS)
					yield return item;
			}
		}

		/// <summary>
		/// Gets the default expression label from field and comparison operator.
		/// </summary>
		public string DefaultExpressionLabel
		{
			get
			{
				var expression = new StringBuilder();
				if (!string.IsNullOrEmpty(FieldAlias) && !string.IsNullOrEmpty(ComparisonOperator))
				{
					expression.Append(FieldAlias);
                    if (!IsNullValue)
    					expression.AppendFormat(" {0} {1}", ComparisonOperator, Strings.Value);
                    else
                        expression.AppendFormat(" {0}", ComparisonOperator); // Don't include [value] suffix for IS [NOT] NULL operators

					return expression.ToString();
				}
				return null;
			}		
		}
	
		/// <summary>
		/// Gets a value indicating whether expression is complete.
		/// </summary>
		private bool IsExpressionComplete
		{
			get
			{
				return Field != null && 
					!string.IsNullOrEmpty(ComparisonOperator) &&
					(HasChoices || IsNullValue);
			}		
		}

		private Action removeAction;
		/// <summary>
		/// Gets or sets the action for removing this expression from the collection.
		/// </summary>
		public Action RemoveAction
		{
			get { return removeAction; }
			set
			{
				if (removeAction != value)
				{
					removeAction = value;
					RemoveExpressionChanged();
				}
			}

		}

		private Action editAction;
		/// <summary>
		/// Gets or sets the action for editing this expression.
		/// </summary>
		public Action EditAction
		{
			get { return editAction; }
			set
			{
				if (editAction != value)
				{
					editAction = value;
					EditChanged();
				}
			}
		}

		private Action saveAction;
		/// <summary>
		/// Gets or sets the action for saving changes to this expression
		/// </summary>
		public Action SaveAction
		{
			get { return saveAction; }
			set
			{
				if (saveAction != value)
				{
					saveAction = value;
					SaveChanged();
				}
			}
		}

		private Action cancelAction;
		/// <summary>
		/// Gets or sets the action for canceling edits to this expression.
		/// </summary>
		public Action CancelAction
		{
			get { return cancelAction; }
			set
			{
				if (cancelAction != value)
				{
					cancelAction = value;
					CancelChanged();
				}
			}
		}
								
		#region INotifyPropertyChanged

		#region Serializable Properties

		private bool isVisible;
		/// <summary>
		/// Gets or sets a value indicating whether this expression is visible in the tool.
		/// </summary>
		[DataMember]
		public bool IsVisible
		{
			get { return isVisible; }
			set
			{
				if (isVisible != value)
				{
					var oldValue = isVisible;
					isVisible = value;
					OnPropertyChanged("IsVisible");
				}
			}
		}

        private bool inMiddleOfGroup;
        /// <summary>
        /// Gets a value indicating whether the expression is in the middle of a group that includes other expressions.
        /// </summary>
        [DataMember]
        public bool InMiddleOfGroup
        {
            get { return inMiddleOfGroup; }
            set
            {
                if (inMiddleOfGroup != value)
                {
                    inMiddleOfGroup = value;
                    OnPropertyChanged("InMiddleOfGroup");
                }
            }
        }

        private bool firstInGroup;
        /// <summary>
        /// Gets a value indicating whether the expression is at the beginning of a group that includes other expressions.
        /// </summary>
        [DataMember]
        public bool FirstInGroup
        {
            get { return firstInGroup; }
            set
            {
                if (firstInGroup != value)
                {
                    firstInGroup = value;
                    OnPropertyChanged("FirstInGroup");
                }
            }
        }

        private bool lastInGroup;
        /// <summary>
        /// Gets a value indicating whether the expression is a the end of a group that includes other expressions.
        /// </summary>
        [DataMember]
        public bool LastInGroup
        {
            get { return lastInGroup; }
            set
            {
                if (lastInGroup != value)
                {
                    lastInGroup = value;
                    OnPropertyChanged("LastInGroup");
                }
            }
        }

		private string joinedBy;
		/// <summary>
		/// Gets or sets the logical operator used to join 
		/// this expression with its preceding expression in the collection.
		/// </summary>
		[DataMember]
		public string JoinedBy
		{
			get { return joinedBy; }
			set
			{
				if (joinedBy != value)
				{
                    if (!LOGICAL_OPERATORS.Contains(value))
                        return;

					joinedBy = LOGICAL_OPERATORS.First(op => op == value);
					OnPropertyChanged("JoinedBy");
					if (!string.IsNullOrEmpty(joinedBy))
					{
						if (joinedBy.Contains("IN") || joinedBy.Contains("NULL"))
							IsVisible = false;
					}
				}			
			}		
		}

		private int position = -1;
		/// <summary>
		/// Gets or sets its position in the collection.
		/// </summary>
		[DataMember]
		public int Position
		{
			get { return position; }
			set
			{
				if (position != value)
				{
					position = value;
					OnPropertyChanged("Position");
					OnPropertyChanged("IsFirst");
					OnPropertyChanged("JoinedBy");
				}
			}
		}

		private int groupID = -1;
		/// <summary>
		/// Gets or sets the ID that represents expression grouping.
		/// </summary>
		[DataMember]
		public int GroupID
		{
			get { return groupID; }
			set
			{
				if (groupID != value)
				{
					groupID = value;
					OnPropertyChanged("GroupID");
				}
			}	
		}

		private string expressionLabel;
		/// <summary>
		/// Gets or sets the label of the expression.
		/// By default, this is Field.Alias [comparison operator].
		/// </summary>
		[DataMember]
		public string ExpressionLabel
		{
			get { return expressionLabel; }
			set
			{
				if (expressionLabel != value)
				{
					expressionLabel = value;
					OnPropertyChanged("ExpressionLabel");
				}
			}
		}
		
		private ChoiceValues choiceValues;
		/// <summary>
		/// Gets or sets the choices, default and current value part of the expression.
		/// </summary>
		[DataMember]
		public ChoiceValues ChoiceValues
		{
			get { return choiceValues; }
			set
			{
				if (choiceValues != value)
				{
					if (choiceValues != null)
						choiceValues.PropertyChanged -= ChoiceValues_PropertyChanged;
					choiceValues = value;					
					if(choiceValues != null)
						choiceValues.PropertyChanged += ChoiceValues_PropertyChanged;
					OnPropertyChanged("ChoiceValues");
				}
			}
		}

		private string fieldName;
		/// <summary>
		/// Gets or sets the field name used in the query.
		/// </summary>
		[DataMember]
		public string FieldName
		{
			get { return fieldName; }
			set
			{
				if (fieldName != value)
				{
					fieldName = value;
					OnPropertyChanged("FieldName");
				}
			}
		}

        private string fieldAlias;
        /// <summary>
        /// Gets or sets the friendly name of the field used in the query.
        /// </summary>
        [DataMember]
        public string FieldAlias
        {
            get { return fieldAlias; }
            set
            {
                if (fieldAlias != value)
                {
                    fieldAlias = value;
                    OnPropertyChanged("FieldAlias");
                }
            }
        }

		private string comparisonOperator;
		/// <summary>
		/// Gets or sets the comparison operator 
		/// </summary>
		[DataMember]
		public string ComparisonOperator
		{
			get { return comparisonOperator; }
			set
			{
				if (comparisonOperator != value)
				{
                    comparisonOperator = value;					
					OnPropertyChanged("ComparisonOperator");
                    OnPropertyChanged("DefaultExpressionLabel");
					
                    if (!ignoreComparisonUpdate && !string.IsNullOrEmpty(comparisonOperator))
						UpdateValueUsage();

					SaveChanged();
				}
			}
		}
		
		private bool useDefaultValue;
		/// <summary>
		/// Gets or sets a value indicating whether default value is used.
		/// </summary>
		[DataMember]
		public bool UseDefaultValue
		{
			get 
			{
				if (useDefaultValue && HasDomain)
					return false;
				else if ((Field != null && Field.Type == Field.FieldType.Date)
					|| (OutField != null && OutField.Type == Field.FieldType.Date))
					return true;
				return useDefaultValue; 
			}
			set
			{
				if (useDefaultValue != value)
				{
					if (Field != null && Field.Domain != null)                    
						useDefaultValue = false;
					else
						useDefaultValue = value;
					if (ChoiceValues != null && ChoiceValues.DefaultValue != null)
						ChoiceValues.DefaultValue.IsEditing = useDefaultValue;

                    if (useDefaultValue && ChoiceValues != null)
                    {
                        ChoiceValues.SelectedChoice = null;
                        
                        if (ChoiceValues.Choices != null)
                            ChoiceValues.Choices.Clear();
                    }
					OnPropertyChanged("UseDefaultValue");
					AddChanged();					
				}
			}
		}
		
		private bool useMultipleValues;
		/// <summary>
		/// Gets or sets a value indicating whether multiple value is used.
		/// This property is true when expression is using IN operator.
		/// </summary>
		[DataMember]
		public bool UseMultipleValues
		{
			get { return useMultipleValues; }
			set
			{
				if (useMultipleValues != value)
				{
					useMultipleValues = value;
					OnPropertyChanged("UseMultipleValues");
				}
			}
		}

		private bool isNullValue;
		/// <summary>
		/// Gets or sets a value indicating whether null value is used.
		/// This property is true when expression is using IS [NOT] NULL operator. 
		/// </summary>
		[DataMember]
		public bool IsNullValue
		{
			get { return isNullValue; }
			set
			{
				if (isNullValue != value)
				{
					isNullValue = value;
					OnPropertyChanged("IsNullValue");
                    OnPropertyChanged("IsExpressionComplete");
                    OnPropertyChanged("DefaultExpressionLabel");
				}
			}
		}

		private OutField outField;
		/// <summary>
		/// Gets or sets the field information.
		/// </summary>
		[DataMember]
		public OutField OutField
		{
			get { return outField; }
			set
			{
				if (outField != value)
				{
					if (outField != null)
					{
						ChoiceValuesCached[outField] = new ChoiceOptions()
						{
							UseDefault = UseDefaultValue,
							ComparisonOperator = ComparisonOperator,
							ComparisonOperators = ComparisonOperators,
							ChoiceValues = ChoiceValues
						};
					}
					outField = value;
					OnPropertyChanged("OutField");
                    OnPropertyChanged("CanUseChoiceList");
				}
			}
		}

		#endregion

		#region Non-serializable Properties

		private Field field;
		/// <summary>
		/// Gets or sets the field to query against.
		/// </summary>
		public Field Field
		{
			get { return field; }
			set
			{
				if (field != value)
				{
                    field = value;
					OnPropertyChanged("Field");
					bool cached = false;
					if (!ignoreFieldUpdate)
					{
						OutField = new OutField(field);
						cached = UpdateChoiceValues();
					}

                    if (field != null)
                    {
                        FieldName = field.Name;
                        FieldAlias = !string.IsNullOrEmpty(field.Alias) ? field.Alias : field.Name;
                    }
					if (!cached)
					{
						// Updates operators based on field.
						UpdateOperators();
					}

                    // Raises property change for read-only properties 
                    // that are dependent on field.
                    OnPropertyChanged("HasDomain");
                    OnPropertyChanged("UseDefaultValue");
                    OnPropertyChanged("CanUseChoiceList");
                    OnPropertyChanged("DefaultExpressionLabel");

                    AddChanged();
					RemoveChanged();
					SaveChanged();
				}
			}
		}

		private bool isSelected;	
		/// <summary>
		/// Gets or sets a value indicating whether this expression is selected.
		/// </summary>
		public bool IsSelected
		{
			get { return isSelected; }
			set
			{
				if (isSelected != value)
				{
					isSelected = value;
					OnPropertyChanged("IsSelected");
				}
			}
		}

		private IEnumerable<string> comparisonOperators;
		/// <summary>
		/// Gets or sets the comparison operators
		/// </summary>
		public IEnumerable<string> ComparisonOperators
		{
			get { return comparisonOperators; }
			private set
			{
				if (comparisonOperators != value)
				{
					comparisonOperators = value;
					OnPropertyChanged("ComparisonOperators");

                    if (comparisonOperator == null && comparisonOperators.Count() > 0)
                        ComparisonOperator = comparisonOperators.ElementAt(0);
				}
			}
		}
	
		private IEnumerable<Field> fields;
		/// <summary>
		/// Gets or sets the fields for the given service.
		/// </summary>
		public IEnumerable<Field> Fields
		{
			get { return fields; }
			set
			{
				if (fields != value)
				{
					fields = value;
					OnPropertyChanged("Fields");
				}
			}
		}

		private string serviceUrl;
		/// <summary>
		/// Gets or sets the service URL where sample data is retrieved from.
		/// </summary>
		public string ServiceUrl
		{
			get { return serviceUrl; }
			set
			{
				if (serviceUrl != value)
				{
					serviceUrl = value;
					OnPropertyChanged("ServiceUrl");
				}
			}
		}

		private bool useProxy;
		/// <summary>
		/// Gets or sets a value indicating whether proxy is used in web requests.
		/// </summary>
		public bool UseProxy
		{
			get { return useProxy; }
			set
			{
				if (useProxy != value)
				{
					useProxy = value;
					OnPropertyChanged("UseProxy");
				}
			}
		}

        /// <summary>
        /// Gets whether a pick-list can be used to present values to the end-user to choose from
        /// </summary>
        public bool CanUseChoiceList
        {
            get
            {
                bool fieldIsDate = Field != null && Field.Type == Field.FieldType.Date;
                bool outFieldIsDate = OutField != null && OutField.Type == Field.FieldType.Date;
                return !fieldIsDate && !outFieldIsDate;
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

		private DelegateCommand selectToAddCommand;
		/// <summary>
		/// Gets the command responsible for marking SampleValues as selected.
		/// </summary>
		public ICommand SelectToAdd
		{
			get
			{
				if (selectToAddCommand == null)
				{
					selectToAddCommand = new DelegateCommand(OnSelectToAdd, CanSelectToAdd);
				}
				return selectToAddCommand;
			}
		}

		private bool CanSelectToAdd(object commandParameter)
		{
			return ChoiceValues != null && ChoiceValues.SampleValues != null 
				&& ChoiceValues.SampleValues.Any();
		}

		/// <summary>
		/// Updates SampleValues selected state based on items in the command parameter.
		/// and evaluates if Add command can execute.
		/// </summary>
		/// <param name="commandParameter">Items to select.</param>
		private void OnSelectToAdd(object commandParameter)
		{
			if (!CanSelectToAdd(commandParameter)) return;
			var selectedItems = commandParameter as IEnumerable<object>;
			if (selectedItems != null)
			{
				foreach (var item in ChoiceValues.SampleValues)
					item.IsSelected = false;
				foreach (var item in selectedItems)
				{
					var valueToAdd = ChoiceValues.SampleValues.FirstOrDefault(v => v != null && v.Equals(item as FieldValue));
					if (valueToAdd != null)
						valueToAdd.IsSelected = true;
				}
				AddSelectionChanged();
			}
		}

		private DelegateCommand selectToRemoveCommand;
		/// <summary>
		/// Gets the command responsible for marking Choices as selected.
		/// </summary>
		public ICommand SelectToRemove
		{
			get
			{
				if (selectToRemoveCommand == null)
				{
					selectToRemoveCommand = new DelegateCommand(OnSelectToRemove, CanSelectToRemove);
				}
				return selectToRemoveCommand;
			}
		}

		private bool CanSelectToRemove(object commandParameter)
		{
			return ChoiceValues != null && ChoiceValues.Choices.Any();
		}

		/// <summary>
		/// Updates Choices selected state based on items in the command parameter
		/// and evaluates if Remove command can execute.
		/// </summary>
		/// <param name="commandParameter">Items to select.</param>
		private void OnSelectToRemove(object commandParameter)
		{
			if (!CanSelectToRemove(commandParameter)) return;
			var selectedItems = commandParameter as IEnumerable<object>;
			if (selectedItems != null)
			{
				foreach (var item in ChoiceValues.Choices)
					item.IsSelected = false;
				foreach (var item in selectedItems)
				{
					var valueToRemove = ChoiceValues.Choices.FirstOrDefault(v => v == item as FieldValue);
					if (valueToRemove != null)
						valueToRemove.IsSelected = true;
				}
				RemoveChanged();
			}
		}

		private DelegateCommand addCommand;
		/// <summary>
		/// Gets the command responsible for adding entries from SampleValues to Choices.
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
			return !UseDefaultValue && Field != null && Field.Domain == null && Field.Type != Field.FieldType.OID && Field.Type != Field.FieldType.GlobalID	
				&& ChoiceValues != null && (ChoiceValues.SelectedChoice == null || (!ChoiceValues.SelectedChoice.IsEditing || ChoiceValues.SelectedChoice.ValidationException == null));
		}

		/// <summary>
		/// Adds new entry to Choices.
		/// </summary>
		/// <param name="commandParameter"></param>
		private void OnAdd(object commandParameter)
		{
			if (!CanAdd(commandParameter)) return;
			{
				var newFieldValue = new FieldValue(FieldValue.GetDefaultValue(Field.Type), OutField, false, true, true);
				var valueToAdd = ChoiceValues.Choices.FirstOrDefault(v => v != null && v.Equals(newFieldValue));
				while (valueToAdd != null)
				{
					newFieldValue.Value = FieldValue.IncrementValue(Field.Type, newFieldValue.Value);
					valueToAdd = ChoiceValues.Choices.FirstOrDefault(v => v != null && v.Equals(newFieldValue));
				}
				ChoiceValues.Choices.Add(newFieldValue);
				AddChanged();
			}
		}

		private DelegateCommand addSelectionCommand;
		/// <summary>
		/// Gets the command responsible for adding entries from SampleValues to Choices.
		/// </summary>
		public ICommand AddSelection
		{
			get
			{
				if (addSelectionCommand == null)
				{
					addSelectionCommand = new DelegateCommand(OnAddSelection, CanAddSelection);
				}
				return addSelectionCommand;
			}
		}

		private void AddSelectionChanged()
		{
			if (addSelectionCommand != null)
			{
				addSelectionCommand.RaiseCanExecuteChanged();
			}
		}

		private bool CanAddSelection(object commandParameter)
		{
			return ChoiceValues != null && ChoiceValues.SampleValues != null 
				&& ChoiceValues.SampleValues.Any(q => q.IsSelected);
		}

		/// <summary>
		/// Adds entries to Choices based on SampleValues selected items.
		/// </summary>
		/// <param name="commandParameter"></param>
		private void OnAddSelection(object commandParameter)
		{
			if (!CanAddSelection(commandParameter)) return;
			var selectedItems = ChoiceValues.SampleValues.Where(v => v.IsSelected);
			foreach (var item in selectedItems)
			{
				var newFieldValue = new FieldValue(item.Value, OutField, false, true);
				var valueToAdd = ChoiceValues.Choices.FirstOrDefault(v => v != null && v.Equals(newFieldValue));
				if (valueToAdd == null)
					ChoiceValues.Choices.Add(newFieldValue);
			}
		}

		private DelegateCommand cancelCommand;
		/// <summary>
		/// Gets the command responsible for canceling edit to expression.
		/// </summary>
		public ICommand Cancel
		{
			get
			{
				if (cancelCommand == null)
				{
					cancelCommand = new DelegateCommand(OnCancel, CanCancel);
				}
				return cancelCommand;
			}
		}

		private void CancelChanged()
		{
			if (cancelCommand != null)
			{
				cancelCommand.RaiseCanExecuteChanged();
			}
		}

		private bool CanCancel(object commandParameter)
		{
			return CancelAction != null;
		}

		/// <summary>
		/// Executes the cancel action when set.
		/// </summary>
		/// <param name="commandParameter"></param>
		private void OnCancel(object commandParameter)
		{
			if (!CanCancel(commandParameter)) return;
            ClearValidationExceptions();
			CancelAction();
		}

		private DelegateCommand removeExpressionCommand;
		/// <summary>
		///  Gets the command responsible for deleting this expression from
		///  the collection of expressions.
		/// </summary>
		public ICommand RemoveExpression
		{
			get
			{
				if (removeExpressionCommand == null)
				{
					removeExpressionCommand = new DelegateCommand(OnRemoveExpression, CanRemoveExpression);
				}
				return removeExpressionCommand;
			}
		}

		private void RemoveExpressionChanged()
		{
			if (removeExpressionCommand != null)
			{
				removeExpressionCommand.RaiseCanExecuteChanged();
			}
		}

		private bool CanRemoveExpression(object commandParameter)
		{
			return RemoveAction != null;
		}

		/// <summary>
		/// Executes the remove action when set.
		/// </summary>
		/// <param name="commandParameter"></param>
		private void OnRemoveExpression(object commandParameter)
		{
			if (!CanRemoveExpression(commandParameter)) return;
			RemoveAction();
		}

		private DelegateCommand removeCommand;
		/// <summary>
		/// Gets the command responsible for removing the selected Choices.
		/// </summary>
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
			return ChoiceValues != null && ChoiceValues.Choices != null && ChoiceValues.Choices.Any(v => v.IsSelected);
		}

		/// <summary>
		/// Removes selected Choices from its collection.
		/// </summary>
		/// <param name="commandParameter"></param>
		private void OnRemove(object commandParameter)
		{
			if (!CanRemove(commandParameter)) return;
			var selectedItems = ChoiceValues.Choices.Where(v => v.IsSelected).ToList();
			foreach (var item in selectedItems)
			{
				if (ChoiceValues.Choices.Contains(item))
					ChoiceValues.Choices.Remove(item);
			}
			RemoveChanged();
		}

		private DelegateCommand saveCommand;
		/// <summary>
		/// Gets the command responsible for saving the edits made to the expression.
		/// </summary>
		public ICommand Save
		{
			get
			{
				if (saveCommand == null)
				{
					saveCommand = new DelegateCommand(OnSave, CanSave);
				}
				return saveCommand;
			}
		}

		private void SaveChanged()
		{
			if (saveCommand != null)
			{
				saveCommand.RaiseCanExecuteChanged();
			}
		}

		private bool CanSave(object commandParameter)
		{			
			return SaveAction != null && IsExpressionComplete;
		}

		/// <summary>
		/// Executes the save action when set.
		/// </summary>
		/// <param name="commandParameter"></param>
		private void OnSave(object commandParameter)
		{
			if (!CanSave(commandParameter)) return;
			SaveAction();
		}

		private DelegateCommand editCommand;
		/// <summary>
		/// Gets the command responsible for enabling edit on expression.
		/// </summary>
		public ICommand Edit
		{
			get
			{
				if (editCommand == null)
				{
					editCommand = new DelegateCommand(OnEdit, CanEdit);
				}
				return editCommand;
			}
		}

		private bool CanEdit(object commandParameter)
		{
			return EditAction != null;
		}

		private void EditChanged()
		{
			if (editCommand != null)
			{
				editCommand.RaiseCanExecuteChanged();
			}
		}
		/// <summary>
		/// Executes the EditAction when set.
		/// </summary>
		/// <param name="commandParameter"></param>
		private void OnEdit(object commandParameter)
		{
			if (!CanEdit(commandParameter)) return;
			EditAction();
		}
		
		#endregion
		
		#region Utility Methods

        internal void ClearValidationExceptions()
        {
            if (this.ChoiceValues != null)
            {
                if (this.ChoiceValues.Choices != null)
                {
                    foreach (FieldValue f in this.ChoiceValues.Choices)
                    {
                        f.ValidationException = null;
                        f.ValidationEnabled = false;
                    }
                }

                if (this.ChoiceValues.DefaultValue != null)
                {
                    this.ChoiceValues.DefaultValue.ValidationException = null;
                    this.ChoiceValues.DefaultValue.ValidationEnabled = false;
                }

                if (this.ChoiceValues.CurrentValue != null)
                {
                    this.ChoiceValues.CurrentValue.ValidationException = null;
                    this.ChoiceValues.CurrentValue.ValidationEnabled = false;
                }
            }
        }

		/// <summary>
		/// Updates serializable properties with values from another ExpressionViewModel.
		/// </summary>
		/// <param name="source">The source ExpressionViewModel</param>
		private void CopyAllSettings(ExpressionViewModel source)
		{
			if (source == null) return;
			IsVisible = source.IsVisible;
			ServiceUrl = source.ServiceUrl;						
			UseProxy = source.UseProxy;
			Fields = source.Fields;
			OutField = source.OutField;
			ExpressionLabel = source.ExpressionLabel;
			if (OutField != null && Fields != null)
			{
				ignoreFieldUpdate = true;
				Field = Fields.FirstOrDefault(f => f.Name == OutField.Name);
				ignoreFieldUpdate = false;
			}
            Position = source.Position;
			JoinedBy = source.JoinedBy;
			ignoreComparisonUpdate = true;
			ComparisonOperator = source.ComparisonOperator;
			ignoreComparisonUpdate = false;
			ChoiceValues = source.ChoiceValues != null ? new ChoiceValues(source.ChoiceValues) : null;
			UseDefaultValue = source.UseDefaultValue;
			IsNullValue = source.IsNullValue;
			UseMultipleValues = source.UseMultipleValues;
            GroupID = source.GroupID;
            FieldName = source.FieldName;
            FieldAlias = source.FieldAlias;
            FirstInGroup = source.firstInGroup;
            InMiddleOfGroup = source.InMiddleOfGroup;
            LastInGroup = source.LastInGroup;
		}

		/// <summary>
		/// Updates ComparisonOperators based on field data type.
		/// </summary>
		private void UpdateOperators()
		{
			List<string> allowedOperators = new List<string>();
			if (Field != null)
			{
				allowedOperators.AddRange(GENERIC_OPERATORS);
				if (Field.Nullable)
					allowedOperators.AddRange(NULLABLE_OPERATORS);
				if (Field.Type == ESRI.ArcGIS.Client.Field.FieldType.String)
					allowedOperators.AddRange(STRING_ONLY_OPERATORS);
			}
			ComparisonOperators = allowedOperators;
		}

		/// <summary>
		/// Updates IsNull and UseMultiple based on selected comparison operator. 
		/// </summary>
		private void UpdateValueUsage()
		{
			UseMultipleValues = false;
			IsNullValue = false;
			if (ComparisonOperator.Contains("IN"))
				UseMultipleValues = true;
			else if (ComparisonOperator.Contains("NULL"))
				IsNullValue = true;
			if (UseMultipleValues || IsNullValue)
				IsVisible = false;
		}
		
		/// <summary>
		/// Initializes ChoiceValues.SampleValues by querying the service for the
		/// current values for the selected field.
		/// </summary>
		private void InitializeWithServiceData()
		{
			if (!string.IsNullOrEmpty(ServiceUrl))
			{
				var url = !ServiceUrl.StartsWith("http") ? string.Format("http://{0}", ServiceUrl) : ServiceUrl;
				var queryTask = new QueryTask(url) { DisableClientCaching = true };
				if (UseProxy && !string.IsNullOrEmpty(ProxyUrl))
					queryTask.ProxyURL = ProxyUrl;
				EventHandler<QueryEventArgs> completed = null;
				EventHandler<TaskFailedEventArgs> failed = null;
				failed = (a, b) =>
				{
					queryTask.Failed -= failed;
					queryTask.ExecuteCompleted -= completed;
				};
				completed = (a, b) =>
				{
					var fieldName = b.UserState as string;
					queryTask.Failed -= failed;
					queryTask.ExecuteCompleted -= completed;
					if (!string.IsNullOrEmpty(fieldName) && b.FeatureSet != null && b.FeatureSet.Features != null)
					{
						ChoiceValues.SampleValues = new List<FieldValue>((from g in b.FeatureSet.Features
															 where g.Attributes.ContainsKey(fieldName) &&
															 (g.Attributes[fieldName] != null &&															 
															 (!(g.Attributes[fieldName] is string) || ((string)g.Attributes[fieldName]).Trim().Length > 0))
															 select new FieldValue(g.Attributes[fieldName], OutField)).OrderBy(g => g.Value).Distinct(new FieldValueEqualityComparer()));
					}
				};
				queryTask.Failed += failed;
				queryTask.ExecuteCompleted += completed;
				var query = new Query() { ReturnGeometry = false, Where = "1=1" };
				query.OutFields.Add(Field.Name);
				queryTask.ExecuteAsync(query, Field.Name);
			}
		}

		/// <summary>
		/// Initializes ChoiceValues.SampleValues with its Field.Domain
		/// </summary>
		private void InitializeWithDomain()
		{
			if (ChoiceValues == null) return;
			if (Field.Domain is CodedValueDomain)
			{
				var cvd = Field.Domain as CodedValueDomain;
				ChoiceValues.SampleValues = new List<FieldValue>(from c in cvd.CodedValues
													select new FieldValue(c, OutField));
			}
			else if (Field.Domain is RangeDomain<int>)
			{
				var rd = Field.Domain as RangeDomain<int>;
				ChoiceValues.SampleValues = new List<FieldValue>(GetRangeValues(rd.MinimumValue, rd.MaximumValue));
			}
			else if (Field.Domain is RangeDomain<short>)
			{
				var rd = Field.Domain as RangeDomain<short>;
				ChoiceValues.SampleValues = new List<FieldValue>(GetRangeValues(rd.MinimumValue, rd.MaximumValue));
			}
			else if (Field.Domain is RangeDomain<double>)
			{
				var rd = Field.Domain as RangeDomain<double>;
				ChoiceValues.SampleValues = new List<FieldValue>(GetRangeValues(rd.MinimumValue, rd.MaximumValue));
			}
			else if (Field.Domain is RangeDomain<Single>)
			{
				var rd = Field.Domain as RangeDomain<Single>;
				ChoiceValues.SampleValues = new List<FieldValue>(GetRangeValues(rd.MinimumValue, rd.MaximumValue));
			}
		}

		/// <summary>
		/// Updates ChoiceValues from cache, service or domain information.
		/// </summary>
		/// <returns>true if cached, otherwise false</returns>
		private bool UpdateChoiceValues()
		{
			var cached = ChoiceValuesCached.ContainsKey(OutField);
			if (cached)
			{
				var choiceOptions = ChoiceValuesCached[OutField];
				if (choiceOptions != null)
				{
					UseDefaultValue = choiceOptions.UseDefault;
					ComparisonOperator = choiceOptions.ComparisonOperator;
					ComparisonOperators = choiceOptions.ComparisonOperators;
					ChoiceValues = choiceOptions.ChoiceValues;
				}
			}
			else
			{
				ChoiceValues = new ChoiceValues(OutField);
				if (UseDefaultValue && ChoiceValues.DefaultValue != null)
					ChoiceValues.DefaultValue.IsEditing = true;
				if (Field.Domain == null && Field.Type != Field.FieldType.Date)
					InitializeWithServiceData();
				else if(Field.Domain != null)
					InitializeWithDomain();
			}
			return cached;
		}

		/// <summary>
		/// Enumerates possible values based on its Minimum and Maximum values.
		/// </summary>
		private IEnumerable<FieldValue> GetRangeValues(int min, int max)
		{
			for (var i = min; i <= max; i++)
				yield return new FieldValue(i, OutField);
		}

		/// <summary>
		/// Enumerates possible values based on its Minimum and Maximum values.
		/// </summary>
		private IEnumerable<FieldValue> GetRangeValues(short min, short max)
		{
			for (var i = min; i <= max; i++)
				yield return new FieldValue(i, OutField);
		}

		/// <summary>
		/// Enumerates possible values based on its Minimum and Maximum values.
		/// </summary>
		private IEnumerable<FieldValue> GetRangeValues(double min, double max)
		{
			for (var i = min; i <= max; i++)
				yield return new FieldValue(i, OutField);
		}

		/// <summary>
		/// Enumerates possible values based on its Minimum and Maximum values.
		/// </summary>
		private IEnumerable<FieldValue> GetRangeValues(Single min, Single max)
		{
			for (var i = min; i <= max; i++)
				yield return new FieldValue(i, OutField);
		}

		#endregion

		#region Event Handling

		private void ChoiceValues_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "SampleValues")
				AddSelectionChanged();
			else if (e.PropertyName == "SelectedChoice")
				AddChanged();
			SaveChanged();
			OnPropertyChanged("IsExpressionComplete");
            OnPropertyChanged("HasValueSet");
		}

		#endregion
	}
}
