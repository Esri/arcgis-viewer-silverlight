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
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Builder.ApplicationBuilder;
using ESRI.ArcGIS.Mapping.Builder.Controls;
using ESRI.ArcGIS.Client.Application.Controls;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class OpenApplicationsCommand : DependencyObject, ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            // If the builder is currently displaying the "build" page (as opposed to the start page or the map center
            // page) then confirm leaving this page and going to the start page before doing so.
            if (BuilderApplication.Instance.BuilderScreenVisibility == Visibility.Visible)
            {
                MessageBoxDialog.Show(Resources.Strings.AreYouSureYouWantToLeavePageUnsavedChangesLost, Resources.Strings.ConfirmPageChange, MessageBoxButton.OKCancel,
                    new MessageBoxClosedEventHandler(delegate(object obj, MessageBoxClosedArgs args)
                    {
                        if (args.Result == MessageBoxResult.OK)
                        {
                            ResetApplication();
                        }
                    }));

            }
            else
                ResetApplication();
        }

        private void ResetApplication()
        {
            // Remove all layers from the Map
            if (View.Instance != null) 
                View.Instance.Clear();

            if (View != null)
            {
                View.ProxyUrl = View.DefaultProxyUrl;
                if (View.WindowManager != null)
                    View.WindowManager.HideAllWindows();
            }

            if (ViewerApplicationControl.Instance != null)
                ViewerApplicationControl.Instance.ViewerApplication = null;

            if (BuilderApplication.Instance != null)
            {
                if (BuilderApplication.Instance.WindowManager != null)
                    BuilderApplication.Instance.WindowManager.HideAllWindows();
                if (BuilderApplication.Instance.AddContentDialog != null)
                    BuilderApplication.Instance.AddContentDialog.Reset();
            }
            
            // Get global instance of the getting started control and display it if getting started mode is not disabled
            TutorialDialogControl tdc = BuilderApplication.Instance.TutorialDialogControl;
            BuilderApplication.Instance.GettingStartedVisibility = tdc.IsTutorialDisabled == false ? Visibility.Visible : Visibility.Collapsed;

            if (ParentDropDownButton != null)
                ParentDropDownButton.IsContentPopupOpen = false;

            BuilderApplication.Instance.BuilderScreenVisibility = System.Windows.Visibility.Collapsed;
            BuilderApplication.Instance.NewappScreenVisibility = System.Windows.Visibility.Collapsed;
            BuilderApplication.Instance.CatalogScreenVisibility = System.Windows.Visibility.Visible;
        }

        protected void OnCanExecuteChanged(EventArgs e)
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, e);
        }

        #region View
        /// <summary>
        /// 
        /// </summary>
        public View View
        {
            get { return GetValue(ViewProperty) as View; }
            set { SetValue(ViewProperty, value); }
        }

        /// <summary>
        /// Identifies the View dependency property.
        /// </summary>
        public static readonly DependencyProperty ViewProperty =
            DependencyProperty.Register(
                "View",
                typeof(View),
                typeof(OpenApplicationsCommand),
                new PropertyMetadata(null));
        #endregion

        #region ParentDropDownButton
        /// <summary>
        /// 
        /// </summary>
        public DropDownButton ParentDropDownButton
        {
            get { return GetValue(ParentDropDownButtonProperty) as DropDownButton; }
            set { SetValue(ParentDropDownButtonProperty, value); }
        }

        /// <summary>
        /// Identifies the ParentDropDownButton dependency property.
        /// </summary>
        public static readonly DependencyProperty ParentDropDownButtonProperty =
            DependencyProperty.Register(
                "ParentDropDownButton",
                typeof(DropDownButton),
                typeof(OpenApplicationsCommand),
                new PropertyMetadata(null));
        #endregion
    }
}
