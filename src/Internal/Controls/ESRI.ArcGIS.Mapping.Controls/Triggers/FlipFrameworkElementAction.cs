/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
using System.Windows.Interactivity;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class FlipFrameworkElementsAction : TriggerAction<FrameworkElement>
    {
        #region Command DependencyProperty
        public FlipFrameworkElementsCommand Command
        {
            get { return (FlipFrameworkElementsCommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register
        (
            "Command",
            typeof(FlipFrameworkElementsCommand),
            typeof(FlipFrameworkElementsAction),
            new PropertyMetadata(null)
        );
        #endregion

        protected override void Invoke(object parameter)
        {
            if (Command != null && Command.CanExecute(null))
            {
                Command.Execute(null);
            }
        }
    }
}
