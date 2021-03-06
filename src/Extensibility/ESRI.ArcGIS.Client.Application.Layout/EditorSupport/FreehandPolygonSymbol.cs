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
    internal sealed class FreehandPolygonSymbol : FillMarkerSymbol
    {
        private Path _symbolShape = null;
        internal FreehandPolygonSymbol()
            : base()
        {
            this.ControlTemplate = ResourceUtility.LoadEmbeddedResource("EditorSupport/SymbolTemplates.xaml", "FreehandPolygonSymbol") 
                as ControlTemplate;
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(double), typeof(FreehandPolygonSymbol), new PropertyMetadata(45d, OnPropertyChanged));

        internal double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public override void ReceiveObject(object obj)
        {
            if (obj is Path)
                _symbolShape = obj as Path;
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
