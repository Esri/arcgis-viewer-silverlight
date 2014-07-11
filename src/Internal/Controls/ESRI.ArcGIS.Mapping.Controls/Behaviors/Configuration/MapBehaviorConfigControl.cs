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
using ESRI.ArcGIS.Mapping.Core;
using System.ComponentModel.Composition;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Client.Extensibility;
using System.Windows.Data;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = "AvailableBehaviorItemsControl", Type = typeof(AvailableBehaviorItemsControl))]
    public class MapBehaviorConfigControl : Control, INotifyPropertyChanged
    {
        AvailableBehaviorItemsControl _availableBehaviors;
        public event EventHandler OkClicked;
        public event EventHandler CancelClicked;
        
        public MapBehaviorConfigControl()
        {
            this.DefaultStyleKey = typeof(MapBehaviorConfigControl);
            CancelCommand = new DelegateCommand(close);
            OKCommand = new DelegateCommand(ok);
            NextCommand = new DelegateCommand(next);
            MapBehaviors = new ObservableCollection<Behavior<Map>>();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _availableBehaviors = GetTemplateChild("AvailableBehaviorItemsControl") as AvailableBehaviorItemsControl;

            buildListOfAvailableBehaviors();  
        }

        private void buildListOfAvailableBehaviors()
        {
            AssemblyManager.AddAssembly(typeof(ESRI.ArcGIS.Mapping.Controls.ConstrainExtentBehavior).Assembly);

            MapBehaviors.Clear();
            IEnumerable<Type> exportedBehaviors = AssemblyManager.GetExportsForType(typeof(Behavior<Map>));
            if (exportedBehaviors != null)
            {
                foreach (Type type in exportedBehaviors)
                {
                    Behavior<Map> behavior = Activator.CreateInstance(type) as Behavior<Map>;
                    if (behavior != null)
                        MapBehaviors.Add(behavior);
                }
            }
        }

        internal void Refresh()
        {
            buildListOfAvailableBehaviors();
            if (_availableBehaviors != null)
                _availableBehaviors.Refresh();
        }

        private ExtensionBehavior _extensionBehavior;
        public ExtensionBehavior ExtensionBehavior
        {
            get
            {
                return _extensionBehavior;
            }
            set
            {
                _extensionBehavior = value;
                OnPropertyChanged("ExtensionBehavior");
            }
        }

        public ObservableCollection<Behavior<Map>> MapBehaviors { get; private set;}

        private void applyBehavior()
        {
            if (ExtensionBehavior == null || ExtensionBehavior.MapBehavior == null)
                return;

            ExtensionBehavior.CommandValueId = string.Format("Type={0};Assembly={1};IsCustomBehavior=true", ExtensionBehavior.MapBehavior.GetType().FullName, ExtensionBehavior.MapBehavior.GetType().Assembly.FullName);
            if (ExtensionBehavior.MapBehavior is ISupportsConfiguration)
                ExtensionBehavior.CommandValueId += ";SupportsConfiguration=true";
        }

        private bool ValidateBehavior()
        {
            if (string.IsNullOrWhiteSpace(ExtensionBehavior.Title))
            {
                MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.PleaseProvideValidTitleForNewBehavior, ESRI.ArcGIS.Mapping.Controls.Resources.Strings.NoBehaviorTitleSpecified, MessageBoxButton.OK, null, true);
                return false;
            }

            return true;
        }

        #region Cancel Command
        private void close(object commandParameter)
        {
            if (CancelClicked != null)
                CancelClicked(this, EventArgs.Empty);
        }

        public ICommand CancelCommand
        {
            get { return (ICommand)GetValue(CancelCommandProperty); }
            private set { SetValue(CancelCommandProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Cancel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register("CancelCommand", typeof(ICommand), typeof(MapBehaviorConfigControl), null);
        #endregion

        #region OK Command
        private void ok(object commandParameter)
        {
            if (ExtensionBehavior == null)
                return;

            applyBehavior();

            if (!ValidateBehavior())
                return;

            if (OkClicked != null)
                OkClicked(this, EventArgs.Empty);
        }

        public ICommand OKCommand
        {
            get { return (ICommand)GetValue(OKCommandProperty); }
            private set { SetValue(OKCommandProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Cancel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OKCommandProperty =
            DependencyProperty.Register("OKCommand", typeof(ICommand), typeof(MapBehaviorConfigControl), null);
        #endregion

        #region Next Command
        private void next(object commandParameter)
        {
            TypeSelectionVisibility = Visibility.Collapsed;
            ExtensionBehavior = _availableBehaviors.SelectedItem;
            _availableBehaviors.SelectedItem = null;
            _availableBehaviors.Refresh();
        }

        public ICommand NextCommand
        {
            get { return (ICommand)GetValue(NextCommandProperty); }
            private set { SetValue(NextCommandProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Cancel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NextCommandProperty =
            DependencyProperty.Register("NextCommand", typeof(ICommand), typeof(MapBehaviorConfigControl), null);
        #endregion

        #region TypeSelection Visibility Property
        // Using a DependencyProperty as the backing store for TypeSelection Visibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TypeSelectionVisibilityProperty =
            DependencyProperty.Register("TypeSelectionVisibility", typeof(Visibility), typeof(MapBehaviorConfigControl), new PropertyMetadata(Visibility.Visible));

        public Visibility TypeSelectionVisibility
        {
            get { return (Visibility)GetValue(TypeSelectionVisibilityProperty); }
            set { SetValue(TypeSelectionVisibilityProperty, value); }
        }
        #endregion

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
