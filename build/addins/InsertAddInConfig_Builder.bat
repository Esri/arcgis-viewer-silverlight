@echo off

REM =================================================================================
REM Build script for incorporating configuration of a Silverlight Viewer add-in into 
REM the Silverlight Viewer's Application Builder.  This script inserts configuration 
REM elements from the specified config file into the App Builder's config files.
REM 
REM The script has two required parameters:
REM
REM		root directory - the path to the root of the local repo for the viewer's
REM			source code, e.g. c:\projects\silverlight-viewer
REM
REM		source config file - the path to the configuration file containing the elements
REM			to insert into the Builder's config files
REM
REM These parameters must be specified in the order listed above.  Example usage:
REM
REM		AddInsBuildScript.bat c:\projects\silverlight-viewer c:\projects\silverlight-viewer\AddIns\SearchTool\BuilderConfig.xml
REM =================================================================================

set rootDir=%~1
set configFile=%~2%

echo workspace directory is "%rootDir%"

set XmlHelper=%rootDir%\build\utilities\XmlHelper.exe
set BuilderDir=%rootDir%\output\Builder\

echo Copying add-in config to Builder

REM use XmlHelper to copy configuration from add-in's config file to main Viewer configs

if NOT EXIST "%BuilderDir%\App_Data\Extensions.xml" goto End

REM copy the Extension element to Builder's list of available extensions
"%XmlHelper%" -copyelements elementname=Extension sourcefile="%configFile%" targetfile="%BuilderDir%\App_Data\Extensions.xml"

:End
