@echo off

REM =================================================================================
REM Build script for incorporating configuration of a Silverlight Viewer add-in into 
REM the default Silverlight Viewer application.  This script inserts configuration 
REM elements from the specified config file into the Viewer's config files.
REM 
REM The script has two required parameters:
REM
REM		root directory - the path to the root of the local repo for the viewer's
REM			source code, e.g. c:\projects\silverlight-viewer
REM
REM		source config file - the path to the configuration file containing the elements
REM			to insert into the Viewer's config files
REM
REM These parameters must be specified in the order listed above.  Example usage:
REM
REM		AddInsBuildScript.bat c:\projects\silverlight-viewer c:\projects\silverlight-viewer\AddIns\Bookmarks\ViewerConfig.xml
REM =================================================================================

set rootDir=%~1
set configFile=%~2%

echo workspace directory is "%rootDir%"

set XmlHelper=%rootDir%\build\utilities\XmlHelper.exe
set ViewerDir=%rootDir%\output\Viewer\

echo Copying add-in config to Viewer

REM use XmlHelper to copy configuration from add-in's config file to main Viewer configs

if NOT EXIST "%ViewerDir%\Config\Application.xml" goto End

REM first copy the Extension element to Application.xml	
"%XmlHelper%" -copyelements elementname=Extension sourcefile="%configFile%" targetfile="%ViewerDir%\Config\Application.xml"

REM copy the Tool element to Tools.xml.  This will include the tool on Viewer's toolbar.
"%XmlHelper%" -copyelements elementname=Tool sourcefile="%configFile%" targetfile="%ViewerDir%\Config\Tools.xml"

REM copy the Behavior element to Behaviors.xml.  This will include the tool in the Viewer's default behaviors.
"%XmlHelper%" -copyelements elementname=Behavior sourcefile="%configFile%" targetfile="%ViewerDir%\Config\Behaviors.xml"

:End