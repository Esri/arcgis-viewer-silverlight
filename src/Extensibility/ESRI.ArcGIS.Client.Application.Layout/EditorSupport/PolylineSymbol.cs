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
    internal sealed class PolylineSymbol : ShapeMarkerSymbol
    {
        private Polyline _symbolShape = null;
        internal PolylineSymbol() : base()
        {
            this.ControlTemplate = ResourceUtility.LoadEmbeddedResource("EditorSupport/SymbolTemplates.xaml", "PolylineSymbol") 
                as ControlTemplate;
        }

        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
            "Width", typeof(double), typeof(PolylineSymbol), new PropertyMetadata(45d, OnPropertyChanged));

        internal double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public override void ReceiveObject(object obj)
        {
            if (obj is Polyline)
                _symbolShape = obj as Polyline;
            base.ReceiveObject(obj);
        }

        protected override void UpdateSymbolShape()
        {
            base.UpdateSymbolShape();
            if (_symbolShape != null)
                _symbolShape.Width = Width;
        }
    }
}
