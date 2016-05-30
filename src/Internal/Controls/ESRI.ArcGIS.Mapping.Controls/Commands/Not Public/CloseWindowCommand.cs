/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Toolkit;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class CloseWindowCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            if (parameter is Control)
            {
                MapApplication.Current.HideWindow(parameter as Control);
                if (parameter is IDisposable)
                    ((IDisposable)parameter).Dispose();
            }
        }
    }
}
