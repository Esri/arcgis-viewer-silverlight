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
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Client.Application.Controls;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class PublishApplicationCommand : DependencyObject, ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        SiteDetailsDialogControl siteDetailsDialogControl;
        public void Execute(object parameter)
        {
            if (ParentDropDownButton != null)
                ParentDropDownButton.IsContentPopupOpen = false;

            if (siteDetailsDialogControl == null)
                siteDetailsDialogControl = new SiteDetailsDialogControl();

            bool isSaveAs = false;
            if (parameter != null)
                isSaveAs = (bool)parameter;

            if (isSaveAs)
                BuilderApplication.Instance.ShowWindow(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.SaveAs, siteDetailsDialogControl, true);
            else
                BuilderApplication.Instance.ShowWindow(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.PublishApplication, siteDetailsDialogControl, true);
        }

        protected void OnCanExecuteChanged(EventArgs e)
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, e);
        }        

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
                typeof(PublishApplicationCommand),
                new PropertyMetadata(null));
        #endregion
    }
}
