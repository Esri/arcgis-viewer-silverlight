/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Toolkit;
using Bookmarks.AddIns.Resources;

namespace Bookmarks.AddIns
{
    /// <summary>
    /// Responsible for instantiating the View and ViewModel, calling Execute when the Tool on the toolbar
    /// is clicked, Saving and Loading the specified configuration (bookmarks), and creating the pages
    /// for the Add Tool wizard.
    /// </summary>
    [LocalizedDisplayName("Bookmarks")]
    [LocalizedDescription("BookmarksToolDescription")]
    [LocalizedCategory("Map")]
    [DefaultIcon("/Bookmarks.AddIns;component/Images/Bookmark.png")]
    [Export(typeof(ICommand))]
    public class BookmarksTool : ICommand, ISupportsWizardConfiguration
    {
        #region Member Variables

        // The BookmarksConfigurationView. This is the UI that displays when configuring the bookmarks.
        private BookmarksConfigurationView configView;

        // The BookmarksNavigationView. This is the UI that displays the configured bookmarks and allows 
        // the user to zoom to each one.
        private BookmarksNavigationView navView;

        // The collection of bookmarks.
        ObservableCollection<Bookmark.MapBookmark> bookmarkCollection = new ObservableCollection<Bookmark.MapBookmark>();

        // The last saved configuration.  Used to support cancellation of configuration.
        private string lastConfiguration;

        #endregion

        /// <summary>
        /// Instantiates the View and the ViewModel and sets the DataContext of the View to the ViewModel. 
        /// </summary>
        public BookmarksTool()
        {
            // Create an instance of the BookmarksConfigurationView. This View provides the dialog for configuring the
            // bookmarks. Set the DataContext of the configView to a new instance of the BookmarkViewModel, and pass in
            // the Map and the collection of bookmarks. 
            configView = new BookmarksConfigurationView() { Margin = new Thickness(0, 5, 0, 0) };

            // Get the collection of bookmarks that will be used throughout the add-in.  Note this has to come
            // from the View because the Bookmarks control's bookmarks collection is read-only.
            bookmarkCollection = configView.BookmarksControl.Bookmarks;
            configView.DataContext = new BookmarksViewModel(MapApplication.Current.Map, bookmarkCollection);

            // Create an instance of the BookmarksNavigationView that is called when the icon on the toolbar is clicked. Set
            // the DataContext of the navView to the BookmarkViewModel, and pass in the Map and the collection of bookmarks.
            navView = new BookmarksNavigationView() { Margin = new Thickness(10) };
            navView.DataContext = new BookmarksViewModel(MapApplication.Current.Map, bookmarkCollection);

            // When the collection of bookmarks changes, notify the framework that the tool's execution state has changed
            bookmarkCollection.CollectionChanged += (o, e) => RaiseCanExecuteChanged();
        }

        #region ICommand members
        /// <summary>
        /// Shows the BookmarksNavigationView (list of configured Bookmarks) when the icon on the toolbar is clicked.
        /// </summary>
        public void Execute(object parameter)
        {
            MapApplication.Current.ShowWindow(Strings.Bookmarks, navView);
        }

        /// <summary>
        /// Determines whether the tool is available to execute.
        /// </summary>
        public bool CanExecute(object parameter)
        {
            // The command can only be executed when at least one bookmark is present
            return bookmarkCollection.Count > 0;
        }

        public event EventHandler CanExecuteChanged;

        #endregion

        #region ISupportsConfiguration members
        /// <summary>
        /// Fires when the page on the configuration Wizard changes.
        /// </summary>
        public void Configure()
        {
            // No need for logic on page change in this particular example.
        }

        /// <summary>
        /// Deserializes the saved bookmarks collection 
        /// </summary>
        public void LoadConfiguration(string configData)
        {
            // Clear any currently added bookmarks
            bookmarkCollection.Clear();
            lastConfiguration = configData;

            if (!string.IsNullOrEmpty(configData))
            {
                using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(configData)))
                {
                    // Deserializes the bookmarks collection
                    DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(ObservableCollection<Bookmark.MapBookmark>));
                    memoryStream.Position = 0;
                    ObservableCollection<Bookmark.MapBookmark> deserializedBookmarks = (ObservableCollection<Bookmark.MapBookmark>)xmlSerializer.ReadObject(memoryStream);
                    memoryStream.Close();

                    if (deserializedBookmarks != null)
                    {
                        // Copy the deserialized bookmarks 
                        foreach (Bookmark.MapBookmark bookmark in deserializedBookmarks)
                            bookmarkCollection.Add(bookmark);
                    }
                }
            }
        }

        /// <summary>
        /// Serialize the bookmarks from the configuration wizard
        /// </summary>
        /// <returns></returns>
        public string SaveConfiguration()
        {

            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Serialize the bookmarks collection
                DataContractSerializer serializer = new DataContractSerializer(typeof(ObservableCollection<Bookmark.MapBookmark>));
                serializer.WriteObject(memoryStream, bookmarkCollection);
                memoryStream.Position = 0;
                using (StreamReader reader = new StreamReader(memoryStream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        #endregion

        #region ISupportsWizardConfiguration members

        private WizardPage currentPage;
        /// <summary>
        /// The page currently shown by the configuration wizard.
        /// </summary>
        public WizardPage CurrentPage
        {
            get
            {

                return currentPage;
            }
            set
            {
                if (currentPage != value)
                    currentPage = value;
            }
        }

        // Create a Size member variable for DesiredSize.  The Height and Width are set to the minimum values 
        // required to properly display the UI. Note that the configuration UI should also be flexible enough 
        // to fit any width or height beyond that specified by DesiredSize.
        private Size desiredSize = new Size() { Height = 320, Width = 340 };
        /// <summary>
        /// The desired size of the configuration dialog
        /// </summary>
        public Size DesiredSize
        {
            get
            {
                return desiredSize;
            }
            set
            {
                if (desiredSize != value)
                    desiredSize = value;
            }
        }

        /// <summary>
        /// Called by the framework when the page is about to be changed. 
        /// </summary>
        public bool PageChanging()
        {
            // Here you can check the validation state of the current page before the wizard goes to the next page
            // and cancel the page change if necessary (i.e. by returning false).
            return true;
        }

        /// <summary>
        /// The collection of configuration pages to show in the Wizard. 
        /// </summary>
        private ObservableCollection<WizardPage> pages;
        public ObservableCollection<WizardPage> Pages
        {
            get
            {
                // The host application will retrieve the add-in's configuration UI from the Pages property and 
                // display it as it sees fit. It is the add-in's responsibility to return the collection of 
                // configuration pages to show via this property.  In this case, the add-in will nstantiate the 
                // Pages collection and add a single WizardPage to it that hosts the bookmarks configuration view 

                // Initialize configuration pages if not yet initialized
                if (pages == null)
                {
                    // Create a new WizardPage; set the configView as the Content and set the Heading. 
                    WizardPage page = new WizardPage();
                    page.Content = configView;
                    page.Heading = Strings.ConfigureBookmarksHeading;

                    // Create a new WizardPage collection and add the new page.
                    pages = new ObservableCollection<WizardPage>();
                    pages.Add(page);

                    // Determine whether the page input is valid.  In this case, the input is valid if there 
                    // is at least one bookmark added.  If InputValid is false, users will be prevented from 
                    // completeing configuraiton.
                    page.InputValid = bookmarkCollection != null && bookmarkCollection.Count > 0;

                    // Hook to the bookmark collection's changed event to update whether input is valid based
                    // on whether there are bookmarks
                    bookmarkCollection.CollectionChanged += (o, e) =>
                        {
                            page.InputValid = bookmarkCollection != null && bookmarkCollection.Count > 0;
                        };
                }

                return pages;
            }
            set
            {
                //Simply set the pages member variable if the passed-in value does not match.
                if (pages != value)
                    pages = value;
            }

        #endregion

        }

        // Revert configuration on cancel
        public void OnCancelled()
        {
            if (!string.IsNullOrEmpty(lastConfiguration))
                LoadConfiguration(lastConfiguration); // Revert to last saved configuration
            else
                bookmarkCollection.Clear(); // No configuration has been saved yet - revert to no bookmarks
        }

        public void OnCompleted()
        {
            // On the navigation view (i.e. end-user view), unlink and relink the bookmarks collection.  
            // This is necessary to force rebinding of the bookmarks into the UI, since 
            // Bookmark.MapBookmark is not a class that supports property change notifcation. Otherwise, 
            // updates to bookmark names will not show up until the application is saved and reloaded.
            BookmarksViewModel viewModel = navView.DataContext as BookmarksViewModel;
            viewModel.Bookmarks = null;
            viewModel.Bookmarks = bookmarkCollection;

            // Update last saved configuration
            lastConfiguration = SaveConfiguration();
        }

        private void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}
