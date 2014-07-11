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
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Builder.Controls;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class ShowAddToolDialogCommand : CommandBase
    {
        ConfigureToolPanelItemCommand configureToolCommand;
        bool configuring = false;

        public override bool CanExecute(object parameter)
        {
            return !configuring;
        }

        public override void Execute(object parameter)
        {
            if (configureToolCommand == null)
            {
                configureToolCommand = new ConfigureToolPanelItemCommand()
                {
                    DialogTitle = ESRI.ArcGIS.Mapping.Builder.Resources.Strings.AddTool,
                    AllowContainerSelection = true,
                    AllowToolSelection = true
                };

                configureToolCommand.Completed += Configuration_Completed;
                configureToolCommand.Cancelled += (o, e) => { ToggleExecutableState(); };
            }

            ToggleExecutableState();  
            configureToolCommand.Execute(null);
        }

        private void Configuration_Completed(object sender, EventArgs e)
        {
            AddToolCommand addTool = new AddToolCommand()
            {
                ToolPanel = configureToolCommand.ToolPanel,
                ToolType = configureToolCommand.Class.GetType(),
                ToolInstance = configureToolCommand.Class,
                DisplayInfo = configureToolCommand.DisplayInfo
            };
            addTool.Execute(null);

            ToggleExecutableState();
        }

        private void ToggleExecutableState()
        {
            configuring = !configuring;
            OnCanExecuteChanged(EventArgs.Empty);
        }
    }
}
