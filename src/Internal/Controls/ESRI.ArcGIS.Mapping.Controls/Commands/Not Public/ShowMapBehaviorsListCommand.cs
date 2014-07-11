/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;
using System.Windows;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ShowMapBehaviorsListCommand : CommandBase
    {
        #region IsEdit
        public static readonly DependencyProperty IsEditProperty =
            DependencyProperty.Register("IsEdit", typeof(bool),
                                        typeof(ShowMapBehaviorsListCommand), new PropertyMetadata(false));

        public bool IsEdit { get; set; }
        #endregion

        private MapBehaviorsList _mapBehaviorsList;

        public override void Execute(object parameter)
        {
            if (_mapBehaviorsList == null)
            {
                _mapBehaviorsList = new MapBehaviorsList() { Margin = new Thickness(10, 15, 10, 5) };
            }
            _mapBehaviorsList.IsEdit = IsEdit;
            WindowType windowType = MapApplication.Current.IsEditMode ? WindowType.DesignTimeFloating :
                WindowType.Floating;
            MapApplication.Current.ShowWindow(LocalizableStrings.GetString("MapBehaviors"), _mapBehaviorsList, false, null,
                (sender, e) =>
                {
                    _mapBehaviorsList = null;
                }, windowType);
        }
    }
}
