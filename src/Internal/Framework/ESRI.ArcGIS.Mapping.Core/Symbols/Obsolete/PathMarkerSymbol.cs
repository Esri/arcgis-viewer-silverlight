/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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

namespace ESRI.ArcGIS.Mapping.Core.Symbols
{
    public class PathMarkerSymbol : MarkerSymbol, ICustomSymbol
    {
        public PathMarkerSymbol()
        {
            loadControlTemplate();
            PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(PathMarkerSymbol_PropertyChanged);
        }

        private void loadControlTemplate()
        {
            ControlTemplate = ResourceData.Dictionary["PathMarkerSymbol"] as ControlTemplate;
        }

        void PathMarkerSymbol_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Size")
            {
                PathMarkerSymbol dp = sender as PathMarkerSymbol;
                if (dp != null)
                {
                    SymbolExtensions.SetSymbolScaleY(this, dp.Size * dp.ScaleY);
                    SymbolExtensions.SetSymbolScaleX(this, dp.Size * dp.ScaleX);
                    OnPropertyChanged(null);
                }
            }
        }

        protected override void setOffset()
        {
            Offset = new Point(Size * -OriginX + TransformX * Size * ScaleX, Size * -OriginY + TransformY * Size * ScaleY);
        }
        public virtual ESRI.ArcGIS.Client.Symbols.Symbol Clone()
        {
            ESRI.ArcGIS.Mapping.Core.Symbols.PathMarkerSymbol newSymbol = new ESRI.ArcGIS.Mapping.Core.Symbols.PathMarkerSymbol();
            CopyProperties(newSymbol);
            return newSymbol;
        }

        public virtual void Serialize(System.Xml.XmlWriter writer, System.Collections.Generic.Dictionary<string, string> Namespaces)
        {
            StartSerialization(writer, Namespaces);
            EndSerialization(writer, Namespaces);
        }

        #region Properties
        #region ScaleX
        public double ScaleX
        {
            get { return (double)GetValue(ScaleXProperty); }
            set { SetValue(ScaleXProperty, value); }
        }

        private static void OnScaleXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PathMarkerSymbol dp = d as PathMarkerSymbol;
            if (dp != null)
            {
                SymbolExtensions.SetSymbolScaleX(dp, dp.Size * dp.ScaleX);
                dp.setOffset();
                dp.OnPropertyChanged(null);
            }
        }
        // Using a DependencyProperty as the backing store for ScaleX.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleXProperty =
            DependencyProperty.Register("ScaleX", typeof(double), typeof(PathMarkerSymbol), new PropertyMetadata(1d, OnScaleXChanged));
        #endregion

        #region ScaleY
        public double ScaleY
        {
            get { return (double)GetValue(ScaleYProperty); }
            set { SetValue(ScaleYProperty, value); }
        }

        private static void OnScaleYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PathMarkerSymbol dp = d as PathMarkerSymbol;
            if (dp != null)
            {
                SymbolExtensions.SetSymbolScaleY(dp, dp.Size * dp.ScaleY);
                dp.setOffset();
                dp.OnPropertyChanged(null);
            }
        }

        // Using a DependencyProperty as the backing store for ScaleY.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleYProperty =
            DependencyProperty.Register("ScaleY", typeof(double), typeof(PathMarkerSymbol), new PropertyMetadata(1d, OnScaleYChanged));
        #endregion

        #region Width & Height
        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Width.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register("Width", typeof(double), typeof(PathMarkerSymbol), null);

        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Height.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(PathMarkerSymbol), null);
        #endregion

        #region TransformX and Y
        public double TransformX
        {
            get { return (double)GetValue(TransformXProperty); }
            set { SetValue(TransformXProperty, value); }
        }

        private static void OnTransformXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PathMarkerSymbol dp = d as PathMarkerSymbol;
            if (dp != null)
            {
                dp.setOffset();
                dp.OnPropertyChanged("TransformX");
            }
        }

        // Using a DependencyProperty as the backing store for X.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransformXProperty =
            DependencyProperty.Register("TransformX", typeof(double), typeof(PathMarkerSymbol), new PropertyMetadata(0d, OnTransformXChanged));

        public double TransformY
        {
            get { return (double)GetValue(TransformYProperty); }
            set { SetValue(TransformYProperty, value); }
        }

        private static void OnTransformYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PathMarkerSymbol dp = d as PathMarkerSymbol;
            if (dp != null)
            {
                dp.setOffset();
                dp.OnPropertyChanged("TransformY");
            }
        }

        // Using a DependencyProperty as the backing store for Y.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransformYProperty =
            DependencyProperty.Register("TransformY", typeof(double), typeof(PathMarkerSymbol), new PropertyMetadata(0d, OnTransformYChanged));
        #endregion

        #region PathData
        public string PathData
        {
            get { return (string)GetValue(PathDataProperty); }
            set {  SetValue(PathDataProperty, value); }
        }

        private static void OnPathDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PathMarkerSymbol dp = d as PathMarkerSymbol;
            if (dp != null)
                dp.OnPropertyChanged("PathData");
        }

        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...

        public static readonly DependencyProperty PathDataProperty =
            DependencyProperty.Register("PathData", typeof(string), typeof(PathMarkerSymbol), new PropertyMetadata(OnPathDataChanged));
        #endregion
        #endregion

        internal void CopyProperties(ESRI.ArcGIS.Mapping.Core.Symbols.PathMarkerSymbol newSymbol)
        {
            newSymbol.PathData = PathData;
            newSymbol.ScaleX = ScaleX;
            newSymbol.ScaleY = ScaleY;
            newSymbol.TransformX = TransformX;
            newSymbol.TransformY = TransformY;
            newSymbol.Width = Width;
            newSymbol.Height = Height;
            CopyProperties(this, newSymbol);
        }

        protected void CopyProperties(ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol orig, ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol copy)
        {
            copy.DisplayName = orig.DisplayName;
            copy.Offset = new Point(orig.Offset.X, orig.Offset.Y);
            copy.Color = orig.Color != null ? orig.Color.CloneBrush() : null;
            copy.ControlTemplate = orig.ControlTemplate;
            copy.OffsetX = orig.OffsetX;
            copy.OffsetY = orig.OffsetY;
            copy.OriginX = orig.OriginX;
            copy.OriginY = orig.OriginY;
            copy.Opacity = orig.Opacity;
            copy.Size = orig.Size;
        }

        protected void StartSerialization(System.Xml.XmlWriter writer, System.Collections.Generic.Dictionary<string, string> Namespaces)
        {
            CustomSymbolXamlWriter customWriter = new CustomSymbolXamlWriter(writer, Namespaces);
            customWriter.StartType(this, Constants.esriMappingPrefix);
            customWriter.WriteAttribute("DisplayName", DisplayName);
            customWriter.WriteAttribute("OriginX", OriginX);
            customWriter.WriteAttribute("OriginY", OriginY);
            if (OffsetX != 0)
                customWriter.WriteAttribute("OffsetX", OffsetX);
            if (OffsetY != 0)
                customWriter.WriteAttribute("OffsetY", OffsetY);
            if (Opacity < 1)
                customWriter.WriteAttribute("Opacity", Opacity);
            customWriter.WriteAttribute("Size", Size);
            customWriter.WriteAttribute("PathData", PathData);
            customWriter.WriteAttribute("ScaleX", ScaleX);
            customWriter.WriteAttribute("ScaleY", ScaleY);
            customWriter.WriteAttribute("TransformX", TransformX);
            customWriter.WriteAttribute("TransformY", TransformY);
            customWriter.WriteAttribute("Width", Width);
            customWriter.WriteAttribute("Height", Height);
        }
        protected void EndSerialization(System.Xml.XmlWriter writer, System.Collections.Generic.Dictionary<string, string> Namespaces)
        {
            if (Color != null)
            {
                writer.WriteStartElement("MarkerSymbol.Color", Namespaces[Constants.esriMappingPrefix]);
                new BrushXamlWriter(writer, Namespaces).WriteBrush(Color);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }
}
