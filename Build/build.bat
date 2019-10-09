@echo off

dotnet msbuild ..\GladosV3.sln /p:Configuration=Release "/p:Platform=Any CPU"
IF %ERRORLEVEL% NEQ 0 GOTO err
del release.zip
7z a release.zip -mx9 -tzip @glados.lst
7z l release.zip
echo Build successful!
timeout /t 10
EXIT /B 0

:err
echo Build failed!
PAUSE
EXIT /B 1