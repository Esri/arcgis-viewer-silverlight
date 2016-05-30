/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client.Extensibility;
using System.Windows;
using System.Windows.Input;
using System;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class DisplayRichTextDialogCommand : DependencyObject, ICommand
    {
        public void Execute(object parameter)
        {
            MapApplication.Current.ShowWindow(DialogTitle, new RichTextDialogControl() { RichTextXaml = RichTextXaml, Width = DialogWidth, Height = DialogHeight });
        }

        public string DialogTitle { get; set; }
        public double DialogWidth { get; set; }
        public double DialogHeight { get; set; }

        #region RichTextXaml
        /// <summary>
        /// 
        /// </summary>
        public string RichTextXaml
        {
            get { return GetValue(RichTextXamlProperty) as string; }
            set { SetValue(RichTextXamlProperty, value); }
        }

        /// <summary>
        /// Identifies the RichTextXaml dependency property.
        /// </summary>
        public static readonly DependencyProperty RichTextXamlProperty =
            DependencyProperty.Register(
                "RichTextXaml",
                typeof(string),
                typeof(DisplayRichTextDialogCommand),
                new PropertyMetadata(null));
        #endregion

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event System.EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged(EventArgs args)
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, args);
        }
    }
}
