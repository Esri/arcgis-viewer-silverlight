/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = "MapBehaviorsDataGrid", Type = typeof(System.Windows.Controls.DataGrid))]
    public class MapBehaviorsList : Control
    {
        ExtensionBehavior selectedExtension = null;
        System.Windows.Controls.DataGrid MapBehaviorsDataGrid;

        public MapBehaviorsList()
        {
            this.DefaultStyleKey = typeof(MapBehaviorsList);
            Close = new DelegateCommand(close);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (ExtensionBehaviors != null)
            {
                if (selectedExtension == null && ExtensionBehaviors.Count > 0)
                    selectedExtension = ExtensionBehaviors[0];

                ExtensionBehaviors.CollectionChanged -= ExtensionBehaviors_CollectionChanged;
                ExtensionBehaviors.CollectionChanged += ExtensionBehaviors_CollectionChanged;
            }
            MapBehaviorsDataGrid = GetTemplateChild("MapBehaviorsDataGrid") as System.Windows.Controls.DataGrid;
            if (MapBehaviorsDataGrid != null)
            {
                MapBehaviorsDataGrid.SelectedItem = selectedExtension;
                MapBehaviorsDataGrid.LoadingRow -= MapBehaviorsDataGrid_LoadingRow;
                MapBehaviorsDataGrid.LoadingRow += MapBehaviorsDataGrid_LoadingRow;
            }
        }

        void ExtensionBehaviors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 0)
                selectedExtension = e.NewItems[0] as ExtensionBehavior;

            if (ExtensionBehaviors.Count == 0)
                selectedExtension = null;

            if (MapBehaviorsDataGrid != null && selectedExtension != null)
                MapBehaviorsDataGrid.SelectedItem = selectedExtension;
        }

        void MapBehaviorsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (MapBehaviorsDataGrid != null && MapBehaviorsDataGrid.Columns != null)
            {
                DataGridRow row = e.Row;
                if (row != null)
                {
                    Behavior<Map> behavior = (row.DataContext as ExtensionBehavior).MapBehavior;
                    if (behavior != null)
                    {
                        foreach (DataGridColumn column in MapBehaviorsDataGrid.Columns)
                        {
                            TextBlock element = column.GetCellContent(row) as TextBlock;
                            if (element != null)
                            {
                                element.SetValue(ToolTipService.ToolTipProperty, LocalizableStrings.GetString("TypeLabel") + " " + behavior.GetType().ToString());
                            }
                        }
                    }
                }
            }
        }

        #region IsEdit
        /// <summary>
        /// 
        /// </summary>
        public bool IsEdit
        {
            get { return (bool)GetValue(IsEditProperty); }
            set { SetValue(IsEditProperty, value); }
        }

        /// <summary>
        /// Identifies the View dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEditProperty =
            DependencyProperty.Register(
                "IsEdit",
                typeof(bool),
                typeof(MapBehaviorsList),
                new PropertyMetadata(false));
        #endregion     

        #region ExtensionBehaviors

        public ObservableCollection<ExtensionBehavior> ExtensionBehaviors
        {
            get
            {
                return View.Instance != null ? View.Instance.ExtensionBehaviors : null;
            }
        }

        #endregion

        public BehaviorConfiguration SelectedBehavior
        {
            get
            {
                if(MapBehaviorsDataGrid != null)
                    return MapBehaviorsDataGrid.SelectedItem as BehaviorConfiguration;

                return null;
            }
        }

        #region Cancel Command
        private void close(object commandParameter)
        {
            if(MapApplication.Current != null)
                MapApplication.Current.HideWindow(this);
        }

        public ICommand Close
        {
            get { return (ICommand)GetValue(CloseProperty); }
            private set { SetValue(CloseProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Cancel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CloseProperty =
            DependencyProperty.Register("Close", typeof(ICommand), typeof(MapBehaviorsList), null);
        #endregion
    }

}
