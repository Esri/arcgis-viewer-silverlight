/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.Xml;

namespace Localizer
{
    public class LocalizedFiles
    {
        public LocalizedFiles()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            System.IO.Stream stream;
            DataContractSerializer dcs;
            XmlDictionaryReader xdr;

            stream = asm.GetManifestResourceStream(
           "Localizer.Resources.FilesToBeLocalized.xml");
            dcs = new DataContractSerializer(typeof(BuilderLocalizationInfo));
            xdr = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas());
            BuilderInfo = (BuilderLocalizationInfo)dcs.ReadObject(xdr);
        }

        public BuilderLocalizationInfo BuilderInfo { get; private set; }
    }

    [DataContract]
    public class BuilderLocalizationInfo
    {
        [DataMember(Name = "BuilderFiles", Order=0)]
        public LocalizedFileList BuilderFiles { get; set; }
        [DataMember(Name = "Templates", Order = 1)]
        public TemplateList Templates { get; set; }
    }

     [CollectionDataContract(ItemName = "Template")]
    public class TemplateList : List<Template> { }

    [DataContract]
    public class Template
    {
        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }
        [DataMember(Name = "XmlTemplateFiles", Order = 1)]
        public LocalizedFileList XmlTemplateFiles { get; set; }
        [DataMember(Name = "TextTemplateFiles", Order = 2)]
        public LocalizedFileList TextTemplateFiles { get; set; }

    }

   [CollectionDataContract(ItemName = "LocalizedFile")]
    public class LocalizedFileList : List<string> { }


}
