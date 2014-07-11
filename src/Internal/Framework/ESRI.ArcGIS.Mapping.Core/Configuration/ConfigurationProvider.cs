/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/


using System.Reflection;
using System.IO;
using System;
using System.Windows.Controls;
using System.Windows;
using ESRI.ArcGIS.Client;
namespace ESRI.ArcGIS.Mapping.Core
{
    public class ConfigurationProvider 
    {
        private string getDefaultConfigurationFromEmbeddedResource()
        {
            string defaultConfigurationXml = string.Empty;
            Assembly a = typeof(ConfigurationProvider).Assembly;
            using (Stream str = a.GetManifestResourceStream("ESRI.ArcGIS.Mapping.Core.Embedded.DefaultConfiguration.xml"))
            {
                using (StreamReader rdr = new StreamReader(str))
                {
                    defaultConfigurationXml = rdr.ReadToEnd();
                }
            }
            return defaultConfigurationXml;
        }

        public virtual void GetConfigurationAsync(object userState, EventHandler<GetConfigurationCompletedEventArgs> onCompleted, EventHandler<ExceptionEventArgs> onFailed)
        {
            string xaml = getDefaultConfigurationFromEmbeddedResource();
            if (string.IsNullOrEmpty(xaml))
            {
                if(onFailed != null)
                    onFailed(this, new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionConfigurationFileEmpty), userState));
            }
            else
            {

                object parsedObject = null;
                try
                {
                    parsedObject = System.Windows.Markup.XamlReader.Load(xaml);
                    if(onCompleted != null)
                        onCompleted(this, new GetConfigurationCompletedEventArgs() { Map = parsedObject as Map, UserState = userState });
                }
                catch (Exception ex)
                {
                    if(onFailed != null)
                        onFailed(this, new ExceptionEventArgs(ex));
                }
            }
        }
    }
}
