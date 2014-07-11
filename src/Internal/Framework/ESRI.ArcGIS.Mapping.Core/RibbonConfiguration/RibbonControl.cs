/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Core
{
    [DataContract]
    public class RibbonControl : INotifyPropertyChanged
    {        
        public RibbonControl(string id)
        {
            ID = id;     
        }

        [DataMember(EmitDefaultValue = false)]
        public string ID { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Command { get; set; }
     
        private bool isAllowed;
        [DataMember(EmitDefaultValue = false)]
        public bool IsAllowed
        {
            get { return isAllowed; }
            set
            {
                if (isAllowed != value)
                {
                    isAllowed = value;
                    OnPropertyChanged("IsAllowed");
                }
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public string Title { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string DisplayIcon { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string RibbonXml { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool Ignore { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string CommandValueId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string QueryCommand { get; set; }

        public RibbonGroup Parent { get; set; }
        
        public bool IsCustomControl { get; set; }
        
        public bool SupportsConfiguration { get; set; }

        public bool IsToggleButton { get; set; }        
        
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        internal string ToRibbonXml(bool isEditMode)
        {
            if (string.IsNullOrEmpty(RibbonXml))
                RibbonXml = generateRibbonXml(isEditMode);
            return RibbonXml;
        }

        private string generateRibbonXml(bool isEditMode)
        {
            return string.Format(@"
                <{0}
					Id=""{1}""
					Command=""{2}""		
                    ToolTipTitle=""{5}""									
                    ToolTipDescription=""{3}""
					Image32by32=""{4}""                        
					LabelText=""{5}""
                    Description=""{3}""
                    CommandValueId=""{6}""
                    {7}
					TemplateAlias=""o1""/>", IsToggleButton ? "ToggleButton" : "Button", ID, Command, Description,
                                           DisplayIcon, Title, CommandValueId,
                                           IsToggleButton ? "QueryCommand=\"" + QueryCommand + "\" " : null);
        }
    }    
}
