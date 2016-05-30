/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using System.Windows.Input;
using System.Windows.Markup;

namespace ESRI.ArcGIS.Mapping.Core
{
    public static class AssemblyManager
    {
        static AggregateCatalog partCatalogs;
        static List<Assembly> assemblies;

        // Contains all the public assemblies that are statically referenced and packaged OOTB in the Silverlight Viewer"
        static List<string> builtInAssemblyNames;

        static AssemblyManager() {
            assemblies = new List<Assembly>();
            partCatalogs = new AggregateCatalog();

            builtInAssemblyNames = new List<string>();
            builtInAssemblyNames.Add("ESRI.ArcGIS.Client");
            builtInAssemblyNames.Add("ESRI.ArcGIS.Client.Bing");
            builtInAssemblyNames.Add("ESRI.ArcGIS.Client.Toolkit");
            builtInAssemblyNames.Add("ESRI.ArcGIS.Client.Toolkit.DataSources");
            builtInAssemblyNames.Add("ESRI.ArcGIS.Client.Portal");
            builtInAssemblyNames.Add("ESRI.ArcGIS.Client.Application.Layout");
            builtInAssemblyNames.Add("ESRI.ArcGIS.Client.Application.Controls");
            builtInAssemblyNames.Add("ESRI.ArcGIS.Client.Extensibility");
            builtInAssemblyNames.Add("System.ComponentModel.Composition");
            builtInAssemblyNames.Add("System.ComponentModel.Composition.Initialization");
            builtInAssemblyNames.Add("System.ComponentModel.DataAnnotations");
            builtInAssemblyNames.Add("System.ServiceModel.Syndication");
            builtInAssemblyNames.Add("System.Windows.Controls.Data");
            builtInAssemblyNames.Add("System.Windows.Controls.Data.Input");
            builtInAssemblyNames.Add("System.Windows.Controls");
            builtInAssemblyNames.Add("System.Windows.Controls.Input.Toolkit");
            builtInAssemblyNames.Add("System.Windows.Controls.Layout.Toolkit");
            builtInAssemblyNames.Add("System.Windows.Controls.Toolkit");
            builtInAssemblyNames.Add("System.Windows.Controls.Toolkit.Internals");
            builtInAssemblyNames.Add("System.Windows.Data");
            builtInAssemblyNames.Add("System.Windows.Interactivity");
            builtInAssemblyNames.Add("System.Xml.Linq");
            builtInAssemblyNames.Add("System.Xml.Serialization");
        }                
        
        public static bool IsBuiltInAssembly(string assemblyName)
        {
            return builtInAssemblyNames.FirstOrDefault<string>(a => string.Compare(assemblyName, a, System.StringComparison.InvariantCultureIgnoreCase) == 0) != null;
        }

        public static void AddAssembly(Assembly assembly)
        {
            if (assembly == null)
                return;
            Assembly assem = assemblies.FirstOrDefault<Assembly>(a => a.FullName == assembly.FullName);
            bool added = addCatalogForAssembly(assembly);
            if (assem == null && added)
                assemblies.Add(assembly);
        }

        public static void DeleteAssembly(Assembly assembly)
        {
            if (assembly == null)
                return;
            Assembly assem = assemblies.FirstOrDefault<Assembly>(a => a.FullName == assembly.FullName);
            if (assem != null)
                assemblies.Remove(assembly);
            deleteCatalogForAssembly(assembly);
        }

        public static Assembly GetAssemblyByName(string assemblyName)
        {
            return assemblies.FirstOrDefault<Assembly>(a => string.Compare(assemblyName, a.FullName.Split(',')[0], System.StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        private const string CLR_NAMESPACE = "clr-namespace:";
        public static string GetAssemblyNameForNamespaceDeclaration(string namespaceDeclaration)
        {
            if (string.IsNullOrWhiteSpace(namespaceDeclaration))
                return null;

            // Ordinary namespace declaration of the form
            // xmlns:controls="clr-namespace:System.Windows.Controls;assembly=System.Windows.Controls"
            if (namespaceDeclaration.StartsWith(CLR_NAMESPACE, StringComparison.Ordinal)) 
            {
                int pos = namespaceDeclaration.IndexOf("=", CLR_NAMESPACE.Length + 1, StringComparison.Ordinal);
                if (pos > -1)
                {
                    Assembly a=  GetAssemblyByName(namespaceDeclaration.Substring(pos + 1));
                    if (a != null)
                        return a.FullName.Split(',')[0];
                }
            }
            else if (namespaceDeclaration.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || namespaceDeclaration.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                // Xmlns declaration of the form
                // xmlns:esri="http://schemas.esri.com/arcgis/client/2009"        
                foreach(Assembly assembly in assemblies)
                {
                    XmlnsDefinitionAttribute defnAttribute = null;
                    object[] attribs = assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), false);
                    if (attribs != null && attribs.Length > 0)
                        defnAttribute = attribs[0] as XmlnsDefinitionAttribute;

                    if (defnAttribute != null)
                    {
                        string namespaceMapping = defnAttribute.XmlNamespace;
                        if(namespaceDeclaration == namespaceMapping)
                        {
                            return assembly.FullName.Split(',')[0];
                        }
                    }
                }
            }
            return null;
        }

        public static IEnumerable<Type> GetExportsForType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            List<Type> types = new List<Type>();
            foreach (Assembly extensionAssembly in assemblies)
            {
                if (extensionAssembly == null)
                    continue;
                Type[] publicTypes = extensionAssembly.GetExportedTypes();
                if (publicTypes == null)
                    continue;

                foreach (Type publicType in publicTypes)
                {
                    object[] exportAttrs = publicType.GetCustomAttributes(typeof(ExportAttribute), true);
                    if (exportAttrs == null || exportAttrs.Length < 1)
                        continue;

                    foreach (object attribObj in exportAttrs)
                    {
                        // Has an export attribute
                        ExportAttribute attrib = attribObj as ExportAttribute;
                        if (attrib == null)
                            continue;

                        if (type.Equals(attrib.ContractType))
                        {
                            types.Add(publicType);
                            break;
                        }
                    }
                }
            }
            return types;
        }

        private static bool addCatalogForAssembly(Assembly assembly)
        {
            try
            {
                initCatalogIfRequired();
                ComposablePartCatalog ctlg = partCatalogs.Catalogs.FirstOrDefault<ComposablePartCatalog>(c => ((AssemblyCatalog)c).Assembly.FullName == assembly.FullName);
                if (ctlg == null) // not added yet
                {
                    partCatalogs.Catalogs.Add(new AssemblyCatalog(assembly));
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void deleteCatalogForAssembly(Assembly assembly)
        {
            ComposablePartCatalog ctlg = partCatalogs.Catalogs.FirstOrDefault<ComposablePartCatalog>(c => ((AssemblyCatalog)c).Assembly.FullName == assembly.FullName);
            if (ctlg != null) // found
            {
                partCatalogs.Catalogs.Remove(ctlg);
            }
        }        

        //public static void SatisfyImports(object objectWithImports)
        //{
        //    initCatalogIfRequired();
        //    CompositionInitializer.SatisfyImports(objectWithImports);
        //}

        static bool initComplete;
        private static void initCatalogIfRequired()
        {
            if (initComplete)
                return;
            CompositionHost.Initialize(partCatalogs);
            initComplete = true;
        }
    }
}
