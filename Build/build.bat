@echo off

dotnet msbuild ..\GladosV3.sln /p:Configuration=Release "/p:Platform=Any CPU"

IF %ERRORLEVEL% NEQ 0 GOTO err
del release.zip
7z a release.zip -mx9 -tzip @glados.lst
7z l release.zip
EXIT /B 0

:err
PAUSE
EXIT /B 1