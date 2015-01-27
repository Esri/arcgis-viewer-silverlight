/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.IO;
using System.Text;
using System.Web;
using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.Builder.Common;
using System.Xml;
using Resources;

namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    public class ApplicationBuilderHelper
    {
        private const string APPLICATIONCONFIGFILE = "Config/Application.xml";
        private const string MAPCONFIGFILE = "Config/Map.xml";
        private const string TOOLSCONFIGFILE = "Config/Tools.xml";
        private const string CONTROLSCONFIGFILE = "Config/Controls.xml";
        private const string BEHAVIORSCONFIGFILE = "Config/Behaviors.xml";
        private const string COLORSCONFIGFILE = "Config/Layouts/ResourceDictionaries/Common/Colors.xaml";

        private const string TemplateFolderPath = "~/Templates";
        private const string ExtensionsFolderPath = "~/Extensions";

        public bool CreateSiteFromTemplate(string templateId, Site targetSite, bool overwriteFiles, out FaultContract Fault)
        {
            Fault = null;
            string status = string.Empty;

            if (ValidatePhysicalPathAndCreateDirectory(targetSite, ref Fault) == false)
                return false;

            Template template = TemplateConfiguration.FindTemplateById(templateId);
            if (template == null)
            {
                Fault = new FaultContract();
                Fault.FaultType = "Error";
                Fault.Message = "Unable to find specified template " + templateId;
                return false;
            }
            string templatePhysicalPath = ServerUtility.MapPath(TemplateFolderPath) + "\\" + template.ID;

            if (!copyWebsiteContentFiles(templatePhysicalPath, targetSite.PhysicalPath, overwriteFiles, out status))
            {
                Fault = new FaultContract();
                Fault.FaultType = "Error";
                Fault.Message = "Unable to copy website files " + status;
                return false;
            }
            
            return true;
        }

        public bool CopySite(string sourcePhysicalPath, Site targetSite, bool overwriteFiles, out FaultContract Fault)
        {
            Fault = null;
            string status = string.Empty;

            if (ValidatePhysicalPathAndCreateDirectory(targetSite, ref Fault) == false)
                return false;

            if (!copyWebsiteContentFiles(sourcePhysicalPath, targetSite.PhysicalPath, overwriteFiles, out status))
            {
                Fault = new FaultContract();
                Fault.FaultType = "Error";
                Fault.Message = "Unable to copy website files " + status;
                return false;
            }

            return true;
        }

        private static bool ValidatePhysicalPathAndCreateDirectory(Site targetSite, ref FaultContract Fault)
        {
            if (targetSite.IsHostedOnIIS)
            {
                if (string.IsNullOrEmpty(targetSite.PhysicalPath))
                    targetSite.PhysicalPath = (new IISManagementHelper()).GetPhysicalPathForSite(targetSite, true, out Fault);
            }
            else
            {
                if (string.IsNullOrEmpty(targetSite.PhysicalPath))
                {
                    Fault = new FaultContract();
                    Fault.FaultType = "Error";
                    Fault.Message = "New site must specify a physical path for non IIS sites";
                    return false;
                }
            }

            if (!Directory.Exists(targetSite.PhysicalPath))
                Directory.CreateDirectory(targetSite.PhysicalPath);

            return true;
        }

        public bool IsSiteEmpty(string templateId, Site newSite, out FaultContract Fault)
        {
            Fault = null;
            string status = string.Empty;
            if (newSite.IsHostedOnIIS)
            {
                if (string.IsNullOrEmpty(newSite.PhysicalPath))
                    newSite.PhysicalPath = (new IISManagementHelper()).GetPhysicalPathForSite(newSite, true, out Fault);
            }
            else
            {
                if (string.IsNullOrEmpty(newSite.PhysicalPath))
                {
                    Fault = new FaultContract();
                    Fault.FaultType = "Error";
                    Fault.Message = "New site must specify a physical path";
                    return false;
                }
            }

            Template template = TemplateConfiguration.FindTemplateById(templateId);
            if (template == null)
            {
                Fault = new FaultContract();
                Fault.FaultType = "Error";
                Fault.Message = "Unable to find specified template " + templateId;
                return false;
            }

            return true;
        }

        public bool SaveConfigurationForSite(Site site, SitePublishInfo info, out FaultContract Fault, string newTitle = null)
        {
            Fault = null;

            if (!Directory.Exists(site.PhysicalPath))
            {
                Fault = new FaultContract();
                Fault.FaultType = "Error";
                Fault.Message = "Website physical path for site " + site.ID + " at " + site.PhysicalPath + " was not found";
                return false;
            }

            // Save Application.xml
            string path = APPLICATIONCONFIGFILE;
            path = path.Replace("/", "\\");
            string absolutePath = string.Format("{0}\\{1}", site.PhysicalPath, path);
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(absolutePath, FileMode.Create, FileAccess.Write), Encoding.UTF8))
                {
                    writer.Write(info.ApplicationXml);
                }
            }
            catch (Exception ex)
            {
                Fault = new FaultContract();
                Fault.FaultType = "Error";
                Fault.Message = "Error writing out XML " + ex.ToString();
                return false;
            }

            // Save Map.xml
            path = MAPCONFIGFILE;
            path = path.Replace("/", "\\");
            absolutePath = string.Format("{0}\\{1}", site.PhysicalPath, path);
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(absolutePath, FileMode.Create, FileAccess.Write), Encoding.UTF8))
                {
                    writer.Write(info.MapXaml);
                }
            }
            catch (Exception ex)
            {
                Fault = new FaultContract();
                Fault.FaultType = "Error";
                Fault.Message = "Error writing out XML to Map.xml " + ex.ToString();
                return false;
            }

            // Save Tools.xml
            path = TOOLSCONFIGFILE;
            path = path.Replace("/", "\\");
            absolutePath = string.Format("{0}\\{1}", site.PhysicalPath, path);
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(absolutePath, FileMode.Create, FileAccess.Write), Encoding.UTF8))
                {
                    writer.Write(info.ToolsXml);
                }
            }
            catch (Exception ex)
            {
                Fault = new FaultContract();
                Fault.FaultType = "Error";
                Fault.Message = "Error writing out XML to Tools.xml " + ex.ToString();
                return false;
            }

            // Save Behaviors.xml
            path = BEHAVIORSCONFIGFILE;
            path = path.Replace("/", "\\");
            absolutePath = string.Format("{0}\\{1}", site.PhysicalPath, path);
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(absolutePath, FileMode.Create, FileAccess.Write), Encoding.UTF8))
                {
                    writer.Write(info.BehaviorsXml);
                }
            }
            catch (Exception ex)
            {
                Fault = new FaultContract();
                Fault.FaultType = "Error";
                Fault.Message = "Error writing out XML to Behaviors.xml " + ex.ToString();
                return false;
            }

            // Save Colors.xaml
            path = COLORSCONFIGFILE;
            path = path.Replace("/", "\\");
            absolutePath = string.Format("{0}\\{1}", site.PhysicalPath, path);
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(absolutePath, FileMode.Create, FileAccess.Write), Encoding.UTF8))
                {
                    writer.Write(info.ColorsXaml);
                }
            }
            catch (Exception ex)
            {
                Fault = new FaultContract();
                Fault.FaultType = "Error";
                Fault.Message = "Error writing out XML to Colors.xaml " + ex.ToString();
                return false;
            }

            // Save Controls.xml
            path = CONTROLSCONFIGFILE;
            path = path.Replace("/", "\\");
            absolutePath = string.Format("{0}\\{1}", site.PhysicalPath, path);
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(absolutePath, FileMode.Create, FileAccess.Write), Encoding.UTF8))
                {
                    writer.Write(info.ControlsXml);
                }
            }
            catch (Exception ex)
            {
                Fault = new FaultContract();
                Fault.FaultType = "Error";
                Fault.Message = "Error writing out XML to Controls.xml " + ex.ToString();
                return false;
            }

            // Copy Extensions
            if (info.ExtensionsXapsInUse != null)
            {
                try
                {
                    // Copy extensions to the extension folder 
					string destDir = string.Format("{0}\\Extensions", site.PhysicalPath);
                    if (!Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);
                    foreach (string extensionXapName in info.ExtensionsXapsInUse)
                    {
                        string sourceFile = string.Format("{0}\\{1}.xap", ServerUtility.MapPath(ExtensionsFolderPath), extensionXapName);
                        if (File.Exists(sourceFile))
                        {
                            string destFile = string.Format("{0}\\{1}.xap", destDir, extensionXapName);
                            File.Copy(sourceFile, destFile, true);
                        }
                    }
                }
                catch (Exception exe)
                {
                    Fault = new FaultContract();
                    Fault.FaultType = "Error";
                    Fault.Message = "Unable to copy website files " + exe.Message;
                    return false;
                }
            }

            // Save a preview image
            PreviewImageManager.SavePreviewImageForSite(site.ID, info.PreviewImageBytes);

            // Apply app title to index.htm

            // Get path of home page
            path = Path.Combine(site.PhysicalPath, "index.htm");
            if (File.Exists(path)) // make sure it exists
            {
                var updateTitle = newTitle != null; // Check whether title needs to be updated or is being specified for the first time

                // Determine title to be replaced and title to replace it with
                var titleToReplace = updateTitle ? site.Title : Strings._ArcGISViewerForMicrosoftSilverlight_;
                var replacementTitle = updateTitle ? newTitle : site.Title;

                // HTML-encode new title
                replacementTitle = HttpUtility.HtmlEncode(replacementTitle); 

                // Format titles as HTML title elements
                var currentTitleElement = string.Format("<title>{0}</title>", titleToReplace);
                var newTitleElement = string.Format("<title>{0}</title>", replacementTitle);

                // Get HTML for home page 
                var html = File.ReadAllText(path);

                // Insert title
                html = html.Replace(currentTitleElement, newTitleElement);

                // Save home page
                File.WriteAllText(path, html);
            }

            return true;
        }

        public void DeleteSite(string siteId, out FaultContract Fault)
        {
            Fault = null;
            Site site = SiteConfiguration.FindExistingSiteByID(siteId);
            if (site == null)
            {
                Fault = new FaultContract();
                Fault.FaultType = "Error";
                Fault.Message = "Site not found " + siteId;
                return;
            }

            if (site.IsHostedOnIIS)
            {
                if (!(new IISManagementHelper()).DeleteSite(site, out Fault))
                    return;
            }

            if (Directory.Exists(site.PhysicalPath))
            {
                try
                {
                    Directory.Delete(site.PhysicalPath, true);
                }
                catch (Exception ex)
                {
                    Fault = new FaultContract();
                    Fault.FaultType = "Error";
                    Fault.Message = "Error Deleting site directory " + ex.ToString();
                    return;
                }
            }

            try
            {
                SiteConfiguration.DeleteSite(siteId);
            }
            catch (Exception exc)
            {
                Fault = new FaultContract();
                Fault.FaultType = "Error";
                Fault.Message = "Error Deleting site entry " + exc.ToString();
            }
        }

        #region Upgrade

        #region Files to copy on upgrade

        // Files to add/overwrite that are specific to upgrading to v3.0
        private static string[] m_changedFilesAt30 = new string[]
            {
                // Files to overwrite
                @"Viewer.xap",
                @"bin\ESRI.ArcGIS.Client.Application.Controls.dll",
                @"bin\ESRI.ArcGIS.Client.Application.Layout.dll",
                @"bin\ESRI.ArcGIS.Client.Extensibility.dll",
                @"Config\Layouts\ResourceDictionaries\Common\HoverPopupContainerStyle.xaml",
                @"Config\Layouts\ResourceDictionaries\Common\Shared_Resources.xaml",
                @"Config\Layouts\Accordion.xaml",
                @"Config\Layouts\Basic.xaml",
                @"Config\Layouts\BlackBox.xaml",
                @"Config\Layouts\BlackBox_-_Reverse.xaml",
                @"Config\Layouts\FloatingPanels.xaml",
                @"Config\Layouts\Glass.xaml",
                @"Config\Layouts\UnderGlow.xaml",
                @"Config\Layouts\Wings.xaml",
                @"Config\Print\PrintLayoutList.xml",
                @"Config\Symbols\BasicFillSymbols.json.txt",
                @"Config\Symbols\BasicSymbols.json.txt",
                @"Config\Symbols\ColorSymbols.json.txt",

                // Resource xaps for non-English apps
                @"Culture\Xaps\ar\Viewer.resources.xap",
                @"Culture\Xaps\de\Viewer.resources.xap",
                @"Culture\Xaps\es\Viewer.resources.xap",
                @"Culture\Xaps\fr\Viewer.resources.xap",
                @"Culture\Xaps\it\Viewer.resources.xap",
                @"Culture\Xaps\ja\Viewer.resources.xap",
                @"Culture\Xaps\pt-BR\Viewer.resources.xap",
                @"Culture\Xaps\ru\Viewer.resources.xap",
                @"Culture\Xaps\zh-CN\Viewer.resources.xap",

                // Files to add
                @"Config\Layouts\Layouts.xml",
                @"Images\MarkerSymbols\Basic\BlackPin1LargeB.png",
                @"Images\MarkerSymbols\Basic\BlackPin2LargeB.png",
                @"Images\MarkerSymbols\Basic\BlackTag.png",
                @"Images\MarkerSymbols\Basic\BluePin1LargeB.png",
                @"Images\MarkerSymbols\Basic\BluePin2LargeB.png",
                @"Images\MarkerSymbols\Basic\BrownTag.png",
                @"Images\MarkerSymbols\Basic\CyanTag.png",
                @"Images\MarkerSymbols\Basic\Gray1Tag.png",
                @"Images\MarkerSymbols\Basic\GreenPin1LargeB.png",
                @"Images\MarkerSymbols\Basic\GreenPin2LargeB.png",
                @"Images\MarkerSymbols\Basic\LimeTag.png",
                @"Images\MarkerSymbols\Basic\NavyTag.png",
                @"Images\MarkerSymbols\Basic\OliveTag.png",
                @"Images\MarkerSymbols\Basic\OrangePin1LargeB.png",
                @"Images\MarkerSymbols\Basic\OrangePin2LargeB.png",
                @"Images\MarkerSymbols\Basic\OrangeTag.png",
                @"Images\MarkerSymbols\Basic\PinkTag.png",
                @"Images\MarkerSymbols\Basic\PurplePin1LargeB.png",
                @"Images\MarkerSymbols\Basic\PurplePin2LargeB.png",
                @"Images\MarkerSymbols\Basic\RedPin1LargeB.png",
                @"Images\MarkerSymbols\Basic\RedPin2LargeB.png",
                @"Images\MarkerSymbols\Basic\RedTag.png",
                @"Images\MarkerSymbols\Basic\WhiteTag.png",
                @"Images\MarkerSymbols\Basic\YellowPin1LargeB.png",
                @"Images\MarkerSymbols\Basic\YellowPin2LargeB.png",
                @"Images\MarkerSymbols\Basic\YellowTag.png"            
            };

            // Files to add/overwrite that are specific to upgrading to v3.1
            private static string[] m_changedFilesAt31 = new string[]
            {
                // Main XAP
                @"Viewer.xap",

                // Resource xaps for non-English apps
                @"Culture\Xaps\ar\Viewer.resources.xap",
                @"Culture\Xaps\de\Viewer.resources.xap",
                @"Culture\Xaps\es\Viewer.resources.xap",
                @"Culture\Xaps\fr\Viewer.resources.xap",
                @"Culture\Xaps\it\Viewer.resources.xap",
                @"Culture\Xaps\ja\Viewer.resources.xap",
                @"Culture\Xaps\pt-BR\Viewer.resources.xap",
                @"Culture\Xaps\ru\Viewer.resources.xap",
                @"Culture\Xaps\zh-CN\Viewer.resources.xap"
            };

        // Files to add/overwrite that are specific to upgrading to 3.2
        private static string[] m_changedFilesAt32 = new string[]
            {
                // Main XAP
                @"Viewer.xap",

                // Shared XAML resources - auto-positioning of pop-up (InfoWindow) is defined here
                @"Config\Layouts\ResourceDictionaries\Common\Shared_Resources.xaml",

                // Resource xaps for non-English apps
                @"Culture\Xaps\ar\Viewer.resources.xap",
                @"Culture\Xaps\cs\Viewer.resources.xap",
                @"Culture\Xaps\da\Viewer.resources.xap",
                @"Culture\Xaps\de\Viewer.resources.xap",
                @"Culture\Xaps\es\Viewer.resources.xap",
                @"Culture\Xaps\et\Viewer.resources.xap",
                @"Culture\Xaps\fi\Viewer.resources.xap",
                @"Culture\Xaps\fr\Viewer.resources.xap",
                @"Culture\Xaps\he\Viewer.resources.xap",
                @"Culture\Xaps\it\Viewer.resources.xap",
                @"Culture\Xaps\ja\Viewer.resources.xap",
                @"Culture\Xaps\ko\Viewer.resources.xap",
                @"Culture\Xaps\lt\Viewer.resources.xap",
                @"Culture\Xaps\lv\Viewer.resources.xap",
                @"Culture\Xaps\nb-NO\Viewer.resources.xap",
                @"Culture\Xaps\nl\Viewer.resources.xap",
                @"Culture\Xaps\pl\Viewer.resources.xap",
                @"Culture\Xaps\pt-BR\Viewer.resources.xap",
                @"Culture\Xaps\pt-PT\Viewer.resources.xap",
                @"Culture\Xaps\ru\Viewer.resources.xap",
                @"Culture\Xaps\ro\Viewer.resources.xap",
                @"Culture\Xaps\zh-CN\Viewer.resources.xap"
            };

        // Files to add/overwrite that are specific to upgrading to 3.3
        private static string[] m_changedFilesAt33 = new string[]
            {
                // Main XAP
                @"Viewer.xap",

                // Resource xaps for non-English apps
                @"Culture\Xaps\ar\Viewer.resources.xap",
                @"Culture\Xaps\cs\Viewer.resources.xap",
                @"Culture\Xaps\da\Viewer.resources.xap",
                @"Culture\Xaps\de\Viewer.resources.xap",
                @"Culture\Xaps\es\Viewer.resources.xap",
                @"Culture\Xaps\et\Viewer.resources.xap",
                @"Culture\Xaps\fi\Viewer.resources.xap",
                @"Culture\Xaps\fr\Viewer.resources.xap",
                @"Culture\Xaps\he\Viewer.resources.xap",
                @"Culture\Xaps\it\Viewer.resources.xap",
                @"Culture\Xaps\ja\Viewer.resources.xap",
                @"Culture\Xaps\ko\Viewer.resources.xap",
                @"Culture\Xaps\lt\Viewer.resources.xap",
                @"Culture\Xaps\lv\Viewer.resources.xap",
                @"Culture\Xaps\nb-NO\Viewer.resources.xap",
                @"Culture\Xaps\nl\Viewer.resources.xap",
                @"Culture\Xaps\pl\Viewer.resources.xap",
                @"Culture\Xaps\pt-BR\Viewer.resources.xap",
                @"Culture\Xaps\pt-PT\Viewer.resources.xap",
                @"Culture\Xaps\ru\Viewer.resources.xap",
                @"Culture\Xaps\ro\Viewer.resources.xap",
                @"Culture\Xaps\zh-CN\Viewer.resources.xap"
            };

        #endregion

        #region Markup for upgrading the print tool
        private static string newPrintToolXml = @"<PrintToolViewModel xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/PrintTool.AddIns"">
            <Author i:nil=""true"" />
            <CopyrightText>Sources: USGS, FAO, NPS, EPA, ESRI, DeLorme, TANA, and other suppliers</CopyrightText>
            <Description>{0}</Description>
            <Format i:nil=""true"" />
            <Formats xmlns:d2p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"" i:nil=""true"" />
            <IsAuthorVisible>true</IsAuthorVisible>
            <IsCopyrightTextVisible>false</IsCopyrightTextVisible>
            <IsDescriptionVisible>false</IsDescriptionVisible>
            <IsFormatsVisible>true</IsFormatsVisible>
            <IsHeightVisible>false</IsHeightVisible>
            <IsLayoutTemplatesVisible>true</IsLayoutTemplatesVisible>
            <IsPrintLayoutVisible>false</IsPrintLayoutVisible>
            <IsServiceAsynchronous>false</IsServiceAsynchronous>
            <IsTitleVisible>false</IsTitleVisible>
            <IsUseScaleVisible>true</IsUseScaleVisible>
            <IsWidthVisible>false</IsWidthVisible>
            <LayoutTemplate i:nil=""true"" />
            <LayoutTemplates xmlns:d2p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"" i:nil=""true"" />
            <MapScaleString>0</MapScaleString>
            <PrintHeightString>{1}</PrintHeightString>
            <PrintLayout>
              {2}
            </PrintLayout>
            <PrintTaskURL>http://sampleserver6.arcgisonline.com/arcgis/rest/services/Utilities/PrintingTools/GPServer/Export%20Web%20Map%20Task</PrintTaskURL>
            <PrintWidthString>{3}</PrintWidthString>
            <PrintWithArcGISServer>false</PrintWithArcGISServer>
            <Title>{4}</Title>
            <UseProxy>false</UseProxy>
            <UseScale>false</UseScale>
          </PrintToolViewModel>";

        private static string basicPrintLayoutXml = 
            @"<Description>A basic print layout.</Description>
              <DisplayName>Basic</DisplayName>
              <ID>Basic</ID>";

        private static string mapElementsPrintLayoutXml = 
            @"<Description>A print layout with map elements like overview map.</Description>
              <DisplayName>With Map Elements</DisplayName>
              <ID>WithMapElements</ID>";
        #endregion

        /// <summary>
        /// Upgrades the specified site to the current version of the product
        /// </summary>
        internal static bool UpgradeSite(Site targetSite, string templateId, out FaultContract fault)
        {
            fault = null;

            try
            {
                // Get the template specified by the ID
                Template template = TemplateConfiguration.FindTemplateById(templateId);

                // Construct the path to the template and extensions folders on disk
                string templatePhysicalPath = ServerUtility.MapPath(TemplateFolderPath) + "\\" + template.ID;
                string extensionsPhysicalPath = ServerUtility.MapPath(ExtensionsFolderPath);

                bool upgradeFrom10 = false;
                bool upgradeFrom30 = false;
                bool upgradeFrom31 = false;
                bool upgradeFrom32 = false;
                string[] copyFiles = null;
                if (targetSite.ProductVersion == "3.2.0.0")
                {
                    copyFiles = m_changedFilesAt33;
                    upgradeFrom32 = true;
                }
                else if (targetSite.ProductVersion == "3.1.0.0")
                {
                    upgradeFrom31 = true;
                    copyFiles = m_changedFilesAt32; // Files changed at 3.2 is super-set of files changed at 3.3, so no need to join
                }
                else if (targetSite.ProductVersion == "3.0.0.0")
                {
                    upgradeFrom30 = true;
                    copyFiles = Join(m_changedFilesAt31, m_changedFilesAt32);
                }
                else
                {
                    upgradeFrom10 = true;
                    copyFiles = Join(Join(m_changedFilesAt30, m_changedFilesAt31), m_changedFilesAt32);
                }

                // Copy files that are new or updated at the current version
                foreach (string file in copyFiles)
                {
                    string sourceFilePath = Path.Combine(templatePhysicalPath, file);
                    if (File.Exists(sourceFilePath))
                    {
                        string destinationFilePath = Path.Combine(targetSite.PhysicalPath, file);
                        if (File.Exists(destinationFilePath)) // make backup
                            File.Copy(destinationFilePath, destinationFilePath + ".bak", true);
                        else if (!Directory.Exists(Path.GetDirectoryName(destinationFilePath)))
                            Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath));
                        File.Copy(sourceFilePath, destinationFilePath, true);
                    }
                }

                // list of add-ins - only upgrade if deployed to current site or will be added to site as part of upgrade
                string[] addins = { 
                    @"Extensions\Esri.ArcGIS.Client.Application.AddIns.CenterQueryStringBehavior.xap",
                    @"Extensions\Esri.ArcGIS.Client.Application.AddIns.ExtentQueryStringBehavior.xap",
                    @"Extensions\Esri.ArcGIS.Client.Application.AddIns.ScaleQueryStringBehavior.xap",
                    @"Extensions\Esri.ArcGIS.Client.Application.AddIns.ScaleLevelQueryStringBehavior.xap",
                    @"Extensions\Esri.ArcGIS.Client.Application.AddIns.WebMapQueryStringBehavior.xap",
                    @"Extensions\Bookmarks.AddIns.xap",
                    @"Extensions\MeasureTool.Addins.xap",
                    @"Extensions\PrintTool.AddIns.xap",
                    @"Extensions\QueryRelatedRecords.AddIns.xap",
                    @"Extensions\QueryTool.AddIns.xap",
                    @"Extensions\SearchTool.AddIns.xap" };

                // Update any optional out-of-the-box add-ins that were deployed to the app, or copy those that will be added as a result of upgrading
                foreach (var addin in addins)
                {
                    var addinPath = Path.Combine(extensionsPhysicalPath, addin).Replace("Extensions\\Extensions", "Extensions");
                    var destinationFilePath = Path.Combine(targetSite.PhysicalPath, addin);
                    if (File.Exists(addinPath))
                    {
                        if (File.Exists(destinationFilePath)    // Add-In already deployed
                            || ((upgradeFrom10 || upgradeFrom30 || upgradeFrom31) && addin.Contains("QueryStringBehavior")) // Query string add-ins will be deployed
                            || ((upgradeFrom10 || upgradeFrom30) && addin.Contains("QueryRelatedRecords"))) // Query related records add-in will be deployed
                        {
                            if (File.Exists(destinationFilePath))
                                File.Copy(destinationFilePath, destinationFilePath + ".bak", true); // back-up previous version
                            File.Copy(addinPath, destinationFilePath, true);
                        }
                    }
                }

                bool hasSearch = false;
                bool hasPrint = false;

                string toolsFilePath = Path.Combine(targetSite.PhysicalPath, @"Config\Tools.xml");
                string behaviorsFilePath = Path.Combine(targetSite.PhysicalPath, @"Config\Behaviors.xml");
                if (File.Exists(toolsFilePath) && File.Exists(behaviorsFilePath))
                {
                    File.Copy(toolsFilePath, toolsFilePath + ".bak", true); // make backup
                    bool toolsConfigChanged = false;

                    // backup behaviors too
                    File.Copy(behaviorsFilePath, behaviorsFilePath + ".bak", true);

                    // Get the config as a string
                    string toolsConfig = null;
                    using (StreamReader streamReader = new StreamReader(toolsFilePath))
                    {
                        toolsConfig = streamReader.ReadToEnd();
                        streamReader.Close();
                    }

                    // Get config as XmlDocument
                    XmlDocument doc = new XmlDocument();
                    doc.Load(toolsFilePath);

                    XmlDocument behaviorsDoc = new XmlDocument();
                    behaviorsDoc.Load(behaviorsFilePath);

                    if (upgradeFrom10)
                    {
                        // Update search and print tools in tools config file

                        #region Update Search Tool XML

                        // Check whether search tool is present and update accordingly
                        string oldSearchElement = "<esri:ToggleSearchCommand ";
                        if (toolsConfig != null && toolsConfig.Contains(oldSearchElement))
                        {
                            int toolsRootIndex = toolsConfig.IndexOf("<ToolPanels ");
                            int xmlnsInsertIndex = toolsConfig.IndexOf(">", toolsRootIndex);
                            toolsConfig = toolsConfig.Insert(xmlnsInsertIndex,
                                " xmlns:SearchTool=\"clr-namespace:SearchTool;assembly=SearchTool.AddIns\"");
                            toolsConfig = toolsConfig.Replace(oldSearchElement, "<SearchTool:SearchTool ");

                            File.WriteAllText(toolsFilePath, toolsConfig);
                            toolsConfigChanged = true;

                            hasSearch = true;
                        }

                        #endregion

                        #region Update Print Tool XML

                        // Update print tools
                        XmlNodeList printToolNodes = doc.GetElementsByTagName("esri:PrintCommand");
                        if (printToolNodes != null && printToolNodes.Count > 0)
                        {
                            // Iterate through each print tool in the config
                            foreach (XmlNode printToolNode in printToolNodes)
                            {
                                XmlNode toolNode = printToolNode.ParentNode.ParentNode;
                                string description = null;
                                string layout = null;
                                string title = null;
                                string height = null;
                                string width = null;
                                bool hasConfigData = false;
                                foreach (XmlNode child in toolNode.ChildNodes)
                                {
                                    // Find the tool's configuration
                                    if (child.LocalName == "Tool.ConfigData")
                                    {
                                        hasConfigData = true;

                                        XmlNode printConfigNode = child.FirstChild;

                                        // Read the configuration from the old version
                                        foreach (XmlNode propertyNode in printConfigNode.ChildNodes)
                                        {
                                            switch (propertyNode.LocalName.ToLower())
                                            {
                                                case "description":
                                                    description = propertyNode.InnerText;
                                                    break;
                                                case "layout":
                                                    layout = propertyNode.InnerText;
                                                    break;
                                                case "title":
                                                    title = propertyNode.InnerText;
                                                    break;
                                                case "printheight":
                                                    height = propertyNode.InnerText;
                                                    break;
                                                case "printwidth":
                                                    width = propertyNode.InnerText;
                                                    break;
                                            }
                                        }

                                        string printLayoutXml = layout != null && layout.EndsWith("WithMapElements.xaml") ?
                                            mapElementsPrintLayoutXml : basicPrintLayoutXml;

                                        // Update configuration to new version
                                        child.InnerXml = string.Format(newPrintToolXml, description, height,
                                            printLayoutXml, width, title);
                                    }
                                }

                                if (!hasConfigData)
                                {
                                    // Old version did not store any config data, so add default configuration
                                    description = "";
                                    title = "Untitled";
                                    height = "650";
                                    width = "650";

                                    XmlNode configNode = doc.CreateElement("Tool.ConfigData");
                                    configNode.InnerXml = string.Format(newPrintToolXml, description, height,
                                        basicPrintLayoutXml, width, title);

                                    toolNode.AppendChild(configNode);
                                }

                                doc.Save(toolsFilePath);

                                toolsConfigChanged = true;
                            }

                            // Get updated tool config as string
                            using (StreamReader streamReader = new StreamReader(toolsFilePath))
                            {
                                toolsConfig = streamReader.ReadToEnd();
                                streamReader.Close();
                            }

                            // Update print tool elements to new namespace and class name
                            toolsConfig = toolsConfig.Replace("<esri:PrintCommand ", "<PrintTool_AddIns:PrintTool ");

                            // Insert xml namespace for print add-in
                            int toolsRootIndex = toolsConfig.IndexOf("<ToolPanels ");
                            int xmlnsInsertIndex = toolsConfig.IndexOf(">", toolsRootIndex);
                            toolsConfig = toolsConfig.Insert(xmlnsInsertIndex,
                                " xmlns:PrintTool_AddIns=\"clr-namespace:PrintTool.AddIns;assembly=PrintTool.AddIns\"");

                            File.WriteAllText(toolsFilePath, toolsConfig);
                            toolsConfigChanged = true;

                            hasPrint = true;
                        }

                        #endregion
                    }

                    if (upgradeFrom10 || upgradeFrom30)
                    {
                        // Reload document to pick up previously saved changes
                        doc.Load(toolsFilePath);

                        // Add query related records tool to tools config file
                        if (toolsConfig != null && doc != null)
                        {
                            // Get all ToolPanel nodes
                            var nodes = doc.GetElementsByTagName("ToolPanel");
                            XmlNode popupToolbarNode = null;

                            // Find the ToolPanel corresponding to the pop-up toolbar
                            foreach (XmlNode xmlNode in nodes)
                            {
                                if (xmlNode.Attributes["ContainerName"].Value == "PopupToolbarContainer")
                                {
                                    popupToolbarNode = xmlNode;
                                    break;
                                }
                            }

                            //bool queryToolInserted = false;
                            if (popupToolbarNode != null)
                            {
                                // Get the Tools node for the pop-up toolbar
                                foreach (XmlNode childNode in popupToolbarNode.ChildNodes)
                                {
                                    if (childNode.LocalName == "Tools")
                                    {
                                        // Create the markup for the query related records tool
                                        var queryRelatedXml = 
@"      <Tool xmlns:QueryRelated=""clr-namespace:QueryRelatedRecords.AddIns;assembly=QueryRelatedRecords.AddIns"" 
            Label=""{0}"" Icon=""/QueryRelatedRecords.AddIns;component/images/GetRelatedRecords16.png"" 
            Description=""{1}"">
        <Tool.Class>
          <QueryRelated:QueryRelatedTool />
        </Tool.Class>
      </Tool>
";
                                        // Insert localized title and description into the markup
                                        queryRelatedXml = string.Format(queryRelatedXml, 
                                            Strings._QueryRelatedToolTitle_, 
                                            Strings._QueryRelatedToolDescription_);

                                        // Insert the query related tool into the tools node
                                        childNode.InnerXml = queryRelatedXml + childNode.InnerXml;

                                        doc.Save(toolsFilePath);

                                        //queryToolInserted = true;
                                        toolsConfigChanged = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (upgradeFrom10 || upgradeFrom30 || upgradeFrom31)
                    {
                        // Add behaviors for query string parameter support
                        var nodes = behaviorsDoc.GetElementsByTagName("Behaviors");

                        // Generate XML for Query String behaviors that are new at v3.2
                        var queryStringBehaviorsXml = @"
  <Behavior xmlns:QueryStringSupport=""clr-namespace:Esri.ArcGIS.Client.Application.AddIns;assembly=Esri.ArcGIS.Client.Application.AddIns.WebMapQueryStringBehavior"" IsEnabled=""True"" 
            Title=""{0}"">
    <Behavior.Class>
      <QueryStringSupport:GetWebMapFromQueryStringBehavior />
    </Behavior.Class>
  </Behavior>
  <Behavior xmlns:QueryStringSupport=""clr-namespace:Esri.ArcGIS.Client.Application.AddIns;assembly=Esri.ArcGIS.Client.Application.AddIns.ScaleQueryStringBehavior"" IsEnabled=""True"" 
            Title=""{1}"">
    <Behavior.Class>
      <QueryStringSupport:GetScaleFromQueryStringBehavior />
    </Behavior.Class>
  </Behavior>
  <Behavior xmlns:QueryStringSupport=""clr-namespace:Esri.ArcGIS.Client.Application.AddIns;assembly=Esri.ArcGIS.Client.Application.AddIns.ScaleLevelQueryStringBehavior"" IsEnabled=""True"" 
            Title=""{2}"">
    <Behavior.Class>
      <QueryStringSupport:GetScaleLevelFromQueryStringBehavior />
    </Behavior.Class>
  </Behavior>
  <Behavior xmlns:QueryStringSupport=""clr-namespace:Esri.ArcGIS.Client.Application.AddIns;assembly=Esri.ArcGIS.Client.Application.AddIns.ExtentQueryStringBehavior"" IsEnabled=""True"" 
            Title=""{3}"">
    <Behavior.Class>
      <QueryStringSupport:GetExtentFromQueryStringBehavior />
    </Behavior.Class>
  </Behavior>
  <Behavior xmlns:QueryStringSupport=""clr-namespace:Esri.ArcGIS.Client.Application.AddIns;assembly=Esri.ArcGIS.Client.Application.AddIns.CenterQueryStringBehavior"" IsEnabled=""True"" 
            Title=""{4}"">
    <Behavior.Class>
      <QueryStringSupport:GetCenterFromQueryStringBehavior />
    </Behavior.Class>
  </Behavior>
";
                        // Insert localized titles into the XML
                        queryStringBehaviorsXml = string.Format(queryStringBehaviorsXml, 
                            Strings._WebMapQueryStringBehaviorTitle_,
                            Strings._ScaleQueryStringBehaviorTitle_, 
                            Strings._ScaleLevelQueryStringBehaviorTitle_,
                            Strings._ExtentQueryStringBehaviorTitle_, 
                            Strings._CenterQueryStringBehaviorTitle_);

                        if (nodes.Count > 0)
                        {
                            // Insert the behaviors XML inside the Behaviors element
                            XmlNode behaviorsNode = nodes[0];
                            behaviorsNode.InnerXml = queryStringBehaviorsXml + behaviorsNode.InnerXml;
                        }

                        behaviorsDoc.Save(behaviorsFilePath);
                    }

                    if (!toolsConfigChanged)
                        File.Delete(toolsFilePath + ".bak"); // configuration was not changed, so remove backup
                }
                else
                {
                    fault = new FaultContract()
                    {
                        FaultType = "Error",
                        Message = "Tools configuration not found.  Upgrade cannot continue"
                    };
                    return false;
                }

                // Update XML namespaces for Map Contents and Editor at v3.2
                var controlsFilePath = Path.Combine(targetSite.PhysicalPath, @"Config\Controls.xml");
                if ((upgradeFrom10 || upgradeFrom30 || upgradeFrom31 || upgradeFrom32) && File.Exists(controlsFilePath))
                {
                    File.Copy(controlsFilePath, controlsFilePath + ".bak", true); // make backup

                    // Get the config as a string
                    var controlsConfig = "";
                    using (StreamReader streamReader = new StreamReader(controlsFilePath))
                    {
                        controlsConfig = streamReader.ReadToEnd();
                        streamReader.Close();
                    }

                    if (!string.IsNullOrEmpty(controlsConfig))
                    {
                        // Update Editor XML namespace
                        var oldEditorXmlns = "xmlns=\"http://schemas.datacontract.org/2004/07/ESRI.ArcGIS.Client.Application.Controls\"";
                        var newEditorXmlns = "xmlns=\"http://schemas.datacontract.org/2004/07/ESRI.ArcGIS.Mapping.Controls.Editor\"";
                        controlsConfig = controlsConfig.Replace(oldEditorXmlns, newEditorXmlns);

                        // Update Map Contents XML namespace
                        var oldMapContentsXmlns = "xmlns=\"http://schemas.datacontract.org/2004/07/ESRI.ArcGIS.Client.Application.Controls.MapContents\"";
                        var newMapContentsXmlns = "xmlns=\"http://schemas.datacontract.org/2004/07/ESRI.ArcGIS.Mapping.Controls.MapContents\"";
                        controlsConfig = controlsConfig.Replace(oldMapContentsXmlns, newMapContentsXmlns);

                        // Save updated configuration
                        File.WriteAllText(controlsFilePath, controlsConfig);
                    }
                }

                // Update version and add-ins in app config
                string appFilePath = Path.Combine(targetSite.PhysicalPath, @"Config\Application.xml");
                if (File.Exists(appFilePath))
                {
                    #region Update Version

                    XmlDocument doc = new XmlDocument();
                    doc.Load(appFilePath);
                    XmlNodeList versionNodes = doc.GetElementsByTagName("Version");
                    if (versionNodes != null && versionNodes.Count == 1)
                    {
                        Version executingVersion =
                            System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

                        string oldVersionString = versionNodes[0].InnerText;
                        if (!string.IsNullOrEmpty(oldVersionString))
                        {
                            Version oldVersion = new Version(oldVersionString);
                            versionNodes[0].InnerText = string.Format("{0}.{1}.{2}.{3}", executingVersion.Major,
                                executingVersion.Minor, executingVersion.Build, oldVersion.Revision);
                        }
                        else
                        {
                            versionNodes[0].InnerText = string.Format("{0}.{1}.{2}.0", executingVersion.Major,
                                executingVersion.Minor, executingVersion.Build);
                        }
                    }

                    #endregion

                    if (upgradeFrom10)
                    {
                        #region Add Print and Search Add-Ins

                        XmlNodeList extensionsNodes = doc.GetElementsByTagName("Extensions");
                        if (extensionsNodes != null && extensionsNodes.Count == 1)
                        {
                            XmlNode extensionNode = extensionsNodes[0];

                            // Add Extension element for search
                            if (hasSearch)
                            {
                                XmlElement searchToolElement = doc.CreateElement("Extension");
                                searchToolElement.SetAttribute("Url", "Extensions/SearchTool.AddIns.xap");
                                extensionNode.AppendChild(searchToolElement);
                            }

                            // Add Extension element for print
                            if (hasPrint)
                            {
                                XmlElement printToolElement = doc.CreateElement("Extension");
                                printToolElement.SetAttribute("Url", "Extensions/PrintTool.AddIns.xap");
                                extensionNode.AppendChild(printToolElement);
                            }
                        }
                        else if (hasSearch || hasPrint)
                        {
                            fault = new FaultContract()
                            {
                                FaultType = "Error",
                                Message = "Application contains tools that need to be upgraded, but configuration does " +
                                    "not contain Extensions element.  Application cannot be upgraded."
                            };
                            return false;
                        }

                        #endregion
                    }

                    if (upgradeFrom10 || upgradeFrom30)
                    {
                        #region Add Query Related Records Add-In

                        XmlNodeList extensionsNodes = doc.GetElementsByTagName("Extensions");
                        if (extensionsNodes != null && extensionsNodes.Count == 1)
                        {
                            XmlNode extensionNode = extensionsNodes[0];

                            XmlElement searchToolElement = doc.CreateElement("Extension");
                            searchToolElement.SetAttribute("Url", "Extensions/QueryRelatedRecords.AddIns.xap");
                            extensionNode.AppendChild(searchToolElement);                            
                        }
                        #endregion
                    }

                    if (upgradeFrom10 || upgradeFrom30 || upgradeFrom31)
                    {
                        #region Add Query String Add-Ins

                        XmlNodeList extensionsNodes = doc.GetElementsByTagName("Extensions");
                        if (extensionsNodes != null && extensionsNodes.Count == 1)
                        {
                            XmlNode extensionNode = extensionsNodes[0];

                            var queryStringAddIns = new string[]
                            {
                                "Extensions/Esri.ArcGIS.Client.Application.AddIns.WebMapQueryStringBehavior.xap",
                                "Extensions/Esri.ArcGIS.Client.Application.AddIns.ScaleQueryStringBehavior.xap",
                                "Extensions/Esri.ArcGIS.Client.Application.AddIns.ScaleLevelQueryStringBehavior.xap",
                                "Extensions/Esri.ArcGIS.Client.Application.AddIns.ExtentQueryStringBehavior.xap",
                                "Extensions/Esri.ArcGIS.Client.Application.AddIns.CenterQueryStringBehavior.xap"
                            };

                            foreach (var addInPath in queryStringAddIns)
                            {
                                XmlElement searchToolElement = doc.CreateElement("Extension");
                                searchToolElement.SetAttribute("Url", addInPath);
                                extensionNode.AppendChild(searchToolElement);
                            }
                        }
                        #endregion
                    }

                    File.Copy(appFilePath, appFilePath + ".bak"); // make backup
                    doc.Save(appFilePath);
                }
                else
                {
                    fault = new FaultContract()
                    {
                        FaultType = "Error",
                        Message = "Application configuration not found.  Upgrade cannot continue"
                    };
                    return false;
                }

                if (upgradeFrom10)
                {
                    #region Update Map Configuration for Changes between 2.4 and 3.0 API

                    // Replace references to ESRI.ArcGIS.Client.WebMap assembly with ESRI.ArcGIS.Client.Portal in Map.xml
                    string mapFilePath = Path.Combine(targetSite.PhysicalPath, @"Config\Map.xml");
                    if (File.Exists(mapFilePath))
                    {

                        // Get the config as a string
                        string mapConfig = null;
                        using (StreamReader streamReader = new StreamReader(mapFilePath))
                        {
                            mapConfig = streamReader.ReadToEnd();
                            streamReader.Close();
                        }

                        if (!string.IsNullOrEmpty(mapConfig)) // Config is empty when linked to web map
                        {

                            // Check whether reference to previous webmap assembly is present and update accordingly
                            string webmapAssembly = ";assembly=ESRI.ArcGIS.Client.WebMap";
                            if (mapConfig != null && mapConfig.Contains(webmapAssembly))
                            {
                                // Update assembly ref
                                mapConfig = mapConfig.Replace(";assembly=ESRI.ArcGIS.Client.WebMap", ";assembly=ESRI.ArcGIS.Client.Portal");

                                // Make backup
                                File.Copy(mapFilePath, mapFilePath + ".bak", true);

                                // Save file
                                File.WriteAllText(mapFilePath, mapConfig);
                            }

                            // Get map configuration as XML document
                            XmlDocument doc = new XmlDocument();
                            doc.Load(mapFilePath);

                            // Routine to replace Attribute property on renderer nodes with Field
                            Action<string> updateRendererNodes = (nodeName) =>
                                {
                                    XmlNodeList uvRendererNodes = doc.GetElementsByTagName(nodeName);
                                    if (uvRendererNodes != null)
                                    {
                                        foreach (XmlNode uvRendererNode in uvRendererNodes)
                                        {
                                            if (uvRendererNode.Attributes["Attribute"] != null)
                                            {
                                                if (uvRendererNode.Attributes["Field"] == null)
                                                    uvRendererNode.Attributes.Append(doc.CreateAttribute("Field"));

                                                uvRendererNode.Attributes["Field"].Value = uvRendererNode.Attributes["Attribute"].Value;
                                                uvRendererNode.Attributes.Remove(uvRendererNode.Attributes["Attribute"]);
                                            }
                                        }
                                    }
                                };

                            updateRendererNodes("esri:UniqueValueRenderer");
                            updateRendererNodes("esri:ClassBreaksRenderer");
                            doc.Save(mapFilePath);
                        }
                    }
                    else
                    {
                        fault = new FaultContract()
                        {
                            FaultType = "Error",
                            Message = "Map configuration not found.  Upgrade cannot continue"
                        };
                        return false;
                    }
                    #endregion
                }

                return true;
            }
            catch (Exception ex)
            {
                fault = new FaultContract()
                {
                    FaultType = "Error",
                    Message = ex.Message
                };
                return false;
            }
        }

        #endregion

        #region Helper Functions
        private Site GetCurrentSiteDetails()
        {
            Site currentSite = new Site();
            currentSite.ID = Guid.NewGuid().ToString();
            currentSite.IISHost = Environment.MachineName;
            currentSite.IISPort = HttpContext.Current == null ? 80 : HttpContext.Current.Request.Url.Port;
            currentSite.IISPath = HttpContext.Current == null ? string.Empty : HttpContext.Current.Request.Path.Replace("/ApplicationBuilder.svc", "");
            currentSite.IsHostedOnIIS = true;
            return currentSite;
        }

        private static bool directoryContainsTemplate(string directory, string appConfigPath, string xapFilePath)
        {
            if (!Directory.Exists(directory))
                return false;

            return File.Exists(directory + "\\" + appConfigPath) || File.Exists(directory + "\\" + xapFilePath);
        }

        private static bool copyWebsiteContentFiles(string sourcePhysicalPath, string targetPhysicalPath, bool overwriteFiles, out string status)
        {
            status = string.Empty;
            try
            {
                // If there are some directories that shouldn't be copied over. Ex: string[] dirsDontCopyContents = new string[] { "\\App_Data\\" };
                string[] dirsDontCopyContents = new string[0];

                string[] subdirectories = Directory.GetDirectories(sourcePhysicalPath);

                IOHelper.CopyDirectoryContents(sourcePhysicalPath, targetPhysicalPath + "\\", false, overwriteFiles);
                foreach (string subdirectory in subdirectories)
                {
                    int indexOfLastSlash = subdirectory.LastIndexOf('\\');
                    string subdirectoryName = "\\" + subdirectory.Substring(indexOfLastSlash + 1) + "\\";

                    Directory.CreateDirectory(targetPhysicalPath + subdirectoryName);

                    if (!stringArrayContainsElement(dirsDontCopyContents, subdirectoryName))
                        IOHelper.CopyDirectoryContents(sourcePhysicalPath + subdirectoryName, targetPhysicalPath + subdirectoryName, true);
                }
            }
            catch (Exception ex)
            {
                status = ex.Message;
                return false;
            }
            return true;
        }

        private static bool stringArrayContainsElement(string[] strings, string elementToSearch)
        {
            foreach (string stringValue in strings)
            {
                if (stringValue.ToLower() == elementToSearch.ToLower())
                    return true;
            }
            return false;
        }

        private static T[] Join<T>(T[] vals, T[] valsToJoin)
        {
            List<T> valsList = new List<T>(vals);
            List<T> valsToJoinList = new List<T>(valsToJoin);
            if (valsToJoin != null)
            {
                foreach (T val in valsToJoinList)
                {
                    if (!valsList.Contains(val))
                        valsList.Add(val);
                }
            }

            return valsList.ToArray();
        }
        #endregion
    }
}
