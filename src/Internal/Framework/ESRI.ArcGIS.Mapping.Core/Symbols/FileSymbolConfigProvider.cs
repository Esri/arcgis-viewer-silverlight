/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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
using System.Collections.Generic;
using System.Linq;
using ESRI.ArcGIS.Mapping.Core.Symbols;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class FileSymbolConfigProvider : SymbolConfigProvider
    {
        private IEnumerable<SymbolResourceDictionaryEntry> m_symbolResourceDictionaries;
        private IEnumerable<LinearGradientBrush> m_defaultLinearGradientBrushes;
        private IEnumerable<LinearGradientBrush> m_heatMapLinearGradientBrushes;

        public string ConfigFileUrl
        {
            get;
            set;
        }

        public string SymbolFolderParentUrl
        {
            get;
            set;
        }

        public string  ClassBreaksColorGradientsConfigFileUrl
        {
            get;
            set;
        }
        public string UniqueValueColorGradientsConfigFileUrl
        {
            get;
            set;
        }
        public string HeatMapColorGradientsConfigFileUrl
        {
            get;
            set;
        }

        public override void GetSymbolResourceDictionaryEntriesAsync(GeometryType geometryType, object userState)
        {
            if (m_symbolResourceDictionaries != null)
            {
                List<SymbolResourceDictionaryEntry> resourceDictionaries = new List<SymbolResourceDictionaryEntry>();
                foreach (SymbolResourceDictionaryEntry entry in m_symbolResourceDictionaries)
                {
                    if (entry.GeometryType == geometryType || (entry.GeometryType == GeometryType.Point && geometryType == GeometryType.MultiPoint))
                        resourceDictionaries.Add(entry);
                }
                OnGetResourceDictionariesCompleted(new GetSymbolResourceDictionaryEntriesCompletedEventArgs() { SymbolResourceDictionaries = resourceDictionaries, UserState = userState });
                return;
            }

            #region Validation
            if (ConfigFileUrl == null)
            {
                base.OnGetResourceDictionariesFailed(new ExceptionEventArgs(Resources.Strings.ExceptionConfigFileUrlMustBeSpecified, userState));
                return;
            }

            if (SymbolFolderParentUrl == null)
            {
                base.OnGetResourceDictionariesFailed(new ExceptionEventArgs(Resources.Strings.ExceptionSymbolFolderParentUrlMustBeSpecified, userState));
                return;
            }
            #endregion

            getSymbolResourceDictionaryEntries(geometryType, userState, (o, e) => {                
                OnGetResourceDictionariesCompleted(e);
            }, (o, e) => {
                    OnGetResourceDictionariesFailed(e);
            });
        }

        private void getSymbolResourceDictionaryEntries(GeometryType geometryType, object userState, EventHandler<GetSymbolResourceDictionaryEntriesCompletedEventArgs> onCompleted, EventHandler<ExceptionEventArgs> onFailed)
        {
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += (o, e) =>
            {
                if (e.Error != null)
                {
                    onFailed(this, new ExceptionEventArgs(e.Error, userState));
                    return;
                }
                else if (string.IsNullOrEmpty(e.Result))
                {
                    onFailed(this, new ExceptionEventArgs(Resources.Strings.ExceptionConfigurationFileEmpty, userState));
                    return;
                }

                IEnumerable<SymbolResourceDictionaryEntry> entries = null;
                try
                {
                    entries = ParseResourceDictionaryEntriesFromXml(e.Result);
                }
                catch (Exception ex)
                {
                    onFailed(this, new ExceptionEventArgs(ex, userState));
                    return;
                }

                // Prefix each of the entries with symbol path
                foreach (SymbolResourceDictionaryEntry entry in entries)
                {
                    entry.Path = string.Format("{0}/{1}", SymbolFolderParentUrl, entry.Path);
                }
                m_symbolResourceDictionaries = entries;

                List<SymbolResourceDictionaryEntry> filteredEntries = new List<SymbolResourceDictionaryEntry>();
                foreach (SymbolResourceDictionaryEntry entry in m_symbolResourceDictionaries)
                {
                    if (entry.GeometryType == geometryType || (entry.GeometryType == GeometryType.Point && geometryType == GeometryType.MultiPoint))
                        filteredEntries.Add(entry);
                }

                onCompleted(this, new GetSymbolResourceDictionaryEntriesCompletedEventArgs()
                {
                    SymbolResourceDictionaries = filteredEntries,
                    UserState = e.UserState,
                });
            };
            wc.DownloadStringAsync(new Uri(ConfigFileUrl, UriKind.RelativeOrAbsolute), userState);
        }

        public override void GetSymbolsForResourceDictionaryAsync(SymbolResourceDictionaryEntry resourceDictionary, object userState)
        {
            if (!Uri.IsWellFormedUriString(resourceDictionary.Path, UriKind.RelativeOrAbsolute))
            {
				base.OnGetSymbolsForResourceDictionaryFailed(new ExceptionEventArgs(string.Format(Resources.Strings.ExceptionInvalidPathForresourceDictionary, resourceDictionary.Path), userState));
                return;
            }

            getSymbolsForResourceDictionary(resourceDictionary, userState,
                (o, e) =>
                {
                    OnGetSymbolsForResourceDictionaryCompleted(e);                    
                },
                (o, e) =>
                {
                    OnGetSymbolsForResourceDictionaryFailed(e);                    
                });
        }

        public override void GetColorGradientBrushesAsync(object userState, ColorRampType colorRampType)
        {
            getColorGradientBrushes(colorRampType, userState, (o, e) =>
            {
                switch (colorRampType)
                {
                    case ColorRampType.HeatMap:
                        m_heatMapLinearGradientBrushes = e.ColorBrushes;
                        break;
                    case ColorRampType.ClassBreaks:
                    case ColorRampType.UniqueValue:
                    default:
                        m_defaultLinearGradientBrushes = e.ColorBrushes;
                        break;
                }

                base.OnGetColorGradientBrushesCompleted(e);
            }, (o, e) =>
            {
                base.OnGetColorGradientBrushesFailed(e);
            });
        }

        public override void GetDefaultLinearGradientBrush(object userState, ColorRampType colorRampType)
        {
            getColorGradientBrushes(colorRampType, userState, (o, e) =>
            {
                LinearGradientBrush brush = (e.ColorBrushes != null) ? e.ColorBrushes.ElementAtOrDefault(0) : null;
                   
                if (brush == null)
                {
                    base.OnGetDefaultSymbolFailed(new ExceptionEventArgs(Resources.Strings.ExceptionNoBrushesFound, userState));
                    return;
                }
                base.OnGetDefaultLinearGradientBrushCompleted(new GetDefaultLinearGradientBrushEventArgs() { DefaultBrush = brush, UserState = userState });
            }, (o, e) =>
            {
                base.OnGetDefaultSymbolFailed(e);
            }); 
        }

        private void getColorGradientBrushes(ColorRampType colorRampType, object userState, EventHandler<GetColorGradientBrushesCompletedEventArgs> onCompleted, EventHandler<ExceptionEventArgs> onFailed)
        {

            string location = ClassBreaksColorGradientsConfigFileUrl;
            if (colorRampType == ColorRampType.HeatMap)
                location = HeatMapColorGradientsConfigFileUrl;
            if (null == location)
            {
                IEnumerable<LinearGradientBrush> brushes = base.getColorRampBrushes(colorRampType);
                if (onCompleted != null)
                {
                    onCompleted(this, new GetColorGradientBrushesCompletedEventArgs()
                    {
                        ColorBrushes = brushes,
                        UserState = userState,
                    });
                }
                return;
            }
            else
            {
                WebClient wc = new WebClient();
                wc.DownloadStringCompleted += (o, e) =>
                {
                    if (e.Error != null)
                    {
                        if (onFailed != null)
                            onFailed(this, new ExceptionEventArgs(e.Error, e.UserState));
                        return;
                    }

                    if (string.IsNullOrEmpty(e.Result))
                    {
                        if (onFailed != null)
                            onFailed(this, new ExceptionEventArgs(Resources.Strings.ExceptionConfigurationFileEmpty, e.UserState));
                        return;
                    }

                    IEnumerable<LinearGradientBrush> brushes = null;
                    try
                    {
                        brushes = GetGradientBrushesFromXaml(e.Result);
                    }
                    catch (Exception ex)
                    {
                        if (onFailed != null)
                            onFailed(this, new ExceptionEventArgs(ex, e.UserState));
                        return;
                    }

                    if (onCompleted != null)
                    {
                        onCompleted(this, new GetColorGradientBrushesCompletedEventArgs()
                        {
                            ColorBrushes = brushes,
                            UserState = e.UserState,
                        });
                    }
                };
                wc.DownloadStringAsync(new Uri(location, UriKind.Absolute), userState);
            }
        }

        public override void GetDefaultSymbol(GeometryType geometryType, object userState)
        {
            #region Validation
            if (null == ConfigFileUrl)
            {
                base.OnGetDefaultSymbolFailed(new ExceptionEventArgs(Resources.Strings.ExceptionConfigFileUrlMustBeSpecified, userState));
                return;
            }

            if (null == SymbolFolderParentUrl)
            {
                base.OnGetDefaultSymbolFailed(new ExceptionEventArgs(Resources.Strings.ExceptionSymbolFolderParentUrlMustBeSpecified, userState));
                return;
            }
            #endregion

            if (geometryType == GeometryType.Unknown)
            {
                base.GetDefaultSymbol(geometryType, userState);
                return;
            }

            if (m_symbolResourceDictionaries == null)
            {
                getSymbolResourceDictionaryEntries(geometryType, userState, (o, e) =>
                {
                    if (e.SymbolResourceDictionaries == null || e.SymbolResourceDictionaries.Count() < 1)
                    {
                        base.OnGetDefaultSymbolFailed(new ExceptionEventArgs(string.Format(Resources.Strings.ExceptionNoResourceDictionaryfoundForGeometryType, geometryType), userState));
                        return;
                    }

                    SymbolResourceDictionaryEntry entry = e.SymbolResourceDictionaries.ElementAt(0);
                    getSymbolsForResourceDictionary(entry, userState, (o2, e2) =>
                    {
                        onGetSymbolsForResourceDictionariesCompleted(geometryType, entry, e2, userState);
                    }, (o2, e2) =>
                    {
                        base.OnGetDefaultSymbolFailed(e2);
                    });

                }, (o, e) =>
                {
                    base.OnGetDefaultSymbolFailed(e);
                });
            }
            else
            {
                // Find the one for the geometry type
                SymbolResourceDictionaryEntry entry = m_symbolResourceDictionaries.FirstOrDefault<SymbolResourceDictionaryEntry>(
                    e => e.GeometryType == geometryType
                    || (e.GeometryType == GeometryType.Point && geometryType == GeometryType.MultiPoint));
                if (entry == null)
                {
                    base.OnGetDefaultSymbolFailed(new ExceptionEventArgs(string.Format(Resources.Strings.ExceptionNoResourceDictionaryfoundForGeometryType, geometryType), userState));
                    return;
                }
                getSymbolsForResourceDictionary(entry, userState, (o2, e2) =>
                {
                    onGetSymbolsForResourceDictionariesCompleted(geometryType, entry, e2, userState);
                }, (o2, e2) =>
                {
                    base.OnGetDefaultSymbolFailed(e2);
                });
            }
        }

        private void onGetSymbolsForResourceDictionariesCompleted(GeometryType geometryType, SymbolResourceDictionaryEntry entry, GetSymbolsForResourceDictionaryCompletedEventArgs e2, object userState)
        {
            IEnumerable<SymbolDescription> symbols = e2.Symbols;
            if (symbols == null || symbols.Count() < 1)
            {
                base.OnGetDefaultSymbolFailed(new ExceptionEventArgs(string.Format(Resources.Strings.ExceptionNoDefaultSymbolsFoundInDictionaryForGeometryType, entry.DisplayName, geometryType), userState));
                return;
            }

            base.OnGetDefaultSymbolCompleted(new GetDefaultSymbolCompletedEventArgs() { DefaultSymbol = symbols.ElementAt(0), UserState = userState, GeometryType = geometryType });
        }

        private void getSymbolsForResourceDictionary(SymbolResourceDictionaryEntry resourceDictionary, object userState,
            EventHandler<GetSymbolsForResourceDictionaryCompletedEventArgs> onCompletedHandler, EventHandler<ExceptionEventArgs> onFailedHandler)
        {
            WebClient wc = new WebClient();            
            wc.DownloadStringCompleted += (o, e) =>
            {
                if (e.Error != null)
                {
                    if (onFailedHandler != null)
                        onFailedHandler(this, new ExceptionEventArgs(e.Error, userState));
                    return;
                }
                try
                {
                    IEnumerable<SymbolDescription> symbols = base.GetSymbolDescriptionsForJsonSymbolSet(e.Result);
                    if (onCompletedHandler != null)
                        onCompletedHandler(this, new GetSymbolsForResourceDictionaryCompletedEventArgs() { Symbols = symbols, UserState = userState });                
                }
                catch (Exception ex)
                {
                    if (onFailedHandler != null)
                        onFailedHandler(this, new ExceptionEventArgs(ex, userState));
                }

             };
            wc.DownloadStringAsync(new Uri(resourceDictionary.Path, UriKind.RelativeOrAbsolute), userState);
        }  
    }
}
