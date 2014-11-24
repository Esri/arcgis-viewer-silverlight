/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
	/// <summary>
	/// Implements the dialog for defining a new Query from a SubLayer or modifying
	/// an existing query.
	/// </summary>
	public partial class DefineQueryControl : Control
	{
		List<FieldWrapper> _visibleFields; // tracks which fields are visible in the query

		string _dataSource;

        public DefineQueryControl()
        {
            this.DefaultStyleKey = typeof(DefineQueryControl);
            OperatorClickCommand = new ESRI.ArcGIS.Mapping.Controls.DelegateCommand(operatorClick, canOperatorClick);
        }
		/// <summary>
		/// Modify an existing Query.
		/// </summary>
        public DefineQueryControl(Query query, SubLayerDescription description) : this()
		{
            QueryProperties = new QueryWindowProperties() { Query = query, SubLayerDescription = description };
		}

		#region Properties
		#region QueryProperties
		public class QueryWindowProperties
		{
			public SubLayerDescription SubLayerDescription { get; set; }
			public Query Query { get; set; }
		}

		public QueryWindowProperties QueryProperties
		{
			get { return (QueryWindowProperties)GetValue(QueryWindowPropsProperty); }
			set { SetValue(QueryWindowPropsProperty, value); }
		}

		// Using a DependencyProperty as the backing store for QueryWindowProps.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty QueryWindowPropsProperty =
				DependencyProperty.Register("QueryWindowProps", typeof(QueryWindowProperties), typeof(DefineQueryControl), new PropertyMetadata(OnQueryWindowPropsChange));

		static void OnQueryWindowPropsChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            DefineQueryControl o = d as DefineQueryControl;
			QueryWindowProperties props = e.NewValue as QueryWindowProperties;
            o.setQueryProps();
		}

        void setQueryProps()
        {
            if (FieldListBox != null && QueryProperties != null && WhereClauseTextBox != null)
            {
                FieldListBox.ItemsSource = QueryProperties.SubLayerDescription.Fields;
                WhereClauseTextBox.Text = (QueryProperties.Query == null || QueryProperties.Query.WhereClause == null) ? string.Empty : QueryProperties.Query.WhereClause;
                SetupVisibleFields();
                SetupDateFormat();
                SetupPreview();
                if (FieldListBox.Items.Count > 0)
                    FieldListBox.ScrollIntoView(FieldListBox.Items[0]);
            }
        }
		#endregion

        #region FeatureLayer
        public FeatureLayer FeatureLayer
        {
            get { return (FeatureLayer)GetValue(FeatureLayerProperty); }
            set { SetValue(FeatureLayerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowFieldsTab.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FeatureLayerProperty =
                DependencyProperty.Register("FeatureLayer", typeof(FeatureLayer), typeof(DefineQueryControl), new PropertyMetadata(OnFeatureLayerChange));

        static void OnFeatureLayerChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DefineQueryControl o = d as DefineQueryControl;
            FeatureLayer layer = e.NewValue as FeatureLayer;
            if (layer != null && !(string.IsNullOrWhiteSpace(layer.Url)))
            {
                ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.SubLayerDescription.GetServiceInfoAsync(layer.Url, (sender2, e2) =>
                {
                    if (e2.Description != null && e2.Description.Fields != null && e2.Description.Fields.Length > 0)
                    {
                        ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Query query = new ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Query(new ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.QueryDescription() { WhereClause = layer.Where });
                        o.QueryProperties = new ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.DefineQueryControl.QueryWindowProperties() { Query = query, SubLayerDescription = e2.Description };
                    }
                }, layer.ProxyUrl);
            }
        }
        #endregion
		#endregion

        #region Helpers
        /// <summary>
		/// Initializes the Data tab with a sample view of attributes of the layer.
		/// </summary>
		private void SetupPreview()
		{
            if (FeatureLayer == null)
                return;
			QueryTask qT = new QueryTask(FeatureLayer.Url);
            qT.ProxyURL = FeatureLayer.ProxyUrl;

			ESRI.ArcGIS.Client.Tasks.Query q = new ESRI.ArcGIS.Client.Tasks.Query();
			foreach (var field in QueryProperties.SubLayerDescription.Fields)
				q.OutFields.Add(field.Name);

			q.Where = "1=1"; //query for features in the layer, todo: any way to limit the number of features returned?
			q.ReturnGeometry = false;

			EventHandler<ESRI.ArcGIS.Client.Tasks.QueryEventArgs> completedHandler = null;
			EventHandler<TaskFailedEventArgs> failedHandler = null;

			completedHandler = (object sender, ESRI.ArcGIS.Client.Tasks.QueryEventArgs e) =>
			{
				PreviewDataGrid.Columns.Clear();
				if (e.FeatureSet == null || e.FeatureSet.Features == null || e.FeatureSet.Features.Count == 0)
					return;

				//set up the columns of the table
				//if a field does not exist in the layer description don't add it to the table
				//this is the case for the geometry field
				//
				List<string> columns = new List<string>();
				foreach (string key in e.FeatureSet.Features[0].Attributes.Keys)
					if (GetField(key) != null)
						columns.Add(key);

				int count = 0;
				Table table = new Table(columns.ToArray());
				foreach (Graphic feature in e.FeatureSet)
				{
					List<object> cells = new List<object>();
					foreach (var attribute in feature.Attributes)
					{
						SubLayerField field = GetField(attribute.Key);
						if (field == null)
							continue;

						if (field.AttributeDomain != null && field.AttributeDomain.Type == "codedValue" && attribute.Value != null)
							cells.Add(field.AttributeDomain[attribute.Value]);
						else
						{
							//if the field value is a date in milliseconds format it to a user readable date
							//
							string formattedDateString = null;
							if (field.Type == "esriFieldTypeDate" && attribute.Value != null && attribute.Value.ToString().TryParseToDateString(out formattedDateString))
								cells.Add(formattedDateString);
							else
								cells.Add(attribute.Value);
						}
					}

					table.Rows.Add(new Row(cells.ToArray()));

					count++;
					if (count == 25)
						break;
				}

				//initialize the DataGrid columns
				//
				foreach (DataGridTextColumn column in table.Columns)
				{
					column.CanUserSort = true;
					PreviewDataGrid.Columns.Add(column);
				}

				//setting the table as the DataContext of the DataGrid initializes the rows via binding
				//
				PreviewDataGrid.DataContext = table;
				PreviewDataGrid.UpdateLayout();
				if (table.Rows.Count > 0 && PreviewDataGrid.Columns.Count > 0)
					PreviewDataGrid.ScrollIntoView(table.Rows[0].Cells[0], PreviewDataGrid.Columns[0]);
				DataPreviewProgressIndicator.Visibility = Visibility.Collapsed;
			};

			failedHandler = ((object sender, TaskFailedEventArgs e) =>
				{
					FailedPreviewTextBlock.Visibility = Visibility.Visible;
					DataPreviewProgressIndicator.Visibility = Visibility.Collapsed;
				});

			qT.ExecuteCompleted += completedHandler;
			qT.Failed += failedHandler;

			//execute the query
			//
			qT.ExecuteAsync(q);
		}

		SubLayerField GetField(string fieldName)
		{
			foreach (SubLayerField field in QueryProperties.SubLayerDescription.Fields)
				if (field.Name == fieldName)
					return field;

			return null;
		}

		/// <summary>
		/// If the feature layer contains fields of type Date, runs a sequence of test queries to determine
		/// the date format required by the underlying data source.
		/// </summary>
		private void SetupDateFormat()
		{
			if (QueryProperties.SubLayerDescription.Fields == null)
				return;

			SubLayerField dateField = null;
			foreach (SubLayerField field in QueryProperties.SubLayerDescription.Fields)
				if (field.Type == "esriFieldTypeDate")
				{
					dateField = field;
					break;
				}

			if (dateField == null)
				return;

			//get the data source type from the QueryDateHelper by running a series of test queries against the dateField
			//
			QueryDateHelper.GetDataSource(QueryProperties.SubLayerDescription.Url, QueryProperties.SubLayerDescription.RequiresProxy, dateField, (object sender, DataSourceEventArgs e) =>
			{
				//query succeeded -> cache the datasource so the correctly formatted dates can be 
				//generated when the expression is created
				//
				if (_dataSource == null)
					_dataSource = e.DataSource;
				else
				{
					//if data source has already been set check if this one has higher priority
					//if yes - replace
					//
					List<string> supportedDS = new List<string>(QueryDateHelper.SupportedDataSources);
					if (supportedDS.IndexOf(e.DataSource) < supportedDS.IndexOf(_dataSource))
						_dataSource = e.DataSource;
				}

				ExpressionDatePickerButton.IsEnabled = true;
			});
		}

		/// <summary>
		/// Initializes the VisibleFieldsListBox.
		/// </summary>
		private void SetupVisibleFields()
		{
			_visibleFields = new List<FieldWrapper>();

			foreach (SubLayerField field in QueryProperties.SubLayerDescription.Fields)
				_visibleFields.Add(new FieldWrapper() { Field = field, Visible = true });

			if (QueryProperties.Query != null && QueryProperties.Query.QueryDescription.VisibleFields != null)
				foreach (FieldWrapper fw in _visibleFields)
					fw.Visible = QueryProperties.Query.QueryDescription.VisibleFields.Contains(fw.Field.Name);
		}

		public class FieldWrapper
		{
			public bool Visible { get; set; }
			public SubLayerField Field { get; set; }
		}

		void raiseQueryDefined()
		{
			if (QueryDefined != null)
				QueryDefined(this, new QueryMapItemEventArgs() { Query = QueryProperties.Query });
		}

		public event EventHandler<QueryMapItemEventArgs> QueryDefined;

		/// <summary>
		/// Applies the changes to the query and closes the dialog.
		/// </summary>
		public void UpdateQuery()
		{
			List<string> fields = new List<string>();
			foreach (FieldWrapper fw in _visibleFields)
				if (fw.Visible)
					fields.Add(fw.Field.Name);

			Query query = QueryProperties.Query;
			if (query == null) // add a new query
			{
				query = new Query(new QueryDescription()
				{
					Name = "New Query",
					Url = QueryProperties.SubLayerDescription.Url,
					RequiresProxy = QueryProperties.SubLayerDescription.RequiresProxy
				});
			}

			query.WhereClause = WhereClauseTextBox.Text;
			query.VisibleFields = fields;
			QueryProperties.Query = query;
            
            if (FeatureLayer != null)
            {
                SetFeatureQuery(query);
            }
            raiseQueryDefined();
		}

        private void SetFeatureQuery(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Query query)
        {
            if (FeatureLayer == null)
                return;

            FeatureLayer.Where = query.WhereClause;
            FeatureLayer.Update();
        }

		/// <summary>
		/// Helper method to add a token to the where clause including any necessary
		/// spacing.
		/// </summary>
		void AddText(string text)
		{
            if (WhereClauseTextBox.Text.Length > 0 && !WhereClauseTextBox.Text.EndsWith(" ", StringComparison.Ordinal))
				WhereClauseTextBox.Text += " ";

			WhereClauseTextBox.Text += text + " ";

			WhereClauseTextBox.SelectionStart = WhereClauseTextBox.Text.Length;
			WhereClauseTextBox.Focus();
		}
        #endregion

        ListBox FieldListBox;
        DataGrid PreviewDataGrid;
        TextBox WhereClauseTextBox;
        Button OKButton;
        DatePicker ExpressionDatePicker;
        ProgressIndicator DataPreviewProgressIndicator;
        TextBlock FailedPreviewTextBlock;
        Button ExpressionDatePickerButton;
        public override void OnApplyTemplate()
        {
            if (FieldListBox != null)
                FieldListBox.SelectionChanged -= FieldListBox_SelectionChanged;
            if (WhereClauseTextBox != null)
                WhereClauseTextBox.KeyDown -= WhereClauseTextBox_KeyDown;
            if (OKButton != null)
                OKButton.Click -= OKButton_Click;
            if (ExpressionDatePicker != null)
                ExpressionDatePicker.SelectedDateChanged -= DatePicker_SelectedDateChanged;
            base.OnApplyTemplate();
            FieldListBox = GetTemplateChild("FieldListBox") as ListBox;
            WhereClauseTextBox = GetTemplateChild("WhereClauseTextBox") as TextBox;
            OKButton = GetTemplateChild("OKButton") as Button;
            ExpressionDatePicker = GetTemplateChild("ExpressionDatePicker") as DatePicker;
            PreviewDataGrid = GetTemplateChild("PreviewDataGrid") as DataGrid;
            DataPreviewProgressIndicator = GetTemplateChild("DataPreviewProgressIndicator") as ProgressIndicator;
            FailedPreviewTextBlock = GetTemplateChild("FailedPreviewTextBlock") as TextBlock;
            ExpressionDatePickerButton = GetTemplateChild("ExpressionDatePickerButton") as Button;
            if (FieldListBox != null)
                FieldListBox.SelectionChanged += FieldListBox_SelectionChanged;
            if (WhereClauseTextBox != null)
                WhereClauseTextBox.KeyDown += WhereClauseTextBox_KeyDown;
            if (OKButton != null)
                OKButton.Click += OKButton_Click;
            if (ExpressionDatePicker != null)
                ExpressionDatePicker.SelectedDateChanged += DatePicker_SelectedDateChanged;
            setQueryProps();
        }

        #region Event handlers
        /// <summary>
		/// Occurs when a field is selected. Adds the field to the where clause then
		/// clears the FieldListBox selection.
		/// </summary>
		internal void FieldListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SubLayerField field = FieldListBox.SelectedItem as SubLayerField;
			if (field != null)
				AddText(field.Name);

			FieldListBox.SelectedItem = null;
		}

		/// <summary>
		/// Occurs when a key is pressed in the WhereClauseTextBox.
		/// </summary>
		private void WhereClauseTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				e.Handled = true;
				UpdateQuery();
			}
		}

		internal void OKButton_Click(object sender, RoutedEventArgs e)
		{
			UpdateQuery();
		}

		private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			DatePicker dP = (DatePicker)sender;

			//format the selected date depending on the underlying data source
			//
			string date = QueryDateHelper.Format(dP.SelectedDate.Value, _dataSource);

			if (dP.Name == "ExpressionDatePicker")
				AddText(date);
        }
        #endregion


        #region OperatorClick Command
        private void operatorClick(object commandParameter)
        {
            AddText(commandParameter as string);
        }

        private bool canOperatorClick(object commandParameter)
        {
            return true;
        }



        public ICommand OperatorClickCommand
        {
            get { return (ICommand)GetValue(OperatorClickCommandProperty); }
            set { SetValue(OperatorClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OperatorClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OperatorClickCommandProperty =
            DependencyProperty.Register("OperatorClickCommand", typeof(ICommand), typeof(DefineQueryControl), null);

        
        //public ICommand OperatorClickCommand { get; set; }
        #endregion
    }
}

