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
	internal class ResourceData
	{
		private static ResourceDictionary dictionary;
		public static ResourceDictionary Dictionary
		{
			get
			{
				if (dictionary == null)
				{
					dictionary = new ResourceDictionary();
                    dictionary.MergedDictionaries.Add(LoadDictionary("/ESRI.ArcGIS.Mapping.Core;component/Symbols/Symbols.xaml"));
				}
				return dictionary;
			}
		}
		private static ResourceDictionary LoadDictionary(string key)
		{
			return new ResourceDictionary() { Source = new Uri(key, UriKind.Relative) };
		}
	}
}
