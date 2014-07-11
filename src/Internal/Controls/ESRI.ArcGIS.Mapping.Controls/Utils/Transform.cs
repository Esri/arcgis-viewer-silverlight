/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Media;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class Transform
    {
        /// <summary>
        /// This method was brought over from core's Transform.cs class to handle
        /// the special cases that can be encountered looking for the root visual
        /// (in Silverlight vs WPF, etc)
        /// </summary>
        internal static GeneralTransform GetTransformToRoot(FrameworkElement child)
        {
            if (Application.Current != null)
            {
                if (child.FlowDirection == FlowDirection.RightToLeft)
                    return child.TransformToVisual(null);
                return child.TransformToVisual(Application.Current.RootVisual);
            }
            else
                return child.TransformToVisual(null);
        }
    }
}
