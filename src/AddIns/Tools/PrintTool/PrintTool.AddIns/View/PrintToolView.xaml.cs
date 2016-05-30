/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows.Controls;

namespace PrintTool.AddIns
{
	/// <summary>
	/// Print tool enables user to select between two modes of printing: 
	/// 1) Using out-of-the-box High-Quality printing support from ArcGIS 10.1 Server.
	/// 2) Using printing capabilities enabled from Silverlight application.
	/// </summary>
	public partial class PrintToolView : UserControl
	{
		public PrintToolView()
		{
			InitializeComponent();
		}
	}
}
