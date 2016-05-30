/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.ComponentModel;
using System.Windows;

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Provides information about popups in the application
    /// </summary>
    public class PopupInfo : INotifyPropertyChanged
    {
        #region Container

        private FrameworkElement container;
        /// <summary>
        /// Gets or sets the element that popups will be shown within
        /// </summary>
        public FrameworkElement Container
        {
            get { return container; }
            set
            {
                if (container != value)
                {
                    container = value;
                    OnPropertyChanged("Container");
                }
            }
        }

        #endregion

        #region AttributeContainer

        private FrameworkElement attributeContainer;
        /// <summary>
        /// Gets or sets the element that feature attributes will be shown within
        /// </summary>
        public FrameworkElement AttributeContainer
        {
            get { return attributeContainer; }
            set
            {
                if (attributeContainer != value)
                {
                    attributeContainer = value;
                    OnPropertyChanged("AttributeContainer");
                }
            }
        }

        #endregion

        #region AttachmentContainer

        private FrameworkElement attachmentContainer;
        /// <summary>
        /// Gets or sets the element that a feature's attachments will be shown within
        /// </summary>
        public FrameworkElement AttachmentContainer
        {
            get { return attachmentContainer; }
            set
            {
                if (attachmentContainer != value)
                {
                    attachmentContainer = value;
                    OnPropertyChanged("AttachmentContainer");
                }
            }
        }

        #endregion

        #region PopupItem
        private PopupItem popupItem;
        /// <summary>
        /// Gets or sets the current <see cref="PopupItem"/>
        /// </summary>
        public PopupItem PopupItem
        {
            get { return popupItem; }
            set
            {
                if (popupItem != null)
                    popupItem.PropertyChanged -= popupItem_PropertyChanged;

                popupItem = value;

                if (popupItem != null)
                    popupItem.PropertyChanged += popupItem_PropertyChanged;

                OnPropertyChanged("PopupItem");
            }
        }

        void popupItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("PopupItem");
        }

        #endregion

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
        /// Occurs when a property has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

    }
}
