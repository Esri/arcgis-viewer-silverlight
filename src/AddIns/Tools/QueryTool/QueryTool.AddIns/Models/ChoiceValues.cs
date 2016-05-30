/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.FeatureService;

namespace QueryTool.AddIns
{
	/// <summary>
	/// This class represents the choices, default and current value
	/// associated with a given field.
	/// </summary>
	[DataContract]
	public sealed class ChoiceValues : INotifyPropertyChanged
	{
		/// <summary>
		/// Constructor for ChoiceValues object.
		/// </summary>
		/// <param name="outField">field information used for validation.</param>
		public ChoiceValues(OutField outField)
		{
			OutField = outField;
			Choices = new ObservableCollection<FieldValue>();
			DefaultValue = new FieldValue(null, OutField, true);
			CurrentValue = new FieldValue(null, OutField);
		}

		/// <summary>
		/// Copy constructor for the ChoiceValues object.
		/// </summary>
		/// <param name="source"></param>
		public ChoiceValues(ChoiceValues source)
		{
			CopyAllSettings(source);
		}

		/// <summary>
		/// Updates properties of current object with properties of source object.
		/// </summary>
		/// <param name="source"></param>
		public void ApplyChanges(ChoiceValues source)
		{
			CopyAllSettings(source);
		}

		/// <summary>
		/// Updates properties of current object with properties of source object.
		/// </summary>
		/// <param name="source"></param>
		/// <summary>
		/// Gets comma-delimited string format of Choices.
		/// This property is used when query uses IN operator.
		/// </summary>
		public string ChoicesAsString
		{
			get
			{
				if (OutField != null && Choices != null)
				{
					var formattedValues = string.Join(" , ",
											from item in Choices
											select (OutField.Type == Field.FieldType.String ||
											OutField.Type == Field.FieldType.Date ||
											OutField.Type == Field.FieldType.GUID ||
											OutField.Type == Field.FieldType.XML) ?
											string.Format("'{0}'", item.Value) : item.Value);
					return string.Format(" ({0})", formattedValues);
				}
				return null;
			}
		}

		/// <summary>
		/// Resets ChoiceValues to default settings.
		/// </summary>
		public void Reset(bool useDefaultValue)
		{
			SelectedChoice = Choices != null && Choices.Any() ? Choices.FirstOrDefault() : null;
			CurrentValue = useDefaultValue ? new FieldValue(DefaultValue) : new FieldValue(SelectedChoice);
		}

		/// <summary>
		/// Gets a value indicating whether default or choices is pending validation.
		/// </summary>
		public bool IsValidating
		{
			get
			{
				return (DefaultValue != null && DefaultValue.ValidationException != null) ||
					(Choices != null && Choices.Any(c => c.ValidationException != null));
			}
		}

		#region INotifyPropertyChanged

		#region Serializable Properties

		private OutField outField;
		/// <summary>
		/// Gets or sets the field information for validation.
		/// </summary>
		[DataMember]
		public OutField OutField
		{
			get { return outField; }
			set
			{
				if (outField != value)
				{
					outField = value;
					OnPropertyChanged("OutField");					
				}
			}
		}

		private FieldValue defaultValue;
		/// <summary>
		/// Gets or sets the default value for field.
		/// </summary>
		[DataMember]
		public FieldValue DefaultValue
		{
			get { return defaultValue; }
			set
			{
				if (defaultValue != value)
				{
					if (defaultValue != null)
						defaultValue.PropertyChanged -= FieldValue_PropertyChanged;
					defaultValue = value;
					if(defaultValue != null)
						defaultValue.PropertyChanged += FieldValue_PropertyChanged;
					OnPropertyChanged("DefaultValue");
					OnPropertyChanged("CurrentValue");
				}
			}
		}

		private List<FieldValue> sampleValues;
		/// <summary>
		/// Gets or sets the sample data for the field.
		/// Field with domain gets sample data from its domain information;
		/// otherwise, the service is queried to return current values for the field.
		/// </summary>
		[DataMember]
		public List<FieldValue> SampleValues
		{
			get { return sampleValues; }
			set
			{
				if (sampleValues != value)
				{
					sampleValues = value;
					OnPropertyChanged("SampleValues");
				}
			}
		}
		
		private ObservableCollection<FieldValue> choices;
		/// <summary>
		/// Gets or sets the choices that where reated or gathered from sample data
		/// </summary>
		[DataMember]
		public ObservableCollection<FieldValue> Choices
		{
			get { return choices; }
			set
			{
				if (choices != value)
				{
					if (choices != null)
					{
						foreach (var item in choices)
							item.PropertyChanged -= FieldValue_PropertyChanged;
						choices.CollectionChanged -= Choices_CollectionChanged;
					}
					choices = value;
					OnPropertyChanged("Choices");
					if (choices != null)
					{
						foreach (var item in choices)
							item.PropertyChanged += FieldValue_PropertyChanged;
						choices.CollectionChanged += Choices_CollectionChanged;
					}
				}
			}
		}

		#endregion

		#region Non-serializableProperties

		private FieldValue currentValue;
		/// <summary>
		/// Gets or sets the current value for field.
		/// If it is not set, it takes the default value.
		/// </summary>
		public FieldValue CurrentValue
		{
			get 
			{
				return currentValue; 
			}
			set
			{
				if (currentValue != value)
				{
					if (currentValue != null)
						currentValue.PropertyChanged -= FieldValue_PropertyChanged;
					currentValue = value;
					if (currentValue != null)
						currentValue.PropertyChanged += FieldValue_PropertyChanged;				
					OnPropertyChanged("CurrentValue");
				}
			}
		}

		private FieldValue selectedChoice;
		/// <summary>
		/// Gets or sets the selected value from choices.
		/// This property is used to set current value.
		/// </summary>
		public FieldValue SelectedChoice
		{
			get { return selectedChoice; }
			set
			{
				if (selectedChoice != value)
				{
					selectedChoice = value == null && Choices != null ? Choices.FirstOrDefault() : value;				
					OnPropertyChanged("SelectedChoice");
					OnPropertyChanged("SelectedChoice.Value");
					OnPropertyChanged("CurrentValue");
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

		#region Event Handling

		private void Choices_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (var item in e.OldItems)
				{
					var fieldValue = item as FieldValue;
					if (SelectedChoice != null && SelectedChoice == fieldValue)
						SelectedChoice = null;
					fieldValue.Choices = null;
					fieldValue.PropertyChanged -= FieldValue_PropertyChanged;
				}
			}
			if (e.NewItems != null)
			{
				foreach (var item in e.NewItems)
				{
					var fieldValue = item as FieldValue;
					fieldValue.Choices = sender as ObservableCollection<FieldValue>;
					fieldValue.PropertyChanged += FieldValue_PropertyChanged;
				}
			}
			OnPropertyChanged("ChoicesAsString");
		}

		private void FieldValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Value")
			{
				OnPropertyChanged("ChoicesAsString");
			}
			else if (e.PropertyName == "IsEditing")
			{
				var fieldValue = sender as FieldValue;
				if (fieldValue.IsEditing)
				{
					if (fieldValue.ValidationException == null)
						SelectedChoice = fieldValue;
				}
				OnPropertyChanged("ChoicesAsString");
			}
		}

		#endregion

		#region Utility Methods

		/// <summary>
		/// Updates serializable properties with values from another ChoiceValues.
		/// </summary>
		/// <param name="source">The source ChoiceValues</param>
		private void CopyAllSettings(ChoiceValues source)
		{
			OutField = new OutField(source.OutField);
			DefaultValue = new FieldValue(source.DefaultValue);
			SampleValues = source.SampleValues != null ? new List<FieldValue>(from v in source.SampleValues select new FieldValue(v)) 
				: new List<FieldValue>();
			Choices = source.Choices != null ? new ObservableCollection<FieldValue>(from c in source.Choices select new FieldValue(c)) : 
				new ObservableCollection<FieldValue>();			
		}
		#endregion
	}
}
