/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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

namespace ESRI.ArcGIS.Mapping.Core
{
    [DataContract]
    public class ChildWindowColorSet : INotifyPropertyChanged
    {
        private Color titleBarColor = Color.FromArgb(255, 36, 57, 78);
        [DataMember]
        public Color TitleBarColor
        {
            get
            {
                return titleBarColor;
            }
            set
            {
                if (titleBarColor != value)
                {
                    titleBarColor = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("TitleBarColor"));
                }
            }
        }

        private Color titleBarForegroundColor = Colors.White;
        [DataMember]
        public Color TitleBarForegroundColor
        {
            get
            {
                return titleBarForegroundColor;
            }
            set
            {
                if (titleBarForegroundColor != value)
                {
                    titleBarForegroundColor = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("TitleBarForegroundColor"));
                }
            }
        }

        private Color backgroundStartGradientColor = Color.FromArgb(255, 251, 252, 253);
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

        private Color backgroundEndGradientColor = Color.FromArgb(255, 224, 227, 231);
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

        public ChildWindowColorSet Clone()
        {
            return this.MemberwiseClone() as ChildWindowColorSet;
        }
    }
}
