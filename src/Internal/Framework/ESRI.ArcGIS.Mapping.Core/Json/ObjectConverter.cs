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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.ComponentModel;

namespace ESRI.ArcGIS.Client.Utils
{
    internal static class ObjectConverter
    {
        // Fields
        private static Type _dictionaryGenericType = typeof(Dictionary<,>);
        private static Type _enumerableGenericType = typeof(IEnumerable<>);
        private static Type _idictionaryGenericType = typeof(IDictionary<,>);
        private static Type _listGenericType = typeof(List<>);
        private static readonly Type[] s_emptyTypeArray = new Type[0];

        // Methods
        private static void AddItemToList(IList oldList, IList newList, Type elementType, JavaScriptSerializer serializer)
        {
            foreach (object obj2 in oldList)
            {
                newList.Add(ConvertObjectToType(obj2, elementType, serializer));
            }
        }

        private static void AssignToPropertyOrField(object propertyValue, object o, string memberName, JavaScriptSerializer serializer)
        {
            IDictionary dictionary = o as IDictionary;
            if (dictionary != null)
            {
                propertyValue = ConvertObjectToType(propertyValue, null, serializer);
                dictionary[memberName] = propertyValue;
            }
            else
            {
                Type type = o.GetType();
                PropertyInfo property = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    MethodInfo setMethod = property.GetSetMethod();
                    if (setMethod != null)
                    {
                        propertyValue = ConvertObjectToType(propertyValue, property.PropertyType, serializer);
                        setMethod.Invoke(o, new object[] { propertyValue });
                        return;
                    }
                }
                FieldInfo field = type.GetField(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (field != null)
                {
                    propertyValue = ConvertObjectToType(propertyValue, field.FieldType, serializer);
                    field.SetValue(o, propertyValue);
                }
            }
        }

        private static object ConvertDictionaryToObject(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            object obj2;
            Type t = type;
            string id = null;
            object o = dictionary;
            if (dictionary.TryGetValue("__type", out obj2))
            {
                id = ConvertObjectToType(obj2, typeof(string), serializer) as string;
                if (id != null)
                {
                    if (serializer.TypeResolver != null)
                    {
                        t = serializer.TypeResolver.ResolveType(id);
                        if (t == null)
                        {
                            throw new InvalidOperationException();
                        }
                    }
                    dictionary.Remove("__type");
                }
            }
            JavaScriptConverter converter = null;
            if ((t != null) && serializer.ConverterExistsForType(t, out converter))
            {
                return converter.Deserialize(dictionary, t, serializer);
            }
            if ((id != null) || ((t != null) && IsClientInstantiatableType(t, serializer)))
            {
                o = Activator.CreateInstance(t);
            }
            List<string> list = new List<string>(dictionary.Keys);
            if ((((type != null) && type.IsGenericType) && (typeof(IDictionary).IsAssignableFrom(type) || (type.GetGenericTypeDefinition() == _idictionaryGenericType))) && (type.GetGenericArguments().Length == 2))
            {
                Type type3 = type.GetGenericArguments()[0];
                if ((type3 != typeof(string)) && (type3 != typeof(object)))
                {
                    throw new InvalidOperationException();//string.Format(CultureInfo.InvariantCulture, AtlasWeb.JSON_DictionaryTypeNotSupported, new object[] { type.FullName }));
                }
                Type type4 = type.GetGenericArguments()[1];
                IDictionary dictionary2 = null;
                if (IsClientInstantiatableType(type, serializer))
                {
                    dictionary2 = (IDictionary)Activator.CreateInstance(type);
                }
                else
                {
                    dictionary2 = (IDictionary)Activator.CreateInstance(_dictionaryGenericType.MakeGenericType(new Type[] { type3, type4 }));
                }
                if (dictionary2 != null)
                {
                    foreach (string str2 in list)
                    {
                        dictionary2[str2] = ConvertObjectToType(dictionary[str2], type4, serializer);
                    }
                    return dictionary2;
                }
            }
            if ((type != null) && !type.IsAssignableFrom(o.GetType()))
            {
                if (type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, s_emptyTypeArray, null) == null)
                {
                    throw new MissingMethodException();//string.Format(CultureInfo.InvariantCulture, AtlasWeb.JSON_NoConstructor, new object[] { type.FullName }));
                }
                throw new InvalidOperationException();//string.Format(CultureInfo.InvariantCulture, AtlasWeb.JSON_DeserializerTypeMismatch, new object[] { type.FullName }));
            }
            foreach (string str3 in list)
            {
                object propertyValue = dictionary[str3];
                AssignToPropertyOrField(propertyValue, o, str3, serializer);
            }
            return o;
        }

        private static IList ConvertListToObject(IList list, Type type, JavaScriptSerializer serializer)
        {
            if ((((type == null) || (type == typeof(object))) || (type.IsArray /*|| (type == typeof(ArrayList))*/)) || (((type == typeof(IEnumerable)) || (type == typeof(IList))) || (type == typeof(ICollection))))
            {
                Type elementType = typeof(object);
                if ((type != null) && (type != typeof(object)))
                {
                    elementType = type.GetElementType();
                }
                List<object> newList = new List<object>();
                AddItemToList(list, newList, elementType, serializer);
                /*if (((type != typeof(ArrayList)) && (type != typeof(IEnumerable))) && ((type != typeof(IList)) && (type != typeof(ICollection))))
                {
                    return newList.ToArray(elementType);
                }*/
                return newList;
            }
            if (type.IsGenericType && (type.GetGenericArguments().Length == 1))
            {
                Type type3 = type.GetGenericArguments()[0];
                if (_enumerableGenericType.MakeGenericType(new Type[] { type3 }).IsAssignableFrom(type))
                {
                    Type type5 = _listGenericType.MakeGenericType(new Type[] { type3 });
                    IList list3 = null;
                    if (IsClientInstantiatableType(type, serializer) && typeof(IList).IsAssignableFrom(type))
                    {
                        list3 = (IList)Activator.CreateInstance(type);
                    }
                    else
                    {
                        if (type5.IsAssignableFrom(type))
                        {
                            throw new InvalidOperationException();//string.Format(CultureInfo.InvariantCulture, AtlasWeb.JSON_CannotCreateListType, new object[] { type.FullName }));
                        }
                        list3 = (IList)Activator.CreateInstance(type5);
                    }
                    AddItemToList(list, list3, type3, serializer);
                    return list3;
                }
            }
            else if (IsClientInstantiatableType(type, serializer) && typeof(IList).IsAssignableFrom(type))
            {
                IList list4 = (IList)Activator.CreateInstance(type);
                AddItemToList(list, list4, null, serializer);
                return list4;
            }
            throw new InvalidOperationException();//string.Format(CultureInfo.CurrentCulture, AtlasWeb.JSON_ArrayTypeNotSupported, new object[] { type.FullName }));
        }

        internal static object ConvertObjectToType(object o, Type type, JavaScriptSerializer serializer)
        {
            if (o == null)
            {
                if (type == typeof(char))
                {
                    return '\0';
                }
                if (((type != null) && type.IsValueType) && (!type.IsGenericType || (type.GetGenericTypeDefinition() != typeof(Nullable<>))))
                {
                    throw new InvalidOperationException();//AtlasWeb.JSON_ValueTypeCannotBeNull);
                }
                return null;
            }
            if (o.GetType() == type)
            {
                return o;
            }
            return o;
            //return ConvertObjectToTypeInternal(o, type, serializer);
        }

        private static object ConvertObjectToTypeInternal(object o, Type type, JavaScriptSerializer serializer)
        {
            IDictionary<string, object> dictionary = o as IDictionary<string, object>;
            if (dictionary != null)
            {
                return ConvertDictionaryToObject(dictionary, type, serializer);
            }
            IList list = o as IList;
            if (list != null)
            {
                return ConvertListToObject(list, type, serializer);
            }
            if ((type != null) && (o.GetType() != type))
            {
                //TypeConverter converter = TypeDescriptor.GetConverter(type);
                TypeConverter converter = (TypeConverter)type.GetCustomAttributes(typeof(TypeConverter), false)[0];
                if (converter.CanConvertFrom(o.GetType()))
                {
                    return converter.ConvertFrom(null, CultureInfo.InvariantCulture, o);
                }
                if (converter.CanConvertFrom(typeof(string)))
                {
                    //string text = TypeDescriptor.GetConverter(o).ConvertToInvariantString(o);
                    //return converter.ConvertFromInvariantString(text);
                    Type type2 = o.GetType();
                    TypeConverter converter2 = (TypeConverter)type2.GetCustomAttributes(typeof(TypeConverter), false)[0];
                    string text = converter2.ConvertToString(o);
                    return converter.ConvertFromString(text);
                }
                if (!type.IsAssignableFrom(o.GetType()))
                {
                    throw new InvalidOperationException();//string.Format(CultureInfo.CurrentCulture, AtlasWeb.JSON_CannotConvertObjectToType, new object[] { o.GetType(), type }));
                }
            }
            return o;
        }

        internal static bool IsClientInstantiatableType(Type t, JavaScriptSerializer serializer)
        {
            if ((t.IsAbstract || t.IsInterface) || t.IsArray)
            {
                return false;
            }
            if (t == typeof(object))
            {
                return false;
            }
            JavaScriptConverter converter = null;
            if (!serializer.ConverterExistsForType(t, out converter))
            {
                if (t.IsValueType)
                {
                    return true;
                }
                if (t.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, s_emptyTypeArray, null) == null)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
