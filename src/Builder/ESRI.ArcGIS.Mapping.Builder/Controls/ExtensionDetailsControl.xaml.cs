/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Mapping.Builder.ApplicationBuilder;
using ESRI.ArcGIS.Mapping.Core;
using System.ComponentModel.Composition;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Builder.Common;
using ESRI.ArcGIS.Mapping.Builder.Resources;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class ExtensionDetailsControl : UserControl
    {
        private Extension extension;
        public ExtensionDetailsControl(Extension extensionName)
        {
            extension = extensionName;
            InitializeComponent();
        }

        private void ExtensionsDetailsControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (extension == null || string.IsNullOrEmpty(extension.Name))
                return;

            List<ExtensionDisplayInfo> displayInfos = new List<ExtensionDisplayInfo>();
            if (extension.Assemblies != null)
            {
                foreach (Assembly extensionAssembly in extension.Assemblies)
                {
                    System.Reflection.Assembly assem = AssemblyManager.GetAssemblyByName(extensionAssembly.Name);
                    if (assem == null)
                        continue;

                    Type[] publicTypes = assem.GetExportedTypes();
                    if (publicTypes == null)
                        continue;

                    List<ExtensionDisplayInfo> exportedCommands = new List<ExtensionDisplayInfo>();
                    List<ExtensionDisplayInfo> exportedMapBehaviors = new List<ExtensionDisplayInfo>();
                    List<ExtensionDisplayInfo> exportedControls = new List<ExtensionDisplayInfo>();
                    List<ExtensionDisplayInfo> exportedLayers = new List<ExtensionDisplayInfo>();
                    foreach (Type publicType in publicTypes)
                    {
                        object[] exportAttrs = publicType.GetCustomAttributes(typeof(ExportAttribute), true);
                        if (exportAttrs == null || exportAttrs.Length < 1)
                            continue;

                        // Has an export attribute
                        ExportAttribute attrib = exportAttrs[0] as ExportAttribute;
                        if (attrib == null)
                            continue;

                        string type = null;
                        if (typeof(ICommand).Equals(attrib.ContractType))
                            type = Strings.Tool;
                        else if (typeof(Behavior<Map>).Equals(attrib.ContractType))
                            type = Strings.Behavior;
                        else if (typeof(Layer).Equals(attrib.ContractType))
                            type = Strings.Layer;
                        else if (typeof(FrameworkElement).Equals(attrib.ContractType))
                            type = Strings.Control;

                        if (type == null)
                            continue;

                        displayInfos.Add(new ExtensionDisplayInfo()
                                {
                                    Type = type,
                                    DisplayName = GetDisplayNameForType(publicType),
                                    Description = GetDescriptionForType(publicType),
                                });
                    }
                }
                DataGrid.ItemsSource = displayInfos;
				Summary.Text = string.Format(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ExtensionContainsTheFollowingAddOns, extension.Name);
            }
            else
            {
				Summary.Text = ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ErrorUnableToRetrieveDetailsForExtension;
            }
        }
        
        private static string GetDescriptionForType(Type type)
        {
            object[] customattr = type.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (customattr != null && customattr.Length > 0)
            {
                DescriptionAttribute displayNameAttr = customattr[0] as DescriptionAttribute;
                if (displayNameAttr != null && !string.IsNullOrEmpty(displayNameAttr.Description))
                    return displayNameAttr.Description;
            }
            return type.Name;
        }

        private static string GetDisplayNameForType(Type type)
        {
            object[] customattr = type.GetCustomAttributes(typeof(DisplayNameAttribute), true);
            if (customattr != null && customattr.Length > 0)
            {
                DisplayNameAttribute displayNameAttr = customattr[0] as DisplayNameAttribute;
                if (displayNameAttr != null && !string.IsNullOrEmpty(displayNameAttr.Name))
                    return displayNameAttr.Name;
            }
            return type.Name;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (CloseClicked != null)
                CloseClicked(this, EventArgs.Empty);
        }

        public event EventHandler CloseClicked;        
    }

    public class ExtensionDisplayInfo
    {
        public string Type { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }
}
