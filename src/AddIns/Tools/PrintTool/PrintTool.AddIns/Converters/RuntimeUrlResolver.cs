/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client.Extensibility;

namespace PrintTool.AddIns
{
	/// <summary>
	/// Resolves URL string to BitmapImage source.
	/// </summary>
	public class RuntimeUrlResolver : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string urlToBeResolved = value as string;
			if (!string.IsNullOrWhiteSpace(urlToBeResolved))
			{
				return new BitmapImage(new Uri(new Uri(MapApplication.Current.Urls.BaseUrl), urlToBeResolved));
			}			
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
