/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Provides information about the item currently shown in the popup
    /// </summary>
    public class PopupItem : INotifyPropertyChanged
    {
        private Graphic graphic;
        /// <summary>
        /// Gets or sets the current graphic feature
        /// </summary>
        public Graphic Graphic
        {
            get { return graphic; }
            set
            {
                if (graphic != null)
                    graphic.AttributeValueChanged -= graphic_AttributeValueChanged;

                graphic = value;

                if (graphic != null)
                    graphic.AttributeValueChanged += graphic_AttributeValueChanged;

                OnPropertyChanged("Graphic");
            }
        }

        void graphic_AttributeValueChanged(object sender, Graphics.DictionaryChangedEventArgs e)
        {
            OnPropertyChanged("Graphic");
        }

        private Layer layer;
        /// <summary>
        /// Gets or sets the layer containing the feature currently shown in the popup
        /// </summary>
        public Layer Layer
        {
            get { return layer; }
            set 
            {
                if (layer != value)
                {
                    layer = value;
                    OnPropertyChanged("Layer");
                }
            }
        }

        private string layerName;
        /// <summary>
        /// Gets or set the name of the layer as it is shown in the popup
        /// </summary>
        public string LayerName
        {
            get { return layerName; }
            set
            {
                if (layerName != value)
                {
                    layerName = value;
                    OnPropertyChanged("LayerName");
                }
            }
        }

        private string title;
        /// <summary>
        /// Gets or sets the current title of the popup
        /// </summary>
        public string Title
        {
            get { return title; }
            set 
            {
                if (title != value)
                {
                    title = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        private string titleExpression;
        /// <summary>
        /// Gets or sets the expression used to construct the popup title for the current layer
        /// </summary>
        public string TitleExpression
        {
            get { return titleExpression; }
            set 
            {
                if (titleExpression != value)
                {
                    titleExpression = value;
                    OnPropertyChanged("TitleExpression");
                }
            }
        }

        private ICollection<FieldSettings> fieldInfos;
        /// <summary>
        /// Gets or sets the collection of <see cref="FieldSettings"/> that are used for field display in the popup
        /// </summary>
        public ICollection<FieldSettings> FieldInfos
        {
            get { return fieldInfos; }
            set 
            {
                if (fieldInfos != value)
                {
                    fieldInfos = value;
                    OnPropertyChanged("FieldInfos");
                }
            }
        }

        private DataTemplate dataTemplate;
        /// <summary>
        /// Gets or sets the template used for displaying attributes on the popup for the current layer
        /// </summary>
        public DataTemplate DataTemplate
        {
            get { return dataTemplate; }
            set 
            {
                if (dataTemplate != value)
                {
                    dataTemplate = value;
                    OnPropertyChanged("DataTemplate");
                }
            }
        }

        private int layerId = -1;
        /// <summary>
        /// Gets or sets the ID of the sub-layer containing the feautre currently shown in the popup
        /// </summary>
        public int LayerId
        {
            get { return layerId; }
            set 
            {
                if (layerId != value)
                {
                    layerId = value;
                    OnPropertyChanged("LayerId");
                }
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when a property of the <see cref="PopupItem"/> has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
