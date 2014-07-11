/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Xml;
using ESRI.ArcGIS.Mapping.Builder.Common;
using System.Xml.Serialization;

namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    internal static class ExtensionsManager
    {        
        static object lockObject = new object();
        private static string ExtensionsConfigFilePath
        {
            get
            {
                string extensionsConfigFile = "~/App_Data/Extensions.xml";
                return ServerUtility.MapPath(extensionsConfigFile);
            }
        }

        private static string ExtensionsFolderPath
        {
            get
            {
                string folderPath = "~/Extensions";
                return ServerUtility.MapPath(folderPath);
            }
        }

        private static Extensions getExtensions()
        {
            string configPath = ExtensionsConfigFilePath;
            Extensions _extensions = null;
            if (File.Exists(configPath))
            {
                try
                {
                    XmlSerializer reader = new XmlSerializer(typeof(Extensions));
                    using (System.IO.StreamReader file = new System.IO.StreamReader(configPath))
                    {
                        _extensions = (Extensions)reader.Deserialize(file);
                    }
                }
                catch {
                    _extensions = new Extensions();
                }
            }
            else
                _extensions = new Extensions();
            return _extensions;
        }

        public static Extensions GetExtensionLibraries()
        {
            lock (lockObject)
            {  
                Extensions _extensions = getExtensions();
                return _extensions;
            }
        }

        public static void AddExtensionLibraryToCatalog(string extensionFileName, IEnumerable<string> assembliesInExtension)
        {
            lock (lockObject)
            {
                Extensions extensions = getExtensions();
                Extension extension = null;
                foreach (Extension ext in extensions)
                {
                    if (string.Compare(ext.Name, extensionFileName, true) == 0)
                    {
                        extension = ext;
                        break;
                    }
                }
                if (extension == null)
                {
                    extension = new Extension(assembliesInExtension);
                    if (extensionFileName.EndsWith(".xap", StringComparison.OrdinalIgnoreCase))
                    {
                        // remove the .xap extension
                        extensionFileName = extensionFileName.Substring(0, extensionFileName.Length - 4);
                    }
                    extension.Name = extensionFileName;
                    extensions.Add(extension);
                }
                else
                {
                    // just modify the assemblies collection
                    extension.Assemblies = new List<Assembly>();
                    if (assembliesInExtension != null)
                    {
                        foreach (string assemblyName in assembliesInExtension)
                            extension.Assemblies.Add(new Assembly(assemblyName));
                    }
                }
                saveExtensions(extensions);
            }            
        }

        public static void RemoveExtensionLibraryFromCatalog(string extensionFileName)
        {
            lock (lockObject)
            {
                Extensions extensions = getExtensions();                
                foreach (Extension ext in extensions)
                {
                    if (string.Compare(ext.Name, extensionFileName, true) == 0)
                    {
                        extensions.Remove(ext);
                        saveExtensions(extensions);
                        break;
                    }
                }
            }
        }

        public static void SaveExtensionLibraryToDisk(string extensionFileName, byte[] fileBytes)
        {
            if (fileBytes.Length > 0)
            {
                if (!Directory.Exists(ExtensionsFolderPath))
                    Directory.CreateDirectory(ExtensionsFolderPath);
                if (!extensionFileName.EndsWith(".xap", StringComparison.OrdinalIgnoreCase))
                    extensionFileName += ".xap";
                string filePath = string.Format("{0}\\{1}", ExtensionsFolderPath.TrimEnd('\\'), extensionFileName);
                if (File.Exists(filePath))
                    File.Delete(filePath);
                using (System.IO.FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(fileBytes, 0, fileBytes.Length);
                    fs.Close();
                }
            }
        }

        public static void DeleteExtensionLibraryFromDisk(string extensionFileName)
        {
            if (!extensionFileName.EndsWith(".xap", StringComparison.OrdinalIgnoreCase))
                extensionFileName += ".xap";
            string filePath = string.Format("{0}\\{1}", ExtensionsFolderPath.TrimEnd('\\'), extensionFileName);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        private static void saveExtensions(Extensions extensions)
        {
            try
            {                 
                string configPath = ExtensionsConfigFilePath;
                XmlSerializer writer = new XmlSerializer(typeof(Extensions));
                using (StreamWriter file = new StreamWriter(configPath))
                {
                    writer.Serialize(file, extensions);
                }
            }
            catch { }
        }
    }
}
