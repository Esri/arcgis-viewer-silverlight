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
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Xml.Linq;
using System.Linq;
using System.Windows.Markup;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class BehaviorConfiguration : INotifyPropertyChanged
    {
        private const string CONFIG_DATA = "Behavior.ConfigData";
        private const string CLASS = "Behavior.Class";
        private const string BEHAVIOR = "Behavior";
        private const string IS_ENABLED = "IsEnabled";
        private const string TITLE = "Title";

        public string _id;
        public string Id
        {
            get 
            {
                if (_id == null)
                    _id = Guid.NewGuid().ToString();
                return _id; 
            }
        }

        public bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; OnPropertyChanged(IS_ENABLED); }
        }

        public string _configData;
        public string ConfigData
        {
            get { return _configData; }
            set { _configData = value; OnPropertyChanged(CONFIG_DATA); }
        }

        public string _class;
        public string Class
        {
            get { return _class; }
            set { _class = value; OnPropertyChanged(CLASS); }
        }

        private object _classImplementation;
        public object ClassImplementation
        {
            get
            {
                if (_classImplementation == null)
                {
                    if (!string.IsNullOrWhiteSpace(Class))
                    {
                        _classImplementation = getBehaviorImplementation(Class);
                    }
                }
                return _classImplementation;
            }
            internal set
            {
                _classImplementation = value;
            }
        }

        public string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged(TITLE); }
        }

        private static string getAttributeValue(XElement elem, string attributeName)
        {
            if(elem != null)
                return elem.Attribute(attributeName) != null ? elem.Attribute(attributeName).Value : null;

            return null;
        }
        private static string getNodeValueFromXmlNode(XElement childNode, string elementName)
        {
            if (childNode.HasElements)
            {
                XElement element = childNode.Element(elementName);
                if (element != null )
                {
                    XNode node = element.FirstNode;
                    if (node != null)
                    {
                        XText nodeAsText = node as XText;
                        if (nodeAsText != null)
                            return nodeAsText.Value;
                        else
                            return node.ToString();
                    }
                    else
                        return element.Value;
                }
            }
            return null;
        }

        public void PopulateBehaviorFromXmlNode(XElement childNode)
        {
            ConfigData = getNodeValueFromXmlNode(childNode, CONFIG_DATA) ?? "";
            Class = getNodeValueFromXmlNode(childNode, CLASS) ?? "";
            IsEnabled = Boolean.Parse(getAttributeValue(childNode, IS_ENABLED) ?? "false");
            Title = getAttributeValue(childNode, TITLE);
        }

        private object getBehaviorImplementation(string implementationXml)
        {
            object classImplementation = null;
            try
            {
                classImplementation = XamlReader.Load(implementationXml);
                ISupportsConfiguration supportsConfiguration = classImplementation as ISupportsConfiguration;
                if (supportsConfiguration != null)
                {
                    try
                    {
                        supportsConfiguration.LoadConfiguration(ConfigData);
                    }
                    catch (Exception innerEx)
                    {
                        // Error in Load Configuration of command
                        Logger.Instance.LogError(innerEx);
                        OnBehaviorClassLoadConfigurationException(new ExceptionEventArgs(new Exception(string.Format("Failed to load configuration of behavior (Class: {0}).", Class), 
                            string.IsNullOrWhiteSpace(implementationXml) ? innerEx : new Exception(string.Format("Behavior Implementation XAML: {0}", implementationXml), innerEx)), null));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError(ex);
                OnBehaviorClassLoadException(new ExceptionEventArgs(new Exception(string.Format("Failed to instantiate behavior (Class: {0}).", Class), 
                            string.IsNullOrWhiteSpace(implementationXml) ? ex : new Exception(string.Format("Behavior Implementation XAML: {0}", implementationXml), ex)), null));
            }
            return classImplementation;
        }

        public XElement GetBehaviorXml(List<XNamespace> namespaces, out List<XAttribute> additionalAttributes)
        {
            additionalAttributes = null;
            try
            {
                XElement behaviorElement = new XElement(BEHAVIOR);

                addAttributesForBehavior(behaviorElement);

                addXElementsForBehavior(behaviorElement, namespaces, out additionalAttributes);
                
                return behaviorElement;
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError(ex);
            }

            return null;
        }

        private void addXElementsForBehavior(XElement rootElement, List<XNamespace> namespaces, out List<XAttribute> additionalAttributes)
        {
            additionalAttributes = null;
            if (rootElement != null)
            {
                addBehaviorClassToXmlElement(rootElement, namespaces, out additionalAttributes);
            }
        }

        private void addBehaviorClassToXmlElement(XElement behavior, List<XNamespace> namespaces, out List<XAttribute> additionalTagPrefixes)
        {
            additionalTagPrefixes = null;
            if (behavior == null)
                return;

            if (ClassImplementation != null)
            {
                XElement behaviorElement = new XElement(CLASS);
                behavior.Add(behaviorElement);

                Type behaviorType = ClassImplementation.GetType();
                XmlnsDefinitionAttribute defnAttribute = null;
                object[] attribs = behaviorType.Assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), false);
                if (attribs != null && attribs.Length > 0)
                    defnAttribute = attribs[0] as XmlnsDefinitionAttribute;

                string behaviorNamespace = behaviorType.Namespace;
                string namespaceMapping = null;
                if (defnAttribute != null)
                    namespaceMapping = defnAttribute.XmlNamespace;
                else
                    namespaceMapping = string.Format("clr-namespace:{0};assembly={1}", behaviorNamespace, behaviorType.Assembly.FullName.Split(',')[0]);

                XNamespace namespaceForCommand = namespaces.FirstOrDefault<XNamespace>(x => x.NamespaceName == namespaceMapping);
                if (namespaceForCommand == null)
                {
                    namespaceForCommand = namespaceMapping;
                    string tagPrefix = behaviorNamespace.Replace('.', '_');
                    
                    if (additionalTagPrefixes == null)
                        additionalTagPrefixes = new List<XAttribute>();

                    additionalTagPrefixes.Add(new XAttribute(XNamespace.Xmlns + tagPrefix, namespaceMapping));
                }

                XElement behaviorCommandImplElement = new XElement(namespaceForCommand + behaviorType.Name);
                behaviorElement.Add(behaviorCommandImplElement);

                ISupportsConfiguration supportsConfiguration = ClassImplementation as ISupportsConfiguration;
                if (supportsConfiguration != null)
                {
                    //string configData = null;
                    //try
                    //{
                    //    configData = supportsConfiguration.SaveConfiguration();
                    //}
                    //catch (Exception ex)
                    //{
                    //    Logger.Instance.LogError(ex);
                    //    OnBehaviorClassSaveConfigurationException(new ExceptionEventArgs(ex, null));
                    //}
                    if (!string.IsNullOrWhiteSpace(ConfigData))
                    {
                        ConfigData = ConfigData.Trim();
                        XElement configDataElement = new XElement(CONFIG_DATA);
                        if (ConfigData.StartsWith("<", StringComparison.Ordinal) && ConfigData.EndsWith(">", StringComparison.Ordinal))
                        {
                            try
                            {
                                XDocument xDoc = XDocument.Parse(ConfigData);
                                configDataElement.Add(xDoc.Root);
                            }
                            catch
                            {
                                configDataElement.Value = ConfigData;
                            }
                        }
                        else
                        {
                            configDataElement.Value = ConfigData;
                        }
                        behavior.Add(configDataElement);
                    }
                }
            }
        }

        private void addAttributesForBehavior(XElement behaviorElement)
        {
            if (behaviorElement != null)
            {
                if (IsEnabled)
                    behaviorElement.Add(new XAttribute(IS_ENABLED, IsEnabled.ToString()));
                if (!string.IsNullOrWhiteSpace(Title))
                    behaviorElement.Add(new XAttribute(TITLE, Title.ToString()));
            }
        }

        protected virtual void OnBehaviorClassLoadConfigurationException(ExceptionEventArgs args)
        {
            EventHandler<ExceptionEventArgs> handler = BehaviorClassLoadConfigurationException;
            if (handler != null)
                handler(this, args);
        }
        public event EventHandler<ExceptionEventArgs> BehaviorClassLoadConfigurationException;

        protected virtual void OnBehaviorClassSaveConfigurationException(ExceptionEventArgs args)
        {
            EventHandler<ExceptionEventArgs> handler = BehaviorClassSaveConfigurationException;
            if (handler != null)
                handler(this, args);
        }
        public event EventHandler<ExceptionEventArgs> BehaviorClassSaveConfigurationException;
        
        protected virtual void OnBehaviorClassLoadException(ExceptionEventArgs args)
        {
            EventHandler<ExceptionEventArgs> handler = BehaviorClassLoadException;
            if (handler != null)
                handler(this, args);
        }
        public event EventHandler<ExceptionEventArgs> BehaviorClassLoadException;

        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
