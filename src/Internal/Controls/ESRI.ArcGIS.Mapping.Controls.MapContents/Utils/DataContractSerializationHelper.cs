/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace ESRI.ArcGIS.Mapping.Controls.MapContents
{
    internal class DataContractSerializationHelper
    {
        public static string Serialize<T>(object obj, IEnumerable<Type> knownTypes)
        {
            DataContractSerializer serializer;
            if (knownTypes == null)
                serializer = new DataContractSerializer(typeof(T));
            else
                serializer = new DataContractSerializer(typeof(T), knownTypes);
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

            T objectInstance = Activator.CreateInstance<T>();
            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            DataContractSerializer xmlSerializer = null;
            if (knownTypes == null)
                xmlSerializer = new DataContractSerializer(typeof(T));
            else
                xmlSerializer = new DataContractSerializer(typeof(T), knownTypes);
            memoryStream.Position = 0;
            objectInstance = (T)xmlSerializer.ReadObject(memoryStream);
            memoryStream.Close();
            return objectInstance;
        }

        public static T Deserialize<T>(string xml)
        {
            return Deserialize<T>(xml, null);
        }

    }
}
