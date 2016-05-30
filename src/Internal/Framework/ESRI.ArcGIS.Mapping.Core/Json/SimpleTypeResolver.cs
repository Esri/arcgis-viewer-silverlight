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

namespace ESRI.ArcGIS.Client.Utils
{
    internal class SimpleTypeResolver : JavaScriptTypeResolver
    {
        // Methods
        public override Type ResolveType(string id)
        {
            return Type.GetType(id);
        }

        public override string ResolveTypeId(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return type.AssemblyQualifiedName;
        }
    }

}
