/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class MoveIntoGroupDialog : UserControl
    {
        bool IsMoveDown;
        string GroupName;
        public MoveIntoGroupClosedEventHandler OnClosedHandler { get; private set; }

        public MoveIntoGroupDialog(bool isMoveDown, string groupName, MoveIntoGroupClosedEventHandler onClosedHandler)
        {
            InitializeComponent();

            IsMoveDown = isMoveDown;
            GroupName = groupName;
            OnClosedHandler = onClosedHandler;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            OnClosedHandler.Invoke(this, new MoveIntoGroupEventArgs() { MoveAroundGroup = rbMoveIntoGroup.IsChecked == true ? false : true });
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsMoveDown)
            {
                rbMoveUp.Visibility = System.Windows.Visibility.Collapsed;
                rbMoveDown.Visibility = System.Windows.Visibility.Visible;
                rbMoveDown.IsChecked = true;
            }
            else
            {
                rbMoveDown.Visibility = System.Windows.Visibility.Collapsed;
                rbMoveUp.Visibility = System.Windows.Visibility.Visible;
                rbMoveUp.IsChecked = true;
            }

            rbMoveIntoText.Text = string.Format(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.MoveIntoGroup, GroupName);
        }
    }

    public delegate void MoveIntoGroupClosedEventHandler(object sender, MoveIntoGroupEventArgs e);

    public class MoveIntoGroupEventArgs : EventArgs
    {
        public bool MoveAroundGroup { get; set; }
    }
}
