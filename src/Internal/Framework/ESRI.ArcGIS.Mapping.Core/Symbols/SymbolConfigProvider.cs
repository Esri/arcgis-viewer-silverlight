/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Mapping.Core.Symbols;
using System.Xml.Linq;
using System.Json;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class SymbolConfigProvider
    {
        private IEnumerable<SymbolResourceDictionaryEntry> m_symbolResourceDictionaries;        
        private IEnumerable<LinearGradientBrush> m_linearGradientBrushes;
        private IEnumerable<LinearGradientBrush> m_heatMapGradientBrushes;
        private const string PICTUREMARKERSYMBOL = "PictureMarkerSymbol";

        public virtual void GetSymbolResourceDictionaryEntriesAsync(GeometryType geometryType, object userState)
        {
            IEnumerable<SymbolResourceDictionaryEntry> dictEntries = null;
            try
            {
                dictEntries = GetSymbolResourceDictionaryEntries(geometryType);
            }
            catch (Exception ex)
            {
                OnGetResourceDictionariesFailed(new ExceptionEventArgs(ex, userState));
            }
            OnGetResourceDictionariesCompleted(new GetSymbolResourceDictionaryEntriesCompletedEventArgs() { SymbolResourceDictionaries = dictEntries, UserState = userState });
        }

        public IEnumerable<SymbolResourceDictionaryEntry> GetSymbolResourceDictionaryEntries(GeometryType geometryType)
        {
            if(m_symbolResourceDictionaries == null)
                m_symbolResourceDictionaries = GetSymbolResourceDictionaryEntries();
            if (m_symbolResourceDictionaries == null)
                return null;

            List<SymbolResourceDictionaryEntry> list = new List<SymbolResourceDictionaryEntry>();            
            foreach (SymbolResourceDictionaryEntry entry in m_symbolResourceDictionaries)
            {                
                if (entry.GeometryType == geometryType || (entry.GeometryType == GeometryType.Point && geometryType == GeometryType.MultiPoint))
                    list.Add(entry);
            }
            return list;
        }

        public IEnumerable<SymbolResourceDictionaryEntry> GetSymbolResourceDictionaryEntries()
        {
            string symbolConfigXml = null;
            Assembly a = typeof(SymbolConfigProvider).Assembly;
            using (Stream str = a.GetManifestResourceStream("ESRI.ArcGIS.Mapping.Core.Embedded.DefaultSymbols.xml"))
            {
                using (StreamReader rdr = new StreamReader(str))
                {
                    symbolConfigXml = rdr.ReadToEnd();
                }
            }

            if (string.IsNullOrEmpty(symbolConfigXml))
            {
				throw new Exception(Resources.Strings.ExceptionSymbolConfigXmlEmpty);
            }

            return ParseResourceDictionaryEntriesFromXml(symbolConfigXml);
        }
		private static System.Collections.ObjectModel.ObservableCollection<ImageSymbolEntry> imageSymbolLookup;
		public static System.Collections.ObjectModel.ObservableCollection<ImageSymbolEntry> ImageSymbolLookup
		{
			get{
				if (imageSymbolLookup == null)
				{
					imageSymbolLookup = GetImageSymbolEntries();
				}
					return imageSymbolLookup;
			}
		}

		private static System.Collections.ObjectModel.ObservableCollection<ImageSymbolEntry> GetImageSymbolEntries()
		{
			Assembly a = typeof(SymbolConfigProvider).Assembly;
			using (Stream str = a.GetManifestResourceStream("ESRI.ArcGIS.Mapping.Core.Embedded.ImageSymbolLookup.xml"))
			{
				var xmlSerializer = new System.Runtime.Serialization.DataContractSerializer(typeof(System.Collections.ObjectModel.ObservableCollection<ImageSymbolEntry>));
				str.Position = 0;
				var imageSymbolLookup = (System.Collections.ObjectModel.ObservableCollection<ImageSymbolEntry>)xmlSerializer.ReadObject(str);
				str.Close();
				return imageSymbolLookup;
			}
		}

        protected IEnumerable<SymbolResourceDictionaryEntry> ParseResourceDictionaryEntriesFromXml(string symbolConfigXml)
        {
            List<SymbolResourceDictionaryEntry> dictEntries = new List<SymbolResourceDictionaryEntry>();
            XDocument xDoc = XDocument.Parse(symbolConfigXml);
            foreach (XElement elem in xDoc.Root.Elements("ResourceDictionary"))
            {
                SymbolResourceDictionaryEntry entry = new SymbolResourceDictionaryEntry()
                {
                    DisplayName = elem.Element("DisplayName").Value,
                    GeometryType = (GeometryType)Enum.Parse(typeof(GeometryType), elem.Element("GeometryType").Value, true),
                    ID = elem.Element("ID").Value,
                    Path = elem.Element("Path").Value,
                };
                dictEntries.Add(entry);
            }
            return dictEntries;
        }

        public virtual void GetColorGradientBrushesAsync(object userState, ColorRampType colorRampType)
        {
            IEnumerable<LinearGradientBrush> brushes = getColorRampBrushes(colorRampType);

            if (brushes == null)
            {
				OnGetColorGradientBrushesFailed(new ExceptionEventArgs(Resources.Strings.ExceptionUnableToRetrieveLinearGradientBrushes, userState));
                return;
            }

            OnGetColorGradientBrushesCompleted(new GetColorGradientBrushesCompletedEventArgs() { ColorBrushes = brushes, UserState = userState });
        }


        string getColorRampResourceName(ColorRampType colorRampType)
        {
            string resourceName = null;;
            switch (colorRampType)
            {
                case ColorRampType.HeatMap:
                    resourceName = "ESRI.ArcGIS.Mapping.Core.Embedded.HeatMapBrushes.xaml";
                    break;
                case ColorRampType.ClassBreaks:
                case ColorRampType.UniqueValue:
                default:
                    resourceName = "ESRI.ArcGIS.Mapping.Core.Embedded.ThematicMapBrushes.xaml";
                    break;
            }
            return resourceName;
        }

        internal IEnumerable<LinearGradientBrush> getColorRampBrushes(ColorRampType colorRampType)
        {
            IEnumerable<LinearGradientBrush> brushes = null;
            switch (colorRampType)
            {
                case ColorRampType.HeatMap:
                    brushes = m_heatMapGradientBrushes;
                    break;
                case ColorRampType.ClassBreaks:
                case ColorRampType.UniqueValue:
                default:
                    brushes = m_linearGradientBrushes;
                    break;
            }

            if (brushes == null)
            {
                brushes = getLinearGradientBrushes(getColorRampResourceName(colorRampType));
                switch (colorRampType)
                {
                    case ColorRampType.HeatMap:
                        m_heatMapGradientBrushes = brushes;
                        break;
                    case ColorRampType.ClassBreaks:
                    case ColorRampType.UniqueValue:
                    default:
                        m_linearGradientBrushes = brushes;
                        break;
                }
            }
            return brushes;
        }

        private IEnumerable<LinearGradientBrush> getLinearGradientBrushes(string resourceName)
        {
            string linearGradientsBrushXaml = null;
            Assembly a = typeof(SymbolConfigProvider).Assembly;
            try
            {
               using (Stream str = a.GetManifestResourceStream(resourceName))
               {
                   using (StreamReader rdr = new StreamReader(str))
                   {
                       linearGradientsBrushXaml = rdr.ReadToEnd();
                   }
               }
            }
            catch (Exception)
            {                
                return null;
            }
            
            return GetGradientBrushesFromXaml(linearGradientsBrushXaml);
        }

        protected IEnumerable<LinearGradientBrush> GetGradientBrushesFromXaml(string linearGradientsBrushXaml)
        {
            ResourceDictionary resourceDictionary = null;
            try
            {
                resourceDictionary = XamlReader.Load(linearGradientsBrushXaml) as ResourceDictionary;
            }
            catch (Exception)
            {
                return null;
            }
            
            if (resourceDictionary == null)
                return null;

            List<LinearGradientBrush> brushes = new List<LinearGradientBrush>();
            foreach (LinearGradientBrush brush in resourceDictionary.Values)
            {
                brushes.Add(brush);
            }
            return brushes;
        }

        protected virtual void OnSymbolSetContentsLoaded(string symbolSetContents, object userState)
        {
            if (string.IsNullOrEmpty(symbolSetContents))
            {
				OnGetResourceDictionariesFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionResourceDictionaryEmpty), userState));
                return;
            }

            try
            {
                IEnumerable<SymbolDescription> descr = GetSymbolDescriptionsForJsonSymbolSet(symbolSetContents);
                if (descr == null)
                {
					OnGetResourceDictionariesFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionSymbolDescriptionValueNull), userState));
                    return;
                }

                OnGetSymbolsForResourceDictionaryCompleted(new GetSymbolsForResourceDictionaryCompletedEventArgs()
                {
                    Symbols = descr,
                    UserState = userState,
                });
            }
            catch (Exception ex)
            {
                OnGetSymbolsForResourceDictionaryFailed(new ExceptionEventArgs(ex, userState));
            }
        }
        protected IEnumerable<SymbolDescription> GetSymbolDescriptionsForJsonSymbolSet(string jsonSymbolSet)
        {
            try
            {
                string json = jsonSymbolSet;
            	JsonArray jsarray = (JsonArray)JsonArray.Parse(json);
                List<SymbolDescription> list = new List<SymbolDescription>();

                var jsymbols = from jsymbol in jsarray
                               select jsymbol;
                foreach (JsonObject j in jsymbols)
                {
                    SymbolDescription description = new SymbolDescription();
                    JsonObject symb = j["symbol"] as JsonObject;
                    description.Symbol = SymbolJsonHelper.SymbolFromJson(symb);
                    description.DisplayName = j["id"];
                    list.Add(description);
                }
                return list;
            }
            catch {}
            return null;

        }

        public virtual void GetSymbolsForResourceDictionaryAsync(SymbolResourceDictionaryEntry resourceDictionary, object userState)
        {
            string resourceDictionaryXaml = getResourceDictionaryContents(resourceDictionary);            
            if(string.IsNullOrEmpty(resourceDictionaryXaml))
            {
				OnGetSymbolsForResourceDictionaryFailed(new ExceptionEventArgs(Resources.Strings.ExceptionUnableToretrieveResourceDictionaryContents, userState));
                return;
            }

            OnSymbolSetContentsLoaded(resourceDictionaryXaml, userState);
        }

        public string getResourceDictionaryContents(SymbolResourceDictionaryEntry resourceDictionary)
        {
            string resourceDictionaryXaml = null;
            Assembly a = typeof(SymbolConfigProvider).Assembly;
            try
            {
                string path = resourceDictionary.Path;
                if (!string.IsNullOrEmpty(path))
                {
                    using (Stream str = a.GetManifestResourceStream("ESRI.ArcGIS.Mapping.Core.Embedded." + path.Replace("/", ".")))
                    {
                        using (StreamReader rdr = new StreamReader(str))
                        {
                            resourceDictionaryXaml = rdr.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception)
            {                
                return null;
            }
            return resourceDictionaryXaml;
        }

        public virtual void GetDefaultSymbol(GeometryType geometryType, object userState)
        {
            if (geometryType == GeometryType.Unknown)
            {
                OnGetDefaultSymbolFailed(new ExceptionEventArgs(string.Format(Resources.Strings.ExceptionNoResourceDictionaryAvailableForGeometryType, geometryType), userState));
                return;
            }

            IEnumerable<SymbolResourceDictionaryEntry> entries = GetSymbolResourceDictionaryEntries(geometryType);
            if (entries == null || entries.Count() < 1)
            {
                OnGetDefaultSymbolFailed(new ExceptionEventArgs(string.Format(Resources.Strings.ExceptionNoResourceDictionaryAvailableForGeometryType, geometryType), userState));
                return;
            }

            SymbolResourceDictionaryEntry entry = entries.ElementAt(0);
                string resourceDictionaryContents = getResourceDictionaryContents(entry);
                IEnumerable<SymbolDescription> symbols = GetSymbolDescriptionsForJsonSymbolSet(resourceDictionaryContents);
                if (symbols == null || symbols.Count() < 1)
            {
                OnGetDefaultSymbolFailed(new ExceptionEventArgs(string.Format(Resources.Strings.ExceptionNoSymbolAvailableInResourceDictionary, entry.DisplayName), userState));
                return;
            }

                OnGetDefaultSymbolCompleted(new GetDefaultSymbolCompletedEventArgs()
                {
                    DefaultSymbol = symbols.ElementAt(0),
                    UserState = userState,
                    GeometryType = geometryType,
                });
                
        }

        public virtual void GetDefaultLinearGradientBrush(object userState, ColorRampType colorRampType)
        {
            IEnumerable<LinearGradientBrush> brushes = getColorRampBrushes(colorRampType);

            if (brushes == null || brushes.Count() < 1)
            {
                OnGetDefaultLinearGradientBrushFailed(new ExceptionEventArgs(Resources.Strings.ExceptionUnableToRetrieveLinearGradientBrushes, userState));
                return;
            }
            LinearGradientBrush brush = brushes.ElementAtOrDefault(0);


            if (brush == null)
            {
				OnGetDefaultLinearGradientBrushFailed(new ExceptionEventArgs(Resources.Strings.ExceptionUnableToretrievedefaultLinearGradientBrushes, userState));
                return;
            }

            OnGetDefaultLinearGradientBrushCompleted(new GetDefaultLinearGradientBrushEventArgs()
            {
                DefaultBrush = brush,
                UserState = userState,
            });
        }

        #region Events
        protected void OnGetSymbolsForResourceDictionaryCompleted(GetSymbolsForResourceDictionaryCompletedEventArgs args)
        {
            if (GetSymbolsForResourceDictionaryCompleted != null)
                GetSymbolsForResourceDictionaryCompleted(this, args);
        }

        protected void OnGetResourceDictionariesCompleted(GetSymbolResourceDictionaryEntriesCompletedEventArgs args)
        {
            if (GetSymbolCategoriesCompleted != null)
                GetSymbolCategoriesCompleted(this, args);
        }

        protected void OnGetResourceDictionariesFailed(ExceptionEventArgs args)
        {
            if (GetSymbolResourceDictionaryEntriesFailed != null)
                GetSymbolResourceDictionaryEntriesFailed(this, args);
        }

        protected void OnGetSymbolsForResourceDictionaryFailed(ExceptionEventArgs args)
        {
            if (GetSymbolsForResourceDictionaryFailed != null)
                GetSymbolsForResourceDictionaryFailed(this, args);
        }

        protected void OnGetColorGradientBrushesCompleted(GetColorGradientBrushesCompletedEventArgs args)
        {
            if (GetColorGradientBrushesCompleted != null)
                GetColorGradientBrushesCompleted(this, args);
        }

        protected void OnGetColorGradientBrushesFailed(ExceptionEventArgs args)
        {
            if (GetColorGradientBrushesFailed != null)
                GetColorGradientBrushesFailed(this, args);
        }

        protected void OnGetDefaultSymbolCompleted(GetDefaultSymbolCompletedEventArgs args)
        {
            if (GetDefaultSymbolCompleted != null)
                GetDefaultSymbolCompleted(this, args);
        }

        protected void OnGetDefaultSymbolFailed(ExceptionEventArgs args)
        {
            if (GetDefaultSymbolFailed != null)
                GetDefaultSymbolFailed(this, args);
        }

        protected void OnGetDefaultLinearGradientBrushCompleted(GetDefaultLinearGradientBrushEventArgs args)
        {
            if (GetDefaultLinearGradientBrushCompleted != null)
                GetDefaultLinearGradientBrushCompleted(this, args);
        }

        protected void OnGetDefaultLinearGradientBrushFailed(ExceptionEventArgs args)
        {
            if (GetDefaultLinearGradientBrushFailed != null)
                GetDefaultLinearGradientBrushFailed(this, args);
        }

        public event EventHandler<ExceptionEventArgs> GetSymbolsForResourceDictionaryFailed;
        public event EventHandler<GetSymbolsForResourceDictionaryCompletedEventArgs> GetSymbolsForResourceDictionaryCompleted;
        public event EventHandler<ExceptionEventArgs> GetSymbolResourceDictionaryEntriesFailed;
        public event EventHandler<GetSymbolResourceDictionaryEntriesCompletedEventArgs> GetSymbolCategoriesCompleted;
        public event EventHandler<GetColorGradientBrushesCompletedEventArgs> GetColorGradientBrushesCompleted;
        public event EventHandler<ExceptionEventArgs> GetColorGradientBrushesFailed;
        public event EventHandler<GetDefaultSymbolCompletedEventArgs> GetDefaultSymbolCompleted;
        public event EventHandler<ExceptionEventArgs> GetDefaultSymbolFailed;
        public event EventHandler<GetDefaultLinearGradientBrushEventArgs> GetDefaultLinearGradientBrushCompleted;
        public event EventHandler<ExceptionEventArgs> GetDefaultLinearGradientBrushFailed;
        #endregion
    }

    public class GetColorGradientBrushesCompletedEventArgs : EventArgs
    {
        public IEnumerable<LinearGradientBrush> ColorBrushes { get; set; }
        public object UserState { get; set; }
    }

    public class GetSymbolResourceDictionaryEntriesCompletedEventArgs : EventArgs
    {
        public IEnumerable<SymbolResourceDictionaryEntry> SymbolResourceDictionaries { get; set; }
        public object UserState { get; set; }
    }

    public class GetSymbolsForResourceDictionaryCompletedEventArgs : EventArgs
    {
        public IEnumerable<SymbolDescription> Symbols { get; set; }
        public object UserState { get; set; }
    }

    public class GetDefaultSymbolCompletedEventArgs : EventArgs
    {
        public SymbolDescription DefaultSymbol { get; set; }
        public GeometryType GeometryType { get; set; }
        public object UserState { get; set; }
    }

    public class GetDefaultLinearGradientBrushEventArgs : EventArgs
    {
        public LinearGradientBrush DefaultBrush { get; set; }
        public ColorRampType ColorRampType { get; set; }
        public object UserState { get; set; }
    }
}
