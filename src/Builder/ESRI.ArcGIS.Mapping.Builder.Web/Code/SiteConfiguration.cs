/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ESRI.ArcGIS.Mapping.Builder.Common;

    

    namespace ESRI.ArcGIS.Mapping.Builder.Web
    {
        public abstract class SiteConfiguration
        {
            private const string SITESCONFIGFILE = "~/App_Data/Sites.xml";
            #region Private Members            
            private static object _lockobject = new object();            

            private static string SiteConfigPath
            {
                get
                {
                    return ServerUtility.MapPath(SITESCONFIGFILE);
                }
            }

            /// <summary>
            /// Load the sites from the config file.
            /// </summary>
            private static Sites loadSites()
            {
                string configPath = SiteConfigPath;
                Sites sites = null;
                if (File.Exists(configPath))
                {                    
                    try
                    {
                        XmlSerializer reader = new XmlSerializer(typeof(Sites));
                        using (System.IO.StreamReader file = new System.IO.StreamReader(configPath))
                        {
                            sites = (Sites)reader.Deserialize(file);
                        }
                    }
                    catch (Exception) {
                        // TODO:- log error
                        sites = new Sites();
                    }
                }
                else
                    sites = new Sites();
                return sites;
            }

            private static void saveSites(Sites sites)
            {
                try
                {
                    string configPath = SiteConfigPath;
                    
                    XmlSerializer writer = new XmlSerializer(typeof(Sites));
                    using (StreamWriter file = new StreamWriter(configPath))
                    {
                        writer.Serialize(file, sites);
                    }
                }
                catch { }
            }
            #endregion

            /// <summary>
            /// Return a read-only array of Sites. Must call the Add and Remove method of this
            /// class to update the site list. Each element of the array, however, is updatable.
            /// </summary>
            /// <returns></returns>
            public static List<Site> GetSites()
            {
                lock (_lockobject)
                {
                    return loadSites();
                }
            }

            public static Site FindExistingSiteByID(string id)
            {
                lock (_lockobject)
                {
                    return FindSiteById(GetSites(), id);
                }
            }

            private static Site FindSiteById(List<Site> sites, string id)
            {
                if (sites != null)
                {
                    foreach (Site s in sites)
                    {
                        if (string.Compare(s.ID, id) == 0)
                        {
                            return s;
                        }
                    }
                }
                return null;
            }

            /// <summary>
            /// Saves the specified site's metadata to disk
            /// </summary>
            internal static bool SaveSite(Site site)
            {
                bool saved = false;

                // Get all the sites
                Sites sites = loadSites();

                // Replace the site that has the same ID as the passed-in site
                Site siteToReplace = FindSiteById(sites, site.ID);
                if (siteToReplace != null)
                {
                    int insertIndex = sites.IndexOf(siteToReplace);
                    sites.Remove(siteToReplace);
                    sites.Insert(insertIndex, site);
                    saveSites(sites);
                    saved = true;
                }

                return saved;
            }

            public static void DeleteSite(string siteID)
            {
                lock (_lockobject)
                {
                    Sites sites = loadSites();
                    Site theSite = FindSiteById(sites, siteID);
                    if (theSite != null)
                    {
                        sites.Remove(theSite);
                        saveSites(sites);
                    }
                }
            }

            public static void AddSite(Site site)
            {
                lock (_lockobject)
                {
                    Sites sites = loadSites();
                    sites.Add(site);
                    saveSites(sites);
                }
            }            
        }
    }
