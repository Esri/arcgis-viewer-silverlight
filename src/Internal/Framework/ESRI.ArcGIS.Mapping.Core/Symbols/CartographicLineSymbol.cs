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
using ESRI.ArcGIS.Client.Symbols;

namespace ESRI.ArcGIS.Mapping.Core.Symbols
{
    public sealed class CartographicLineSymbol : ESRI.ArcGIS.Client.Symbols.CartographicLineSymbol
    {
        #region Ctors
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLineSymbol"/> class.
        /// </summary>
        public CartographicLineSymbol()
        {
            SelectionColor = new SolidColorBrush(Colors.Cyan);
            ControlTemplate = ResourceData.Dictionary["CartographicLineSymbol"] as ControlTemplate;
        }
        #endregion

        #region SelectionColor
        /// <summary>
        /// Identifies the <see cref="Color"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectionColorProperty =
            DependencyProperty.Register("SelectionColor", typeof(Brush), typeof(CartographicLineSymbol), new PropertyMetadata(OnSelectionColorChanged));
     
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
            CartographicLineSymbol dp = d as CartographicLineSymbol;
            if(dp != null)
                dp.OnPropertyChanged("SelectionColor");
        }
        #endregion

    }
}
