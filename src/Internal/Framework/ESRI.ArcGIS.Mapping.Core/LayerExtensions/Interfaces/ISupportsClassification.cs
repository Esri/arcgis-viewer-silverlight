/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Core
{
    public interface ISupportsClassification
    {
        List<FieldInfo> NumericFields { get; }        
    }
}
