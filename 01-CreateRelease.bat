@echo off

SET NAME=OcbClaimAutoRepair

if not exist build\ (
  mkdir build
)

if exist build\%NAME%\ (
  echo remove existing directory
  rmdir build\%NAME% /S /Q
)

mkdir build\%NAME%

SET VERSION=snapshot

if not "%1"=="" (
  SET VERSION=%1
)

echo create %VERSION%

xcopy *.dll build\%NAME%\
xcopy README.md build\%NAME%\
xcopy ModInfo.xml build\%NAME%\
xcopy Config build\%NAME%\Config\ /S
xcopy Resources build\%NAME%\Resources\ /S
xcopy UIAtlases build\%NAME%\UIAtlases\ /S

REM xcopy BepInEx build\%NAME%\BepInEx\ /S
REM xcopy patchers\*.dll build\%NAME%\patchers\
REM xcopy 98-install-bepinex.sh build\%NAME%\
REM xcopy 98-install-bepinex.bat build\%NAME%\
REM xcopy 99-uninstall-bepinex.sh build\%NAME%\
REM xcopy 99-uninstall-bepinex.bat build\%NAME%\

cd build
echo Packaging %NAME%-%VERSION%.zip
powershell Compress-Archive %NAME% %NAME%-%VERSION%.zip -Force
cd ..

SET RV=%ERRORLEVEL%
if "%CI%"=="" pause
exit /B %RV%
