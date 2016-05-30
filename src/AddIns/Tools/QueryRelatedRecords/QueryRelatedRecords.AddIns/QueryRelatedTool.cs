/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.FeatureService;
using ESRI.ArcGIS.Client.Toolkit;
using QueryRelatedRecords.AddIns.Resources;


namespace QueryRelatedRecords.AddIns
{

    [Export(typeof(ICommand))]
    [LocalizedDisplayName("QueryRelatedToolTitle")]
    [LocalizedDescription("QueryRelatedToolDescription")]
    [LocalizedCategory("QueryRelatedToolCategory")]
    [DefaultIcon("/QueryRelatedRecords.AddIns;component/images/GetRelatedRecords16.png")]
    public class QueryRelatedTool : ICommand
    {
        private InfoWindow popupWindow; // The pop-up window where the Views are added or removed.
        private RelationshipSelectionView relationshipView; // The View containing the list of relationships for a feature.
        private KeepOnMap keepOnMapView; // The View containing the "Keep on map" option.
        private QueryRelatedViewModel vm; // The ViewModel.
        private ObservableCollection<Relationship> relationshipList; // The list of relationships for a feature.
        private OnClickPopupInfo popupInfo; // The information contained in the pop-up about a feature.
        private NoRecordsFoundView noRecordsView;
        private string numberOfRelates;

        public QueryRelatedTool()
        {
            // Create the list to hold the relationships.This list is bound to the Listbox control in the View.
            relationshipList = new ObservableCollection<Relationship>();
        }

        #region ICommand Members
        /// <summary>
        /// Retrieves the pop-up info and determines whether there is more than one relationship for the layer. If there is more
        /// than one, the relationships are displayed in a list for the user to choose from before proceeding.
        /// </summary>
        /// <param name="parameter">OnClickPopupInfo from clicked layer</param>
        public void Execute(object parameter)
        {
            relationshipList.Clear();

            OnClickPopupInfo popupInfo = parameter as OnClickPopupInfo;
            popupWindow = popupInfo.Container as InfoWindow;

            // Instantiate the View Model
            if (vm == null)
                vm = new QueryRelatedViewModel(relationshipList, MapApplication.Current.Map);

            // Instantiate the Relationship View
            if (relationshipView == null)
                relationshipView = new RelationshipSelectionView();

            // Instantiate the Keep on Map View
            if (keepOnMapView == null)
                keepOnMapView = new KeepOnMap();

            // Instantiate the No results found View
            if (noRecordsView == null)
                noRecordsView = new NoRecordsFoundView();

            // Set the variables on the ViewModel
            vm.PopupInfo = popupInfo;
            vm.KeepOnMapView = keepOnMapView;
            vm.RelationshipView = relationshipView;
            vm.NoRecordsFoundView = noRecordsView;

            // Set the data context of the RelationshipView and the KeepOnMapView to the QueryRelatedViewModel
            relationshipView.DataContext = vm;
            keepOnMapView.DataContext = vm;
            noRecordsView.DataContext = vm;

            // Get the layer info from the pop-up to access the Relationships. Verify the layer is a FeatureLayer to proceed.
            FeatureLayer relatesLayer;
            Layer lyr = popupInfo.PopupItem.Layer;
            if (lyr is FeatureLayer)
            {
                relatesLayer = lyr as FeatureLayer;

                // Check the number of relationships for the layer
                int relCount = relatesLayer.LayerInfo.Relationships.Count();
                if (relCount > 1) // Layer has more than one relationship
                {
                    foreach (Relationship rel in relatesLayer.LayerInfo.Relationships)
                    {
                        relationshipList.Add(rel);
                    }

                    numberOfRelates = string.Format(Strings.RelationshipsFound, 
                        relatesLayer.LayerInfo.Relationships.Count().ToString());
                    vm.NumberOfRelationships = numberOfRelates;

                    // Add the available relationships view to the popupWindow.  
                    Grid infoWindowGrid = Utils.FindChildOfType<Grid>(popupWindow, 3);
                    infoWindowGrid.Children.Add(relationshipView);


                    // Can now navigate back to attributes and close the popup from the relationship view, 
                    // so notify about change in executable state
                    ((DelegateCommand)vm.GoBack).RaiseCanExecuteChanged();
                    ((DelegateCommand)vm.CloseRelationshipView).RaiseCanExecuteChanged();
                }
                else // Layer only has one relationship, so can use Relationship.First to retrieve the ID.
                {
                    // Set the SelectedRelationship property on the ViewModel
                    vm.SelectedRelationship = relatesLayer.LayerInfo.Relationships.First();
                    // Call Execute method on the ViewModel
                    vm.QueryRelated.Execute(vm.SelectedRelationship);
                }
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Checks whether the QueryRelatedRecords tool can be used. Layer must have a relationship associated
        /// with it for the tool to be enabled. 
        /// </summary>
        /// <param name="parameter">OnClickPopupInfo from clicked layer</param>
        public bool CanExecute(object parameter)
        {

            popupInfo = parameter as OnClickPopupInfo;

            if (popupWindow != null && popupInfo.PopupItem != null)
            {
                // If the pop-up item (and therefore pop-up) changes, then verify whether the
                // KeepOnMap View should still be included in the pop-up.
                popupWindow = popupInfo.Container as InfoWindow;
                popupInfo.PropertyChanged += PopupItem_PropertyChanged;
            }

            return popupInfo != null && popupInfo.PopupItem != null
            && popupInfo.PopupItem.Layer is FeatureLayer
            && ((FeatureLayer)popupInfo.PopupItem.Layer).LayerInfo != null
            && ((FeatureLayer)popupInfo.PopupItem.Layer).LayerInfo.Relationships != null
            && ((FeatureLayer)popupInfo.PopupItem.Layer).LayerInfo.Relationships.Count() > 0
            && MapApplication.Current != null && MapApplication.Current.Map != null
            && MapApplication.Current.Map.Layers != null
            && MapApplication.Current.Map.Layers.Contains(popupInfo.PopupItem.Layer)
            && popupInfo.PopupItem.Graphic != null;

        }

        ///<summary>
        /// When moving between features in the pop-up window, this removes the "Keep on map" checkbox until a new query is executed.
        ///</summary>
        private void PopupItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            popupInfo.PropertyChanged -= PopupItem_PropertyChanged;

            if (e.PropertyName == "SelectedIndex")
            {
                if (popupWindow != null)
                {
                    // Remove the Keep on Map View (checkbox) if appropriate.
                    Grid infoWindowGrid = Utils.FindChildOfType<Grid>(popupWindow, 3);
                    if (infoWindowGrid.Children.Contains(keepOnMapView))
                    {
                        infoWindowGrid.Children.Remove(keepOnMapView);
                    }
                }
            }
        }

#pragma warning disable 67 // Disable warning for event not being used.  The ICommand interface requires that the event must be declared 
        public event EventHandler CanExecuteChanged; 
#pragma warning restore 67

        #endregion

    }
}



