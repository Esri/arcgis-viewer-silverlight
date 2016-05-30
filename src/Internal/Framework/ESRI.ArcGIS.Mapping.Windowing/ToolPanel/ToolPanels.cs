/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using ESRI.ArcGIS.Client.Application.Controls.Toolbars;

namespace ESRI.ArcGIS.Client.Application.Controls
{
    public class ToolPanels : List<ToolPanel>
    {
        public ToolPanel this[string containerName]
        {
            get
            {
                foreach (ToolPanel bar in this)
                {
                    if (bar.ContainerName != null && bar.ContainerName.Equals(containerName))
                        return bar;
                }
                return null;
            }
        }

        private static ToolPanels _currentToolPanels;
        public static ToolPanels Current
        {
            get
            {
                if (_currentToolPanels == null)
                    _currentToolPanels = new ToolPanels();

                return _currentToolPanels;
            }
            set
            {
                _currentToolPanels = value;
            }
        }
        /// <summary>
        /// Returns references to all instantiated objects (Commands, DropDownFrameworkElements) on all ToolPanels
        /// This include internal and external Exports
        /// </summary>
        /// <returns></returns>
        public IEnumerable<object> GetToolItemObjects()
        {
            List<object> toolItems = new List<object>();
            foreach (ToolPanel bar in this)
            {
                if (bar == null)
                    continue;
                IEnumerable<object> toolPanelItems = bar.GetToolItemObjects();
                if (toolPanelItems == null)
                    continue;
                foreach (object obj in toolPanelItems)
                {
                    if(!toolItems.Contains(obj))
                        toolItems.Add(obj);
                }
            }
            return toolItems;
        }

        private Dictionary<string, string> OriginalToolNamespaces { get; set; }

        public void PopulateToolPanelsFromXml(XElement rootElement)
        {
            try
            {
                this.OriginalToolNamespaces = new Dictionary<string, string>();
                foreach (XAttribute attrib in rootElement.Attributes())
                    this.OriginalToolNamespaces.Add(attrib.Name.LocalName, attrib.Value);

                if (rootElement.HasElements)
                {
                    IEnumerable<XElement> toolPanelsXml = rootElement.Elements(Constants.TOOLPANEL);
                    foreach (XElement childNode in toolPanelsXml)
                    {
                        ToolPanel toolPanel = new ToolPanel();
                        toolPanel.ToolClassLoadConfigurationException += toolPanel_ToolClassLoadConfigurationException;
                        toolPanel.ToolClassLoadException += toolPanel_ToolClassLoadException;
                        toolPanel.ToolClassSaveConfigurationException += toolPanel_ToolClassSaveConfigurationException;
                        toolPanel.PopulateToolPanelFromXml(childNode);
                        this.Add(toolPanel);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError(ex);
            }
        }

        private void toolPanel_ToolClassSaveConfigurationException(object sender, CoreExceptionEventArgs e)
        {
            OnToolClassSaveConfigurationException(e);
        }

        private void toolPanel_ToolClassLoadException(object sender, CoreExceptionEventArgs e)
        {
            OnToolClassLoadException(e);
        }

        private void toolPanel_ToolClassLoadConfigurationException(object sender, CoreExceptionEventArgs e)
        {
            OnToolClassLoadConfigurationException(e);
        }

        public void Dispose()
        {
            for (int i = this.Count - 1; i >= 0; i--)
            {
                ToolPanel toolPanel = this[i];
                toolPanel.ToolClassLoadConfigurationException -= toolPanel_ToolClassLoadConfigurationException;
                toolPanel.ToolClassLoadException -= toolPanel_ToolClassLoadException;
                toolPanel.ToolClassSaveConfigurationException -= toolPanel_ToolClassSaveConfigurationException;
                this.RemoveAt(i);
            }
        }

        public string GetToolPanelsXml()
        {
            XDocument xDoc = new XDocument();

            List<XNamespace> namespaces = new List<XNamespace>();

            XElement rootElement = new XElement(Constants.TOOLPANELS);
            xDoc.Add(rootElement);

            if (OriginalToolNamespaces != null)
            {
                foreach (KeyValuePair<string, string> pair in OriginalToolNamespaces)
                {
                    namespaces.Add(XNamespace.Get(pair.Value));
                    rootElement.Add(new XAttribute(XNamespace.Xmlns + pair.Key, pair.Value));
                }
            }

            foreach (ToolPanel toolPanel in this)
            {
                if (!toolPanel.CanSerialize)
                    continue;

                List<XAttribute> additionalAttributes;
                XElement toolPanelXml = toolPanel.GetToolPanelXml(namespaces, out additionalAttributes);
                if (toolPanelXml != null)
                {
                    if (additionalAttributes != null)
                    {
                        foreach (XAttribute attribute in additionalAttributes)
                        {
                            if (rootElement.Attribute(attribute.Name) == null)
                                rootElement.Add(attribute);
                        }
                    }
                    rootElement.Add(toolPanelXml);
                }
            }

            return xDoc.ToString(SaveOptions.OmitDuplicateNamespaces);
        }

        protected virtual void OnToolClassLoadException(CoreExceptionEventArgs args)
        {
            EventHandler<CoreExceptionEventArgs> handler = ToolClassLoadException;
            if (handler != null)
                handler(this, args);
        }
        public event EventHandler<CoreExceptionEventArgs> ToolClassLoadException;

        protected virtual void OnToolClassSaveConfigurationException(CoreExceptionEventArgs args)
        {
            EventHandler<CoreExceptionEventArgs> handler = ToolClassSaveConfigurationException;
            if (handler != null)
                handler(this, args);
        }
        public event EventHandler<CoreExceptionEventArgs> ToolClassSaveConfigurationException;

        protected virtual void OnToolClassLoadConfigurationException(CoreExceptionEventArgs args)
        {
            EventHandler<CoreExceptionEventArgs> handler = ToolClassLoadConfigurationException;
            if (handler != null)
                handler(this, args);
        }
        public event EventHandler<CoreExceptionEventArgs> ToolClassLoadConfigurationException;
    }


}
