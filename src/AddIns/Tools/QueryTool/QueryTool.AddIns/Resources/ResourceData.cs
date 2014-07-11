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

namespace QueryTool.AddIns
{
	internal sealed class ResourceData
	{
		private static ResourceDictionary dictionary;
		public static ResourceDictionary Dictionary
		{
			get
			{
				if (dictionary == null)
				{
					dictionary = new ResourceDictionary();
					dictionary.MergedDictionaries.Add(LoadDictionary("/QueryTool.AddIns;component/Resources/SymbolResource.xaml"));					
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
