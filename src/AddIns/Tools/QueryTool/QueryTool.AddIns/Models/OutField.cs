/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.FeatureService;

namespace QueryTool.AddIns
{
	/// <summary>
	/// This class represents the fields requested to be part of the query result.
	/// </summary>
	[DataContract]
	public sealed class OutField : Object, INotifyPropertyChanged
	{
		/// <summary>
		/// Constructor for OutField object.
		/// </summary>
		/// <param name="field">field information (i.e. Name and Alias).</param>
		public OutField(Field field)
		{
			if (field != null)
			{
				Name = field.Name;
				Alias = field.Alias;				
				Type = field.Type;
				Length = field.Length;
				IsEditable = field.Domain == null && field.Type != Field.FieldType.OID && 
					field.Type != Field.FieldType.GlobalID	? true : false;
				Domain = field.Domain is CodedValueDomain ?
						(field.Domain as CodedValueDomain).CodedValues : null;
			}
			IsVisible = true;
		}

		/// <summary>
		/// Copy constructor for the OutField object.
		/// </summary>
		/// <param name="source"></param>
		public OutField(OutField source)
		{
			CopyAllSettings(source);
		}

		/// <summary>
		/// Updates properties of current object with properties of source object.
		/// </summary>
		/// <param name="source"></param>
		public void ApplyChanges(OutField source)
		{
			CopyAllSettings(source);
		}

        public OutField Clone()
        {
            return new OutField(this);
        }

		#region Overrides Equals Operator

		public override int GetHashCode()
		{
			if (this.Name == null) return 0;
			return this.Name.GetHashCode();
		}

		/// <summary>
		/// Overrides equals operator to compare based on name.
		/// </summary>
		/// <param name="obj">right operand.</param>
		/// <returns>true if Value is equal to the parameter Value.</returns>
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is OutField))
				return false;
			return string.Equals(Name, (obj as OutField).Name);
		}

		#endregion
	
		#region INotifyPropertyChanged
		
		#region Serializable Properties

		private string name;
		/// <summary>
		/// Gets or sets the field name.
		/// </summary>
		[DataMember]
		public string Name
		{
			get { return name; }
			set
			{
				if (name != value)
				{
					name = value;
					OnPropertyChanged("Name");
				}
			}
		}

		private string alias;
		/// <summary>
		/// Gets or sets the field alias.
		/// </summary>
		[DataMember]
		public string Alias
		{
			get { return alias; }
			set
			{
				if (alias != value)
				{
					alias = value;
					OnPropertyChanged("Alias");
				}
			}
		}

		private Field.FieldType type;
		/// <summary>
		/// Gets or sets the field type.
		/// </summary>
		[DataMember]
		public Field.FieldType Type
		{
			get { return type; }
			set
			{
				if (type != value)
				{
					type = value;
					OnPropertyChanged("Type");
				}
			}
		}

		private bool isEditable;
		/// <summary>
		/// Gets or sets value indicating whether field can be modified.
		/// </summary>
		[DataMember]
		public bool IsEditable
		{
			get { return isEditable; }
			set
			{
				if (isEditable != value)
				{
					isEditable = value;
					OnPropertyChanged("IsEditable");
				}
			}
		}

		private int length;
		/// <summary>
		/// Gets or sets the length restriction for string fields.
		/// </summary>
		[DataMember]
		public int Length
		{
			get { return length; }
			set
			{
				if (length != value)
				{
					length = value;
					OnPropertyChanged("Length");
				}
			}
		}

		private IDictionary<object, string> domain;
		/// <summary>
		/// Gets or sets coded-value domain information.
		/// </summary>
		[DataMember]
		public IDictionary<object, string> Domain
		{
			get { return domain; }
			set
			{
				if (domain != value)
				{
					domain = value;
					OnPropertyChanged("Domain");
				}
			}
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether field is visible
		/// or requested to be included in the query result.
		/// </summary>
		private bool isVisible;
		[DataMember]
		public bool IsVisible
		{
			get { return isVisible; }
			set
			{
				if (isVisible != value)
				{
					isVisible = value;
					OnPropertyChanged("IsSelected");
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
		/// Updates serializable properties with values from another OutField.
		/// </summary>
		/// <param name="source">The source OutField</param>
		private void CopyAllSettings(OutField source)
		{
			if(source == null)return;
			Name = source.Name;
			Alias = source.Alias;
			Type = source.Type;
			IsEditable = source.IsEditable;
			Length = source.Length;
			Domain = new Dictionary<object, string>();
			if (source.Domain != null)
			{
				foreach (var item in source.Domain)
					Domain[item.Key] = item.Value;
			}
			IsVisible = source.IsVisible;
		}

		#endregion
	}
}
