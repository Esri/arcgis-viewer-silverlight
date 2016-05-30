/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.FeatureService;
using QueryTool.Resources;

namespace QueryTool.AddIns
{
	/// <summary>
	/// This class represents the value part or the right operand of a query.
	/// </summary>
	[DataContract]
	public sealed class FieldValue : Object, INotifyPropertyChanged
	{
		internal static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		
		// Fields for unique and non-empty validation.
		private bool validatesUniqueAndNonEmpty = false;
		internal ObservableCollection<FieldValue> Choices { get; set; }

		private object valueInCorrectType = null;
		/// <summary>
		/// Gets value in its correct data type.
		/// </summary>
		internal object ValueInCorrectType
		{
			get 
			{
				if (valueInCorrectType == null)
				{
					Exception ex = null;
					valueInCorrectType = FieldValue.ValidateValue(OutField, Value, out ex);
				}
				return valueInCorrectType;  
			}
			
		}
		
		/// <summary>
		/// Constructor for FieldValue object.
		/// </summary>
		/// <param name="value">actual value.</param>
		/// <param name="outField">field information used for validation.</param>
		/// <param name="allowNullOrEmpty">null or empty value allowed.</param>
		/// <param name="validatesUniqueAndNonEmpty">enforces fields to be unique and non-empty.</param>
		/// <param name="activateEditing">activates editing.</param>
		public FieldValue(object value, OutField outField, bool allowNullOrEmpty = false, bool validatesUniqueAndNonEmpty = false, bool activateEditing = false)
		{
			this.validatesUniqueAndNonEmpty = validatesUniqueAndNonEmpty;
			OutField = outField;
			Value = value;
			IsEditing = activateEditing;
		}

		/// <summary>
		/// Copy constructor for the FieldValue object.
		/// </summary>
		/// <param name="source"></param>
		public FieldValue(FieldValue source)
		{
			CopyAllSettings(source);
		}

		/// <summary>
		/// Updates properties of current object with properties of source object.
		/// </summary>
		/// <param name="source"></param>
		public void ApplyChanges(FieldValue source)
		{
			CopyAllSettings(source);
		}

		#region Overrides Equals && ToString Operator

		public override int GetHashCode()
		{
			if (this.Value == null) return 0;
			return this.Value.GetHashCode();
		}

		/// <summary>
		/// Overrides equals operator to use the same compare method
		/// found in FieldValueEqualityComparer.
		/// </summary>
		/// <param name="obj">right operand.</param>
		/// <returns>true if Value is equal to the parameter Value.</returns>
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is FieldValue))
				return false;
			return FieldValueEqualityComparer.EqualValue(this, (obj as FieldValue));
		}

		/// <summary>
		/// Returns FieldValue into string format with 
		/// its type and usage considered.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			if (Value != null)
			{
				if (OutField != null)
				{
					var objValue = Value;

					if (OutField.Domain != null)
					{
						var result = OutField.Domain.FirstOrDefault(c => FieldValueEqualityComparer.EqualObject(c.Value, Value));
						objValue = result.Key;
					}
					string strValue = Convert.ToString(objValue, CultureInfo.InvariantCulture);
					return (OutField.Type == Field.FieldType.String ||
							OutField.Type == Field.FieldType.GUID ||
							OutField.Type == Field.FieldType.XML ?
							string.Format("'{0}'", strValue) : string.Format("{0}", strValue));
				}
				return Convert.ToString(Value, CultureInfo.InvariantCulture);
			}
			return "NULL";
		}

		#endregion
				
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

		private object _value;
		/// <summary>
		/// Gets or sets the visible value of field.
		/// </summary>
		[DataMember]
		public object Value
		{	
			get 
			{
				if (_value != null && OutField != null && OutField.Domain != null)
				{
					if (_value is string)
					{
						foreach (var item in OutField.Domain)
							if (item.Value == _value as string)
								return item.Key;
					}
					else
					{
						var kvp = (KeyValuePair<object, string>)_value;
						return kvp.Value;
					}
				}		
				return _value; 
			}
			set
			{
				if (_value != value)
				{
					object temp = ValidateValue(value);
					_value = temp;
					// reset converted value.
					valueInCorrectType = null;
					OnPropertyChanged("Value");
				}
			}
		}
		
		#endregion

		#region Non-serializable Properties

		private Exception validationException;
		/// <summary>
		/// Gets or sets the exception from validation.
		/// </summary>
		public Exception ValidationException
		{
			get { return validationException; }
			set
			{
				if (validationException != value && ValidationEnabled)
				{
					validationException = value;
					OnPropertyChanged("ValidationException");
				}
			}
		}

        private bool validationEnabled = true;
        internal bool ValidationEnabled
        {
            get { return validationEnabled; }
            set
            {
                if (validationEnabled != value)
                    validationEnabled = value;
            }
        }

        public bool allowEmptyValues;
        public bool AllowEmptyValues
        {
            get { return allowEmptyValues; }
            set
            {
                if (allowEmptyValues != value)
                {
                    allowEmptyValues = value;

                    // Validate value to update ValidationException
                    if (this.Value != null)
                        ValidateValue(this.Value);

                    OnPropertyChanged("AllowEmptyValues");
                }
            }
        }

		private bool isSelected;
		/// <summary>
		/// Gets or sets a value indicating whether field value is selected.
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

		private bool isEditing;
		/// <summary>
		/// Gets or sets a value indicating whether field value is in edit mode.
		/// </summary>
		public bool IsEditing
		{
			get
			{
				if (isEditing)
				{
					return OutField != null && OutField.Domain == null &&
						OutField.Type != Field.FieldType.OID && OutField.Type != Field.FieldType.GlobalID						
						? true : false;
				}
				else
				{
					if (ValidationException == null)
						return false;
					else
						throw ValidationException;
				}
			}
			set
			{
				if (isEditing != value)
				{
					isEditing = value;
					OnPropertyChanged("IsEditing");
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

		#region Utility Methods

		/// <summary>
		/// Updates serializable properties with values from another FieldValue.
		/// </summary>
		/// <param name="source">The source FieldValue</param>
		private void CopyAllSettings(FieldValue source)
		{
			if (source == null) return;
			OutField = source.OutField;
			Value = source.Value;
		}

		/// <summary>
		/// Performs value validation.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private object ValidateValue(object value)
		{
            Exception ex = null;
            ValidationException = null;
			object temp = _value;
			// Performs type and length validation.							
			if (OutField != null)
			{
				if (OutField.Domain != null)
				{
					if (value is string)
					{
						foreach (var item in OutField.Domain)
						{
							if (item.Value == value as string)
							{
								temp = item;
								break;
							}
						}
					}
					else
						temp = value;
				}
				else
				{
                    temp = FieldValue.ValidateValue(OutField, value, out ex);
                    if (ex != null && (value == null || string.IsNullOrWhiteSpace(value.ToString())))
                    {
                        // Special handling of validation errors due to null or empty input - set value to null and 
                        // only set ValidationException if empty values are not allowed
                        temp = null;
                        if (!AllowEmptyValues)
                            ValidationException = new Exception(Strings.EmptyEntryError);
                    }
                    else if (ex != null)
                    {
                        ValidationException = new Exception(Strings.InvalidValue);
                    }
				}
			}

            if (ex == null)
            {
                if (Choices != null)
                {
                    // Checks for duplicate entries.
                    if (Choices.Any(c => c != this && FieldValueEqualityComparer.EqualObject(c.ValueInCorrectType, temp)))
                        ValidationException = new ArgumentException(Strings.DuplicateError);
                }
            }

			return temp;
		}

		/// <summary>
		/// Converts value to its correct data type while performing type and length validation.
		/// </summary>
		/// <param name="field">field information</param>
		/// <param name="value">value to validate.</param>
		/// <param name="exception">validation exception to be set.</param>
		/// <returns>value converted to its correct data type.</returns>
		private static object ValidateValue(OutField field, object value, out Exception exception)
		{
			object result = value;
			exception = null;
			try
			{
				switch (field.Type)
				{
					case Field.FieldType.Date:
						// Due to datetime fields format being dependent on the type of backend database
						// no datetime validation was put in place for this version so as not to be restrictive. 
						// It is assumed that users will know the correct datetime format when working against 
						// their own services. Sample servers often use "date 'yyyy-MM-dd'" or "date 'yyyy-MM-dd hh:mm:ss'".
						// (see: http://resources.arcgis.com/en/help/main/10.1/index.html#//00s500000033000000)
						break;
					case Field.FieldType.Double:
						if (value.GetType() != typeof(double))
						{
							result = Convert.ToDouble(value, CultureInfo.InvariantCulture);
						}
						break;
					case Field.FieldType.Single:
						if (value.GetType() != typeof(float))
						{
							result = Convert.ToSingle(value, CultureInfo.InvariantCulture);
						}
						break;
					case Field.FieldType.OID:
					case Field.FieldType.Integer:
						if (value.GetType() != typeof(int))
						{
							result = Convert.ToInt32(value, CultureInfo.InvariantCulture);
						}
						break;
					case Field.FieldType.SmallInteger:
						if (value.GetType() != typeof(short))
						{
							result = Convert.ToInt16(value, CultureInfo.InvariantCulture);
						}
						break;
					case Field.FieldType.GUID:
						if (value.GetType() != typeof(Guid))
						{
							Guid output;
							var valueStr = Convert.ToString(value, CultureInfo.InvariantCulture);
							if (!string.IsNullOrEmpty(valueStr))
							{
								if (Guid.TryParse(valueStr, out output))
									result = output;
							}
						}
						break;
				}
			}
			catch
			{
				exception = new ArgumentException(string.Format(Resources.Strings.DataTypeMismatch, value, field.Type));				
			}

			if (exception == null && field.Type != Field.FieldType.Date && field.Length > 0 && result is string && !((string)result).Contains(Resources.Strings.EnterValue))
			{
				if ((result as string).Length > field.Length)
					exception = new ArgumentOutOfRangeException(string.Format(Resources.Strings.StringExceededMaxLength, !string.IsNullOrEmpty(field.Alias) ? field.Alias : field.Name, field.Length));
			}
			return result;
		}

		/// <summary>
		/// Gets the default value of field based on type.
		/// </summary>
		/// <param name="fieldType">data type.</param>
		/// <returns>default value.</returns>
		public static object GetDefaultValue(Field.FieldType fieldType)
		{
			switch (fieldType)
			{
				case Field.FieldType.Double:
					return new double();
				case Field.FieldType.Single:
					return new float();
				case Field.FieldType.Integer:
					return new int();
				case Field.FieldType.SmallInteger:
					return new short();
				case Field.FieldType.String:
					return Resources.Strings.EnterValue;
				case Field.FieldType.GUID:
					return Guid.NewGuid();
				default:
					return null;
			}
		}

		/// <summary>
		/// Increments current value of field based on type.
		/// </summary>
		/// <param name="fieldType">data type.</param>
		/// <param name="currentValue">current value to increment from.</param>
		/// <returns>incremented value.</returns>
		public static object IncrementValue(Field.FieldType fieldType, object currentValue)
		{
			switch (fieldType)
			{
				case Field.FieldType.Double:
					return ((double)currentValue) + 1;
				case Field.FieldType.Single:
					return ((float)currentValue) + 1;
				case Field.FieldType.Integer:
					return ((int)currentValue) + 1;
				case Field.FieldType.SmallInteger:
					return ((short)currentValue) + 1;
				case Field.FieldType.String:
					{
						var strNum = ((string)currentValue).Replace(Resources.Strings.EnterValue, string.Empty);
						int count = 0;
						if (strNum.Trim().Length > 0)
						{
							int.TryParse(strNum, out count);
						}
						return string.Format("{0}{1}", Resources.Strings.EnterValue, count + 1);
					}
				case Field.FieldType.GUID:
					return Guid.NewGuid();
				default:
					return null;
			}
		}

		#endregion
	}

	/// <summary>
	/// This comparer is used by Linq query Distinct operation.
	/// </summary>
	public sealed class FieldValueEqualityComparer : IEqualityComparer<FieldValue>
	{
		public bool Equals(FieldValue x, FieldValue y)
		{
			return EqualValue(x, y);
		}

		public int GetHashCode(FieldValue obj)
		{
			if (Object.ReferenceEquals(obj, null)) return 0;
			return obj.Value == null ? 0 : obj.Value.GetHashCode();
		}

		#region Utility Methods

		/// <summary>
		/// Checks for equality between the left and right operand by determining FieldValue.Value data type
		/// and using its respective Equals operator.
		/// </summary>
		/// <param name="x">left operand.</param>
		/// <param name="y">right operand.</param>
		/// <returns>equality</returns>
		public static bool EqualValue(FieldValue x, FieldValue y)
		{
			if (Object.ReferenceEquals(x, y))
				return true;
			if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
				return false;
			return EqualObject(x.ValueInCorrectType, y.ValueInCorrectType);
		}

		/// <summary>
		/// Checks for equality between the left and right operand by determining value data type 
		/// and using its respective Equals operator.
		/// </summary>
		/// <param name="x">left operand.</param>
		/// <param name="y">righto operand.</param>
		/// <returns>equality</returns>
		public static bool EqualObject(object x, object y)
		{
			if (Object.ReferenceEquals(x, y))
				return true;
			if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
				return false;
			if (x != null && y != null)
			{
                if (x.GetType() != y.GetType())
                    return false;
			}
			if (x is string && y is string)
				return string.Equals((x as string), (y as string));
			if (x is DateTime && y is DateTime)
				return DateTime.Equals((DateTime)x, (DateTime)y);
			if (x is int && y is int)
				return int.Equals((int)x, (int)y);
			if (x is double && y is double)
				return double.Equals((double)x, (double)y);
			if (x is short && y is short)
				return short.Equals((short)x, (short)y);
			if (x is Single && y is Single)
				return Single.Equals(x, y);
			return x == y;
		}

		#endregion
	}
}
