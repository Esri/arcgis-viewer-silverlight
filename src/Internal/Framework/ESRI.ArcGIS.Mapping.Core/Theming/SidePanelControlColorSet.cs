/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace ESRI.ArcGIS.Mapping.Core
{
    [DataContract]
    public class SidePanelControlColorSet : INotifyPropertyChanged
    {
        private Color selectedStateOutlineColor = Color.FromArgb(255, 12, 51, 106);
        [DataMember]
        public Color SelectedStateOutlineColor
        {
            get
            {
                return selectedStateOutlineColor;
            }
            set
            {
                if (selectedStateOutlineColor != value)
                {
                    selectedStateOutlineColor = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("SelectedStateOutlineColor"));
                }
            }
        }

        private Color selectedStateFillColor = Color.FromArgb(255,0,0,255);
        [DataMember]
        public Color SelectedStateFillColor
        {
            get
            {
                return selectedStateFillColor;
            }
            set
            {
                if (selectedStateFillColor != value)
                {
                    selectedStateFillColor = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("SelectedStateFillColor"));
                }
            }
        }

        private Color separatorLineColor = Color.FromArgb(255, 0x2D, 0x67, 0xBA);
        [DataMember]
        public Color SeparatorLineColor
        {
            get
            {
                return separatorLineColor;
            }
            set
            {
                if (separatorLineColor != value)
                {
                    separatorLineColor = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("SeparatorLineColor"));
                }
            }
        }

        private Color mouseOverOutlineColor = Color.FromArgb(0x7F, 0x1C, 0x65, 0xCA);
        [DataMember]
        public Color MouseOverOutlineColor
        {
            get
            {
                return mouseOverOutlineColor;
            }
            set
            {
                if (mouseOverOutlineColor != value)
                {
                    mouseOverOutlineColor = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("MouseOverOutlineColor"));
                }
            }
        }

        private Color mouseOverFillColor = Color.FromArgb(0x19, 0x1C, 0x65, 0xCA);
        [DataMember]
        public Color MouseOverFillColor
        {
            get
            {
                return mouseOverFillColor;
            }
            set
            {
                if (mouseOverFillColor != value)
                {
                    mouseOverFillColor = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("MouseOverFillColor"));
                }
            }
        }

        private Color backgroundStartGradientColor = Color.FromArgb(255, 0xE0, 0xE3, 0xE7);
        [DataMember]
        public Color BackgroundStartGradientColor
        {
            get
            {
                return backgroundStartGradientColor;
            }
            set
            {
                if (backgroundStartGradientColor != value)
                {
                    backgroundStartGradientColor = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("BackgroundStartGradientColor"));
                }
            }
        }

        private Color backgroundEndGradientColor = Color.FromArgb(255, 0xFB, 0xFC, 0xFD);
        [DataMember]
        public Color BackgroundEndGradientColor
        {
            get
            {
                return backgroundEndGradientColor;
            }
            set
            {
                if (backgroundEndGradientColor != value)
                {
                    backgroundEndGradientColor = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("BackgroundEndGradientColor"));
                }
            }
        }

        #region INotifyPropertyChanged Members

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public SidePanelControlColorSet Clone()
        {
            return this.MemberwiseClone() as SidePanelControlColorSet;
        }
    }
}
