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
using System.IO;
using System.Text;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class FileSaveCommand : DependencyObject, ICommand
    {
        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return View != null;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (View == null)
                return;

            try
            {
                ESRI.ArcGIS.Mapping.Core.MapDocumentUserPersistance.SaveUserDocument(View.GetMapConfiguration(null));
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                MessageBoxDialog.Show("Error saving File"
#if DEBUG
 + ": " + err
#endif
); ;
            }
        }

        #endregion

        protected void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
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
                typeof(FileSaveCommand),
                new PropertyMetadata(null, OnViewPropertyChanged));

        /// <summary>
        /// ViewProperty property changed handler.
        /// </summary>
        /// <param name="d">FileSaveAsCommand that changed its View.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnViewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FileSaveCommand source = d as FileSaveCommand;
            source.OnViewChanged();
        }
        #endregion 

        private void OnViewChanged()
        {
            OnCanExecuteChanged();
        }
    }
}
