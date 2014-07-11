/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Text;
using ESRI.ArcGIS.Client.Tasks;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.GP.ParameterSupport
{
    public class MultiValueStringConfig : ParameterConfig
    {
        public static readonly string HtmlComma = "&#44;";

        protected override void AddToJsonDictionary(ref Dictionary<string, object> dictionary)
        {
            base.AddToJsonDictionary(ref dictionary);

            var val = DefaultValue as GPMultiValue<GPString>;
            if (val != null)
            {
                var sb = new StringBuilder();
                bool first = true;
                foreach (GPString gpString in val.Value)
                {
                    if (first)
                        first = false;
                    else
                        sb.Append(",");

                    // encode any embedded commas
                    //string delimited = gpString.Value.Replace(",", HtmlComma);
                    sb.Append(gpString.Value);
                }
                dictionary.Add("defaultValue", sb.ToString());
            }
        }
    }
}
