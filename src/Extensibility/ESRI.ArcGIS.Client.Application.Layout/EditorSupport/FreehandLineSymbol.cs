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
    internal sealed class FreehandLineSymbol : ShapeMarkerSymbol
    {
        private Path _symbolShape = null;
        internal FreehandLineSymbol()
            : base()
        {
            this.ControlTemplate = ResourceUtility.LoadEmbeddedResource("EditorSupport/SymbolTemplates.xaml", "FreehandLineSymbol") 
                as ControlTemplate;
        }

        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
            "Width", typeof(double), typeof(FreehandLineSymbol), new PropertyMetadata(45d, OnPropertyChanged));

        internal double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
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
                _symbolShape.Width = Width;
        }
    }
}
