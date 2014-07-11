/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

namespace ESRI.ArcGIS.Mapping.Core
{
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class LocationProviderField
    {
        public abstract string GetName();

        public abstract string GetDisplayName();

        public abstract void SetDisplayName(string displayName);
    }
}
