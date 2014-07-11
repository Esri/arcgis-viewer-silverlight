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

namespace ESRI.ArcGIS.Mapping.Core.Symbols
{

    public sealed class SimpleFillSymbol : ESRI.ArcGIS.Client.Symbols.FillSymbol
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleFillSymbol"/> class.
		/// </summary>
        public SimpleFillSymbol() : base()
		{
			ControlTemplate = ResourceData.Dictionary["FillSymbol"] as ControlTemplate;
        }

        #region SelectionColor
        /// <summary>
        /// Identifies the <see cref="Color"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectionColorProperty =
            DependencyProperty.Register("SelectionColor", typeof(Brush), typeof(SimpleFillSymbol), new PropertyMetadata(OnSelectionColorChanged));
        
        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        public Brush SelectionColor
        {
            get
            {
                return (Brush)GetValue(SelectionColorProperty);
            }
            set
            {
                SetValue(SelectionColorProperty, value);
            }
        }

        private static void OnSelectionColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SimpleFillSymbol dp = d as SimpleFillSymbol;
            if (dp != null)
                dp.OnPropertyChanged("SelectionColor");
        }
        #endregion
 
    }
}
