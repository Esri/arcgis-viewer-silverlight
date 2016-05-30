/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
    public class RibbonConfiguration
    {
        [DataMember]
        public List<RibbonTab> Tabs { get; set; }        

        public string ToRibbonXml(bool isEditMode)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"<ContextualGroup
		 Id=""ESRI.ArcGIS.Mapping.SharePoint.MapWebPart.Ribbon""
		 Sequence=""92""
		 Color=""Orange""
		 Command=""ESRI.ArcGIS.Mapping.SharePoint.MapWebPart.Commands.EnableContextualGroup""
		 Title=""ArcGIS""
		 ContextualGroupId=""ESRI.ArcGIS.Mapping.SharePoint.MapWebPart.Ribbon"">
            ");
            if (Tabs != null)
            {
                foreach (RibbonTab ribbonTab in Tabs)
                {
                    sb.AppendLine(ribbonTab.ToRibbonXml(isEditMode));
                }
            }
            sb.AppendLine("</ContextualGroup>");
            return sb.ToString();
        }
    }

    [DataContract]
    public class RibbonTab : INotifyPropertyChanged
    {
        [DataMember(EmitDefaultValue = false)]
        public string Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Title { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Command { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int Sequence { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsCustomTab { get; set; }

        [DataMember]
        public List<GroupScalingInfo> MaxSizeInfos { get; set; }

        [DataMember]
        public List<GroupScalingInfo> SizeInfos { get; set; }

        [DataMember]
        public List<RibbonGroup> Groups { get; set; }
        
        [DataMember(EmitDefaultValue=false)]
        private bool? isAllowed;
        public bool? IsAllowed
        {
            get { return isAllowed; }
            set {
                if (isAllowed != value)
                {
                    isAllowed = value;
                    OnPropertyChanged("IsAllowed");
                }
            }
        }

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
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"<Tab
		 Id=""{0}""
		 Title=""{1}""
		 Description=""{2}""
		 Command=""{3}"">", Id, Title, Description, Command);
            
            sb.AppendLine("<Scaling>");
            if (this.MaxSizeInfos != null)
                foreach (GroupScalingInfo groupScaleInfo in MaxSizeInfos)
                    sb.AppendFormat("<MaxSize GroupId=\"{0}\" Size=\"{1}\" {2} />", groupScaleInfo.GroupId, groupScaleInfo.Size, (groupScaleInfo.Sequence != default(int) ? string.Format("Sequence=\"{0}\"", groupScaleInfo.Sequence) : string.Empty)); 

            if (this.SizeInfos != null)
                foreach (GroupScalingInfo groupScaleInfo in SizeInfos)
                    sb.AppendFormat("<Scale GroupId=\"{0}\" Size=\"{1}\" {2} />", groupScaleInfo.GroupId, groupScaleInfo.Size, (groupScaleInfo.Sequence != default(int) ? string.Format("Sequence=\"{0}\"", groupScaleInfo.Sequence) : string.Empty));
                
            sb.AppendLine("</Scaling>");

            sb.AppendLine("<Groups>");
            if (Groups != null)
            {
                foreach (RibbonGroup group in Groups)
                {
                    sb.AppendLine(group.ToRibbonXml(isEditMode));
                }
            }
            sb.AppendLine("</Groups>");
            
            sb.Append("</Tab>");
            return sb.ToString();
        }
    }

    [DataContract]
    public class RibbonGroup : INotifyPropertyChanged
    {
        [DataMember(EmitDefaultValue = false)]
        public string Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Title { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Command { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int Sequence { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsCustomGroup { get; set; }

        [DataMember]
        public string Image32by32Popup { get; set; }

        [DataMember]
        public string Template { get; set; }

        [DataMember]
        public List<RibbonControl> Controls { get; set; }        

        [DataMember(EmitDefaultValue = false)]
        private bool? isAllowed;
        public bool? IsAllowed
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

		public RibbonTab Parent { get; set; }

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
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"<Group Id=""{0}""
                 Description=""{1}""
			 Title=""{2}""
			 Command=""{3}""
			 Template=""{4}""
             {5}>", Id, Description, Title, Command, Template, 
                  !string.IsNullOrEmpty(Image32by32Popup) ? string.Format("Image32by32Popup=\"{0}\"", Image32by32Popup) : string.Empty);

            sb.AppendLine("<Controls>");
            if (Controls != null)
            {                
                foreach (RibbonControl control in Controls)
                {
                    sb.AppendLine(control.ToRibbonXml(isEditMode));
                }             
            }
            sb.AppendLine("</Controls>");
            sb.AppendLine("</Group>");
            return sb.ToString();
        }
    }

    [DataContract]
    public class GroupScalingInfo
    {
        [DataMember(EmitDefaultValue=false)]
        public string GroupId { get; set; }

        [DataMember(EmitDefaultValue=false)]
        public string Size { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int Sequence { get; set; }
    }
}
