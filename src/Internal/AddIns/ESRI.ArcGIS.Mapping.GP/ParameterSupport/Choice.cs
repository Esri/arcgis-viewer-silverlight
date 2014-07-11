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
using ESRI.ArcGIS.Client.Tasks;
using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.GP.ParameterSupport
{
    //Currently only supports GPString
    public class Choice
    {
        public string DisplayText { get; set; }
        public GPParameter Value { get; set; }

        internal virtual void ToJson(JsonWriter jw)
        {
            jw.StartObject();
            jw.WriteProperty("displayText", DisplayText);
            jw.Writer.Write(",");
            if (Value is GPString)
            {
                jw.WriteProperty("value", (Value as GPString).Value);
            }
            jw.EndObject();
        }

        internal virtual void FromJsonDictionary(IDictionary<string, object> dictionary, string paramName)
        {
            if (dictionary.ContainsKey("displayText"))
                DisplayText = dictionary["displayText"] as string;
            if (dictionary.ContainsKey("value") && dictionary["value"] is string)
                Value = new GPString(paramName, dictionary["value"] as string);
        }

    }


}
