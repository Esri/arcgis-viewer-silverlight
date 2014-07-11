@echo off
@echo ---- Localization: create zipped Xap files.

if NOT "%1"=="" set XapName=%1
if NOT "%2"=="" set RootDir=%2
if NOT "%3"=="" set ProjectFolder=%3
if NOT %4=="" set SatelliteAssemblies=%~4

set OutputFolderName=%XapName%
if NOT "%5"=="" set OutputFolderName=%5

@echo XapName=%XapName%
@echo ArcGIS Directory=%RootDir%%
@echo ProjectFolder=%ProjectFolder%
@echo SatelliteAssemblies=%SatelliteAssemblies%

if "%XapName%"=="" goto Error
if "%RootDir%"=="" goto Error
if "%ProjectFolder%"=="" goto Error
if "%SatelliteAssemblies%"=="" goto Error

set Zip=%RootDir%build\utilities\7za.exe

if NOT EXIST "%ProjectFolder%" (goto Error)
cd %ProjectFolder%

set SatelliteAssemblyDir=%RootDir%output\localization
set OutputDir=%RootDir%output\%OutputFolderName%\Culture\Xaps

set LanguageFile=%OutputDir%\Languages.txt

@echo .
@echo XapName=%XapName%
@echo ProjectFolder=%ProjectFolder%
@echo SatelliteAssemblies=%SatelliteAssemblies%
@echo OutputDir=%OutputDir%

if NOT EXIST "%OutputDir%\Languages.txt" (xcopy "%RootDir%build\localization\Languages.txt" %OutputDir%\ /C/I/R/Y )
FOR /F "eol=; tokens=1* delims=," %%C in (%LanguageFile%) do (
if EXIST "%SatelliteAssemblyDir%\%%C" (
cd "%SatelliteAssemblyDir%\%%C"
mkdir "%OutputDir%\%%C"
xcopy "%APISrc%\%%C" "%SatelliteAssemblyDir%\%%C" /S/C/I/R/Y
"%ZIP%" a -tzip "%OutputDir%\%%C\%XapName%.resources.xap" %ProjectFolder%\Resources\AppResourcesManifest.xaml %SatelliteAssemblies% ))
del /F /Q "%OutputDir%\Languages.txt"
goto End

:Error
@echo ------------ Error: invalid commandline
@echo Resource Name: %XapName%
@echo Project Directory: %ProjectFolder%
@echo ArcGIS Directory: %RootDir%

:End
