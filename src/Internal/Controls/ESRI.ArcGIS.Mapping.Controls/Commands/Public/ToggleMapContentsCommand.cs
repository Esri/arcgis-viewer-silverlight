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

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
	[DisplayName("ToggleMapContentsDisplayName")]
	[Category("CategoryMap")]
	[Description("ToggleMapContentsDescription")]
    public class ToggleMapContentsCommand : ToggleControlVisibilityCommand
    {        
        public ToggleMapContentsCommand() : base(ControlNames.SIDEPANELCONTAINER, ControlNames.MAPCONTENTSTABITEM)
        {
        }
    }
}
