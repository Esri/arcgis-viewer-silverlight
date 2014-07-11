@echo off
set RootDir=%1

set BuildLocalizedResourceAssemblies=%RootDir%\build\localization\BuildLocalizedResourceAssemblies.cmd
set BuildLocalizedXaps=%RootDir%build\localization\BuildLocalizedXaps.cmd
set Localizer=%RootDir%build\utilities\Localizer\Localizer.exe

@echo ----------------- Extensibility -------------------------------------
set OutDirName=Extensibility

set AssemblyName=ESRI.ArcGIS.Client.Extensibility
set ResourceDir=%RootDir%\src\%OutDirName%\%AssemblyName%
call "%BuildLocalizedResourceAssemblies%" %AssemblyName% %OutDirName% %RootDir% %ResourceDir%

set AssemblyName=ESRI.ArcGIS.Client.Application.Layout
set ResourceDir=%RootDir%\src\%OutDirName%\%AssemblyName%
call "%BuildLocalizedResourceAssemblies%" %AssemblyName% %OutDirName% %RootDir% %ResourceDir%

xcopy "%RootDir%\output\%OutDirName%\Localization" "%RootDir%\output\localization\" /S /Y

@echo ----------------- Internal Framework -------------------------------------
set OutDirName=Internal\Framework

set AssemblyName=ESRI.ArcGIS.Mapping.Core
set ResourceDir=%RootDir%\src\%OutDirName%\%AssemblyName%
call "%BuildLocalizedResourceAssemblies%" %AssemblyName% %OutDirName% %RootDir% %ResourceDir%

set AssemblyName=ESRI.ArcGIS.Mapping.Windowing
set ResourceDir=%RootDir%\src\%OutDirName%\%AssemblyName%
call "%BuildLocalizedResourceAssemblies%" %AssemblyName% %OutDirName% %RootDir% %ResourceDir%

set AssemblyName=ESRI.ArcGIS.Mapping.DataSources
set ResourceDir=%RootDir%\src\%OutDirName%\%AssemblyName%
call "%BuildLocalizedResourceAssemblies%" %AssemblyName% %OutDirName% %RootDir% %ResourceDir%

xcopy "%RootDir%\output\%OutDirName%\Localization" "%RootDir%\output\localization\" /S /Y

@echo ----------------- Internal Controls -------------------------------------
set OutDirName=Internal\Controls

set AssemblyName=ESRI.ArcGIS.Mapping.Controls
set ResourceDir=%RootDir%\src\%OutDirName%\%AssemblyName%
call "%BuildLocalizedResourceAssemblies%" %AssemblyName% %OutDirName% %RootDir% %ResourceDir%
xcopy "%RootDir%\output\%OutDirName%\Localization\" "%RootDir%\output\localization\" /S /Y

set AssemblyName=ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
set ResourceDir=%RootDir%\src\%OutDirName%\%AssemblyName%
call "%BuildLocalizedResourceAssemblies%" %AssemblyName% %OutDirName% %RootDir% %ResourceDir%

set AssemblyName=ESRI.ArcGIS.Mapping.Controls.Editor
set ResourceDir=%RootDir%\src\%OutDirName%\%AssemblyName%
call "%BuildLocalizedResourceAssemblies%" %AssemblyName% %OutDirName% %RootDir% %ResourceDir%

set AssemblyName=ESRI.ArcGIS.Mapping.Controls.MapContents
set ResourceDir=%RootDir%\src\%OutDirName%\%AssemblyName%
call "%BuildLocalizedResourceAssemblies%" %AssemblyName% %OutDirName% %RootDir% %ResourceDir%

xcopy "%RootDir%\output\%OutDirName%\Localization" "%RootDir%\output\localization\" /S /Y

@echo ----------------- Internal Add-Ins -------------------------------------
set OutDirName=Internal\AddIns

set AssemblyName=ESRI.ArcGIS.Mapping.Identify
set ResourceDir=%RootDir%\src\%OutDirName%\%AssemblyName%
call "%BuildLocalizedResourceAssemblies%" %AssemblyName% %OutDirName% %RootDir% %ResourceDir%

set AssemblyName=ESRI.ArcGIS.Mapping.GP
set ResourceDir=%RootDir%\src\%OutDirName%\%AssemblyName%
call "%BuildLocalizedResourceAssemblies%" %AssemblyName% %OutDirName% %RootDir% %ResourceDir%

xcopy "%RootDir%\output\%OutDirName%\Localization" "%RootDir%\output\localization\" /S /Y

@echo ----------------- Viewer -------------------------------------
set ProjectFolderName=ESRI.ArcGIS.Mapping.Viewer
set AppName=Viewer
set SatelliteAssemblies=ESRI.ArcGIS.Client.Extensibility.resources.dll ESRI.ArcGIS.Client.Application.Layout.resources.dll ESRI.ArcGIS.Mapping.Controls.MapContents.resources.dll ESRI.ArcGIS.Mapping.Controls.resources.dll ESRI.ArcGIS.Mapping.Controls.Editor.resources.dll ESRI.ArcGIS.Mapping.Core.resources.dll ESRI.ArcGIS.Mapping.DataSources.resources.dll ESRI.ArcGIS.Mapping.Identify.resources.dll ESRI.ArcGIS.Mapping.Windowing.resources.dll ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.resources.dll ESRI.ArcGIS.Mapping.GP.resources.dll
set APISatelliteAssemblies=ESRI.ArcGIS.Client.Behaviors.resources.dll ESRI.ArcGIS.Client.Bing.resources.dll ESRI.ArcGIS.Client.resources.dll ESRI.ArcGIS.Client.Toolkit.DataSources.resources.dll ESRI.ArcGIS.Client.Toolkit.resources.dll ESRI.ArcGIS.Client.Portal.resources.dll

REM copy satellite assemblies from App Builder output directory to localization staging directory.
REM this includes the satellite resource assemblies from dependencies
xcopy "%RootDir%\output\localization\Builder" "%RootDir%\output\localization\" /S /Y

REM build localized satellite XAP files for Viewer
set ResourceDir=%RootDir%\src\%AppName%\%ProjectFolderName%
call "%BuildLocalizedXaps%" %AppName% %RootDir% %ResourceDir% "%SatelliteAssemblies% %APISatelliteAssemblies%"

REM localize loose files (e.g. html and config files)
call "%Localizer%" %AppName% "%RootDir%\output\%AppName%" 

@echo ----------------- Builder -------------------------------------
set ProjectFolderName=ESRI.ArcGIS.Mapping.Builder
set AppName=Builder
set OutputFolder=localization\Builder
set ResourceDir=%RootDir%\src\%AppName%\%ProjectFolderName%

REM build App Builder satellite resource assembly
call "%BuildLocalizedResourceAssemblies%" %ProjectFolderName% %OutputFolder% %RootDir% %ResourceDir%

xcopy "%RootDir%\output\%OutputFolder%\Localization" "%RootDir%\output\localization\" /S /Y

set SatelliteAssemblies=ESRI.ArcGIS.Client.Extensibility.resources.dll ESRI.ArcGIS.Client.Application.Layout.resources.dll ESRI.ArcGIS.Mapping.Controls.MapContents.resources.dll ESRI.ArcGIS.Mapping.Controls.resources.dll ESRI.ArcGIS.Mapping.Controls.Editor.resources.dll ESRI.ArcGIS.Mapping.Core.resources.dll ESRI.ArcGIS.Mapping.DataSources.resources.dll ESRI.ArcGIS.Mapping.Identify.resources.dll ESRI.ArcGIS.Mapping.Windowing.resources.dll ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.resources.dll ESRI.ArcGIS.Mapping.GP.resources.dll ESRI.ArcGIS.Mapping.Builder.resources.dll

REM build localized satellite XAP files for App Builder
set ResourceDir=%RootDir%\src\%AppName%\%ProjectFolderName%
call "%BuildLocalizedXaps%" %ProjectFolderName% %RootDir% %ResourceDir% "%SatelliteAssemblies% %APISatelliteAssemblies%" %AppName%

REM localize loose files (e.g. html and config files)
call "%Localizer%" %AppName% "%RootDir%\output\%AppName%" 

REM cleanup folder to which resource satellite assemblies were copied
rmdir "%RootDir%\output\localization\" /S /Q

:End
