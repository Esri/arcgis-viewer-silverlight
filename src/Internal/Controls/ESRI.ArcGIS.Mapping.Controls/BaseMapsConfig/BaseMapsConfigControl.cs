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
using ESRI.ArcGIS.Mapping.Core;
using System.Collections.ObjectModel;
using System.Linq;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class BaseMapsConfigControl : Control
    {
        internal ListBox BaseMapsList { get; private set; }
        private const string BASEMAPSLIST = "BaseMapsList";

        public BaseMapsConfigControl()
        {
            DefaultStyleKey = typeof(BaseMapsConfigControl);
            AddNewBaseMapCommand = new DelegateCommand(onAddNewBaseMapCommand);
            DeleteBaseMapCommand = new DelegateCommand(onDeleteBaseMapCommand, canDeleteBaseMapCommand);
            MoveBaseMapUpCommand = new DelegateCommand(onMoveBaseMapUpCommand, canMoveBaseMapUpCommand);
            MoveBaseMapDownCommand = new DelegateCommand(onMoveBaseMapDownCommand, canMoveBaseMapDownCommand);
        }

        #region BingMapsAppID
        public string BingMapsAppID
        {
            get { return (string)GetValue(BingMapsAppIDProperty); }
            set { SetValue(BingMapsAppIDProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BingMapsAppID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BingMapsAppIDProperty =
            DependencyProperty.Register("BingMapsAppID", typeof(string), typeof(BaseMapsConfigControl), new PropertyMetadata(OnBaseMapsPropertyChanged));
        #endregion

        #region BaseMaps
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<BaseMapInfo> BaseMaps
        {
            get { return GetValue(BaseMapsProperty) as ObservableCollection<BaseMapInfo>; }
            set { SetValue(BaseMapsProperty, value); }
        }

        /// <summary>
        /// Identifies the BaseMaps dependency property.
        /// </summary>
        public static readonly DependencyProperty BaseMapsProperty =
            DependencyProperty.Register(
                "BaseMaps",
                typeof(ObservableCollection<BaseMapInfo>),
                typeof(BaseMapsConfigControl),
                new PropertyMetadata(null, OnBaseMapsPropertyChanged));

        /// <summary>
        /// BaseMapsProperty property changed handler.
        /// </summary>
        /// <param name="d">BaseMapsConfigControl that changed its BaseMaps.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnBaseMapsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BaseMapsConfigControl source = d as BaseMapsConfigControl;
            source.setItemsSourceAndSelectedItemForBaseMapsList();
        }

        private void setItemsSourceAndSelectedItemForBaseMapsList()
        {
            if (BaseMapsList != null)
            {
                if (BaseMaps != null)
                {
                    BaseMapsList.ItemsSource = BaseMaps;
                    if (BaseMaps.Count > 0)
                    {
                        BaseMapsList.SelectedIndex = 0;
                    }
                    foreach (BaseMapInfo baseMapInfo in BaseMaps)
                    {
                        baseMapInfo.PropertyChanged -= baseMapInfo_PropertyChanged;
                        baseMapInfo.PropertyChanged += baseMapInfo_PropertyChanged;
                        baseMapInfo.BingMapsAppID = BingMapsAppID;
                    }
                }
            }
            refreshCommands();
        }

        void baseMapInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            BaseMapInfo baseMap = sender as BaseMapInfo;
            if (e.PropertyName == "BaseMapType")
            {
                BaseMapType _baseMapType = baseMap.BaseMapType;
                if (_baseMapType == Core.BaseMapType.BingMaps)
                {
                    baseMap.Url = "http://dev.virtualearth.net/webservices/v1/imageryservice/imageryservice.svc";
                    baseMap.Name = "Roads";
                }
                else if (_baseMapType == Core.BaseMapType.OpenStreetMap)
                {
                    baseMap.Url = "http://OpenStreetMap.org";
                    baseMap.Name = "Mapnik";
                }
                else
                {
                    baseMap.Url = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer";                    
                    // Name is not important for AGS Server layers
                }
            }
        }
        #endregion 

        #region Description
        /// <summary>
        /// 
        /// </summary>
        public string Description
        {
            get { return GetValue(DescriptionProperty) as string; }
            set { SetValue(DescriptionProperty, value); }
        }

        /// <summary>
        /// Identifies the Description dependency property.
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(
                "Description",
                typeof(string),
                typeof(BaseMapsConfigControl),
                new PropertyMetadata(null));
        #endregion

        #region OkButtonText
        /// <summary>
        /// 
        /// </summary>
        public string OkButtonText
        {
            get { return GetValue(OkButtonTextProperty) as string; }
            set { SetValue(OkButtonTextProperty, value); }
        }

        /// <summary>
        /// Identifies the OkButtonText dependency property.
        /// </summary>
        public static readonly DependencyProperty OkButtonTextProperty =
            DependencyProperty.Register(
                "OkButtonText",
                typeof(string),
                typeof(BaseMapsConfigControl),
                new PropertyMetadata(null));
        #endregion 

        #region OkButtonCommand
        /// <summary>
        /// 
        /// </summary>
        public ICommand OkButtonCommand
        {
            get { return GetValue(OkButtonCommandProperty) as ICommand; }
            set { SetValue(OkButtonCommandProperty, value); }
        }

        /// <summary>
        /// Identifies the OkButtonCommand dependency property.
        /// </summary>
        public static readonly DependencyProperty OkButtonCommandProperty =
            DependencyProperty.Register(
                "OkButtonCommand",
                typeof(ICommand),
                typeof(BaseMapsConfigControl),
                new PropertyMetadata(null));
        #endregion

        public double ScrollViewerHeight { get; set; }

        public DelegateCommand AddNewBaseMapCommand { get; private set; }
        public DelegateCommand DeleteBaseMapCommand { get; private set; }
        public DelegateCommand MoveBaseMapUpCommand { get; private set; }
        public DelegateCommand MoveBaseMapDownCommand { get; private set; }

        public override void OnApplyTemplate()
        {
            if(BaseMapsList != null)
                BaseMapsList.SelectionChanged += BaseMapsList_SelectionChanged;
            base.OnApplyTemplate();

            BaseMapsList = GetTemplateChild(BASEMAPSLIST) as ListBox;
            if (BaseMapsList != null)
                BaseMapsList.SelectionChanged += BaseMapsList_SelectionChanged;            

            setItemsSourceAndSelectedItemForBaseMapsList();
        }

        void BaseMapsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            refreshCommands();
        }

        private void onAddNewBaseMapCommand(object parameter)
        {
            if (BaseMaps == null || BaseMapsList == null)
                return;

            BaseMapInfo newBaseMap = null;
           // Clone current selected base map
            BaseMapInfo currentSelectedBaseMap = BaseMapsList.SelectedItem as BaseMapInfo;
            if (currentSelectedBaseMap == null)
                currentSelectedBaseMap = BaseMaps.FirstOrDefault();
            if (currentSelectedBaseMap != null)
            {
                newBaseMap = new BaseMapInfo()
                {
                    BaseMapType = currentSelectedBaseMap.BaseMapType,
					DisplayName = ESRI.ArcGIS.Mapping.Controls.Resources.Strings.NewBasemap,
                    ThumbnailImage = currentSelectedBaseMap.ThumbnailImage,
                    BingMapsAppID = BingMapsAppID
                };
                if (currentSelectedBaseMap.BaseMapType == BaseMapType.BingMaps
                    || currentSelectedBaseMap.BaseMapType == BaseMapType.OpenStreetMap)
                {
                    // Keep same name (needed)
                    newBaseMap.Name = currentSelectedBaseMap.Name;
                    newBaseMap.Url = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer";
                }
                else
                {
                    newBaseMap.Name = Guid.NewGuid().ToString("N"); // needs to be unique for AGS
                    newBaseMap.Url = currentSelectedBaseMap.Url;
                }
            }
            else
            {
                newBaseMap = new BaseMapInfo()
                {
                    BaseMapType = BaseMapType.ArcGISServer,
                    Name = Guid.NewGuid().ToString("N"), // needs to be unique for AGS
					DisplayName = ESRI.ArcGIS.Mapping.Controls.Resources.Strings.NewBasemap,
                    ThumbnailImage = "Images/basemaps/agol_imagery.png",
                    Url = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer",
                };
            }
            BaseMaps.Add(newBaseMap);
            BaseMapsList.SelectedItem = newBaseMap;
        }       

        private void onDeleteBaseMapCommand(object parameter)
        {
			MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.AreYouSureYouWantToDeleteBasemap, ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ConfirmDelete, MessageBoxButton.OKCancel, deleteSelectedBaseMap, true);
        }

        private void deleteSelectedBaseMap(object sender, MessageBoxClosedArgs e)
        {
            if (e.Result != MessageBoxResult.OK)
                return;

            BaseMapInfo currentSelectedBaseMap = BaseMapsList.SelectedItem as BaseMapInfo;
            if (currentSelectedBaseMap == null)
                return;

            int currIndex = BaseMaps.IndexOf(currentSelectedBaseMap);
            BaseMaps.Remove(currentSelectedBaseMap);
            if (currIndex > BaseMaps.Count - 1) // if last item is being deleted
                currIndex = BaseMaps.Count - 1;

            if (currIndex > -1) // select the next item in the list
                BaseMapsList.SelectedIndex = currIndex;
        }

        private bool canDeleteBaseMapCommand(object parameter)
        {
            return BaseMapsList != null && BaseMapsList.SelectedItem != null;
        }


        private void onMoveBaseMapUpCommand(object parameter)
        {
            BaseMapInfo currentSelectedBaseMap = BaseMapsList.SelectedItem as BaseMapInfo;
            if (currentSelectedBaseMap == null)
                return;

            int pos = BaseMaps.IndexOf(currentSelectedBaseMap);
            if (pos < 1) 
                return;
            BaseMaps.RemoveAt(pos);
            BaseMaps.Insert(pos - 1, currentSelectedBaseMap);

            // Preserve selection
            BaseMapsList.SelectedIndex = pos-1;

            refreshMoveCommands();
        }

        private bool canMoveBaseMapUpCommand(object parameter)
        {
            return BaseMapsList != null && BaseMapsList.SelectedIndex > 0;
        }

        private void onMoveBaseMapDownCommand(object parameter)
        {
            BaseMapInfo currentSelectedBaseMap = BaseMapsList.SelectedItem as BaseMapInfo;
            if (currentSelectedBaseMap == null)
                return;

            int pos = BaseMaps.IndexOf(currentSelectedBaseMap);
            if (pos < 0 || pos >= BaseMaps.Count)
                return;
            BaseMaps.RemoveAt(pos);
            BaseMaps.Insert(pos + 1, currentSelectedBaseMap);

            // Preserve selection
            BaseMapsList.SelectedIndex = pos + 1;

            refreshMoveCommands();
        }

        private bool canMoveBaseMapDownCommand(object parameter)
        {
            return BaseMaps != null && BaseMapsList != null && BaseMapsList.SelectedIndex >= 0 && BaseMapsList.SelectedIndex < BaseMaps.Count - 1;
        }

        void refreshCommands()
        {
            AddNewBaseMapCommand.RaiseCanExecuteChanged();
            DeleteBaseMapCommand.RaiseCanExecuteChanged();
            refreshMoveCommands();
        }

        private void refreshMoveCommands()
        {
            MoveBaseMapDownCommand.RaiseCanExecuteChanged();
            MoveBaseMapUpCommand.RaiseCanExecuteChanged();
        }
    }
}
