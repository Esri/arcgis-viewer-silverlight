/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/


namespace ESRI.ArcGIS.Mapping.Core
{
    public class Constants
    {        
        public const string ArcGISServer = "ArcGIS Server";
        public const string SharePoint = "SharePoint";
        public const string ArcGISOnline = "ArcGIS Online";
        public const string BingMaps = "Bing Maps";
        public const string OpenStreetMap = "Open Street Map";        
        public const string SpatialDataService = "Spatial Data Service";

        public const string ClassBreaksRenderer = "ClassBreaksRenderer";
        public const string UniqueValueRenderer = "UniqueValueRenderer";
        public const string SimpleRenderer = "SimpleRenderer";

        public const string esriNamespace = "http://schemas.esri.com/arcgis/client/2009";
        public const string esriPrefix = "esri";
        public const string esriMappingNamespace = "http://schemas.esri.com/arcgis/mapping/2009";
        public const string esriMappingPrefix = "esriMapping";
        public const string esriBehaviorsNamespace = "clr-namespace:ESRI.ArcGIS.Mapping.Behaviors;assembly=ESRI.ArcGIS.Mapping.Behaviors";
        public const string esriBehaviorsPrefix = "eb";
        public const string esriExtensibilityNamespace = "http://schemas.esri.com/arcgis/client/extensibility/2010";
        public const string esriExtensibilityPrefix = "esriExtensibility";
        public const string sysPrefix = "sys";
        public const string sysNamespace = "clr-namespace:System;assembly=mscorlib";
        public const string esriFSSymbolsNamespace = "clr-namespace:ESRI.ArcGIS.Client.FeatureService.Symbols;assembly=ESRI.ArcGIS.Client";
        public const string esriFSSymbolsPrefix = "esriFSSymbols";
       
        public const double ZoomWidth = 1000;
        public const double ZoomWidthGSC = 0.008;
        public const double ReverseGeocodeTolerance = 20;

        public const string GRAPHIC_ID = "e.a.m.id"; // ESRI.ArcGIS.Mapping.ID
        public const int AutoClusterFeaturesThresholdLimit = 1000;
		public static string[] SupportedLanguages = { "ar", "cs", "da", "de", "es", "et", "fi", "fr", "he", "it", "ja", "ko", "lt", "lv", "nb-NO", "nl", "pl", "pt-BR", "pt-PT", "ro", "sv", "ru", "zh-CN" };
	}
}
