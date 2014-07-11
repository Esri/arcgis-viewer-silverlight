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
    internal sealed class EllipseSymbol : FillMarkerSymbol
    {
        private Ellipse _symbolShape = null;
        internal EllipseSymbol()
            : base()
        {
            this.ControlTemplate = ResourceUtility.LoadEmbeddedResource("EditorSupport/SymbolTemplates.xaml", "EllipseSymbol") 
                as ControlTemplate;
        }

        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
            "Width", typeof(double), typeof(EllipseSymbol), new PropertyMetadata(45d, OnPropertyChanged));

        internal double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly DependencyProperty HeightProperty = DependencyProperty.Register(
            "Height", typeof(double), typeof(EllipseSymbol), new PropertyMetadata(30d, OnPropertyChanged));

        internal double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
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
            {
                _symbolShape.Width = Width;
                _symbolShape.Height = Height;
            }
        }
    }
}
