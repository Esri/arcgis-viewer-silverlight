/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace MeasureTool.Addins
{
    /// <summary>
    /// Enumeration of linear units
    /// </summary>
    public enum LengthUnits
    {
        [LocalizedPropertyDescription("UnitsCentimeters")]
        UnitsCentimeters,
        [LocalizedPropertyDescription("UnitsMeters")]
        UnitsMeters,
        [LocalizedPropertyDescription("UnitsKilometers")]
        UnitsKilometers,
        [LocalizedPropertyDescription("UnitsInches")]
        UnitsInches,
        [LocalizedPropertyDescription("UnitsFeet")]
        UnitsFeet,
        [LocalizedPropertyDescription("UnitsYards")]
        UnitsYards,
        [LocalizedPropertyDescription("UnitsMiles")]
        UnitsMiles,
        [LocalizedPropertyDescription("UnitsNauticalMiles")]
        UnitsNauticalMiles
    }

    /// <summary>
    /// Enumeration of areal units
    /// </summary>
    public enum AreaUnits
    {
        [LocalizedPropertyDescription("UnitsSquareMiles")]
        UnitsSquareMiles,
        [LocalizedPropertyDescription("UnitsSquareMeters")]
        UnitsSquareMeters,
        [LocalizedPropertyDescription("UnitsSquareKilometers")]
        UnitsSquareKilometers,
        [LocalizedPropertyDescription("UnitsSquareFeet")]
        UnitsSquareFeet,
        [LocalizedPropertyDescription("UnitsAcres")]
        UnitsAcres,
        [LocalizedPropertyDescription("UnitsHectares")]
        UnitsHectares
    }

    /// <summary>
    /// Represents the different freehand drawing modes
    /// </summary>
    public enum FreehandDrawMode { Polyline, Polygon }
}
