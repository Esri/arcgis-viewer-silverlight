/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows.Input;

namespace ESRI.ArcGIS.Client.Application.Controls
{
    public class CommandEventArgs : EventArgs
    {
        public ICommand Command { get; set; }
    }
}
