/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.FeatureService;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Toolkit;
using QueryRelatedRecords.AddIns.Resources;


namespace QueryRelatedRecords.AddIns
{
    public class QueryRelatedViewModel : INotifyPropertyChanged
    {
        #region Member Variables

        private QueryTask queryTask; // The QueryTask for executing the relationship query.
        private FeatureLayer resultsLayer; // The layer containing the related features.
        private FeatureLayer relatesLayer; // The layer to query for related features.
        private RelationshipResult queryResult; // The results from the Query task.
        private BusyIndicator indicator; // Busy indicator to add to the popup window while processing results
        private InfoWindow popupWindow; // The popup window element. The Views and BusyIndicator are added and removed from this.
        private string objectIDField; // The field containing the ObjectID of a feature. Used for the QueryTask input parameters and for creating the layer name.
        private string CONTAINER_NAME = "FeatureDataGridContainer"; // Provides access to the Attribute table in the layout.
        private FrameworkElement attributeGridContainer; // The feature data grid (attribute table).
        private FeatureLayer temporaryLayer; // The layer added to the map after the initial query is complete. This layer displays the features with various "temporary" renderers.
        private FeatureLayer permanentLayer; // The layer added to the map when the "Keep on map" option is checked. This layer displays the features with the default renderers of the layer.
        private bool _popupItemChanged; // Tracks whether the individual pop-up item has changed. Set when the pop-up item changes or the pop-up closes.
        private Map map; // The current map.

        #endregion

        /// <summary>
        /// Constructor for the QueryRelatedViewModel. Sets the iniial members and instantiates commands.
        /// </summary>
        /// <param name="relationships">The list of relationships for the clicked feature.</param>
        /// <param name="Map">The current Map</param>
        public QueryRelatedViewModel(ObservableCollection<Relationship> relationships, Map Map)
        {

            RelationshipList = relationships;
            queryTask = new QueryTask();
            map = Map;

            // Instantiation of a command to execute the Query.
            QueryRelated = new DelegateCommand(doQuery, canDoQuery);

            // Instantiation of a command to go back to the original pop-up window from the RelationshipSelectionView (i.e. user clicks Back button).
            GoBack = new DelegateCommand(goBack, canGoBack);

            // Instantiation of a command to close the pop-up window while viewing the relationship list (i.e. user clicks Close (X) button).
            CloseRelationshipView = new DelegateCommand(closeRelationshipView, canCloseRelationshipView);

            CloseNoRecordsView = new DelegateCommand(closeNoRecordsView, canCloseNoRecordsView);
        }

        #region Commands

        /// <summary>
        /// Gets the command for executing the Query.
        /// </summary>
        public ICommand QueryRelated { get; private set; }

        /// <summary>
        /// Gets the command to execute the "Back" button on the RelationshipSelectionView.
        /// </summary>
        public ICommand GoBack { get; private set; }

        /// <summary>
        /// Gets the command to close the popup window while viewing the relationship list.
        /// </summary>
        public ICommand CloseRelationshipView { get; private set; }

        /// <summary>
        /// Gets the command to close the message box that displays when no related records are found.
        /// </summary>
        public ICommand CloseNoRecordsView { get; private set; }

        #endregion

        #region Command Methods

        /// <summary>
        /// Determines whether the query can be executed
        /// </summary>
        /// <remarks>The executable state is determined by whether the input parameter 
        /// (selected Relationship) is a Relationship. </remarks>
        private bool canDoQuery(object parameter)
        {
            return (parameter != null && parameter is Relationship);
        }

        /// <summary>
        /// Executes the query with the given Relationship
        /// </summary>
        /// <remarks>The input parameter is the selected relationship</remarks>
        private void doQuery(object parameter)
        {
            SelectedRelationship = parameter as Relationship;

            // Call the method that executes the Query.
            QueryRelationship();
        }

        /// <summary>
        /// Determines whether the Back command is in an executable state
        /// </summary>
        /// <remarks>
        /// The executable state is determined by whether QueryRelatedViewModel.PopupInfo.Container 
        /// container is an InfoWindow. If it is, the command is executable.
        /// </remarks>
        private bool canGoBack(object parameter)
        {
            popupWindow = PopupInfo.Container as InfoWindow;
            Grid infoWindowGrid = Utils.FindChildOfType<Grid>(popupWindow, 3);

            return popupWindow != null && infoWindowGrid.Children.Contains(RelationshipView);

        }

        /// <summary>
        /// Returns to the original pop-up attribute display.
        /// </summary>
        private void goBack(object parameter)
        {

            popupWindow = PopupInfo.Container as InfoWindow;
            //remove the relationship view
            Grid infoWindowGrid = Utils.FindChildOfType<Grid>(popupWindow, 3);
            if (infoWindowGrid.Children.Contains(RelationshipView))
            {
                infoWindowGrid.Children.Remove(RelationshipView);
            }
        }

        /// <summary>
        /// Determines whether the Close button is in an executable state
        /// </summary>
        /// <remarks>        
        /// The executable state is determined by whether QueryRelatedViewModel.PopupInfo.Container 
        /// container is an InfoWindow. If it is, the command is executable.</remarks>
        private bool canCloseRelationshipView(object parameter)
        {
            popupWindow = PopupInfo.Container as InfoWindow;
            Grid infoWindowGrid = Utils.FindChildOfType<Grid>(popupWindow, 3);
            return popupWindow != null && infoWindowGrid.Children.Contains(RelationshipView);

        }

        /// <summary>
        /// Closes the pop-up window
        /// </summary>
        private void closeRelationshipView(object parameter)
        {
            // Remove the list of relationships from the pop-up and close it
            popupWindow = PopupInfo.Container as InfoWindow;
            Grid infoWindowGrid = Utils.FindChildOfType<Grid>(popupWindow, 3);
            if (infoWindowGrid.Children.Contains(RelationshipView))
            {
                infoWindowGrid.Children.Remove(RelationshipView);
            }
            popupWindow.IsOpen = false;
        }

        /// <summary>
        /// Determines whether the popupWindow is null
        /// </summary>
        private bool canCloseNoRecordsView(object parameter)
        {
            // Check whether the grid contained in the pop-up window contains the NoRecordsFoundView
            popupWindow = PopupInfo.Container as InfoWindow;
            Grid infoWindowGrid = Utils.FindChildOfType<Grid>(popupWindow, 3);

            return popupWindow != null && infoWindowGrid.Children.Contains(NoRecordsFoundView);
        }

        /// <summary>
        /// Closes the dialog displaying the message that no related records were found.
        /// </summary>
        private void closeNoRecordsView(object parameter)
        {
            Grid infoWindowGrid = Utils.FindChildOfType<Grid>(popupWindow, 3);
            if (infoWindowGrid.Children.Contains(NoRecordsFoundView))
            {
                infoWindowGrid.Children.Remove(NoRecordsFoundView);
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The RelationshipSelectionView that displays the list of relationships.
        /// </summary>
        public RelationshipSelectionView RelationshipView { get; set; }

        /// <summary>
        /// The KeepOnMap View that displays the "Keep results on map" option.
        /// </summary>
        public KeepOnMap KeepOnMapView { get; set; }

        /// <summary>
        /// The NoRecordsFound View that displays the message that there are no related records for the feature
        /// </summary>
        public NoRecordsFoundView NoRecordsFoundView { get; set; }

        private string numberOfRelationships;
        /// <summary>
        /// Gets or sets the number of relationships for a layer
        /// </summary>
        public string NumberOfRelationships
        {
            get { return numberOfRelationships; }
            set
            {
                if (numberOfRelationships != value)
                    numberOfRelationships = value;
                OnPropertyChanged("NumberOfRelationships");
            }
        }

        private ObservableCollection<Relationship> relationshipList;
        /// <summary>
        /// Gets or sets the list of Relationships associated with a layer.
        /// </summary>
        public ObservableCollection<Relationship> RelationshipList
        {
            get { return relationshipList; }
            set
            {
                if (relationshipList != value)
                    relationshipList = value;
                OnPropertyChanged("RelationshipList");
            }
        }

        /// <summary>
        /// Gets or sets the selected Relationship for the Query.
        /// </summary>
        public Relationship SelectedRelationship { get; set; }

        private OnClickPopupInfo popupInfo;
        /// <summary>
        /// Gets or sets the <see cref="ESRI.ArcGIS.Client.Extensibility.OnClickPopupInfo"/> used by the ViewModel.
        /// </summary>
        public OnClickPopupInfo PopupInfo
        {
            get { return popupInfo; }
            set
            {
                if (popupInfo != value)
                {
                    popupInfo = value;
                    popupWindow = popupInfo.Container as InfoWindow;
                    if (popupWindow != null)
                        ExtensionMethods.Properties.NotifyOnDependencyPropertyChanged("IsOpen", popupWindow, OnIsOpenChanged);
                }
            }
        }

        private bool keepOnMap;
        /// <summary>
        /// Tracks whether the Keep on map option is checked. Adds and removes the results layer (temporary and permanent) to the
        /// map as apropriate. Sets attribute grid and map contents panel visibility. Also sets the selected layer in the Map Contents.
        /// </summary>
        public bool KeepOnMap
        {
            get { return keepOnMap; }
            set
            {
                if (keepOnMap != value)
                    keepOnMap = value;


                if (KeepOnMap) // Convert temp layer to permanent layer
                {
                    // Null out and remove temp layer
                    if (temporaryLayer != null)
                    {
                        if (map.Layers.Contains(temporaryLayer))
                            map.Layers.Remove(temporaryLayer);
                    }

                    temporaryLayer = null;

                    //  Add permanent layer
                    permanentLayer = CreatePermanentLayer();
                    map.Layers.Add(permanentLayer);
                    MapApplication.Current.SelectedLayer = permanentLayer;

                    //  Show in attribute table and map contents
                    ShowFeatureDataGrid();
                    ShowMapContents();
                }
                else // Change permanent layer to temp layer
                {
                    // Null out and remove permanent layer
                    if (permanentLayer != null)
                    {

                        if (map.Layers.Contains(permanentLayer))
                            map.Layers.Remove(permanentLayer);
                    }

                    permanentLayer = null;

                    //  Add temporary layer
                    temporaryLayer = CreateTempLayer();
                    map.Layers.Add(temporaryLayer);
                    MapApplication.Current.SelectedLayer = temporaryLayer;

                    // Just show in attribute table (temp layer is not shown in map contents)
                    ShowFeatureDataGrid();
                }

                OnPropertyChanged("KeepOnMap"); //"IsChecked"
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Performs the relationship query.
        /// </summary>
        /// <remarks>Called from the doQuery method.</remarks>
        private void QueryRelationship()
        {
            // Set the popupWindow and subscribe to changes on the popupWindow.IsOpen property
            popupWindow = PopupInfo.Container as InfoWindow;

            // Set the attribute grid container and subscripbe to changes on the Visibility property
            attributeGridContainer = MapApplication.Current.FindObjectInLayout(CONTAINER_NAME) as FrameworkElement;
            ExtensionMethods.Properties.NotifyOnDependencyPropertyChanged("Visibility", attributeGridContainer, OnDataGridVisibilityChanged);

            // Listen for selection of a different layer in the map contents.
            MapApplication.Current.SelectedLayerChanged += Current_SelectedLayerChanged;

            // Listen for a change in the selected index of the PopupInfo (indicating the popup item has changed).
            PopupInfo.PropertyChanged += PopupInfo_PropertyChanged;

            // Locate the grid inside the popup window and remove the RelationshipView. This was initially inserted if
            // multiple relationships for a feature were detected.
            Grid infoWindowGrid = Utils.FindChildOfType<Grid>(popupWindow, 3);
            infoWindowGrid.Children.Remove(RelationshipView);

            // Set the relationshipID for the QueryTask.
            int relationshipID = SelectedRelationship.Id;

            // Get the feature and layer info from the pop-up. The PopupItem property of OnClickPopupInfo 
            // provides information about the item currently shown in the pop-up.
            Graphic inputFeature = PopupInfo.PopupItem.Graphic;
            relatesLayer = PopupInfo.PopupItem.Layer as FeatureLayer; // The layer to get related records for. This is used to get the RelationshipID and Query url.

            // Get the name of the ObjectID field.
            objectIDField = relatesLayer.LayerInfo.ObjectIdField;

            // Input parameters for QueryTask
            RelationshipParameter relationshipParameters = new RelationshipParameter()
            {
                ObjectIds = new int[] { (int)inputFeature.Attributes[objectIDField] },
                OutFields = new string[] { "*" }, // Return all fields
                ReturnGeometry = true, // Return the geometry so that features can be displayed on the map if applicable
                RelationshipId = relationshipID, // Obtain the desired RelationshipID from the Service Details page. Here it takes the first relationship it finds if there is more than one.
                OutSpatialReference = map.SpatialReference
            };

            // Specify the Feature Service url for the QueryTask.
            queryTask.Url = relatesLayer.Url;
            queryTask.ProxyURL = relatesLayer.ProxyUrl;

            // Events for the successful completion of the RelationshipQuery and for if the Query fails
            queryTask.ExecuteRelationshipQueryCompleted += QueryTask_ExecuteRelationshipQueryCompleted;
            queryTask.Failed += QueryTask_Failed;

            // Execute the Query Task with specified parameters
            queryTask.ExecuteRelationshipQueryAsync(relationshipParameters);

            // Create the BusyIndicator and insert into the grid of the popup window.
            indicator = new BusyIndicator();
            indicator.BusyContent = Strings.RetrievingRecords;
            if (infoWindowGrid != null)
            {
                infoWindowGrid.Children.Add(indicator);
                indicator.IsBusy = true;
            }
        }

        /// <summary>
        /// Create the temporary layer from the results of the Query
        /// </summary>
        /// <returns>Temporary <see cref="ESRI.ArcGIS.Client.FeatureLayer"/></returns>
        /// <remarks>Uses "temporary" symbols to render the points, lines, or polygons. The temporary
        /// layer is not displayed in the Map Contents panel, but the attribute grid opens to show the feature attributes.</remarks>
        private FeatureLayer CreateTempLayer()
        {
            // Check that the results layer is not null and use the results layer to create the temporary layer
            if (resultsLayer != null)
            {
                temporaryLayer = resultsLayer;

                // ControlTemplate to create ellipse for temporary point symbol
                var template = (ControlTemplate)System.Windows.Markup.XamlReader.Load("<ControlTemplate " +
                    "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
                    "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" +
                    "<Grid RenderTransformOrigin=\"{Binding Symbol.RenderTransformPoint}\">" +
                        "<Ellipse " +
                            "Fill=\"#99FFFFFF\" " +
                            "Width=\"21\" " +
                            "Height=\"21\" " +
                            "Stroke=\"Red\" " +
                            "StrokeThickness=\"2\">" +
                        "</Ellipse>" +
                        "<Ellipse Fill=\"#99FFFFFF\" " +
                        "Width=\"3\" " +
                        "Height=\"3\" " +
                        "Stroke=\"Red\" " +
                        "StrokeThickness=\"2\">" +
                        "</Ellipse>" +
                    "</Grid> " +
                "</ControlTemplate>");

                Symbol test = new SimpleMarkerSymbol();

                // Set the renderers to a "temporary" look for the temp Layer
                if (temporaryLayer.LayerInfo.GeometryType == GeometryType.Point)
                    temporaryLayer.Renderer = new SimpleRenderer()
                   {
                       Symbol = new MarkerSymbol() { ControlTemplate = template as ControlTemplate, OffsetX = 10, OffsetY = 10 }

                   };
                else if (temporaryLayer.LayerInfo.GeometryType == GeometryType.Polyline)
                    temporaryLayer.Renderer = new SimpleRenderer()
                    {
                        Symbol = new SimpleLineSymbol()
                        {
                            Color = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
                            Width = 3
                        }
                    };
                else if (temporaryLayer.LayerInfo.GeometryType == GeometryType.Polygon)
                    temporaryLayer.Renderer = new SimpleRenderer()
                    {
                        Symbol = new SimpleFillSymbol()
                        {
                            Fill = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
                            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
                            BorderThickness = 3
                        }
                    };

                // Specify a name to set the ID property on the Layer. This name is associated with the layer but does not display in the map contents panel
                string tempLayerID = string.Format(Strings.RelatedTo, temporaryLayer.LayerInfo.Name, PopupInfo.PopupItem.Title);

                MapApplication.SetLayerName(temporaryLayer, tempLayerID);

                LayerProperties.SetIsVisibleInMapContents(temporaryLayer, false);
            }

            return temporaryLayer;
        }


        /// <summary>
        /// Creates the permanent layer from the results of the Query
        /// </summary>
        /// <returns>Permanent <see cref="ESRI.ArcGIS.Client.FeatureLayer"/></returns>
        /// <remarks>The permanent layer uses the default renderer of the layer. 
        /// The permanent layer is visible in the Map Contents panel.</remarks>
        private FeatureLayer CreatePermanentLayer()
        {
            // Check that the results layer is not null and use the results layer to create the permanent layer
            if (resultsLayer != null)
            {
                permanentLayer = resultsLayer;

                // Create a layer ID for displaying the layer in the map contents and attribute table
                string permanentLayerID = string.Format(Strings.RelatedTo, permanentLayer.LayerInfo.Name, PopupInfo.PopupItem.Title);

                // If there is more than one layer with the same name, add a number to the end of the name
                int layerCountSuffix = 0;

                string layerName;
                // Verify the next number in the sequence hasn't been used. If layers are renamed after the name is generated, a scenario may occur where
                // the last number used in the name is actually greater than the number of existing layers with the same name. We don't want a number to 
                // repeat, so we check the last number used.
                foreach (Layer layer in map.Layers)
                {
                    layerName = MapApplication.GetLayerName(layer);
                    if (layerName != null && layerName.StartsWith(permanentLayerID))
                    {
                        if (layerName.EndsWith(")"))
                        {
                            // Split the layer name at the end to get the last number used (number contained within the parentheses)
                            string[] splitLayerName = layerName.Split('(');
                            int lastNumberAppended = Convert.ToInt32(splitLayerName.Last<string>().TrimEnd(')'));
                            // If the last number used is greater than the count of number of existing layers with the same name, 
                            // set the count to the last number used.
                            if (lastNumberAppended > layerCountSuffix)
                            {
                                layerCountSuffix = lastNumberAppended;
                            }
                        }
                        else // Found a layer with the same name, but no (#) suffix.  This is the first layer added with this name.
                        {
                            // Only set the suffix based on this layer if the suffix has not yet been set (i.e. is still zero)
                            if (layerCountSuffix == 0)
                            {
                                layerCountSuffix = 1;
                            }

                        }
                    }
                }

                if (layerCountSuffix != 0)
                {
                    permanentLayerID += string.Format(" ({0})", layerCountSuffix + 1);
                }


                MapApplication.SetLayerName(permanentLayer, permanentLayerID);

                // Create a unique ID for the layer. 
                permanentLayer.ID = Guid.NewGuid().ToString();

                // If the results layer is a table, don't show it in the map contents.
                if (resultsLayer.LayerInfo.Type == "Table")
                    LayerProperties.SetIsVisibleInMapContents(permanentLayer, false);
                else
                    LayerProperties.SetIsVisibleInMapContents(permanentLayer, true);

                // Set the layer renderer to the renderer of the original layer
                permanentLayer.Renderer = resultsLayer.LayerInfo.Renderer;

            }

            return permanentLayer;
        }

        /// <summary>
        /// Sets the title of the pop-up
        /// </summary>
        /// <remarks>
        /// In cases where the pop-up title is not set automatically (and therefore results in a blank title), 
        /// this method loops through the fields in the layer to determine an appropriate title to display.
        /// </remarks>
        private void SetPopupTitleExpression()
        {

            // Dictionary with integer keys and string values. Integer keys are layer ids: -1 for a GraphicLayer, 
            // otherwise Layer ID. String values are format strings like this: "{NAME}: {POPULATION}"
            IDictionary<int, string> titleExpression = new Dictionary<int, string>();

            // Check that the LayerInfo and DisplayField are not null. Use the DisplayField if it exists.
            if (resultsLayer.LayerInfo != null && resultsLayer.LayerInfo.DisplayField != null)
            {
                string layerInfoDisplayField = resultsLayer.LayerInfo.DisplayField;
                string displayFieldExpression = "{" + layerInfoDisplayField + "}";

                titleExpression.Add(-1, displayFieldExpression);

                LayerProperties.SetPopupTitleExpressions(resultsLayer, titleExpression);
            }
            else // display field is null
            {
                // Create a list of all the fields in the layer
                List<Field> fields = resultsLayer.LayerInfo.Fields;

                // Loop through each field to see if there is a FieldName called "Name"(case-insensitive).
                foreach (Field field in fields)
                {
                    string fieldName = field.FieldName;

                    if (fieldName != null)
                    {
                        if (string.Compare(fieldName, "Name", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            titleExpression.Add(-1, "{" + fieldName + "}");
                            LayerProperties.SetPopupTitleExpressions(resultsLayer, titleExpression);
                            return; // If the Popup title expression is set, exit.
                        }
                    }
                }

                // Loop through each field to see if there is a FieldName that contains "Name" (case-insensitive).
                foreach (Field field in fields)
                {
                    string fieldName = field.FieldName;

                    if (fieldName != null)
                    {
                        // if index is -1 then the FieldName does not contain "Name" (case-insensitive)   
                        int index = fieldName.IndexOf("Name", StringComparison.InvariantCultureIgnoreCase);

                        if (index >= 0)
                        {
                            titleExpression.Add(-1, "{" + fieldName + "}");
                            LayerProperties.SetPopupTitleExpressions(resultsLayer, titleExpression);
                            return; // If the Popup title expression is set, exit.
                        }
                    }
                }

                // Loop through each field and look for the first field that is both visible in the pop-up attribute list and is a string
                foreach (Field field in fields)
                {
                    string fieldName = field.FieldName;

                    foreach (FieldSettings fSetting in popupInfo.PopupItem.FieldInfos)
                    {
                        if (fieldName != null && fieldName == fSetting.Name && fSetting.FieldType == FieldType.Text)
                        {
                            titleExpression.Add(-1, "{" + fieldName + "}");
                            LayerProperties.SetPopupTitleExpressions(resultsLayer, titleExpression);
                        }
                        return;
                    }
                }
            }
        }


        /// <summary>
        /// Displays the Map Contents panel
        /// </summary>
        private void ShowMapContents()
        {
            // Locate the SidePanelContainer and MapContentsTabItem.
            TabControl sidePanelContainer = MapApplication.Current.FindObjectInLayout("SidePanelContainer") as TabControl;
            TabItem mapContents = MapApplication.Current.FindObjectInLayout("MapContentsTabItem") as TabItem;

            // Verify they are not null before displaying
            if (sidePanelContainer != null && mapContents != null)
            {
                sidePanelContainer.SelectedItem = mapContents;

                // try to get storyboard (animation) for showing attribute table
                Storyboard showStoryboard = sidePanelContainer.FindStoryboard("SidePanelContainer" + "_Show");
                if (showStoryboard != null)
                    showStoryboard.Begin(); // use storyboard if available
                else
                    sidePanelContainer.Visibility = Visibility.Visible; // no storyboard, so set visibility directly
            }
        }

        /// <summary>
        /// Displays the attribute table.
        /// </summary>
        private void ShowFeatureDataGrid()
        {
            // Get the attribute table container
            if (attributeGridContainer != null)
            {
                // try to get storyboard (animation) for showing attribute table
                Storyboard showStoryboard = attributeGridContainer.FindStoryboard(CONTAINER_NAME + "_Show");
                if (showStoryboard != null)
                    showStoryboard.Begin(); // use storyboard if available
                else
                    attributeGridContainer.Visibility = Visibility.Visible; // no storyboard, so set visibility directly
            }
        }

        /// <summary>
        /// Hides the attribute table.
        /// </summary>
        private void HideFeatureDataGrid()
        {
            // Get the attribute table container
            if (attributeGridContainer != null)
            {
                // try to get storyboard (animation) for showing attribute table
                Storyboard hideStoryboard = attributeGridContainer.FindStoryboard(CONTAINER_NAME + "_Hide");
                if (hideStoryboard != null)
                    hideStoryboard.Begin(); // use storyboard if available
                else
                    attributeGridContainer.Visibility = Visibility.Collapsed; // no storyboard, so set visibility directly
            }
        }


        #endregion

        #region Event Handlers

        /// <summary>
        /// Handle successful query task and initializes the results layer
        /// </summary>
        private void QueryTask_ExecuteRelationshipQueryCompleted(object sender, RelationshipEventArgs e)
        {
            // Unhook the queryTask.ExecuteRelationshipQueryCompleted
            queryTask.ExecuteRelationshipQueryCompleted -= QueryTask_ExecuteRelationshipQueryCompleted;

            // queryResult used once the results layer is initialized
            queryResult = e.Result;

            if (queryResult.RelatedRecordsGroup.Count > 0)
            {

                string relatedTableID = SelectedRelationship.RelatedTableId.ToString();

                if (relatedTableID == null)
                {
                    relatedTableID = relatesLayer.LayerInfo.Relationships.First().RelatedTableId.ToString();
                }

                // Determine the url for the related table and use to populate the Url property of the new resultsLayer.
                string resultsUrl = relatesLayer.Url;

                resultsUrl = resultsUrl.Substring(0, resultsUrl.LastIndexOf('/') + 1);
                resultsUrl = resultsUrl.Insert(resultsUrl.Length, relatedTableID);


                // Create a FeatureLayer for the results based on the url of the related records. At this point, the feature layer will
                // retrieve all the graphics/info from the layer. In the resultsLayer_Initialized, we trim that down based on the Object id field.
                resultsLayer = new FeatureLayer()
                {
                    Url = resultsUrl,
                    ProxyUrl = relatesLayer.ProxyUrl
                };

                resultsLayer.OutFields.Add("*");

                // Initialize the resultsLayer to populate layer metadata (LayerInfo) so the OID field can be retrieved.
                resultsLayer.Initialized += resultsLayer_Initialized;
                resultsLayer.InitializationFailed += resultsLayer_InitializationFailed;
                resultsLayer.UpdateCompleted += resultsLayer_UpdateCompleted;

                resultsLayer.Initialize();

            }
            else
            {
                Grid infoWindowGrid = Utils.FindChildOfType<Grid>(popupWindow, 3);
                infoWindowGrid.Children.Remove(indicator);

                infoWindowGrid.Children.Add(NoRecordsFoundView);

                ((DelegateCommand)CloseNoRecordsView).RaiseCanExecuteChanged();
            }
        }


        /// <summary>
        /// Shows the feature data grid and map contents panel if applicable
        /// </summary>
        void resultsLayer_UpdateCompleted(object sender, EventArgs e)
        {
            resultsLayer.UpdateCompleted -= resultsLayer_UpdateCompleted;

            if (map.Layers.Contains(temporaryLayer))
            {
                ShowFeatureDataGrid();
            }
            else if (map.Layers.Contains(permanentLayer))
            {
                ShowFeatureDataGrid();
                ShowMapContents();
            }
        }


        /// <summary>
        /// Handle successful initialization of the resultsLayer
        /// </summary>
        private void resultsLayer_Initialized(object sender, EventArgs e)
        {

            if (LayerProperties.GetPopupTitleExpressions(resultsLayer) == null)
            {
                SetPopupTitleExpression();
            }

            Grid infoWindowGrid = Utils.FindChildOfType<Grid>(popupWindow, 3);
            // Only display the Keep results on map option of the results layer is not a table
            if (!infoWindowGrid.Children.Contains(KeepOnMapView) && resultsLayer.LayerInfo.Type != "Table")
            {
                infoWindowGrid.Children.Insert(1, KeepOnMapView);
            }

            indicator.IsBusy = false;

            // Get the FeatureLayer's OID field from the LayerInfo
            string oidField = resultsLayer.LayerInfo.ObjectIdField;

            // Create a List to hold on to the selected ObjectIds (related records)
            List<int> list = new List<int>();

            //Go through the RelatedRecordsGroup and create a list with the object ids.
            foreach (var records in queryResult.RelatedRecordsGroup)
            {
                foreach (Graphic graphic in records.Value)
                {
                    //  graphic.Geometry.SpatialReference.WKID = map.SpatialReference.WKID;
                    list.Add((int)graphic.Attributes[oidField]);
                    resultsLayer.Graphics.Add(graphic);
                }
            }

            // Limit the records displayed to only the ones that match the given object ids. 
            resultsLayer.ObjectIDs = list.ToArray();

            // Query is executed and keep on map is still checked (from previous query). Set permanent and temporary layers to null. 
            if (KeepOnMap)
            {
                if (permanentLayer != null)
                    permanentLayer = null;

                if (temporaryLayer != null)
                    temporaryLayer = null;

                // Create the permanent layer and add it to the map
                permanentLayer = CreatePermanentLayer();
                map.Layers.Add(permanentLayer);
                ShowMapContents();
            }
            else // Keep on map is unchecked. 
            {
                // Create a temp layer first and then wait to see if user clicks checkbox
                if (temporaryLayer != null && map.Layers.Contains(temporaryLayer))
                {
                    map.Layers.Remove(temporaryLayer);
                    temporaryLayer = null;
                }
                temporaryLayer = CreateTempLayer();
                map.Layers.Add(temporaryLayer);
            }
        }


        private ThrottleTimer _updatePopupVisThrottler; // Throttles 

        /// <summary>
        /// Handles when a different layer is selected.
        /// </summary>
        private void Current_SelectedLayerChanged(object sender, EventArgs e)
        {

            // Initialize the throttler, if necessary 
            if (_updatePopupVisThrottler == null)
                _updatePopupVisThrottler = new ThrottleTimer(10, () => { UpdatePopupWindow(); });

            // Invoke the throttler. When it executes, it will update the IsOpen property
            // of the popupWindow.
            _updatePopupVisThrottler.Invoke();

        }

        /// <summary>
        /// Closes the pop-up window if another layer is set as the selected layer
        /// </summary>
        /// <remarks>        
        /// Called from the Current_SelectedLayerChanged. Used in conjunction with the ThrottleTimer to
        /// avoid problems associated with the frequent firing of the SelectedLayerChanged event while adding 
        /// and removing layers from the map's layer collection.</remarks>
        private void UpdatePopupWindow()
        {
            // The selected layer changed, but don't do anything unless the popupWindow is open. 
            if (popupWindow.IsOpen)
            {
                // If the permanent layer exists in the map (and is therefore visible in the Map Contents)  
                // and another layer is selected, close the popup window.
                if (map.Layers.Contains(permanentLayer) && MapApplication.Current.SelectedLayer != permanentLayer)
                {
                    popupWindow.IsOpen = false;
                }

                // If the temporary layer exists, then by default it is not visible in the Map Contents, but it can still
                // be the "SelectedLayer" (set programmatically). If another layer is selected, close the popup window.
                if (map.Layers.Contains(temporaryLayer) && MapApplication.Current.SelectedLayer != temporaryLayer)
                {
                    popupWindow.IsOpen = false;
                }
            }
        }

        /// <summary>
        /// Handle events when the popup window closes
        /// </summary>
        private void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Popup window is closed
            if (!popupWindow.IsOpen)
            {
                // Pop-up is closing, so detach PopupInfo.PropertyChanged and MapApplication.Current.SelectedLayerChanged events
                PopupInfo.PropertyChanged -= PopupInfo_PropertyChanged;
                MapApplication.Current.SelectedLayerChanged -= Current_SelectedLayerChanged;

                // If the pop-up is closing and the temporary layer still exists, remove it from the map.
                if (temporaryLayer != null)
                {
                    if (MapApplication.Current.SelectedLayer == temporaryLayer)
                        HideFeatureDataGrid();

                    if (map.Layers.Contains(temporaryLayer))
                        map.Layers.Remove(temporaryLayer);

                    temporaryLayer = null;
                }


                Grid infoWindowGrid = Utils.FindChildOfType<Grid>(popupWindow, 3);
                if (infoWindowGrid.Children.Contains(indicator))
                    infoWindowGrid.Children.Remove(indicator); // Remove the busy indicator from the pop-up window

                if (CloseNoRecordsView.CanExecute(popupWindow))
                    CloseNoRecordsView.Execute(popupWindow);

                if (GoBack.CanExecute(popupWindow))
                    GoBack.Execute(popupWindow);

                _popupItemChanged = false;
            }
        }

        /// <summary>
        /// Handles when a different item in the pop-up is selected.
        /// </summary>
        private void PopupInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedIndex") // Verify the property that has changed is the SelectedIndex
            {
                if (map.Layers.Contains(temporaryLayer))
                {
                    map.Layers.Remove(temporaryLayer);
                    temporaryLayer = null;

                    // PopupInfo_PropertyChanged is unhooked in OnIsOpenChanged
                    _popupItemChanged = true;
                    HideFeatureDataGrid();
                }

            }
        }

        private ThrottleTimer handleAttributeTableCloseTimer;
        /// <summary>
        /// Handles when feature data grid closes
        /// </summary>
        private void OnDataGridVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // The e.OldValue can be Null, Visible, or Collapsed. If the value was null, this means the application
            // had just started and we don't want to do anything when the Visibility initially changes.
            if (e.OldValue != null)
            {
                string oldValue = e.OldValue.ToString();

                if (oldValue == "Visible" && attributeGridContainer.Visibility == Visibility.Collapsed)
                {
                    if (handleAttributeTableCloseTimer == null)
                    {
                        handleAttributeTableCloseTimer = new ThrottleTimer(10, () =>
                        {
                            if (map.Layers.Contains(temporaryLayer))
                            {
                                map.Layers.Remove(temporaryLayer);
                                temporaryLayer = null;
                            }

                            // If the attribute table closes and the pop-up item has not changed, close the pop-up window.
                            if (!_popupItemChanged)
                            {
                                popupWindow.IsOpen = false;
                            }
                            else
                            {
                                _popupItemChanged = false;
                            }
                        });
                    }

                    handleAttributeTableCloseTimer.Invoke();
                }
            }
        }

        /// <summary>
        /// Handles failed QueryTask by displaying an error message. 
        /// </summary>
        private void QueryTask_Failed(object sender, TaskFailedEventArgs e)
        {
            // Unhook the queryTask.Failed even
            queryTask.Failed -= QueryTask_Failed;

            // Show failure error message
            TextBlock failureText = new TextBlock()
            {
                Margin = new Thickness(10),
                Text = e.Error.Message
            };
            string windowTitle = string.Format("{0}", "Error");

            MapApplication.Current.ShowWindow(windowTitle, failureText, true);
        }

        /// <summary>
        /// Handles failed layer initialization
        /// </summary>
        private void resultsLayer_InitializationFailed(object sender, EventArgs e)
        {
            resultsLayer.InitializationFailed -= resultsLayer_InitializationFailed;

            // Show failure error message
            TextBlock failureText = new TextBlock()
            {
                Margin = new Thickness(10),
                Text = "Could not create layer."
            };
            string windowTitle = string.Format("{0}", "Error");
            MapApplication.Current.ShowWindow(windowTitle, failureText, true);

        }
        #endregion

        #region Property changed

        /// <summary>
        /// Fires the PropertyChanged event
        /// </summary>
        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public event PropertyChangedEventHandler PropertyChanged;


        #endregion
    }
}
