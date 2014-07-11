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
using System.ComponentModel;
using System.Collections.Generic;
using System.Xml;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows.Markup;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Core
{
     [DataContract]
    public class LayerInformation : INotifyPropertyChanged
    {
         public LayerInformation()
         {
             fields = new Collection<FieldInfo>();
         }
        public event PropertyChangedEventHandler PropertyChanged;

        void onPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        private string name;

        [DataMember]
        public string Name
        {
            get { return name; }
            set { name = value; onPropertyChanged("Name"); }
        }

        private int id;

        [DataMember]
        public int ID
        {
            get { return id; }
            set { id = value; onPropertyChanged("ID"); }
        }

        private bool enabled;

        [DataMember]
        public bool PopUpsEnabled
        {
            get { return enabled; }
            set { enabled = value; onPropertyChanged("PopUpsEnabled"); }
        }

        private Collection<FieldInfo> fields;

        [DataMember]
        public Collection<FieldInfo> Fields
        {
            get { return fields; }
            set
            {
                fields = value;
                onPropertyChanged("Fields");
                if (fields != null)
                {
                    foreach (FieldInfo field in fields)
                    {
                        field.PropertyChanged += field_PropertyChanged;
                    }
                }
            }
        }

        void field_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            onPropertyChanged("Fields");
        }

        private string displayField;

        [DataMember]
        public string DisplayField
        {
            get { return displayField; }
            set { displayField = value; onPropertyChanged("DisplayField"); }
        }

		private string layerJson;

		[DataMember(EmitDefaultValue = false)]
		public string LayerJson
		{
			get { return layerJson; }
			set { layerJson = value; flayer = null; onPropertyChanged("LayerJson"); }
		}
		FeatureLayer flayer;
        public FeatureLayer FeatureLayer
        {
            get
            {
                if (flayer == null && !string.IsNullOrEmpty(LayerJson))
                {
                    flayer = FeatureLayer.FromJson(LayerJson);
                }
                return flayer;
            }
            set { flayer = value; }
        }

        public static void WriteLayerInfos(IEnumerable<LayerInformation> layers, XmlWriter writer)
        {
            foreach (LayerInformation layer in layers)
            {
                writer.WriteStartElement(Constants.esriMappingPrefix, "LayerInformation", Constants.esriMappingNamespace);
                if (!string.IsNullOrEmpty(layer.Name))
                    writer.WriteAttributeString("Name", layer.Name);
                if (!string.IsNullOrEmpty(layer.DisplayField))
                    writer.WriteAttributeString("DisplayField", layer.DisplayField);
                writer.WriteAttributeString("ID", layer.ID.ToString());
                if (layer.PopUpsEnabled == false)
                    writer.WriteAttributeString("PopUpsEnabled", "false");
                else
                    writer.WriteAttributeString("PopUpsEnabled", "true");
				if (layer.Fields != null)
                {
                    writer.WriteStartElement(Constants.esriMappingPrefix, "LayerInformation.Fields", Constants.esriMappingNamespace);
                    FieldInfo.WriteFieldInfos(layer.Fields, writer);
                    writer.WriteEndElement();
                }
				if (!string.IsNullOrEmpty(layer.LayerJson))
					writer.WriteElementString(Constants.esriMappingPrefix, "LayerInformation.LayerJson", Constants.esriMappingNamespace, layer.LayerJson);
				writer.WriteEndElement();
            }
        }
    }
}
