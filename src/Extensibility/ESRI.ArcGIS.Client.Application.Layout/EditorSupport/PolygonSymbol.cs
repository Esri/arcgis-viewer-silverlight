/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ESRI.ArcGIS.Client.Application.Layout.EditorSupport
{
    internal sealed class PolygonSymbol : FillMarkerSymbol
    {
        private Polygon _symbolShape = null;
        internal PolygonSymbol() : base()
        {
            this.ControlTemplate = ResourceUtility.LoadEmbeddedResource("EditorSupport/SymbolTemplates.xaml", "PolygonSymbol") 
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
            if (obj is Polygon)
                _symbolShape = obj as Polygon;
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
