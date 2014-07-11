/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client.Symbols;

namespace ESRI.ArcGIS.Client.Application.Layout.EditorSupport
{
    internal class ShapeMarkerSymbol : MarkerSymbol, IReceiveObject
    {
        private Shape _symbolShape = null;
        private ESRI.ArcGIS.Client.FeatureService.Symbols.DashArrayConverter _dashConverter = null;
        internal ShapeMarkerSymbol()
            : base()
        {
            Stroke = new SolidColorBrush(Colors.Black);
            _dashConverter = ResourceUtility.LoadEmbeddedResource("EditorSupport/SymbolTemplates.xaml", "DashArrayConverter")
                as ESRI.ArcGIS.Client.FeatureService.Symbols.DashArrayConverter;
        }

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            "Stroke", typeof(Brush), typeof(ShapeMarkerSymbol), new PropertyMetadata(OnPropertyChanged));

        internal Brush Stroke
        {
            get { return GetValue(StrokeProperty) as Brush; }
            set { SetValue(StrokeProperty, value); }
        }

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            "StrokeThickness", typeof(double), typeof(ShapeMarkerSymbol), new PropertyMetadata(1.0, OnPropertyChanged));

        internal double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public static readonly DependencyProperty StrokeStyleProperty = DependencyProperty.Register(
            "StrokeStyle", typeof(SimpleLineSymbol.LineStyle), typeof(ShapeMarkerSymbol), 
            new PropertyMetadata(SimpleLineSymbol.LineStyle.Solid, OnPropertyChanged));

        internal SimpleLineSymbol.LineStyle StrokeStyle
        {
            get { return (SimpleLineSymbol.LineStyle)GetValue(StrokeStyleProperty); }
            set { SetValue(StrokeStyleProperty, value); }
        }

        public static readonly DependencyProperty SelectionColorProperty = DependencyProperty.Register(
            "SelectionColor", typeof(Brush), typeof(ShapeMarkerSymbol), new PropertyMetadata(OnPropertyChanged));

        internal Brush SelectionColor
        {
            get { return GetValue(SelectionColorProperty) as Brush; }
            set { SetValue(SelectionColorProperty, value); }
        }

        protected static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ShapeMarkerSymbol symbol = (ShapeMarkerSymbol)d;
            symbol.UpdateSymbolShape();
        }

        public virtual void ReceiveObject(object obj)
        {
            if (obj is Shape)
            {
                _symbolShape = obj as Shape;
                UpdateSymbolShape();
            }
        }

        protected virtual void UpdateSymbolShape()
        {
            if (_symbolShape != null)
            {
                _symbolShape.StrokeThickness = StrokeThickness;
                _symbolShape.Stroke = Stroke;
                _symbolShape.StrokeDashArray = _dashConverter.Convert(StrokeStyle, null, null, null) as DoubleCollection;
            }
        }
    }
}
