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
using System.Xml.Linq;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client;
using System.Runtime.Serialization;
using System.Windows.Markup;
using ESRI.ArcGIS.Client.Extensibility;
using System.Collections.ObjectModel;
using System.Windows.Interactivity;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class BehaviorsConfiguration : List<BehaviorConfiguration>
    {
        private const string BEHAVIOR = "Behavior";
        private const string BEHAVIORS = "Behaviors";

        private Dictionary<string, string> OriginalBehaviorsNamespaces { get; set; }

        public void PopulateBehaviorsFromXml(string behaviorsXml)
        {
            try
            {
                this.Clear();
                XDocument xDoc = XDocument.Parse(behaviorsXml);
                XElement rootElement = xDoc.FirstNode as XElement;

                this.OriginalBehaviorsNamespaces = new Dictionary<string, string>();
                foreach (XAttribute attrib in rootElement.Attributes())
                    this.OriginalBehaviorsNamespaces.Add(attrib.Name.LocalName, attrib.Value);

                if (rootElement.HasElements)
                {
                    IEnumerable<XElement> toolPanelsXml = rootElement.Elements(BEHAVIOR);
                    foreach (XElement childNode in toolPanelsXml)
                    {
                        BehaviorConfiguration behavior = new BehaviorConfiguration();
                        behavior.BehaviorClassLoadConfigurationException += behavior_BehaviorClassLoadConfigurationException;
                        behavior.BehaviorClassLoadException += behavior_BehaviorClassLoadException;
                        behavior.PopulateBehaviorFromXmlNode(childNode);
                        if (behavior != null && behavior.ClassImplementation != null)
                            this.Add(behavior);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError(ex);
            }
        }

        public void Dispose()
        {
            for (int i = this.Count - 1; i >= 0; i--)
            {
                BehaviorConfiguration behavior = this[i];
                behavior.BehaviorClassLoadConfigurationException -= behavior_BehaviorClassLoadConfigurationException;
                behavior.BehaviorClassLoadException -= behavior_BehaviorClassLoadException;
                this.RemoveAt(i);
            }
        }

        public XDocument GetBehaviorsXml()
        {
            XDocument xDoc = new XDocument();

            List<XNamespace> namespaces = new List<XNamespace>();

            XElement rootElement = new XElement(BEHAVIORS);
            xDoc.Add(rootElement);

            if (OriginalBehaviorsNamespaces != null)
            {
                foreach (KeyValuePair<string, string> pair in OriginalBehaviorsNamespaces)
                {
                    namespaces.Add(XNamespace.Get(pair.Value));
                    rootElement.Add(new XAttribute(XNamespace.Xmlns + pair.Key, pair.Value));
                }
            }

            foreach (BehaviorConfiguration behavior in this)
            {
                List<XAttribute> additionalAttributes;
                XElement behaviorXml = behavior.GetBehaviorXml(namespaces, out additionalAttributes);
                if (additionalAttributes != null)
                {
                    foreach(XAttribute attribute in additionalAttributes)
                    {
                        if (rootElement.Attribute(attribute.Name) == null)
                            rootElement.Add(attribute);
                    }
                }
                if (behaviorXml != null)
                    rootElement.Add(behaviorXml);
            }

            return xDoc;
        }

        public BehaviorsConfiguration GetFromExtensionBehaviors(IEnumerable<ExtensionBehavior> extensions, ExtensionsDataManager dataManager, BehaviorsConfiguration originalConfiguration)
        {
            BehaviorsConfiguration conf = new BehaviorsConfiguration();
            if (originalConfiguration != null)
                conf.OriginalBehaviorsNamespaces = originalConfiguration.OriginalBehaviorsNamespaces;
            try
            {
                foreach (ExtensionBehavior extensionBehavior in extensions)
                {
                    Behavior<Map> behavior = extensionBehavior.MapBehavior;
                    if (behavior != null)
                    {
                        BehaviorConfiguration behaviorConf = new BehaviorConfiguration();
                        behaviorConf.Title = extensionBehavior.Title;
                        behaviorConf.IsEnabled = extensionBehavior.IsEnabled;
                        behaviorConf.ClassImplementation = extensionBehavior.MapBehavior;
                        if (dataManager != null)
                            behaviorConf.ConfigData = dataManager.GetExtensionDataForExtension(extensionBehavior.BehaviorId);
                        conf.Add(behaviorConf);
                    }
                }
            }
            catch { }
            return conf;
        }

        public List<ExtensionBehavior> GetAsExtensionBehaviors(ExtensionsDataManager dataManager)
        {
            List<ExtensionBehavior> list = new List<ExtensionBehavior>();

            try
            {
                foreach (BehaviorConfiguration beh in this)
                {
                    Behavior<Map> behavior = beh.ClassImplementation as System.Windows.Interactivity.Behavior<Map>;
                    if (behavior != null)
                    {
                        ExtensionBehavior extensionBehavior = new ExtensionBehavior();
                        extensionBehavior.Title = ExtensionDisplayNameConverter.Convert(behavior);
                        extensionBehavior.MapBehavior = behavior;
                        extensionBehavior.IsEnabled = beh.IsEnabled;
                        extensionBehavior.BehaviorId = beh.Id;
                        extensionBehavior.Title = beh.Title;
                        if (dataManager != null)
                            dataManager.SetExtensionDataForExtension(beh.Id, beh.ConfigData);

                        list.Add(extensionBehavior);
                    }
                }
            }
            catch { }
            return list;
        }

        #region Events

        void behavior_BehaviorClassLoadException(object sender, ExceptionEventArgs e)
        {
            OnBehaviorClassLoadException(e);
        }

        void behavior_BehaviorClassLoadConfigurationException(object sender, ExceptionEventArgs e)
        {
            OnBehaviorClassLoadConfigurationException(e);
        }

        protected virtual void OnBehaviorClassLoadException(ExceptionEventArgs args)
        {
            EventHandler<ExceptionEventArgs> handler = BehaviorClassLoadException;
            if (handler != null)
                handler(this, args);
        }
        public event EventHandler<ExceptionEventArgs> BehaviorClassLoadException;

        protected virtual void OnBehaviorClassSaveConfigurationException(ExceptionEventArgs args)
        {
            EventHandler<ExceptionEventArgs> handler = BehaviorClassSaveConfigurationException;
            if (handler != null)
                handler(this, args);
        }
        public event EventHandler<ExceptionEventArgs> BehaviorClassSaveConfigurationException;

        protected virtual void OnBehaviorClassLoadConfigurationException(ExceptionEventArgs args)
        {
            EventHandler<ExceptionEventArgs> handler = BehaviorClassLoadConfigurationException;
            if (handler != null)
                handler(this, args);
        }
        public event EventHandler<ExceptionEventArgs> BehaviorClassLoadConfigurationException;

        #endregion
    }
}
