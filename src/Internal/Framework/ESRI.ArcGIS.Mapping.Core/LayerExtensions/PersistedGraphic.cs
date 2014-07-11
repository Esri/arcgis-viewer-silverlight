/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Runtime.Serialization;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Mapping.Core.Symbols;

namespace ESRI.ArcGIS.Mapping.Core
{
    [DataContract]
    public class PersistedGraphic
    {
        public PersistedGraphic() { }

        public PersistedGraphic(Graphic graphic)
        {
            if(graphic == null)
                return;

            Symbol = graphic.Symbol != null ? graphic.Symbol.CloneSymbol() : null;            
            Geometry = graphic.Geometry;
            Selected = graphic.Selected;
            TimeExtent = graphic.TimeExtent;

            if (Symbol != null)
            {
                if (Symbol is IJsonSerializable)
                {
                    SymbolJson = (Symbol as IJsonSerializable).ToJson();
                    Symbol = null;
                }
                else
                {
                    setNonSerializablePropertiesOfSymbolToNull();
                    if (!Symbol.GetType().IsVisible) // don't serialize private symbol types
                        Symbol = null;
                }
            }

            if (graphic.Attributes != null)
            {
                Attributes = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> pair in graphic.Attributes)
                {
                    string key = pair.Key;
                    string attributeValue = pair.Value as string;
                    if (attributeValue != null)
                    {
                        // String values are HtmlEncoded http://msdn.microsoft.com/en-us/library/system.windows.browser.httputility.htmlencode%28v=VS.95%29.aspx
                        // Replace & with &amp;
                        // Replace < with &lt;
                        // Replace > with &gt;
                        // Note:- We however don't want to encode quotes as it is a special character in JSON formatting as well
                        Attributes.Add(key, System.Windows.Browser.HttpUtility.HtmlEncode(attributeValue).Replace("&quot;","\""));
                    }
                    else
                    {
                        Attributes.Add(pair);
                    }
                }
            }
        }

        private void setNonSerializablePropertiesOfSymbolToNull()
        {
            // Control templates cannot be serialized
            Symbol.ControlTemplate = null;

            // Set all brushes to null - we can't serialize brushes
            ImageFillSymbol imageFillSymbol = Symbol as ImageFillSymbol;
            if (imageFillSymbol != null)
            {
                imageFillSymbol.Fill = null;
                imageFillSymbol.Color = null;
            }
            else
            {
                ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol markerSymbol = Symbol as ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol;
                if (markerSymbol != null)
                {
                    markerSymbol.Color = null;
                }
                else
                {
                    FillSymbol fillSymbol = Symbol as FillSymbol;
                    if (fillSymbol != null)
                    {
                        fillSymbol.BorderBrush = null;
                        fillSymbol.Fill = null;
                    }
                    else
                    {
                        LineSymbol lineSymbol = Symbol as LineSymbol;
                        if (lineSymbol != null)
                        {
                            lineSymbol.Color = null;
                        }
                    }
                }
            }            
        }

        [DataMember(EmitDefaultValue = false)]
        public Symbol Symbol { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string SymbolJson { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, object> Attributes { get; set; }

        [DataMember(EmitDefaultValue=false)]
        public Geometry Geometry { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool Selected { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public TimeExtent TimeExtent { get; set; }

        public Graphic ToGraphic()
        {
            Graphic graphic = new Graphic();
            graphic.Geometry = Geometry;
            if (!string.IsNullOrEmpty(SymbolJson))
                graphic.Symbol = ESRI.ArcGIS.Client.Symbols.Symbol.FromJson(SymbolJson);
            else
                graphic.Symbol = Symbol;
            graphic.Selected = Selected;
            graphic.TimeExtent = TimeExtent;
            if (Attributes != null)
            {
                foreach (KeyValuePair<string, object> pair in Attributes)
                {
                    string key = pair.Key;
                    string attributeValue = pair.Value as string;
                    if (attributeValue != null)
                    {
                        // String values which were previosly HtmlEncoded are now HtmlDecoded
                        // Replace &amp; with &
                        // Replace &lt; with <
                        // Replace &gt; with >
                        graphic.Attributes.Add(key, System.Windows.Browser.HttpUtility.HtmlDecode(attributeValue));
                    }
                    else
                    {
                        graphic.Attributes.Add(pair);
                    }
                }
            }
            return graphic;
        }


    }
}
