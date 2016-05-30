/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ESRI.ArcGIS.Client.Application.Layout.EditorSupport
{
    internal sealed class CircleSymbol : FillMarkerSymbol
    {
        private Ellipse _symbolShape = null;
        internal CircleSymbol() : base()
        {
            ControlTemplate = ResourceUtility.LoadEmbeddedResource("EditorSupport/SymbolTemplates.xaml", "CircleSymbol") 
                as ControlTemplate;
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(double), typeof(PolygonSymbol), new PropertyMetadata(45d, OnPropertyChanged));

        internal double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public override void ReceiveObject(object obj)
        {
            if (obj is Ellipse)
                _symbolShape = obj as Ellipse;
            base.ReceiveObject(obj);
        }

        protected override void UpdateSymbolShape()
        {
            base.UpdateSymbolShape();
            if (_symbolShape != null)
                _symbolShape.Width = _symbolShape.Height = Size;
        }
    }
}
