/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Client.Application.Controls
{
    /// <summary>
    /// Provides a control to host wizard pages in
    /// </summary>
    public class Wizard : Control
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Wizard"/> control
        /// </summary>
        public Wizard()
        {
            DefaultStyleKey = typeof(Wizard);
            Pages = new ObservableCollection<WizardPage>();
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code
        /// or internal processes (such as a rebuilding layout pass) call
        /// <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // handle case where current page is populated before template is applied
            if (CurrentPage != null && Pages.Count > 0)
            {
                int newIndex = CurrentPageIndex >= 0 ? CurrentPageIndex : 0;
                if (newIndex != getPageIndex(CurrentPage))
                    CurrentPage = Pages[CurrentPageIndex];
                else
                    updatePage(newIndex, -1);
            }
        }

        #region Properties

        #region Page Handling Properties - CurrentPage, CurrentPageIndex, Pages

        /// <summary>
        /// Identifies the <see cref="CurrentPage"/> DependencyProperty
        /// </summary>
        public static readonly DependencyProperty CurrentPageProperty = DependencyProperty.Register(
            "CurrentPage", typeof(WizardPage), typeof(Wizard), 
            new PropertyMetadata(CurrentPagePropertyChanged));

        /// <summary>
        /// Gets or sets the page currently shown by the wizard
        /// </summary>
        public WizardPage CurrentPage 
        {
            get { return GetValue(CurrentPageProperty) as WizardPage; }
            set { SetValue(CurrentPageProperty, value); }
        }

        // Apply the page update
        private static void CurrentPagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Wizard wizard = d as Wizard;
            int oldIndex = wizard.getPageIndex(e.OldValue as WizardPage);
            int newIndex = wizard.getPageIndex(e.NewValue as WizardPage);
            wizard.updatePage(newIndex, oldIndex);
        }

        private static void InputValid_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Wizard)d).checkCommandsCanExecute();
        }

        /// <summary>
        /// Identifies the <see cref="CurrentPageIndex"/> DependencyProperty
        /// </summary>
        public static readonly DependencyProperty CurrentPageIndexProperty = DependencyProperty.Register(
            "CurrentPageIndex", typeof(int), typeof(Wizard), new PropertyMetadata(-1, CurrentPageIndexPropertyChanged));

        /// <summary>
        /// Gets or sets the index of the current page in the pages collection
        /// </summary>
        public int CurrentPageIndex
        {
            get { return (int)GetValue(CurrentPageIndexProperty); }
            set { SetValue(CurrentPageIndexProperty, value); }
        }

        // Apply the page update
        private static void CurrentPageIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Wizard wizard = d as Wizard;
            int newIndex = (int)e.NewValue;

            // Don't throw exception if Pages are not populated yet - index will be
            // applied when pages collection is initialized
            if (wizard.Pages == null || wizard.Pages.Count == 0)
                return;
            // Pages are initialized.  If index is invalid, throw exception.
            else if (newIndex < -1 || newIndex > wizard.Pages.Count - 1)
                throw new ArgumentOutOfRangeException("CurrentPageIndex");

            if (newIndex > -1)
                wizard.CurrentPage = wizard.Pages[newIndex];
            else if (wizard.CurrentPage != null)
                wizard.CurrentPage = null;
        }

        private ObservableCollection<WizardPage> pages;
        /// <summary>
        /// Gets the collection of pages to be displayed by the wizard
        /// </summary>
        public ObservableCollection<WizardPage> Pages 
        {
            get { return pages; }
            private set 
            {
                pages = value;
                // Initialize current page based on collection and CurrentPageIndex
                if (CurrentPageIndex >= 0 && CurrentPageIndex < pages.Count)
                    CurrentPage = pages[CurrentPageIndex];
                else if (pages.Count > 0)
                    CurrentPage = pages[0];
                pages.CollectionChanged += Pages_CollectionChanged; 
            } 
        }

        private void Pages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Initialize CurrentPage
            if (e.Action == NotifyCollectionChangedAction.Add && CurrentPage == null)
            {
                // if CurrentPageIndex is initialized before the pages collection pages (i.e. when it is
                // declared in XAML), set the current page once the pages collection is populated
                if (CurrentPageIndex >= 0 && CurrentPageIndex < Pages.Count)
                    CurrentPage = Pages[CurrentPageIndex];
                // CurrentPageIndex has not been set, so set CurrentPage to the first page in the collection
                else if (CurrentPageIndex == -1)
                    CurrentPage = Pages[0];
            }
            // Handle page index when pages are removed and the index is higher than the number of pages
            else if (e.Action == NotifyCollectionChangedAction.Remove && CurrentPageIndex >= Pages.Count &&
            Pages.Count > 0)
                CurrentPageIndex = Pages.Count - 1;
            // Check execution state of commands
            else
                checkCommandsCanExecute();
        }

        #endregion

        #region Wizard Commands - Next, Back, Complete, Cancel

        private DelegateCommand nextCommand;
        /// <summary>
        /// Gets the command to advance to the next page of the wizard
        /// </summary>
        public ICommand Next
        {
            get
            {
                if (nextCommand == null)
                    nextCommand = new DelegateCommand(OnNext, CanExecuteNext);
                return nextCommand;
            }
        }

        private DelegateCommand backCommand;
        /// <summary>
        /// Gets the command to advance to the previous page of the wizard
        /// </summary>
        public ICommand Back
        {
            get
            {
                if (backCommand == null)
                    backCommand = new DelegateCommand(OnBack, CanExecuteBack);
                return backCommand;
            }
        }

        private DelegateCommand completeCommand;
        /// <summary>
        /// Gets the command for completing the wizard.  Raises the Completed event.
        /// </summary>
        public ICommand Complete
        {
            get
            {
                if (completeCommand == null)
                    completeCommand = new DelegateCommand(OnComplete, CanComplete);
                return completeCommand;
            }
        }

        private DelegateCommand cancelCommand;
        /// <summary>
        /// Gets the command for cancelling the wizard.  Raises the Cancelled event.
        /// </summary>
        public ICommand Cancel
        {
            get
            {
                if (cancelCommand == null)
                    cancelCommand = new DelegateCommand(OnCancel, (o) => { return true; });
                return cancelCommand;
            }
        }

        #endregion

        #region Dimension Properties (widths and heights)

        /// <summary>
        /// Backing dependency property for ContentWidth
        /// </summary>
        public static readonly DependencyProperty ContentWidthProperty = DependencyProperty.Register(
            "ContentWidth", typeof(double), typeof(Wizard), new PropertyMetadata(double.NaN));

        /// <summary>
        /// Gets or sets the width of wizard page content
        /// </summary>
        public double ContentWidth
        {
            get { return (double)GetValue(ContentWidthProperty); }
            set { SetValue(ContentWidthProperty, value); }
        }

        /// <summary>
        /// Backing dependency property for ContentHeight
        /// </summary>
        public static readonly DependencyProperty ContentHeightProperty = DependencyProperty.Register(
            "ContentHeight", typeof(double), typeof(Wizard), new PropertyMetadata(double.NaN));

        /// <summary>
        /// Gets or sets the height of wizard page content
        /// </summary>
        public double ContentHeight
        {
            get { return (double)GetValue(ContentHeightProperty); }
            set { SetValue(ContentHeightProperty, value); }
        }

        /// <summary>
        /// Backing dependency property for HeadingWidth
        /// </summary>
        public static readonly DependencyProperty HeadingWidthProperty = DependencyProperty.Register(
            "HeadingWidth", typeof(double), typeof(Wizard), new PropertyMetadata(double.NaN));

        /// <summary>
        /// Gets or sets the width of wizard page headings
        /// </summary>
        public double HeadingWidth
        {
            get { return (double)GetValue(HeadingWidthProperty); }
            set { SetValue(HeadingWidthProperty, value); }
        }

        /// <summary>
        /// Backing dependency property for HeadingHeight
        /// </summary>
        public static readonly DependencyProperty HeadingHeightProperty = DependencyProperty.Register(
            "HeadingHeight", typeof(double), typeof(Wizard), new PropertyMetadata(double.NaN));

        /// <summary>
        /// Gets or sets the height of wizard page headings
        /// </summary>
        public double HeadingHeight
        {
            get { return (double)GetValue(HeadingHeightProperty); }
            set { SetValue(HeadingHeightProperty, value); }
        }

        /// <summary>
        /// Backing dependency property for DescriptionWidth
        /// </summary>
        public static readonly DependencyProperty DescriptionWidthProperty = DependencyProperty.Register(
            "DescriptionWidth", typeof(double), typeof(Wizard), new PropertyMetadata(double.NaN));

        /// <summary>
        /// Gets or sets the width of wizard page descriptions
        /// </summary>
        public double DescriptionWidth
        {
            get { return (double)GetValue(DescriptionWidthProperty); }
            set { SetValue(DescriptionWidthProperty, value); }
        }

        /// <summary>
        /// Backing dependency property for DescriptionHeight
        /// </summary>
        public static readonly DependencyProperty DescriptionHeightProperty = DependencyProperty.Register(
            "DescriptionHeight", typeof(double), typeof(Wizard), new PropertyMetadata(double.NaN));

        /// <summary>
        /// Gets or sets the height of wizard page descriptions
        /// </summary>
        public double DescriptionHeight
        {
            get { return (double)GetValue(DescriptionHeightProperty); }
            set { SetValue(DescriptionHeightProperty, value); }
        }

        #endregion

        #endregion

        #region Events - PageChanged, Completed, Cancelled

        /// <summary>
        /// Event for performing validation on page change.  Raised before the wizard's CurrentPage 
        /// has been updated
        /// </summary>
        public event EventHandler<CancelEventArgs> PageChanging;

        /// <summary>
        /// Event for tracking page changes.  Raised after the wizard's CurrentPage has been updated
        /// </summary>
        public event EventHandler<EventArgs> PageChanged;

        /// <summary>
        /// Event for handling wizard completion.  Raised by the Complete command.
        /// </summary>
        public event EventHandler<EventArgs> Completed;

        /// <summary>
        /// Event for handling wizard cancellation.  Raised by the Cancel command.
        /// </summary>
        public event EventHandler<EventArgs> Cancelled;

        #endregion

        #region Command Handling - Next, Back, Complete, Cancel

        // Move to next page
        private void OnNext(object commandParameter)
        {
            // Fire page changing event
            CancelEventArgs args = new CancelEventArgs(false);
            if (PageChanging != null)
                PageChanging(this, args);

            // Make sure page change was not cancelled
            if (!args.Cancel)
                CurrentPage = Pages[getPageIndex(CurrentPage) + 1];
        }

        private bool CanExecuteNext(object commandParameter)
        {
            // Can go to next page only if current page's input is valid and current page is not last
            return CurrentPage != null && CurrentPage.InputValid && getPageIndex(CurrentPage) != Pages.Count - 1;
        }

        // Move to previous page
        private void OnBack(object commandParameter)
        {
            // Fire page changing event
            CancelEventArgs args = new CancelEventArgs(false);
            if (PageChanging != null)
                PageChanging(this, args);

            // Make sure page change was not cancelled
            if (!args.Cancel)
                CurrentPage = Pages[getPageIndex(CurrentPage) - 1];
        }

        private bool CanExecuteBack(object CommandParameter)
        {
            // Can go to previous page only if current page is not first
            return getPageIndex(CurrentPage) > 0;
        }

        // Raise completed event
        private void OnComplete(object commandParameter)
        {
            // Fire page changing event
            CancelEventArgs args = new CancelEventArgs(false);
            if (PageChanging != null)
                PageChanging(this, args);

            // Make sure page change was not cancelled and reset current page to 
            // release elements from visual tree
            if (!args.Cancel)
                CurrentPage = null;

            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }

        private bool CanComplete(object commandParameter)
        {
            // Wizard can be completed only if input of all pages is valid
            foreach (WizardPage page in Pages)
            {
                if (!page.InputValid)
                    return false;
            }
            return Pages.Count > 0;
        }

        // Raise cancelled event
        private void OnCancel(object commandParameter)
        {
            // Reset current page to release elements from visual tree
            CurrentPage = null;

            if (Cancelled != null)
                Cancelled(this, EventArgs.Empty);
        }

        #endregion

        private int getPageIndex(WizardPage page)
        {
            int index = -1;
            if (page != null)
            {
                for (int i = 0; i < Pages.Count; i++)
                {
                    if (page.Equals(Pages[i]))
                    {
                        index = i;
                        break;
                    }
                }
            }

            return index;
        }

        private void updatePage(int newPageIndex, int oldPageIndex)
        {
            if (newPageIndex > -1)
            {
                WizardPage newPage = Pages[newPageIndex];

                // Register for change notification on the InputValid property of the new wizard page
                List<ChangeNotification> notifications = DependencyPropertyHelper.GetChangeNotifications(this);
                bool notificationRegistered = false;
                if (notifications != null)
                {
                    foreach (ChangeNotification notification in notifications)
                    {
                        if (notification.Source.Equals(newPage) && notification.Callback == InputValid_PropertyChanged
                        && notification.PropertyName == "InputValid")
                        {
                            notificationRegistered = true;
                            break;
                        }
                    }
                }

                if (!notificationRegistered)
                {
                    ChangeNotification notification = new ChangeNotification()
                    {
                        Source = newPage,
                        PropertyName = "InputValid",
                        Target = this,
                        Callback = InputValid_PropertyChanged
                    };
                    DependencyPropertyHelper.RegisterForChangeNotification(notification);
                }

                checkCommandsCanExecute();
            }

            CurrentPageIndex = newPageIndex;

            if (PageChanged != null)
                PageChanged(this, EventArgs.Empty);
        }

        private void checkCommandsCanExecute()
        {
            if (nextCommand == null)
                nextCommand = new DelegateCommand(OnNext, CanExecuteNext);
            nextCommand.RaiseCanExecuteChanged();

            if (backCommand == null)
                backCommand = new DelegateCommand(OnBack, CanExecuteBack);
            backCommand.RaiseCanExecuteChanged();

            if (completeCommand == null)
                completeCommand = new DelegateCommand(OnComplete, CanComplete);
            completeCommand.RaiseCanExecuteChanged();
        }
    }
}
