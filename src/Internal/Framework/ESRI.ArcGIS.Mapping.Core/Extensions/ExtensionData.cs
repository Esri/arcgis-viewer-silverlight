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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class ExtensionData
    {
        public string ConfigData { get; set; }
    }

    [DataContract]
    public class ExtensionsConfigData
    {
        [DataMember]
        public Dictionary<string, ExtensionData> ExtensionsData { get; set; }

        public ExtensionsConfigData() { }

        public ExtensionsConfigData(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return;

            XDocument xDoc = XDocument.Parse(xml, LoadOptions.None);
            XElement rootElement = xDoc.FirstNode as XElement;
            ExtensionsData = new Dictionary<string, ExtensionData>();
            if (rootElement.HasElements)
            {                
                IEnumerable<XElement> toolPanelsXml = rootElement.Elements("Control");
                foreach (XElement childNode in toolPanelsXml)
                {
                    string name = childNode.Attribute("Name") != null ? childNode.Attribute("Name").Value : null;
                    if (string.IsNullOrEmpty(name))
                        continue;
                    string configData = string.Empty;
                    XElement configDataNode = childNode.Element("Control.ConfigData");
                    if (configDataNode != null)
                    {
                        XNode configNode = configDataNode.FirstNode;
                        if (configNode != null)
                        {
                            XText configNodeAsText = configNode as XText;
                            if (configNodeAsText != null)
                            {
                                configData = configNodeAsText.Value;
                            }
                            else
                                configData = configNode.ToString();
                        }
                    }    
                    ExtensionsData.Add(name, new ExtensionData(){ ConfigData = configData.Trim()});
                }
            }
        }

        public string ToXml()
        {
            XDocument xDoc = new XDocument();
            XElement rootElem = new XElement("Controls");
            xDoc.Add(rootElem);
            if (ExtensionsData != null)
            {
                foreach (KeyValuePair<string,ExtensionData> data in ExtensionsData)
                {
                    if (string.IsNullOrEmpty(data.Key))
                        continue;

                    XElement controlElement = new XElement("Control");                    
                    controlElement.SetAttributeValue("Name", data.Key);
                    rootElem.Add(controlElement);

                    ExtensionData dataValue = data.Value;
                    if (dataValue == null)
                        continue;

                    string configData = dataValue.ConfigData;
                    if (configData == null)
                        continue;
                    configData = configData.Trim();
                    XElement configDataElement = new XElement("Control.ConfigData");
                    if (configData.StartsWith("<", StringComparison.Ordinal) && configData.EndsWith(">", StringComparison.Ordinal))
                    {
                        try
                        {
                            XDocument xDoc2 = XDocument.Parse(configData);
                            configDataElement.Add(xDoc2.Root);
                        }
                        catch
                        {
                            configDataElement.Value = configData;
                        }
                    }
                    else
                    {
                        configDataElement.Value = configData;
                    }
                    controlElement.Add(configDataElement);
                }
            }
            return xDoc.ToString(
                SaveOptions.OmitDuplicateNamespaces
);
        }
    }
}
