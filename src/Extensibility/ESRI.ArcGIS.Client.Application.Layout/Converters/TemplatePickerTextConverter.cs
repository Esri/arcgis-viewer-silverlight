/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ESRI.ArcGIS.Client.FeatureService;
using ESRI.ArcGIS.Client.Application.Layout;
using System.ComponentModel;


namespace ESRI.ArcGIS.Client.Application.Layout.Converters
{
    /// <summary>
    /// Converts a <see cref="ESRI.ArcGIS.Client.FeatureService.FeatureTemplate"/> to a string containing
    /// instructions for drawing features on the map
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TemplatePickerTextConverter : IValueConverter
    {
        /// <summary>
        /// Perform the conversion to an instructional string
        /// </summary>
        /// <param name="value">The source FeatureTemplate being passed to the target</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic</param>
        /// <param name="culture">The culture of the conversion</param>
        /// <returns>
        /// The string to be passed to the target dependency property
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = string.Empty;
            FeatureTemplate ft = value as FeatureTemplate;
            if (ft != null)
            {
                if (ft.DrawingTool != FeatureEditTool.None)
                {
                    if (ft.DrawingTool == FeatureEditTool.Point)
                        text = string.Format(Resources.Strings.AddPointInstructions, ft.Name);
                    else
                        text = string.Format(Resources.Strings.AddShapeInstructions, ft.Name);
                }
                else
                    text = ft.Name;
                
            }
            
            return text;
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
