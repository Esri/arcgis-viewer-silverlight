/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
    public class MultiplePathMarkerSymbol : PathMarkerSymbol, ICustomSymbol
    {

        public MultiplePathMarkerSymbol()
        {
            loadControlTemplate();
        }

        private void loadControlTemplate()
        {
            ControlTemplate = ResourceData.Dictionary["MultiplePathMarkerSymbol"] as ControlTemplate;
        }

        #region Path Data
        public string PathData1
        {
            get { return (string)GetValue(PathData1Property); }
            set { SetValue(PathData1Property, value); }
        }

        private static void OnPathData1Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiplePathMarkerSymbol dp = d as MultiplePathMarkerSymbol;
            if (dp != null)
                dp.OnPropertyChanged("PathData1");
        }

        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...

        public static readonly DependencyProperty PathData1Property =
            DependencyProperty.Register("PathData1", typeof(string), typeof(MultiplePathMarkerSymbol), new PropertyMetadata(OnPathData1Changed));

        public string PathData2
        {
            get { return (string)GetValue(PathData2Property); }
            set { SetValue(PathData2Property, value); }
        }

        private static void OnPathData2Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiplePathMarkerSymbol dp = d as MultiplePathMarkerSymbol;
            if (dp != null)
                dp.OnPropertyChanged("PathData2");
        }

        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...

        public static readonly DependencyProperty PathData2Property =
            DependencyProperty.Register("PathData2", typeof(string), typeof(MultiplePathMarkerSymbol), new PropertyMetadata(OnPathData2Changed));
        
        #endregion

        public override ESRI.ArcGIS.Client.Symbols.Symbol Clone()
        {
            ESRI.ArcGIS.Mapping.Core.Symbols.MultiplePathMarkerSymbol newSymbol = new ESRI.ArcGIS.Mapping.Core.Symbols.MultiplePathMarkerSymbol();
            CopyProperties(newSymbol);
            newSymbol.PathData1 = PathData1;
            newSymbol.PathData2 = PathData2;
            return newSymbol;
        }

        public override void Serialize(System.Xml.XmlWriter writer, System.Collections.Generic.Dictionary<string, string> Namespaces)
        {
            StartSerialization(writer, Namespaces);
            CustomSymbolXamlWriter customWriter = new CustomSymbolXamlWriter(writer, Namespaces);
            customWriter.WriteAttribute("PathData1", PathData1);
            customWriter.WriteAttribute("PathData2", PathData2);
            EndSerialization(writer, Namespaces);
        }
    }
}
