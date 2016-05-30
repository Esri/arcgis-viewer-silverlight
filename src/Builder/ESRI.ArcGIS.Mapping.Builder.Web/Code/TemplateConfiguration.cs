/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.IO;

using System.Xml.Serialization;
using ESRI.ArcGIS.Mapping.Builder.Common;


namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    internal abstract class TemplateConfiguration
    {
        private const string TEMPLATECONFIGPATH = "~/App_Data/Templates.xml";
        static object lockObject = new object();
        private static string TemplateConfigPath
        {
            get
            {                
                return ServerUtility.MapPath(TEMPLATECONFIGPATH);
            }
        }        

        public static Template FindTemplateById(string templateId)
        {
            foreach (Template template in InstalledTemplates)
            {
                if (string.Compare(template.ID, templateId, true) == 0)
                    return template;
            }
            return null;
        }        

        private static Templates InstalledTemplates
        {
            get
            {
                Templates _templates;
                string configPath = TemplateConfigPath;
                if (File.Exists(configPath))
                {
                    lock (lockObject)
                    {
                        try
                        {
                            XmlSerializer reader = new XmlSerializer(typeof(Templates));
                            using (System.IO.StreamReader file = new System.IO.StreamReader(configPath))
                            {
                                Templates templates = (Templates)reader.Deserialize(file);
                                _templates = templates;
                            }
                        }
                        catch (Exception)
                        {
                            // TODO:- log error
                            _templates = new Templates();
                        }
                    }
                }
                else
                {
                    _templates = new Templates();
                }                
                return _templates;
            }
        }

        public static Templates GetTemplates()
        {            
            return InstalledTemplates;
        }
    }    
}

