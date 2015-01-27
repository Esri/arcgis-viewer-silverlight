@echo off

set solutionDir=%~1
set configName=%~2

set outputPath=%solutionDir%..\..\..\output\win_sl5_%configName%\bin\
set targetStartPath=%solutionDir%packages\ArcGISSilverlight-
set targetEndPath=.3.2.0.22\lib\sl50\
echo outputPath: %outputPath%
echo targetStartPath: %targetStartPath%
echo targetEndPath: %targetEndPath%

if exist "%outputPath%ESRI.ArcGIS.Client.dll" (
    xcopy "%outputPath%ESRI.ArcGIS.Client.dll" "%targetStartPath%Core%targetEndPath%*" /Y /R
    xcopy "%outputPath%ESRI.ArcGIS.Client.xml" "%targetStartPath%Core%targetEndPath%*" /Y /R
    xcopy "%outputPath%ESRI.ArcGIS.Client.pdb" "%targetStartPath%Core%targetEndPath%*" /Y /R
)

if exist "%outputPath%ESRI.ArcGIS.Client.Behaviors.dll" (
    xcopy "%outputPath%ESRI.ArcGIS.Client.Behaviors.dll" "%targetStartPath%Behaviors%targetEndPath%*" /Y /R
    xcopy "%outputPath%ESRI.ArcGIS.Client.Behaviors.xml" "%targetStartPath%Behaviors%targetEndPath%*" /Y /R
    xcopy "%outputPath%ESRI.ArcGIS.Client.Behaviors.pdb" "%targetStartPath%Behaviors%targetEndPath%*" /Y /R
)

if exist "%outputPath%ESRI.ArcGIS.Client.Bing.dll" (
    xcopy "%outputPath%ESRI.ArcGIS.Client.Bing.dll" "%targetStartPath%Bing%targetEndPath%*" /Y /R
    xcopy "%outputPath%ESRI.ArcGIS.Client.Bing.xml" "%targetStartPath%Bing%targetEndPath%*" /Y /R
    xcopy "%outputPath%ESRI.ArcGIS.Client.Bing.pdb" "%targetStartPath%Bing%targetEndPath%*" /Y /R
)

if exist "%outputPath%ESRI.ArcGIS.Client.Portal.dll" (
    xcopy "%outputPath%ESRI.ArcGIS.Client.Portal.dll" "%targetStartPath%Portal%targetEndPath%*" /Y /R
    xcopy "%outputPath%ESRI.ArcGIS.Client.Portal.xml" "%targetStartPath%Portal%targetEndPath%*" /Y /R
    xcopy "%outputPath%ESRI.ArcGIS.Client.Portal.pdb" "%targetStartPath%Portal%targetEndPath%*" /Y /R
)

if exist "%outputPath%ESRI.ArcGIS.Client.Printing.dll" (
    xcopy "%outputPath%ESRI.ArcGIS.Client.Printing.dll" "%targetStartPath%Printing%targetEndPath%*" /Y /R
    xcopy "%outputPath%ESRI.ArcGIS.Client.Printing.xml" "%targetStartPath%Printing%targetEndPath%*" /Y /R
    xcopy "%outputPath%ESRI.ArcGIS.Client.Printing.pdb" "%targetStartPath%Printing%targetEndPath%*" /Y /R
)

if exist "%outputPath%ESRI.ArcGIS.Client.Toolkit.dll" (
    xcopy "%outputPath%ESRI.ArcGIS.Client.Toolkit.dll" "%targetStartPath%Toolkit%targetEndPath%*" /Y /R
    xcopy "%outputPath%ESRI.ArcGIS.Client.Toolkit.xml" "%targetStartPath%Toolkit%targetEndPath%*" /Y /R
    xcopy "%outputPath%ESRI.ArcGIS.Client.Toolkit.pdb" "%targetStartPath%Toolkit%targetEndPath%*" /Y /R
)

if exist "%outputPath%ESRI.ArcGIS.Client.Toolkit.DataSources.dll" (
    xcopy "%outputPath%ESRI.ArcGIS.Client.Toolkit.DataSources.dll" "%targetStartPath%ToolkitDataSources%targetEndPath%*" /Y /R
    xcopy "%outputPath%ESRI.ArcGIS.Client.Toolkit.DataSources.xml" "%targetStartPath%ToolkitDataSources%targetEndPath%*" /Y /R
    xcopy "%outputPath%ESRI.ArcGIS.Client.Toolkit.DataSources.pdb" "%targetStartPath%ToolkitDataSources%targetEndPath%*" /Y /R
)