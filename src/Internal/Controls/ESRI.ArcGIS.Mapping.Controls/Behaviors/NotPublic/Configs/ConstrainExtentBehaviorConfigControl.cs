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
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ConstrainExtentBehaviorConfigControl : Control
    {
        public ConstrainExtentBehaviorConfigControl()
        {
            this.DefaultStyleKey = typeof(ConstrainExtentBehaviorConfigControl);
            CloseCommand = new DelegateCommand(ConfigurationDone);
            ApplyCommand = new DelegateCommand(ConfigurationApplied);
        }

        public Envelope ConstrainedExtent
        {
            get { return GetValue(ConstrainedExtentProperty) as Envelope; }
            set { SetValue(ConstrainedExtentProperty, value); }
        }

        public static readonly DependencyProperty ConstrainedExtentProperty =
                DependencyProperty.Register("ConstrainedExtent", typeof(Envelope), typeof(ConstrainExtentBehaviorConfigControl), new PropertyMetadata(null));

        #region Close Command
        private void ConfigurationDone(object commandParameter)
        {
            MapApplication.Current.HideWindow(this);
        }

        public ICommand CloseCommand
        {
            get { return (ICommand)GetValue(CloseCommandProperty); }
            private set { SetValue(CloseCommandProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Cancel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register("CloseCommand", typeof(ICommand), typeof(ConstrainExtentBehaviorConfigControl), null);
        #endregion

        #region Apply Command
        private void ConfigurationApplied(object commandParameter)
        {
            ConstrainedExtent = MapApplication.Current.Map.Extent;
        }

        public ICommand ApplyCommand
        {
            get { return (ICommand)GetValue(ApplyCommandProperty); }
            private set { SetValue(ApplyCommandProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Cancel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ApplyCommandProperty =
            DependencyProperty.Register("ApplyCommand", typeof(ICommand), typeof(ConstrainExtentBehaviorConfigControl), new PropertyMetadata(null));
        #endregion
        
    }
}
