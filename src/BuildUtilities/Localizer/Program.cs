/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Resources;
using System.Collections;
using System.IO;
using System.Xml.Linq;
using System.Security;

namespace Localizer
{
    class Program
    {
        private static List<KeyValuePair<string, string>> _textWrappingCharacters;
        private static List<KeyValuePair<string, string>> TextWrappingCharacters
        {
            get
            {
                if(_textWrappingCharacters == null)
                {
                    _textWrappingCharacters = new List<KeyValuePair<string, string>>();
                    _textWrappingCharacters.Add(new KeyValuePair<string, string>(">", " <"));
                    _textWrappingCharacters.Add(new KeyValuePair<string, string>(">", "<"));
                    _textWrappingCharacters.Add(new KeyValuePair<string, string>("\"", " \""));
                    _textWrappingCharacters.Add(new KeyValuePair<string, string>("\"", "\""));
                    _textWrappingCharacters.Add(new KeyValuePair<string, string>("$", "$"));
                }
                return _textWrappingCharacters;
            }
        }

        static void Main(string[] args)
        {
            if (args != null && args.Length > 1 && !string.IsNullOrEmpty(args[0]) && !string.IsNullOrEmpty(args[1]))
            {
                string toLocalize = args[0];
                string directory = args[1];

                if (toLocalize.ToLower() == "builder")
                {
                    LocalizeBuilder(directory);
                }
                else if (toLocalize.ToLower() == "viewer")
                {
                    LocalizeViewer(directory);
                }

                foreach (ResourceSet item in ResourceSets)
                {
                    item.Close();
                }
            }
        }

        static List<ResourceSet> ResourceSets = new List<ResourceSet>();

        static void LocalizeBuilder(string builderDirectory)
        {
            LocalizedFiles localizedFiles = new LocalizedFiles();

            foreach (string file in localizedFiles.BuilderInfo.BuilderFiles)
            {
                foreach (string culture in Cultures.Supported)
                {
                    bool isXmlFile = file.ToLower().EndsWith(".xml", StringComparison.OrdinalIgnoreCase) ||
                        file.ToLower().EndsWith(".xaml", StringComparison.OrdinalIgnoreCase);
                    ReplaceStrings(culture, file, builderDirectory, isXmlFile);
                }
                string englishFile = Path.Combine(builderDirectory, "Culture", "en-US", file);
                if (File.Exists(englishFile))
                {
                    string originalFile = Path.Combine(builderDirectory, file);
                    RemoveReadOnly(originalFile);
                    File.Copy(englishFile, originalFile, true);
                }
            }
        }

        static void LocalizeViewer(string viewerDirectory)
        {
            LocalizedFiles localizedFiles = new LocalizedFiles();

            foreach (Template template in localizedFiles.BuilderInfo.Templates)
            {
                LocalizeFiles(template.XmlTemplateFiles, viewerDirectory, true);
                LocalizeFiles(template.TextTemplateFiles, viewerDirectory);
            }
        }

        public static void LocalizeFiles(LocalizedFileList files, string templateFolder, bool areXmlFiles = false)
        {
            if (files == null)
                return;

            foreach (string file in files)
            {
                foreach (string culture in Cultures.Supported)
                    ReplaceStrings(culture, file, templateFolder, areXmlFiles);
                string englishFile = Path.Combine(templateFolder, "Culture", "en-US", file);
                if (File.Exists(englishFile))
                {
                    string originalFile = Path.Combine(templateFolder, file);
                    RemoveReadOnly(originalFile);
                    File.Copy(englishFile, originalFile, true);
                }
            }
        }

        private static void RemoveReadOnly(string originalFile)
        {
            FileAttributes attributes = File.GetAttributes(originalFile);
            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                File.SetAttributes(originalFile, attributes ^ FileAttributes.ReadOnly);
        }

        private static void ReplaceStrings(string culture, string file, string parentFolder, bool areXmlFiles = false)
        {
            string sourceFile = Path.Combine(parentFolder, file);
            if (!File.Exists(sourceFile))
                return;
            string fileContents = File.ReadAllText(sourceFile);
            
            fileContents = Regex.Replace(fileContents, "en-US", culture);

            ResourceSet rs = StringResourcesManager.Instance.ResourceManager.GetResourceSet(
                new System.Globalization.CultureInfo(culture), true, true);
            if (rs == null)
                rs = StringResourcesManager.Instance.ResourceManager.GetResourceSet(
               System.Globalization.CultureInfo.CurrentCulture, true, true);
            // Create an IDictionaryEnumerator to read the data in the ResourceSet.
            IDictionaryEnumerator id = rs.GetEnumerator();
            // Iterate through the ResourceSet and display the contents to the console. 
            string toSearchFor;
            string wrappedSearch;
            string toReplace;
            string wrappedReplace;
            while (id.MoveNext())
            {
                // Get the strings to search for and replace from the resource file
                toSearchFor = id.Key.ToString();
                toReplace = id.Value.ToString();

                // Encode the replacement string for XML
                toReplace = SecurityElement.Escape(toReplace);

                if (areXmlFiles)
                {
                    foreach (KeyValuePair<string, string> wrappers in TextWrappingCharacters)
                    {
                        // Surround the strings to search for and replace with the expected enclosing characters
                        wrappedSearch = wrappers.Key + id.Key.ToString() + wrappers.Value;
                        wrappedReplace = wrappers.Key + toReplace + wrappers.Value;

                        // Do the replacement
                        fileContents = Regex.Replace(fileContents, wrappedSearch, wrappedReplace);
                    }
                }
                else
                    fileContents = Regex.Replace(fileContents, id.Key.ToString(), id.Value.ToString());
            }
            if (!ResourceSets.Contains(rs))
                ResourceSets.Add(rs);

            try
            {
                string cultureDirectory = Path.Combine(parentFolder, "Culture", culture);
                string filePath = Path.Combine(cultureDirectory, file);
                string subDirectory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(subDirectory))
                    Directory.CreateDirectory(subDirectory);
                File.WriteAllText(filePath, fileContents, System.Text.Encoding.UTF8);
            }
            catch (Exception) { }
        }
    }
}
