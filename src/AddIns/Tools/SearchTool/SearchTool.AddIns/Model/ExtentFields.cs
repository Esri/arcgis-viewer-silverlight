/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using SearchTool.Resources;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace SearchTool
{
    /// <summary>
    /// Represents the names of fields containing extent paramaters
    /// </summary>
    [DataContract]
    public class ExtentFields : INotifyPropertyChanged
    {
        public ExtentFields() { }

        public ExtentFields(Field xMinField, Field yMinField, Field xMaxField, Field yMaxField)
        {
            XMinField = xMinField;
            XMaxField = xMaxField;
            YMinField = yMinField;
            YMaxField = yMaxField;
        }

        private Field xMinField;
        [DataMember]
        public Field XMinField 
        {
            get { return xMinField; }
            set
            {
                if (xMinField != value)
                {
                    xMinField = value;
                    OnPropertyChanged("XMinField");
                }
            }
        }

        private Field yMinField;
        [DataMember]
        public Field YMinField
        {
            get { return yMinField; }
            set
            {
                if (yMinField != value)
                {
                    yMinField = value;
                    OnPropertyChanged("YMinField");
                }
            }
        }

        private Field xMaxField;
        [DataMember]
        public Field XMaxField 
        {
            get { return xMaxField; }
            set
            {
                if (xMaxField != value)
                {
                    xMaxField = value;
                    OnPropertyChanged("XMaxField");
                }
            }
        }

        private Field yMaxField;
        [DataMember]
        public Field YMaxField 
        {
            get { return yMaxField; }
            set
            {
                if (yMaxField != value)
                {
                    yMaxField = value;
                    OnPropertyChanged("YMaxField");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
