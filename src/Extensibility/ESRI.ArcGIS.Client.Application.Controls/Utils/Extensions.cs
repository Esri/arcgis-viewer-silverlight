/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;

namespace ESRI.ArcGIS.Client.Application.Controls
{
    internal static class Extensions
    {
        /// <summary>
        /// Returns if the double value is approximately the same as 
        /// the input double (within a tolerance of 0.0001)
        /// </summary>
        public static bool NearlyEquals(this double a, double b)
        {
            // addresses the double rounding loss of precision
            double tolerance = 0.0001;
            return (((b - tolerance) < a) && (a < (b + tolerance)));
        }
    }
}
