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
using ESRI.ArcGIS.Client.WebMap;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class WebMapSettings : INotifyPropertyChanged
    {
        private string id;
        /// <summary>
        /// Gets or sets the ID of the web map.
        /// </summary>
        public string ID 
        {
            get { return id; } 
            set
            {
                if (value != id)
                {
                    id = value;
                    OnPropertyChanged("ID");
                }
            } 
        }

        private string title;
        /// <summary>
        /// Gets or sets the title of the web map.
        /// </summary>
        public string Title
        {
            get { return title; }
            set
            {
                if (value != title)
                {
                    title = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        private Document document;
        /// <summary>
        /// Gets or sets the <see cref="Document"/> containing information about the web map.
        /// </summary>
        public Document Document 
        {
            get { return document; }
            set
            {
                if (document != value)
                {
                    document = value;
                    OnPropertyChanged("Document");
                }
            }
        }

        private ItemInfo itemInfo;
        /// <summary>
        /// Gets or sets the <see cref="ItemInfo"/> containing information about the item.
        /// </summary>
        public ItemInfo ItemInfo
        {
            get { return itemInfo; }
            set
            {
                if (itemInfo != value)
                {
                    itemInfo = value;
                    OnPropertyChanged("ItemInfo");

                    if (itemInfo == null)
                    {
                        ID = null;
                        Title = null;
                    }
                    else
                    {
                        ID = itemInfo.ID;
                        Title = itemInfo.Title;
                    }
                }
            }
        }

        private bool? linked;
        /// <summary>
        /// Gets or sets whether a link to the web map is enforced
        /// </summary>
        public bool? Linked 
        {
            get { return linked; }
            set
            {
                if (linked != value)
                {
                    linked = value;
                    OnPropertyChanged("Linked");
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
