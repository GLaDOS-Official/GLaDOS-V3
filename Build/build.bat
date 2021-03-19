@echo off
set DOTNET_CLI_TELEMETRY_OPTOUT=1
set GLADOSRELEASE=..\GLaDOSV3\bin\Release\net5.0

mkdir Build
del /q /f /s .\Build
dotnet nuget add source -n Discord.net https://www.myget.org/F/discord-net/api/v3/index.json
dotnet restore ..\GladosV3.sln
dotnet msbuild ..\GladosV3.sln /p:Configuration=Release "/p:Platform=x64"
dotnet publish ..\GLaDOSV3\GladosV3.csproj -c Release -p:PublishSingleFile=true --self-contained true -r win-x64 -o .\Build\win-x64
IF %ERRORLEVEL% NEQ 0 GOTO err
set BUILDLOCATION=.\Build\win-x64
mkdir %BUILDLOCATION%\Binaries\
mkdir %BUILDLOCATION%\Modules\
mkdir %BUILDLOCATION%\Images\
mkdir %BUILDLOCATION%\Dependencies\
copy %GLADOSRELEASE%\Binaries\     %BUILDLOCATION%\Binaries\
copy %GLADOSRELEASE%\Modules\      %BUILDLOCATION%\Modules\
copy %GLADOSRELEASE%\Images\       %BUILDLOCATION%\Images\
copy %GLADOSRELEASE%\Dependencies\ %BUILDLOCATION%\Dependencies\

dotnet msbuild ..\GladosV3.sln /p:Configuration=Release "/p:Platform=x86"
dotnet publish ..\GLaDOSV3\GladosV3.csproj -c Release -p:PublishSingleFile=true --self-contained true -r win-x86 -o .\Build\win-x86
IF %ERRORLEVEL% NEQ 0 GOTO err
set BUILDLOCATION=.\Build\win-x86
mkdir %BUILDLOCATION%\Binaries\
mkdir %BUILDLOCATION%\Modules\
mkdir %BUILDLOCATION%\Images\
mkdir %BUILDLOCATION%\Dependencies\
copy %GLADOSRELEASE%\Binaries\     %BUILDLOCATION%\Binaries\
copy %GLADOSRELEASE%\Modules\      %BUILDLOCATION%\Modules\
copy %GLADOSRELEASE%\Images\       %BUILDLOCATION%\Images\
copy %GLADOSRELEASE%\Dependencies\ %BUILDLOCATION%\Dependencies\

dotnet msbuild ..\GladosV3.sln /p:Configuration=Release "/p:Platform=x64"
dotnet publish ..\GLaDOSV3\GladosV3.csproj -c Release -p:PublishSingleFile=true --self-contained true -r linux-x64 -o .\Build\linux-x64
IF %ERRORLEVEL% NEQ 0 GOTO err
set BUILDLOCATION=.\Build\linux-x64
mkdir %BUILDLOCATION%\Binaries\
mkdir %BUILDLOCATION%\Modules\
mkdir %BUILDLOCATION%\Images\
mkdir %BUILDLOCATION%\Dependencies\
copy %GLADOSRELEASE%\Binaries\     %BUILDLOCATION%\Binaries\
copy %GLADOSRELEASE%\Modules\      %BUILDLOCATION%\Modules\
copy %GLADOSRELEASE%\Images\       %BUILDLOCATION%\Images\
copy %GLADOSRELEASE%\Dependencies\ %BUILDLOCATION%\Dependencies\
IF %ERRORLEVEL% NEQ 0 GOTO err
dotnet msbuild ..\GladosV3.sln /p:Configuration=Release "/p:Platform=x64"
dotnet publish ..\GLaDOSV3\GladosV3.csproj -c Release -r portable -o .\Build\portable-x64

set BUILDLOCATION=.\Build\portable-x64
mkdir %BUILDLOCATION%\Binaries\
mkdir %BUILDLOCATION%\Modules\
mkdir %BUILDLOCATION%\Images\
mkdir %BUILDLOCATION%\Dependencies\
copy %GLADOSRELEASE%\Binaries\     %BUILDLOCATION%\Binaries\
copy %GLADOSRELEASE%\Modules\      %BUILDLOCATION%\Modules\
copy %GLADOSRELEASE%\Images\       %BUILDLOCATION%\Images\
copy %GLADOSRELEASE%\Dependencies\ %BUILDLOCATION%\Dependencies\
IF %ERRORLEVEL% NEQ 0 GOTO err
dotnet msbuild ..\GladosV3.sln /p:Configuration=Release "/p:Platform=x86"
dotnet publish ..\GLaDOSV3\GladosV3.csproj -c Release -r portable -o .\Build\portable-x86

set BUILDLOCATION=.\Build\portable-x86
mkdir %BUILDLOCATION%\Binaries\
mkdir %BUILDLOCATION%\Modules\
mkdir %BUILDLOCATION%\Images\
mkdir %BUILDLOCATION%\Dependencies\
copy %GLADOSRELEASE%\Binaries\     %BUILDLOCATION%\Binaries\
copy %GLADOSRELEASE%\Modules\      %BUILDLOCATION%\Modules\
copy %GLADOSRELEASE%\Images\       %BUILDLOCATION%\Images\
copy %GLADOSRELEASE%\Dependencies\ %BUILDLOCATION%\Dependencies\

IF %ERRORLEVEL% NEQ 0 GOTO err

del *.zip
7z a release-portable.zip -mx9 -r .\Build\portable-* .\Build\portable-*
7z l release-portable.zip
7z a release-linux.zip -mx9 -r .\Build\linux-* .\Build\linux-*
7z l release-linux.zip
7z a release-windows.zip -mx9 -r .\Build\win-* .\Build\win-*
7z l release-windows.zip
7z a release-all.zip -mx9 *.zip
del /q /f /s .\Build
rmdir .\Build /s /q
echo Build successful!
timeout /t 10
EXIT /B 0

:err
echo Build failed!
PAUSE
EXIT /B 1
