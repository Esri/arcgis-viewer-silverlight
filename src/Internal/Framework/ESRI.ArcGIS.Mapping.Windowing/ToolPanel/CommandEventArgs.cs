/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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
