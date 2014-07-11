/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.ComponentModel;
using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Represents the configuration of a layer's field or attribute in an application
    /// </summary>
    [DataContract]
    public class FieldSettings : INotifyPropertyChanged
    {
        private string _name;
        /// <summary>
        /// Gets or sets the name of the field in the dataset
        /// </summary>
        [DataMember]        
        public string Name {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        private string _displayName;
        /// <summary>
        /// Gets or sets the display name of the field to use when the field name is shown within the application
        /// </summary>
        [DataMember]
        public string DisplayName
        {
            get
            {
                return _displayName ?? Name;
            }
            set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    OnPropertyChanged("DisplayName");
                }
            }
        }

        private bool _visibleOnMapTip = true;
        /// <summary>
        /// Gets or sets whether the field should be included on popups
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool VisibleOnMapTip
        {
            get
            {
                return _visibleOnMapTip;
            }
            set
            {
                if (_visibleOnMapTip != value)
                {
                    _visibleOnMapTip = value;
                    OnPropertyChanged(("VisibleOnMapTip"));
                }
            }
        }

        private bool _visibleInAttributeDisplay = true;
        /// <summary>
        /// Gets or sets whether the field should be shown in the attribute table
        /// </summary>
        [DataMember(EmitDefaultValue=false)]
        public bool VisibleInAttributeDisplay
        {
            get
            {
                return _visibleInAttributeDisplay;
            }
            set
            {
                if (_visibleInAttributeDisplay != value)
                {
                    _visibleInAttributeDisplay = value;
                    OnPropertyChanged(("VisibleInAttributeDisplay"));
                }
            }
        }

        /// <summary>
        /// Gets or sets the field's <see cref="FieldType"/>
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public FieldType FieldType { get; set; }

        /// <summary>
        /// Gets the string representation of the field
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return DisplayName ?? Name;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property of the field has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
