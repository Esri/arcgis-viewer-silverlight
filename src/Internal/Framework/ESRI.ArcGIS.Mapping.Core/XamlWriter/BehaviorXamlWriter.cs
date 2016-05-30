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
using System.Xml;
using System.Collections.Generic;
using System.Windows.Interactivity;
using System.Reflection;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class BehaviorXamlWriter : XamlWriterBase
    {
        public BehaviorXamlWriter(XmlWriter writer, Dictionary<string, string> namespaces)
            : base(writer, namespaces)
        {
        }

        public void WriteBehavior(Behavior behavior)
        {
            if (behavior == null)
                return;

            writer.WriteStartElement(behavior.GetType().Name, Constants.esriBehaviorsNamespace);
            WriteObjectPropertiesAsAttributes(writer, behavior);
            writer.WriteEndElement();
        }

        private void WriteObjectPropertiesAsAttributes(XmlWriter writer, object obj)
        {
            if (obj == null)
                return;

            Type type = obj.GetType();

            PropertyInfo[] propertyInfo = type.GetProperties();
            for (int i = 0; i < propertyInfo.Length; i++)
            {
                PropertyInfo pi = propertyInfo[i];
                Type propertyType = pi.PropertyType;

                string nameOfProperty = pi.Name;
                string valueOfProperty = null;

                object value = null;
                try
                {
                    // Get the value of the property for this object                
                    if (pi.CanRead)
                        value = pi.GetValue(obj, null);
                    // Check if the property is writable, public and serializable
                    if (value != null && CanPropertyBeSerialized(pi, propertyType, nameOfProperty))
                    {
                        if (pi.PropertyType.Equals(typeof(string)))
                        {
                            valueOfProperty = value as string;
                        }
                        else if (pi.PropertyType.IsEnum || pi.PropertyType.IsPrimitive)
                        {
                            valueOfProperty = Convert.ToString(value);
                        }
                        else if(pi.PropertyType.Equals(typeof(DateTime)) || pi.PropertyType.Equals(typeof(TimeSpan)))
                        {
                            valueOfProperty = Convert.ToString(value);
                        }
                        else
                        {
                            MethodInfo mi = pi.PropertyType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
                            if (mi != null)
                            {
                                object o = mi.Invoke(null, new object[] { value });
                                valueOfProperty = Convert.ToString(pi.GetValue(o, null));
                            }
                            else
                            {
                                //try to invoke converter
                                object[] attribs = pi.GetCustomAttributes(typeof(TypeConverterAttribute), true);

                                if (attribs == null || attribs.Length == 0)
                                    attribs = pi.PropertyType.GetCustomAttributes(typeof(TypeConverterAttribute), true);

                                if (attribs != null && attribs.Length > 0)
                                {
                                    string converterTypeName = ((TypeConverterAttribute)attribs[0]).ConverterTypeName;
                                    Type converterType = Type.GetType(converterTypeName);
                                    TypeConverter converter = (TypeConverter)converterType.Assembly.CreateInstance(converterType.FullName);
                                    valueOfProperty = converter.ConvertToString(value);
                                }
                            }
                        }
                    }
                }
                catch { }

                if (valueOfProperty != null)
                {
                    writer.WriteAttributeString(nameOfProperty, valueOfProperty);
                }
            }
        }

        private static bool CanPropertyBeSerialized(PropertyInfo pi, Type propertyType, string propertyName)
        {
            bool isWritable = pi.CanWrite;
            bool isPublicProperty = propertyType.IsPublic;
            return isWritable && isPublicProperty;
        }
    }
}
