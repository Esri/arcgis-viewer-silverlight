/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class JsonSerializer
    {
        public static string Serialize<T>(object obj, IEnumerable<Type> knownTypes)
        {
            if (obj == null)
                return null;

            DataContractJsonSerializer serializer;
            if (knownTypes == null)
                serializer = new DataContractJsonSerializer(typeof(T));
            else
                serializer = new DataContractJsonSerializer(typeof(T), knownTypes);
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, obj);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string result = reader.ReadToEnd();
            //TODO:- SL has a bug where UTF8 isn't generated.
            return result;
        }

        public static string Serialize<T>(object obj)
        {
            return Serialize<T>(obj, null);
        }

        public static T Deserialize<T>(string xml, IEnumerable<Type> knownTypes)
        {
            if (string.IsNullOrEmpty(xml))
                return default(T);

            T objectInstance = default(T); // Activator.CreateInstance<T>();
            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            DataContractJsonSerializer jsonSerializer = null;
            if (knownTypes == null)
                jsonSerializer = new DataContractJsonSerializer(typeof(T));
            else
                jsonSerializer = new DataContractJsonSerializer(typeof(T), knownTypes);
            memoryStream.Position = 0;
            objectInstance = (T)jsonSerializer.ReadObject(memoryStream);
            memoryStream.Close();
            return objectInstance;
        }

        public static T Deserialize<T>(string json)
        {
            return Deserialize<T>(json, null);
        }
    }
}
