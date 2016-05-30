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
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls.Resources;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class LinkWebMapCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

#pragma warning disable 0067
		public event EventHandler CanExecuteChanged;
#pragma warning restore 0067

		public void Execute(object parameter)
        {
            Map map = parameter as Map;
            if (map == null)
                map = MapApplication.Current.Map;
            if (map != null)
            {
                MessageBoxDialog.Show(Strings.LinkWebMapDialogMessage, Strings.LinkWebMapDialogCaption, 
                    MessageType.Warning,
                    MessageBoxButton.OKCancel, (o, e) =>
                        {
                            if (e.Result == MessageBoxResult.OK)
                            {
                                View.Instance.LoadWebMap(ViewerApplication.WebMapSettings.ID,
                                    ViewerApplication.WebMapSettings.Document);
                            }
                        });
            }
        }
    }
}
