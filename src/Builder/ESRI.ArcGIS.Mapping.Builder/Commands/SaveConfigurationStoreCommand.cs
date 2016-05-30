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
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class SaveConfigurationStoreCommand : CommandBase
    {

        public override void Execute(object parameter)
        {
            ApplicationBuilder.ApplicationBuilderClient client = WCFProxyFactory.CreateApplicationBuilderProxy();
            client.SaveConfigurationStoreXmlCompleted += new EventHandler<ApplicationBuilder.SaveConfigurationStoreXmlCompletedEventArgs>(client_SaveConfigurationStoreXmlCompleted);
            client.SaveConfigurationStoreXmlAsync(XmlSerializer.Serialize<ConfigurationStore>(BuilderApplication.Instance.ConfigurationStore));
        }

        void client_SaveConfigurationStoreXmlCompleted(object sender, ApplicationBuilder.SaveConfigurationStoreXmlCompletedEventArgs e)
        {
            MessageBoxDialog.Show(Resources.Strings.ChangesSavedSuccessfully);
        }
    }
}
