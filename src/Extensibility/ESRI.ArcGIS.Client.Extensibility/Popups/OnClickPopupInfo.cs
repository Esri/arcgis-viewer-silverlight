/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Provides information about popups in the application that are shown on click
    /// </summary>
    public class OnClickPopupInfo : PopupInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnClickPopupInfo"/> class
        /// </summary>
        public OnClickPopupInfo()
        {
            Next = new DelegateCommand(nextCommand);
            Previous = new DelegateCommand(previousCommand);
            PopupItems = new ObservableCollection<PopupItem>();
        }

        private int selectedIndex = -1;
        /// <summary>
        /// Gets or sets the index of the selected <see cref="PopupItem"/>
        /// </summary>
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;

                    if (SelectedIndex < 0 || PopupItems.Count < 1)
                    {
                        SelectionDescription = string.Empty;
                        PopupItem = null;
                        return;
                    }

                    setSelectionDescription();
                    PopupItem = PopupItems[SelectedIndex];

                    OnPropertyChanged("SelectedIndex");
                }
            }
        }

        /// <summary>
        /// Updates the <see cref="SelectionDescription"/> property
        /// </summary>
        private void setSelectionDescription()
        {
            SelectionDescription = string.Format(Resources.Strings.SelectedIndexOfTotal, SelectedIndex + 1, PopupItems.Count);
        }

        private string selectionDescription;
        /// <summary>
        /// Gets the descriptive text for the currently selected <see cref="PopupItem"/>
        /// </summary>
        public string SelectionDescription
        {
            get { return selectionDescription; }
            private set
            {
                if (selectionDescription != value)
                {
                    selectionDescription = value;
                    OnPropertyChanged("SelectionDescription");
                }
            }
        }

        private ObservableCollection<PopupItem> popupItems;
        /// <summary>
        /// Gets or sets the collection of PopupItems for the popup
        /// </summary>
        public ObservableCollection<PopupItem> PopupItems
        {
            get { return popupItems; }
            set
            {
                if (popupItems != value)
                {
                    if (popupItems != null)
                        popupItems.CollectionChanged -= popupItems_CollectionChanged;

                    popupItems = value;

                    if (popupItems != null)
                        popupItems.CollectionChanged += popupItems_CollectionChanged;

                    if (popupItems != null && popupItems.Count > 0)
                    {
                        SelectedIndex = 0;
                    }
                    else
                    {
                        SelectedIndex = -1;
                    }

                    OnPropertyChanged("PopupItems");
                }
            }
        }

        void popupItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            setSelectionDescription();
        }

        /// <summary>
        /// Method invoked when the <see cref="Next"/> command is executed
        /// </summary>
        /// <param name="commandParameter">Object to use during execution of the command</param>
        private void nextCommand(object commandParameter)
        {
            if (SelectedIndex + 1 < PopupItems.Count)
                SelectedIndex++;
            else
                SelectedIndex = 0;
        }

        private ICommand next;
        /// <summary>
        /// Moves to the next <see cref="PopupItem"/> in the popup
        /// </summary>
        public ICommand Next
        {
            get { return next; }
            private set { next = value; }
        }

        /// <summary>
        /// Method invoked when the <see cref="Previous"/> command is executed
        /// </summary>
        /// <param name="commandParameter">Object to use during execution of the command</param>
        private void previousCommand(object commandParameter)
        {
            if (SelectedIndex - 1 >= 0)
                SelectedIndex--;
            else
                SelectedIndex = PopupItems.Count - 1;
        }

        private ICommand previous;
        /// <summary>
        /// Moves to the previous <see cref="PopupItem"/> in the popup
        /// </summary>
        public ICommand Previous
        {
            get { return previous; }
            private set
            {
                if (previous != value)
                {
                    previous = value;
                    OnPropertyChanged("Previous");
                }
            }
        }

        private bool inProgress;
        /// <summary>
        /// Gets or sets the busy state of the popup
        /// </summary>
        public bool InProgress
        {
            get { return inProgress; }
            set
            {
                if (inProgress != value)
                {
                    inProgress = value;
                    OnPropertyChanged("InProgress");
                }
            }
        }

        /// <summary>
        /// Always returns null
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return null;
        }
    }
}
