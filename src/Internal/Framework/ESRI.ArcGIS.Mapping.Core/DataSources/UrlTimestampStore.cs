/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;

namespace ESRI.ArcGIS.Mapping.Core.DataSources
{
    public class UrlTimeStampStore
    {
        private const string FILENAME = "esri.arcgis.mapping.url.timestamps.txt";
        private static object lockObject = new object();

        public static bool HasTimeStampForUrl(string url)
        {
            bool found = false;
            IsolatedStorageFile isoStore = null;
            try
            {
                isoStore = IsolatedStorageFile.GetUserStoreForSite();
            }
            catch { }
            if (isoStore != null)
            {
                if (isoStore.FileExists(FILENAME))
                {
                    lock (lockObject)
                    {
                        using (IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream(FILENAME, FileMode.Open, FileAccess.Read, isoStore))
                        {
                            using (StreamReader reader = new StreamReader(fileStream))
                            {
                                while (!reader.EndOfStream)
                                {
                                    string s = reader.ReadLine();
                                    if (string.IsNullOrEmpty(s))
                                        continue;
                                    if (s.StartsWith(url, StringComparison.OrdinalIgnoreCase))
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                reader.Close();
                            }
                        }
                    }
                }
            }
            return found;
        }

        public static string GetTimeStampForUrl(string url)
        {
            string value = null;
            IsolatedStorageFile isoStore = null;
            try
            {
                isoStore = IsolatedStorageFile.GetUserStoreForSite();
            }
            catch { }
            if (isoStore != null)
            {
                if (isoStore.FileExists(FILENAME))
                {
                    lock (lockObject)
                    {
                        using (IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream(FILENAME, FileMode.Open, FileAccess.Read, isoStore))
                        {
                            using (StreamReader reader = new StreamReader(fileStream))
                            {
                                while (!reader.EndOfStream)
                                {
                                    string s = reader.ReadLine();
                                    if (string.IsNullOrEmpty(s))
                                        continue;
                                    if (s.StartsWith(url, StringComparison.OrdinalIgnoreCase))
                                    {
                                        // value written was url concat with timestamp
                                        value = s.Replace(url, "");
                                        break;
                                    }
                                }
                                reader.Close();
                            }
                        }
                    }
                }
            }
            return value;
        }

        public static void SetTimeStampForUrl(string url, string timestamp)
        {
            try
            {
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForSite();
                if (isoStore != null)
                {
                    StringBuilder sb = new StringBuilder();
                    bool entryExists = false;
                    lock (lockObject)
                    {
                        using (IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream(FILENAME, FileMode.OpenOrCreate, FileAccess.Read, isoStore))
                        {
                            using (StreamReader reader = new StreamReader(fileStream))
                            {
                                while (!reader.EndOfStream)
                                {
                                    string s = reader.ReadLine();
                                    if (string.IsNullOrEmpty(s))
                                        continue;
                                    if (s.StartsWith(url, StringComparison.OrdinalIgnoreCase))
                                    {
                                        // value written was url concat with timestamp
                                        sb.AppendLine(url + timestamp);
                                        entryExists = true;
                                    }
                                    else
                                    {
                                        sb.AppendLine(s);
                                    }
                                }
                                reader.Close();
                            }
                        }
                    }

                    if (!entryExists)
                    {
                        sb.AppendLine(url + timestamp);
                    }

                    lock (lockObject)
                    {
                        using (IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream(FILENAME, FileMode.Truncate, FileAccess.Write, isoStore))
                        {
                            using (StreamWriter writer = new StreamWriter(fileStream))
                            {
                                // value written is url concat with timestamp
                                writer.Write(sb.ToString());
                                writer.Close();
                            }
                        }
                    }
                }
            }
            catch { }
        }
    }
}
