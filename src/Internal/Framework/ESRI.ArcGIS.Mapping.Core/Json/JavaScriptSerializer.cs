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
using System.Collections.Generic;
using System.Collections;
using System.Runtime.CompilerServices;


namespace ESRI.ArcGIS.Client.Utils
{
    public sealed class JavaScriptSerializer
    {
        // Fields
        private Dictionary<Type, JavaScriptConverter> _converters;
        private int _recursionLimit;
        private JavaScriptTypeResolver _typeResolver;
        internal static readonly long DatetimeMinTimeTicks;
        internal const int DefaultRecursionLimit = 100;
        internal const string ServerTypeFieldName = "__type";

        // Methods
        static JavaScriptSerializer()
        {
            DateTime time = new DateTime(0x7b2, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DatetimeMinTimeTicks = time.Ticks;
        }

        public JavaScriptSerializer()
            : this(null)
        {
        }

        public JavaScriptSerializer(JavaScriptTypeResolver resolver)
        {
            this._typeResolver = resolver;
			this.RecursionLimit = DefaultRecursionLimit;
        }

        internal bool ConverterExistsForType(Type t, out JavaScriptConverter converter)
        {
            converter = this.GetConverter(t);
            return (converter != null);
        }

        public T ConvertToType<T>(object obj)
        {
            return (T)ObjectConverter.ConvertObjectToType(obj, typeof(T), this);
        }

        public T Deserialize<T>(string input)
        {
            return (T)Deserialize(this, input, typeof(T), this.RecursionLimit);
        }

        public static object Deserialize(JavaScriptSerializer serializer, string input, Type type, int depthLimit)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
           return ObjectConverter.ConvertObjectToType(JavaScriptObjectDeserializer.BasicDeserialize(input, depthLimit, serializer), type, serializer);
        }

        public object DeserializeObject(string input)
        {
            return Deserialize(this, input, null, this.RecursionLimit);
        }

        private JavaScriptConverter GetConverter(Type t)
        {
            if (this._converters != null)
            {
                while (t != null)
                {
                    if (this._converters.ContainsKey(t))
                    {
                        return this._converters[t];
                    }
                    t = t.BaseType;
                }
            }
            return null;
        }

        public void RegisterConverters(IEnumerable<JavaScriptConverter> converters)
        {
            if (converters == null)
            {
                throw new ArgumentNullException("converters");
            }
            foreach (JavaScriptConverter converter in converters)
            {
                IEnumerable<Type> supportedTypes = converter.SupportedTypes;
                if (supportedTypes != null)
                {
                    foreach (Type type in supportedTypes)
                    {
                        this.Converters[type] = converter;
                    }
                    continue;
                }
            }
        }
/*
        public string Serialize(object obj)
        {
            StringBuilder output = new StringBuilder();
            this.Serialize(obj, output);
            return output.ToString();
        }

        public void Serialize(object obj, StringBuilder output)
        {
            this.SerializeValue(obj, output, 0, null);
            if (output.Length > this.MaxJsonLength)
            {
                throw new InvalidOperationException(AtlasWeb.JSON_MaxJsonLengthExceeded);
            }
        }

        private static void SerializeBoolean(bool o, StringBuilder sb)
        {
            if (o)
            {
                sb.Append("true");
            }
            else
            {
                sb.Append("false");
            }
        }

        private void SerializeCustomObject(object o, StringBuilder sb, int depth, Hashtable objectsInUse)
        {
            bool flag = true;
            Type type = o.GetType();
            sb.Append('{');
            if (this.TypeResolver != null)
            {
                string str = this.TypeResolver.ResolveTypeId(type);
                if (str != null)
                {
                    SerializeString("__type", sb);
                    sb.Append(':');
                    this.SerializeValue(str, sb, depth, objectsInUse);
                    flag = false;
                }
            }
            foreach (FieldInfo info in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!info.IsDefined(typeof(ScriptIgnoreAttribute), true))
                {
                    if (!flag)
                    {
                        sb.Append(',');
                    }
                    SerializeString(info.Name, sb);
                    sb.Append(':');
                    this.SerializeValue(info.GetValue(o), sb, depth, objectsInUse);
                    flag = false;
                }
            }
            foreach (PropertyInfo info2 in type.GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance))
            {
                if (!info2.IsDefined(typeof(ScriptIgnoreAttribute), true))
                {
                    MethodInfo getMethod = info2.GetGetMethod();
                    if ((getMethod != null) && (getMethod.GetParameters().Length <= 0))
                    {
                        if (!flag)
                        {
                            sb.Append(',');
                        }
                        SerializeString(info2.Name, sb);
                        sb.Append(':');
                        this.SerializeValue(getMethod.Invoke(o, null), sb, depth, objectsInUse);
                        flag = false;
                    }
                }
            }
            sb.Append('}');
        }

        private static void SerializeDateTime(DateTime datetime, StringBuilder sb)
        {
            sb.Append("\"\\/Date(");
            sb.Append((long)((datetime.ToUniversalTime().Ticks - DatetimeMinTimeTicks) / 0x2710L));
            sb.Append(")\\/\"");
        }

        private void SerializeDictionary(IDictionary o, StringBuilder sb, int depth, Hashtable objectsInUse)
        {
            sb.Append('{');
            bool flag = true;
            foreach (DictionaryEntry entry in o)
            {
                if (!flag)
                {
                    sb.Append(',');
                }
                if (!(entry.Key is string))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, AtlasWeb.JSON_DictionaryTypeNotSupported, new object[] { o.GetType().FullName }));
                }
                SerializeString((string)entry.Key, sb);
                sb.Append(':');
                this.SerializeValue(entry.Value, sb, depth, objectsInUse);
                flag = false;
            }
            sb.Append('}');
        }

        private void SerializeEnumerable(IEnumerable enumerable, StringBuilder sb, int depth, Hashtable objectsInUse)
        {
            sb.Append('[');
            bool flag = true;
            foreach (object obj2 in enumerable)
            {
                if (!flag)
                {
                    sb.Append(',');
                }
                this.SerializeValue(obj2, sb, depth, objectsInUse);
                flag = false;
            }
            sb.Append(']');
        }

        private static void SerializeGuid(Guid guid, StringBuilder sb)
        {
            sb.Append("\"").Append(guid.ToString()).Append("\"");
        }

        internal static string SerializeInternal(object o)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(o);
        }

        private static void SerializeString(string input, StringBuilder sb)
        {
            sb.Append('"');
            sb.Append(JavaScriptString.QuoteString(input));
            sb.Append('"');
        }

        private static void SerializeUri(Uri uri, StringBuilder sb)
        {
            sb.Append("\"").Append(uri.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped)).Append("\"");
        }

        private void SerializeValue(object o, StringBuilder sb, int depth, Hashtable objectsInUse)
        {
            if (++depth > this._recursionLimit)
            {
                throw new ArgumentException(AtlasWeb.JSON_DepthLimitExceeded);
            }
            JavaScriptConverter converter = null;
            if ((o != null) && this.ConverterExistsForType(o.GetType(), out converter))
            {
                IDictionary<string, object> dictionary = converter.Serialize(o, this);
                if (this.TypeResolver != null)
                {
                    string str = this.TypeResolver.ResolveTypeId(o.GetType());
                    if (str != null)
                    {
                        dictionary["__type"] = str;
                    }
                }
                sb.Append(this.Serialize(dictionary));
            }
            else
            {
                this.SerializeValueInternal(o, sb, depth, objectsInUse);
            }
        }

        private void SerializeValueInternal(object o, StringBuilder sb, int depth, Hashtable objectsInUse)
        {
            if ((o == null) || DBNull.Value.Equals(o))
            {
                sb.Append("null");
            }
            else
            {
                string input = o as string;
                if (input != null)
                {
                    SerializeString(input, sb);
                }
                else if (o is char)
                {
                    if (((char)o) == '\0')
                    {
                        sb.Append("null");
                    }
                    else
                    {
                        SerializeString(o.ToString(), sb);
                    }
                }
                else if (o is bool)
                {
                    SerializeBoolean((bool)o, sb);
                }
                else if (o is DateTime)
                {
                    SerializeDateTime((DateTime)o, sb);
                }
                else if (o is Guid)
                {
                    SerializeGuid((Guid)o, sb);
                }
                else
                {
                    Uri uri = o as Uri;
                    if (uri != null)
                    {
                        SerializeUri(uri, sb);
                    }
                    else if (o is double)
                    {
                        sb.Append(((double)o).ToString("r", CultureInfo.InvariantCulture));
                    }
                    else if (o is float)
                    {
                        sb.Append(((float)o).ToString("r", CultureInfo.InvariantCulture));
                    }
                    else if (o.GetType().IsPrimitive || (o is decimal))
                    {
                        IConvertible convertible = o as IConvertible;
                        if (convertible != null)
                        {
                            sb.Append(convertible.ToString(CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            sb.Append(o.ToString());
                        }
                    }
                    else
                    {
                        Type type = o.GetType();
                        if (type.IsEnum)
                        {
                            sb.Append((int)o);
                        }
                        else
                        {
                            try
                            {
                                if (objectsInUse == null)
                                {
                                    objectsInUse = new Hashtable(new ReferenceComparer());
                                }
                                else if (objectsInUse.ContainsKey(o))
                                {
                                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, AtlasWeb.JSON_CircularReference, new object[] { type.FullName }));
                                }
                                objectsInUse.Add(o, null);
                                IDictionary dictionary = o as IDictionary;
                                if (dictionary != null)
                                {
                                    this.SerializeDictionary(dictionary, sb, depth, objectsInUse);
                                }
                                else
                                {
                                    IEnumerable enumerable = o as IEnumerable;
                                    if (enumerable != null)
                                    {
                                        this.SerializeEnumerable(enumerable, sb, depth, objectsInUse);
                                    }
                                    else
                                    {
                                        this.SerializeCustomObject(o, sb, depth, objectsInUse);
                                    }
                                }
                            }
                            finally
                            {
                                if (objectsInUse != null)
                                {
                                    objectsInUse.Remove(o);
                                }
                            }
                        }
                    }
                }
            }
        }
*/
        // Properties
        private Dictionary<Type, JavaScriptConverter> Converters
        {
            get
            {
                if (this._converters == null)
                {
                    this._converters = new Dictionary<Type, JavaScriptConverter>();
                }
                return this._converters;
            }
        }

        public int RecursionLimit
        {
            get
            {
                return this._recursionLimit;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException();//AtlasWeb.JSON_InvalidRecursionLimit);
                }
                this._recursionLimit = value;
            }
        }

        internal JavaScriptTypeResolver TypeResolver
        {
            get
            {
                return this._typeResolver;
            }
        }

        // Nested Types
        private class ReferenceComparer : IEqualityComparer
        {
            // Methods
            bool IEqualityComparer.Equals(object x, object y)
            {
                return (x == y);
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
