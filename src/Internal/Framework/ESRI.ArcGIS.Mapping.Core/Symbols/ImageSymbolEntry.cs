/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Mapping.Core
{
	[DataContract]
	public class ImageSymbolEntry
	{

		public ImageSymbolEntry(string source, string imageData)
		{
			Source = source;
			ImageData = imageData;
		}

		[DataMember]
		public string Source { get; set; }
		[DataMember]
		public string ImageData { get; set; }
	}
}
