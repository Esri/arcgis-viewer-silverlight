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
using System.Reflection;
using System.Windows.Resources;
using System.IO;
using System.Windows.Markup;

namespace ESRI.ArcGIS.Mapping.GP
{
    public class ResourceUtility
    {
        private static string assemblyName = "ESRI.ArcGIS.Mapping.GP";
        /// <summary>
        /// Gets or sets the name of the assembly to load resources from
        /// </summary>
        public static string AssemblyName
        {
            get { return assemblyName; }
            set { assemblyName = value; }
        }

        /// <summary>
        /// Load a style from a resource dictionary embedded in the assembly
        /// </summary>
        /// <param name="path">The relative path to the embedded resource dictionary from the root of the assembly</param>
        /// <param name="key">The key of the style to retrieve</param>
        /// <returns>The style specified by the resource dictionary path and style key</returns>
        public static Style LoadEmbeddedStyle(string path, string key)
        {
            ResourceDictionary dict = ResourceUtility.LoadEmbeddedDictionary(path);
            if (dict == null)
                return null;
            else
                return dict[key] as Style;
        }

        /// <summary>
        /// Load a resource dictionary embedded in the assembly
        /// </summary>
        /// <param name="path">The path to the resource dictionary.  Can be specified as a relative path from
        /// the root of the assembly or using component path syntax.</param>
        /// <returns>The resource dictionary specified by the path</returns>
        public static ResourceDictionary LoadEmbeddedDictionary(string path)
        {
            string xaml;
            if (!path.Contains(";component/"))
                path = string.Format("{0};component/{1}", ResourceUtility.AssemblyName, path);
            StreamResourceInfo sri =
                Application.GetResourceStream(new Uri(path, UriKind.Relative));
            using (Stream stream = sri.Stream)
            {
                using (StreamReader sr = new StreamReader(stream))
                    xaml = sr.ReadToEnd();
            }

            return XamlReader.Load(xaml) as ResourceDictionary;
        }
    }
}
