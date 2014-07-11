/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace PrintTool.AddIns
{
	/// <summary>
	/// Print layout when ArcGIS 10.1 Server printing is not used.
	/// </summary>
	[XmlRoot("PrintLayout")]
	[DataContract]
	public class PrintLayout : INotifyPropertyChanged
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PrintLayout"/> class.
		/// </summary>
		public PrintLayout()
		{ 
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PrintLayout"/> class.
		/// </summary>
		/// <param name="printLayout">The print layout.</param>
		public PrintLayout(PrintLayout printLayout)
		{
			if (printLayout != null)
			{
				ID = printLayout.ID;
				Description = printLayout.Description;
                DisplayName = printLayout.DisplayName;
			}
		}

		#endregion
		
		#region Properties

		private const string XAML_PATH = "./Config/Print/{0}.xaml";
		private const string IMAGE_PATH = "./Config/Print/{0}.png";

		#region DataContractSerializable and XmlSerializable properties

		private string id;
		/// <summary>
		/// Gets or sets the print layout ID.
		/// </summary>
		[XmlAttribute("ID")]
		[DataMember]
		public string ID
		{
			get { return id; }
			set
			{
				if (id != value)
				{
					id = value;
					OnPropertyChanged("ID");
					OnPropertyChanged("DisplayName");
					OnPropertyChanged("XamlFilePath");
					OnPropertyChanged("PreviewImagePath");
				}
			}
		}

		private string description;
		/// <summary>
		/// Gets or sets the print layout description.
		/// </summary>
		[XmlAttribute(AttributeName = "Description")]
		[DataMember]
		public string Description
		{
			get { return description; }
			set
			{
				if (description != value)
				{
					description = value;
					OnPropertyChanged("Description");
				}
			}
		}


        private string displayName;
        /// <summary>
        /// Gets or sets the display name of the print layout
        /// </summary>
        [XmlAttribute("DisplayName")]
        [DataMember]
        public string DisplayName
        {
            get { return displayName; }
            set
            {
                if (displayName != value)
                {
                    displayName = value;
                    OnPropertyChanged("DisplayName");
                }
            }
        }

		#endregion

		/// <summary>
		/// Gets the XAML file path of the print layout from its ID.
		/// </summary>
		public string XamlFilePath
		{
			get
			{
				if (!string.IsNullOrEmpty(ID))
					return string.Format(XAML_PATH, ID);
				return null;
			}
		}

		/// <summary>
		/// Gets the preview image path of the print layout from its ID.
		/// </summary>
		public string PreviewImagePath
		{
			get
			{
				if (!string.IsNullOrEmpty(ID))
					return string.Format(IMAGE_PATH, ID);
				return null;
			}
		}

		#endregion

		#region INotifyPropertyChanged

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string property)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}

		#endregion
	}
}
