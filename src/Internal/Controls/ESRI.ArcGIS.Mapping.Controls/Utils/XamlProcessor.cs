/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class XamlProcessor
    {
        public class ResourceDictionaryExtractor
        {
            public List<string> ResourceDictionaryNames = new List<string>();
            public string ExtractResourceDictionary(Match m)
            {
                string matchedString = m.ToString();
                //<ResourceDictionary Source="Basic_Resources.xaml" />
                int start = matchedString.IndexOf("\"", StringComparison.Ordinal);
                    start = start + 1;
                if (start > 0)
                {
                    int end = matchedString.IndexOf("\"", start, StringComparison.Ordinal);
                    if (end > 0)
                    {
                        string file = matchedString.Substring(start, end - start);
                        if (!string.IsNullOrWhiteSpace(file))
                            ResourceDictionaryNames.Add(file);
                    }

                }
                return string.Empty;
            }
 
        }

        public static string ExtractResourceDictionaries(string fileContents, out List<string> resourceDictionaryNames)
        {
            #region Extract resource dictionary list, remove resource dictionary from XAML file
            //Test string: <ResourceDictionary Source="Basic_Resources.xaml" />
            //Pattern without escapes:  \<ResourceDictionary(\s*?)Source(\s*?)=(\s*?)"(.*?)"(\s*?)/\>
            string pattern = "\\<ResourceDictionary(\\s*?)Source(\\s*?)=(\\s*?)\"(.*?)\"(\\s*?)/\\>";
            ResourceDictionaryExtractor extractor = new ResourceDictionaryExtractor();
            fileContents = Regex.Replace(fileContents, pattern, new MatchEvaluator(extractor.ExtractResourceDictionary));
            resourceDictionaryNames = extractor.ResourceDictionaryNames;
            #endregion

            #region Remove empty resource dictionaries
            //Pattern without escapes:  \<ResourceDictionary\>(\s*?)\<ResourceDictionary.MergedDictionaries\>(\s*?)\</ResourceDictionary.MergedDictionaries\>(\s*?)\</ResourceDictionary\>
            pattern = "\\<ResourceDictionary\\>(\\s*?)\\<ResourceDictionary.MergedDictionaries\\>(\\s*?)\\</ResourceDictionary.MergedDictionaries\\>(\\s*?)\\</ResourceDictionary\\>";
            fileContents = Regex.Replace(fileContents, pattern, string.Empty);
            #endregion

            return fileContents;
        }
    }
}
