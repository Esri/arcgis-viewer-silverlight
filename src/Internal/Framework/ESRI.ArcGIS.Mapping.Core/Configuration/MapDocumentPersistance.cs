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
using System.IO.IsolatedStorage;
using System.IO;

namespace ESRI.ArcGIS.Mapping.Core
{
    public static class MapDocumentUserPersistance
    {
        private const string FILENAME = "esri.arcgis.mapping.core.mapdocument.txt";
        private static object lockObject = new object();

        public static bool HasSavedUserDocument()
        {            
            IsolatedStorageFile isoStore = null;
            try
            {
                isoStore = IsolatedStorageFile.GetUserStoreForSite();
            }
            catch { }
            if (isoStore != null)
            {
                return isoStore.FileExists(FILENAME);
            }
            return false;
        }

        public static void SaveUserDocument(string documentContents)
        {
            if (string.IsNullOrWhiteSpace(documentContents))
                return;

            try
            {
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForSite();
                if (isoStore != null)
                {                    
                    lock (lockObject)
                    {
                        // Replace existing file
                        if(isoStore.FileExists(FILENAME))
                            isoStore.DeleteFile(FILENAME);

                        using (IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream(FILENAME, FileMode.OpenOrCreate, FileAccess.Write, isoStore))
                        {
                            using (StreamWriter writer = new StreamWriter(fileStream) { AutoFlush = true, })
                            {                                
                                writer.Write(documentContents);
                                writer.Flush();
                                writer.Close();
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public static string GetSavedUserDocument()
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
                                value = reader.ReadToEnd();
                                reader.Close();
                            }
                        }
                    }
                }
            }
            return value;
        }
    }
}
