/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Mapping.Controls;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class AddToolCommand : CommandBase
    {
        public ButtonDisplayInfo DisplayInfo { get; set; }
        public Type ToolType { get; set; }
        public ToolPanel ToolPanel { get; set; }
        public object ToolInstance { get; set; }

        public override void Execute(object parameter)
        {
            object instance = ToolInstance ?? Activator.CreateInstance(ToolType);
            ICommand command = instance as ICommand;
            if (command != null)
            {
                ToolPanel.AddToolButton(DisplayInfo, command, ToolPanel.ToolPanelItems);
            }
            else
            {
                FrameworkElement frameworkElement = instance as FrameworkElement;
                if (frameworkElement != null)
                {
                    DropDownButton galleryButton = ToolPanel.AddToolGroupButton(DisplayInfo, null, ToolPanel.ToolPanelItems) as DropDownButton;
                    if (galleryButton != null)
                    {
                        Panel sp = galleryButton.PopupContent as Panel;
                        if (sp != null)
                            sp.Children.Add(frameworkElement);
                    }
                }
            }
        }

        public override bool CanExecute(object parameter)
        {
            return DisplayInfo != null && ToolType != null && ToolPanel != null;
        }
    }
}
