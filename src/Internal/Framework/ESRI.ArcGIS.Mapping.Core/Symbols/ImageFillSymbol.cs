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
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client;
using System.Collections.ObjectModel;
using System.Linq;

namespace ESRI.ArcGIS.Mapping.Core.Symbols
{
	public class ImageFillSymbol : MarkerSymbol, IJsonSerializable
	{
		public ImageFillSymbol()
		{
			loadControlTemplate();
		}
		
		private void loadControlTemplate()
		{
			ControlTemplate = ResourceData.Dictionary["ImageFillSymbol"] as ControlTemplate;
		}
		
		private const string IMAGELOCATION = "/Images/MarkerSymbols/";

		/// <summary>
		/// Identifies the <see cref="Fill"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FillProperty =
						DependencyProperty.Register("Fill", typeof(Brush), typeof(ImageFillSymbol), new PropertyMetadata(OnFillChanged));
		/// <summary>
		/// Gets or sets Fill.
		/// </summary>
		public Brush Fill
		{
			get { return (Brush)GetValue(FillProperty); }
			set { SetValue(FillProperty, value); }
		}

		private static void OnFillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ImageFillSymbol dp = d as ImageFillSymbol;
			if (dp != null)
				dp.OnPropertyChanged("Fill");
		}

		/// <summary>
		/// Identifies the <see cref="CursorName"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty CursorNameProperty =
						DependencyProperty.Register("CursorName", typeof(string), typeof(ImageFillSymbol),
						new PropertyMetadata("Arrow"));
		/// <summary>
		/// Gets or sets Cursor by name.
		/// </summary>
		/// 		
		public string CursorName
		{
			get { return (string)GetValue(CursorNameProperty); }
			set { SetValue(CursorNameProperty, value); }
		}

		private string _source;
		public string Source
		{
			get
			{
				return _source;
			}
			set
			{
				if (_source != value)
				{
					_source = value;
					UpdateSymbolProperties();
				}
			}
		}

		private string contentType = "image/png";

		private void UpdateSymbolProperties()
		{
			BitmapImage bmi = ImageUrlResolver.ResolveUrlForImage(Source);
			if (Source != null)
			{
				var uriKey = ImageUrlResolver.ResolveUrl(Source);
				contentType = GetContentType(System.IO.Path.GetExtension(Source).TrimStart(new char[] { '.' }));
				var key = Source.Contains(IMAGELOCATION) ? Source.Replace(IMAGELOCATION, string.Empty).Replace("/", "\\") : uriKey.OriginalString;
				var imageSymbolEntry = SymbolConfigProvider.ImageSymbolLookup.FirstOrDefault(i => string.Equals(i.Source, key, StringComparison.OrdinalIgnoreCase));
				if (imageSymbolEntry != null)
					_data = imageSymbolEntry.ImageData;
				else if (uriKey.IsAbsoluteUri)
				{
					bmi.ImageOpened += (s, e) =>
					{
						var uriSource = (s as BitmapImage).UriSource;
						var webClient = new WebClient();
						webClient.OpenReadCompleted += (a, b) =>
						{
							var symbolKey = b.UserState as Uri;
							var length = (int)b.Result.Length;
							var contentInBytes = new byte[length];
							b.Result.Read(contentInBytes, 0, length);
							_data = System.Convert.ToBase64String(contentInBytes);
							SymbolConfigProvider.ImageSymbolLookup.Add(new ImageSymbolEntry(symbolKey.OriginalString, _data));
						};
						webClient.OpenReadAsync(uriSource, uriSource);
					};
				}
				else
				{					
					Uri contentUri = new Uri(uriKey.OriginalString.TrimStart(new char[] { '/' }), UriKind.Relative);
					System.Windows.Resources.StreamResourceInfo sri = Application.GetResourceStream(contentUri);					
					if (sri != null && sri.Stream != null)
					{
						int length = (int)sri.Stream.Length;
						byte[] contentInBytes = new byte[length];
						sri.Stream.Read(contentInBytes, 0, length);
						_data = System.Convert.ToBase64String(contentInBytes);
					}
				}
			}
			ImageBrush brush = new ImageBrush()
			{
				ImageSource = bmi
			};
			Fill = brush;
		}

		private static string GetContentType(string ext)
		{
			switch (ext.ToLower())
			{
				case "gif": return "image/gif";
				case "jpeg":
				case "jpg": return "image/jpeg";
				case "png": return "image/png";
				case "tif":
				case "tiff": return "image/tiff";
				default: return "image/png";
			}
		}

		private string _data;
		public string ImageData
		{
			get
			{
				return _data;
			}
			set
			{
				_data = value;
				if (!string.IsNullOrEmpty(_data))
				{
					byte[] contentInBytes =
									System.Convert.FromBase64String(_data);
					BitmapImage bmi = new BitmapImage();
					bmi.SetSource(new System.IO.MemoryStream(contentInBytes));
					Fill = new ImageBrush()
					{
						ImageSource = bmi,
					};
				}
			}
		}

		/// <summary>
		/// Serializes the ImageFillSymbol to JSON
		/// </summary>
		/// <returns>
		/// A JSON string representation of the ImageFillSymbol.
		/// </returns>
		public string ToJson()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			double size = Size;
			double xoffset = 0 - ((size * OriginX) - size / 2);
			double yoffset = (size * OriginY) - size / 2;
			sb.Append("{\"type\" : \"esriPMS\",");
			if (!string.IsNullOrEmpty(ImageData))
				sb.AppendFormat("\"imageData\" : \"{0}\",", ImageData);
			else if (!string.IsNullOrEmpty(Source))
				sb.AppendFormat("\"url\" : \"{0}\",", ImageUrlResolver.ResolveUrl(Source));
			sb.AppendFormat("\"width\" : {0}, \"height\" : {0}, \"xoffset\" : {1}, \"yoffset\" : {2}, \"contentType\" : \"{3}\"", size, xoffset, yoffset, contentType);
			sb.Append(" }");
			return sb.ToString();
		}
	}
}
