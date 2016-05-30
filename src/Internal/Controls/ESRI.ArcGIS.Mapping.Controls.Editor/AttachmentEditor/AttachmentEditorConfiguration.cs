/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Controls.Editor
{
    [DataContract]
    public class AttachmentEditorConfiguration: INotifyPropertyChanged
    {
        #region Filter
        private string _filter = "All Files (*.*)|*.*";
        [DataMember(Name = "Filter", IsRequired = false, EmitDefaultValue=false, Order = 0)]
        public string Filter
        {
            get {return _filter;}
            set { _filter = value; }
        }
        #endregion

        #region FilterIndex
        private int _filterIndex = 1;
        [DataMember(Name = "FilterIndex", IsRequired = false, EmitDefaultValue = false, Order = 1)]
        public int FilterIndex
        {
            get { return _filterIndex; }
            set { _filterIndex = value; }
        }
        #endregion

        #region MultiSelect
        private bool _multiSelect;
        [DataMember(Name = "MultiSelect", IsRequired = false, EmitDefaultValue = false, Order = 3)]
        public bool MultiSelect
        {
            get { return _multiSelect; }
            set { _multiSelect = value; }
        }
        #endregion

        #region Width
        private int _width = 280;
        [DataMember(Name = "Width", IsRequired = false, EmitDefaultValue = false, Order = 4)]
        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }
        #endregion

        #region Height
        private int _height = 190;
        [DataMember(Name = "Height", IsRequired = false, EmitDefaultValue = false, Order = 5)]
        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }
        #endregion

        #region Serialization
        public override string ToString()
        {
            return DataContractSerializationHelper.Serialize<AttachmentEditorConfiguration>(this);
        }
        public static AttachmentEditorConfiguration FromString(string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
                return DataContractSerializationHelper.Deserialize<AttachmentEditorConfiguration>(str);

            return null;
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
