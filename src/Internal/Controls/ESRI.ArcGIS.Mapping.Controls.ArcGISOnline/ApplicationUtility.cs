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
using System.Windows.Resources;
using System.IO;
using System.Windows.Markup;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
	internal class ApplicationUtility
	{
		static System.Windows.Threading.Dispatcher dispatcher;
		public static System.Windows.Threading.Dispatcher Dispatcher
		{
			get
			{
				if (dispatcher == null)
				{
					if (RootVisual != null)
						dispatcher = RootVisual.Dispatcher;
				}
				return dispatcher;
			}
			set
			{
				dispatcher = value;
			}
		}

		static UIElement rootVisual;
		public static UIElement RootVisual
		{
			get
			{
				if (rootVisual == null)
				{
					rootVisual = Application.Current.RootVisual;
				}
				return rootVisual;
			}
			set
			{
				rootVisual = value;
			}
		}

		public static FrameworkElement RootVisualAsFrameworkElement
		{
			get { return RootVisual as FrameworkElement; }
		}

	}
}
