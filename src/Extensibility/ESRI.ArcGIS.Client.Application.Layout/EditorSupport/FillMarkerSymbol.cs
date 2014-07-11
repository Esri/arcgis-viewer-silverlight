/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ESRI.ArcGIS.Client.Application.Layout.EditorSupport
{
    internal class FillMarkerSymbol : ShapeMarkerSymbol
    {
        private Shape _symbolShape = null;
        internal FillMarkerSymbol()
            : base()
        {
            Fill = new SolidColorBrush(Colors.White);
        }

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            "Fill", typeof(Brush), typeof(FillMarkerSymbol), new PropertyMetadata(OnPropertyChanged));

        internal Brush Fill
        {
            get { return GetValue(FillProperty) as Brush; }
            set { SetValue(FillProperty, value); }
        }

        public override void ReceiveObject(object obj)
        {
            if (obj is Shape)
                _symbolShape = obj as Shape;
            base.ReceiveObject(obj);
        }

        protected override void UpdateSymbolShape()
        {
            base.UpdateSymbolShape();
            if (_symbolShape != null)
                _symbolShape.Fill = Fill;
        }
    }
}
