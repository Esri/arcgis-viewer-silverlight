/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;
using System.Xml;
using System.Windows.Media.Imaging;
using System;

namespace ESRI.ArcGIS.Mapping.Core
{
    public abstract class XamlWriterBase 
    {
        protected XmlWriter writer;
        protected XmlWriterSettings writerSettings;
        public Dictionary<string, string> Namespaces { get; protected set; }

        public XamlWriterBase(XmlWriter writer, Dictionary<string, string> namespaces) 
        {
            if (namespaces == null)
                throw new ArgumentNullException("namespaces");
            this.writer = writer;
            Namespaces = namespaces;
        }
        
        protected void WriteNamespaces()
        {
            foreach (string key in Namespaces.Keys)
            {
                string _namespace = "http://schemas.microsoft.com/winfx/2006/xaml"; // default
                if (Namespaces.ContainsKey(key))
                    _namespace = Namespaces[key];
                writer.WriteAttributeString("xmlns", key, null, _namespace);
            }
        }

        protected void WriteXName(string name)
        {
            WriteAttribute("x", "Name", name);
        }

        public void WriteAttribute(string name, Color color)
        {
            WriteAttribute(name, string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}",
                    color.A, color.R, color.G, color.B));
        }

        public void WriteAttribute(string name, double value)
        {
            WriteAttribute(name, value.ToString(CultureInfo.InvariantCulture));
        }
        public void WriteAttribute(string name, int value)
        {
            WriteAttribute(name, value.ToString(CultureInfo.InvariantCulture));
        }
        public void WriteAttribute(string name, bool value)
        {
            if (value) 
                WriteAttribute(name, "True");
            else
                WriteAttribute(name, "False");
        }
        public void WriteAttribute(string name, string value)
        {
            if (value == null) return;
            writer.WriteAttributeString(name, value);
        }

        public void WriteAttribute(string prefix, string localname, string value)
        {
            if (value == null) return;
            string _namespace = "http://schemas.microsoft.com/winfx/2006/xaml"; // default
            if (Namespaces.ContainsKey(prefix))
                _namespace = Namespaces[prefix];
            writer.WriteAttributeString(localname, _namespace, value);
        }
        public void StartType(object type, string prefix)
        {
            if (prefix != null)
            {
                string _namespace = "http://schemas.microsoft.com/winfx/2006/xaml"; // default
                if (Namespaces.ContainsKey(prefix))
                    _namespace = Namespaces[prefix];
                writer.WriteStartElement(type.GetType().Name, _namespace);
            }
            else
                writer.WriteStartElement(type.GetType().Name);
        }        
    }
}
