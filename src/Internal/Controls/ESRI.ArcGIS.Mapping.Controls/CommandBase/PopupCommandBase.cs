/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    /// <summary>
    /// Base class for Editor Commands used in Identify Popup
    /// </summary>
    public class PopupCommandBase : DependencyObject, ICommand
    {
        protected virtual void RefreshCommand()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        #region PopupInfo

        /// <summary>
        /// PopupInfo associated with the Command that contains the PopupItem
        /// </summary>
        public ESRI.ArcGIS.Client.Extensibility.PopupInfo PopupInfo
        {
            get { return (ESRI.ArcGIS.Client.Extensibility.PopupInfo)GetValue(PopupInfoProperty); }
            set { SetValue(PopupInfoProperty, value); }
        }

        /// <summary>
        /// PopupInfo Dependency Property
        /// </summary>
        public static readonly DependencyProperty PopupInfoProperty =
            DependencyProperty.Register("PopupInfo", typeof(ESRI.ArcGIS.Client.Extensibility.PopupInfo), typeof(PopupCommandBase),
                new PropertyMetadata((ESRI.ArcGIS.Client.Extensibility.PopupInfo)null,
                    new PropertyChangedCallback(OnPopupInfoChanged)));

        /// <summary>
        /// Handles changes to the PopupInfo property.
        /// </summary>
        private static void OnPopupInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PopupCommandBase cmd = (PopupCommandBase)d;
            ESRI.ArcGIS.Client.Extensibility.PopupInfo oldPopupInfo = (ESRI.ArcGIS.Client.Extensibility.PopupInfo)e.OldValue;
            ESRI.ArcGIS.Client.Extensibility.PopupInfo newPopupInfo = cmd.PopupInfo;
            if (newPopupInfo != oldPopupInfo)
                cmd.OnPopupInfoChanged(oldPopupInfo, newPopupInfo);
        }

        /// <summary>
        /// Normally the popupinfo changes every time a new popup is displayed.  Each time, we bind the 
        /// current PopupItem to our dependency property so that if it changes, we can fire a CanExecuteChanged 
        /// event on our command
        /// </summary>
        protected virtual void OnPopupInfoChanged(ESRI.ArcGIS.Client.Extensibility.PopupInfo oldPopupInfo, ESRI.ArcGIS.Client.Extensibility.PopupInfo newPopupInfo)
        {
            // remove existing binding
            ClearValue(PopupItemProperty);
            if (newPopupInfo != null)
            {
                // Bind the popupinfo's popupItem to ours so that we can cause
                // the command to check its CanExecute state
                var b = new Binding("PopupItem") {Source = newPopupInfo};
                BindingOperations.SetBinding(this, PopupItemProperty, b);
            }
            RefreshCommand();
        }

        #endregion

        #region PopupItem

        public static readonly DependencyProperty PopupItemProperty =
            DependencyProperty.Register("PopupItem", typeof (PopupItem), typeof (PopupCommandBase),
                                        new PropertyMetadata(null,
                                                             OnPopupItemChanged));

        public PopupItem PopupItem
        {
            get { return (PopupItem) GetValue(PopupItemProperty); }
            set { SetValue(PopupItemProperty, value); }
        }

        private static void OnPopupItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (PopupCommandBase) d;
            var oldPopupItem = (PopupItem) e.OldValue;
            PopupItem newPopupItem = target.PopupItem;
            target.OnPopupItemChanged(oldPopupItem, newPopupItem);
        }

        protected virtual void OnPopupItemChanged(PopupItem oldPopupItem, PopupItem newPopupItem)
        {
            RefreshCommand();
        }

        #endregion

        #region ICommand Support

        public virtual  bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public virtual void Execute(object parameter)
        {
            
        }

        #endregion
    }
}
