/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using ESRI.ArcGIS.Client.Application.Controls;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class LayerConfiguration : Control, INotifyPropertyChanged
    {
        Accordion LayerConfigurationAccordion;
        public LayerConfiguration()
        {
            this.DefaultStyleKey = typeof(LayerConfiguration);
        }

        private View _view;
        public View View
        {
            get
            {
                return _view;
            }
            set
            {
                _view = value;
                OnPropertyChanged("View");
            }
        }

        #region PopupContentContainerStyle
        public Style PopupContentContainerStyle
        {
            get { return GetValue(PopupContentContainerStyleProperty) as Style; }
            set { SetValue(PopupContentContainerStyleProperty, value); }
        }
        public static readonly DependencyProperty PopupContentContainerStyleProperty =
                    DependencyProperty.Register(
                    "PopupContentContainerStyle",
                    typeof(Style),
                    typeof(LayerConfiguration),
                    new PropertyMetadata(layoutStyleHelperPopupContainer));

        private static Style layoutStyleHelperPopupContainer
        {
            get
            {
                return LayoutStyleHelper.Instance.GetPopupStyle("LayerConfiguration", PopupStyleName.PopupContentControl);
            }
        }
        #endregion

        #region PopupLeaderStyle
        public Style PopupLeaderStyle
        {
            get { return GetValue(PopupLeaderStyleProperty) as Style; }
            set { SetValue(PopupLeaderStyleProperty, value); }
        }
        public static readonly DependencyProperty PopupLeaderStyleProperty =
                    DependencyProperty.Register(
                    "PopupLeaderStyle",
                    typeof(Style),
                    typeof(LayerConfiguration),
                    new PropertyMetadata(layoutStyleHelperPopupLeader));

        private static Style layoutStyleHelperPopupLeader
        {
            get
            {
                return LayoutStyleHelper.Instance.GetPopupStyle("LayerConfiguration", PopupStyleName.PopupLeader);
            }
        }
        #endregion         

        public override void OnApplyTemplate()
        {
            LayerConfigurationAccordion = GetTemplateChild("LayerConfigurationAccordion") as Accordion;
        }

        private void refreshButtonCommands()
        {
            if (LayerConfigurationAccordion == null)
                return;
            List<ButtonBase> customButtons = ControlTreeHelper.FindChildrenOfType<ButtonBase>(LayerConfigurationAccordion as DependencyObject, 12);
            if (customButtons != null)
            {
                foreach (ButtonBase btn in customButtons)
                {
                    ICommand cmd = btn.Command;
                    if (cmd != null)
                    {
                        cmd.CanExecuteChanged -= cmd_CanExecuteChanged;
                        cmd.CanExecuteChanged += cmd_CanExecuteChanged;
                        btn.Command = null;
                        btn.Command = cmd;
                        ToggleButton toggleButton = btn as ToggleButton;
                        if (toggleButton != null)
                        {
                            IToggleCommand toggleButtonCmd = cmd as IToggleCommand;
                            if (toggleButtonCmd != null)
                                toggleButton.IsChecked = toggleButtonCmd.IsChecked();
                        }
                    }
                }
            }
        }
        void cmd_CanExecuteChanged(object sender, EventArgs e)
        {
            refreshButtonCommands();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
