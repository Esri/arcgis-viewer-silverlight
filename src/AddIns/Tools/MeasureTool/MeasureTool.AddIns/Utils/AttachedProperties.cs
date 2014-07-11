/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows;
using ESRI.ArcGIS.Client;

namespace MeasureTool.Addins
{
    /// <summary>
    /// Provides attached properties relevant to the Measure add-in
    /// </summary>
    public static class AttachedProperties
    {
        /// <summary>
        /// Backing DependencyProperty for the <see cref="FreehandDrawMode"/> attached property
        /// </summary>
        public static DependencyProperty FreehandDrawModeProperty = DependencyProperty.RegisterAttached(
            "FreehandDrawMode", typeof(FreehandDrawMode), typeof(Draw), null);

        /// <summary>
        /// Gets the freehand drawing mode of the Draw object
        /// </summary>
        public static FreehandDrawMode GetFreehandDrawMode(Draw draw)
        {
            return (FreehandDrawMode)draw.GetValue(FreehandDrawModeProperty);
        }

        /// <summary>
        /// Sets the freehand drawing mode of the Draw object
        /// </summary>
        public static void SetFreehandDrawMode(Draw draw, FreehandDrawMode mode)
        {
            draw.SetValue(FreehandDrawModeProperty, mode);
        }
    }
}
