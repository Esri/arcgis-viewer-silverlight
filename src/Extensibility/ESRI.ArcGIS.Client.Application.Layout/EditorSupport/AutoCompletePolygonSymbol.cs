/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client.FeatureService.Symbols;

namespace ESRI.ArcGIS.Client.Application.Layout.EditorSupport
{
    internal sealed class AutoCompletePolygonSymbol : FillMarkerSymbol
    {
        private StackPanel _symbolStackPanel = null;
        private Polygon _symbolShape = null;
        private Polygon _selectedShape = null;
        private DashArrayConverter _dashConverter = null;
        internal AutoCompletePolygonSymbol()
            : base()
        {
            ControlTemplate = ResourceUtility.LoadEmbeddedResource("EditorSupport/SymbolTemplates.xaml", "AutoCompletePolygonSymbol") 
                as ControlTemplate;
            _dashConverter = ResourceUtility.LoadEmbeddedResource("EditorSupport/SymbolTemplates.xaml", "DashArrayConverter") 
                as DashArrayConverter;
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(double), typeof(AutoCompletePolygonSymbol), new PropertyMetadata(45d, onPropertyChanged));

        internal double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        private static void onPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompletePolygonSymbol symbol = (AutoCompletePolygonSymbol)d;
            symbol.UpdateSymbolShape();
        }

        public override void ReceiveObject(object obj)
        {
            if (obj is StackPanel)
            {
                _symbolStackPanel = obj as StackPanel;
                if (_symbolStackPanel.Children.Count == 2)
                {
                    _symbolShape = _symbolStackPanel.Children[0] as Polygon;
                    _selectedShape = _symbolStackPanel.Children[1] as Polygon;
                }
            }

            UpdateSymbolShape();
        }

        protected override void UpdateSymbolShape()
        {
            if (_symbolShape != null)
            {
                _symbolShape.Fill = Fill;
                _symbolShape.StrokeThickness = StrokeThickness;
                _symbolShape.Stroke = Stroke;
                _symbolShape.StrokeDashArray = _dashConverter.Convert(StrokeStyle, null, null, null) as DoubleCollection;
            }

            if (_selectedShape != null)
            {
                _selectedShape.Fill = SelectionColor;
                _selectedShape.StrokeThickness = StrokeThickness;
                _selectedShape.Stroke = Stroke;
                _selectedShape.StrokeDashArray = _dashConverter.Convert(StrokeStyle, null, null, null) as DoubleCollection;
            }

            updateScaleTransform();
        }

        private void updateScaleTransform()
        {
            if (_symbolStackPanel != null)
            {
                ScaleTransform transform = _symbolStackPanel.RenderTransform as ScaleTransform;

                if (transform != null)
                {
                    double size = (double)Size;

                    // 48 is the natural size of the symbol.  Since the template specifies adjacent polygons,
                    // sizing must be done by scaling the symbol relative to this natural size rather than 
                    // manipulating widths.
                    transform.ScaleX = transform.ScaleY = size / 48;
                }
            }
        }
    }
}
