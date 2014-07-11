@echo off
@echo ---- Localization: Compile Resources

if NOT "%1"=="" set ResourceName=%1
if NOT "%2"=="" set ProjectOut=%2
if NOT "%3"=="" set RootDir=%3
if NOT "%4"=="" set ProjectFolder=%4
if NOT EXIST %ProjectFolder% (set ProjectFolder=%4 %5)


@echo ResourceName=%ResourceName%
@echo ProjectOut=%ProjectOut%
@echo ArcGIS Directory=%RootDir%%
@echo ProjectFolder=%ProjectFolder%

if "%ResourceName%"=="" goto Error
if "%ProjectOut%"=="" goto Error
if "%RootDir%"=="" goto Error
if "%ProjectFolder%"=="" goto Error

if NOT EXIST "%ProjectFolder%" (goto Error)
cd %ProjectFolder%

set OutputDir=%RootDir%output\%ProjectOut%\Localization

rem Get Version number for resource assembly
set VersionNumberAssembly=%OutputDir%\..\%ResourceName%.dll

REM get build number
"%RootDir%Build\Utilities\sigcheck" -n -q "%OutputDir%\..\%ResourceName%.dll">version.txt
SET /p AssemblyVersion=<version.txt
del version.txt /F /Q

set ResourceNamespace=%ResourceName%
set Platform=/platform:anycpu 
set Version=/v:%AssemblyVersion% /fileversion:%AssemblyVersion%
set Title=/title:%ResourceName% Resource Assembly.
set Product=/product:%ResourceName% Resource Assembly.
set Description=/descr:%ResourceName% Resource Assembly.
set Company=/company:Environmental Systems Research Institute
set Copyright=/copy:E.S.R.I. All rights reserved.

set ResGen="C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\ResGen.exe"
set AsmLinker="C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\Al.exe"

@echo .
@echo ResourceName:%ResourceName%
@echo ResourceNamespace:%ResourceNamespace%
@echo AppBuildOutput:%ProjectOut%
@echo Directory:%ProjectFolder%
@echo AssemblyVersion:%AssemblyVersion%

attrib -R /S /D "%ProjectFolder%\Resources"
attrib -R /S "%ProjectFolder%\Resources\*.resx"
if NOT EXIST "%OutputDir%\Languages.txt" (xcopy "%RootDir%build\localization\Languages.txt" %OutputDir%\ /C/I/R/Y )

set LanguageFile=%OutputDir%\Languages.txt
FOR /F "eol=; tokens=1* delims=," %%C in (%LanguageFile%) do (
if EXIST Resources\Strings.%%C.resx (
mkdir %OutputDir%\%%C
%ResGen% /compile Resources\Strings.%%C.resx,Resources\Strings.%%C.resources
REM %AsmLinker% /t:lib /embed:Resources\Strings.%%C.resources,%ResourceNamespace%.Resources.Strings.%%C.resources /culture:%%C /out:%OutputDir%\%%C\%ResourceName%.resources.dll %Platform% %Version% "%Keyfile%" "%Title%" "%Product%" "%Description%" "%Company%" "%Copyright%" ))
%AsmLinker% /t:lib /embed:Resources\Strings.%%C.resources,%ResourceNamespace%.Resources.Strings.%%C.resources /culture:%%C /out:%OutputDir%\%%C\%ResourceName%.resources.dll %Platform% %Version% "%Title%" "%Product%" "%Description%" "%Company%" "%Copyright%" ))
del /F /Q "%OutputDir%\Languages.txt"
goto End

:Error
@echo ---- Error: invalid commandline
@echo Resource Name: %ResourceName%
@echo Project Output: %ProjectOut%
@echo Project Directory: %ProjectFolder%
@echo ArcGIS Directory: %RootDir%%

:End
