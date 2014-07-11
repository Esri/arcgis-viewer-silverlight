/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public static class BehaviorUtils
    {
        public static Behavior<Map> GetMapBehavior(ExtensionBehavior extensionBehavior)
        {
            if (extensionBehavior == null || string.IsNullOrEmpty(extensionBehavior.CommandValueId))
                return null;

            if (extensionBehavior.MapBehavior != null)
                return extensionBehavior.MapBehavior as Behavior<Map>;

            string[] splits = extensionBehavior.CommandValueId.Split(new char[] { ';' });
            if (splits == null || splits.Length < 2)
                return null;

            string typeName = splits[0].Replace("Type=", "");
            string assemblyName = splits[1].Replace("Assembly=", "");

            foreach (Behavior<Map> mapBehavior in View.Instance.ExtensionMapBehaviors)
            {
                if (mapBehavior == null)
                    continue;
                Type type = mapBehavior.GetType();
                if (type.FullName == typeName && type.Assembly.FullName == assemblyName)
                {
                    return type.Assembly.CreateInstance(type.FullName) as Behavior<Map>;
                }
            }
            return null;
        }
    }
}
