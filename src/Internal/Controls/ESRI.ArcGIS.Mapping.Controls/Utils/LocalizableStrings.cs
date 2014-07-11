/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/


namespace ESRI.ArcGIS.Mapping.Controls
{
    public static class LocalizableStrings
    {
        private static StringResourcesManager StringResources = new StringResourcesManager();

        public static string GetString(string key)
        {
            return StringResources.Get(key);
        }

        internal static string ArcGISOnline
        {
            get
            {
                return StringResources.Get("ArcGISOnline");
            }
        }

        internal static string BingMaps
        {
            get
            {
                return StringResources.Get("BingMaps");
            }
        }

        internal static string ArcGISServer
        {
            get
            {
                return StringResources.Get("ArcGISServer");
            }
        }

        internal static string BingMapsRoads
        {
            get
            {
                return StringResources.Get("BingMapsRoads");
            }
        }

        internal static string BingMapsAerial
        {
            get
            {
                return StringResources.Get("BingMapsAerial");
            }
        }

        internal static string BingMapsAerialWithLabels
        {
            get
            {
                return StringResources.Get("BingMapsAerialWithLabels");
            }
        }

        internal static string LocationFieldsTabHeader
        {
            get
            {
                return StringResources.Get("LocationFieldsTabHeader");
            }
        }

        internal static string ConfigurePopups
        {
            get
            {
                return StringResources.Get("ConfigurePopups");
            }
        }

        internal static string Properties
        {
            get
            {
                return StringResources.Get("Properties");
            }
        }

        internal static string ChooseBaseMapLayer
        {
            get
            {
                return StringResources.Get("ChooseBaseMapLayer");
            }
        }

        internal static string MultiLayerCacheNotSupported
        {
            get
            {
                return StringResources.Get("MultiLayerCacheNotSupported");
            }
        }

        internal static string MultiLayerCacheNotSupportedCaption
        {
            get
            {
                return StringResources.Get("MultiLayerCacheNotSupportedCaption");
            }
        }

        internal static string BingMapsAccountInfo
        {
            get
            {
                return StringResources.Get("BingMapsAccountInfo");
            }
        }

        internal static string NotUsingBingMapsResource
        {
            get
            {
                return StringResources.Get("NotUsingBingMapsResource");
            }
        }

        internal static string InvalidClassBreakValue
        {
            get
            {
                return StringResources.Get("InvalidClassBreakValue");
            }
        }

        internal static string NotUsingBingMapsResourceCaption
        {
            get
            {
                return StringResources.Get("NotUsingBingMapsResourceCaption");
            }
        }

        internal static string SymbolOptions
        {
            get
            {
                return StringResources.Get("SymbolOptions");
            }
        }

        internal static string FindLocation
        {
            get
            {
                return StringResources.Get("FindLocation");
            }
        }

        internal static string UpdateAddress
        {
            get
            {
                return StringResources.Get("UpdateAddress");
            }
        }

        internal static string ChooseLayerHeader
        {
            get
            {
                return StringResources.Get("ChooseLayerHeader");
            }
        }                

        internal static string ErrorCaption
        {
            get
            {
                return StringResources.Get("ErrorCaption");
            }
        }

        internal static string AdvancedClusterProperties
        {
            get
            {
                return StringResources.Get("AdvancedClusterProperties");
            }
        }

        internal static string AGSOStreets
        {
            get
            {
                return StringResources.Get("AGSOStreets");
            }
        }

        internal static string AGSOTopo
        {
            get
            {
                return StringResources.Get("AGSOTopo");
            }
        }

        internal static string DuplicateMapServiceLayer
        {
            get
            {
                return StringResources.Get("DuplicateMapServiceLayer");
            }
        }

        internal static string DuplicateMapServiceLayerCaption
        {
            get
            {
                return StringResources.Get("DuplicateMapServiceLayerCaption");
            }
        }        

        internal static string AGSOImagery
        {
            get
            {
                return StringResources.Get("AGSOImagery");
            }
        }

        internal static string NoAddressMappingHasBeenDefined
        {
            get
            {
                return StringResources.Get("NoAddressMappingHasBeenDefined");
            }
        }

        internal static string NoAddressMappingHasBeenDefinedCaption
        {
            get
            {
                return StringResources.Get("NoAddressMappingHasBeenDefinedCaption");
            }
        }

        internal static string ValidAddressError
        {
            get
            {
                return StringResources.Get("ValidAddressError");
            }
        }

        internal static string ValidCoordinatesError
        {
            get
            {
                return StringResources.Get("ValidCoordinatesError");
            }
        }

        internal static string ValidAddressErrorCaption
        {
            get
            {
                return StringResources.Get("ValidAddressErrorCaption");
            }
        }

        internal static string ValidCoordinatesErrorCaption
        {
            get
            {
                return StringResources.Get("ValidCoordinatesErrorCaption");
            }
        }

        internal static string ShowHideTooltip
        {
            get
            {
                return StringResources.Get("ShowHide");
            }
        }

        internal static string ConfigureTooltip
        {
            get
            {
                return StringResources.Get("ConfigureTooltip");
            }
        }

        internal static string TokenAuthenticationFailed
        {
            get
            {
                return StringResources.Get("TokenAuthenticationFailed");
            }
        }

        internal static string TokenConnectionFailed
        {
            get { return StringResources.Get("TokenConnectionFailed"); }
        }

        internal static string EnterCredentials
        {
            get { return StringResources.Get("EnterCredentials"); }
        }

        internal static string BingMapsInitializeFailed
        {
            get { return StringResources.Get("BingMapsInitializeFailed"); }
        }

        internal static string GetListItemsFailedPromptRemove
        {
            get { return StringResources.Get("GetListItemsFailedPromptRemove"); }
        }

        internal static string ServiceConnectionErrorDuringInit
        {
            get { return StringResources.Get("ServiceConnectionErrorDuringInit"); }
        }

        internal static string RetrievingInformation
        {
            get { return StringResources.Get("RetrievingInformation"); }
        }

        internal static string Cancelling
        {
            get { return StringResources.Get("Cancelling"); }
        }

        internal static string Cancel
        {
            get { return StringResources.Get("Cancel"); }
        }

        internal static string NoGeographicListsFound
        {
            get { return StringResources.Get("NoGeographicListsFound"); }
        }

        internal static string GetListsFailed
        {
            get { return StringResources.Get("GetListsFailed"); }
        }

        internal static string MultipleViewsNotAllowed
        {
            get { return StringResources.Get("MultipleViewsNotAllowed"); }
        }

        internal static string ServiceConnectionError
        {
            get { return StringResources.Get("ServiceConnectionError"); }
        }

        internal static string NoMapServicesFound
        {
            get { return StringResources.Get("NoMapServicesFound"); }
        }
        internal static string NoGPServicesFound
        {
            get { return StringResources.Get("NoGPServicesFound"); }
        }
        internal static string ServiceConnectionErrorCaption
        {
            get { return StringResources.Get("ServiceConnectionErrorCaption"); }
        }

        internal static string DeleteConnectionPrompt
        {
            get { return StringResources.Get("DeleteConnectionPrompt"); }
        }

        internal static string DeleteConnectionCaption
        {
            get { return StringResources.Get("DeleteConnectionCaption"); }
        }

        internal static string HeatMapAdvancedProperties
        {
            get { return StringResources.Get("HeatMapAdvancedProperties"); }
        }

        internal static string DuplicateDataSource
        {
            get { return StringResources.Get("DuplicateDataSource"); }
        }

        internal static string NoSpatialTablesFound
        {
            get { return StringResources.Get("NoSpatialTablesFound"); }
        }

        internal static string RefreshError
        {
            get { return StringResources.Get("RefreshError"); }
        }

        internal static string InvalidUrlPrompt
        {
            get { return StringResources.Get("InvalidUrlPrompt"); }
        }

        internal static string InvalidUrlCaption
        {
            get { return StringResources.Get("InvalidUrlCaption"); }
        }

        internal static string AddressMatchError
        {
            get { return StringResources.Get("AddressMatchError"); }
        }

        internal static string UpdateLayerError
        {
            get { return StringResources.Get("UpdateLayerError"); }
        }

        internal static string InitializationError
        {
            get { return StringResources.Get("InitializationError"); }
        }

        internal static string LayerInfoRetrievalError
        {
            get { return StringResources.Get("LayerInfoRetrievalError"); }
        }

        internal static string GetListItemsFailed
        {
            get { return StringResources.Get("GetListItemsFailed"); }
        }

        internal static string ClearAll
        {
            get
            {
                return StringResources.Get("ClearAll");
            }
        }

        internal static string SelectAll
        {
            get
            {
                return StringResources.Get("SelectAll");
            }
        }

        internal static string VisibleHeader
        {
            get
            {
                return StringResources.Get("VisibleHeader");
            }
        }        

        internal static string FieldHeader
        {
            get
            {
                return StringResources.Get("FieldHeader");
            }
        }

        internal static string AliasHeader
        {
            get
            {
                return StringResources.Get("AliasHeader");
            }
        }

        internal static string NoNumericFieldsFound
        {
            get { return StringResources.Get("NoNumericFieldsFound"); }
        }

        internal static string UpdateListError
        {
            get { return StringResources.Get("UpdateListError"); }
        }

        internal static string AddCoordsToListFailedPrompt
        {
            get { return StringResources.Get("AddCoordsToListFailedPrompt"); }
        }

        internal static string AddCoordsToListFailedCaption
        {
            get { return StringResources.Get("AddCoordsToListFailedCaption"); }
        }

        internal static string UnhandledSilverlightError
        {
            get { return StringResources.Get("UnhandledSilverlightError"); }
        }

        internal static string MoveRangeTooltip
        {
            get { return StringResources.Get("MoveRangeTooltip"); }
        }

        internal static string SelectRangeTooltip
        {
            get { return StringResources.Get("SelectRangeTooltip"); }
        }

        internal static string ZoomRangeTooltip
        {
            get { return StringResources.Get("ZoomRangeTooltip"); }
        }

        internal static string ConfigurationTooltip
        {
            get { return StringResources.Get("ConfigurationTooltip"); }
        }

        internal static string Seconds
        {
            get { return StringResources.Get("Seconds"); }
        }

        internal static string Minutes
        {
            get { return StringResources.Get("Minutes"); }
        }

        internal static string NoDistinctNumericFields
        {
            get { return StringResources.Get("NoDistinctNumericFields"); }
        }

        internal static string NumericFieldNotDistinct
        {
            get { return StringResources.Get("NumericFieldNotDistinct"); }
        }

        internal static string ClassifyBy
        {
            get { return StringResources.Get("ClassifyBy"); }
        }

        internal static string LocateTooltip
        {
            get { return StringResources.Get("LocateTooltip"); }
        }

        internal static string LocateUpdateTooltip
        {
            get { return StringResources.Get("LocateUpdateTooltip"); }
        }

        internal static string DeleteLayerPrompt
        {
            get { return StringResources.Get("DeleteLayerPrompt"); }
        }

        internal static string DeleteLayerMessageCaption
        {
            get { return StringResources.Get("DeleteLayerMessageCaption"); }
        }

        internal static string BingMapsAccountStatus
        {
            get { return StringResources.Get("BingMapsAccountStatus"); }
        }                        

        private static string scaleBarUnit_meters;
        internal static string ScaleBarUnit_meters
        {
            get
            {
                if (string.IsNullOrEmpty(scaleBarUnit_meters))
                    scaleBarUnit_meters = StringResources.Get("ScaleBarUnit_meters");
                return scaleBarUnit_meters;
            }
        }

        private static string scaleBarUnit_kilometers;
        internal static string ScaleBarUnit_kilometers
        {
            get
            {
                if (string.IsNullOrEmpty(scaleBarUnit_kilometers))
                    scaleBarUnit_kilometers = StringResources.Get("ScaleBarUnit_kilometers");
                return scaleBarUnit_kilometers;
            }
        }

        private static string scaleBarUnit_miles;
        internal static string ScaleBarUnit_miles
        {
            get
            {
                if (string.IsNullOrEmpty(scaleBarUnit_miles))
                    scaleBarUnit_miles = StringResources.Get("ScaleBarUnit_miles");
                return scaleBarUnit_miles;
            }
        }

        private static string scaleBarUnit_feet;
        internal static string ScaleBarUnit_feet
        {
            get
            {
                if (string.IsNullOrEmpty(scaleBarUnit_feet))
                    scaleBarUnit_feet = StringResources.Get("ScaleBarUnit_feet");
                return scaleBarUnit_feet;
            }
        }

        internal static string Locate
        {
            get
            {
                return StringResources.Get("Locate");
            }
        }

        internal static string OK
        {
            get
            {
                return StringResources.Get("OK");
            }
        }

        internal static string NewDataSourceHelpText
        {
            get
            {
                return StringResources.Get("NewDataSourceHelpText");
            }
        }

        internal static string ConnectToSharePointDataSource
        {
            get
            {
                return StringResources.Get("ConnectToSharePointDataSource");
            }
        }

        internal static string MustEnterFileName
        {
            get
            {
                return StringResources.Get("MustEnterFileName");
            }
        }

        internal static string MustEnterFileNameTitle
        {
            get
            {
                return StringResources.Get("MustEnterFileNameTitle");
            }
        }

        internal static string CachedMapServiceSpatialReferenceMisMatch
        {
            get
            {
                return StringResources.Get("CachedMapServiceSpatialReferenceMisMatch");
            }
        }

        internal static string CachedMapServiceSpatialReferenceMisMatchCaption
        {
            get
            {
                return StringResources.Get("CachedMapServiceSpatialReferenceMisMatchCaption");
            }
        }

        internal static string OperationMapServicesLayersWillBeRemoved
        {
            get
            {
                return StringResources.Get("OperationMapServicesLayersWillBeRemoved");
            }
        }

        internal static string OperationMapServicesLayersWillBeRemovedCaption
        {
            get
            {
                return StringResources.Get("OperationMapServicesLayersWillBeRemovedCaption");
            }
        }

        internal static string EnterUrlDialogTitle
        {
            get
            {
                return StringResources.Get("EnterUrlDialogTitle");
            }
        }


        internal static string MapContentsTitle
        {
            get
            {
                return StringResources.Get("MapContentsTitle");
            }
        }

        internal static string BrowseForBaseMapTitle
        {
            get
            {
                return StringResources.Get("BrowseForBaseMapTitle");
            }
        }

        internal static string BaseMapGalleryTitle
        {
            get
            {
                return StringResources.Get("BaseMapGalleryTitle");
            }
        }

        internal static string AddContentTitle
        {
            get
            {
                return StringResources.Get("AddContentTitle");
            }
        }

        internal static string CanOnlyUseCachedMapServicesAsBaseMap
        {
            get
            {
                return StringResources.Get("CanOnlyUseCachedMapServicesAsBaseMap");
            }
        }

        internal static string InvalidMapServiceCaption
        {
            get
            {
                return StringResources.Get("InvalidMapServiceCaption");
            }
        }

        internal static string SingleSymbol
        {
            get { return StringResources.Get("SingleSymbol"); }
        }

        internal static string ClassBreaks
        {
            get { return StringResources.Get("ClassBreaks"); }
        }

        internal static string UniqueValues
        {
            get { return StringResources.Get("UniqueValues"); }
        }

        internal static string AddGeoRSSTitle
        {
            get
            {
                return StringResources.Get("AddGeoRSSTitle");
            }
        }

        internal static string RemoveLayerPrompt
        {
            get
            {
                return StringResources.Get("RemoveLayerPrompt");
            }
        }

        internal static string RemoveLayerCaption
        {
            get
            {
                return StringResources.Get("RemoveLayerCaption");
            }
        }


        internal static string RemoveLayerNameSubstitude
        {
            get
            {
                return StringResources.Get("RemoveLayerNameSubstitude");
            }
        }

        #region Notification Panel Properties

        internal static string ToolClassLoadFailedExceptionHeader
        {
            get
            {
                return StringResources.Get("ToolClassLoadFailed");
            }
        }

        internal static string ToolClassLoadConfigurationFailedExceptionHeader
        {
            get
            {
                return StringResources.Get("ToolClassLoadConfigurationFailed");
            }
        }

        internal static string ExtensionLoadFailedExceptionHeader
        {
            get
            {
                return StringResources.Get("ExtensionLoadFailed");
            }
        }
        internal static string ExtensionInitializationFailed
        {
            get
            {
                return StringResources.Get("ExtensionInitializationFailed");
            }
        }
        internal static string ExtensionInitializationFailedHeader
        {
            get
            {
                return StringResources.Get("ExtensionInitializationFailedHeader");
            }
        }
        #endregion

        #region Geolist Specific properties
        internal static string InvalidDistance
        {
            get
            {
                return StringResources.Get("InvalidDistance");
            }
        }

        internal static string InvalidDistanceCaption
        {
            get
            {
                return StringResources.Get("InvalidDistanceCaption");
            }
        }

        internal static string FindNearbyTitle
        {
            get
            {
                return StringResources.Get("FindNearbyTitle");
            }
        }

        internal static string NoResults
        {
            get
            {
                return StringResources.Get("NoResults");
            }
        }

        internal static string NoResultsLayerNotVisible
        {
            get
            {
                return StringResources.Get("NoResultsLayerNotVisible");
            }
        }

        internal static string NoResultsCaption
        {
            get
            {
                return StringResources.Get("NoResultsCaption");
            }
        }

        internal static string Meters
        {
            get
            {
                return StringResources.Get("Meters");
            }
        }

        internal static string Kilometers
        {
            get
            {
                return StringResources.Get("Kilometers");
            }
        }

        internal static string Miles
        {
            get
            {
                return StringResources.Get("Miles");
            }
        }

        internal static string SelectedRecordsFormatString
        {
            get
            {
                return StringResources.Get("SelectedRecordsFormatString");
            }
        }

        internal static string Settings
        {
            get
            {
                return StringResources.Get("Settings");
            }
        }
        #endregion
    }
}
