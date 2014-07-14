arcgis-viewer-silverlight
===============================

This repository contains the source code for the ArcGIS Viewer for Silverlight (Silverlight Viewer).  This includes the Application Builder, Viewer, Tools, Extensibility SDK, and Visual Studio Project Template that are all part of the ArcGIS Viewer for Silverlight product.  Learn more about the Silverlight Viewer at the [ArcGIS Viewer for Silverlight Resource Center](http://links.esri.com/silverlightviewer).  

![ArcGIS Viewer for Silverlight](https://cloud.githubusercontent.com/assets/3933510/3559658/46dac540-094e-11e4-89fa-b57765e6e726.png)

###Overview
The ArcGIS Viewer for Silverlight is a ready-to-deploy GIS web mapping app.  The product includes an Application Builder web application that allows users to build Viewer applications interactively in the browser by pointing and clicking - no code or editing of configuration files is required.  The Viewer provides the ability to configure interactive maps containing content from ArcGIS Server and ArcGIS Online, and includes many tools and capabilities for analyzing and interacting with map data.  The Viewer's capabilities can be extended by developing add-ins with the Extensibility SDK.  A Visual Studio project template is included to provide a starting point for creating add-ins.

###Getting Started

To get started working with the source code, simply [clone](https://help.github.com/articles/duplicating-a-repository) or [fork](https://help.github.com/articles/fork-a-repo) this repo to create a local copy on your computer.  Alternatively, you can download a zip file containing the source code, although doing so will remove any association between your copy of the source code and the repo.  This means that your local copy of the source code will not be under version control, so you will not be able to compare your copy to the master to see, for instance, when there are updates that you may want to incorporate into your local copy.  All the product's source code is located beneath the *src* folder.

**Prerequisites**

To work with the Viewer’s source code, the development machine must have the following:

- Visual Studio 2013 Update 2, including:
 - Silverlight SDK
 - NuGet Package Manager (updates [here](http://visualstudiogallery.msdn.microsoft.com/27077b70-9dad-4c64-adcf-c7cf6bc9970c))
- [NuGet Package Restore](http://docs.nuget.org/docs/reference/package-restore) feature enabled (Tools --> Options --> NuGet Package Manager --> Allow NuGet to Download Missing Packages)
![Allow NuGet to Download Missing Packages](https://cloud.githubusercontent.com/assets/3933510/3559669/5ff952b2-094e-11e4-8a65-a72e149c117a.png) 

**Compiling and Debugging**

To compile the product, simply open the *ArcGISSilverlightViewer.sln* file in Visual Studio and build it.  The solution is located within the *src* folder located at the root of the repository.  Building the solution will compile the Viewer, Application Builder, Extensibility SDK, Add-Ins, and Visual Studio Project Template.  You can find the build outputs in the following locations:

- Application Builder – output\Builder
- Viewer – output\Viewer
- Extensibility SDK assemblies – output\Extensibility
- Add-Ins – output\Builder\Extensions
- Project Template – output\ProjectTemplate

For debugging the Application Builder and Viewer, two ASP.NET website projects, Builder and Viewer, are included in the ArcGISSilverlightViewer solution.  The Builder project is located within the Builder solution folder, while the Viewer project can be found under the Viewer folder.  To debug, simply set one of these as the startup project and start debugging. By default, the Builder project is set as the startup project.  Note that, to step through the source code, Silverlight debugging must be enabled on the Builder or Viewer project.  This setting can be found on the Start Options tab of the project properties dialog:

![Enable Silverlight debugging](https://cloud.githubusercontent.com/assets/3933510/3559674/711eff9c-094e-11e4-89ef-d310d1a4d5a3.png)

**Deploying Viewer Applications**

In order to deploy Viewer Applications from your locally compiled version of Application Builder, a bit of configuration is required.  First, the directory to which you want applications deployed must be specified in the Application Builder's *web.config* file.  This file is located in the source code at *<repo root\>\src\Builder\ESRI.ArcGIS.Mapping.Builder.Web\Web.config*.  The settings that must be specified are:

- AppsPhysicalDir - this is the path to the folder on disk, such as *c:\inetpub\wwwroot\apps*
- AppsBaseUrl - this is the URL to the folder, such as `http://server.mydomain.com/apps`

Once specified in the *web.config* file, these would appear as follows:

![Application Builder *web.config* settings](https://cloud.githubusercontent.com/assets/3933510/3559675/8264386c-094e-11e4-84dc-341376d8c306.png)

Additionally, the directory specified must be [configured as a virtual directory in IIS](http://technet.microsoft.com/en-us/library/cc771804%28v=WS.10%29.aspx) and must have write permissions granted to the identity under which the Application Builder's web service runs.  When debugging from Visual Studio, the web server process (iisexpress.exe) generally runs under the identity of the logged-in user.  

Finally, to ensure the ability to deploy Viewer Applications, the machine that is targeted for deployment may require a [*clientaccesspolicy.xml*](http://msdn.microsoft.com/en-us/library/cc197955%28v=vs.95%29.aspx) or [*crossdomain.xml* file](http://www.adobe.com/devnet/articles/crossdomain_policy_file_spec.html) that permits requests from the Application Builder's URL.  When debugging from Visual Studio, this will generally be `http://localhost:<port number>/default.aspx`.  The *crossdomain.xml* file may be modified to explicitly allow this domain, or wildcards may be used to permit broader access that is inclusive of this domain.

Note also that, if you are deploying applications while debugging in Visual Studio, and the deployment directory for your applications is within the root directory of IIS (wwwroot), you may need to run Visual Studio as an administrator.  This is because User Access Control (UAC) on Windows requires administrator privileges to write to this location.

**Installing the Visual Studio Project Template**

When the ArcGISSilverlightViewer solution is compiled, a Visual Studio project template is created that provides a starting point for developing add-ins to extend the capabilities of the Viewer.  This template can be installed as follows:

1. If Visual Studio is open, close it.
2. Copy the *ESRIViewer.zip* file from *<repo root\>\output\ProjectTemplate* to *<Program Files x86\>\Microsoft Visual Studio 12.0\Common7\IDE\ProjectTemplates\CSharp\Silverlight\Esri*.  If the *Esri* folder does not exist within the *Silverlight* folder, create it.
3. Open the [Visual Studio Command Prompt](http://msdn.microsoft.com/en-us/library/ms229859%28v=vs.110%29.aspx) as an administrator.
4. Type the following command and hit *Enter*:
```devenv.exe /InstallVSTemplates```

Once the template is installed, it will be available in Visual Studio's New Project dialog under *Visual C# --> Silverlight --> Esri:*

![Viewer Visual Studio Template](https://cloud.githubusercontent.com/assets/3933510/3559679/92e97986-094e-11e4-8947-f08c53ec19e9.png)

**Note**: In order for the ArcGIS assemblies referenced by the project template to be resolved, version 3.2 of the ArcGIS API for Silverlight must be installed, and the paths to the Extensibility SDK assemblies will need to be updated to those of the locally compiled versions.

**Using Localized Versions**

The ArcGIS Viewer for Silverlight is available in 23 languages.  While compiling and debugging the Viewer source code shows the application in English by default, any of the other 22 languages can be shown as follows:

*For Application Builder*

1. Compile the ArcGISSilverlightViewer solution
2. Copy the contents of the folder *<repo root\>\output\Builder\Culture\\<culture code\>* folder to the *<repo root\>\output\Builder* folder.  
3. When prompted, select the option to replace existing files.
4. Copy the contents of the folder *<repo root\>\output\Builder\Templates\Default\Culture\\<culture code\>* folder to *<repo root\>\output\Builder\Templates\Default*.
5. When prompted, select the option to replace existing files.
6. Clear your browser's cache
7. Load or reload the Application Builder in the browser.

*For Viewer*

1. Compile the ArcGISSilverlightViewer solution.
2. Copy the contents of the *<repo root\>\output\Viewer\Culture\\<culture code\>* folder to the *<repo root\>\output\Viewer* folder.
3. When prompted, select the option to replace existing files.
4. Clear your browser's cache.
5. Load or reload the Viewer in the browser.

Once these steps are complete, the application should appear with text in the desired language:

![Localized Application Builder](https://cloud.githubusercontent.com/assets/3933510/3559683/a03887a8-094e-11e4-806f-ea6e0a96e0f7.png)

###Dependencies

The ArcGIS Viewer for Silverlight references the 3rd party dependencies listed below.  Where possible, NuGet is used to dynamically download the assemblies for these dependencies at compile time by leveraging its [Package Restore](http://docs.nuget.org/docs/reference/package-restore) capability.  In a few cases, dependencies are not available via NuGet.  Assemblies for these are included in the repo.  The dependencies are as follows:

- [ArcGIS API 3.2 for Silverlight](http://links.esri.com/silverlight)
- [ArcGIS Silverlight Toolkit 3.2](https://github.com/esri/arcgis-toolkit-sl-wpf/)
- Expression Blend 5 SDK
- [Silverlight 5 Toolkit](http://silverlight.codeplex.com/)
- [Microsoft Async Targeting Pack](http://blogs.msdn.com/b/bclteam/p/asynctargetingpackkb.aspx)
- [Microsoft Http Client Libraries](http://blogs.msdn.com/b/bclteam/p/httpclient.aspx)


### Issues

Find a bug or want to request a new feature?  Please let us know by submitting an issue.

### Contributing

Anyone and everyone is welcome to [contribute](https://www.github.com/arcgis/arcgis-viewer-silverlight/wiki/contributing).

### Licensing
Copyright 2014 Esri

This source is subject to the Microsoft Public License (Ms-PL).
You may obtain a copy of the License at

http://www.microsoft.com/en-us/openness/licenses.aspx#MPL

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

A copy of the license is available in the repository's [license.txt](https://www.github.com/arcgis/arcgis-viewer-silverlight/blob/master/license.txt) file.

[](Esri Tags: ArcGIS Silverlight SDK .NET Viewer Applications C# C-Sharp cs DotNet XAML)
[](Esri Language: DotNet)
