/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;
using System.Windows;
using System;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
    [DisplayName("ToggleEditDisplayName")]
    [Category("CategoryEditing")]
    [Description("ToggleEditDescription")]
    public class ToggleEditCommand : ToggleControlVisibilityCommand
    {
        public ToggleEditCommand()
            : base(ControlNames.SIDEPANELCONTAINER, ControlNames.EDITTABITEM)
        { 
        }

        public override void Execute(object parameter)
        {
            try
            {
                base.Execute(parameter);
            }
            catch
            {
                throw new Exception(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ToggleEditFailed);
            }
        }
    }
}
