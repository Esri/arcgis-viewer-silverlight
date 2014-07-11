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
using ESRI.ArcGIS.Mapping.GP;
using ESRI.ArcGIS.Client.Application.Controls;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class AddGPCommand : CommandBase
    {
        ConfigureToolPanelItemCommand configureToolCommand;
        Type gpCommandType = typeof(GeoprocessingCommand);
        AddToolbarItem gpItem = ToolbarManagement.CreateToolbarItemForType(typeof(GeoprocessingCommand));

        public override void Execute(object parameter)
        {
            if (configureToolCommand == null)
            {
                configureToolCommand = new ConfigureToolPanelItemCommand()
                    {
                        DialogTitle = ESRI.ArcGIS.Mapping.Builder.Resources.Strings.AddGeoprocessing,
                        AllowContainerSelection = true
                    };

                configureToolCommand.Completed += Configuration_Completed;
				configureToolCommand.Cancelled += new EventHandler<EventArgs>(configureToolCommand_Cancelled);

				configureToolCommand.DisplayInfo = new ButtonDisplayInfo()
					{
						Label = gpItem.Name,
						Description = gpItem.Description,
						Icon = ToolbarManagement.GetDefaultIconUrl(gpCommandType)
					};
				configureToolCommand.Execute(new GeoprocessingCommand());
			}
		}

		void configureToolCommand_Cancelled(object sender, EventArgs e)
		{
			configureToolCommand = null;			
		}

        private void Configuration_Completed(object sender, EventArgs e)
        {
            AddToolCommand addGP = new AddToolCommand()
            {
                ToolPanel = configureToolCommand.ToolPanel,
                ToolType = gpCommandType,
                ToolInstance = configureToolCommand.Class,
                DisplayInfo = configureToolCommand.DisplayInfo
            };
            addGP.Execute(null);

			configureToolCommand = null;
        }
    }
}
