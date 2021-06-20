@echo off
title GLaDOS Builder
set DOTNET_CLI_TELEMETRY_OPTOUT=1
set GLADOSRELEASE=..\GLaDOSV3\bin\Release\net5.0

mkdir Build
del /q /f /s .\Build  >nul 2>&1
dotnet nuget add source -n Discord.net https://www.myget.org/F/discord-net/api/v3/index.json  >nul 2>&1
echo Restoring nuget packages...
dotnet restore ..\GladosV3.sln  >nul 2>&1

echo Building win-x64!
dotnet msbuild ..\GladosV3.sln /p:Configuration=Release "/p:Platform=x64" >nul 2>&1
dotnet publish ..\GLaDOSV3\GladosV3.csproj -c Release -p:PublishSingleFile=true --self-contained true -r win-x64 -o .\Build\win-x64 >nul 2>&1
IF %ERRORLEVEL% NEQ 0 GOTO err
echo Copying important files!
set BUILDLOCATION=.\Build\win-x64
mkdir %BUILDLOCATION%\Binaries\ >nul 2>&1
mkdir %BUILDLOCATION%\Modules\ >nul 2>&1
mkdir %BUILDLOCATION%\Images\ >nul 2>&1
mkdir %BUILDLOCATION%\Dependencies\ >nul 2>&1
mkdir %BUILDLOCATION%\runtimes\ >nul 2>&1
xcopy %GLADOSRELEASE%\Binaries\     %BUILDLOCATION%\Binaries\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\Modules\      %BUILDLOCATION%\Modules\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\Images\       %BUILDLOCATION%\Images\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\Dependencies\ %BUILDLOCATION%\Dependencies\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\runtimes\ %BUILDLOCATION%\runtimes\ /E /H /C /I >nul 2>&1
IF EXIST %GLADOSRELEASE%\database.db (
	echo Deleting database.db to prevent any leaks!
	del %BUILDLOCATION%\database.db /q /f >nul 2>&1
)

echo Building win-x86!
dotnet msbuild ..\GladosV3.sln /p:Configuration=Release "/p:Platform=x86" >nul 2>&1
dotnet publish ..\GLaDOSV3\GladosV3.csproj -c Release -p:PublishSingleFile=true --self-contained true -r win-x86 -o .\Build\win-x86 >nul 2>&1
IF %ERRORLEVEL% NEQ 0 GOTO err
echo Copying important files!
set BUILDLOCATION=.\Build\win-x86
mkdir %BUILDLOCATION%\Binaries\ >nul 2>&1
mkdir %BUILDLOCATION%\Modules\ >nul 2>&1
mkdir %BUILDLOCATION%\Images\ >nul 2>&1
mkdir %BUILDLOCATION%\Dependencies\ >nul 2>&1
mkdir %BUILDLOCATION%\runtimes\ >nul 2>&1
xcopy %GLADOSRELEASE%\Binaries\     %BUILDLOCATION%\Binaries\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\Modules\      %BUILDLOCATION%\Modules\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\Images\       %BUILDLOCATION%\Images\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\Dependencies\ %BUILDLOCATION%\Dependencies\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\runtimes\ %BUILDLOCATION%\runtimes\ /E /H /C /I >nul 2>&1
IF EXIST %GLADOSRELEASE%\database.db ( 
	echo Deleting database.db to prevent any leaks!
	del %BUILDLOCATION%\database.db /q /f >nul 2>&1
)

echo Building linux-x64!
dotnet msbuild ..\GladosV3.sln /p:Configuration=Release "/p:Platform=x64" >nul 2>&1
dotnet publish ..\GLaDOSV3\GladosV3.csproj -c Release -p:PublishSingleFile=true --self-contained true -r linux-x64 -o .\Build\linux-x64 >nul 2>&1
IF %ERRORLEVEL% NEQ 0 GOTO err
echo Copying important files!
set BUILDLOCATION=.\Build\linux-x64
mkdir %BUILDLOCATION%\Binaries\ >nul 2>&1
mkdir %BUILDLOCATION%\Modules\ >nul 2>&1
mkdir %BUILDLOCATION%\Images\ >nul 2>&1
mkdir %BUILDLOCATION%\Dependencies\ >nul 2>&1
mkdir %BUILDLOCATION%\runtimes\ >nul 2>&1
xcopy %GLADOSRELEASE%\Binaries\     %BUILDLOCATION%\Binaries\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\Modules\      %BUILDLOCATION%\Modules\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\Images\       %BUILDLOCATION%\Images\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\Dependencies\ %BUILDLOCATION%\Dependencies\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\runtimes\ %BUILDLOCATION%\runtimes\ /E /H /C /I >nul 2>&1
IF EXIST %GLADOSRELEASE%\database.db ( 
	echo Deleting database.db to prevent any leaks!
	del %BUILDLOCATION%\database.db /q /f >nul 2>&1
)

echo Building portable-x64!
dotnet msbuild ..\GladosV3.sln /p:Configuration=Release "/p:Platform=x64" >nul 2>&1
dotnet publish ..\GLaDOSV3\GladosV3.csproj -c Release -p:UseAppHost=false -o .\Build\portable-x64 >nul 2>&1
IF %ERRORLEVEL% NEQ 0 GOTO err
echo Copying important files!
set BUILDLOCATION=.\Build\portable-x64
mkdir %BUILDLOCATION%\Binaries\ >nul 2>&1
mkdir %BUILDLOCATION%\Modules\ >nul 2>&1
mkdir %BUILDLOCATION%\Images\ >nul 2>&1
mkdir %BUILDLOCATION%\Dependencies\ >nul 2>&1
mkdir %BUILDLOCATION%\runtimes\ >nul 2>&1
xcopy %GLADOSRELEASE%\Binaries\     %BUILDLOCATION%\Binaries\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\Modules\      %BUILDLOCATION%\Modules\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\Images\       %BUILDLOCATION%\Images\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\Dependencies\ %BUILDLOCATION%\Dependencies\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\runtimes\ %BUILDLOCATION%\runtimes\ /E /H /C /I >nul 2>&1
IF EXIST %GLADOSRELEASE%\database.db ( 
	echo Deleting database.db to prevent any leaks!
	del %BUILDLOCATION%\database.db /q /f >nul 2>&1
)
echo Building portable-x86!
dotnet msbuild ..\GladosV3.sln /p:Configuration=Release "/p:Platform=x86" >nul 2>&1
dotnet publish ..\GLaDOSV3\GladosV3.csproj -c Release -p:UseAppHost=false -o .\Build\portable-x86 >nul 2>&1
IF %ERRORLEVEL% NEQ 0 GOTO err
echo Copying important files!
set BUILDLOCATION=.\Build\portable-x86
mkdir %BUILDLOCATION%\Binaries\ >nul 2>&1
mkdir %BUILDLOCATION%\Modules\ >nul 2>&1
mkdir %BUILDLOCATION%\Images\ >nul 2>&1
mkdir %BUILDLOCATION%\Dependencies\ >nul 2>&1
mkdir %BUILDLOCATION%\runtimes\ >nul 2>&1
xcopy %GLADOSRELEASE%\Binaries\     %BUILDLOCATION%\Binaries\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\Modules\      %BUILDLOCATION%\Modules\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\Images\       %BUILDLOCATION%\Images\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\Dependencies\ %BUILDLOCATION%\Dependencies\  /E /H /C /I >nul 2>&1
xcopy %GLADOSRELEASE%\runtimes\ %BUILDLOCATION%\runtimes\ /E /H /C /I >nul 2>&1
IF EXIST %GLADOSRELEASE%\database.db ( 
	echo Deleting database.db to prevent any leaks!
	del %BUILDLOCATION%\database.db /q /f >nul 2>&1
)
echo Zipping all files!
del *.zip >nul 2>&1
7z a release-portable.zip -mx9 -r .\Build\portable-x64 .\Build\portable-x86 >nul 2>&1
7z l release-portable.zip >nul 2>&1
del /q /f /s .\Build\portable-x86 >nul 2>&1
rmdir .\Build\portable-x86 /s /q >nul 2>&1
del /q /f /s .\Build\portable-x64 >nul 2>&1
rmdir .\Build\portable-x64 /s /q >nul 2>&1
7z a release-linux.zip -mx9 -r .\Build\linux-x64 >nul 2>&1
7z l release-linux.zip >nul 2>&1
7z a release-windows.zip -mx9 -r .\Build\win-x64 .\Build\win-x86 >nul 2>&1
7z l release-windows.zip >nul 2>&1
7z a release-all.zip -mx9 *.zip >nul 2>&1
del /q /f /s .\Build >nul 2>&1
rmdir .\Build /s /q >nul 2>&1
echo Build successful!
timeout /t 10
EXIT /B 0

:err
echo Build failed!
echo Error level: %ERRORLEVEL%
PAUSE
EXIT /B 1
